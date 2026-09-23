import React, { useEffect, useState, useRef } from 'react';
import { useTenant } from '../../context/TenantContext';
import { Invoice } from '../../types';

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
  const { customers, warehouses, products, formatCurrency, createSalesOrder, settings, language } = useTenant();

  const [selectedCustomerId, setSelectedCustomerId] = useState(customers[0]?.id || '');
  const [selectedWarehouseId, setSelectedWarehouseId] = useState(warehouses[0]?.id || '');
  const [items, setItems] = useState<OrderLineInput[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [productSearch, setProductSearch] = useState('');
  const [scanMessage, setScanMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const scanInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (!isOpen) return;
    setSelectedCustomerId(customers[0]?.id || '');
    const defaultWarehouse = warehouses[0];
    setSelectedWarehouseId(defaultWarehouse?.id || '');
    setItems(products.map(product => ({
      id: `item-${product.id}`,
      productId: product.id,
      quantity: 0,
      unitPrice: product.unitPrice || 0,
    })));
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

  const handlePriceChange = (itemId: string, price: number) => {
    setItems(prev => prev.map(item => {
      if (item.id === itemId) {
        return { ...item, unitPrice: Math.max(0, price) };
      }
      return item;
    }));
  };

  const subtotal = items.reduce((sum, i) => sum + (i.quantity * i.unitPrice), 0);
  const selectedItemCount = items.filter(item => item.quantity > 0).length;

  const taxAmount = settings.taxEnabled ? subtotal * (settings.taxPercentage / 100) : 0;
  const grandTotal = subtotal + taxAmount;

  const handleBarcodeScan = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      const code = productSearch.trim().toLowerCase();
      if (!code) return;

      const matched = products.find(p =>
        (p.sku && p.sku.toLowerCase() === code) ||
        (p.name && p.name.toLowerCase().includes(code))
      );

      if (matched) {
        setItems(prev => prev.map(item => {
          if (item.productId === matched.id) {
            return { ...item, quantity: item.quantity + 1 };
          }
          return item;
        }));
        setScanMessage({ type: 'success', text: `+1 Added: ${matched.name}` });
        setProductSearch('');
      } else {
        setScanMessage({ type: 'error', text: `No matching product found: "${productSearch}"` });
      }
      setTimeout(() => setScanMessage(null), 2500);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    const validItems = items.filter(i => i.quantity > 0);
    if (validItems.length === 0) {
      setError(language === 'ar' ? 'يرجى تحديد كمية لصنف واحد على الأقل.' : 'Please add at least one item quantity.');
      return;
    }
    if (!selectedCustomerId) {
      setError(language === 'ar' ? 'يرجى اختيار العميل.' : 'Please select a customer.');
      return;
    }
    if (!selectedWarehouseId) {
      setError(language === 'ar' ? 'يرجى اختيار المستودع.' : 'Please select a warehouse.');
      return;
    }

    try {
      setIsSubmitting(true);
      const invoice = await createSalesOrder({
        customerId: selectedCustomerId,
        warehouseId: selectedWarehouseId,
        items: validItems.map(i => ({
          productId: i.productId,
          quantity: i.quantity,
          unitPrice: i.unitPrice,
        })),
      });

      onInvoiceCreated(invoice);
      onClose();
    } catch (err: any) {
      const msg = err.response?.data?.detail || err.response?.data?.title || err.message || 'Failed to create sales order.';
      setError(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  const visibleProducts = products.filter(p => {
    if (!productSearch) return true;
    const q = productSearch.toLowerCase();
    return (p.name || '').toLowerCase().includes(q) || (p.sku || '').toLowerCase().includes(q);
  });

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/75 backdrop-blur-xs p-4 overflow-y-auto">
      <div className="bg-[#1a1c1f] border border-[#26292e] rounded shadow-2xl w-full max-w-4xl max-h-[92vh] flex flex-col overflow-hidden my-auto">
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-3.5 bg-[#111316] border-b border-white/5">
          <div className="flex items-center gap-2">
            <span className="material-symbols-outlined text-[#4edea3]">receipt_long</span>
            <div>
              <h3 className="text-sm font-bold text-white">
                {language === 'ar' ? 'إنشاء أمر مبيعات جديد' : 'New Sales Order Entry'}
              </h3>
              <p className="text-[11px] text-[#8f9194]">
                Direct booking and instantaneous invoice generation
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-1 rounded text-[#8f9194] hover:text-white hover:bg-[#282a2d] transition-colors"
          >
            <span className="material-symbols-outlined text-base">close</span>
          </button>
        </div>

        {/* Form Body */}
        <form onSubmit={handleSubmit} className="flex-1 overflow-y-auto p-5 space-y-4">
          {error && (
            <div className="p-3 rounded bg-rose-500/15 border border-rose-500/30 text-rose-400 text-xs">
              {error}
            </div>
          )}

          {scanMessage && (
            <div
              className={`p-2.5 rounded text-xs ${
                scanMessage.type === 'success'
                  ? 'bg-[#10b981]/15 text-[#4edea3] border border-[#10b981]/30'
                  : 'bg-rose-500/15 text-rose-400 border border-rose-500/30'
              }`}
            >
              {scanMessage.text}
            </div>
          )}

          {/* Customer & Warehouse Selection */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                {language === 'ar' ? 'العميل / الطرف المقابل' : 'Counterparty / Customer'} *
              </label>
              <select
                value={selectedCustomerId}
                onChange={(e) => setSelectedCustomerId(e.target.value)}
                required
                className="w-full px-3 py-2 bg-[#111316] border border-[#26292e] rounded text-xs text-white outline-none focus:border-[#4edea3]"
              >
                {customers.map(c => (
                  <option key={c.id} value={c.id}>
                    {c.name} {c.company ? `(${c.company})` : ''} - Limit: {formatCurrency(c.creditLimit || 0)}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                {language === 'ar' ? 'مستودع الصرف' : 'Fulfillment Warehouse'} *
              </label>
              <select
                value={selectedWarehouseId}
                onChange={(e) => setSelectedWarehouseId(e.target.value)}
                required
                className="w-full px-3 py-2 bg-[#111316] border border-[#26292e] rounded text-xs text-white outline-none focus:border-[#4edea3]"
              >
                {warehouses.map(w => (
                  <option key={w.id} value={w.id}>
                    {w.name} - {w.location}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {/* Barcode & Search Input */}
          <div className="relative">
            <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-sm text-[#8f9194]">
              search
            </span>
            <input
              ref={scanInputRef}
              type="text"
              value={productSearch}
              onChange={(e) => setProductSearch(e.target.value)}
              onKeyDown={handleBarcodeScan}
              placeholder={language === 'ar' ? 'ابحث عن اسم الصنف أو امسح الباركود واضغط Enter...' : 'Search item name, SKU or scan barcode and press Enter...'}
              className="w-full pl-9 pr-3 py-2 bg-[#111316] border border-[#26292e] rounded text-xs text-white placeholder-[#8f9194] outline-none focus:border-[#4edea3]"
            />
          </div>

          {/* Line Items Table */}
          <div className="rounded bg-[#111316] border border-white/5 overflow-hidden">
            <div className="max-h-60 overflow-y-auto">
              <table className="w-full text-left font-mono text-xs">
                <thead className="bg-[#181a1e] sticky top-0 text-[#8f9194] text-[10px] uppercase border-b border-white/5">
                  <tr>
                    <th className="py-2 px-3">Item / SKU</th>
                    <th className="py-2 px-3 text-right">Stock</th>
                    <th className="py-2 px-3 text-right">Unit Price</th>
                    <th className="py-2 px-3 text-center w-36">Quantity</th>
                    <th className="py-2 px-3 text-right">Total</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-white/5">
                  {visibleProducts.map(product => {
                    const line = items.find(i => i.productId === product.id) || { quantity: 0, unitPrice: product.unitPrice || 0 };
                    const isSelected = line.quantity > 0;
                    return (
                      <tr key={product.id} className={`hover:bg-[#282a2d]/40 transition-colors ${isSelected ? 'bg-[#282a2d]/30' : ''}`}>
                        <td className="py-2.5 px-3">
                          <div className="text-white font-sans font-medium">{product.name}</div>
                          <div className="text-[10px] text-[#8f9194]">{product.sku}</div>
                        </td>
                        <td className="py-2.5 px-3 text-right text-[#8f9194]">
                          {product.availableStock ?? product.currentStock ?? 0}
                        </td>
                        <td className="py-2.5 px-3 text-right text-[#8f9194]">
                          <input
                            type="number"
                            step="0.01"
                            value={line.unitPrice}
                            onChange={(e) => handlePriceChange(`item-${product.id}`, parseFloat(e.target.value) || 0)}
                            className="w-20 px-1.5 py-0.5 bg-[#1a1c1f] border border-[#26292e] rounded text-right text-xs text-white"
                          />
                        </td>
                        <td className="py-2.5 px-3 text-center">
                          <div className="flex items-center justify-center gap-1">
                            <button
                              type="button"
                              onClick={() => handleQtyChange(`item-${product.id}`, line.quantity - 1)}
                              className="w-6 h-6 rounded bg-[#282a2d] hover:bg-[#333538] text-white flex items-center justify-center font-bold"
                            >
                              -
                            </button>
                            <input
                              type="number"
                              min="0"
                              value={line.quantity}
                              onChange={(e) => handleQtyChange(`item-${product.id}`, parseInt(e.target.value) || 0)}
                              className="w-14 px-1 py-0.5 bg-[#1a1c1f] border border-[#26292e] rounded text-center text-xs text-white font-bold"
                            />
                            <button
                              type="button"
                              onClick={() => handleQtyChange(`item-${product.id}`, line.quantity + 1)}
                              className="w-6 h-6 rounded bg-[#282a2d] hover:bg-[#333538] text-white flex items-center justify-center font-bold"
                            >
                              +
                            </button>
                          </div>
                        </td>
                        <td className="py-2.5 px-3 text-right font-bold text-white">
                          {formatCurrency(line.quantity * line.unitPrice)}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>

          {/* Settlement Summary */}
          <div className="p-3.5 bg-[#111316] rounded border border-white/5 flex flex-col sm:flex-row sm:items-center justify-between gap-3 text-xs font-mono">
            <div className="text-[#8f9194]">
              <span>Selected items: </span>
              <strong className="text-white">{selectedItemCount}</strong>
            </div>

            <div className="space-y-1 text-right">
              <div className="text-[#8f9194] flex justify-end gap-3">
                <span>Subtotal:</span>
                <span className="text-white font-bold">{formatCurrency(subtotal)}</span>
              </div>
              {settings.taxEnabled && (
                <div className="text-[#8f9194] flex justify-end gap-3">
                  <span>VAT ({settings.taxPercentage}%):</span>
                  <span>{formatCurrency(taxAmount)}</span>
                </div>
              )}
              <div className="text-base text-white font-bold flex justify-end gap-3 pt-1 border-t border-white/10">
                <span>Total Settlement:</span>
                <span className="text-[#4edea3]">{formatCurrency(grandTotal)}</span>
              </div>
            </div>
          </div>

          {/* Footer Actions */}
          <div className="flex items-center justify-end gap-2 pt-2 border-t border-white/5">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 rounded bg-[#282a2d] hover:bg-[#333538] text-[#8f9194] hover:text-white text-xs font-semibold cursor-pointer"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting || selectedItemCount === 0}
              className={`px-5 py-2 rounded font-bold text-xs uppercase tracking-wider transition-all cursor-pointer ${
                selectedItemCount > 0 && !isSubmitting
                  ? 'bg-[#ffffff] hover:bg-[#e2e2e4] text-[#111316] shadow-md'
                  : 'bg-[#282a2d] text-[#8f9194] cursor-not-allowed'
              }`}
            >
              {isSubmitting ? 'Confirming Order...' : 'Confirm & Generate Invoice'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
