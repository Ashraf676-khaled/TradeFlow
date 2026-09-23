import React, { useState } from 'react';
import { useTenant } from '../../context/TenantContext';
import { SalesOrder, OrderStatus } from '../../types';
import { InvoiceDetailModal } from '../invoices/InvoiceDetailModal';

interface OrdersPageProps {
  onOpenCreateOrder: () => void;
}

export const OrdersPage: React.FC<OrdersPageProps> = ({ onOpenCreateOrder }) => {
  const {
    orders,
    warehouses,
    formatCurrency,
    confirmSalesOrder,
    completeSalesOrder,
    cancelSalesOrder,
    language,
    refreshAllData,
  } = useTenant();

  const [activeStatusTab, setActiveStatusTab] = useState<string>('all');
  const [selectedWarehouseFilter, setSelectedWarehouseFilter] = useState<string>('all');
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [inspectingOrder, setInspectingOrder] = useState<SalesOrder | null>(null);
  const [selectedInvoice, setSelectedInvoice] = useState<import('../../types').Invoice | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [actionSuccess, setActionSuccess] = useState<string | null>(null);
  const [copiedId, setCopiedId] = useState<string | null>(null);

  const filteredOrders = orders.filter(order => {
    const matchesTab = activeStatusTab === 'all' || order.status === activeStatusTab;
    const matchesWarehouse = selectedWarehouseFilter === 'all' || order.warehouseId === selectedWarehouseFilter;
    const q = searchQuery.toLowerCase();
    const matchesSearch =
      (order.orderNumber || '').toLowerCase().includes(q) ||
      (order.customerName || '').toLowerCase().includes(q) ||
      (order.warehouseName || '').toLowerCase().includes(q) ||
      (order.items || []).some(item => (item.productName || '').toLowerCase().includes(q) || (item.sku || '').toLowerCase().includes(q));

    return matchesTab && matchesWarehouse && matchesSearch;
  });

  const handleCopyId = (id: string, e: React.MouseEvent) => {
    e.stopPropagation();
    navigator.clipboard?.writeText(id);
    setCopiedId(id);
    setTimeout(() => setCopiedId(null), 2000);
  };

  const handleConfirm = async (orderId: string) => {
    try {
      setActionError(null);
      const generatedInvoice = await confirmSalesOrder(orderId);
      setActionSuccess(`Order confirmed and invoice ${generatedInvoice?.invoiceNumber || ''} generated.`);
      setTimeout(() => setActionSuccess(null), 4000);
    } catch (err: any) {
      setActionError(err.response?.data?.detail || err.response?.data?.title || err.message || 'Failed to confirm order.');
    }
  };

  const handleComplete = async (orderId: string) => {
    try {
      setActionError(null);
      await completeSalesOrder(orderId);
      setActionSuccess('Order completed and inventory archived.');
      setTimeout(() => setActionSuccess(null), 4000);
    } catch (err: any) {
      setActionError(err.response?.data?.detail || err.response?.data?.title || err.message || 'Failed to complete order.');
    }
  };

  const handleCancel = async (orderId: string) => {
    if (!window.confirm(language === 'ar' ? 'هل أنت متأكد من إلغاء هذا الأمر وإرجاع المخزون؟' : 'Are you sure you want to cancel this order and release reserved stock?')) {
      return;
    }
    try {
      setActionError(null);
      await cancelSalesOrder(orderId);
      setActionSuccess('Order cancelled.');
      setTimeout(() => setActionSuccess(null), 4000);
    } catch (err: any) {
      setActionError(err.response?.data?.detail || err.response?.data?.title || err.message || 'Failed to cancel order.');
    }
  };

  const handleExportCSV = () => {
    const csvContent = "data:text/csv;charset=utf-8," +
      ["Order ID,Customer,Warehouse,Items,Total,Status,Date",
        ...filteredOrders.map(o => `"${o.orderNumber}","${o.customerName || ''}","${o.warehouseName || ''}",${o.items?.length || 1},${o.totalAmount},"${o.status}","${o.issueDate || ''}"`)
      ].join("\n");
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement("a");
    link.setAttribute("href", encodedUri);
    link.setAttribute("download", `tradeflow_orders_${new Date().toISOString().slice(0,10)}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const handlePrint = (order: SalesOrder) => {
    window.print();
  };

  return (
    <div className="space-y-4">
      {/* Top Breadcrumb & Action Banner */}
      <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4">
        <div className="space-y-1">
          <div className="flex items-center gap-2">
            <h1 className="text-2xl font-semibold text-white tracking-tight">
              {language === 'ar' ? 'سجل العمليات والأوامر' : 'Transactions & Order Records'}
            </h1>
            <span className="px-2 py-0.5 rounded-full bg-[#282a2d] text-[#4edea3] text-[10px] font-mono font-bold">
              LIVE LEDGER
            </span>
          </div>
          <p className="text-xs text-[#8f9194]">
            {language === 'ar'
              ? 'سجل العمليات الموحد لجميع طلبات المبيعات والتسويات والمستودعات'
              : 'Unified institutional ledger across multi-asset trading venues, clearing houses, and internal execution desks.'}
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <button
            onClick={() => refreshAllData()}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-[#1a1c1f] hover:bg-[#282a2d] text-[#e2e2e6] rounded text-xs transition-colors border border-white/5 cursor-pointer"
          >
            <span className="material-symbols-outlined text-sm text-[#8f9194]">sync</span>
            <span>{language === 'ar' ? 'تحديث السجل' : 'Reconcile'}</span>
          </button>

          <button
            onClick={handleExportCSV}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-[#1a1c1f] hover:bg-[#282a2d] text-[#e2e2e6] rounded text-xs transition-colors border border-white/5 cursor-pointer"
          >
            <span className="material-symbols-outlined text-sm text-[#8f9194]">file_download</span>
            <span>{language === 'ar' ? 'تصدير CSV' : 'Export CSV'}</span>
          </button>

          <button
            onClick={onOpenCreateOrder}
            className="flex items-center gap-1.5 px-3.5 py-1.5 bg-[#ffffff] hover:bg-[#e2e2e4] text-[#111316] font-semibold rounded text-xs transition-all shadow-md cursor-pointer"
          >
            <span className="material-symbols-outlined text-sm font-bold">add</span>
            <span>{language === 'ar' ? 'أمر مبيعات جديد' : 'New Order'}</span>
          </button>
        </div>
      </div>

      {/* Notifications */}
      {actionSuccess && (
        <div className="p-3 rounded bg-[#10b981]/15 border border-[#10b981]/30 text-[#4edea3] text-xs">
          {actionSuccess}
        </div>
      )}
      {actionError && (
        <div className="p-3 rounded bg-rose-500/15 border border-rose-500/30 text-rose-400 text-xs">
          {actionError}
        </div>
      )}

      {/* Filters & Search Toolbar */}
      <div className="p-3 rounded bg-[#1a1c1f] border border-white/5 flex flex-col md:flex-row md:items-center justify-between gap-3">
        <div className="flex flex-wrap items-center gap-2">
          {/* Status Tabs */}
          <div className="flex items-center bg-[#111316] p-0.5 rounded border border-white/5">
            {[
              { id: 'all', label: language === 'ar' ? 'الكل' : 'All' },
              { id: 'مسودة', label: language === 'ar' ? 'مسودة' : 'Draft' },
              { id: 'مؤكد', label: language === 'ar' ? 'مؤكد' : 'Confirmed' },
              { id: 'مكتمل', label: language === 'ar' ? 'مكتمل' : 'Completed' },
              { id: 'ملغى', label: language === 'ar' ? 'ملغى' : 'Cancelled' },
            ].map(tab => (
              <button
                key={tab.id}
                onClick={() => setActiveStatusTab(tab.id)}
                className={`px-2.5 py-1 text-xs font-medium rounded transition-colors ${
                  activeStatusTab === tab.id
                    ? 'bg-[#282a2d] text-white shadow-xs'
                    : 'text-[#8f9194] hover:text-[#e2e2e6]'
                }`}
              >
                {tab.label}
              </button>
            ))}
          </div>

          {/* Warehouse Selector */}
          <select
            value={selectedWarehouseFilter}
            onChange={(e) => setSelectedWarehouseFilter(e.target.value)}
            className="px-2.5 py-1 bg-[#111316] border border-white/10 rounded text-xs text-[#e2e2e6] outline-none"
          >
            <option value="all">{language === 'ar' ? 'جميع المستودعات' : 'All Warehouses'}</option>
            {warehouses.map(w => (
              <option key={w.id} value={w.id}>{w.name}</option>
            ))}
          </select>
        </div>

        {/* Search */}
        <div className="relative w-full md:w-64">
          <span className="material-symbols-outlined absolute left-2.5 top-1/2 -translate-y-1/2 text-sm text-[#8f9194]">
            search
          </span>
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder={language === 'ar' ? 'بحث بالرقم أو العميل أو الصنف...' : 'Search Order ID, Client, Item...'}
            className="w-full pl-8 pr-3 py-1.5 bg-[#111316] border border-white/10 rounded text-xs text-white placeholder-[#8f9194] outline-none focus:border-[#4edea3]"
          />
        </div>
      </div>

      {/* Orders Ledger Table */}
      <div className="rounded bg-[#1a1c1f] border border-white/5 overflow-hidden shadow-sm">
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-[#111316] text-[#8f9194] text-[11px] font-semibold uppercase tracking-wider border-b border-white/5">
                <th className="py-2.5 px-3">Order ID</th>
                <th className="py-2.5 px-3">Date / Time</th>
                <th className="py-2.5 px-3">Counterparty</th>
                <th className="py-2.5 px-3">Warehouse</th>
                <th className="py-2.5 px-3 text-right">Items</th>
                <th className="py-2.5 px-3 text-right">Net Settlement</th>
                <th className="py-2.5 px-3">Status</th>
                <th className="py-2.5 px-3 text-center">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-white/5 text-xs font-mono">
              {filteredOrders.length === 0 ? (
                <tr>
                  <td colSpan={8} className="py-12 text-center text-xs text-[#8f9194] font-sans">
                    {language === 'ar' ? 'لم يتم العثور على أي أوامر مبيعات تطابق البحث' : 'No order transactions found.'}
                  </td>
                </tr>
              ) : (
                filteredOrders.map(order => (
                  <tr
                    key={order.id}
                    onClick={() => setInspectingOrder(order)}
                    className="hover:bg-[#282a2d]/50 transition-colors cursor-pointer group"
                  >
                    <td className="py-3 px-3 text-white font-bold flex items-center gap-1.5">
                      <span>{order.orderNumber}</span>
                      <button
                        onClick={(e) => handleCopyId(order.orderNumber || order.id, e)}
                        title="Copy ID"
                        className="text-[#8f9194] hover:text-white opacity-0 group-hover:opacity-100 transition-opacity"
                      >
                        <span className="material-symbols-outlined text-[13px]">
                          {copiedId === (order.orderNumber || order.id) ? 'check' : 'content_copy'}
                        </span>
                      </button>
                    </td>

                    <td className="py-3 px-3 text-[#8f9194] font-sans">
                      {order.issueDate ? new Date(order.issueDate).toLocaleDateString() : 'Active'}
                    </td>

                    <td className="py-3 px-3 font-sans text-[#e2e2e6] font-medium">
                      {order.customerName || 'Direct Counterparty'}
                    </td>

                    <td className="py-3 px-3 font-sans text-[#8f9194]">
                      {order.warehouseName || 'Central Hub'}
                    </td>

                    <td className="py-3 px-3 text-right text-[#8f9194]">
                      {order.items?.length || 1} units
                    </td>

                    <td className="py-3 px-3 text-right font-bold text-white">
                      {formatCurrency(Number(order.totalAmount) || 0)}
                    </td>

                    <td className="py-3 px-3 font-sans">
                      <span className={`px-2 py-0.5 rounded-full text-[10px] font-semibold ${
                        order.status === 'مكتمل'
                          ? 'bg-[#10b981]/15 text-[#4edea3] border border-[#10b981]/30'
                          : order.status === 'مؤكد'
                          ? 'bg-sky-500/15 text-sky-400 border border-sky-500/30'
                          : order.status === 'ملغى'
                          ? 'bg-rose-500/15 text-rose-400 border border-rose-500/30'
                          : 'bg-amber-500/15 text-amber-400 border border-amber-500/30'
                      }`}>
                        {order.status}
                      </span>
                    </td>

                    <td className="py-3 px-3 text-center" onClick={(e) => e.stopPropagation()}>
                      <div className="flex items-center justify-center gap-1">
                        <button
                          onClick={() => setInspectingOrder(order)}
                          title="Inspect Order"
                          className="p-1 rounded bg-[#282a2d] hover:bg-[#333538] text-[#8f9194] hover:text-white transition-colors"
                        >
                          <span className="material-symbols-outlined text-[15px]">visibility</span>
                        </button>

                        {order.status === 'مسودة' && (
                          <button
                            onClick={() => handleConfirm(order.id)}
                            title="Confirm & Invoice"
                            className="p-1 rounded bg-[#10b981]/20 hover:bg-[#10b981]/40 text-[#4edea3] transition-colors"
                          >
                            <span className="material-symbols-outlined text-[15px]">done</span>
                          </button>
                        )}

                        {order.status === 'مؤكد' && (
                          <button
                            onClick={() => handleComplete(order.id)}
                            title="Complete Order"
                            className="p-1 rounded bg-sky-500/20 hover:bg-sky-500/40 text-sky-400 transition-colors"
                          >
                            <span className="material-symbols-outlined text-[15px]">check_circle</span>
                          </button>
                        )}

                        {order.status !== 'ملغى' && order.status !== 'مكتمل' && (
                          <button
                            onClick={() => handleCancel(order.id)}
                            title="Cancel Order"
                            className="p-1 rounded bg-rose-500/20 hover:bg-rose-500/40 text-rose-400 transition-colors"
                          >
                            <span className="material-symbols-outlined text-[15px]">close</span>
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Inspect Order Slide-over / Modal */}
      {inspectingOrder && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 backdrop-blur-xs p-4">
          <div className="bg-[#1a1c1f] border border-[#26292e] rounded shadow-2xl w-full max-w-2xl max-h-[90vh] overflow-y-auto p-5 space-y-4">
            <div className="flex items-center justify-between pb-3 border-b border-white/5">
              <div>
                <div className="flex items-center gap-2">
                  <h3 className="text-base font-bold text-white font-mono">
                    {inspectingOrder.orderNumber}
                  </h3>
                  <span className={`px-2 py-0.5 rounded text-[10px] font-semibold ${
                    inspectingOrder.status === 'مكتمل'
                      ? 'bg-[#10b981]/15 text-[#4edea3]'
                      : 'bg-sky-500/15 text-sky-400'
                  }`}>
                    {inspectingOrder.status}
                  </span>
                </div>
                <p className="text-xs text-[#8f9194]">
                  Counterparty: <strong className="text-white">{inspectingOrder.customerName || 'Direct'}</strong> · Warehouse: {inspectingOrder.warehouseName || 'Central'}
                </p>
              </div>

              <button
                onClick={() => setInspectingOrder(null)}
                className="p-1 text-[#8f9194] hover:text-white"
              >
                <span className="material-symbols-outlined">close</span>
              </button>
            </div>

            {/* Item Breakdown */}
            <div>
              <h4 className="text-xs font-semibold text-[#8f9194] uppercase tracking-wider mb-2">
                Order Line Items
              </h4>
              <div className="rounded bg-[#111316] border border-white/5 overflow-hidden">
                <table className="w-full text-left font-mono text-xs">
                  <thead>
                    <tr className="text-[#8f9194] text-[10px] uppercase border-b border-white/5 bg-[#181a1e]">
                      <th className="py-2 px-3">Item / SKU</th>
                      <th className="py-2 px-3 text-right">Qty</th>
                      <th className="py-2 px-3 text-right">Unit Price</th>
                      <th className="py-2 px-3 text-right">Total</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-white/5">
                    {inspectingOrder.items?.map((item, idx) => (
                      <tr key={idx}>
                        <td className="py-2 px-3">
                          <div className="text-white font-sans">{item.productName}</div>
                          <div className="text-[10px] text-[#8f9194]">{item.sku}</div>
                        </td>
                        <td className="py-2 px-3 text-right text-white">{item.quantity}</td>
                        <td className="py-2 px-3 text-right text-[#8f9194]">
                          {formatCurrency(Number(item.unitPrice) || 0)}
                        </td>
                        <td className="py-2 px-3 text-right font-bold text-[#4edea3]">
                          {formatCurrency(Number(item.totalPrice || ((item.quantity || 1) * (item.unitPrice || 0))))}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Totals Summary */}
            <div className="p-3 bg-[#111316] rounded border border-white/5 space-y-1 font-mono text-xs">
              <div className="flex justify-between text-[#8f9194]">
                <span>Total Amount:</span>
                <span className="text-white font-bold text-sm">
                  {formatCurrency(Number(inspectingOrder.totalAmount) || 0)}
                </span>
              </div>
            </div>

            {/* Modal Actions */}
            <div className="flex items-center justify-between pt-3 border-t border-white/5">
              <button
                onClick={() => handlePrint(inspectingOrder)}
                className="flex items-center gap-1 px-3 py-1.5 rounded bg-[#282a2d] hover:bg-[#333538] text-white text-xs cursor-pointer"
              >
                <span className="material-symbols-outlined text-sm">print</span>
                <span>Print Voucher</span>
              </button>

              <div className="flex items-center gap-2">
                {inspectingOrder.status === 'مسودة' && (
                  <button
                    onClick={() => {
                      handleConfirm(inspectingOrder.id);
                      setInspectingOrder(null);
                    }}
                    className="px-3 py-1.5 rounded bg-[#10b981] hover:bg-[#059669] text-white text-xs font-semibold cursor-pointer"
                  >
                    Confirm & Generate Invoice
                  </button>
                )}
                {inspectingOrder.status === 'مؤكد' && (
                  <button
                    onClick={() => {
                      handleComplete(inspectingOrder.id);
                      setInspectingOrder(null);
                    }}
                    className="px-3 py-1.5 rounded bg-sky-600 hover:bg-sky-500 text-white text-xs font-semibold cursor-pointer"
                  >
                    Mark Fulfilled & Complete
                  </button>
                )}
                <button
                  onClick={() => setInspectingOrder(null)}
                  className="px-3 py-1.5 rounded bg-[#282a2d] text-[#8f9194] hover:text-white text-xs cursor-pointer"
                >
                  Close
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {selectedInvoice && (
        <InvoiceDetailModal
          invoice={selectedInvoice}
          onClose={() => setSelectedInvoice(null)}
        />
      )}
    </div>
  );
};
