import { apiClient } from './apiClient';
import { SalesOrder } from '../types';

export interface CreateSalesOrderRequest {
  customerId: string;
  warehouseId: string;
  items: {
    productId: string;
    quantity: number;
    unitPrice: number;
  }[];
}

export class SalesOrderService {
  private mapSalesOrder(o: any): SalesOrder {
    const orderStatusMap: Record<string, 'مسودة' | 'مؤكد' | 'مكتمل' | 'ملغى'> = {
      'Draft': 'مسودة',
      'Confirmed': 'مؤكد',
      'Completed': 'مكتمل',
      'Cancelled': 'ملغى',
      'مسودة': 'مسودة',
      'مؤكد': 'مؤكد',
      'مكتمل': 'مكتمل',
      'ملغى': 'ملغى',
    };

    const rawStatus = o.status || 'Draft';
    const mappedStatus = orderStatusMap[rawStatus] || 'مسودة';

    const items = (o.items || []).map((it: any) => ({
      id: it.id || it.productId,
      productId: it.productId,
      quantity: Number(it.quantity ?? 1),
      unitPrice: Number(it.unitPrice ?? 0),
      totalPrice: Number((it.quantity ?? 1) * (it.unitPrice ?? 0)),
    }));

    const computedTotal = items.reduce((sum: number, it: any) => sum + (it.totalPrice ?? 0), 0);
    const totalAmount = Number(o.totalAmount ?? computedTotal);

    return {
      ...o,
      id: o.id,
      orderNumber: o.orderNumber || `SO-${String(o.id || '').substring(0, 8).toUpperCase()}`,
      customerId: o.customerId,
      customerName: o.customerName || 'غير محدد',
      customerEmail: o.customerEmail || 'sales@customer.com',
      warehouseId: o.warehouseId,
      warehouseName: o.warehouseName || 'المستودع الرئيسي',
      issueDate: o.orderDate ? new Date(o.orderDate).toLocaleDateString('ar-SA') : (o.issueDate || '—'),
      status: mappedStatus,
      items,
      subtotal: totalAmount,
      totalAmount,
    };
  }

  async getSalesOrders(pageNumber = 1, pageSize = 50, status?: string, customerId?: string): Promise<SalesOrder[]> {
    const params: any = { pageNumber, pageSize };
    if (status) params.status = status;
    if (customerId) params.customerId = customerId;

    const response = await apiClient.get('/api/sales-orders', { params });
    const data = response.data;
    const items = Array.isArray(data) ? data : (data && Array.isArray(data.items) ? data.items : []);
    return items.map((o: any) => this.mapSalesOrder(o));
  }

  async getSalesOrderById(id: string): Promise<SalesOrder> {
    const response = await apiClient.get<SalesOrder>(`/api/sales-orders/${id}`);
    return this.mapSalesOrder(response.data);
  }

  async createSalesOrder(order: CreateSalesOrderRequest): Promise<{ id: string }> {
    const response = await apiClient.post<{ id: string }>('/api/sales-orders', order);
    return response.data;
  }

  async confirmSalesOrder(id: string): Promise<{ salesOrderId: string; invoiceId: string }> {
    // The backend confirms the order, reserves stock and generates the invoice
    // in one atomic transaction, returning the invoice id for the print flow.
    const response = await apiClient.post<{ salesOrderId: string; invoiceId: string }>(
      `/api/sales-orders/${id}/confirm`
    );
    return response.data;
  }

  async completeSalesOrder(id: string): Promise<void> {
    await apiClient.post(`/api/sales-orders/${id}/complete`);
  }

  async cancelSalesOrder(id: string): Promise<void> {
    await apiClient.post(`/api/sales-orders/${id}/cancel`);
  }
}

export const salesOrderService = new SalesOrderService();
