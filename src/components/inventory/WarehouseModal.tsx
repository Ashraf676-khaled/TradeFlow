import React, { useState } from 'react';
import { useTenant } from '../../context/TenantContext';

interface WarehouseModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const WarehouseModal: React.FC<WarehouseModalProps> = ({ isOpen, onClose }) => {
  const { createWarehouse, language } = useTenant();

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
      setError(language === 'ar' ? 'اسم المستودع وموقعه مطلوبان.' : 'Warehouse name and location are required.');
      return;
    }

    setIsSaving(true);
    setError('');
    try {
      await createWarehouse({ name: trimmedName, location: trimmedLocation });
      resetAndClose();
    } catch (err: any) {
      setError(err.response?.data?.detail || err.response?.data?.title || err.message || 'Failed to save warehouse.');
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/75 backdrop-blur-xs">
      <form
        onSubmit={handleSubmit}
        className="w-full max-w-md bg-[#1a1c1f] border border-[#26292e] rounded shadow-2xl p-5 space-y-4"
      >
        <div className="flex items-center justify-between pb-3 border-b border-white/5">
          <div className="flex items-center gap-2">
            <span className="material-symbols-outlined text-[#4edea3]">warehouse</span>
            <div>
              <h3 className="text-sm font-bold text-white">
                {language === 'ar' ? 'إضافة مستودع جديد' : 'New Warehouse Facility'}
              </h3>
              <p className="text-[11px] text-[#8f9194]">Storage facility and logistics hub</p>
            </div>
          </div>
          <button
            type="button"
            onClick={resetAndClose}
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

        <div className="space-y-3 font-sans">
          <div>
            <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
              {language === 'ar' ? 'اسم المستودع' : 'Warehouse Name'} *
            </label>
            <input
              type="text"
              required
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="e.g. Central Distribution Hub A"
              className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-xs text-white focus:border-[#4edea3] outline-none"
            />
          </div>

          <div>
            <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
              {language === 'ar' ? 'الموقع الجغرافي / العنوان' : 'Location / Address'} *
            </label>
            <input
              type="text"
              required
              value={location}
              onChange={(e) => setLocation(e.target.value)}
              placeholder="e.g. Zone 4, Port Logistics Park, Cairo"
              className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-xs text-white focus:border-[#4edea3] outline-none"
            />
          </div>
        </div>

        <div className="flex items-center justify-end gap-2 pt-2 border-t border-white/5">
          <button
            type="button"
            onClick={resetAndClose}
            className="px-3 py-1.5 rounded bg-[#282a2d] hover:bg-[#333538] text-[#8f9194] hover:text-white text-xs cursor-pointer"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={isSaving}
            className="px-4 py-1.5 rounded bg-[#ffffff] hover:bg-[#e2e2e4] text-[#111316] font-bold text-xs uppercase tracking-wider transition-all cursor-pointer"
          >
            {isSaving ? 'Registering...' : 'Register Warehouse'}
          </button>
        </div>
      </form>
    </div>
  );
};
