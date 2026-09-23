import React from 'react';
import { 
  Plus, 
  ChevronLeft,
  Building2,
  FileText,
  ShoppingCart,
  Package,
  CreditCard,
  ArrowLeft,
} from 'lucide-react';
import { useTenant } from '../../context/TenantContext';


interface OverviewPageProps {
  onOpenCreateOrder: () => void;
}

export const OverviewPage: React.FC<OverviewPageProps> = ({ onOpenCreateOrder }) => {
  const { 
    userSession, 
    currencySymbol,
    orders,
    invoices,
    setCurrentPage 
  } = useTenant();

  return (
    <div className="space-y-6 animate-in fade-in duration-300">
      
      {/* Header Banner */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
        <div className="space-y-1">
          <div className="flex items-center gap-2">
            <span className="px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-blue-50 text-blue-700 border border-blue-200 flex items-center gap-1">
              <Building2 className="w-3.5 h-3.5" /> {userSession?.name || 'المؤسسة الإدارية'}
            </span>
          </div>
          <h1 className="text-xl font-extrabold text-slate-900 tracking-tight">
            العمليات الرئيسية
          </h1>
          <p className="text-xs text-slate-500">
            إدارة الطلبات والمخزون من مكان واحد.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={onOpenCreateOrder}
            className="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs transition shadow-sm"
          >
            <Plus className="w-4 h-4" /> أمر مبيعات جديد
          </button>

          <button
            onClick={() => setCurrentPage('inventory')}
            className="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs transition border border-slate-200"
          >
            إدارة المخزون
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div className="flex items-center justify-between rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
          <div><p className="text-xs font-bold text-slate-500">إجمالي صافي المبيعات</p><p className="mt-1 text-2xl font-extrabold text-slate-900">{orders.filter(order => order.status !== 'ملغى').reduce((total, order) => total + Number(order.totalAmount ?? 0), 0).toLocaleString(undefined, { minimumFractionDigits: 2 })} <span className="text-xs font-normal text-slate-500">{currencySymbol}</span></p></div>
          <FileText className="h-5 w-5 text-blue-700" />
        </div>
        <div className="flex items-center justify-between rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
          <div><p className="text-xs font-bold text-slate-500">فواتير غير مسددة</p><p className="mt-1 text-2xl font-extrabold text-slate-900">{invoices.filter(invoice => invoice.status !== 'مدفوع').length}</p></div>
          <FileText className="h-5 w-5 text-blue-700" />
        </div>
      </div>

      {/* Quick Actions */}
      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <h2 className="text-sm font-bold text-slate-900">إجراءات سريعة</h2>
          <span className="text-[11px] text-slate-400 font-bold">اختصارات لأهم عمليات اليوم</span>
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          {/* Primary CTA — Issue a new sales order (opens CreateOrderModal) */}
          <button
            type="button"
            onClick={onOpenCreateOrder}
            className="group relative text-right p-5 rounded-2xl bg-gradient-to-br from-blue-600 via-blue-700 to-indigo-700 text-white shadow-md hover:shadow-xl hover:-translate-y-0.5 transition overflow-hidden"
          >
            <span className="absolute -top-10 -left-10 w-32 h-32 rounded-full bg-white/10 group-hover:scale-110 transition" aria-hidden />
            <span className="relative inline-flex p-3 rounded-xl bg-white/15 mb-3">
              <ShoppingCart className="w-5 h-5" />
            </span>
            <h3 className="relative text-sm font-extrabold">إصدار أمر مبيعات جديد</h3>
            <p className="relative text-[11px] text-blue-100 mt-1 leading-relaxed">
              خطوة واحدة: حدد الأصناف والكميات، خصم المخزون، وإصدار الفاتورة تلقائياً.
            </p>
            <span className="relative mt-3 inline-flex items-center gap-1 text-[11px] font-bold bg-white/15 px-3 py-1.5 rounded-lg group-hover:bg-white/25 transition">
              ابدأ الآن <ArrowLeft className="w-3.5 h-3.5" />
            </span>
          </button>

          {/* Inventory shortcut */}
          <button
            type="button"
            onClick={() => setCurrentPage('inventory')}
            className="group text-right p-5 rounded-2xl bg-white border border-slate-200 shadow-sm hover:border-blue-300 hover:shadow-md transition"
          >
            <span className="inline-flex p-3 rounded-xl bg-amber-50 text-amber-600 mb-3">
              <Package className="w-5 h-5" />
            </span>
            <h3 className="text-sm font-extrabold text-slate-900">إدارة المخزون والمستودعات</h3>
            <p className="text-[11px] text-slate-500 mt-1 leading-relaxed">
              تصفّح الأصناف، تنبيهات حد الأمان، والتوريد المخزني.
            </p>
            <span className="mt-3 inline-flex items-center gap-1 text-[11px] font-bold text-blue-700 group-hover:underline">
              الدخول للقسم <ChevronLeft className="w-3.5 h-3.5" />
            </span>
          </button>

          {/* Invoices shortcut */}
          <button
            type="button"
            onClick={() => setCurrentPage('invoices')}
            className="group text-right p-5 rounded-2xl bg-white border border-slate-200 shadow-sm hover:border-emerald-300 hover:shadow-md transition"
          >
            <span className="inline-flex p-3 rounded-xl bg-emerald-50 text-emerald-600 mb-3">
              <CreditCard className="w-5 h-5" />
            </span>
            <h3 className="text-sm font-extrabold text-slate-900">الفواتير والتحصيل</h3>
            <p className="text-[11px] text-slate-500 mt-1 leading-relaxed">
              تسجيل الدفعات ومتابعة المتبقي والباقي للعملاء.
            </p>
            <span className="mt-3 inline-flex items-center gap-1 text-[11px] font-bold text-blue-700 group-hover:underline">
              الدخول للقسم <ChevronLeft className="w-3.5 h-3.5" />
            </span>
          </button>
        </div>
      </div>

      {/* Recent Sales Orders Table */}
      <div className="p-5 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-4">
        <div className="flex items-center justify-between pb-3 border-b border-slate-100">
          <div>
            <h2 className="text-sm font-bold text-slate-900">
              أحدث أوامر المبيعات
            </h2>
            <p className="text-[11px] text-slate-500">
              قائمة بأحدث المعاملات والطلبات المسجلة في النظام
            </p>
          </div>
          <button
            onClick={() => setCurrentPage('orders')}
            className="flex items-center gap-1 text-xs text-blue-700 font-bold hover:underline"
          >
            الانتقال لأوامر المبيعات <ChevronLeft className="w-4 h-4" />
          </button>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-right text-xs">
            <thead>
              <tr className="border-b border-slate-200 text-slate-500 uppercase text-[10px] font-bold bg-slate-50">
                <th className="py-2.5 px-3">رقم الأمر</th>
                <th className="py-2.5 px-3">العميل</th>
                <th className="py-2.5 px-3">المستودع المنفذ</th>
                <th className="py-2.5 px-3">التاريخ</th>
                <th className="py-2.5 px-3 text-left">الإجمالي</th>
                <th className="py-2.5 px-3 text-center">حالة الأمر</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {orders.slice(0, 5).map((order) => (
                <tr key={order.id} className="hover:bg-slate-50 transition">
                  <td className="py-3 px-3 font-mono font-bold text-blue-700">
                    {order.orderNumber}
                  </td>
                  <td className="py-3 px-3 font-bold text-slate-800">
                    {order.customerName}
                  </td>
                  <td className="py-3 px-3 text-slate-600">
                    {order.warehouseName}
                  </td>
                  <td className="py-3 px-3 text-slate-500 font-mono">
                    {order.issueDate ?? '—'}
                  </td>
                  <td className="py-3 px-3 text-left font-mono font-extrabold text-slate-900">
                    {Number(order.totalAmount ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2 })} {currencySymbol}
                  </td>
                  <td className="py-3 px-3 text-center">
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-[10px] font-bold ${
                      order.status === 'مكتمل'
                        ? 'bg-emerald-100 text-emerald-800 border border-emerald-200'
                        : order.status === 'مؤكد'
                        ? 'bg-blue-100 text-blue-800 border border-blue-200'
                        : order.status === 'مسودة'
                        ? 'bg-amber-100 text-amber-800 border border-amber-200'
                        : 'bg-rose-100 text-rose-800 border border-rose-200'
                    }`}>
                      {order.status}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

    </div>
  );
};
