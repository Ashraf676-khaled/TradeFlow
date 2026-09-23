import fs from 'fs';
import path from 'path';
import { execSync } from 'child_process';

const rootDir = process.cwd();
const distDir = path.resolve(rootDir, 'dist');
const binDir = path.resolve(rootDir, 'bin');
const outputDir = path.resolve(rootDir, 'dist-standalone');

console.log('[1/4] Ensuring frontend build is up to date...');
execSync('npm run build', { stdio: 'inherit', cwd: rootDir });

if (!fs.existsSync(distDir)) {
  console.error('Error: dist directory does not exist after build.');
  process.exit(1);
}

if (!fs.existsSync(binDir)) {
  fs.mkdirSync(binDir, { recursive: true });
}

if (!fs.existsSync(outputDir)) {
  fs.mkdirSync(outputDir, { recursive: true });
}

console.log('[2/4] Collecting frontend static assets from dist/...');
const assets: Record<string, { type: string; base64: string }> = {};

function getMimeType(filePath: string): string {
  const ext = path.extname(filePath).toLowerCase();
  switch (ext) {
    case '.html': return 'text/html; charset=utf-8';
    case '.js': return 'application/javascript; charset=utf-8';
    case '.css': return 'text/css; charset=utf-8';
    case '.svg': return 'image/svg+xml';
    case '.json': return 'application/json';
    case '.png': return 'image/png';
    case '.jpg':
    case '.jpeg': return 'image/jpeg';
    case '.ico': return 'image/x-icon';
    case '.woff2': return 'font/woff2';
    case '.woff': return 'font/woff';
    case '.ttf': return 'font/ttf';
    default: return 'application/octet-stream';
  }
}

function scanFiles(dir: string, prefix = '') {
  const items = fs.readdirSync(dir);
  for (const item of items) {
    const fullPath = path.join(dir, item);
    const relPath = path.join(prefix, item).replace(/\\/g, '/');
    const stat = fs.statSync(fullPath);
    if (stat.isDirectory()) {
      scanFiles(fullPath, relPath);
    } else {
      const content = fs.readFileSync(fullPath);
      const mime = getMimeType(fullPath);
      assets['/' + relPath] = {
        type: mime,
        base64: content.toString('base64'),
      };
      if (relPath === 'index.html') {
        assets['/'] = {
          type: mime,
          base64: content.toString('base64'),
        };
      }
      console.log(`   Embedded asset: /${relPath} (${(content.length / 1024).toFixed(1)} KB, ${mime})`);
    }
  }
}

scanFiles(distDir);

console.log('[3/4] Generating standalone server source with embedded assets & auto-launcher...');
const serverSrc = fs.readFileSync(path.resolve(rootDir, 'server.ts'), 'utf-8');

// Extract routes and database logic up to Vite integration
const splitToken = '// ==========================================\n// Vite Integration & Static Serving';
let baseLogic = serverSrc;
if (serverSrc.includes(splitToken)) {
  baseLogic = serverSrc.split(splitToken)[0];
} else if (serverSrc.includes('async function startServer()')) {
  baseLogic = serverSrc.split('async function startServer()')[0];
}

const standaloneCode = `
${baseLogic}

// ==========================================
// Embedded Static Assets Ledger
// ==========================================
const EMBEDDED_ASSETS: Record<string, { type: string; base64: string }> = ${JSON.stringify(assets, null, 2)};

// Serve static assets from memory
app.get('*', (req: Request, res: Response) => {
  if (req.path.startsWith('/api')) {
    return res.status(404).json({ error: 'Endpoint not found', path: req.path });
  }

  const normalizedPath = req.path;
  const asset = EMBEDDED_ASSETS[normalizedPath] || EMBEDDED_ASSETS[normalizedPath + '/index.html'];

  if (asset) {
    res.setHeader('Content-Type', asset.type);
    if (normalizedPath.startsWith('/assets/')) {
      res.setHeader('Cache-Control', 'public, max-age=31536000, immutable');
    }
    const buf = Buffer.from(asset.base64, 'base64');
    return res.send(buf);
  }

  // Fallback to index.html for client-side SPA routing
  const indexHtml = EMBEDDED_ASSETS['/index.html'] || EMBEDDED_ASSETS['/'];
  if (indexHtml) {
    res.setHeader('Content-Type', 'text/html; charset=utf-8');
    const buf = Buffer.from(indexHtml.base64, 'base64');
    return res.send(buf);
  }

  res.status(404).send('Not Found');
});

// Helper to launch browser across operating systems
function launchBrowser(url: string) {
  try {
    const { exec } = require('child_process');
    if (process.platform === 'win32') {
      exec(\`cmd.exe /c start "" "\${url}"\`, (err: any) => {
        if (err) {
          exec(\`powershell -Command "Start-Process '\${url}'"\`);
        }
      });
    } else if (process.platform === 'darwin') {
      exec(\`open "\${url}"\`);
    } else if (process.platform === 'linux' && !process.env.DOCKER) {
      exec(\`xdg-open "\${url}"\`);
    }
  } catch (err) {
    // Non-fatal
  }
}

// Function to find an open port starting from 3000
function startStandaloneServer(port = 3000) {
  const server = app.listen(port, '0.0.0.0', () => {
    const url = \`http://localhost:\${port}\`;
    console.log('');
    console.log('========================================================');
    console.log('  TradeFlow ERP - Standalone Desktop System v1.0.0');
    console.log('========================================================');
    console.log(\`  [✓] Status:    Active & Running\`);
    console.log(\`  [✓] Currency:  Egyptian Pound (ج.م / EGP)\`);
    console.log(\`  [✓] Interface: \${url}\`);
    console.log(\`  [✓] Auto-opening system in default browser...\`);
    console.log('');
    console.log('  Default Login Credentials:');
    console.log('  --------------------------');
    console.log('  Email:    admin@tradeflow.io');
    console.log('  Password: Password123!');
    console.log('');
    console.log('  [Notice] Keep this window open while using TradeFlow.');
    console.log('  To close the app, press Ctrl+C or close this window.');
    console.log('========================================================');
    console.log('');

    // Immediately launch user browser
    launchBrowser(url);
  });

  server.on('error', (err: any) => {
    if (err.code === 'EADDRINUSE' && port < 3010) {
      console.log(\`Port \${port} is in use, attempting port \${port + 1}...\`);
      startStandaloneServer(port + 1);
    } else {
      console.error('Server error:', err);
    }
  });
}

startStandaloneServer(3000);
`;

const standalonePath = path.resolve(outputDir, 'standalone-server.ts');
fs.writeFileSync(standalonePath, standaloneCode, 'utf-8');
console.log(`   Generated: ${standalonePath} (${(standaloneCode.length / 1024).toFixed(1)} KB)`);

console.log('[4/4] Compiling standalone Windows executable with Bun (--target=bun-windows-x64)...');
const exeOutPath = path.resolve(binDir, 'TradeFlow-win-x64.exe');
const compileCmd = `bun build --compile --target=bun-windows-x64 --outfile="${exeOutPath}" "${standalonePath}"`;

console.log(`   Running: ${compileCmd}`);
execSync(compileCmd, { stdio: 'inherit', cwd: rootDir });

// Create helper launcher batch file in bin
const batPath = path.resolve(binDir, 'Start-TradeFlow.bat');
fs.writeFileSync(batPath, `@echo off
title TradeFlow ERP Launcher
echo ========================================================
echo   Launching TradeFlow ERP Standalone System...
echo ========================================================
start "" "%~dp0TradeFlow-win-x64.exe"
exit
`, 'utf-8');

console.log('\n[SUCCESS] Standalone Windows executable and launcher created!');
console.log(`   Executable: ${exeOutPath} (${(fs.statSync(exeOutPath).size / (1024 * 1024)).toFixed(2)} MB)`);
console.log(`   Launcher:   ${batPath}`);
