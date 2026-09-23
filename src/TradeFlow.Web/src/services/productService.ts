import { apiClient } from './apiClient';
import { Product } from '../types';

export class ProductService {
  private mapProduct(p: any): Product {
    const minStock = Number(p.minimumStock ?? p.minStockLevel ?? 5);
    const currStock = Number(p.currentStock ?? 0);
    const unitPrice = Number(p.sellingPrice ?? p.unitPrice ?? 0);
    const costPrice = Number(p.cost ?? p.costPrice ?? 0);

    let status: Product['status'] = 'متوفر';
    if (p.isActive === false || currStock === 0) {
      status = 'نفد المخزون';
    } else if (currStock <= minStock) {
      status = 'مخزون منخفض';
    }

    return {
      ...p,
      id: p.id,
      sku: p.sku || 'SKU-000',
      name: p.name || 'صنف غير مسمى',
      description: p.description || 'منتج تجاري مسجل في النظام',
      category: p.category || 'بضائع عامة',
      currentStock: currStock,
      reservedStock: Number(p.reservedStock ?? 0),
      availableStock: Number(p.availableStock ?? currStock),
      minStockLevel: minStock,
      maxStockLevel: Number(p.maxStockLevel ?? 100),
      unitPrice,
      costPrice,
      warehouseId: p.warehouseId || '',
      warehouseName: p.warehouseName || 'المستودع الرئيسي',
      binLocation: p.binLocation || 'A-01',
      unitOfMeasure: p.unitOfMeasure || 'قطعة',
      status: p.status || status,
      isActive: p.isActive !== false,
    };
  }

  async getProducts(pageNumber = 1, pageSize = 50, isActive?: boolean): Promise<Product[]> {
    const params: any = { pageNumber, pageSize };
    if (isActive !== undefined) params.isActive = isActive;

    const response = await apiClient.get('/api/products', { params });
    const data = response.data;
    const items = Array.isArray(data) ? data : (data && Array.isArray(data.items) ? data.items : []);
    return items.map((p: any) => this.mapProduct(p));
  }

  async getProductById(id: string): Promise<Product> {
    const response = await apiClient.get<Product>(`/api/products/${id}`);
    return this.mapProduct(response.data);
  }

  async createProduct(productData: {
    sku: string;
    name: string;
    description?: string;
    unitPrice?: number;
    sellingPrice?: number;
    costPrice?: number;
    cost?: number;
    minimumStock?: number;
    minStockLevel?: number;
    categoryId?: string;
    unitOfMeasure?: string;
    openingStockQuantity?: number;
    openingStockWarehouseId?: string;
  }): Promise<{ id: string }> {
    const sku = this.normalizeSku(productData.sku);
    this.validateProduct(sku, productData.name, productData.sellingPrice ?? productData.unitPrice, productData.cost ?? productData.costPrice);
    const openingStockQuantity = Math.max(0, Number(productData.openingStockQuantity ?? 0));
    const payload = {
      name: productData.name,
      sku,
      sellingPrice: Number(productData.sellingPrice ?? productData.unitPrice ?? 1),
      cost: Number(productData.cost ?? productData.costPrice ?? 0),
      minimumStock: Number(productData.minimumStock ?? productData.minStockLevel ?? 5),
      openingStockQuantity,
      // Optional: when omitted the backend falls back to the default warehouse.
      ...(openingStockQuantity > 0 && productData.openingStockWarehouseId
        ? { openingStockWarehouseId: productData.openingStockWarehouseId }
        : {}),
    };
    const response = await apiClient.post<{ id: string }>('/api/products', payload);
    return response.data;
  }

  async updateProduct(productId: string, productData: {
    sku: string;
    name: string;
    description?: string;
    unitPrice?: number;
    sellingPrice?: number;
    costPrice?: number;
    cost?: number;
    minimumStock?: number;
    minStockLevel?: number;
    unitOfMeasure?: string;
  }): Promise<void> {
    const sellingPrice = productData.sellingPrice ?? productData.unitPrice;
    const cost = productData.cost ?? productData.costPrice;
    const sku = this.normalizeSku(productData.sku);
    this.validateProduct(sku, productData.name, sellingPrice, cost);
    await apiClient.put(`/api/products/${productId}`, {
      ...productData,
      sku,
      sellingPrice: Number(sellingPrice),
      cost: Number(cost ?? 0),
      minimumStock: Number(productData.minimumStock ?? productData.minStockLevel ?? 0),
    });
  }

  async deleteProduct(productId: string): Promise<void> {
    await apiClient.delete(`/api/products/${productId}`);
  }

  async changeSellingPrice(productId: string, newPrice: number): Promise<void> {
    if (!Number.isFinite(newPrice) || newPrice < 0) throw new Error('يجب أن يكون سعر البيع صفراً أو قيمة موجبة.');
    await apiClient.put(`/api/products/${productId}/price`, newPrice);
  }

  // Backend Sku value object only accepts letters, digits and hyphens (A-Z0-9-).
  private normalizeSku(sku: string): string {
    return sku.trim().replace(/\s+/g, '-').replace(/[^A-Za-z0-9-]/g, '').toUpperCase();
  }

  private validateProduct(sku: string, name: string, sellingPrice?: number, cost?: number): void {
    if (!sku.trim()) throw new Error('رمز الصنف مطلوب.');
    if (!name.trim()) throw new Error('اسم الصنف مطلوب.');
    if (this.normalizeSku(sku).length === 0) throw new Error('رمز الصنف لازم يحتوي على حروف أو أرقام فقط.');
    if (sellingPrice !== undefined && (!Number.isFinite(sellingPrice) || sellingPrice <= 0)) throw new Error('سعر البيع لازم يكون أكبر من صفر.');
    if (cost !== undefined && (!Number.isFinite(cost) || cost < 0)) throw new Error('التكلفة غير صالحة.');
    // The backend rejects cost >= selling price, so mirror that exact rule here
    // to stop the request before it turns into a 400.
    if (sellingPrice !== undefined && cost !== undefined && sellingPrice <= cost) throw new Error('سعر البيع لازم يكون أكبر من التكلفة.');
  }

  async activateProduct(productId: string): Promise<void> {
    await apiClient.post(`/api/products/${productId}/activate`);
  }

  async deactivateProduct(productId: string): Promise<void> {
    await apiClient.post(`/api/products/${productId}/deactivate`);
  }
}

export const productService = new ProductService();
