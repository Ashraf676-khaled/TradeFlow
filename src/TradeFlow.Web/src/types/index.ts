export type TenantId = 'tenant-apex' | 'tenant-global' | 'tenant-nordic';

export interface Tenant {
  id: TenantId;
  name: string;
  code: string;
  logo: string;
  plan: string;
  currency: string;
  currencySymbol: string;
  warehouseCount: number;
  activeOrdersCount: number;
  totalRevenue: number;
  userRole: string;
}

export type OrderStatus = 'مسودة' | 'مؤكد' | 'مكتمل' | 'ملغى';
export type PaymentStatus = 'مدفوع' | 'غير مدفوع' | 'مدفوع جزئياً' | 'متأخر' | 'ملغاة';
export type ProductStatus = 'متوفر' | 'مخزون منخفض' | 'نفد المخزون';
export type InvoiceLayoutStyle = 'A4' | 'Thermal';

/**
 * Global tenant settings managed from the Settings page (/api/settings).
 * Defaults apply whenever a key has not been persisted yet.
 */
export interface SystemSettings {
  taxEnabled: boolean;
  taxPercentage: number;
  creditSalesEnabled: boolean;
  lowStockThreshold: number;
  invoiceLayoutStyle: InvoiceLayoutStyle;
}

export interface Customer {
  id: string;
  code?: string;
  name?: string;
  contactPerson?: string;
  email?: string;
  phone?: string;
  company?: string;
  city?: string;
  country?: string;
  creditLimit?: number;
  outstandingBalance?: number;
  isActive?: boolean;
}

export interface Warehouse {
  id: string;
  code?: string;
  name?: string;
  location?: string;
  manager?: string;
  capacityUsedPct?: number;
  totalSkus?: number;
  isActive?: boolean;
}

export interface Product {
  id: string;
  sku?: string;
  name?: string;
  description?: string;
  category?: string;
  currentStock?: number;
  reservedStock?: number;
  availableStock?: number;
  minStockLevel?: number;
  maxStockLevel?: number;
  unitPrice?: number;
  costPrice?: number;
  warehouseId?: string;
  warehouseName?: string;
  binLocation?: string;
  unitOfMeasure?: string;
  status?: ProductStatus;
  isActive?: boolean;
}

export interface OrderItem {
  id: string;
  productId?: string;
  sku?: string;
  productName?: string;
  quantity?: number;
  unitPrice?: number;
  totalPrice?: number;
}

export interface SalesOrder {
  id: string;
  orderNumber?: string;
  tenantId?: TenantId;
  customerId?: string;
  customerName?: string;
  customerEmail?: string;
  warehouseId?: string;
  warehouseName?: string;
  issueDate?: string;
  expectedDeliveryDate?: string;
  status: OrderStatus;
  paymentStatus?: PaymentStatus;
  items?: OrderItem[];
  subtotal?: number;
  tax?: number;
  shippingFee?: number;
  totalAmount?: number;
  notes?: string;
}

export interface Invoice {
  id: string;
  invoiceNumber?: string;
  orderId?: string;
  orderNumber?: string;
  tenantId?: TenantId;
  customerId?: string;
  customerName?: string;
  customerEmail?: string;
  issueDate?: string;
  dueDate?: string;
  subtotal?: number;
  tax?: number;
  totalAmount?: number;
  paidAmount?: number;
  balanceDue?: number;
  status: PaymentStatus;
  paymentMethod?: string;
  lineItemsCount?: number;
}

export interface SystemNotification {
  id: string;
  title: string;
  message: string;
  timestamp: string;
  type: 'warning' | 'info' | 'success' | 'danger';
  read: boolean;
}

