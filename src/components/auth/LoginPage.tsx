import React, { useState } from 'react';
import { authService, UserSession } from '../../services/authService';

interface LoginPageProps {
  onLoginSuccess: (session: UserSession) => void;
}

type AuthMode = 'login' | 'register';

export const LoginPage: React.FC<LoginPageProps> = ({ onLoginSuccess }) => {
  const [mode, setMode] = useState<AuthMode>('login');

  const [loginEmail, setLoginEmail] = useState('admin@tradeflow.io');
  const [loginPassword, setLoginPassword] = useState('Password123!');

  const [regName, setRegName] = useState('');
  const [regCompanyName, setRegCompanyName] = useState('');
  const [regEmail, setRegEmail] = useState('');
  const [regPassword, setRegPassword] = useState('');

  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setErrorMessage(null);

    try {
      const session = await authService.login(loginEmail, loginPassword);
      onLoginSuccess(session);
    } catch (err: any) {
      if (err.response?.status === 401) {
        setErrorMessage('Invalid credentials. Please verify your email and password.');
      } else {
        setErrorMessage(err.response?.data?.title || 'Connection to authentication service failed.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);
    setSuccessMessage(null);

    if (!regName.trim() || !regCompanyName.trim() || !regEmail.trim() || !regPassword.trim()) {
      setErrorMessage('All registration fields are required.');
      return;
    }

    setIsLoading(true);
    try {
      await authService.register(regEmail, regPassword, regName, regCompanyName);
      setSuccessMessage('Account registered successfully. Signing in...');
      const session = await authService.login(regEmail, regPassword);
      onLoginSuccess(session);
    } catch (err: any) {
      setErrorMessage(err.response?.data?.detail || err.response?.data?.title || 'Registration could not be completed.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-[#0b0c0e] text-[#e2e2e6] flex items-center justify-center p-4">
      <div className="w-full max-w-md bg-[#15171a] border border-[#26292e] rounded-xl shadow-2xl p-8 space-y-6">
        {/* Brand Header */}
        <div className="text-center space-y-3">
          <div className="inline-flex items-center justify-center">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 160 36" fill="none" className="h-10 w-auto">
              <rect x="2" y="4" width="28" height="28" rx="6" fill="#181A1E" stroke="#2E3238" strokeWidth="1.5" />
              <path d="M9 22L16 11L23 22" stroke="#EDEDEF" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round" />
              <path d="M12 18H20" stroke="#9CA3AF" strokeWidth="1.8" strokeLinecap="round" />
              <path d="M16 9V25" stroke="#10B981" strokeWidth="1.8" strokeLinecap="round" strokeDasharray="1 3" />
              <text x="38" y="23" fill="#EDEDEF" fontFamily="Inter, sans-serif" fontSize="18" fontWeight="700" letterSpacing="-0.03em">
                Trade<tspan fill="#9CA3AF" fontWeight="400">Flow</tspan>
              </text>
            </svg>
          </div>
          <p className="text-xs text-[#8f9194]">
            Institutional Trade Execution & Global Inventory Ledger
          </p>
        </div>

        {/* Tab switcher */}
        <div className="grid grid-cols-2 gap-1 p-1 bg-[#0b0c0e] rounded border border-white/5 text-xs font-semibold">
          <button
            type="button"
            onClick={() => { setMode('login'); setErrorMessage(null); }}
            className={`py-1.5 rounded transition-colors ${
              mode === 'login' ? 'bg-[#282a2d] text-white shadow-xs' : 'text-[#8f9194] hover:text-white'
            }`}
          >
            Access Terminal
          </button>
          <button
            type="button"
            onClick={() => { setMode('register'); setErrorMessage(null); }}
            className={`py-1.5 rounded transition-colors ${
              mode === 'register' ? 'bg-[#282a2d] text-white shadow-xs' : 'text-[#8f9194] hover:text-white'
            }`}
          >
            Open Account
          </button>
        </div>

        {errorMessage && (
          <div className="p-3 rounded bg-rose-500/15 border border-rose-500/30 text-rose-400 text-xs text-center">
            {errorMessage}
          </div>
        )}

        {successMessage && (
          <div className="p-3 rounded bg-[#10b981]/15 border border-[#10b981]/30 text-[#4edea3] text-xs text-center">
            {successMessage}
          </div>
        )}

        {mode === 'login' ? (
          <form onSubmit={handleLogin} className="space-y-4 text-xs font-sans">
            <div>
              <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                Trader Email
              </label>
              <input
                type="email"
                required
                value={loginEmail}
                onChange={(e) => setLoginEmail(e.target.value)}
                placeholder="admin@tradeflow.io"
                className="w-full px-3 py-2 bg-[#0b0c0e] border border-[#26292e] rounded text-white focus:border-[#4edea3] outline-none"
              />
            </div>

            <div>
              <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                Security Password
              </label>
              <input
                type="password"
                required
                value={loginPassword}
                onChange={(e) => setLoginPassword(e.target.value)}
                placeholder="••••••••••••"
                className="w-full px-3 py-2 bg-[#0b0c0e] border border-[#26292e] rounded text-white focus:border-[#4edea3] outline-none"
              />
            </div>

            <button
              type="submit"
              disabled={isLoading}
              className="w-full py-2.5 bg-[#ffffff] hover:bg-[#e2e2e4] text-[#111316] font-bold text-xs uppercase tracking-wider rounded shadow-md transition-all cursor-pointer"
            >
              {isLoading ? 'Verifying Credentials...' : 'Sign In to Terminal'}
            </button>
          </form>
        ) : (
          <form onSubmit={handleRegister} className="space-y-3 text-xs font-sans">
            <div>
              <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                Full Name
              </label>
              <input
                type="text"
                required
                value={regName}
                onChange={(e) => setRegName(e.target.value)}
                placeholder="e.g. Alex Morgan"
                className="w-full px-3 py-2 bg-[#0b0c0e] border border-[#26292e] rounded text-white focus:border-[#4edea3] outline-none"
              />
            </div>

            <div>
              <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                Company / Organization
              </label>
              <input
                type="text"
                required
                value={regCompanyName}
                onChange={(e) => setRegCompanyName(e.target.value)}
                placeholder="e.g. Apex Trading Corp"
                className="w-full px-3 py-2 bg-[#0b0c0e] border border-[#26292e] rounded text-white focus:border-[#4edea3] outline-none"
              />
            </div>

            <div>
              <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                Work Email
              </label>
              <input
                type="email"
                required
                value={regEmail}
                onChange={(e) => setRegEmail(e.target.value)}
                placeholder="trader@apextrading.com"
                className="w-full px-3 py-2 bg-[#0b0c0e] border border-[#26292e] rounded text-white focus:border-[#4edea3] outline-none"
              />
            </div>

            <div>
              <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                Password
              </label>
              <input
                type="password"
                required
                value={regPassword}
                onChange={(e) => setRegPassword(e.target.value)}
                placeholder="••••••••••••"
                className="w-full px-3 py-2 bg-[#0b0c0e] border border-[#26292e] rounded text-white focus:border-[#4edea3] outline-none"
              />
            </div>

            <button
              type="submit"
              disabled={isLoading}
              className="w-full py-2.5 bg-[#ffffff] hover:bg-[#e2e2e4] text-[#111316] font-bold text-xs uppercase tracking-wider rounded shadow-md transition-all cursor-pointer"
            >
              {isLoading ? 'Creating Account...' : 'Register Prime Account'}
            </button>
          </form>
        )}

        <div className="pt-3 border-t border-white/5 flex items-center justify-between text-[11px] text-[#8f9194]">
          <span>Security Protocol</span>
          <span className="font-mono text-[#4edea3]">TLS 1.3 / AES-256</span>
        </div>
      </div>
    </div>
  );
};
