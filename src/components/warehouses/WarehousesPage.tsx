import React, { useState } from 'react';
import { useTenant } from '../../context/TenantContext';
import { WarehouseModal } from '../inventory/WarehouseModal';

export const WarehousesPage: React.FC = () => {
  const { warehouses, products, formatCurrency, language } = useTenant();
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');

  const filteredWarehouses = warehouses.filter(w =>
    (w.name || '').toLowerCase().includes(searchQuery.toLowerCase()) ||
    (w.location || '').toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="space-y-4">
      {/* Header Banner */}
      <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-white tracking-tight">
            {language === 'ar' ? 'المستودعات والمراكز اللوجستية' : 'Warehouses & Logistics Hubs'}
          </h1>
          <p className="text-xs text-[#8f9194] mt-0.5">
            {language === 'ar'
              ? 'إدارة مراكز التخزين الإقليمية وتوزيع البضائع والمستودعات المركزية'
              : 'Multi-location facility management, capacity utilization, and storage operations'}
          </p>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={() => setIsCreateOpen(true)}
            className="flex items-center gap-1.5 px-3.5 py-1.5 bg-[#ffffff] hover:bg-[#e2e2e4] text-[#111316] font-semibold rounded text-xs transition-all shadow-md cursor-pointer"
          >
            <span className="material-symbols-outlined text-sm font-bold">add</span>
            <span>{language === 'ar' ? 'إضافة مستودع' : 'Add Warehouse'}</span>
          </button>
        </div>
      </div>

      {/* Facilities Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
        {filteredWarehouses.map(wh => {
          const warehouseProducts = products.filter(p => p.warehouseId === wh.id);
          const totalUnits = warehouseProducts.reduce((acc, p) => acc + (p.currentStock || 0), 0);
          const totalValuation = warehouseProducts.reduce((acc, p) => acc + ((p.currentStock || 0) * (p.unitPrice || 0)), 0);

          return (
            <div key={wh.id} className="p-4 rounded bg-[#1a1c1f] border border-white/5 space-y-4 shadow-sm">
              <div className="flex items-start justify-between">
                <div className="flex items-center gap-2.5">
                  <div className="w-9 h-9 rounded bg-[#282a2d] border border-white/10 flex items-center justify-center text-[#4edea3]">
                    <span className="material-symbols-outlined text-xl">warehouse</span>
                  </div>
                  <div>
                    <h3 className="text-sm font-bold text-white">{wh.name}</h3>
                    <p className="text-xs text-[#8f9194] flex items-center gap-1">
                      <span className="material-symbols-outlined text-xs">location_on</span>
                      {wh.location}
                    </p>
                  </div>
                </div>

                <span className="px-2 py-0.5 rounded-full text-[10px] font-semibold bg-[#10b981]/15 text-[#4edea3] border border-[#10b981]/30">
                  Operational
                </span>
              </div>

              {/* Warehouse Metrics */}
              <div className="grid grid-cols-2 gap-2 p-3 bg-[#111316] rounded border border-white/5 font-mono text-xs">
                <div>
                  <div className="text-[10px] uppercase text-[#8f9194]">Stock Units</div>
                  <div className="text-white font-bold text-sm mt-0.5">{totalUnits}</div>
                </div>
                <div>
                  <div className="text-[10px] uppercase text-[#8f9194]">Held Value</div>
                  <div className="text-[#4edea3] font-bold text-sm mt-0.5">{formatCurrency(totalValuation)}</div>
                </div>
              </div>

              {/* Storage Capacity Gauge */}
              <div className="space-y-1">
                <div className="flex justify-between text-[11px] text-[#8f9194]">
                  <span>Capacity Utilization</span>
                  <span className="text-white font-mono">{Math.min(94, 20 + totalUnits * 2)}%</span>
                </div>
                <div className="h-1.5 w-full bg-[#282a2d] rounded-full overflow-hidden">
                  <div
                    className="h-full bg-[#4edea3] rounded-full"
                    style={{ width: `${Math.min(94, 20 + totalUnits * 2)}%` }}
                  ></div>
                </div>
              </div>

              {/* Stock Items Sample */}
              <div className="pt-2 border-t border-white/5 text-[11px] text-[#8f9194]">
                <div className="flex justify-between items-center mb-1">
                  <span>Assigned Instruments</span>
                  <span className="text-white font-mono">{warehouseProducts.length} items</span>
                </div>
                <div className="flex flex-wrap gap-1">
                  {warehouseProducts.slice(0, 4).map(p => (
                    <span key={p.id} className="px-1.5 py-0.5 rounded bg-[#282a2d] text-white font-mono text-[10px]">
                      {p.sku}
                    </span>
                  ))}
                  {warehouseProducts.length > 4 && (
                    <span className="text-[10px] text-[#8f9194] self-center">
                      +{warehouseProducts.length - 4} more
                    </span>
                  )}
                </div>
              </div>
            </div>
          );
        })}
      </div>

      <WarehouseModal
        isOpen={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
      />
    </div>
  );
};
