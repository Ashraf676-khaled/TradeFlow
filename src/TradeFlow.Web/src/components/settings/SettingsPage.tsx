import React, { useEffect, useState } from 'react';
import { Settings, Save, Percent, CreditCard, AlertTriangle, Printer, CheckCircle2, LoaderCircle } from 'lucide-react';
import { useTenant } from '../../context/TenantContext';
import { SystemSettings } from '../../types';
import { getApiErrorMessage } from '../../services/apiClient';

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
    className={`relative inline-flex h-6 w-11 shrink-0 items-center rounded-full transition ${
      enabled ? 'bg-blue-600 justify-end' : 'bg-slate-300 justify-start'
    }`}
  >
    <span className="mx-0.5 inline-block h-5 w-5 transform rounded-full bg-white shadow transition" />
  </button>
);

export const SettingsPage: React.FC = () => {
  const { settings, updateSettings } = useTenant();

  const [draft, setDraft] = useState<SystemSettings>(settings);
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState<{ ok: boolean; text: string } | null>(null);

  // Keep the form in sync whenever fresh settings arrive (initial load / refresh).
  useEffect(() => {
    setDraft(settings);
  }, [settings]);

  const handleSave = async () => {
    setMessage(null);

    const taxPercentage = Number(draft.taxPercentage);
    const lowStockThreshold = Number(draft.lowStockThreshold);
    if (!Number.isFinite(taxPercentage) || taxPercentage < 0 || taxPercentage > 100 || (draft.taxEnabled && taxPercentage <= 0)) {
      setMessage({ ok: false, text: 'أدخل نسبة ضريبة صحيحة أكبر من صفر (وأقل من أو تساوي 100).' });
      return;
    }
    if (!Number.isFinite(lowStockThreshold) || lowStockThreshold < 0) {
      setMessage({ ok: false, text: 'حد تنبيه المخزون يجب أن يكون صفرًا أو أكثر.' });
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
      setMessage({ ok: true, text: 'تم حفظ إعدادات النظام بنجاح.' });
    } catch (error) {
      setMessage({ ok: false, text: getApiErrorMessage(error, 'تعذر حفظ الإعدادات.') });
    } finally {
      setIsSaving(false);
    }
  };

  const cardClass = 'rounded-2xl bg-white border border-slate-200 shadow-sm p-6 space-y-4';

  return (
    <div className="space-y-6 animate-in fade-in duration-300" dir="rtl">
      {/* Title */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Settings className="w-5 h-5 text-blue-700" /> إعدادات النظام
          </h1>
          <p className="text-xs text-slate-500">
            تحكم ديناميكي في الضريبة، البيع الآجل، تنبيهات المخزون وتنسيق طباعة الفواتير — بدون تعديل الكود.
          </p>
        </div>

        <button
          onClick={handleSave}
          disabled={isSaving}
          className="flex items-center justify-center gap-2 rounded-xl bg-blue-600 px-5 py-2.5 text-xs font-bold text-white hover:bg-blue-700 disabled:opacity-60 shadow-sm transition"
        >
          {isSaving ? <LoaderCircle className="h-4 w-4 animate-spin" /> : <Save className="h-4 w-4" />}
          {isSaving ? 'جارٍ الحفظ...' : 'حفظ الإعدادات'}
        </button>
      </div>

      {message && (
        <div
          className={`p-3 rounded-xl text-xs font-bold flex items-center gap-2 ${
            message.ok ? 'bg-emerald-50 border border-emerald-200 text-emerald-700' : 'bg-rose-50 border border-rose-200 text-rose-700'
          }`}
        >
          <CheckCircle2 className="w-4 h-4 shrink-0" />
          {message.text}
        </div>
      )}

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* VAT / Tax */}
        <section className={cardClass}>
          <div className="flex items-center justify-between border-b border-slate-100 pb-3">
            <div className="flex items-center gap-3">
              <div className="p-2.5 rounded-xl bg-blue-50 text-blue-700"><Percent className="w-5 h-5" /></div>
              <div>
                <h2 className="text-sm font-bold text-slate-900">ضريبة القيمة المضافة (VAT)</h2>
                <p className="text-[11px] text-slate-500">تفعيل/تعطيل احتساب الضريبة وتحديد نسبتها.</p>
              </div>
            </div>
            <Toggle enabled={draft.taxEnabled} onChange={v => setDraft(d => ({ ...d, taxEnabled: v }))} label="تفعيل الضريبة" />
          </div>

          <div className="space-y-1">
            <label className="block text-[10px] font-bold text-slate-600">نسبة الضريبة (%)</label>
            <input
              type="number"
              min={1}
              max={100}
              disabled={!draft.taxEnabled}
              value={draft.taxPercentage}
              onChange={e => setDraft(d => ({ ...d, taxPercentage: Number(e.target.value) }))}
              className="w-full rounded-xl border border-slate-200 px-3 py-2 text-xs font-mono font-bold disabled:bg-slate-50 disabled:text-slate-400"
            />
            <p className="text-[10px] text-slate-400">
              الأسعار الحالية تُعامل كأسعار شاملة للضريبة، ويُعرض تفصيلها في الفاتورة وشاشة الطباعة.
            </p>
          </div>
        </section>

        {/* Credit / Deferred Sales */}
        <section className={cardClass}>
          <div className="flex items-center justify-between border-b border-slate-100 pb-3">
            <div className="flex items-center gap-3">
              <div className="p-2.5 rounded-xl bg-emerald-50 text-emerald-700"><CreditCard className="w-5 h-5" /></div>
              <div>
                <h2 className="text-sm font-bold text-slate-900">البيع الآجل / الدفع المؤجل (الأجل)</h2>
                <p className="text-[11px] text-slate-500">السماح بإصدار الفواتير كرصيد مستحق على العميل.</p>
              </div>
            </div>
            <Toggle enabled={draft.creditSalesEnabled} onChange={v => setDraft(d => ({ ...d, creditSalesEnabled: v }))} label="تفعيل البيع الآجل" />
          </div>

          <div className={`p-3 rounded-xl border text-[11px] leading-relaxed ${
            draft.creditSalesEnabled
              ? 'bg-blue-50 border-blue-100 text-blue-800'
              : 'bg-amber-50 border-amber-100 text-amber-800'
          }`}>
            {draft.creditSalesEnabled
              ? 'مفعّل: عند تأكيد الطلب تُصدر الفاتورة غير المدفوعة (رصيد مستحق على العميل) ويمكن تحصيلها لاحقًا.'
              : 'معطّل (بيع نقدي/POS): عند تأكيد الطلب يُحصَّل المبلغ كاملًا وتُصدر الفاتورة كمدفوعة تلقائيًا في نفس عملية التأكيد.'}
          </div>
        </section>

        {/* Low stock threshold */}
        <section className={cardClass}>
          <div className="flex items-center gap-3 border-b border-slate-100 pb-3">
            <div className="p-2.5 rounded-xl bg-amber-50 text-amber-700"><AlertTriangle className="w-5 h-5" /></div>
            <div>
              <h2 className="text-sm font-bold text-slate-900">حد تنبيه المخزون المنخفض</h2>
              <p className="text-[11px] text-slate-500">الحد الافتراضي الذي تُطلق عنده تنبيهات إعادة الطلب.</p>
            </div>
          </div>

          <div className="space-y-1">
            <label className="block text-[10px] font-bold text-slate-600">الحد الافتراضي (عدد الوحدات)</label>
            <input
              type="number"
              min={0}
              value={draft.lowStockThreshold}
              onChange={e => setDraft(d => ({ ...d, lowStockThreshold: Number(e.target.value) }))}
              className="w-full rounded-xl border border-slate-200 px-3 py-2 text-xs font-mono font-bold"
            />
            <p className="text-[10px] text-slate-400">
              يُستخدم للأصناف التي لم يُحدد لها حد مخصص، ويُستخدم أيضًا كقيمة افتراضي جديدة في نموذج إضافة الصنف.
            </p>
          </div>
        </section>

        {/* Invoice print layout */}
        <section className={cardClass}>
          <div className="flex items-center gap-3 border-b border-slate-100 pb-3">
            <div className="p-2.5 rounded-xl bg-violet-50 text-violet-700"><Printer className="w-5 h-5" /></div>
            <div>
              <h2 className="text-sm font-bold text-slate-900">تنسيق طباعة الفاتورة الافتراضي</h2>
              <p className="text-[11px] text-slate-500">يُستخدم كوضع معاينة وطباعة افتراضي للفاتورة.</p>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <button
              type="button"
              onClick={() => setDraft(d => ({ ...d, invoiceLayoutStyle: 'A4' }))}
              className={`rounded-xl border p-4 text-right transition ${
                draft.invoiceLayoutStyle === 'A4'
                  ? 'border-blue-600 bg-blue-50 ring-2 ring-blue-600/20'
                  : 'border-slate-200 hover:border-slate-300'
              }`}
            >
              <Printer className="w-4 h-4 mb-2 text-blue-700" />
              <p className="text-xs font-bold text-slate-900">قياسي A4</p>
              <p className="text-[10px] text-slate-500">طباعة على ورق A4 للأرشيف والفوترة الرسمية</p>
            </button>

            <button
              type="button"
              onClick={() => setDraft(d => ({ ...d, invoiceLayoutStyle: 'Thermal' }))}
              className={`rounded-xl border p-4 text-right transition ${
                draft.invoiceLayoutStyle === 'Thermal'
                  ? 'border-blue-600 bg-blue-50 ring-2 ring-blue-600/20'
                  : 'border-slate-200 hover:border-slate-300'
              }`}
            >
              <Printer className="w-4 h-4 mb-2 text-violet-700" />
              <p className="text-xs font-bold text-slate-900">حراري 80mm</p>
              <p className="text-[10px] text-slate-500">رول حراري للكاشير ونقاط البيع السريعة</p>
            </button>
          </div>
        </section>
      </div>
    </div>
  );
};