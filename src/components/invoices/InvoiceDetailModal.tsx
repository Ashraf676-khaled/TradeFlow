import React, { useEffect, useState } from 'react';
import { useTenant } from '../../context/TenantContext';
import { Invoice } from '../../types';

interface InvoiceDetailModalProps {
  invoice: Invoice | null;
  onClose: () => void;
}

export const InvoiceDetailModal: React.FC<InvoiceDetailModalProps> = ({ invoice, onClose }) => {
  const { userSession, formatCurrency, registerPayment, settings, language } = useTenant();

  const [paymentAmount, setPaymentAmount] = useState<number>(invoice?.balanceDue || 0);
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState('');
  const [printMode, setPrintMode] = useState<'a4' | 'thermal'>('a4');

  useEffect(() => {
    const cleanup = () => document.body.classList.remove('print-thermal');
    window.addEventListener('afterprint', cleanup);
    return () => window.removeEventListener('afterprint', cleanup);
  }, []);

  useEffect(() => {
    setPrintMode(settings.invoiceLayoutStyle === 'Thermal' ? 'thermal' : 'a4');
  }, [settings.invoiceLayoutStyle, invoice?.id]);

  useEffect(() => {
    setPaymentAmount(invoice?.balanceDue || 0);
    setError('');
  }, [invoice?.id, invoice?.balanceDue]);

  if (!invoice) return null;

  const printInvoice = () => {
    document.body.classList.toggle('print-thermal', printMode === 'thermal');
    window.print();
  };

  const invoiceTotal = invoice.totalAmount ?? 0;
  const taxAmount = settings.taxEnabled && settings.taxPercentage > 0
    ? invoiceTotal * (settings.taxPercentage / (100 + settings.taxPercentage))
    : 0;
  const netSubtotal = invoiceTotal - taxAmount;

  const paidAmount = invoice.paidAmount ?? 0;
  const balanceDue = invoice.balanceDue ?? 0;

  const handlePaymentSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!Number.isFinite(paymentAmount) || paymentAmount <= 0) {
      setError(language === 'ar' ? 'أدخل مبلغًا أكبر من صفر.' : 'Enter an amount greater than zero.');
      return;
    }

    setIsProcessing(true);
    setError('');
    try {
      await registerPayment(invoice.id, paymentAmount);
      onClose();
    } catch (err: any) {
      setError(err.response?.data?.detail || err.response?.data?.title || err.message || 'Payment registration failed.');
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/75 backdrop-blur-xs overflow-y-auto">
      <div className="bg-[#1a1c1f] border border-[#26292e] rounded shadow-2xl w-full max-w-2xl max-h-[92vh] flex flex-col overflow-hidden my-auto">
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-3.5 bg-[#111316] border-b border-white/5">
          <div className="flex items-center gap-2.5">
            <span className="material-symbols-outlined text-[#4edea3]">receipt</span>
            <div>
              <div className="flex items-center gap-2">
                <h3 className="text-sm font-bold text-white font-mono">{invoice.invoiceNumber}</h3>
                <span className={`px-2 py-0.5 rounded text-[10px] font-semibold ${
                  invoice.status === 'مدفوع'
                    ? 'bg-[#10b981]/15 text-[#4edea3]'
                    : invoice.status === 'مدفوع جزئياً'
                    ? 'bg-sky-500/15 text-sky-400'
                    : 'bg-amber-500/15 text-amber-400'
                }`}>
                  {invoice.status}
                </span>
              </div>
              <p className="text-[11px] text-[#8f9194]">
                Counterparty: <strong className="text-white">{invoice.customerName}</strong> · Ref Order: {invoice.orderNumber}
              </p>
            </div>
          </div>

          <div className="flex items-center gap-1">
            <button
              onClick={printInvoice}
              className="flex items-center gap-1 px-2.5 py-1 rounded bg-[#282a2d] hover:bg-[#333538] text-white text-xs cursor-pointer"
            >
              <span className="material-symbols-outlined text-sm">print</span>
              <span>Print</span>
            </button>
            <button
              onClick={onClose}
              className="p-1 rounded text-[#8f9194] hover:text-white"
            >
              <span className="material-symbols-outlined text-base">close</span>
            </button>
          </div>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-5 space-y-4 font-mono text-xs">
          {error && (
            <div className="p-2.5 rounded bg-rose-500/15 border border-rose-500/30 text-rose-400 text-xs font-sans">
              {error}
            </div>
          )}

          {/* Invoice Summary Card */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-2.5">
            <div className="p-3 bg-[#111316] rounded border border-white/5">
              <span className="text-[10px] text-[#8f9194] uppercase">Total Billed</span>
              <div className="text-sm font-bold text-white mt-1">{formatCurrency(invoiceTotal)}</div>
            </div>
            <div className="p-3 bg-[#111316] rounded border border-white/5">
              <span className="text-[10px] text-[#8f9194] uppercase">Paid Amount</span>
              <div className="text-sm font-bold text-[#4edea3] mt-1">{formatCurrency(paidAmount)}</div>
            </div>
            <div className="p-3 bg-[#111316] rounded border border-white/5">
              <span className="text-[10px] text-[#8f9194] uppercase">Balance Due</span>
              <div className={`text-sm font-bold mt-1 ${balanceDue > 0 ? 'text-amber-400' : 'text-white'}`}>
                {formatCurrency(balanceDue)}
              </div>
            </div>
            <div className="p-3 bg-[#111316] rounded border border-white/5">
              <span className="text-[10px] text-[#8f9194] uppercase">Due Date</span>
              <div className="text-sm font-bold text-white mt-1">
                {invoice.dueDate ? new Date(invoice.dueDate).toLocaleDateString() : 'Immediate'}
              </div>
            </div>
          </div>

          {/* Itemized Table */}
          <div className="rounded bg-[#111316] border border-white/5 overflow-hidden">
            <table className="w-full text-left">
              <thead className="bg-[#181a1e] text-[#8f9194] text-[10px] uppercase border-b border-white/5">
                <tr>
                  <th className="py-2 px-3">Item Details</th>
                  <th className="py-2 px-3 text-right">Qty</th>
                  <th className="py-2 px-3 text-right">Price</th>
                  <th className="py-2 px-3 text-right">Total</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/5">
                {invoice.items?.map((item, idx) => (
                  <tr key={idx}>
                    <td className="py-2.5 px-3">
                      <div className="text-white font-sans">{item.productName}</div>
                      <div className="text-[10px] text-[#8f9194]">{item.sku}</div>
                    </td>
                    <td className="py-2.5 px-3 text-right text-white">{item.quantity}</td>
                    <td className="py-2.5 px-3 text-right text-[#8f9194]">{formatCurrency(item.unitPrice)}</td>
                    <td className="py-2.5 px-3 text-right font-bold text-white">
                      {formatCurrency(item.totalPrice || item.quantity * item.unitPrice)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Settle / Register Payment Section */}
          {balanceDue > 0 && (
            <form onSubmit={handlePaymentSubmit} className="p-4 bg-[#111316] rounded border border-white/5 space-y-3 font-sans">
              <div className="flex items-center justify-between">
                <span className="text-xs font-bold text-white uppercase tracking-wider flex items-center gap-1.5">
                  <span className="material-symbols-outlined text-sm text-[#4edea3]">payments</span>
                  {language === 'ar' ? 'تسجيل دفعة نقدية أو بنكية' : 'Register Payment Receipt'}
                </span>
                <span className="text-xs text-[#8f9194] font-mono">
                  Remaining: {formatCurrency(balanceDue)}
                </span>
              </div>

              <div className="flex gap-2">
                <input
                  type="number"
                  step="0.01"
                  max={balanceDue}
                  min="0.01"
                  value={paymentAmount}
                  onChange={(e) => setPaymentAmount(parseFloat(e.target.value) || 0)}
                  placeholder="0.00"
                  className="flex-1 px-3 py-1.5 bg-[#1a1c1f] border border-[#26292e] rounded text-xs text-white font-mono outline-none focus:border-[#4edea3]"
                />
                <button
                  type="button"
                  onClick={() => setPaymentAmount(balanceDue)}
                  className="px-2.5 py-1.5 rounded bg-[#282a2d] hover:bg-[#333538] text-xs text-[#8f9194] hover:text-white font-mono cursor-pointer"
                >
                  Pay In Full
                </button>
                <button
                  type="submit"
                  disabled={isProcessing}
                  className="px-4 py-1.5 rounded bg-[#10b981] hover:bg-[#059669] text-white font-bold text-xs uppercase tracking-wider transition-all cursor-pointer"
                >
                  {isProcessing ? 'Recording...' : 'Register Payment'}
                </button>
              </div>
            </form>
          )}
        </div>
      </div>
    </div>
  );
};
