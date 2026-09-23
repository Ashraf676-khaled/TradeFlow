import { apiClient } from './apiClient';

export interface ReceiveStockRequest {
  warehouseId: string;
  productId: string;
  quantity: number;
  unitCost: number;
}

export interface TransferStockRequest {
  sourceWarehouseId: string;
  targetWarehouseId: string;
  productId: string;
  quantity: number;
}

export class StockService {
  async getStockByWarehouse(warehouseId: string, pageNumber = 1, pageSize = 50): Promise<any[]> {
    const response = await apiClient.get(`/api/stock/warehouse/${warehouseId}`, {
      params: { pageNumber, pageSize },
    });
    const data = response.data;
    if (Array.isArray(data)) return data;
    if (data && Array.isArray(data.items)) return data.items;
    return [];
  }

  async receiveStock(request: ReceiveStockRequest): Promise<{ id: string }> {
    const response = await apiClient.post<{ id: string }>('/api/stock/receive', request);
    return response.data;
  }

  async transferStock(request: TransferStockRequest): Promise<void> {
    await apiClient.post('/api/stock/transfer', request);
  }
}

export const stockService = new StockService();
