import React, { useState } from 'react';
import { 
  ShoppingCart, 
  Search, 
  Plus, 
  Eye, 
  CheckCircle, 
  Clock, 
  AlertCircle, 
  XCircle,
  Check,
  X,
  Printer
} from 'lucide-react';
import { useTenant } from '../../context/TenantContext';
import { OrderStatus, SalesOrder } from '../../types';
import { InvoiceDetailModal } from '../invoices/InvoiceDetailModal';
import { getApiErrorMessage } from '../../services/apiClient';

interface OrdersPageProps {
  onOpenCreateOrder: () => void;
}

export const OrdersPage: React.FC<OrdersPageProps> = ({ onOpenCreateOrder }) => {
  const { orders, currencySymbol, confirmSalesOrder, completeSalesOrder, cancelSalesOrder } = useTenant();

  const [activeTab, setActiveTab] = useState<string>('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedOrder, setSelectedOrder] = useState<SalesOrder | null>(null);
  const [selectedInvoice, setSelectedInvoice] = useState<import('../../types').Invoice | null>(null);
  const [invoiceError, setInvoiceError] = useState('');

  const filteredOrders = orders.filter(order => {
    const matchesTab = activeTab === 'all' || order.status === activeTab;
    const q = searchTerm.toLowerCase();
    const ordNum = (order.orderNumber ?? '').toLowerCase();
    const cust = (order.customerName ?? '').toLowerCase();
    return matchesTab && (ordNum.includes(q) || cust.includes(q));
  });

  const getStatusBadge = (status: OrderStatus) => {
    switch (status) {
      case 'مكتمل':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-emerald-100 text-emerald-800 border border-emerald-200">
            <CheckCircle className="w-3 h-3" /> مكتمل
          </span>
        );
      case 'مؤكد':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-blue-100 text-blue-800 border border-blue-200">
            <Clock className="w-3 h-3" /> مؤكد
          </span>
        );
      case 'مسودة':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-amber-100 text-amber-800 border border-amber-200">
            <AlertCircle className="w-3 h-3" /> مسودة
          </span>
        );
      case 'ملغى':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-rose-100 text-rose-800 border border-rose-200">
            <XCircle className="w-3 h-3" /> ملغى
          </span>
        );
      default:
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-slate-100 text-slate-700">
            {status}
          </span>
        );
    }
  };

  return (
    <div className="space-y-6 animate-in fade-in duration-300">
      
      {/* Header & Action */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <ShoppingCart className="w-5 h-5 text-blue-700" /> إدارة أوامر المبيعات
          </h1>
          <p className="text-xs text-slate-500">
            متابعة وتأكيد واعتماد أوامر المبيعات وإصدار الفواتير للعملاء.
          </p>
        </div>

        <button
          onClick={onOpenCreateOrder}
          className="flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs shadow-sm transition"
        >
          <Plus className="w-4 h-4" /> إنشاء أمر مبيعات جديد
        </button>
      </div>

      {invoiceError && <div className="rounded-xl border border-rose-200 bg-rose-50 p-3 text-xs font-bold text-rose-700">{invoiceError}</div>}

      {/* Tabs & Search Bar */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 p-4 rounded-2xl bg-white border border-slate-200 shadow-sm">
        
        {/* Tabs */}
        <div className="flex items-center gap-1 overflow-x-auto pb-1 md:pb-0">
          {[
            { id: 'all', label: 'جميع الأوامر', count: orders.length },
            { id: 'مسودة', label: 'المسودات', count: orders.filter(o => o.status === 'مسودة').length },
            { id: 'مؤكد', label: 'المؤكدة', count: orders.filter(o => o.status === 'مؤكد').length },
            { id: 'مكتمل', label: 'المكتملة', count: orders.filter(o => o.status === 'مكتمل').length },
          ].map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`flex items-center gap-2 px-3 py-1.5 rounded-xl text-xs font-bold whitespace-nowrap transition ${
                activeTab === tab.id
                  ? 'bg-blue-600 text-white shadow-xs'
                  : 'text-slate-600 hover:bg-slate-100'
              }`}
            >
              <span>{tab.label}</span>
              <span className={`px-1.5 py-0.2 rounded-full text-[10px] ${
                activeTab === tab.id ? 'bg-blue-700 text-white' : 'bg-slate-200 text-slate-600'
              }`}>
                {tab.count}
              </span>
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
            placeholder="البحث برقم الأمر، العميل..."
            className="w-full pr-9 pl-3 py-1.5 text-xs rounded-xl bg-slate-50 border border-slate-200 text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

      </div>

      {/* Orders Table */}
      <div className="rounded-2xl bg-white border border-slate-200 shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-right text-xs">
            <thead>
              <tr className="border-b border-slate-200 text-slate-500 uppercase text-[10px] font-bold bg-slate-50">
                <th className="py-3 px-4">رقم الأمر</th>
                <th className="py-3 px-4">اسم العميل</th>
                <th className="py-3 px-4">تاريخ الإصدار</th>
                <th className="py-3 px-4 text-left">الإجمالي</th>
                <th className="py-3 px-4 text-center">حالة الأمر</th>
                <th className="py-3 px-4 text-left">الإجراءات</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {filteredOrders.length === 0 ? (
                <tr>
                  <td colSpan={6} className="py-8 text-center text-slate-400 text-xs">
                    لا توجد أوامر مبيعات مطابقة لمعايير البحث.
                  </td>
                </tr>
              ) : (
                filteredOrders.map((order) => (
                  <tr key={order.id} className="hover:bg-slate-50 transition">
                    <td className="py-3.5 px-4 font-mono font-bold text-blue-700">
                      {order.orderNumber ?? '—'}
                    </td>
                    <td className="py-3.5 px-4 font-bold text-slate-800">
                      {order.customerName || 'غير محدد'}
                    </td>
                    <td className="py-3.5 px-4 text-slate-500 font-mono">
                      {order.issueDate ?? '—'}
                    </td>
                    <td className="py-3.5 px-4 text-left font-mono font-extrabold text-slate-900">
                      {(order.totalAmount ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2 })} {currencySymbol}
                    </td>
                    <td className="py-3.5 px-4 text-center">
                      {getStatusBadge(order.status)}
                    </td>
                    <td className="py-3.5 px-4 text-left">
                      <div className="flex items-center justify-end gap-1.5">
                        {order.status === 'مسودة' && (
                          <button
                            onClick={async () => {
                              try {
                                const invoice = await confirmSalesOrder(order.id);
                                setSelectedInvoice(invoice);
                              } catch (error) {
                                setInvoiceError(getApiErrorMessage(error, 'تعذر تأكيد الطلب وإصدار الفاتورة.'));
                              }
                            }}
                            className="px-2 py-1 rounded-lg text-[10px] font-bold text-emerald-700 bg-emerald-50 border border-emerald-200 hover:bg-emerald-100"
                            title="تأكيد واعتماد الأمر"
                          >
                            تأكيد
                          </button>
                        )}
                        {order.status === 'مؤكد' && (
                          <button
                            onClick={() => completeSalesOrder(order.id)}
                            className="px-2 py-1 rounded-lg text-[10px] font-bold text-blue-700 bg-blue-50 border border-blue-200 hover:bg-blue-100"
                            title="إكمال وتسليم الأمر"
                          >
                            إكمال
                          </button>
                        )}
                        {order.status !== 'ملغى' && order.status !== 'مكتمل' && (
                          <button
                            onClick={() => cancelSalesOrder(order.id)}
                            className="px-2 py-1 rounded-lg text-[10px] font-bold text-rose-700 bg-rose-50 border border-rose-200 hover:bg-rose-100"
                            title="إلغاء الأمر"
                          >
                            إلغاء
                          </button>
                        )}
                        <button
                          onClick={() => setSelectedOrder(order)}
                          className="flex items-center gap-1 rounded-lg px-2 py-1 text-[10px] font-bold text-slate-600 hover:bg-slate-100 hover:text-blue-700"
                          title="عرض التفاصيل"
                        >
                          <Eye className="h-3.5 w-3.5" /> عرض
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Order View Modal */}
      {selectedOrder && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-xs animate-in fade-in duration-200">
          <div className="order-print-area w-full max-w-2xl bg-white border border-slate-200 rounded-2xl shadow-2xl p-6 space-y-6 text-right font-sans" dir="rtl">
            <div className="flex justify-between items-start border-b border-slate-100 pb-4">
              <div>
                <span className="text-[10px] uppercase font-bold text-slate-400">
                  تفاصيل أمر المبيعات
                </span>
                <h2 className="text-xl font-extrabold text-slate-900 font-mono">
                  {selectedOrder.orderNumber ?? '—'}
                </h2>
                <p className="text-xs text-slate-500">
                  تاريخ الإصدار: {selectedOrder.issueDate ?? '—'}
                </p>
                <p className="text-xs font-bold text-slate-700">العميل: {selectedOrder.customerName || 'غير محدد'}</p>
                <p className="text-xs text-slate-500">المستودع: {selectedOrder.warehouseName || 'المستودع الرئيسي'}</p>
              </div>
              <button
                onClick={() => setSelectedOrder(null)}
                className="p-2 rounded-xl text-slate-400 hover:text-slate-600 hover:bg-slate-100"
              >
                &times;
              </button>
            </div>

            <div className="p-4 rounded-xl bg-blue-50 border border-blue-100 flex justify-between items-center text-sm">
              <span className="font-bold text-slate-800">المبلغ الإجمالي الكلي</span>
              <span className="font-mono font-extrabold text-blue-700 text-lg">
                {(selectedOrder.totalAmount ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2 })} {currencySymbol}
              </span>
            </div>

            <table className="w-full border-collapse text-xs">
              <thead>
                <tr className="border-b border-slate-200 text-slate-500">
                  <th className="py-2 text-right">الصنف</th>
                  <th className="py-2 text-center">الكمية</th>
                  <th className="py-2 text-left">سعر الوحدة</th>
                  <th className="py-2 text-left">الإجمالي</th>
                </tr>
              </thead>
              <tbody>
                {(selectedOrder.items ?? []).map(item => (
                  <tr key={item.id || item.productId} className="border-b border-slate-100">
                    <td className="py-2">{item.productName || item.sku || item.productId || 'صنف'}</td>
                    <td className="py-2 text-center">{item.quantity ?? 1}</td>
                    <td className="py-2 text-left">{Number(item.unitPrice ?? 0).toFixed(2)} {currencySymbol}</td>
                    <td className="py-2 text-left">{Number(item.totalPrice ?? ((item.quantity ?? 1) * (item.unitPrice ?? 0))).toFixed(2)} {currencySymbol}</td>
                  </tr>
                ))}
              </tbody>
            </table>

            <div className="flex justify-end gap-2 pt-2">
              <button
                onClick={() => window.print()}
                className="flex items-center gap-2 rounded-xl bg-blue-600 px-4 py-2 text-xs font-bold text-white hover:bg-blue-700"
              >
                <Printer className="h-4 w-4" /> طباعة الفاتورة
              </button>
              <button
                onClick={() => setSelectedOrder(null)}
                className="px-4 py-2 rounded-xl text-xs font-bold text-slate-600 hover:bg-slate-100"
              >
                إغلاق النافذة
              </button>
            </div>
          </div>
        </div>
      )}

      <InvoiceDetailModal invoice={selectedInvoice} onClose={() => setSelectedInvoice(null)} />

    </div>
  );
};
