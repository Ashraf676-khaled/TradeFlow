import express, { Request, Response } from 'express';
import cors from 'cors';
import { createServer as createViteServer } from 'vite';
import path from 'path';
import fs from 'fs';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const app = express();
const PORT = 3000;
const HOST = '0.0.0.0';

app.use(cors());
app.use(express.json());

// ==========================================
// In-Memory Database State
// ==========================================

interface WarehouseEntity {
  id: string;
  code: string;
  name: string;
  location: string;
  manager: string;
  capacityUsedPct: number;
  totalSkus: number;
  isActive: boolean;
}

interface ProductEntity {
  id: string;
  sku: string;
  name: string;
  description: string;
  category: string;
  sellingPrice: number;
  unitPrice: number;
  cost: number;
  costPrice: number;
  minimumStock: number;
  minStockLevel: number;
  maxStockLevel: number;
  currentStock: number;
  reservedStock: number;
  availableStock: number;
  warehouseId: string;
  warehouseName: string;
  binLocation: string;
  unitOfMeasure: string;
  status: 'متوفر' | 'مخزون منخفض' | 'نفد المخزون';
  isActive: boolean;
}

interface StockItemEntity {
  id: string;
  warehouseId: string;
  productId: string;
  quantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  unitCost: number;
}

interface CustomerEntity {
  id: string;
  code: string;
  name: string;
  contactPerson: string;
  email: string;
  phone: string;
  company: string;
  city: string;
  country: string;
  creditLimit: number;
  currentBalance: number;
  outstandingBalance: number;
  isActive: boolean;
}

interface OrderItemEntity {
  id: string;
  productId: string;
  sku?: string;
  productName?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

interface SalesOrderEntity {
  id: string;
  orderNumber: string;
  customerId: string;
  customerName?: string;
  customerEmail?: string;
  warehouseId: string;
  warehouseName?: string;
  orderDate: string;
  issueDate?: string;
  expectedDeliveryDate?: string;
  status: 'مسودة' | 'مؤكد' | 'مكتمل' | 'ملغى';
  items: OrderItemEntity[];
  subtotal: number;
  tax: number;
  totalAmount: number;
}

interface InvoiceEntity {
  id: string;
  invoiceNumber: string;
  salesOrderId: string;
  orderId?: string;
  orderNumber: string;
  customerId: string;
  customerName?: string;
  customerEmail?: string;
  issuedAt: string;
  dueDate: string;
  subtotal: number;
  tax: number;
  totalAmount: number;
  paidAmount: number;
  balanceDue: number;
  status: 'مدفوع' | 'غير مدفوع' | 'مدفوع جزئياً' | 'متأخر' | 'ملغاة';
  lineItemsCount: number;
}

// Initial Data
let systemSettings = {
  taxEnabled: true,
  taxPercentage: 15,
  creditSalesEnabled: true,
  lowStockThreshold: 5,
  invoiceLayoutStyle: 'A4' as 'A4' | 'Thermal',
};

const warehouses: WarehouseEntity[] = [
  {
    id: 'wh-1',
    code: 'WH-RYD',
    name: 'المستودع الرئيسي - الرياض',
    location: 'الرياض، المنطقة الصناعية الثانية',
    manager: 'م. أحمد خالد',
    capacityUsedPct: 68,
    totalSkus: 28,
    isActive: true,
  },
  {
    id: 'wh-2',
    code: 'WH-JED',
    name: 'مستودع جدة المركزي',
    location: 'جدة، حي الخمرة اللوجستي',
    manager: 'أ. طارق الشريف',
    capacityUsedPct: 45,
    totalSkus: 18,
    isActive: true,
  },
  {
    id: 'wh-3',
    code: 'WH-DMM',
    name: 'مستودع الدمام الإقليمي',
    location: 'الدمام، طريق الميناء',
    manager: 'م. سامي الزهراني',
    capacityUsedPct: 32,
    totalSkus: 12,
    isActive: true,
  },
];

const products: ProductEntity[] = [
  {
    id: 'prod-1',
    sku: 'ELE-2701',
    name: 'شاشة سامسونج 27 بوصة IPS 144Hz',
    description: 'شاشة احترافية فائقة الدقة متوافقة مع الألعاب وأعمال التصميم',
    category: 'إلكترونيات',
    sellingPrice: 1250,
    unitPrice: 1250,
    cost: 850,
    costPrice: 850,
    minimumStock: 5,
    minStockLevel: 5,
    maxStockLevel: 50,
    currentStock: 24,
    reservedStock: 2,
    availableStock: 22,
    warehouseId: 'wh-1',
    warehouseName: 'المستودع الرئيسي - الرياض',
    binLocation: 'A-01-04',
    unitOfMeasure: 'قطعة',
    status: 'متوفر',
    isActive: true,
  },
  {
    id: 'prod-2',
    sku: 'KEY-902',
    name: 'لوحة مفاتيح ميكانيكية لاسلكية RGB',
    description: 'لوحة مفاتيح ذات مفاتيح حمراء هادئة وتوصيل بلوتوث وسلكي',
    category: 'إلكترونيات',
    sellingPrice: 380,
    unitPrice: 380,
    cost: 210,
    costPrice: 210,
    minimumStock: 10,
    minStockLevel: 10,
    maxStockLevel: 80,
    currentStock: 45,
    reservedStock: 0,
    availableStock: 45,
    warehouseId: 'wh-1',
    warehouseName: 'المستودع الرئيسي - الرياض',
    binLocation: 'A-02-02',
    unitOfMeasure: 'قطعة',
    status: 'متوفر',
    isActive: true,
  },
  {
    id: 'prod-3',
    sku: 'MOU-105',
    name: 'فأرة ألعاب ليزرية 16000 DPI',
    description: 'فأرة خفيفة الوزن مزودة بمستشعر بصري عالي الدقة',
    category: 'إلكترونيات',
    sellingPrice: 195,
    unitPrice: 195,
    cost: 110,
    costPrice: 110,
    minimumStock: 8,
    minStockLevel: 8,
    maxStockLevel: 60,
    currentStock: 32,
    reservedStock: 0,
    availableStock: 32,
    warehouseId: 'wh-1',
    warehouseName: 'المستودع الرئيسي - الرياض',
    binLocation: 'A-02-03',
    unitOfMeasure: 'قطعة',
    status: 'متوفر',
    isActive: true,
  },
  {
    id: 'prod-4',
    sku: 'ARM-401',
    name: 'حامل شاشة هيدروليكي مزدوج',
    description: 'ذراع ألومنيوم مرن يدعم شاشات حتى 32 بوصة متوافق مع VESA',
    category: 'ملحقات مكتبية',
    sellingPrice: 290,
    unitPrice: 290,
    cost: 160,
    costPrice: 160,
    minimumStock: 6,
    minStockLevel: 6,
    maxStockLevel: 40,
    currentStock: 4,
    reservedStock: 0,
    availableStock: 4,
    warehouseId: 'wh-1',
    warehouseName: 'المستودع الرئيسي - الرياض',
    binLocation: 'B-01-01',
    unitOfMeasure: 'قطعة',
    status: 'مخزون منخفض',
    isActive: true,
  },
  {
    id: 'prod-5',
    sku: 'NET-600',
    name: 'راوتر واي فاي 6 متطور 3000 Mbps',
    description: 'راوتر شبكي للأعمال يدعم تغطية واسعة وأمان WPA3',
    category: 'شبكات',
    sellingPrice: 480,
    unitPrice: 480,
    cost: 310,
    costPrice: 310,
    minimumStock: 5,
    minStockLevel: 5,
    maxStockLevel: 30,
    currentStock: 16,
    reservedStock: 0,
    availableStock: 16,
    warehouseId: 'wh-2',
    warehouseName: 'مستودع جدة المركزي',
    binLocation: 'C-03-02',
    unitOfMeasure: 'قطعة',
    status: 'متوفر',
    isActive: true,
  },
  {
    id: 'prod-6',
    sku: 'CAB-201',
    name: 'كابل HDMI 2.1 فائق السرعة 2 متر',
    description: 'كابل معتمد يدعم 8K@60Hz و 4K@120Hz مطلي بالذهب',
    category: 'كابلات وتوصيلات',
    sellingPrice: 70,
    unitPrice: 70,
    cost: 32,
    costPrice: 32,
    minimumStock: 15,
    minStockLevel: 15,
    maxStockLevel: 150,
    currentStock: 90,
    reservedStock: 0,
    availableStock: 90,
    warehouseId: 'wh-1',
    warehouseName: 'المستودع الرئيسي - الرياض',
    binLocation: 'A-03-01',
    unitOfMeasure: 'قطعة',
    status: 'متوفر',
    isActive: true,
  },
];

const stockItems: StockItemEntity[] = [
  { id: 'st-1', warehouseId: 'wh-1', productId: 'prod-1', quantity: 24, reservedQuantity: 2, availableQuantity: 22, unitCost: 850 },
  { id: 'st-2', warehouseId: 'wh-1', productId: 'prod-2', quantity: 45, reservedQuantity: 0, availableQuantity: 45, unitCost: 210 },
  { id: 'st-3', warehouseId: 'wh-1', productId: 'prod-3', quantity: 32, reservedQuantity: 0, availableQuantity: 32, unitCost: 110 },
  { id: 'st-4', warehouseId: 'wh-1', productId: 'prod-4', quantity: 4, reservedQuantity: 0, availableQuantity: 4, unitCost: 160 },
  { id: 'st-5', warehouseId: 'wh-2', productId: 'prod-5', quantity: 16, reservedQuantity: 0, availableQuantity: 16, unitCost: 310 },
  { id: 'st-6', warehouseId: 'wh-1', productId: 'prod-6', quantity: 90, reservedQuantity: 0, availableQuantity: 90, unitCost: 32 },
];

const customers: CustomerEntity[] = [
  {
    id: 'cust-1',
    code: 'CUST-AFK',
    name: 'شركة الأفق الحديثة للتقنية',
    contactPerson: 'أ. عبد الله المنصور',
    email: 'info@alofok-tech.com',
    phone: '01012345678',
    company: 'شركة الأفق الحديثة للتقنية',
    city: 'الرياض',
    country: 'المملكة العربية السعودية',
    creditLimit: 50000,
    currentBalance: 7500,
    outstandingBalance: 7500,
    isActive: true,
  },
  {
    id: 'cust-2',
    code: 'CUST-NUR',
    name: 'مؤسسة النور للتوريدات والتجارة',
    contactPerson: 'م. إبراهيم فؤاد',
    email: 'alnoor.supply@gmail.com',
    phone: '01123456789',
    company: 'مؤسسة النور للتوريدات',
    city: 'جدة',
    country: 'المملكة العربية السعودية',
    creditLimit: 35000,
    currentBalance: 3200,
    outstandingBalance: 3200,
    isActive: true,
  },
  {
    id: 'cust-3',
    code: 'CUST-BDR',
    name: 'شركة البدر للحلول الرقمية',
    contactPerson: 'سارة العتيبي',
    email: 'contact@albadr-solutions.sa',
    phone: '01234567890',
    company: 'شركة البدر للحلول الرقمية',
    city: 'الدمام',
    country: 'المملكة العربية السعودية',
    creditLimit: 25000,
    currentBalance: 0,
    outstandingBalance: 0,
    isActive: true,
  },
  {
    id: 'cust-4',
    code: 'CUST-SHQ',
    name: 'مكتبة الشرق الهندسية',
    contactPerson: 'عمر القحطاني',
    email: 'sharq.engineering@outlook.com',
    phone: '01543216789',
    company: 'مكتبة الشرق الهندسية',
    city: 'الرياض',
    country: 'المملكة العربية السعودية',
    creditLimit: 15000,
    currentBalance: 4200,
    outstandingBalance: 4200,
    isActive: true,
  },
];

const salesOrders: SalesOrderEntity[] = [
  {
    id: 'so-1',
    orderNumber: 'SO-2026-001',
    customerId: 'cust-1',
    customerName: 'شركة الأفق الحديثة للتقنية',
    customerEmail: 'info@alofok-tech.com',
    warehouseId: 'wh-1',
    warehouseName: 'المستودع الرئيسي - الرياض',
    orderDate: new Date(Date.now() - 3 * 86400000).toISOString(),
    issueDate: new Date(Date.now() - 3 * 86400000).toLocaleDateString('ar-SA'),
    status: 'مؤكد',
    items: [
      { id: 'item-1', productId: 'prod-1', sku: 'ELE-2701', productName: 'شاشة سامسونج 27 بوصة IPS 144Hz', quantity: 2, unitPrice: 1250, totalPrice: 2500 },
      { id: 'item-2', productId: 'prod-2', sku: 'KEY-902', productName: 'لوحة مفاتيح ميكانيكية لاسلكية RGB', quantity: 5, unitPrice: 380, totalPrice: 1900 },
    ],
    subtotal: 4400,
    tax: 660,
    totalAmount: 5060,
  },
  {
    id: 'so-2',
    orderNumber: 'SO-2026-002',
    customerId: 'cust-2',
    customerName: 'مؤسسة النور للتوريدات والتجارة',
    customerEmail: 'alnoor.supply@gmail.com',
    warehouseId: 'wh-1',
    warehouseName: 'المستودع الرئيسي - الرياض',
    orderDate: new Date(Date.now() - 1 * 86400000).toISOString(),
    issueDate: new Date(Date.now() - 1 * 86400000).toLocaleDateString('ar-SA'),
    status: 'مكتمل',
    items: [
      { id: 'item-3', productId: 'prod-3', sku: 'MOU-105', productName: 'فأرة ألعاب ليزرية 16000 DPI', quantity: 10, unitPrice: 195, totalPrice: 1950 },
    ],
    subtotal: 1950,
    tax: 292.5,
    totalAmount: 2242.5,
  },
];

const invoices: InvoiceEntity[] = [
  {
    id: 'inv-1',
    invoiceNumber: 'INV-2026-001',
    salesOrderId: 'so-1',
    orderId: 'so-1',
    orderNumber: 'SO-2026-001',
    customerId: 'cust-1',
    customerName: 'شركة الأفق الحديثة للتقنية',
    customerEmail: 'info@alofok-tech.com',
    issuedAt: new Date(Date.now() - 3 * 86400000).toISOString(),
    dueDate: new Date(Date.now() + 27 * 86400000).toISOString(),
    subtotal: 4400,
    tax: 660,
    totalAmount: 5060,
    paidAmount: 2000,
    balanceDue: 3060,
    status: 'مدفوع جزئياً',
    lineItemsCount: 2,
  },
  {
    id: 'inv-2',
    invoiceNumber: 'INV-2026-002',
    salesOrderId: 'so-2',
    orderId: 'so-2',
    orderNumber: 'SO-2026-002',
    customerId: 'cust-2',
    customerName: 'مؤسسة النور للتوريدات والتجارة',
    customerEmail: 'alnoor.supply@gmail.com',
    issuedAt: new Date(Date.now() - 1 * 86400000).toISOString(),
    dueDate: new Date(Date.now() + 29 * 86400000).toISOString(),
    subtotal: 1950,
    tax: 292.5,
    totalAmount: 2242.5,
    paidAmount: 2242.5,
    balanceDue: 0,
    status: 'مدفوع',
    lineItemsCount: 1,
  },
];

// Helper to recalculate product stock
function syncProductStock(productId: string) {
  const prod = products.find(p => p.id === productId);
  if (!prod) return;
  const items = stockItems.filter(s => s.productId === productId);
  const totalQty = items.reduce((sum, s) => sum + s.quantity, 0);
  const reservedQty = items.reduce((sum, s) => sum + s.reservedQuantity, 0);
  const availQty = items.reduce((sum, s) => sum + s.availableQuantity, 0);

  prod.currentStock = totalQty;
  prod.reservedStock = reservedQty;
  prod.availableStock = availQty;
  const minStock = prod.minStockLevel || systemSettings.lowStockThreshold || 5;
  if (totalQty === 0) {
    prod.status = 'نفد المخزون';
  } else if (totalQty <= minStock) {
    prod.status = 'مخزون منخفض';
  } else {
    prod.status = 'متوفر';
  }
}

// ==========================================
// Authentication Endpoints
// ==========================================

app.post('/api/auth/login', (req: Request, res: Response) => {
  const { email, password } = req.body;
  if (!email || !password) {
    return res.status(400).json({ detail: 'البريد الإلكتروني وكلمة المرور مطلوبان.' });
  }

  const token = 'tradeflow_token_' + Buffer.from(email).toString('base64');
  const expires = new Date(Date.now() + 7 * 86400000).toISOString();

  return res.json({
    accessToken: token,
    accessTokenExpiresUtc: expires,
  });
});

app.post('/api/auth/register', (req: Request, res: Response) => {
  const { email, password, name, companyName } = req.body;
  if (!email || !password) {
    return res.status(400).json({ detail: 'البيانات المطلوبة غير مكتملة.' });
  }

  const token = 'tradeflow_token_' + Buffer.from(email).toString('base64');
  const expires = new Date(Date.now() + 7 * 86400000).toISOString();

  return res.json({
    accessToken: token,
    accessTokenExpiresUtc: expires,
  });
});

app.post('/api/auth/refresh', (_req: Request, res: Response) => {
  const expires = new Date(Date.now() + 7 * 86400000).toISOString();
  return res.json({
    accessToken: 'tradeflow_token_refreshed_' + Date.now(),
    accessTokenExpiresUtc: expires,
  });
});

app.post('/api/auth/revoke', (_req: Request, res: Response) => {
  return res.status(204).send();
});

// ==========================================
// Settings Endpoints
// ==========================================

app.get('/api/settings', (_req: Request, res: Response) => {
  return res.json(systemSettings);
});

app.put('/api/settings', (req: Request, res: Response) => {
  const { taxEnabled, taxPercentage, creditSalesEnabled, lowStockThreshold, invoiceLayoutStyle } = req.body;
  if (taxEnabled !== undefined) systemSettings.taxEnabled = Boolean(taxEnabled);
  if (taxPercentage !== undefined) systemSettings.taxPercentage = Number(taxPercentage);
  if (creditSalesEnabled !== undefined) systemSettings.creditSalesEnabled = Boolean(creditSalesEnabled);
  if (lowStockThreshold !== undefined) systemSettings.lowStockThreshold = Number(lowStockThreshold);
  if (invoiceLayoutStyle !== undefined) systemSettings.invoiceLayoutStyle = invoiceLayoutStyle === 'Thermal' ? 'Thermal' : 'A4';

  // Update product statuses according to new threshold
  products.forEach(p => syncProductStock(p.id));

  return res.json(systemSettings);
});

// ==========================================
// Warehouses Endpoints
// ==========================================

app.get('/api/warehouses', (req: Request, res: Response) => {
  const isActive = req.query.isActive !== undefined ? req.query.isActive === 'true' : undefined;
  let list = warehouses;
  if (isActive !== undefined) {
    list = list.filter(w => w.isActive === isActive);
  }
  return res.json(list);
});

app.get('/api/warehouses/:id', (req: Request, res: Response) => {
  const wh = warehouses.find(w => w.id === req.params.id);
  if (!wh) return res.status(404).json({ detail: 'المستودع غير موجود.' });
  return res.json(wh);
});

app.post('/api/warehouses', (req: Request, res: Response) => {
  const { name, location } = req.body;
  if (!name || !name.trim()) return res.status(400).json({ detail: 'اسم المستودع مطلوب.' });
  if (!location || !location.trim()) return res.status(400).json({ detail: 'موقع المستودع مطلوب.' });

  const id = 'wh-' + (warehouses.length + 1);
  const code = 'WH-' + String(warehouses.length + 1).padStart(3, '0');
  const newWh: WarehouseEntity = {
    id,
    code,
    name: name.trim(),
    location: location.trim(),
    manager: 'مدير المستودع الجديد',
    capacityUsedPct: 0,
    totalSkus: 0,
    isActive: true,
  };
  warehouses.push(newWh);
  return res.status(201).json({ id });
});

app.put('/api/warehouses/:id/name', (req: Request, res: Response) => {
  const wh = warehouses.find(w => w.id === req.params.id);
  if (!wh) return res.status(404).json({ detail: 'المستودع غير موجود.' });
  const { newName } = req.body;
  if (!newName || !newName.trim()) return res.status(400).json({ detail: 'اسم المستودع مطلوب.' });
  wh.name = newName.trim();
  return res.status(204).send();
});

app.put('/api/warehouses/:id/location', (req: Request, res: Response) => {
  const wh = warehouses.find(w => w.id === req.params.id);
  if (!wh) return res.status(404).json({ detail: 'المستودع غير موجود.' });
  const { newLocation } = req.body;
  if (!newLocation || !newLocation.trim()) return res.status(400).json({ detail: 'موقع المستودع مطلوب.' });
  wh.location = newLocation.trim();
  return res.status(204).send();
});

// ==========================================
// Products Endpoints
// ==========================================

app.get('/api/products', (req: Request, res: Response) => {
  const isActive = req.query.isActive !== undefined ? req.query.isActive === 'true' : undefined;
  let list = products;
  if (isActive !== undefined) {
    list = list.filter(p => p.isActive === isActive);
  }
  return res.json(list);
});

app.get('/api/products/:id', (req: Request, res: Response) => {
  const p = products.find(x => x.id === req.params.id);
  if (!p) return res.status(404).json({ detail: 'الصنف غير موجود.' });
  return res.json(p);
});

app.post('/api/products', (req: Request, res: Response) => {
  const { name, sku, sellingPrice, cost, minimumStock, openingStockQuantity, openingStockWarehouseId } = req.body;
  if (!name || !name.trim()) return res.status(400).json({ detail: 'اسم الصنف مطلوب.' });
  if (!sku || !sku.trim()) return res.status(400).json({ detail: 'رمز الصنف مطلوب.' });

  const id = 'prod-' + (products.length + 1);
  const whId = openingStockWarehouseId || warehouses[0]?.id || 'wh-1';
  const wh = warehouses.find(w => w.id === whId);
  const openingQty = Number(openingStockQuantity ?? 0);
  const sellPrice = Number(sellingPrice ?? 1);
  const costPrice = Number(cost ?? 0);
  const minStock = Number(minimumStock ?? 5);

  const newProd: ProductEntity = {
    id,
    sku: sku.trim().toUpperCase(),
    name: name.trim(),
    description: req.body.description || 'منتج تجاري مسجل في النظام',
    category: req.body.category || 'بضائع عامة',
    sellingPrice: sellPrice,
    unitPrice: sellPrice,
    cost: costPrice,
    costPrice: costPrice,
    minimumStock: minStock,
    minStockLevel: minStock,
    maxStockLevel: 100,
    currentStock: openingQty,
    reservedStock: 0,
    availableStock: openingQty,
    warehouseId: whId,
    warehouseName: wh ? wh.name : 'المستودع الرئيسي',
    binLocation: 'A-01',
    unitOfMeasure: 'قطعة',
    status: openingQty === 0 ? 'نفد المخزون' : openingQty <= minStock ? 'مخزون منخفض' : 'متوفر',
    isActive: true,
  };
  products.push(newProd);

  if (openingQty > 0) {
    stockItems.push({
      id: 'st-' + (stockItems.length + 1),
      warehouseId: whId,
      productId: id,
      quantity: openingQty,
      reservedQuantity: 0,
      availableQuantity: openingQty,
      unitCost: costPrice,
    });
  }

  return res.status(201).json({ id });
});

app.put('/api/products/:id', (req: Request, res: Response) => {
  const p = products.find(x => x.id === req.params.id);
  if (!p) return res.status(404).json({ detail: 'الصنف غير موجود.' });

  if (req.body.name) p.name = req.body.name.trim();
  if (req.body.sku) p.sku = req.body.sku.trim().toUpperCase();
  if (req.body.sellingPrice !== undefined) {
    p.sellingPrice = Number(req.body.sellingPrice);
    p.unitPrice = p.sellingPrice;
  }
  if (req.body.cost !== undefined) {
    p.cost = Number(req.body.cost);
    p.costPrice = p.cost;
  }
  if (req.body.minimumStock !== undefined) {
    p.minimumStock = Number(req.body.minimumStock);
    p.minStockLevel = p.minimumStock;
  }
  syncProductStock(p.id);
  return res.json(p);
});

app.put('/api/products/:id/price', (req: Request, res: Response) => {
  const p = products.find(x => x.id === req.params.id);
  if (!p) return res.status(404).json({ detail: 'الصنف غير موجود.' });
  const newPrice = typeof req.body === 'number' ? req.body : Number(req.body?.price ?? req.body?.newPrice);
  if (!Number.isFinite(newPrice) || newPrice < 0) {
    return res.status(400).json({ detail: 'سعر البيع غير صالح.' });
  }
  p.sellingPrice = newPrice;
  p.unitPrice = newPrice;
  return res.status(204).send();
});

app.post('/api/products/:id/activate', (req: Request, res: Response) => {
  const p = products.find(x => x.id === req.params.id);
  if (!p) return res.status(404).json({ detail: 'الصنف غير موجود.' });
  p.isActive = true;
  return res.status(204).send();
});

app.post('/api/products/:id/deactivate', (req: Request, res: Response) => {
  const p = products.find(x => x.id === req.params.id);
  if (!p) return res.status(404).json({ detail: 'الصنف غير موجود.' });
  p.isActive = false;
  return res.status(204).send();
});

app.delete('/api/products/:id', (req: Request, res: Response) => {
  const idx = products.findIndex(x => x.id === req.params.id);
  if (idx !== -1) products.splice(idx, 1);
  return res.status(204).send();
});

// ==========================================
// Stock Operations Endpoints
// ==========================================

app.get('/api/stock/warehouse/:warehouseId', (req: Request, res: Response) => {
  const items = stockItems.filter(s => s.warehouseId === req.params.warehouseId);
  return res.json(items);
});

app.post('/api/stock/receive', (req: Request, res: Response) => {
  const { warehouseId, productId, quantity, unitCost } = req.body;
  if (!warehouseId || !productId || !quantity || quantity <= 0) {
    return res.status(400).json({ detail: 'بيانات استلام المخزون غير صالحة.' });
  }

  let item = stockItems.find(s => s.warehouseId === warehouseId && s.productId === productId);
  if (item) {
    item.quantity += Number(quantity);
    item.availableQuantity += Number(quantity);
    item.unitCost = Number(unitCost || item.unitCost);
  } else {
    item = {
      id: 'st-' + (stockItems.length + 1),
      warehouseId,
      productId,
      quantity: Number(quantity),
      reservedQuantity: 0,
      availableQuantity: Number(quantity),
      unitCost: Number(unitCost || 0),
    };
    stockItems.push(item);
  }

  syncProductStock(productId);
  return res.json({ id: item.id });
});

app.post('/api/stock/transfer', (req: Request, res: Response) => {
  const { sourceWarehouseId, targetWarehouseId, productId, quantity } = req.body;
  const qty = Number(quantity);
  if (qty <= 0) return res.status(400).json({ detail: 'الكمية يجب أن تكون أكبر من صفر.' });

  const sourceItem = stockItems.find(s => s.warehouseId === sourceWarehouseId && s.productId === productId);
  if (!sourceItem || sourceItem.availableQuantity < qty) {
    return res.status(400).json({ detail: 'الكمية المتاحة في مستودع المصدر غير كافية.' });
  }

  sourceItem.quantity -= qty;
  sourceItem.availableQuantity -= qty;

  let targetItem = stockItems.find(s => s.warehouseId === targetWarehouseId && s.productId === productId);
  if (targetItem) {
    targetItem.quantity += qty;
    targetItem.availableQuantity += qty;
  } else {
    targetItem = {
      id: 'st-' + (stockItems.length + 1),
      warehouseId: targetWarehouseId,
      productId,
      quantity: qty,
      reservedQuantity: 0,
      availableQuantity: qty,
      unitCost: sourceItem.unitCost,
    };
    stockItems.push(targetItem);
  }

  syncProductStock(productId);
  return res.status(204).send();
});

// ==========================================
// Customers Endpoints
// ==========================================

app.get(['/api/customers', '/api/customers/'], (req: Request, res: Response) => {
  const isActive = req.query.isActive !== undefined ? req.query.isActive === 'true' : undefined;
  let list = customers;
  if (isActive !== undefined) {
    list = list.filter(c => c.isActive === isActive);
  }
  return res.json(list);
});

app.get('/api/customers/:id', (req: Request, res: Response) => {
  const cust = customers.find(c => c.id === req.params.id);
  if (!cust) return res.status(404).json({ detail: 'العميل غير موجود.' });
  return res.json(cust);
});

app.post('/api/customers', (req: Request, res: Response) => {
  const { name, phone, email, creditLimit } = req.body;
  if (!name || !name.trim()) return res.status(400).json({ detail: 'اسم العميل مطلوب.' });

  const id = 'cust-' + (customers.length + 1);
  const code = 'CUST-' + String(customers.length + 1).padStart(3, '0');
  const newCust: CustomerEntity = {
    id,
    code,
    name: name.trim(),
    contactPerson: name.trim(),
    email: email?.trim() || 'customer@tradeflow.io',
    phone: phone || '01000000000',
    company: name.trim(),
    city: 'الرياض',
    country: 'المملكة العربية السعودية',
    creditLimit: Number(creditLimit ?? 10000),
    currentBalance: 0,
    outstandingBalance: 0,
    isActive: true,
  };
  customers.push(newCust);
  return res.status(201).json({ id });
});

app.put('/api/customers/:id/contact-info', (req: Request, res: Response) => {
  const cust = customers.find(c => c.id === req.params.id);
  if (!cust) return res.status(404).json({ detail: 'العميل غير موجود.' });
  if (req.body.phone) cust.phone = req.body.phone;
  if (req.body.email) cust.email = req.body.email;
  return res.status(204).send();
});

app.put('/api/customers/:id/credit-limit', (req: Request, res: Response) => {
  const cust = customers.find(c => c.id === req.params.id);
  if (!cust) return res.status(404).json({ detail: 'العميل غير موجود.' });
  const newLimit = Number(req.body.newLimit);
  if (!Number.isFinite(newLimit) || newLimit < 0) {
    return res.status(400).json({ detail: 'الحد الائتماني غير صالح.' });
  }
  cust.creditLimit = newLimit;
  return res.status(204).send();
});

app.post('/api/customers/:id/activate', (req: Request, res: Response) => {
  const cust = customers.find(c => c.id === req.params.id);
  if (!cust) return res.status(404).json({ detail: 'العميل غير موجود.' });
  cust.isActive = true;
  return res.status(204).send();
});

app.post('/api/customers/:id/deactivate', (req: Request, res: Response) => {
  const cust = customers.find(c => c.id === req.params.id);
  if (!cust) return res.status(404).json({ detail: 'العميل غير موجود.' });
  cust.isActive = false;
  return res.status(204).send();
});

app.delete('/api/customers/:id', (req: Request, res: Response) => {
  const idx = customers.findIndex(c => c.id === req.params.id);
  if (idx !== -1) customers.splice(idx, 1);
  return res.status(204).send();
});

// ==========================================
// Sales Orders Endpoints
// ==========================================

app.get('/api/sales-orders', (req: Request, res: Response) => {
  let list = salesOrders;
  if (req.query.status) {
    list = list.filter(o => o.status === req.query.status);
  }
  if (req.query.customerId) {
    list = list.filter(o => o.customerId === req.query.customerId);
  }
  return res.json(list);
});

app.get('/api/sales-orders/:id', (req: Request, res: Response) => {
  const o = salesOrders.find(x => x.id === req.params.id);
  if (!o) return res.status(404).json({ detail: 'طلب المبيعات غير موجود.' });
  return res.json(o);
});

app.post('/api/sales-orders', (req: Request, res: Response) => {
  const { customerId, warehouseId, items } = req.body;
  if (!customerId || !warehouseId || !items || !items.length) {
    return res.status(400).json({ detail: 'بيانات أمر البيع غير مكتملة.' });
  }

  const cust = customers.find(c => c.id === customerId);
  const wh = warehouses.find(w => w.id === warehouseId);

  const orderId = 'so-' + (salesOrders.length + 1);
  const orderNumber = 'SO-2026-' + String(salesOrders.length + 1).padStart(3, '0');

  let subtotal = 0;
  const processedItems: OrderItemEntity[] = (items as any[]).map((it, idx) => {
    const prod = products.find(p => p.id === it.productId);
    const price = Number(it.unitPrice ?? prod?.sellingPrice ?? 0);
    const qty = Number(it.quantity ?? 1);
    const lineTotal = price * qty;
    subtotal += lineTotal;

    return {
      id: `so-it-${orderId}-${idx + 1}`,
      productId: it.productId,
      sku: prod?.sku || 'SKU-000',
      productName: prod?.name || 'صنف',
      quantity: qty,
      unitPrice: price,
      totalPrice: lineTotal,
    };
  });

  const tax = systemSettings.taxEnabled ? (subtotal * (systemSettings.taxPercentage / 100)) : 0;
  const totalAmount = subtotal + tax;

  const newOrder: SalesOrderEntity = {
    id: orderId,
    orderNumber,
    customerId,
    customerName: cust?.name || 'غير محدد',
    customerEmail: cust?.email || '',
    warehouseId,
    warehouseName: wh?.name || 'المستودع الرئيسي',
    orderDate: new Date().toISOString(),
    issueDate: new Date().toLocaleDateString('ar-SA'),
    status: 'مسودة',
    items: processedItems,
    subtotal,
    tax,
    totalAmount,
  };

  salesOrders.unshift(newOrder);
  return res.status(201).json({ id: orderId });
});

app.post('/api/sales-orders/:id/confirm', (req: Request, res: Response) => {
  const order = salesOrders.find(o => o.id === req.params.id);
  if (!order) return res.status(404).json({ detail: 'أمر البيع غير موجود.' });

  order.status = 'مؤكد';

  // Deduct/reserve inventory from warehouse
  for (const item of order.items) {
    const stock = stockItems.find(s => s.warehouseId === order.warehouseId && s.productId === item.productId);
    if (stock) {
      stock.quantity = Math.max(0, stock.quantity - item.quantity);
      stock.availableQuantity = Math.max(0, stock.availableQuantity - item.quantity);
      syncProductStock(item.productId);
    }
  }

  // Create corresponding invoice if not already existing
  let inv = invoices.find(i => i.salesOrderId === order.id);
  if (!inv) {
    const invId = 'inv-' + (invoices.length + 1);
    const invNumber = 'INV-2026-' + String(invoices.length + 1).padStart(3, '0');
    inv = {
      id: invId,
      invoiceNumber: invNumber,
      salesOrderId: order.id,
      orderId: order.id,
      orderNumber: order.orderNumber,
      customerId: order.customerId,
      customerName: order.customerName,
      customerEmail: order.customerEmail,
      issuedAt: new Date().toISOString(),
      dueDate: new Date(Date.now() + 30 * 86400000).toISOString(),
      subtotal: order.subtotal,
      tax: order.tax,
      totalAmount: order.totalAmount,
      paidAmount: 0,
      balanceDue: order.totalAmount,
      status: 'غير مدفوع',
      lineItemsCount: order.items.length,
    };
    invoices.unshift(inv);

    // Update customer outstanding balance
    const cust = customers.find(c => c.id === order.customerId);
    if (cust) {
      cust.currentBalance += order.totalAmount;
      cust.outstandingBalance += order.totalAmount;
    }
  }

  return res.json({ salesOrderId: order.id, invoiceId: inv.id });
});

app.post('/api/sales-orders/:id/complete', (req: Request, res: Response) => {
  const order = salesOrders.find(o => o.id === req.params.id);
  if (!order) return res.status(404).json({ detail: 'أمر البيع غير موجود.' });
  order.status = 'مكتمل';
  return res.status(204).send();
});

app.post('/api/sales-orders/:id/cancel', (req: Request, res: Response) => {
  const order = salesOrders.find(o => o.id === req.params.id);
  if (!order) return res.status(404).json({ detail: 'أمر البيع غير موجود.' });
  order.status = 'ملغى';
  return res.status(204).send();
});

// ==========================================
// Invoices Endpoints
// ==========================================

app.get('/api/invoices', (req: Request, res: Response) => {
  let list = invoices;
  if (req.query.customerId) {
    list = list.filter(i => i.customerId === req.query.customerId);
  }
  return res.json(list);
});

app.get('/api/invoices/:id', (req: Request, res: Response) => {
  const inv = invoices.find(i => i.id === req.params.id);
  if (!inv) return res.status(404).json({ detail: 'الفاتورة غير موجودة.' });
  return res.json(inv);
});

app.post('/api/invoices', (req: Request, res: Response) => {
  const { salesOrderId, dueInDays } = req.body;
  const order = salesOrders.find(o => o.id === salesOrderId);
  if (!order) return res.status(404).json({ detail: 'أمر البيع غير موجود.' });

  const invId = 'inv-' + (invoices.length + 1);
  const invNumber = 'INV-2026-' + String(invoices.length + 1).padStart(3, '0');
  const days = Number(dueInDays || 30);

  const inv: InvoiceEntity = {
    id: invId,
    invoiceNumber: invNumber,
    salesOrderId: order.id,
    orderId: order.id,
    orderNumber: order.orderNumber,
    customerId: order.customerId,
    customerName: order.customerName,
    customerEmail: order.customerEmail,
    issuedAt: new Date().toISOString(),
    dueDate: new Date(Date.now() + days * 86400000).toISOString(),
    subtotal: order.subtotal,
    tax: order.tax,
    totalAmount: order.totalAmount,
    paidAmount: 0,
    balanceDue: order.totalAmount,
    status: 'غير مدفوع',
    lineItemsCount: order.items.length,
  };
  invoices.unshift(inv);
  return res.status(201).json({ id: invId });
});

app.post('/api/invoices/:id/payments', (req: Request, res: Response) => {
  const inv = invoices.find(i => i.id === req.params.id);
  if (!inv) return res.status(404).json({ detail: 'الفاتورة غير موجودة.' });
  const amount = Number(req.body.amount);
  if (!Number.isFinite(amount) || amount <= 0) {
    return res.status(400).json({ detail: 'قيمة الدفعة غير صالحة.' });
  }

  inv.paidAmount = Math.min(inv.totalAmount, inv.paidAmount + amount);
  inv.balanceDue = Math.max(0, inv.totalAmount - inv.paidAmount);

  if (inv.balanceDue === 0) {
    inv.status = 'مدفوع';
  } else {
    inv.status = 'مدفوع جزئياً';
  }

  // Update customer balance
  const cust = customers.find(c => c.id === inv.customerId);
  if (cust) {
    cust.currentBalance = Math.max(0, cust.currentBalance - amount);
    cust.outstandingBalance = Math.max(0, cust.outstandingBalance - amount);
  }

  return res.status(204).send();
});

app.post('/api/invoices/:id/cancel', (req: Request, res: Response) => {
  const inv = invoices.find(i => i.id === req.params.id);
  if (!inv) return res.status(404).json({ detail: 'الفاتورة غير موجودة.' });
  inv.status = 'ملغاة';
  return res.status(204).send();
});

// ==========================================
// Vite Integration & Static Serving
// ==========================================

async function startServer() {
  const isProd = process.env.NODE_ENV === 'production';
  const distPath = path.resolve(__dirname, 'dist');

  if (isProd && fs.existsSync(distPath)) {
    app.use(express.static(distPath));
    app.get('*', (_req: Request, res: Response) => {
      res.sendFile(path.resolve(distPath, 'index.html'));
    });
  } else {
    const vite = await createViteServer({
      server: { middlewareMode: true, host: HOST, port: PORT },
      appType: 'spa',
    });

    app.use(vite.middlewares);

    app.use('*', async (req: Request, res: Response, next) => {
      const url = req.originalUrl;
      try {
        let template = fs.readFileSync(path.resolve(__dirname, 'index.html'), 'utf-8');
        template = await vite.transformIndexHtml(url, template);
        res.status(200).set({ 'Content-Type': 'text/html' }).end(template);
      } catch (e) {
        vite.ssrFixStacktrace(e as Error);
        next(e);
      }
    });
  }

  app.listen(PORT, HOST, () => {
    console.log(`[TradeFlow ERP] Server listening on http://${HOST}:${PORT}`);
  });
}

startServer();
