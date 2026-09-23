import React, { createContext, useContext, useState, useEffect } from 'react';
import { Product, SalesOrder, Invoice, Warehouse, Customer, SystemSettings } from '../types';
import { authService, UserSession } from '../services/authService';
import { productService } from '../services/productService';
import { salesOrderService, CreateSalesOrderRequest } from '../services/salesOrderService';
import { invoiceService } from '../services/invoiceService';
import { customerService } from '../services/customerService';
import { warehouseService } from '../services/warehouseService';
import { stockService } from '../services/stockService';
import { settingsService, DEFAULT_SYSTEM_SETTINGS } from '../services/settingsService';

export type NavigationPage = 'overview' | 'orders' | 'inventory' | 'invoices' | 'customers' | 'settings';

interface TenantContextType {
  isAuthenticated: boolean;
  userSession: UserSession | null;
  currencySymbol: string;
  loginSession: (session: UserSession) => void;
  logoutSession: () => void;

  currentPage: NavigationPage;
  setCurrentPage: (page: NavigationPage) => void;

  products: Product[];
  orders: SalesOrder[];
  invoices: Invoice[];
  warehouses: Warehouse[];
  customers: Customer[];
  settings: SystemSettings;
  isLoadingData: boolean;
  apiError: string | null;

  refreshAllData: () => Promise<boolean>;
  updateSettings: (settings: SystemSettings) => Promise<void>;
  createSalesOrder: (order: CreateSalesOrderRequest) => Promise<Invoice>;
  createProduct: (prod: { sku: string; name: string; description?: string; unitPrice: number; costPrice: number; minimumStock?: number; openingStockQuantity?: number; openingStockWarehouseId?: string }) => Promise<void>;
  createCustomer: (cust: { name: string; phone: string; email?: string; creditLimit?: number }) => Promise<void>;
  updateCustomer: (customerId: string, cust: { name: string; phone: string; email?: string; creditLimit?: number }) => Promise<void>;
  changeCustomerCreditLimit: (customerId: string, newLimit: number) => Promise<void>;
  setCustomerStatus: (customerId: string, isActive: boolean) => Promise<void>;
  deleteCustomer: (customerId: string) => Promise<void>;
  createWarehouse: (wh: { name: string; location: string }) => Promise<void>;
  receiveStock: (warehouseId: string, productId: string, quantity: number, unitCost: number) => Promise<void>;
  registerPayment: (invoiceId: string, amount: number) => Promise<void>;
  createInvoiceFromOrder: (orderId: string, dueInDays?: number) => Promise<Invoice>;
  confirmSalesOrder: (orderId: string) => Promise<Invoice>;
  completeSalesOrder: (orderId: string) => Promise<void>;
  cancelSalesOrder: (orderId: string) => Promise<void>;
}

const TenantContext = createContext<TenantContextType | undefined>(undefined);

export const TenantProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(authService.isAuthenticated());
  const [userSession, setUserSession] = useState<UserSession | null>(authService.getCurrentSession());
  const [currentPage, setCurrentPage] = useState<NavigationPage>('overview');

  const [products, setProducts] = useState<Product[]>([]);
  const [orders, setOrders] = useState<SalesOrder[]>([]);
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [settings, setSettings] = useState<SystemSettings>(DEFAULT_SYSTEM_SETTINGS);
  const [isLoadingData, setIsLoadingData] = useState<boolean>(false);
  const [apiError, setApiError] = useState<string | null>(null);

  const currencySymbol = 'جنيه';

  const loginSession = (session: UserSession) => {
    setIsAuthenticated(true);
    setUserSession(session);
  };

  const clearApplicationSession = () => {
    setIsAuthenticated(false);
    setUserSession(null);
    setProducts([]);
    setOrders([]);
    setInvoices([]);
    setWarehouses([]);
    setCustomers([]);
    localStorage.removeItem('tradeflow_user_name');
  };

  const logoutSession = () => {
    authService.logout();
    clearApplicationSession();
  };

  useEffect(() => {
    const handleLogoutEvent = () => {
      authService.clearSession();
      clearApplicationSession();
    };
    window.addEventListener('tradeflow_auth_logout', handleLogoutEvent);
    return () => window.removeEventListener('tradeflow_auth_logout', handleLogoutEvent);
  }, []);

  const loadData = async (): Promise<boolean> => {
    if (!isAuthenticated) return false;

    setIsLoadingData(true);
    setApiError(null);
    let hasErrors = false;

    try {
      const [prods, ords, invs, whs, custs] = await Promise.allSettled([
        productService.getProducts(),
        salesOrderService.getSalesOrders(),
        invoiceService.getInvoices(),
        warehouseService.getWarehouses(),
        customerService.getCustomers(),
      ]);

      // Global settings are required before the product low-stock mapping below.
      let currentSettings = settings;
      try {
        currentSettings = await settingsService.getSettings();
        setSettings(currentSettings);
      } catch {
        // Keep the last-known settings (or defaults) if the settings call fails.
      }

      if (prods.status === 'fulfilled') setProducts(prods.value);
      if (ords.status === 'fulfilled') setOrders(ords.value);
      if (invs.status === 'fulfilled') setInvoices(invs.value);
      if (whs.status === 'fulfilled') setWarehouses(whs.value);
      if (custs.status === 'fulfilled') setCustomers(custs.value);

      if (ords.status === 'fulfilled') {
        const customerMap = new Map((custs.status === 'fulfilled' ? custs.value : []).map(customer => [customer.id, customer.name]));
        const warehouseMap = new Map((whs.status === 'fulfilled' ? whs.value : []).map(warehouse => [warehouse.id, warehouse.name]));
        const productMap = new Map((prods.status === 'fulfilled' ? prods.value : []).map(product => [product.id, { name: product.name, sku: product.sku }]));
        setOrders(ords.value.map(order => ({
          ...order,
          customerName: customerMap.get(order.customerId ?? '') || order.customerName || 'غير محدد',
          warehouseName: warehouseMap.get(order.warehouseId ?? '') || order.warehouseName || 'المستودع الرئيسي',
          items: order.items?.map(item => ({
            ...item,
            productName: productMap.get(item.productId ?? '')?.name || item.productName,
            sku: productMap.get(item.productId ?? '')?.sku || item.sku,
          })),
        })));
      }
      if (invs.status === 'fulfilled') {
        const customerMap = new Map((custs.status === 'fulfilled' ? custs.value : []).map(customer => [customer.id, customer.name]));
        const orderMap = new Map((ords.status === 'fulfilled' ? ords.value : []).map(order => [order.id, order.orderNumber]));
        setInvoices(invs.value.map(invoice => ({
          ...invoice,
          customerName: customerMap.get(invoice.customerId ?? '') || invoice.customerName || 'غير محدد',
          orderNumber: orderMap.get(invoice.orderId ?? '') || invoice.orderNumber,
        })));
      }

      const failedLoads = [prods, ords, invs, whs, custs].filter(result => result.status === 'rejected');
      if (failedLoads.length > 0) {
        hasErrors = true;
        const firstError = failedLoads[0] as PromiseRejectedResult;
        const responseMessage = firstError.reason?.response?.data?.detail || firstError.reason?.response?.data?.title;
        setApiError(responseMessage || 'تعذر تحديث بعض البيانات. يرجى المحاولة مرة أخرى.');
      }

      if (prods.status === 'fulfilled' && whs.status === 'fulfilled') {
        const stockByProduct = new Map<string, number>();
        const stockLists = await Promise.all(whs.value.map(warehouse => stockService.getStockByWarehouse(warehouse.id, 1, 500)));
        stockLists.flat().forEach((stock: { productId: string; availableQuantity: number }) => {
          stockByProduct.set(stock.productId, (stockByProduct.get(stock.productId) ?? 0) + Number(stock.availableQuantity ?? 0));
        });
        // Low-stock limit: per-product minimum when set (> 0), otherwise the
        // system-wide LowStock.Threshold setting from the Settings page.
        const fallbackThreshold = currentSettings.lowStockThreshold > 0 ? currentSettings.lowStockThreshold : 5;
        setProducts(prods.value.map(product => {
          const currentStock = stockByProduct.get(product.id) ?? 0;
          const minStockLevel = Number(product.minStockLevel ?? 0) > 0
            ? Number(product.minStockLevel)
            : fallbackThreshold;
          return {
            ...product,
            minStockLevel,
            currentStock,
            availableStock: currentStock,
            status: currentStock === 0 ? 'نفد المخزون' : currentStock <= minStockLevel ? 'مخزون منخفض' : 'متوفر',
          };
        }));
      }
    } catch (err: any) {
      setApiError('تعذر تحميل بيانات النظام. يرجى التحقق من الاتصال والمحاولة مرة أخرى.');
      return false;
    } finally {
      setIsLoadingData(false);
    }
    return !hasErrors;
  };

  useEffect(() => {
    if (isAuthenticated) {
      loadData();
    }
  }, [isAuthenticated]);

  useEffect(() => {
    const fallback = settings.lowStockThreshold > 0 ? settings.lowStockThreshold : 5;
    const lowStockProducts = products.filter(product =>
      (product.currentStock ?? 0) <= (product.minStockLevel && product.minStockLevel > 0 ? product.minStockLevel : fallback));
    if (lowStockProducts.length > 0 && typeof Notification !== 'undefined' && Notification.permission === 'granted') {
      new Notification('تنبيه المخزون', { body: `يوجد ${lowStockProducts.length} صنف بلغ حد إعادة الطلب (${fallback}).` });
    }
  }, [products, settings.lowStockThreshold]);

  const refreshAllData = async (): Promise<boolean> => {
    return loadData();
  };

  const createSalesOrder = async (order: CreateSalesOrderRequest): Promise<Invoice> => {
    const result = await salesOrderService.createSalesOrder(order);
    return confirmSalesOrder(result.id);
  };

  const createProduct = async (prod: { sku: string; name: string; description?: string; unitPrice: number; costPrice: number; minimumStock?: number; openingStockQuantity?: number; openingStockWarehouseId?: string }) => {
    await productService.createProduct(prod);
    await loadData();
  };

  const createCustomer = async (cust: { name: string; phone: string; email?: string; creditLimit?: number }) => {
    await customerService.createCustomer(cust);
    await loadData();
  };

  const updateCustomer = async (customerId: string, cust: { name: string; phone: string; email?: string; creditLimit?: number }) => {
    await customerService.updateCustomer(customerId, cust);
    await loadData();
  };

  const changeCustomerCreditLimit = async (customerId: string, newLimit: number) => {
    await customerService.changeCreditLimit(customerId, newLimit);
    await loadData();
  };

  const setCustomerStatus = async (customerId: string, isActive: boolean) => {
    if (isActive) await customerService.activateCustomer(customerId);
    else await customerService.deactivateCustomer(customerId);
    await loadData();
  };

  const deleteCustomer = async (customerId: string) => {
    await customerService.deleteCustomer(customerId);
    await loadData();
  };

  const createWarehouse = async (wh: { name: string; location: string }) => {
    await warehouseService.createWarehouse(wh);
    await loadData();
  };

  const updateSettings = async (next: SystemSettings) => {
    const saved = await settingsService.updateSettings(next);
    setSettings(saved);
    await loadData();
  };

  const receiveStock = async (warehouseId: string, productId: string, quantity: number, unitCost: number) => {
    await stockService.receiveStock({ warehouseId, productId, quantity, unitCost });
    await loadData();
  };

  const registerPayment = async (invoiceId: string, amount: number) => {
    await invoiceService.registerPayment(invoiceId, amount);
    await loadData();
  };

  const createInvoiceFromOrder = async (orderId: string, dueInDays = 30): Promise<Invoice> => {
    const result = await invoiceService.createInvoiceFromOrder(orderId, dueInDays);
    await loadData();
    return invoiceService.getInvoiceById(result.id);
  };

  const confirmSalesOrder = async (orderId: string): Promise<Invoice> => {
    // The backend confirms the order, deducts/reserves inventory and generates the
    // invoice in ONE atomic transaction — no second invoice-creation call needed.
    const confirmation = await salesOrderService.confirmSalesOrder(orderId);
    await loadData();
    return invoiceService.getInvoiceById(confirmation.invoiceId);
  };

  const completeSalesOrder = async (orderId: string) => {
    await salesOrderService.completeSalesOrder(orderId);
    await loadData();
  };

  const cancelSalesOrder = async (orderId: string) => {
    await salesOrderService.cancelSalesOrder(orderId);
    await loadData();
  };

  return (
    <TenantContext.Provider value={{
      isAuthenticated,
      userSession,
      currencySymbol,
      loginSession,
      logoutSession,
      currentPage,
      setCurrentPage,
      products,
      orders,
      invoices,
      warehouses,
      customers,
      settings,
      isLoadingData,
      apiError,
      refreshAllData,
      updateSettings,
      createSalesOrder,
      createProduct,
      createCustomer,
      updateCustomer,
      changeCustomerCreditLimit,
      setCustomerStatus,
      deleteCustomer,
      createWarehouse,
      receiveStock,
      registerPayment,
      createInvoiceFromOrder,
      confirmSalesOrder,
      completeSalesOrder,
      cancelSalesOrder,
    }}>
      {children}
    </TenantContext.Provider>
  );
};

export const useTenant = () => {
  const context = useContext(TenantContext);
  if (!context) {
    throw new Error('useTenant must be used within a TenantProvider');
  }
  return context;
};
