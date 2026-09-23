import React, { useState } from 'react';
import { Users, Search, Building2, Phone, MapPin, Plus, Pencil, Power, Trash2, X } from 'lucide-react';
import { useTenant } from '../../context/TenantContext';

export const CustomersPage: React.FC = () => {
  const { customers, currencySymbol, createCustomer, updateCustomer, setCustomerStatus, deleteCustomer } = useTenant();
  const [searchTerm, setSearchTerm] = useState('');
  const [editingCustomer, setEditingCustomer] = useState<string | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [form, setForm] = useState({ name: '', phone: '', email: '', creditLimit: '0' });
  const [error, setError] = useState('');

  const filteredCustomers = customers.filter(c =>
    (c.name ?? '').toLowerCase().includes(searchTerm.toLowerCase()) ||
    (c.company ?? '').toLowerCase().includes(searchTerm.toLowerCase()) ||
    (c.contactPerson ?? '').toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="space-y-6 animate-in fade-in duration-300">
      
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Users className="w-5 h-5 text-blue-700" /> إدارة العملاء
          </h1>
          <p className="text-xs text-slate-500">
            إدارة ملفات العملاء والحدود الائتمانية وحالة الحساب.
          </p>
        </div>
        <button
          onClick={() => { setEditingCustomer(null); setForm({ name: '', phone: '', email: '', creditLimit: '0' }); setError(''); setIsFormOpen(true); }}
          className="flex items-center gap-2 px-4 py-2 rounded-xl bg-blue-600 text-white text-xs font-bold hover:bg-blue-700"
        >
          <Plus className="w-4 h-4" /> إضافة عميل
        </button>
      </div>

      {isFormOpen && (
        <form onSubmit={async (event) => {
          event.preventDefault();
          setError('');
          try {
            const creditLimit = Number(form.creditLimit);
            if (!Number.isFinite(creditLimit) || creditLimit < 0) throw new Error('الحد الائتماني يجب أن يكون صفرًا أو قيمة موجبة.');
            const data = { ...form, creditLimit };
            if (editingCustomer) await updateCustomer(editingCustomer, data);
            else await createCustomer(data);
            setIsFormOpen(false);
          } catch (err: any) {
            const responseData = err?.response?.data;
            const validationMessage = responseData?.errors
              ? Object.values(responseData.errors).flat().join(' ')
              : responseData?.detail || responseData?.title;
            setError(validationMessage || (err instanceof Error ? err.message : 'تعذر حفظ بيانات العميل.'));
          }
        }} className="p-5 rounded-2xl bg-white border border-blue-200 shadow-sm space-y-4">
          <div className="flex items-center justify-between"><h2 className="font-bold text-slate-900">{editingCustomer ? 'تعديل ملف العميل' : 'إضافة عميل'}</h2><button type="button" onClick={() => setIsFormOpen(false)}><X className="w-4 h-4" /></button></div>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
            <input required value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} placeholder="اسم العميل" className="px-3 py-2 text-xs rounded-xl border border-slate-200" />
            <input required pattern="01[0125][0-9]{8}" value={form.phone} onChange={e => setForm({ ...form, phone: e.target.value })} placeholder="رقم الهاتف المصري" className="px-3 py-2 text-xs rounded-xl border border-slate-200" />
            <input type="email" value={form.email} onChange={e => setForm({ ...form, email: e.target.value })} placeholder="البريد الإلكتروني (اختياري)" className="px-3 py-2 text-xs rounded-xl border border-slate-200" />
            <input required min="0" type="number" value={form.creditLimit} onChange={e => setForm({ ...form, creditLimit: e.target.value })} placeholder="الحد الائتماني" className="px-3 py-2 text-xs rounded-xl border border-slate-200" />
          </div>
          {error && <p className="text-xs font-bold text-rose-600">{error}</p>}
          <button type="submit" className="px-4 py-2 rounded-xl bg-blue-600 text-white text-xs font-bold">حفظ بيانات العميل</button>
        </form>
      )}

      <div className="p-4 rounded-2xl bg-white border border-slate-200 shadow-sm flex items-center justify-between">
        <div className="relative w-full md:w-80">
          <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="البحث باسم الشركة، مسؤول الاتصال..."
            className="w-full pr-9 pl-3 py-1.5 text-xs rounded-xl bg-slate-50 border border-slate-200 text-slate-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <span className="text-xs text-slate-400 font-bold hidden sm:block">
          {customers.length} عميل
        </span>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {filteredCustomers.map(c => (
          <div key={c.id} className="p-5 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-4 hover:border-blue-400 transition">
            <div className="flex justify-between items-start">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-blue-50 text-blue-700 font-bold flex items-center justify-center text-sm shadow-xs">
                  {(c.name ?? '?').substring(0, 2)}
                </div>
                <div>
                  <h3 className="font-bold text-slate-900 text-sm">{c.name ?? '—'}</h3>
                  <p className="text-xs text-slate-500 flex items-center gap-1 mt-0.5">
                    <Building2 className="w-3 h-3" /> {c.company ?? '—'} • <MapPin className="w-3 h-3" /> {c.city ?? '—'}، {c.country ?? '—'}
                  </p>
                </div>
              </div>
              <span className="text-[10px] font-mono font-bold px-2 py-0.5 rounded bg-slate-100 text-slate-600 border border-slate-200">
                {c.code}
              </span>
            </div>

            <div className="p-3 rounded-xl bg-slate-50 border border-slate-100 text-xs space-y-1.5">
              <div className="flex justify-between text-slate-600">
                <span className="flex items-center gap-1"><Phone className="w-3 h-3" /> الهاتف</span>
                <span className="font-semibold text-slate-900 font-mono">{c.phone}</span>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-3 pt-2 border-t border-slate-100 text-xs">
              <div>
                <p className="text-[10px] text-slate-500 font-bold">الحد الائتماني المعتمد</p>
                <p className="font-mono font-extrabold text-slate-900">{Number(c.creditLimit ?? 0).toLocaleString()} {currencySymbol}</p>
              </div>
              <div>
                <p className="text-[10px] text-slate-500 font-bold">المبالغ المستحقة المتبقية</p>
                <p className={`font-mono font-extrabold ${(c.outstandingBalance ?? 0) > 0 ? 'text-amber-600' : 'text-emerald-600'}`}>
                  {Number(c.outstandingBalance ?? 0).toLocaleString()} {currencySymbol}
                </p>
              </div>
            </div>
            <div className="flex items-center justify-between pt-2 border-t border-slate-100">
              <span className={`text-[10px] font-bold ${c.isActive === false ? 'text-rose-600' : 'text-emerald-600'}`}>{c.isActive === false ? 'غير نشط' : 'نشط'}</span>
              <div className="flex items-center gap-1">
                <button title="تعديل الملف" onClick={() => { setEditingCustomer(c.id); setForm({ name: c.name ?? '', phone: c.phone ?? '', email: c.email ?? '', creditLimit: String(c.creditLimit ?? 0) }); setError(''); setIsFormOpen(true); }} className="p-1.5 text-blue-700 hover:bg-blue-50 rounded-lg"><Pencil className="w-3.5 h-3.5" /></button>
                <button title={c.isActive === false ? 'تفعيل الحساب' : 'تعطيل الحساب'} onClick={() => setCustomerStatus(c.id, c.isActive === false)} className="p-1.5 text-amber-700 hover:bg-amber-50 rounded-lg"><Power className="w-3.5 h-3.5" /></button>
                <button title="حذف العميل" onClick={() => { if (window.confirm('هل تريد حذف هذا العميل؟')) deleteCustomer(c.id); }} className="p-1.5 text-rose-700 hover:bg-rose-50 rounded-lg"><Trash2 className="w-3.5 h-3.5" /></button>
              </div>
            </div>
          </div>
        ))}
      </div>

    </div>
  );
};
