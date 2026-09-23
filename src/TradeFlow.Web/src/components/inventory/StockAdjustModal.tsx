import React, { useEffect, useState } from 'react';
import { X, PackagePlus, LoaderCircle } from 'lucide-react';
import { useTenant } from '../../context/TenantContext';
import { Product } from '../../types';
import { getApiErrorMessage } from '../../services/apiClient';

const pickDefaultWarehouseId = (warehouses: { id: string; name?: string }[]) =>
  (warehouses.find(w => w.name?.includes('رئيسي')) || warehouses[0])?.id || '';

interface StockAdjustModalProps {
  isOpen: boolean;
  onClose: () => void;
  initialProduct?: Product | null;
}

export const StockAdjustModal: React.FC<StockAdjustModalProps> = ({
  isOpen,
  onClose,
  initialProduct,
}) => {
  const { products, warehouses, currencySymbol, receiveStock } = useTenant();

  const [selectedProductId, setSelectedProductId] = useState(initialProduct?.id || products[0]?.id || '');
  const [selectedWarehouseId, setSelectedWarehouseId] = useState(pickDefaultWarehouseId(warehouses));
  const [adjustQty, setAdjustQty] = useState<number>(10);
  const [unitCost, setUnitCost] = useState<number>(100);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!isOpen) return;
    setSelectedProductId(initialProduct?.id || products[0]?.id || '');
    setSelectedWarehouseId(pickDefaultWarehouseId(warehouses));
    setError('');
  }, [isOpen, initialProduct, products, warehouses]);

  if (!isOpen) return null;

  const product = products.find(p => p.id === selectedProductId) || products[0];

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!product || !selectedWarehouseId || adjustQty <= 0 || unitCost < 0) {
      setError('يرجى إدخال المستودع والكمية والتكلفة بشكل صحيح.');
      return;
    }

    setIsSubmitting(true);
    try {
      await receiveStock(selectedWarehouseId, product.id, adjustQty, unitCost);
      onClose();
    } catch (err) {
      setError(getApiErrorMessage(err, 'تعذر تسجيل إذن التوريد.'));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-xs animate-in fade-in duration-200">
      <div className="w-full max-w-lg bg-white border border-slate-200 rounded-2xl shadow-2xl p-6 space-y-6 text-right font-sans" dir="rtl">
        
        <div className="flex justify-between items-center border-b border-slate-100 pb-4">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-xl bg-blue-50 text-blue-700">
              <PackagePlus className="w-5 h-5" />
            </div>
            <div>
              <h2 className="text-base font-bold text-slate-900">
                تسجيل إذن توريد مخزني
              </h2>
              <p className="text-xs text-slate-500">
                إضافة وتوريد كميات الأصناف المعتمدة إلى المستودع المحدد
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-2 rounded-xl text-slate-400 hover:text-slate-600 hover:bg-slate-100"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4 text-xs">
          
          <div>
            <label className="block font-bold text-slate-700 mb-1.5">
              اختيار المستودع المستلم
            </label>
            <select
              value={selectedWarehouseId}
              onChange={(e) => setSelectedWarehouseId(e.target.value)}
              className="w-full px-3 py-2 rounded-xl bg-slate-50 border border-slate-200 text-slate-900 font-medium"
            >
              {warehouses.length === 0 ? (
                <option value="">لا توجد مستودعات — أعد تحميل الصفحة</option>
              ) : warehouses.map(w => (
                <option key={w.id} value={w.id}>
                  {w.name} ({w.location})
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block font-bold text-slate-700 mb-1.5">
              اختيار المنتج
            </label>
            <select
              value={selectedProductId}
              onChange={(e) => setSelectedProductId(e.target.value)}
              className="w-full px-3 py-2 rounded-xl bg-slate-50 border border-slate-200 text-slate-900 font-medium"
            >
              {products.map(p => (
                <option key={p.id} value={p.id}>
                  {p.name} [{p.sku}] 
                </option>
              ))}
            </select>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block font-bold text-slate-700 mb-1.5">
                الكمية الموردة
              </label>
              <input
                type="number"
                min={1}
                value={adjustQty}
                onChange={(e) => setAdjustQty(Math.max(1, Number.parseInt(e.target.value, 10) || 1))}
                onBlur={() => setAdjustQty(current => Math.max(1, current || 1))}
                className="w-full px-3 py-2 rounded-xl bg-slate-50 border border-slate-200 text-slate-900 font-mono text-center font-bold text-sm"
              />
            </div>

            <div>
              <label className="block font-bold text-slate-700 mb-1.5">
                تكلفة الوحدة ({currencySymbol})
              </label>
              <input
                type="number"
                min={1}
                value={unitCost}
                onChange={(e) => setUnitCost(Math.max(1, Number.parseFloat(e.target.value) || 1))}
                onBlur={() => setUnitCost(current => Math.max(1, current || 1))}
                className="w-full px-3 py-2 rounded-xl bg-slate-50 border border-slate-200 text-slate-900 font-mono text-center font-bold text-sm"
              />
            </div>
          </div>

          {error && <p className="text-xs font-bold text-rose-600">{error}</p>}

          <div className="pt-4 border-t border-slate-100 flex justify-end gap-3">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 rounded-xl font-bold text-slate-600 hover:bg-slate-100"
            >
              إلغاء
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="flex items-center gap-2 px-5 py-2 rounded-xl bg-blue-600 hover:bg-blue-700 text-white font-bold shadow-sm"
            >
              {isSubmitting && <LoaderCircle className="w-3.5 h-3.5 animate-spin" />} {isSubmitting ? 'جارٍ تسجيل التوريد' : 'تسجيل التوريد'}
            </button>
          </div>

        </form>
      </div>
    </div>
  );
};
