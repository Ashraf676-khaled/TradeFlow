import { apiClient } from './apiClient';
import { Invoice } from '../types';

export class InvoiceService {
  private mapInvoice(inv: any): Invoice {
    const statusMap: Record<string, Invoice['status']> = {
      'Unpaid': 'غير مدفوع',
      'PartiallyPaid': 'مدفوع جزئياً',
      'Completed': 'مدفوع',
      'Cancelled': 'ملغاة',
      'غير مدفوع': 'غير مدفوع',
      'مدفوع جزئياً': 'مدفوع جزئياً',
      'مدفوع': 'مدفوع',
      'متأخر': 'متأخر',
      'ملغاة': 'ملغاة',
    };

    const total = Number(inv.totalAmount ?? 0);
    // Tax breakdown is derived from system settings where displayed; totalAmount
    // from the API is the authoritative (tax-inclusive) grand total.
    const subtotal = Number(inv.subtotal ?? total);
    const tax = Number(inv.tax ?? 0);
    const rawStatus = inv.status || 'Unpaid';
    const mappedStatus = statusMap[rawStatus] || 'غير مدفوع';
    const paid = Number(inv.paidAmount ?? (mappedStatus === 'مدفوع' ? total : 0));
    const balance = Number(inv.balanceDue ?? Math.max(0, total - paid));

    return {
      ...inv,
      id: inv.id,
      invoiceNumber: inv.invoiceNumber || `INV-${String(inv.id || '').substring(0, 8).toUpperCase()}`,
      orderId: inv.salesOrderId || inv.orderId || '',
      orderNumber: inv.orderNumber || (inv.salesOrderId ? `SO-${String(inv.salesOrderId).substring(0, 8).toUpperCase()}` : 'SO-001'),
      customerId: inv.customerId,
      customerName: inv.customerName || 'غير محدد',
      customerEmail: inv.customerEmail || '',
      issueDate: inv.issuedAt ? new Date(inv.issuedAt).toLocaleDateString('ar-SA') : (inv.issueDate || '—'),
      dueDate: inv.dueDate ? new Date(inv.dueDate).toLocaleDateString('ar-SA') : (inv.dueDate || '—'),
      totalAmount: total,
      subtotal,
      tax,
      paidAmount: paid,
      balanceDue: balance,
      status: mappedStatus,
      lineItemsCount: inv.lineItemsCount ?? 1,
    };
  }

  async getInvoices(pageNumber = 1, pageSize = 50, customerId?: string): Promise<Invoice[]> {
    const params: any = { pageNumber, pageSize };
    if (customerId) params.customerId = customerId;

    const response = await apiClient.get('/api/invoices', { params });
    const data = response.data;
    const items = Array.isArray(data) ? data : (data && Array.isArray(data.items) ? data.items : []);
    return items.map((inv: any) => this.mapInvoice(inv));
  }

  async getInvoiceById(id: string): Promise<Invoice> {
    const response = await apiClient.get<Invoice>(`/api/invoices/${id}`);
    return this.mapInvoice(response.data);
  }

  async createInvoiceFromOrder(salesOrderId: string, dueInDays = 30): Promise<{ id: string }> {
    if (!salesOrderId.trim()) throw new Error('أمر البيع مطلوب لإصدار الفاتورة.');
    if (!Number.isInteger(dueInDays) || dueInDays <= 0) throw new Error('مدة السداد يجب أن تكون أكبر من صفر.');
    const response = await apiClient.post<{ id: string }>('/api/invoices', { salesOrderId, dueInDays });
    return response.data;
  }

  async registerPayment(invoiceId: string, amount: number): Promise<void> {
    if (!Number.isFinite(amount) || amount <= 0) throw new Error('يجب أن تكون قيمة الدفعة أكبر من صفر.');
    await apiClient.post(`/api/invoices/${invoiceId}/payments`, { amount });
  }

  async cancelInvoice(invoiceId: string): Promise<void> {
    await apiClient.post(`/api/invoices/${invoiceId}/cancel`);
  }
}

export const invoiceService = new InvoiceService();
