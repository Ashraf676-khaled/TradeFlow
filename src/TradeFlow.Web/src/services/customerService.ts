import { apiClient } from './apiClient';
import { Customer } from '../types';

export class CustomerService {
  private mapCustomer(c: any): Customer {
    return {
      ...c,
      id: c.id,
      code: c.code || `CUST-${String(c.id || '').substring(0, 6).toUpperCase()}`,
      name: c.name || 'غير محدد',
      contactPerson: c.contactPerson || c.name || 'مسؤول الحساب',
      email: c.email || 'customer@tradeflow.io',
      phone: c.phone || '01000000000',
      company: c.company || c.name || 'شركة عميل',
      city: c.city || 'الرياض',
      country: c.country || 'المملكة العربية السعودية',
      creditLimit: Number(c.creditLimit ?? 10000),
      outstandingBalance: Number(c.currentBalance ?? c.outstandingBalance ?? 0),
      isActive: c.isActive !== false,
    };
  }

  async getCustomers(pageNumber = 1, pageSize = 50, isActive?: boolean): Promise<Customer[]> {
    const params: any = { pageNumber, pageSize };
    if (isActive !== undefined) params.isActive = isActive;

    const response = await apiClient.get('/api/customers/', { params });
    const data = response.data;
    const items = Array.isArray(data) ? data : (data && Array.isArray(data.items) ? data.items : []);
    return items.map((c: any) => this.mapCustomer(c));
  }

  async getCustomerById(id: string): Promise<Customer> {
    const response = await apiClient.get<Customer>(`/api/customers/${id}`);
    return this.mapCustomer(response.data);
  }

  async createCustomer(customer: { name: string; phone: string; email?: string; creditLimit?: number }): Promise<{ id: string }> {
    this.validateCustomer(customer.name, customer.phone, customer.creditLimit);
    const payload = {
      name: customer.name,
      phone: customer.phone || '01000000000',
      email: customer.email?.trim() || null,
      creditLimit: Number(customer.creditLimit ?? 10000),
    };
    const response = await apiClient.post<{ id: string }>('/api/customers', payload);
    return response.data;
  }

  async updateCustomer(customerId: string, customer: { name: string; phone: string; email?: string; creditLimit?: number }): Promise<void> {
    this.validateCustomer(customer.name, customer.phone, customer.creditLimit);
    await apiClient.put(`/api/customers/${customerId}/contact-info`, { phone: customer.phone, email: customer.email?.trim() || null });
    if (customer.creditLimit !== undefined) {
      await this.changeCreditLimit(customerId, Number(customer.creditLimit));
    }
  }

  async deleteCustomer(customerId: string): Promise<void> {
    await apiClient.delete(`/api/customers/${customerId}`);
  }

  async changeCreditLimit(customerId: string, newLimit: number): Promise<void> {
    if (!Number.isFinite(newLimit) || newLimit < 0) throw new Error('يجب أن يكون الحد الائتماني صفراً أو قيمة موجبة.');
    await apiClient.put(`/api/customers/${customerId}/credit-limit`, { newLimit });
  }

  private validateCustomer(name: string, phone: string, creditLimit?: number): void {
    if (!name.trim()) throw new Error('اسم العميل مطلوب.');
    if (!/^01[0125]\d{8}$/.test(phone.replace(/[ -]/g, '').trim())) {
      throw new Error('رقم الهاتف يجب أن يكون رقمًا مصريًا صحيحًا مثل 01000000000.');
    }
    if (creditLimit !== undefined && (!Number.isFinite(creditLimit) || creditLimit < 0)) {
      throw new Error('يجب أن يكون الحد الائتماني صفراً أو قيمة موجبة.');
    }
  }

  async activateCustomer(customerId: string): Promise<void> {
    await apiClient.post(`/api/customers/${customerId}/activate`);
  }

  async deactivateCustomer(customerId: string): Promise<void> {
    await apiClient.post(`/api/customers/${customerId}/deactivate`);
  }
}

export const customerService = new CustomerService();
