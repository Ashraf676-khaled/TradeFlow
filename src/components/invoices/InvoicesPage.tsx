import React, { useState } from 'react';
import { 
  FileText, 
  Search, 
  CheckCircle2, 
  AlertTriangle, 
  Eye, 
  Clock,
  CreditCard
} from 'lucide-react';
import { useTenant } from '../../context/TenantContext';
import { Invoice, PaymentStatus } from '../../types';
import { InvoiceDetailModal } from './InvoiceDetailModal';

export const InvoicesPage: React.FC = () => {
  const { invoices, currencySymbol } = useTenant();

  const [activeTab, setActiveTab] = useState<string>('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedInvoice, setSelectedInvoice] = useState<Invoice | null>(null);

  // Compute metrics
  const totalBilled = invoices.reduce((sum, i) => sum + (i.totalAmount ?? 0), 0);
  const totalCollected = invoices.reduce((sum, i) => sum + (i.paidAmount ?? 0), 0);
  const totalOutstanding = invoices.reduce((sum, i) => sum + (i.balanceDue ?? 0), 0);
  const overdueCount = invoices.filter(i => i.status === 'متأخر').length;

  const filteredInvoices = invoices.filter(inv => {
    const matchesTab = activeTab === 'all' || inv.status === activeTab;
    const q = searchTerm.toLowerCase();
    const invNum = (inv.invoiceNumber ?? '').toLowerCase();
    const cust = (inv.customerName ?? '').toLowerCase();
    const ordNum = (inv.orderNumber ?? '').toLowerCase();
    const matchesSearch = invNum.includes(q) || cust.includes(q) || ordNum.includes(q);
    return matchesTab && matchesSearch;
  });

  const getStatusBadge = (status: PaymentStatus) => {
    switch (status) {
      case 'مدفوع':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-emerald-100 text-emerald-800 border border-emerald-200">
            <CheckCircle2 className="w-3 h-3" /> مدفوع بالكامل
          </span>
        );
      case 'مدفوع جزئياً':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-blue-100 text-blue-800 border border-blue-200">
            <CreditCard className="w-3 h-3" /> مدفوع جزئياً
          </span>
        );
      case 'غير مدفوع':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-amber-100 text-amber-800 border border-amber-200">
            <Clock className="w-3 h-3" /> غير مدفوع
          </span>
        );
      case 'متأخر':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-rose-100 text-rose-800 border border-rose-200">
            <AlertTriangle className="w-3 h-3 text-rose-600" /> متأخر عن السداد
          </span>
        );
      case 'ملغاة':
        return (
          <span className="inline-flex items-center gap-1 rounded-full border border-slate-200 bg-slate-100 px-2.5 py-0.5 text-[10px] font-bold text-slate-700">
            ملغاة
          </span>
        );
      default:
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-slate-100 text-slate-700">
            {status || 'غير محدد'}
          </span>
        );
    }
  };

  return (
    <div className="space-y-6 animate-in fade-in duration-300">
      
      {/* Title */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <FileText className="w-5 h-5 text-blue-700" /> الفواتير الحسابية والتحصيل
          </h1>
          <p className="text-xs text-slate-500">
            متابعة التحصيلات المالية، سداد المستحقات، والفواتير الضريبية للعملاء.
          </p>
        </div>
      </div>

      {/* Metric Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="p-4 rounded-2xl bg-white border border-slate-200 shadow-2xs">
          <p className="text-xs font-bold text-slate-500">إجمالي الفواتير المفلترة</p>
          <h3 className="text-xl font-extrabold text-slate-900 mt-1">
            {totalBilled.toLocaleString(undefined, { minimumFractionDigits: 2 })} <span className="text-xs font-normal text-slate-500">{currencySymbol}</span>
          </h3>
          <p className="text-[10px] text-slate-400 mt-1">{invoices.length} سجل فاتورة صادرة</p>
        </div>

        <div className="p-4 rounded-2xl bg-white border border-slate-200 shadow-2xs">
          <p className="text-xs font-bold text-slate-500">المبالغ المحصلة فعلياً</p>
          <h3 className="text-xl font-extrabold text-emerald-700 mt-1">
            {totalCollected.toLocaleString(undefined, { minimumFractionDigits: 2 })} <span className="text-xs font-normal text-slate-500">{currencySymbol}</span>
          </h3>
          <p className="text-[10px] text-emerald-700 mt-1 font-bold">
            نسبة التحصيل: {Math.round((totalCollected / (totalBilled || 1)) * 100)}%
          </p>
        </div>

        <div className="p-4 rounded-2xl bg-white border border-slate-200 shadow-2xs">
          <p className="text-xs font-bold text-slate-500">المبالغ المتبقية في ذمة العملاء</p>
          <h3 className="text-xl font-extrabold text-amber-700 mt-1">
            {totalOutstanding.toLocaleString(undefined, { minimumFractionDigits: 2 })} <span className="text-xs font-normal text-slate-500">{currencySymbol}</span>
          </h3>
          <p className="text-[10px] text-slate-400 mt-1">رصيد آجل بانتظار السداد</p>
        </div>

        <div className="p-4 rounded-2xl bg-white border border-slate-200 shadow-2xs">
          <p className="text-xs font-bold text-slate-500">الفواتير المتأخرة</p>
          <h3 className="text-xl font-extrabold text-rose-700 mt-1">
            {overdueCount} <span className="text-xs font-normal text-slate-400">فواتير</span>
          </h3>
          <p className="text-[10px] text-rose-600 mt-1 font-bold">تتطلب إرسال إشعار تذكير</p>
        </div>
      </div>

      {/* Filter Toolbar */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 p-4 rounded-2xl bg-white border border-slate-200 shadow-sm">
        
        {/* Status Tabs */}
        <div className="flex items-center gap-1 overflow-x-auto pb-1 md:pb-0">
          {[
            { id: 'all', label: 'جميع الفواتير' },
            { id: 'مدفوع', label: 'مدفوع بالكامل' },
            { id: 'مدفوع جزئياً', label: 'مدفوع جزئياً' },
            { id: 'غير مدفوع', label: 'غير مدفوع' },
            { id: 'متأخر', label: 'متأخر' },
          ].map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`px-3 py-1.5 rounded-xl text-xs font-bold whitespace-nowrap transition ${
                activeTab === tab.id
                  ? 'bg-blue-600 text-white shadow-xs'
                  : 'text-slate-600 hover:bg-slate-100'
              }`}
            >
              {tab.label}
            </button>
          ))}
        </div>

        {/* Search */}
        <div className="relative w-full md:w-64">
          <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="البحث برقم الفاتورة، العميل..."
            className="w-full pr-9 pl-3 py-1.5 text-xs rounded-xl bg-slate-50 border border-slate-200 text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

      </div>

      {/* Invoices Table */}
      <div className="rounded-2xl bg-white border border-slate-200 shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-right text-xs">
            <thead>
              <tr className="border-b border-slate-200 text-slate-500 uppercase text-[10px] font-bold bg-slate-50">
                <th className="py-3 px-4">رقم الفاتورة</th>
                <th className="py-3 px-4">العميل المفوتر</th>
                <th className="py-3 px-4">تاريخ الإصدار</th>
                <th className="py-3 px-4">تاريخ الاستحقاق</th>
                <th className="py-3 px-4 text-left">الإجمالي المفوتر</th>
                <th className="py-3 px-4 text-left">المتبقي غير المدفوع</th>
                <th className="py-3 px-4 text-center">حالة الفاتورة</th>
                <th className="py-3 px-4 text-left">إجراءات</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {filteredInvoices.length === 0 ? (
                <tr>
                  <td colSpan={8} className="py-8 text-center text-slate-400 text-xs">
                    لا توجد فواتير مطابقة لمعايير البحث.
                  </td>
                </tr>
              ) : (
                filteredInvoices.map((inv) => (
                  <tr key={inv.id} className="hover:bg-slate-50 transition">
                    <td className="py-3.5 px-4 font-mono font-bold text-blue-700">
                      {inv.invoiceNumber ?? '—'}
                    </td>
                    <td className="py-3.5 px-4 font-bold text-slate-800">
                      <div>{inv.customerName ?? 'غير محدد'}</div>
                      <div className="text-[10px] font-mono text-slate-400">{inv.orderNumber ?? '—'}</div>
                    </td>
                    <td className="py-3.5 px-4 text-slate-500 font-mono">
                      {inv.issueDate ?? '—'}
                    </td>
                    <td className="py-3.5 px-4 text-slate-500 font-mono">
                      {inv.dueDate ?? '—'}
                    </td>
                    <td className="py-3.5 px-4 text-left font-mono font-extrabold text-slate-900">
                      {(inv.totalAmount ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2 })} {currencySymbol}
                    </td>
                    <td className="py-3.5 px-4 text-left font-mono">
                      {(inv.balanceDue ?? 0) > 0.009 ? (
                        <span className="font-extrabold text-blue-700">
                          {(inv.balanceDue ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2 })} {currencySymbol}
                        </span>
                      ) : (
                        <span className="text-slate-300" title="الفاتورة مسددة بالكامل">—</span>
                      )}
                    </td>
                    <td className="py-3.5 px-4 text-center">
                      {getStatusBadge(inv.status)}
                    </td>
                    <td className="py-3.5 px-4 text-left">
                      <button
                        onClick={() => setSelectedInvoice(inv)}
                        className="flex items-center gap-1 rounded-lg px-2 py-1 text-[10px] font-bold text-slate-600 hover:bg-slate-100 hover:text-blue-700 transition"
                        title="معاينة الفاتورة وتسجيل الدفعة"
                      >
                        <Eye className="h-3.5 w-3.5" /> عرض
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Invoice Detail Modal */}
      <InvoiceDetailModal
        invoice={selectedInvoice}
        onClose={() => setSelectedInvoice(null)}
      />

    </div>
  );
};
