import React, { useEffect, useState } from 'react';
import { useTenant } from '../../context/TenantContext';
import { SystemSettings } from '../../types';

interface ToggleProps {
  enabled: boolean;
  onChange: (value: boolean) => void;
  label: string;
}

const Toggle: React.FC<ToggleProps> = ({ enabled, onChange, label }) => (
  <button
    type="button"
    role="switch"
    aria-checked={enabled}
    aria-label={label}
    onClick={() => onChange(!enabled)}
    className={`relative inline-flex h-6 w-11 shrink-0 items-center rounded-full transition-colors cursor-pointer ${
      enabled ? 'bg-[#10b981]' : 'bg-[#282a2d]'
    }`}
  >
    <span
      className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
        enabled ? 'translate-x-6' : 'translate-x-1'
      }`}
    />
  </button>
);

export const SettingsPage: React.FC = () => {
  const { settings, updateSettings, language } = useTenant();

  const [draft, setDraft] = useState<SystemSettings>(settings);
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState<{ ok: boolean; text: string } | null>(null);

  useEffect(() => {
    setDraft(settings);
  }, [settings]);

  const handleSave = async () => {
    setMessage(null);

    const taxPercentage = Number(draft.taxPercentage);
    const lowStockThreshold = Number(draft.lowStockThreshold);
    if (!Number.isFinite(taxPercentage) || taxPercentage < 0 || taxPercentage > 100 || (draft.taxEnabled && taxPercentage <= 0)) {
      setMessage({ ok: false, text: language === 'ar' ? 'أدخل نسبة ضريبة صحيحة أكبر من صفر.' : 'Please enter a valid tax percentage between 0 and 100.' });
      return;
    }
    if (!Number.isFinite(lowStockThreshold) || lowStockThreshold < 0) {
      setMessage({ ok: false, text: language === 'ar' ? 'حد تنبيه المخزون يجب أن يكون صفرًا أو أكثر.' : 'Stock warning threshold must be 0 or greater.' });
      return;
    }

    setIsSaving(true);
    try {
      await updateSettings({
        taxEnabled: draft.taxEnabled,
        taxPercentage,
        creditSalesEnabled: draft.creditSalesEnabled,
        lowStockThreshold: Math.round(lowStockThreshold),
        invoiceLayoutStyle: draft.invoiceLayoutStyle,
      });
      setMessage({ ok: true, text: language === 'ar' ? 'تم حفظ إعدادات النظام بنجاح.' : 'System configuration saved successfully.' });
      setTimeout(() => setMessage(null), 3000);
    } catch (err: any) {
      setMessage({ ok: false, text: err.response?.data?.detail || err.response?.data?.title || err.message || 'Failed to update settings.' });
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="space-y-4 max-w-4xl">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-white tracking-tight">
            {language === 'ar' ? 'إعدادات النظام والتهيئة' : 'System Configuration & Settings'}
          </h1>
          <p className="text-xs text-[#8f9194] mt-0.5">
            {language === 'ar'
              ? 'ضبط نسب الضرائب، البيع الآجل، تنبيهات إعادة الطلب وقوالب الطباعة'
              : 'Global institutional parameters, tax rates, risk limits, and invoice rendering templates'}
          </p>
        </div>

        <button
          onClick={handleSave}
          disabled={isSaving}
          className="flex items-center gap-1.5 px-4 py-2 bg-[#ffffff] hover:bg-[#e2e2e4] text-[#111316] font-bold rounded text-xs transition-all shadow-md cursor-pointer"
        >
          <span className="material-symbols-outlined text-sm font-bold">save</span>
          <span>{isSaving ? 'Saving...' : 'Save Settings'}</span>
        </button>
      </div>

      {message && (
        <div
          className={`p-3 rounded text-xs ${
            message.ok
              ? 'bg-[#10b981]/15 text-[#4edea3] border border-[#10b981]/30'
              : 'bg-rose-500/15 text-rose-400 border border-rose-500/30'
          }`}
        >
          {message.text}
        </div>
      )}

      {/* Settings Sections */}
      <div className="space-y-4 font-sans text-xs">
        {/* Tax Settings */}
        <div className="p-4 rounded bg-[#1a1c1f] border border-white/5 space-y-3">
          <div className="flex items-start justify-between">
            <div className="space-y-0.5">
              <h2 className="text-sm font-bold text-white flex items-center gap-2">
                <span className="material-symbols-outlined text-[#4edea3] text-base">percent</span>
                {language === 'ar' ? 'ضريبة القيمة المضافة (VAT)' : 'Value Added Tax (VAT)'}
              </h2>
              <p className="text-[#8f9194]">
                Enable automatic tax calculations on sales orders and generated billing invoices.
              </p>
            </div>
            <Toggle
              enabled={draft.taxEnabled}
              onChange={(enabled) => setDraft({ ...draft, taxEnabled: enabled })}
              label="Enable Tax"
            />
          </div>

          {draft.taxEnabled && (
            <div className="pt-3 border-t border-white/5 flex items-center gap-3">
              <label className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
                VAT Percentage (%)
              </label>
              <input
                type="number"
                step="0.1"
                min="0"
                max="100"
                value={draft.taxPercentage}
                onChange={(e) => setDraft({ ...draft, taxPercentage: parseFloat(e.target.value) || 0 })}
                className="w-24 px-2 py-1 bg-[#111316] border border-[#26292e] rounded text-white font-mono outline-none focus:border-[#4edea3]"
              />
            </div>
          )}
        </div>

        {/* Credit Facilities */}
        <div className="p-4 rounded bg-[#1a1c1f] border border-white/5 space-y-3">
          <div className="flex items-start justify-between">
            <div className="space-y-0.5">
              <h2 className="text-sm font-bold text-white flex items-center gap-2">
                <span className="material-symbols-outlined text-[#4edea3] text-base">credit_card</span>
                {language === 'ar' ? 'البيع الآجل والتسهيلات الائتمانية' : 'Credit Sales & Revolving Accounts'}
              </h2>
              <p className="text-[#8f9194]">
                Permit checkout on account without immediate settlement up to customer credit limits.
              </p>
            </div>
            <Toggle
              enabled={draft.creditSalesEnabled}
              onChange={(enabled) => setDraft({ ...draft, creditSalesEnabled: enabled })}
              label="Enable Credit Sales"
            />
          </div>
        </div>

        {/* Low Stock Alerts */}
        <div className="p-4 rounded bg-[#1a1c1f] border border-white/5 space-y-3">
          <div className="flex items-start justify-between">
            <div className="space-y-0.5">
              <h2 className="text-sm font-bold text-white flex items-center gap-2">
                <span className="material-symbols-outlined text-amber-400 text-base">warning</span>
                {language === 'ar' ? 'حد تنبيه انخفاض المخزون' : 'Default Low-Stock Threshold'}
              </h2>
              <p className="text-[#8f9194]">
                Inventory items at or below this quantity trigger low-stock alerts across dashboards.
              </p>
            </div>
            <div className="flex items-center gap-2">
              <input
                type="number"
                min="0"
                value={draft.lowStockThreshold}
                onChange={(e) => setDraft({ ...draft, lowStockThreshold: parseInt(e.target.value) || 0 })}
                className="w-20 px-2 py-1 bg-[#111316] border border-[#26292e] rounded text-white font-mono text-center outline-none focus:border-[#4edea3]"
              />
              <span className="text-[#8f9194]">units</span>
            </div>
          </div>
        </div>

        {/* Invoice Layout Template */}
        <div className="p-4 rounded bg-[#1a1c1f] border border-white/5 space-y-3">
          <div className="space-y-0.5">
            <h2 className="text-sm font-bold text-white flex items-center gap-2">
              <span className="material-symbols-outlined text-[#4edea3] text-base">print</span>
              {language === 'ar' ? 'قالب طباعة الفواتير' : 'Invoice Voucher Print Format'}
            </h2>
            <p className="text-[#8f9194]">
              Select default layout between standard institutional A4 format and 80mm POS thermal receipt.
            </p>
          </div>

          <div className="grid grid-cols-2 gap-3 pt-2">
            <div
              onClick={() => setDraft({ ...draft, invoiceLayoutStyle: 'A4' })}
              className={`p-3 rounded border cursor-pointer transition-all ${
                draft.invoiceLayoutStyle !== 'Thermal'
                  ? 'bg-[#282a2d] border-[#4edea3]'
                  : 'bg-[#111316] border-[#26292e] hover:border-white/20'
              }`}
            >
              <div className="font-bold text-white mb-1">Standard A4 Format</div>
              <div className="text-[11px] text-[#8f9194]">
                Comprehensive formal invoice layout for enterprise clients.
              </div>
            </div>

            <div
              onClick={() => setDraft({ ...draft, invoiceLayoutStyle: 'Thermal' })}
              className={`p-3 rounded border cursor-pointer transition-all ${
                draft.invoiceLayoutStyle === 'Thermal'
                  ? 'bg-[#282a2d] border-[#4edea3]'
                  : 'bg-[#111316] border-[#26292e] hover:border-white/20'
              }`}
            >
              <div className="font-bold text-white mb-1">80mm Thermal Receipt (POS)</div>
              <div className="text-[11px] text-[#8f9194]">
                Compact receipt format for direct point-of-sale thermal printers.
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
