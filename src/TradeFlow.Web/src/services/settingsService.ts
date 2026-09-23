import { apiClient } from './apiClient';
import { SystemSettings } from '../types';

export const DEFAULT_SYSTEM_SETTINGS: SystemSettings = {
  taxEnabled: false,
  taxPercentage: 15,
  creditSalesEnabled: true,
  lowStockThreshold: 5,
  invoiceLayoutStyle: 'A4',
};

// Coerces anything coming over the wire into a fully-populated, safe settings object.
const coerce = (raw: Partial<SystemSettings> | null | undefined): SystemSettings => {
  const taxPercentage = Number(raw?.taxPercentage);
  const lowStockThreshold = Number(raw?.lowStockThreshold);
  return {
    taxEnabled: Boolean(raw?.taxEnabled ?? DEFAULT_SYSTEM_SETTINGS.taxEnabled),
    taxPercentage: Number.isFinite(taxPercentage)
      ? Math.min(100, Math.max(0, taxPercentage))
      : DEFAULT_SYSTEM_SETTINGS.taxPercentage,
    creditSalesEnabled: Boolean(raw?.creditSalesEnabled ?? DEFAULT_SYSTEM_SETTINGS.creditSalesEnabled),
    lowStockThreshold: Number.isFinite(lowStockThreshold)
      ? Math.max(0, Math.round(lowStockThreshold))
      : DEFAULT_SYSTEM_SETTINGS.lowStockThreshold,
    invoiceLayoutStyle: raw?.invoiceLayoutStyle === 'Thermal' ? 'Thermal' : 'A4',
  };
};

export class SettingsService {
  async getSettings(): Promise<SystemSettings> {
    const response = await apiClient.get('/api/settings');
    return coerce(response.data);
  }

  async updateSettings(settings: SystemSettings): Promise<SystemSettings> {
    const response = await apiClient.put('/api/settings', settings);
    return coerce(response.data);
  }
}

export const settingsService = new SettingsService();