import React, { useState, useMemo } from 'react';
import { useTenant } from '../../context/TenantContext';

export const TradingTerminalPage: React.FC = () => {
  const {
    products,
    warehouses,
    customers,
    orders,
    createSalesOrder,
    formatCurrency,
    settings,
    language,
    setCurrentPage,
  } = useTenant();

  const [selectedProductId, setSelectedProductId] = useState<string>(products[0]?.id || '');
  const [selectedWarehouseId, setSelectedWarehouseId] = useState<string>(warehouses[0]?.id || '');
  const [selectedCustomerId, setSelectedCustomerId] = useState<string>(customers[0]?.id || '');
  const [orderSide, setOrderSide] = useState<'BUY' | 'SELL'>('SELL'); // SELL = Sales Order, BUY = Stock replenishment
  const [quantity, setQuantity] = useState<number>(1);
  const [customPrice, setCustomPrice] = useState<number | ''>('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [executionMessage, setExecutionMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const activeProduct = useMemo(() => {
    return products.find(p => p.id === selectedProductId) || products[0];
  }, [products, selectedProductId]);

  const effectivePrice = customPrice !== '' ? Number(customPrice) : (activeProduct?.unitPrice || 0);
  const subtotal = effectivePrice * (quantity || 0);
  const taxAmount = settings.taxEnabled ? subtotal * (settings.taxPercentage / 100) : 0;
  const totalAmount = subtotal + taxAmount;

  const handleQuickQuantity = (qty: number) => {
    setQuantity(qty);
  };

  const handleMaxQuantity = () => {
    if (activeProduct) {
      setQuantity(Math.max(1, activeProduct.availableStock || activeProduct.currentStock || 10));
    }
  };

  const handleExecute = async () => {
    if (!activeProduct) {
      setExecutionMessage({ type: 'error', text: 'Please select an instrument/product.' });
      return;
    }
    if (!selectedCustomerId) {
      setExecutionMessage({ type: 'error', text: 'Please select a customer/counterparty.' });
      return;
    }
    if (!quantity || quantity <= 0) {
      setExecutionMessage({ type: 'error', text: 'Order quantity must be greater than 0.' });
      return;
    }

    try {
      setIsSubmitting(true);
      setExecutionMessage(null);

      await createSalesOrder({
        customerId: selectedCustomerId,
        warehouseId: selectedWarehouseId || warehouses[0]?.id,
        items: [
          {
            productId: activeProduct.id,
            quantity: Number(quantity),
            unitPrice: Number(effectivePrice),
          },
        ],
      });

      setExecutionMessage({
        type: 'success',
        text: `Execution confirmed: Order executed for ${quantity}x ${activeProduct.name} at ${formatCurrency(effectivePrice)}.`,
      });
      setQuantity(1);
    } catch (err: any) {
      const msg = err.response?.data?.detail || err.response?.data?.title || err.message || 'Execution rejected by engine.';
      setExecutionMessage({ type: 'error', text: msg });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="space-y-4">
      {/* Top Ticker & Execution Control Strip */}
      <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5 flex flex-col lg:flex-row lg:items-center justify-between gap-4">
        {/* Active Instrument Badge & Realtime Price */}
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-2.5">
            <span className="w-8 h-8 rounded bg-[#282a2d] border border-white/10 flex items-center justify-center font-mono font-bold text-xs text-[#4edea3]">
              {activeProduct?.sku?.slice(0, 3) || 'SKU'}
            </span>
            <div>
              <div className="flex items-center gap-2">
                <h1 className="text-sm font-semibold text-white">
                  {activeProduct?.name || 'Select Instrument'}
                </h1>
                <span className="text-[10px] font-mono px-1.5 py-0.5 rounded bg-[#282a2d] text-[#8f9194]">
                  {activeProduct?.sku || 'SKU-000'}
                </span>
              </div>
              <p className="text-[11px] text-[#8f9194]">
                {activeProduct?.category || 'Trading Instrument'} · Spot Settlement
              </p>
            </div>
          </div>

          <div className="h-8 w-px bg-white/10 hidden sm:block"></div>

          {/* Realtime Price Metric */}
          <div>
            <div className="text-lg font-bold font-mono text-white">
              {formatCurrency(activeProduct?.unitPrice || 0)}
            </div>
            <div className="text-[10px] text-[#4edea3] font-mono flex items-center gap-1">
              <span>Cost: {formatCurrency(activeProduct?.costPrice || 0)}</span>
              <span>· Margin: {activeProduct && activeProduct.unitPrice ? (((activeProduct.unitPrice - (activeProduct.costPrice || 0)) / activeProduct.unitPrice) * 100).toFixed(0) : 0}%</span>
            </div>
          </div>
        </div>

        {/* Ticker Quick Switcher Chips */}
        <div className="flex flex-wrap items-center gap-1.5 overflow-x-auto max-w-full">
          {products.slice(0, 6).map(prod => (
            <button
              key={prod.id}
              onClick={() => {
                setSelectedProductId(prod.id);
                setCustomPrice('');
              }}
              className={`px-2 py-1 rounded text-xs font-mono transition-colors cursor-pointer ${
                prod.id === activeProduct?.id
                  ? 'bg-[#333538] text-white border border-[#4edea3]/40'
                  : 'bg-[#282a2d] text-[#8f9194] hover:text-white'
              }`}
            >
              {prod.sku}
            </button>
          ))}
        </div>
      </div>

      {/* Main Trading Floor Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-4">
        {/* Left Column: Order Entry Ticket (4 Cols) */}
        <div className="lg:col-span-5 xl:col-span-4 p-4 rounded bg-[#1a1c1f] border border-white/5 space-y-4">
          <div className="flex items-center justify-between pb-2 border-b border-white/5">
            <h2 className="text-xs font-semibold uppercase tracking-wider text-[#8f9194]">
              {language === 'ar' ? 'تذكرة تنفيذ الأوامر' : 'Order Entry Ticket'}
            </h2>
            <div className="flex items-center gap-1 text-[11px] text-[#8f9194]">
              <span className="w-1.5 h-1.5 rounded-full bg-[#10b981]"></span>
              <span>FIX Active</span>
            </div>
          </div>

          {/* Buy / Sell Segmented Control */}
          <div className="grid grid-cols-2 gap-1 p-1 bg-[#111316] rounded border border-white/5">
            <button
              type="button"
              onClick={() => setOrderSide('SELL')}
              className={`py-1.5 text-xs font-bold rounded transition-colors ${
                orderSide === 'SELL'
                  ? 'bg-rose-600 text-white shadow-sm'
                  : 'text-[#8f9194] hover:text-white'
              }`}
            >
              {language === 'ar' ? 'أمر مبيعات (بيع)' : 'SELL / ORDER'}
            </button>
            <button
              type="button"
              onClick={() => setOrderSide('BUY')}
              className={`py-1.5 text-xs font-bold rounded transition-colors ${
                orderSide === 'BUY'
                  ? 'bg-[#10b981] text-white shadow-sm'
                  : 'text-[#8f9194] hover:text-white'
              }`}
            >
              {language === 'ar' ? 'أمر شراء (توريد)' : 'BUY / REPLENISH'}
            </button>
          </div>

          {/* Execution feedback */}
          {executionMessage && (
            <div
              className={`p-2.5 rounded text-xs ${
                executionMessage.type === 'success'
                  ? 'bg-[#10b981]/15 text-[#4edea3] border border-[#10b981]/30'
                  : 'bg-rose-500/15 text-rose-400 border border-rose-500/30'
              }`}
            >
              {executionMessage.text}
            </div>
          )}

          {/* Form Fields */}
          <div className="space-y-3">
            {/* Instrument Selection */}
            <div>
              <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                Instrument (Product)
              </label>
              <select
                value={selectedProductId}
                onChange={(e) => {
                  setSelectedProductId(e.target.value);
                  setCustomPrice('');
                }}
                className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-xs text-white focus:border-[#4edea3] outline-none"
              >
                {products.map(p => (
                  <option key={p.id} value={p.id}>
                    {p.sku} - {p.name} (Avail: {p.availableStock ?? p.currentStock ?? 0})
                  </option>
                ))}
              </select>
            </div>

            {/* Counterparty / Customer */}
            <div>
              <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                Counterparty (Customer)
              </label>
              <select
                value={selectedCustomerId}
                onChange={(e) => setSelectedCustomerId(e.target.value)}
                className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-xs text-white focus:border-[#4edea3] outline-none"
              >
                {customers.map(c => (
                  <option key={c.id} value={c.id}>
                    {c.name} {c.company ? `(${c.company})` : ''} - Credit: {formatCurrency(c.creditLimit || 0)}
                  </option>
                ))}
              </select>
            </div>

            {/* Warehouse Allocation */}
            <div>
              <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                Fulfillment Warehouse
              </label>
              <select
                value={selectedWarehouseId}
                onChange={(e) => setSelectedWarehouseId(e.target.value)}
                className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-xs text-white focus:border-[#4edea3] outline-none"
              >
                {warehouses.map(w => (
                  <option key={w.id} value={w.id}>
                    {w.name} - {w.location}
                  </option>
                ))}
              </select>
            </div>

            {/* Quantity Input & Power Presets */}
            <div>
              <div className="flex items-center justify-between mb-1">
                <label className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
                  Quantity (Units)
                </label>
                <span className="text-[10px] text-[#8f9194] font-mono">
                  In Hand: {activeProduct?.availableStock ?? activeProduct?.currentStock ?? 0}
                </span>
              </div>
              <input
                type="number"
                min="1"
                value={quantity}
                onChange={(e) => setQuantity(Math.max(1, parseInt(e.target.value) || 0))}
                className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-xs font-mono text-white focus:border-[#4edea3] outline-none"
              />
              {/* Presets */}
              <div className="grid grid-cols-5 gap-1 mt-1.5">
                {[5, 10, 25, 50].map(q => (
                  <button
                    key={q}
                    type="button"
                    onClick={() => handleQuickQuantity(q)}
                    className="py-1 text-[10px] font-mono rounded bg-[#282a2d] hover:bg-[#333538] text-[#e2e2e6] transition-colors cursor-pointer"
                  >
                    +{q}
                  </button>
                ))}
                <button
                  type="button"
                  onClick={handleMaxQuantity}
                  className="py-1 text-[10px] font-mono rounded bg-[#282a2d] hover:bg-[#333538] text-[#4edea3] font-semibold transition-colors cursor-pointer"
                >
                  MAX
                </button>
              </div>
            </div>

            {/* Limit Price */}
            <div>
              <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                Execution Price
              </label>
              <div className="relative">
                <input
                  type="number"
                  step="0.01"
                  placeholder={activeProduct?.unitPrice?.toString() || '0'}
                  value={customPrice}
                  onChange={(e) => setCustomPrice(e.target.value === '' ? '' : parseFloat(e.target.value))}
                  className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-xs font-mono text-white focus:border-[#4edea3] outline-none"
                />
                <button
                  type="button"
                  onClick={() => setCustomPrice('')}
                  className="absolute right-2 top-1.5 text-[10px] text-[#8f9194] hover:text-white"
                >
                  Reset
                </button>
              </div>
            </div>

            {/* Settlement Breakdown Box */}
            <div className="p-3 bg-[#111316] rounded border border-white/5 space-y-1.5 text-xs font-mono">
              <div className="flex justify-between text-[#8f9194]">
                <span>Gross Nominal:</span>
                <span>{formatCurrency(subtotal)}</span>
              </div>
              {settings.taxEnabled && (
                <div className="flex justify-between text-[#8f9194]">
                  <span>VAT ({settings.taxPercentage}%):</span>
                  <span>{formatCurrency(taxAmount)}</span>
                </div>
              )}
              <div className="flex justify-between text-white font-bold pt-1 border-t border-white/5 text-sm">
                <span>Net Settlement:</span>
                <span className="text-[#4edea3]">{formatCurrency(totalAmount)}</span>
              </div>
            </div>

            {/* Submit Execution Button */}
            <button
              type="button"
              onClick={handleExecute}
              disabled={isSubmitting}
              className={`w-full py-2.5 rounded font-bold text-xs uppercase tracking-wider transition-all shadow-md cursor-pointer ${
                orderSide === 'SELL'
                  ? 'bg-rose-600 hover:bg-rose-500 text-white'
                  : 'bg-[#10b981] hover:bg-[#059669] text-white'
              } ${isSubmitting ? 'opacity-50 cursor-not-allowed' : ''}`}
            >
              {isSubmitting ? 'Executing in FIX Engine...' : `EXECUTE ${orderSide} ORDER`}
            </button>
          </div>
        </div>

        {/* Right Columns: Book Depth & Live Market Feeds (7-8 cols) */}
        <div className="lg:col-span-7 xl:col-span-8 space-y-4">
          {/* Depth / Stock Distribution by Warehouse */}
          <div className="p-4 rounded bg-[#1a1c1f] border border-white/5 space-y-3">
            <div className="flex items-center justify-between pb-2 border-b border-white/5">
              <h2 className="text-xs font-semibold uppercase tracking-wider text-[#8f9194]">
                {language === 'ar' ? 'توزيع المخزون عبر المراكز' : 'Warehouse Inventory Distribution & Book Depth'}
              </h2>
              <span className="text-xs text-[#8f9194] font-mono">
                Instrument: {activeProduct?.sku}
              </span>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
              {warehouses.map(wh => (
                <div key={wh.id} className="p-3 bg-[#111316] rounded border border-white/5">
                  <div className="flex items-center justify-between">
                    <span className="text-xs font-semibold text-white">{wh.name}</span>
                    <span className="text-[10px] font-mono text-[#8f9194]">{wh.location}</span>
                  </div>
                  <div className="mt-2 flex items-baseline justify-between">
                    <span className="text-lg font-bold font-mono text-[#4edea3]">
                      {activeProduct?.warehouseId === wh.id ? (activeProduct.currentStock || 0) : 0}
                    </span>
                    <span className="text-[10px] text-[#8f9194]">units ready</span>
                  </div>
                  <div className="mt-1 h-1 w-full bg-[#282a2d] rounded-full overflow-hidden">
                    <div
                      className="h-full bg-[#4edea3]"
                      style={{ width: `${Math.min(100, ((activeProduct?.currentStock || 0) / 100) * 100)}%` }}
                    ></div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Recent Executions Stream */}
          <div className="p-4 rounded bg-[#1a1c1f] border border-white/5">
            <div className="flex items-center justify-between pb-2 mb-2 border-b border-white/5">
              <div className="flex items-center gap-2">
                <h2 className="text-xs font-semibold uppercase tracking-wider text-[#8f9194]">
                  {language === 'ar' ? 'آخر التنفيذات في السوق' : 'Latest Execution Ledger'}
                </h2>
                <span className="w-1.5 h-1.5 rounded-full bg-[#4edea3] animate-pulse"></span>
              </div>
              <button
                onClick={() => setCurrentPage('orders')}
                className="text-xs text-[#8f9194] hover:text-[#4edea3] transition-colors"
              >
                Inspect All
              </button>
            </div>

            <div className="overflow-x-auto">
              <table className="w-full text-left font-mono text-xs">
                <thead>
                  <tr className="text-[#8f9194] text-[10px] uppercase border-b border-white/5">
                    <th className="py-1.5 px-2">Order #</th>
                    <th className="py-1.5 px-2">Counterparty</th>
                    <th className="py-1.5 px-2 text-right">Qty</th>
                    <th className="py-1.5 px-2 text-right">Settlement</th>
                    <th className="py-1.5 px-2 text-right">Status</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-white/5">
                  {orders.slice(0, 5).map(o => (
                    <tr key={o.id} className="hover:bg-[#282a2d]/40">
                      <td className="py-2 px-2 text-white font-semibold">{o.orderNumber}</td>
                      <td className="py-2 px-2 text-[#e2e2e6] font-sans truncate max-w-[150px]">
                        {o.customerName || 'Direct'}
                      </td>
                      <td className="py-2 px-2 text-right text-[#8f9194]">
                        {o.items?.reduce((acc, i) => acc + (i.quantity || 0), 0) || 1}
                      </td>
                      <td className="py-2 px-2 text-right font-bold text-white">
                        {formatCurrency(Number(o.totalAmount) || 0)}
                      </td>
                      <td className="py-2 px-2 text-right">
                        <span className={`px-1.5 py-0.5 rounded text-[10px] ${
                          o.status === 'مكتمل' ? 'text-[#4edea3] bg-[#10b981]/15' : 'text-sky-400 bg-sky-500/15'
                        }`}>
                          {o.status}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
