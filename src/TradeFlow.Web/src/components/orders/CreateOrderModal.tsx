import React, { useEffect, useState } from 'react';
import { X, ShoppingBag, LoaderCircle, Search } from 'lucide-react';
import { useTenant } from '../../context/TenantContext';
import { Invoice } from '../../types';
import { getApiErrorMessage } from '../../services/apiClient';

interface CreateOrderModalProps {
  isOpen: boolean;
  onClose: () => void;
  onInvoiceCreated: (invoice: Invoice) => void;
}

interface OrderLineInput {
  id: string;
  productId: string;
  quantity: number;
  unitPrice: number;
}

export const CreateOrderModal: React.FC<CreateOrderModalProps> = ({ isOpen, onClose, onInvoiceCreated }) => {
  const { customers, warehouses, products, currencySymbol, createSalesOrder, settings } = useTenant();

  const [selectedCustomerId, setSelectedCustomerId] = useState(customers[0]?.id || '');
  const [selectedWarehouseId, setSelectedWarehouseId] = useState(warehouses[0]?.id || '');
  const [items, setItems] = useState<OrderLineInput[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [productSearch, setProductSearch] = useState('');
  const [scanMessage, setScanMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const scanInputRef = React.useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (!isOpen) return;
    setSelectedCustomerId(customers[0]?.id || '');
    const defaultWarehouse = warehouses.find(warehouse => warehouse.name?.includes('رئيسي')) || warehouses[0];
    setSelectedWarehouseId(defaultWarehouse?.id || '');
    setItems(products.map(product => ({ id: `item-${product.id}`, productId: product.id, quantity: 0, unitPrice: product.unitPrice || 0 })));
    setError('');
    setProductSearch('');
  }, [isOpen, customers, warehouses, products]);

  if (!isOpen) return null;

  const handleQtyChange = (itemId: string, qty: number) => {
    setItems(prev => prev.map(item => {
      if (item.id === itemId) {
        return { ...item, quantity: Math.max(0, qty) };
      }
      return item;
    }));
  };

  const subtotal = items.reduce((sum, i) => sum + (i.quantity * i.unitPrice), 0);
  const selectedItemCount = items.filter(item => item.quantity > 0).length;

  // Prices are treated as tax-inclusive; split out the VAT for display when enabled.
  const taxApplicable = settings.taxEnabled && settings.taxPercentage > 0;
  const taxAmount = taxApplicable ? subtotal - subtotal / (1 + settings.taxPercentage / 100) : 0;
  const netSubtotal = subtotal - taxAmount;

  const normalizedSearch = productSearch.trim().toLowerCase();
  const visibleItems = normalizedSearch
    ? items.filter(item => {
        const product = products.find(p => p.id === item.productId);
        return (product?.name ?? '').toLowerCase().includes(normalizedSearch)
          || (product?.sku ?? '').toLowerCase().includes(normalizedSearch);
      })
    : items;

  // Barcode/SKU quick-scan: Enter on the scan box matches a product (exact SKU,
  // then exact name, then the single filtered result), adds one unit and moves
  // focus straight to that product's quantity field for fast number punching.
  const handleScanKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key !== 'Enter') return;
    e.preventDefault();

    const raw = productSearch.trim();
    if (!raw) return;

    const lower = raw.toLowerCase();
    const exactMatch = products.find(p => (p.sku ?? '').trim().toLowerCase() === lower)
      ?? products.find(p => (p.name ?? '').trim().toLowerCase() === lower)
      ?? (visibleItems.length === 1 ? products.find(p => p.id === visibleItems[0].productId) : undefined);

    if (!exactMatch) {
      setScanMessage({ type: 'error', text: `لا يوجد صنف يطابق «${raw}» — تحقق من الباركود أو الرمز.` });
      return;
    }

    setItems(prev => prev.map(item => (item.productId === exactMatch.id
      ? { ...item, quantity: item.quantity > 0 ? item.quantity + 1 : 1 }
      : item)));
    setProductSearch('');
    setScanMessage({ type: 'success', text: `تمت إضافة ${exactMatch.name} — أدخل الكمية أو امسح التالي.` });

    window.setTimeout(() => {
      const qtyInput = document.getElementById(`qty-${exactMatch.id}`) as HTMLInputElement | null;
      qtyInput?.focus();
      qtyInput?.select();
    }, 0);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const selectedItems = items.filter(item => item.quantity > 0);
    if (!selectedCustomerId || !selectedWarehouseId || selectedItems.length === 0) {
      setError('يرجى اختيار العميل والمستودع وكتابة كمية صنف واحدة على الأقل.');
      return;
    }

    // Guard: the backend rejects unit prices <= 0 — surface a clear message up-front.
    if (selectedItems.some(item => !(item.unitPrice > 0))) {
      setError('سعر بيع واحد أو أكثر غير صالح (صفر). راجع أسعار الأصناف قبل الحفظ.');
      return;
    }

    setIsSubmitting(true);
    try {
      const invoice = await createSalesOrder({
        customerId: selectedCustomerId,
        warehouseId: selectedWarehouseId,
        items: selectedItems.map(i => ({
          productId: i.productId,
          quantity: i.quantity,
          unitPrice: i.unitPrice,
        })),
      });
      onClose();
      onInvoiceCreated(invoice);
    } catch (err) {
      setError(getApiErrorMessage(err, 'تعذر حفظ أمر المبيعات.'));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-xs animate-in fade-in duration-200">
      <div className="w-full max-w-2xl max-h-[90vh] overflow-y-auto bg-white border border-slate-200 rounded-2xl shadow-2xl flex flex-col text-right font-sans" dir="rtl">
        
        {/* Header */}
        <div className="px-6 py-4 border-b border-slate-100 flex items-center justify-between sticky top-0 bg-white z-10">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-xl bg-blue-50 text-blue-700">
              <ShoppingBag className="w-5 h-5" />
            </div>
            <div>
              <h2 className="text-base font-bold text-slate-900">
                إصدار أمر مبيعات جديد
              </h2>
              <p className="text-xs text-slate-500">
                خطوة واحدة: حفظ الأمر، خصم المخزون، وإصدار الفاتورة تلقائيًا
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-2 rounded-xl text-slate-400 hover:text-slate-600 hover:bg-slate-100 transition"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Form Body */}
        <form onSubmit={handleSubmit} className="p-6 space-y-6 flex-1 text-xs">
          
          {/* Customer & Warehouse Selection */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block font-bold text-slate-700 mb-1.5">
                اسم العميل
              </label>
              <select
                value={selectedCustomerId}
                onChange={(e) => setSelectedCustomerId(e.target.value)}
                className="w-full px-3 py-2 text-xs rounded-xl bg-slate-50 border border-slate-200 text-slate-900 font-medium focus:ring-2 focus:ring-blue-500"
              >
                {customers.length === 0 ? (
                  <option value="">لا يوجد عملاء متاحين</option>
                ) : (
                  customers.map(c => (
                    <option key={c.id} value={c.id}>
                      {c.name}
                    </option>
                  ))
                )}
              </select>
            </div>

            <div>
              <label className="block font-bold text-slate-700 mb-1.5">
                المخزن
              </label>
              <select
                value={selectedWarehouseId}
                onChange={(e) => setSelectedWarehouseId(e.target.value)}
                className="w-full px-3 py-2 text-xs rounded-xl bg-slate-50 border border-slate-200 text-slate-900 font-medium focus:ring-2 focus:ring-blue-500"
              >
                {warehouses.length === 0 ? (
                  <option value="">لا يوجد مستودعات متاحة</option>
                ) : (
                  warehouses.map(w => (
                    <option key={w.id} value={w.id}>
                      {w.name} ({w.location})
                    </option>
                  ))
                )}
              </select>
            </div>
          </div>

          {/* Line Items Table */}
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <h3 className="text-xs font-bold text-slate-900 uppercase tracking-wider">
                بنود ومحتويات أمر المبيعات
              </h3>
              <span className="text-[10px] text-slate-500">اكتب الكمية أمام كل صنف مطلوب</span>
            </div>

            {/* Barcode / SKU quick-scan box */}
            <div className="relative">
              <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-slate-400" />
              <input
                ref={scanInputRef}
                type="text"
                autoFocus
                value={productSearch}
                onChange={(e) => {
                  setProductSearch(e.target.value);
                  if (scanMessage) setScanMessage(null);
                }}
                onKeyDown={handleScanKeyDown}
                placeholder="امسح الباركود أو اكتب SKU ثم اضغط Enter..."
                className="w-full pr-9 pl-3 py-2 rounded-xl bg-slate-50 border border-slate-200 text-xs text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            {scanMessage && (
              <p className={`text-[11px] font-bold ${scanMessage.type === 'success' ? 'text-emerald-600' : 'text-rose-600'}`}>
                {scanMessage.text}
              </p>
            )}

            <div className="border border-slate-200 rounded-xl overflow-hidden divide-y divide-slate-100">
              {visibleItems.map((item) => (
                <div key={item.id} className="p-3 bg-slate-50/50 grid grid-cols-12 gap-3 items-center">
                  <div className="col-span-5">
                    <p className="font-bold text-slate-900">{products.find(product => product.id === item.productId)?.name || 'صنف'}</p>
                    <p className="text-[10px] text-blue-700">رمز: {products.find(product => product.id === item.productId)?.sku || '—'}</p>
                  </div>

                  <div className="col-span-3">
                    <label className="block text-[10px] text-slate-500 font-medium mb-1">
                      الكمية المطلوبة (اتركها 0 لعدم الاختيار)
                    </label>
                    <input
                      id={`qty-${item.productId}`}
                      type="number"
                      value={item.quantity}
                      min={0}
                      onChange={(e) => handleQtyChange(item.id, Math.max(0, Number.parseInt(e.target.value, 10) || 0))}
                      onBlur={(e) => handleQtyChange(item.id, Math.max(0, Number.parseInt(e.target.value, 10) || 0))}
                      onKeyDown={(e) => {
                        // After punching the quantity, Enter returns to the scan box
                        // for the next product — continuous POS-style flow.
                        if (e.key === 'Enter') {
                          e.preventDefault();
                          scanInputRef.current?.focus();
                        }
                      }}
                      className="w-full px-2.5 py-1.5 text-xs rounded-lg bg-white border border-slate-200 text-slate-900 font-mono font-bold text-center"
                    />
                  </div>

                  <div className="col-span-3 text-left">
                    <label className="block text-[10px] text-slate-500 font-medium mb-1">
                      سعر الوحدة
                    </label>
                    <span className="text-xs font-mono font-bold text-slate-700">
                      {item.unitPrice} {currencySymbol}
                    </span>
                  </div>

                  <div className="col-span-1 text-center text-[10px] text-slate-400">{item.quantity > 0 ? 'مختار' : ''}</div>
                </div>
              ))}
              {visibleItems.length === 0 && (
                <div className="p-4 text-center text-xs text-slate-400">لا توجد أصناف مطابقة للبحث.</div>
              )}
            </div>
          </div>

          {/* VAT breakdown (from system settings) + cash-sale notice */}
          {taxApplicable && (
            <div className="rounded-xl bg-white border border-slate-200 p-3 space-y-1 text-xs">
              <div className="flex justify-between text-slate-600">
                <span>المجموع قبل الضريبة</span>
                <span className="font-mono font-bold">{netSubtotal.toFixed(2)} {currencySymbol}</span>
              </div>
              <div className="flex justify-between text-slate-600">
                <span>ضريبة القيمة المضافة ({settings.taxPercentage}%)</span>
                <span className="font-mono font-bold">{taxAmount.toFixed(2)} {currencySymbol}</span>
              </div>
            </div>
          )}
          {!settings.creditSalesEnabled && (
            <p className="text-[10px] font-bold text-amber-700 bg-amber-50 border border-amber-100 rounded-lg px-2.5 py-1.5">
              البيع الآجل معطّل: سيتم تحصيل كامل المبلغ وإصدار الفاتورة كمدفوعة فور التأكيد (بيع نقدي).
            </p>
          )}

          {/* Subtotal & Action */}
          <div className="p-4 rounded-xl bg-slate-50 border border-slate-200 flex justify-between items-center text-sm">
            <div>
              <span className="font-bold text-slate-900">الإجمالي {taxApplicable ? '(شامل الضريبة)' : 'الصافي'}</span>
              <span className="block text-[10px] text-slate-500">{selectedItemCount} صنف محدد</span>
            </div>
            <span className="font-mono font-extrabold text-blue-700 text-base">
              {subtotal.toFixed(2)} {currencySymbol}
            </span>
          </div>

          {error && <p className="text-xs font-bold text-rose-600">{error}</p>}

          <div className="pt-4 border-t border-slate-100 flex justify-end gap-3">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 rounded-xl text-xs font-bold text-slate-600 hover:bg-slate-100"
            >
              إلغاء
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="flex items-center gap-2 px-5 py-2 rounded-xl bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs shadow-sm transition"
            >
              {isSubmitting && <LoaderCircle className="w-3.5 h-3.5 animate-spin" />} {isSubmitting ? 'جارٍ الحفظ والتأكيد والإصدار' : 'حفظ وتأكيد وإصدار الفاتورة'}
            </button>
          </div>

        </form>
      </div>
    </div>
  );
};
