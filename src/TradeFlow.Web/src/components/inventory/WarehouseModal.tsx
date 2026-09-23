import React, { useState } from 'react';
import { X, Warehouse, MapPin, CheckCircle2 } from 'lucide-react';
import { useTenant } from '../../context/TenantContext';
import { getApiErrorMessage } from '../../services/apiClient';

interface WarehouseModalProps {
  isOpen: boolean;
  onClose: () => void;
}

/**
 * Professional RTL dialog for creating a warehouse. Wraps the existing
 * `createWarehouse` context action (POST /api/warehouses) and mirrors the
 * backend limits: Name ≤ 150 chars, Location ≤ 300 chars, both required.
 */
export const WarehouseModal: React.FC<WarehouseModalProps> = ({ isOpen, onClose }) => {
  const { createWarehouse } = useTenant();

  const [name, setName] = useState('');
  const [location, setLocation] = useState('');
  const [error, setError] = useState('');
  const [isSaving, setIsSaving] = useState(false);

  if (!isOpen) return null;

  const resetAndClose = () => {
    setName('');
    setLocation('');
    setError('');
    onClose();
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();

    const trimmedName = name.trim();
    const trimmedLocation = location.trim();

    if (!trimmedName || !trimmedLocation) {
      setError('اسم المستودع وموقعه مطلوبان.');
      return;
    }

    setIsSaving(true);
    setError('');
    try {
      await createWarehouse({ name: trimmedName, location: trimmedLocation });
      resetAndClose();
    } catch (err) {
      setError(getApiErrorMessage(err, 'تعذر حفظ المستودع.'));
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-xs animate-in fade-in duration-200" dir="rtl">
      <form
        onSubmit={handleSubmit}
        className="w-full max-w-md bg-white border border-slate-200 rounded-2xl shadow-2xl overflow-hidden text-right"
      >
        {/* Header */}
        <div className="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
          <div className="flex items-center gap-2.5">
            <span className="p-2 rounded-xl bg-indigo-50 text-indigo-700">
              <Warehouse className="w-4 h-4" />
            </span>
            <div>
              <h2 className="text-sm font-extrabold text-slate-900">إضافة مستودع جديد</h2>
              <p className="text-[10px] text-slate-400 font-bold">سيظهر فوراً في قوائم التوريد وأوامر البيع</p>
            </div>
          </div>
          <button
            type="button"
            onClick={resetAndClose}
            className="p-2 rounded-xl text-slate-400 hover:text-slate-600 hover:bg-slate-100 transition"
            title="إغلاق"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Body */}
        <div className="p-6 space-y-4">
          <div className="space-y-1">
            <label className="block text-[10px] font-bold text-slate-600">اسم المستودع *</label>
            <input
              required
              maxLength={150}
              autoFocus
              placeholder="مثال: مستودع القاهرة الرئيسي"
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="w-full rounded-xl border border-slate-200 px-3 py-2 text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div className="space-y-1">
            <label className="block text-[10px] font-bold text-slate-600">الموقع *</label>
            <div className="relative">
              <MapPin className="absolute right-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-slate-400" />
              <input
                required
                maxLength={300}
                placeholder="مثال: المنطقة الصناعية، القاهرة"
                value={location}
                onChange={(e) => setLocation(e.target.value)}
                className="w-full pr-8 pl-3 py-2 text-xs rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>
          </div>

          {error && (
            <p className="text-xs font-bold text-rose-600 rounded-xl bg-rose-50 border border-rose-100 px-3 py-2">
              {error}
            </p>
          )}
        </div>

        {/* Footer */}
        <div className="px-6 py-4 border-t border-slate-100 flex items-center justify-end gap-2">
          <button
            type="button"
            onClick={resetAndClose}
            className="rounded-xl border border-slate-200 px-4 py-2 text-xs font-bold text-slate-600 hover:bg-slate-50 transition"
          >
            إلغاء
          </button>
          <button
            type="submit"
            disabled={isSaving}
            className="flex items-center gap-1.5 rounded-xl bg-indigo-600 px-5 py-2 text-xs font-bold text-white hover:bg-indigo-700 transition disabled:opacity-60"
          >
            <CheckCircle2 className={`w-3.5 h-3.5 ${isSaving ? 'animate-spin' : ''}`} />
            {isSaving ? 'جارٍ الحفظ...' : 'حفظ المستودع'}
          </button>
        </div>
      </form>
    </div>
  );
};
