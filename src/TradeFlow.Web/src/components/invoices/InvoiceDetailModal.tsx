import React, { useEffect, useState } from 'react';
import { X, FileText, CheckCircle2, Printer, CreditCard } from 'lucide-react';
import { useTenant } from '../../context/TenantContext';
import { Invoice } from '../../types';

interface InvoiceDetailModalProps {
  invoice: Invoice | null;
  onClose: () => void;
}

export const InvoiceDetailModal: React.FC<InvoiceDetailModalProps> = ({ invoice, onClose }) => {
  const { userSession, currencySymbol, registerPayment, settings } = useTenant();

  const [paymentAmount, setPaymentAmount] = useState<number>(invoice?.balanceDue || 0);
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState('');
  const [printMode, setPrintMode] = useState<'a4' | 'thermal'>('a4');

  // Always reset the thermal print mode after printing (or when the user cancels).
  useEffect(() => {
    const cleanup = () => document.body.classList.remove('print-thermal');
    window.addEventListener('afterprint', cleanup);
    return () => window.removeEventListener('afterprint', cleanup);
  }, []);

  // The default print layout follows the Invoice.LayoutStyle system setting.
  useEffect(() => {
    setPrintMode(settings.invoiceLayoutStyle === 'Thermal' ? 'thermal' : 'a4');
  }, [settings.invoiceLayoutStyle, invoice?.id]);

  // Reset the payment draft whenever another invoice is opened.
  useEffect(() => {
    setPaymentAmount(invoice?.balanceDue || 0);
    setError('');
  }, [invoice?.id, invoice?.balanceDue]);

  if (!invoice) return null;

  const printInvoice = () => {
    document.body.classList.toggle('print-thermal', printMode === 'thermal');
    window.print();
  };

  // Prices are tax-inclusive: derive the VAT breakdown from system settings.
  const invoiceTotal = invoice.totalAmount ?? 0;
  const taxApplicable = settings.taxEnabled && settings.taxPercentage > 0;
  const taxAmount = taxApplicable ? invoiceTotal - invoiceTotal / (1 + settings.taxPercentage / 100) : 0;
  const netSubtotal = invoiceTotal - taxAmount;

  // Paid / remaining / change breakdown — displayed only when it actually applies.
  const paidAmount = invoice.paidAmount ?? 0;
  const balanceDue = invoice.balanceDue ?? 0;
  const hasRemaining = balanceDue > 0;
  const changeDue = Math.max(0, paidAmount - invoiceTotal);

  const handlePaymentSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!Number.isFinite(paymentAmount) || paymentAmount <= 0) {
      setError('أدخل مبلغًا أكبر من صفر.');
      return;
    }

    // Overpayment: only the outstanding amount is applied to the invoice and the
    // excess is returned as change to the customer (backend accepts <= balance).
    const appliedAmount = Math.min(paymentAmount, balanceDue);

    setIsProcessing(true);
    try {
      await registerPayment(invoice.id, appliedAmount);
      setError('');
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'تعذر تسجيل الدفعة.');
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-xs animate-in fade-in duration-200">
      <div className="w-full max-w-2xl max-h-[90vh] overflow-y-auto bg-white border border-slate-200 rounded-2xl shadow-2xl flex flex-col text-right" dir="rtl">
        
        {/* Header Actions */}
        <div className="px-6 py-4 border-b border-slate-100 flex items-center justify-between sticky top-0 bg-white z-10">
          <div className="flex items-center gap-2 text-xs font-bold text-slate-700">
            <FileText className="w-4 h-4 text-blue-700" /> فاتورة مبيعات ضريبية #{invoice.invoiceNumber}
          </div>
          <div className="flex items-center gap-2">
            <div className="flex items-center gap-1 rounded-xl bg-slate-100 p-1">
              <button
                type="button"
                onClick={() => setPrintMode('a4')}
                className={`rounded-lg px-2.5 py-1 text-[11px] font-bold transition ${
                  printMode === 'a4' ? 'bg-white shadow text-blue-700' : 'text-slate-500 hover:text-slate-700'
                }`}
              >
                A4
              </button>
              <button
                type="button"
                onClick={() => setPrintMode('thermal')}
                className={`rounded-lg px-2.5 py-1 text-[11px] font-bold transition ${
                  printMode === 'thermal' ? 'bg-white shadow text-blue-700' : 'text-slate-500 hover:text-slate-700'
                }`}
              >
                حراري 80mm
              </button>
            </div>
            <button
              onClick={printInvoice}
              className="px-3 py-1.5 rounded-xl bg-blue-600 hover:bg-blue-700 text-white transition text-xs font-bold flex items-center gap-1.5"
              title={printMode === 'a4' ? 'طباعة الفاتورة على ورق A4' : 'طباعة الفاتورة على رول حراري عرض 80mm'}
            >
              <Printer className="w-3.5 h-3.5" /> طباعة الفاتورة
            </button>
            <button
              onClick={onClose}
              className="p-2 rounded-xl text-slate-400 hover:text-slate-600 hover:bg-slate-100"
            >
              <X className="w-5 h-5" />
            </button>
          </div>
        </div>

        {/* Invoice Printable Document */}
        <div className="invoice-print-area p-8 space-y-6 flex-1 text-slate-800 text-xs">
          
          {/* Company Brand Header */}
          <div className="flex justify-between items-start border-b border-slate-200 pb-6">
            <div>
              <h2 className="text-2xl font-black text-slate-900 font-sans tracking-tight">
                {userSession?.name || 'مؤسسة تريد فلو التجاريه'}
              </h2>
              <p className="text-xs text-slate-500 mt-1">
                فاتورة مبيعات ضريبية معتمدة • نظام إدارة المعاملات المالية
              </p>
            </div>
            <div className="text-left">
              <span className={`inline-block px-3 py-1 rounded-full text-xs font-bold ${
                invoice.status === 'مدفوع'
                  ? 'bg-emerald-100 text-emerald-800 border border-emerald-200'
                  : invoice.status === 'متأخر'
                  ? 'bg-rose-100 text-rose-800 border border-rose-200'
                  : 'bg-amber-100 text-amber-800 border border-amber-200'
              }`}>
                {invoice.status}
              </span>
              <p className="text-xs text-slate-400 mt-2 font-mono">رقم الفاتورة: {invoice.invoiceNumber}</p>
            </div>
          </div>

          {/* Client Info & Dates */}
          <div className="grid grid-cols-2 gap-6 text-xs">
            <div>
              <p className="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">
                العميل المفوتر
              </p>
              <p className="font-extrabold text-slate-900 text-sm">{invoice.customerName ?? 'غير محدد'}</p>
              <p className="text-slate-400 mt-1">أمر المبيعات المرتبط: <strong className="font-mono text-blue-700">{invoice.orderNumber ?? '—'}</strong></p>
            </div>

            <div className="text-left space-y-1">
              <div>
                <span className="text-slate-500">تاريخ الإصدار: </span>
                <span className="font-mono font-bold">{invoice.issueDate ?? '—'}</span>
              </div>
              <div>
                <span className="text-slate-500">تاريخ الاستحقاق: </span>
                <span className="font-mono font-bold text-rose-600">{invoice.dueDate ?? '—'}</span>
              </div>
            </div>
          </div>

          {/* Summary Items Table */}
          <div className="border border-slate-200 rounded-xl overflow-hidden text-xs">
            <table className="w-full text-right">
              <thead className="bg-slate-50 text-slate-500 uppercase text-[10px] font-bold border-b border-slate-200">
                <tr>
                  <th className="py-2.5 px-4">بيان الخدمات والمنتجات</th>
                  <th className="py-2.5 px-4 text-center">عدد الأصناف</th>
                  <th className="py-2.5 px-4 text-left">المبلغ الإجمالي</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                <tr>
                  <td className="py-3 px-4">
                    <p className="font-bold text-slate-900">أمر المبيعات رقم ({invoice.orderNumber ?? '—'})</p>
                    <p className="text-[10px] text-slate-400">توريد وشحن محتويات طلبية العميل</p>
                  </td>
                  <td className="py-3 px-4 text-center font-mono font-bold">{invoice.lineItemsCount ?? 1}</td>
                  <td className="py-3 px-4 text-left font-mono font-extrabold">{(invoice.subtotal ?? 0).toFixed(2)} {currencySymbol}</td>
                </tr>
              </tbody>
            </table>
          </div>

          {/* Calculations */}
          <div className="flex justify-end">
            <div className="w-72 space-y-2 text-xs">
              <div className="flex justify-between text-slate-600">
                <span>المجموع الفرعي</span>
                <span className="font-mono font-bold">{netSubtotal.toFixed(2)} {currencySymbol}</span>
              </div>
              {taxApplicable && (
                <div className="flex justify-between text-slate-600">
                  <span>ضريبة القيمة المضافة ({settings.taxPercentage}%)</span>
                  <span className="font-mono font-bold">{taxAmount.toFixed(2)} {currencySymbol}</span>
                </div>
              )}
              <div className="flex justify-between font-extrabold text-slate-900 text-sm pt-2 border-t border-slate-200">
                <span>المبلغ الإجمالي</span>
                <span className="font-mono">{invoiceTotal.toFixed(2)} {currencySymbol}</span>
              </div>
              {hasRemaining && (
                <>
                  <div className="flex justify-between text-emerald-700 font-bold">
                    <span>دُفع حتى الآن</span>
                    <span className="font-mono">{paidAmount.toFixed(2)} {currencySymbol}</span>
                  </div>
                  <div className="flex justify-between font-extrabold text-blue-700 text-sm pt-1">
                    <span>المتبقي</span>
                    <span className="font-mono">{balanceDue.toFixed(2)} {currencySymbol}</span>
                  </div>
                </>
              )}
              {changeDue > 0 && (
                <div className="flex justify-between font-extrabold text-amber-700 text-sm pt-1">
                  <span>الباقي للعميل (دفع زيادة)</span>
                  <span className="font-mono">{changeDue.toFixed(2)} {currencySymbol}</span>
                </div>
              )}
            </div>
          </div>

          {(invoice.balanceDue ?? 0) > 0 && (
            <div className="p-4 rounded-xl bg-blue-50 border border-blue-100 space-y-3">
              <div className="flex items-center gap-2">
                <CreditCard className="w-4 h-4 text-blue-700" />
                <h4 className="text-xs font-bold text-slate-900">
                  تسجيل دفعة سداد جديدة للفاتورة
                </h4>
              </div>

              <form onSubmit={handlePaymentSubmit} className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <div>
                  <label className="block text-[10px] font-bold text-slate-600 mb-1">
                    قيمة الدفعة — مبلغ العميل ({currencySymbol})
                  </label>
                  <input
                    type="text"
                    inputMode="decimal"
                    value={paymentAmount || ''}
                    onChange={(e) => {
                      const value = e.target.value.replace(/[^0-9.]/g, '');
                      setPaymentAmount(value === '' ? 0 : Number(value));
                    }}
                    onBlur={() => setPaymentAmount(current => Math.max(1, current || 1))}
                    placeholder="اكتب المبلغ"
                    className="w-full px-3 py-1.5 text-xs rounded-xl bg-white border border-slate-200 font-mono font-bold"
                  />
                </div>

                <div className="flex items-end">
                  <button
                    type="submit"
                    disabled={isProcessing || paymentAmount <= 0}
                    className="w-full py-2 px-3 rounded-xl bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs transition shadow-xs flex items-center justify-center gap-1.5"
                  >
                    <CheckCircle2 className={`w-3.5 h-3.5 ${isProcessing ? 'animate-spin' : ''}`} /> تأكيد تسجيل الدفعة
                  </button>
                </div>
              </form>
              {paymentAmount > balanceDue && balanceDue > 0 && (
                <div className="rounded-xl border border-amber-200 bg-amber-50 p-3 text-[11px] font-bold text-amber-800 space-y-1">
                  <p>
                    المبلغ يتجاوز المتبقي — سيتم تسجيل <span className="font-mono">{balanceDue.toFixed(2)} {currencySymbol}</span> على الفاتورة فقط.
                  </p>
                  <p>
                    الباقي (فولة) يُعاد للعميل: <span className="font-mono">{(paymentAmount - balanceDue).toFixed(2)} {currencySymbol}</span>
                  </p>
                </div>
              )}
              {error && <p className="text-xs font-bold text-rose-600">{error}</p>}
            </div>
          )}

        </div>
      </div>
    </div>
  );
};
