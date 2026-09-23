import { apiClient } from './apiClient';
import { Warehouse } from '../types';

export class WarehouseService {
  private mapWarehouse(w: any): Warehouse {
    return {
      ...w,
      id: w.id,
      code: w.code || `WH-${String(w.id || '').substring(0, 4).toUpperCase()}`,
      name: w.name || 'المستودع الرئيسي',
      location: w.location || 'الرياض، المستودعات المركزية',
      manager: w.manager || 'مدير العمليات اللوجستية',
      capacityUsedPct: Number(w.capacityUsedPct ?? 65),
      totalSkus: Number(w.totalSkus ?? 30),
      isActive: w.isActive !== false,
    };
  }

  async getWarehouses(pageNumber = 1, pageSize = 50, isActive?: boolean): Promise<Warehouse[]> {
    const params: any = { pageNumber, pageSize };
    if (isActive !== undefined) params.isActive = isActive;

    const response = await apiClient.get('/api/warehouses', { params });
    const data = response.data;
    const items = Array.isArray(data) ? data : (data && Array.isArray(data.items) ? data.items : []);
    return items.map((w: any) => this.mapWarehouse(w));
  }

  async getWarehouseById(id: string): Promise<Warehouse> {
    const response = await apiClient.get<Warehouse>(`/api/warehouses/${id}`);
    return this.mapWarehouse(response.data);
  }

  async createWarehouse(warehouse: { name: string; location: string }): Promise<{ id: string }> {
    this.validateWarehouse(warehouse.name, warehouse.location);
    const response = await apiClient.post<{ id: string }>('/api/warehouses', warehouse);
    return response.data;
  }

  async renameWarehouse(warehouseId: string, newName: string): Promise<void> {
    await apiClient.put(`/api/warehouses/${warehouseId}/name`, { newName });
  }

  async changeWarehouseLocation(warehouseId: string, newLocation: string): Promise<void> {
    if (!newLocation.trim()) throw new Error('موقع المستودع مطلوب.');
    await apiClient.put(`/api/warehouses/${warehouseId}/location`, { newLocation });
  }

  private validateWarehouse(name: string, location: string): void {
    if (!name.trim()) throw new Error('اسم المستودع مطلوب.');
    if (!location.trim()) throw new Error('موقع المستودع مطلوب.');
  }
}

export const warehouseService = new WarehouseService();
