import React, { useEffect, useState } from 'react';
import { useTenant } from '../../context/TenantContext';
import { Product } from '../../types';

interface StockTransferModalProps {
  isOpen: boolean;
  onClose: () => void;
  initialProduct?: Product | null;
}

export const StockTransferModal: React.FC<StockTransferModalProps> = ({
  isOpen,
  onClose,
  initialProduct,
}) => {
  const { products, warehouses, transferStock, language } = useTenant();

  const [selectedProductId, setSelectedProductId] = useState(initialProduct?.id || products[0]?.id || '');
  const [sourceWarehouseId, setSourceWarehouseId] = useState(warehouses[0]?.id || '');
  const [targetWarehouseId, setTargetWarehouseId] = useState(warehouses[1]?.id || warehouses[0]?.id || '');
  const [quantity, setQuantity] = useState<number>(5);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!isOpen) return;
    const prod = initialProduct || products[0];
    setSelectedProductId(prod?.id || '');
    setSourceWarehouseId(warehouses[0]?.id || '');
    setTargetWarehouseId(warehouses[1]?.id || warehouses[0]?.id || '');
    setError('');
  }, [isOpen, initialProduct, products, warehouses]);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (sourceWarehouseId === targetWarehouseId) {
      setError(language === 'ar' ? 'لا يمكن التحويل إلى نفس المستودع.' : 'Source and target warehouses cannot be the same.');
      return;
    }
    if (quantity <= 0) {
      setError(language === 'ar' ? 'يرجى إدخال كمية صالحة.' : 'Please enter a valid transfer quantity.');
      return;
    }

    setIsSubmitting(true);
    try {
      await transferStock(sourceWarehouseId, targetWarehouseId, selectedProductId, quantity);
      onClose();
    } catch (err: any) {
      setError(err.response?.data?.detail || err.response?.data?.title || err.message || 'Failed to transfer stock.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/75 backdrop-blur-xs">
      <div className="bg-[#1a1c1f] border border-[#26292e] rounded shadow-2xl w-full max-w-md p-5 space-y-4">
        {/* Header */}
        <div className="flex items-center justify-between pb-3 border-b border-white/5">
          <div className="flex items-center gap-2">
            <span className="material-symbols-outlined text-amber-400">swap_horiz</span>
            <div>
              <h3 className="text-sm font-bold text-white">
                {language === 'ar' ? 'تحويل مخزون بين المستودعات' : 'Inter-Warehouse Stock Transfer'}
              </h3>
              <p className="text-[11px] text-[#8f9194]">Internal logistics relocation</p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-1 rounded text-[#8f9194] hover:text-white hover:bg-[#282a2d]"
          >
            <span className="material-symbols-outlined text-base">close</span>
          </button>
        </div>

        {error && (
          <div className="p-2.5 rounded bg-rose-500/15 border border-rose-500/30 text-rose-400 text-xs">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-3 font-sans">
          <div>
            <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
              {language === 'ar' ? 'الصنف المحول' : 'Product / Instrument'}
            </label>
            <select
              value={selectedProductId}
              onChange={(e) => setSelectedProductId(e.target.value)}
              className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-xs text-white focus:border-[#4edea3] outline-none"
            >
              {products.map(p => (
                <option key={p.id} value={p.id}>
                  {p.sku} - {p.name}
                </option>
              ))}
            </select>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                {language === 'ar' ? 'من مستودع (المصدر)' : 'Source Warehouse'}
              </label>
              <select
                value={sourceWarehouseId}
                onChange={(e) => setSourceWarehouseId(e.target.value)}
                className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-xs text-white focus:border-[#4edea3] outline-none"
              >
                {warehouses.map(w => (
                  <option key={w.id} value={w.id}>
                    {w.name}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                {language === 'ar' ? 'إلى مستودع (الوجهة)' : 'Target Warehouse'}
              </label>
              <select
                value={targetWarehouseId}
                onChange={(e) => setTargetWarehouseId(e.target.value)}
                className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-xs text-white focus:border-[#4edea3] outline-none"
              >
                {warehouses.map(w => (
                  <option key={w.id} value={w.id}>
                    {w.name}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div>
            <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
              {language === 'ar' ? 'الكمية المحولة' : 'Transfer Quantity Units'}
            </label>
            <input
              type="number"
              min="1"
              value={quantity}
              onChange={(e) => setQuantity(Math.max(1, parseInt(e.target.value) || 0))}
              className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-xs font-mono text-white focus:border-[#4edea3] outline-none"
            />
          </div>

          <div className="flex items-center justify-end gap-2 pt-2 border-t border-white/5">
            <button
              type="button"
              onClick={onClose}
              className="px-3 py-1.5 rounded bg-[#282a2d] hover:bg-[#333538] text-[#8f9194] hover:text-white text-xs cursor-pointer"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="px-4 py-1.5 rounded bg-[#ffffff] hover:bg-[#e2e2e4] text-[#111316] font-bold text-xs uppercase tracking-wider transition-all cursor-pointer"
            >
              {isSubmitting ? 'Transferring...' : 'Execute Stock Relocation'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
