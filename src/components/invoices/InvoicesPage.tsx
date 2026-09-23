import React, { useState } from 'react';
import { useTenant } from '../../context/TenantContext';
import { Invoice, PaymentStatus } from '../../types';
import { InvoiceDetailModal } from './InvoiceDetailModal';

export const InvoicesPage: React.FC = () => {
  const { invoices, formatCurrency, language } = useTenant();

  const [activeTab, setActiveTab] = useState<string>('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedInvoice, setSelectedInvoice] = useState<Invoice | null>(null);

  // Compute metrics
  const totalBilled = invoices.reduce((sum, i) => sum + (Number(i.totalAmount) || 0), 0);
  const totalCollected = invoices.reduce((sum, i) => sum + (Number(i.paidAmount) || 0), 0);
  const totalOutstanding = invoices.reduce((sum, i) => sum + (Number(i.balanceDue) || 0), 0);
  const overdueCount = invoices.filter(i => i.status === 'متأخر').length;

  const filteredInvoices = invoices.filter(inv => {
    const matchesTab = activeTab === 'all' || inv.status === activeTab;
    const q = searchTerm.toLowerCase();
    const invNum = (inv.invoiceNumber ?? '').toLowerCase();
    const cust = (inv.customerName ?? '').toLowerCase();
    const ordNum = (inv.orderNumber ?? '').toLowerCase();
    return matchesTab && (invNum.includes(q) || cust.includes(q) || ordNum.includes(q));
  });

  const handleExportCSV = () => {
    const csvContent = "data:text/csv;charset=utf-8," +
      ["Invoice #,Order #,Customer,Total,Paid,Balance,Status,Date",
        ...filteredInvoices.map(i => `"${i.invoiceNumber}","${i.orderNumber || ''}","${i.customerName || ''}",${i.totalAmount},${i.paidAmount},${i.balanceDue},"${i.status}","${i.issueDate || ''}"`)
      ].join("\n");
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement("a");
    link.setAttribute("href", encodedUri);
    link.setAttribute("download", `tradeflow_invoices_${new Date().toISOString().slice(0, 10)}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <div className="space-y-4">
      {/* Header Banner */}
      <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-white tracking-tight">
            {language === 'ar' ? 'الفواتير والتحصيل المالي' : 'Billing & Invoice Ledger'}
          </h1>
          <p className="text-xs text-[#8f9194] mt-0.5">
            {language === 'ar'
              ? 'سجل الفواتير الضريبية ومتابعة تحصيل الدفعات وتسوية الذمم'
              : 'Institutional receivables ledger, tax invoicing, and instant payment settlement'}
          </p>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={handleExportCSV}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-[#1a1c1f] hover:bg-[#282a2d] text-[#e2e2e6] rounded text-xs transition-colors border border-white/5 cursor-pointer"
          >
            <span className="material-symbols-outlined text-sm text-[#8f9194]">file_download</span>
            <span>{language === 'ar' ? 'تصدير السجل' : 'Export Ledger'}</span>
          </button>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-3.5">
        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5">
          <span className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
            {language === 'ar' ? 'إجمالي المطالبات' : 'Total Billed Value'}
          </span>
          <div className="text-xl font-bold font-mono text-white mt-1">
            {formatCurrency(totalBilled)}
          </div>
          <div className="text-[10px] text-[#8f9194] mt-1 font-mono">
            {invoices.length} invoices generated
          </div>
        </div>

        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5">
          <span className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
            {language === 'ar' ? 'المبالغ المحصلة' : 'Collected Receipts'}
          </span>
          <div className="text-xl font-bold font-mono text-[#4edea3] mt-1">
            {formatCurrency(totalCollected)}
          </div>
          <div className="text-[10px] text-[#8f9194] mt-1">
            {totalBilled > 0 ? ((totalCollected / totalBilled) * 100).toFixed(1) : 100}% collected
          </div>
        </div>

        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5">
          <span className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
            {language === 'ar' ? 'الرصيد المتبقي' : 'Outstanding Balance'}
          </span>
          <div className={`text-xl font-bold font-mono mt-1 ${totalOutstanding > 0 ? 'text-amber-400' : 'text-white'}`}>
            {formatCurrency(totalOutstanding)}
          </div>
          <div className="text-[10px] text-[#8f9194] mt-1">
            Across active counterparties
          </div>
        </div>

        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5">
          <span className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
            {language === 'ar' ? 'الفواتير المتأخرة' : 'Overdue Invoices'}
          </span>
          <div className={`text-xl font-bold font-mono mt-1 ${overdueCount > 0 ? 'text-rose-400' : 'text-white'}`}>
            {overdueCount} accounts
          </div>
          <div className="text-[10px] text-[#8f9194] mt-1">
            {overdueCount === 0 ? 'All collections on schedule' : 'Requires immediate attention'}
          </div>
        </div>
      </div>

      {/* Filter and Search Ribbon */}
      <div className="p-3 rounded bg-[#1a1c1f] border border-white/5 flex flex-col md:flex-row md:items-center justify-between gap-3">
        <div className="flex flex-wrap items-center gap-1 bg-[#111316] p-0.5 rounded border border-white/5">
          {[
            { id: 'all', label: language === 'ar' ? 'الكل' : 'All' },
            { id: 'غير مدفوع', label: language === 'ar' ? 'غير مدفوع' : 'Unpaid' },
            { id: 'مدفوع جزئياً', label: language === 'ar' ? 'مدفوع جزئياً' : 'Partial' },
            { id: 'مدفوع', label: language === 'ar' ? 'مدفوع بالكامل' : 'Paid' },
            { id: 'متأخر', label: language === 'ar' ? 'متأخر' : 'Overdue' },
          ].map(tab => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`px-2.5 py-1 text-xs font-medium rounded transition-colors ${
                activeTab === tab.id
                  ? 'bg-[#282a2d] text-white shadow-xs'
                  : 'text-[#8f9194] hover:text-[#e2e2e6]'
              }`}
            >
              {tab.label}
            </button>
          ))}
        </div>

        <div className="relative w-full md:w-64">
          <span className="material-symbols-outlined absolute left-2.5 top-1/2 -translate-y-1/2 text-sm text-[#8f9194]">
            search
          </span>
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder={language === 'ar' ? 'بحث برقم الفاتورة أو العميل...' : 'Search Invoice #, Client...'}
            className="w-full pl-8 pr-3 py-1.5 bg-[#111316] border border-white/10 rounded text-xs text-white placeholder-[#8f9194] outline-none focus:border-[#4edea3]"
          />
        </div>
      </div>

      {/* Invoices Table */}
      <div className="rounded bg-[#1a1c1f] border border-white/5 overflow-hidden shadow-sm">
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-[#111316] text-[#8f9194] text-[11px] font-semibold uppercase tracking-wider border-b border-white/5">
                <th className="py-2.5 px-3">Invoice #</th>
                <th className="py-2.5 px-3">Ref Order</th>
                <th className="py-2.5 px-3">Counterparty</th>
                <th className="py-2.5 px-3 text-right">Total Billed</th>
                <th className="py-2.5 px-3 text-right">Paid</th>
                <th className="py-2.5 px-3 text-right">Balance Due</th>
                <th className="py-2.5 px-3">Status</th>
                <th className="py-2.5 px-3 text-center">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-white/5 text-xs font-mono">
              {filteredInvoices.length === 0 ? (
                <tr>
                  <td colSpan={8} className="py-12 text-center text-xs text-[#8f9194] font-sans">
                    {language === 'ar' ? 'لم يتم العثور على فواتير' : 'No invoices found.'}
                  </td>
                </tr>
              ) : (
                filteredInvoices.map(invoice => (
                  <tr
                    key={invoice.id}
                    onClick={() => setSelectedInvoice(invoice)}
                    className="hover:bg-[#282a2d]/50 transition-colors cursor-pointer"
                  >
                    <td className="py-3 px-3 text-white font-bold">
                      {invoice.invoiceNumber}
                    </td>

                    <td className="py-3 px-3 text-[#8f9194]">
                      {invoice.orderNumber || 'Direct'}
                    </td>

                    <td className="py-3 px-3 font-sans text-[#e2e2e6] font-medium">
                      {invoice.customerName}
                    </td>

                    <td className="py-3 px-3 text-right font-bold text-white">
                      {formatCurrency(Number(invoice.totalAmount) || 0)}
                    </td>

                    <td className="py-3 px-3 text-right text-[#4edea3]">
                      {formatCurrency(Number(invoice.paidAmount) || 0)}
                    </td>

                    <td className="py-3 px-3 text-right">
                      <span className={invoice.balanceDue && invoice.balanceDue > 0 ? 'text-amber-400 font-bold' : 'text-[#8f9194]'}>
                        {formatCurrency(Number(invoice.balanceDue) || 0)}
                      </span>
                    </td>

                    <td className="py-3 px-3 font-sans">
                      <span className={`px-2 py-0.5 rounded-full text-[10px] font-semibold ${
                        invoice.status === 'مدفوع'
                          ? 'bg-[#10b981]/15 text-[#4edea3] border border-[#10b981]/30'
                          : invoice.status === 'مدفوع جزئياً'
                          ? 'bg-sky-500/15 text-sky-400 border border-sky-500/30'
                          : invoice.status === 'متأخر'
                          ? 'bg-rose-500/15 text-rose-400 border border-rose-500/30'
                          : 'bg-amber-500/15 text-amber-400 border border-amber-500/30'
                      }`}>
                        {invoice.status}
                      </span>
                    </td>

                    <td className="py-3 px-3 text-center">
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          setSelectedInvoice(invoice);
                        }}
                        className="px-2 py-1 rounded bg-[#282a2d] hover:bg-[#333538] text-white text-[11px] font-sans transition-colors cursor-pointer"
                      >
                        Inspect
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      <InvoiceDetailModal
        invoice={selectedInvoice}
        onClose={() => setSelectedInvoice(null)}
      />
    </div>
  );
};
