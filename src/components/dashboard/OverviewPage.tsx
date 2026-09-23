import React, { useState } from 'react';
import { useTenant } from '../../context/TenantContext';

interface OverviewPageProps {
  onOpenCreateOrder: () => void;
}

export const OverviewPage: React.FC<OverviewPageProps> = ({ onOpenCreateOrder }) => {
  const {
    orders,
    invoices,
    products,
    customers,
    warehouses,
    formatCurrency,
    activeTimeframe,
    setActiveTimeframe,
    setCurrentPage,
    language,
    refreshAllData,
    isLoadingData,
  } = useTenant();

  const [benchmarkTickers] = useState([
    { symbol: 'SPX', price: '5,088.80', change: '+0.42%', up: true },
    { symbol: 'NDX', price: '18,120.30', change: '+0.68%', up: true },
    { symbol: 'EUR/USD', price: '1.0842', change: '-0.12%', up: false },
    { symbol: 'XAU/USD', price: '$2,185.40', change: '+0.85%', up: true },
  ]);

  // Real calculations
  const totalRevenue = invoices.reduce((sum, inv) => sum + (Number(inv.totalAmount) || 0), 0) +
    orders.filter(o => o.status === 'مكتمل' || o.status === 'مؤكد').reduce((sum, o) => sum + (Number(o.totalAmount) || 0), 0);

  const totalCost = products.reduce((sum, p) => sum + ((p.currentStock || 0) * (p.costPrice || 0)), 0);
  const totalInventoryValuation = products.reduce((sum, p) => sum + ((p.currentStock || 0) * (p.unitPrice || 0)), 0);

  const netRealizedMargin = Math.max(0, totalRevenue * 0.32); // 32% margin estimate on volume
  const operatingYieldPct = totalRevenue > 0 ? ((netRealizedMargin / totalRevenue) * 100).toFixed(1) : '31.4';

  const outstandingReceivables = invoices.reduce((sum, inv) => sum + (Number(inv.balanceDue) || 0), 0);
  const collectedRevenue = invoices.reduce((sum, inv) => sum + (Number(inv.paidAmount) || 0), 0);
  const collectionRate = totalRevenue > 0 ? Math.min(100, Math.round((collectedRevenue / (totalRevenue || 1)) * 100)) : 100;

  const lowStockCount = products.filter(p => p.status === 'مخزون منخفض' || p.status === 'نفد المخزون').length;
  const recentOrders = orders.slice(0, 7);

  const handleExport = () => {
    const csvContent = "data:text/csv;charset=utf-8," +
      ["OrderNumber,Customer,Warehouse,Total,Status",
        ...orders.map(o => `"${o.orderNumber}","${o.customerName || ''}","${o.warehouseName || ''}",${o.totalAmount},"${o.status}"`)
      ].join("\n");
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement("a");
    link.setAttribute("href", encodedUri);
    link.setAttribute("download", `tradeflow_report_${new Date().toISOString().slice(0,10)}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <div className="space-y-6">
      {/* Top Status Ribbon & Global Executive Controls */}
      <div className="flex flex-col xl:flex-row xl:items-center justify-between gap-4 pb-1">
        <div className="flex flex-col sm:flex-row sm:items-center gap-4">
          <div>
            <h1 className="text-2xl font-semibold text-white tracking-tight">
              {language === 'ar' ? 'نظرة عامة تنفيذية' : 'Executive Overview'}
            </h1>
            <p className="text-xs text-[#8f9194] mt-0.5">
              {language === 'ar'
                ? 'المحفظة الاستثمارية الموحدة · تجميع FIX 4.4 فوري'
                : 'Global consolidated prime book · Real-time FIX 4.4 aggregation'}
            </p>
          </div>

          {/* Live Benchmark Tickers */}
          <div className="flex flex-wrap items-center gap-1.5 pt-1 sm:pt-0">
            {benchmarkTickers.map(t => (
              <div key={t.symbol} className="flex items-center gap-1.5 px-2.5 py-1 rounded bg-[#1a1c1f] border border-white/5">
                <span className="text-[10px] font-semibold text-[#8f9194] uppercase tracking-wider">{t.symbol}</span>
                <span className="text-xs text-[#e2e2e6] font-mono font-medium">{t.price}</span>
                <span className={`text-[10px] font-semibold font-mono ${t.up ? 'text-[#4edea3]' : 'text-rose-400'}`}>
                  {t.change}
                </span>
              </div>
            ))}
          </div>
        </div>

        {/* Actions & Timeframe Filter */}
        <div className="flex flex-wrap items-center gap-2">
          <div className="flex items-center p-0.5 rounded bg-[#1a1c1f] border border-white/5">
            {(['Today', '7D', '30D', 'YTD'] as const).map(tf => (
              <button
                key={tf}
                onClick={() => setActiveTimeframe(tf)}
                className={`px-2.5 py-1 rounded text-[11px] font-semibold transition-colors ${
                  activeTimeframe === tf
                    ? 'bg-[#333538] text-white'
                    : 'text-[#8f9194] hover:text-white'
                }`}
              >
                {tf}
              </button>
            ))}
          </div>

          <button
            onClick={handleExport}
            className="flex items-center gap-1.5 px-3 py-1.5 rounded bg-[#282a2d] hover:bg-[#37393d] text-[#e2e2e6] text-xs transition-colors shadow-sm cursor-pointer"
          >
            <span className="material-symbols-outlined text-sm text-[#8f9194]">download</span>
            <span>{language === 'ar' ? 'تصدير التقرير' : 'Export Report'}</span>
          </button>

          <button
            onClick={refreshAllData}
            disabled={isLoadingData}
            className="flex items-center gap-1.5 px-3 py-1.5 rounded bg-[#282a2d] hover:bg-[#37393d] text-[#e2e2e6] text-xs transition-colors shadow-sm cursor-pointer"
          >
            <span className={`w-1.5 h-1.5 rounded-full bg-[#4edea3] ${isLoadingData ? 'animate-spin' : 'animate-pulse'}`}></span>
            <span>{isLoadingData ? 'Syncing...' : 'Feed Live'}</span>
          </button>
        </div>
      </div>

      {/* 4 High-Density Executive KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-3.5">
        {/* KPI 1: Total Revenue / Volume */}
        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5 shadow-sm flex flex-col justify-between">
          <div className="flex items-center justify-between">
            <span className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
              {language === 'ar' ? 'إجمالي الإيرادات / الحجم' : 'Total Revenue / Volume'}
            </span>
            <span className="px-1.5 py-0.5 rounded-full bg-[#10b981]/15 text-[#4edea3] text-[11px] font-semibold">
              +14.2%
            </span>
          </div>
          <div className="my-2.5 flex items-end justify-between">
            <div>
              <div className="text-2xl font-bold text-white tracking-tight font-mono">
                {formatCurrency(totalRevenue)}
              </div>
              <div className="text-[11px] text-[#8f9194] mt-0.5">
                {language === 'ar' ? 'معدل التشغيل اليومي' : 'Daily run-rate'}: {formatCurrency(totalRevenue / 30)}
              </div>
            </div>
            {/* Sparkline chart */}
            <svg className="w-20 h-9 shrink-0" fill="none" viewBox="0 0 80 36">
              <path d="M1 32 C12 28, 20 30, 32 18 C44 8, 54 22, 65 10 C72 4, 76 3, 79 2" stroke="#4edea3" strokeLinecap="round" strokeWidth="1.5" />
              <path d="M1 32 C12 28, 20 30, 32 18 C44 8, 54 22, 65 10 C72 4, 76 3, 79 2 L79 36 L1 36 Z" fill="#4edea3" opacity="0.12" />
            </svg>
          </div>
          <div className="pt-1 border-t border-white/5 text-[#8f9194] text-[11px] flex items-center justify-between">
            <span>{language === 'ar' ? 'تحقيق المستهدف' : 'Target attainment'}</span>
            <span className="text-[#e2e2e6] font-medium">104.2% of target</span>
          </div>
        </div>

        {/* KPI 2: Net Realized Margin */}
        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5 shadow-sm flex flex-col justify-between">
          <div className="flex items-center justify-between">
            <span className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
              {language === 'ar' ? 'صافي الهامش المحقق' : 'Net Realized Margin'}
            </span>
            <span className="px-1.5 py-0.5 rounded-full bg-[#10b981]/15 text-[#4edea3] text-[11px] font-semibold">
              +8.6%
            </span>
          </div>
          <div className="my-2.5 flex items-end justify-between">
            <div>
              <div className="text-2xl font-bold text-white tracking-tight font-mono">
                {formatCurrency(netRealizedMargin)}
              </div>
              <div className="text-[11px] text-[#8f9194] mt-0.5">
                {language === 'ar' ? 'العائد التشغيلي' : 'Operating yield'}: {operatingYieldPct}%
              </div>
            </div>
            <div className="w-20 flex flex-col gap-1 pb-1">
              <div className="h-1.5 w-full bg-[#333538] rounded-full overflow-hidden">
                <div className="h-full bg-[#4edea3] rounded-full" style={{ width: '74%' }}></div>
              </div>
              <span className="text-[10px] text-right font-mono text-[#8f9194]">74% optimal</span>
            </div>
          </div>
          <div className="pt-1 border-t border-white/5 text-[#8f9194] text-[11px] flex items-center justify-between">
            <span>{language === 'ar' ? 'الرسوم وتكلفة التشغيل' : 'Estimated Cost'}</span>
            <span className="text-[#e2e2e6] font-medium font-mono">{formatCurrency(totalCost)}</span>
          </div>
        </div>

        {/* KPI 3: Active Inventory Valuation */}
        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5 shadow-sm flex flex-col justify-between">
          <div className="flex items-center justify-between">
            <span className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
              {language === 'ar' ? 'تقييم مخزون الأصول' : 'Asset Inventory Valuation'}
            </span>
            <span className="px-1.5 py-0.5 rounded-full bg-[#10b981]/15 text-[#4edea3] text-[11px] font-semibold">
              {products.length} SKUs
            </span>
          </div>
          <div className="my-2.5 flex items-end justify-between">
            <div>
              <div className="text-2xl font-bold text-white tracking-tight font-mono">
                {formatCurrency(totalInventoryValuation)}
              </div>
              <div className="text-[11px] text-[#8f9194] mt-0.5">
                {language === 'ar' ? 'مراكز التخزين' : 'Warehouses'}: {warehouses.length} locations
              </div>
            </div>
            <div className="flex flex-col items-end">
              <span className="text-[11px] font-mono text-[#4edea3]">
                {products.reduce((acc, p) => acc + (p.currentStock || 0), 0)} Units
              </span>
              <span className="text-[10px] text-[#8f9194]">In active stock</span>
            </div>
          </div>
          <div className="pt-1 border-t border-white/5 text-[#8f9194] text-[11px] flex items-center justify-between">
            <span>{language === 'ar' ? 'أصناف منخفضة' : 'Reorder Alerts'}</span>
            <span className={lowStockCount > 0 ? 'text-amber-400 font-semibold' : 'text-[#e2e2e6]'}>
              {lowStockCount > 0 ? `${lowStockCount} items low` : 'All stocks healthy'}
            </span>
          </div>
        </div>

        {/* KPI 4: Outstanding Receivables */}
        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5 shadow-sm flex flex-col justify-between">
          <div className="flex items-center justify-between">
            <span className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
              {language === 'ar' ? 'المستحقات غير المحصلة' : 'Outstanding Receivables'}
            </span>
            <span className="px-1.5 py-0.5 rounded-full bg-[#333538] text-[#c5c6c9] text-[11px] font-semibold">
              {invoices.filter(i => i.status === 'غير مدفوع' || i.status === 'متأخر').length} open
            </span>
          </div>
          <div className="my-2.5 flex items-end justify-between">
            <div>
              <div className="text-2xl font-bold text-white tracking-tight font-mono">
                {formatCurrency(outstandingReceivables)}
              </div>
              <div className="text-[11px] text-[#8f9194] mt-0.5">
                {language === 'ar' ? 'نسبة التحصيل' : 'Collection rate'}: {collectionRate}%
              </div>
            </div>
            <div className="w-20 flex flex-col gap-1 pb-1">
              <div className="h-1.5 w-full bg-[#333538] rounded-full overflow-hidden">
                <div
                  className="h-full bg-[#4edea3] rounded-full"
                  style={{ width: `${Math.min(100, collectionRate)}%` }}
                ></div>
              </div>
              <span className="text-[10px] text-right font-mono text-[#8f9194]">Settlement status</span>
            </div>
          </div>
          <div className="pt-1 border-t border-white/5 text-[#8f9194] text-[11px] flex items-center justify-between">
            <span>{language === 'ar' ? 'إجمالي المحصل' : 'Collected Settle'}</span>
            <span className="text-[#4edea3] font-medium font-mono">{formatCurrency(collectedRevenue)}</span>
          </div>
        </div>
      </div>

      {/* Main Grid: Performance Chart & Quick Desk Triggers */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        {/* Left 2 Cols: Execution Volume Aggregation */}
        <div className="lg:col-span-2 p-4 rounded bg-[#1a1c1f] border border-white/5 flex flex-col justify-between">
          <div className="flex items-center justify-between pb-3 border-b border-white/5">
            <div>
              <h2 className="text-sm font-semibold text-white">
                {language === 'ar' ? 'حجم التنفيذ والعمليات الموحدة' : 'Consolidated Trading & Order Volume'}
              </h2>
              <p className="text-[11px] text-[#8f9194]">
                Aggregated institutional turnover across desks and client accounts
              </p>
            </div>
            <div className="flex items-center gap-2">
              <div className="flex items-center gap-1.5 text-xs text-[#8f9194]">
                <span className="w-2 h-2 rounded-full bg-[#4edea3]"></span>
                <span>Fulfilled</span>
              </div>
              <div className="flex items-center gap-1.5 text-xs text-[#8f9194]">
                <span className="w-2 h-2 rounded-full bg-[#333538]"></span>
                <span>Active Book</span>
              </div>
            </div>
          </div>

          {/* High-Precision SVG Chart Graphic */}
          <div className="py-4">
            <svg className="w-full h-44" viewBox="0 0 600 160" preserveAspectRatio="none">
              <defs>
                <linearGradient id="volGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="#4edea3" stopOpacity="0.25" />
                  <stop offset="100%" stopColor="#4edea3" stopOpacity="0" />
                </linearGradient>
              </defs>
              {/* Grid lines */}
              <line x1="0" y1="40" x2="600" y2="40" stroke="#26292e" strokeDasharray="3 3" />
              <line x1="0" y1="80" x2="600" y2="80" stroke="#26292e" strokeDasharray="3 3" />
              <line x1="0" y1="120" x2="600" y2="120" stroke="#26292e" strokeDasharray="3 3" />
              {/* Area */}
              <polygon
                points="0,140 50,110 100,125 150,85 200,95 250,60 300,75 350,45 400,65 450,30 500,40 550,20 600,15 600,160 0,160"
                fill="url(#volGrad)"
              />
              {/* Line */}
              <polyline
                points="0,140 50,110 100,125 150,85 200,95 250,60 300,75 350,45 400,65 450,30 500,40 550,20 600,15"
                fill="none"
                stroke="#4edea3"
                strokeWidth="2"
              />
              {/* Data points */}
              <circle cx="350" cy="45" r="3.5" fill="#ffffff" stroke="#4edea3" strokeWidth="2" />
              <circle cx="550" cy="20" r="3.5" fill="#ffffff" stroke="#4edea3" strokeWidth="2" />
            </svg>
            <div className="flex items-center justify-between text-[10px] text-[#8f9194] font-mono pt-1">
              <span>08:00 UTC</span>
              <span>10:00</span>
              <span>12:00</span>
              <span>14:00</span>
              <span>16:00</span>
              <span>18:00</span>
              <span>20:00 UTC</span>
            </div>
          </div>

          <div className="pt-3 border-t border-white/5 flex items-center justify-between text-xs text-[#8f9194]">
            <span>Average Trade Value: <strong className="text-white font-mono">{formatCurrency(orders.length > 0 ? totalRevenue / orders.length : 0)}</strong></span>
            <span>Total Orders Logged: <strong className="text-white font-mono">{orders.length}</strong></span>
          </div>
        </div>

        {/* Right Col: Instant Desks & Controls */}
        <div className="p-4 rounded bg-[#1a1c1f] border border-white/5 flex flex-col justify-between space-y-4">
          <div>
            <h2 className="text-sm font-semibold text-white">
              {language === 'ar' ? 'محطات الإجراءات الفورية' : 'Fast-Track Operations'}
            </h2>
            <p className="text-[11px] text-[#8f9194] mt-0.5">
              Direct access to live execution desks
            </p>
          </div>

          <div className="space-y-2">
            <button
              onClick={() => setCurrentPage('terminal')}
              className="w-full flex items-center justify-between p-2.5 rounded bg-[#282a2d] hover:bg-[#333538] border border-white/5 text-left transition-colors group cursor-pointer"
            >
              <div className="flex items-center gap-2.5">
                <span className="material-symbols-outlined text-[#4edea3] text-lg">candlestick_chart</span>
                <div>
                  <div className="text-xs font-semibold text-white group-hover:text-[#4edea3] transition-colors">
                    {language === 'ar' ? 'محطة التداول والتنفيذ' : 'Trading Terminal'}
                  </div>
                  <div className="text-[11px] text-[#8f9194]">High-speed order entry & book</div>
                </div>
              </div>
              <span className="material-symbols-outlined text-[#8f9194] text-sm group-hover:translate-x-1 transition-transform">
                arrow_forward
              </span>
            </button>

            <button
              onClick={onOpenCreateOrder}
              className="w-full flex items-center justify-between p-2.5 rounded bg-[#282a2d] hover:bg-[#333538] border border-white/5 text-left transition-colors group cursor-pointer"
            >
              <div className="flex items-center gap-2.5">
                <span className="material-symbols-outlined text-white text-lg">add_shopping_cart</span>
                <div>
                  <div className="text-xs font-semibold text-white group-hover:text-white transition-colors">
                    {language === 'ar' ? 'تسجيل أمر بيع' : 'New Sales Order'}
                  </div>
                  <div className="text-[11px] text-[#8f9194]">Create customer allocation & invoice</div>
                </div>
              </div>
              <span className="material-symbols-outlined text-[#8f9194] text-sm group-hover:translate-x-1 transition-transform">
                arrow_forward
              </span>
            </button>

            <button
              onClick={() => setCurrentPage('inventory')}
              className="w-full flex items-center justify-between p-2.5 rounded bg-[#282a2d] hover:bg-[#333538] border border-white/5 text-left transition-colors group cursor-pointer"
            >
              <div className="flex items-center gap-2.5">
                <span className="material-symbols-outlined text-amber-400 text-lg">swap_horiz</span>
                <div>
                  <div className="text-xs font-semibold text-white group-hover:text-amber-400 transition-colors">
                    {language === 'ar' ? 'توريد ونقل المخزون' : 'Stock Operations'}
                  </div>
                  <div className="text-[11px] text-[#8f9194]">Receive goods & warehouse transfer</div>
                </div>
              </div>
              <span className="material-symbols-outlined text-[#8f9194] text-sm group-hover:translate-x-1 transition-transform">
                arrow_forward
              </span>
            </button>

            <button
              onClick={() => setCurrentPage('customers')}
              className="w-full flex items-center justify-between p-2.5 rounded bg-[#282a2d] hover:bg-[#333538] border border-white/5 text-left transition-colors group cursor-pointer"
            >
              <div className="flex items-center gap-2.5">
                <span className="material-symbols-outlined text-sky-400 text-lg">person_add</span>
                <div>
                  <div className="text-xs font-semibold text-white group-hover:text-sky-400 transition-colors">
                    {language === 'ar' ? 'حسابات العملاء والائتمان' : 'Counterparty Ledger'}
                  </div>
                  <div className="text-[11px] text-[#8f9194]">Credit limits and account balances</div>
                </div>
              </div>
              <span className="material-symbols-outlined text-[#8f9194] text-sm group-hover:translate-x-1 transition-transform">
                arrow_forward
              </span>
            </button>
          </div>

          <div className="pt-2 border-t border-white/5 flex items-center justify-between text-[11px] text-[#8f9194]">
            <span>System Version</span>
            <span className="font-mono text-[#e2e2e6]">v2.4.0 Prime</span>
          </div>
        </div>
      </div>

      {/* Live Transaction Ledger Stream (Recent Orders & Invoices) */}
      <div className="p-4 rounded bg-[#1a1c1f] border border-white/5">
        <div className="flex items-center justify-between pb-3 mb-3 border-b border-white/5">
          <div className="flex items-center gap-2">
            <h2 className="text-sm font-semibold text-white">
              {language === 'ar' ? 'سجل العمليات الأخير' : 'Live Execution Stream'}
            </h2>
            <span className="px-1.5 py-0.5 rounded text-[10px] font-bold bg-[#282a2d] text-[#4edea3] font-mono">
              LIVE
            </span>
          </div>
          <button
            onClick={() => setCurrentPage('orders')}
            className="text-xs text-[#8f9194] hover:text-[#4edea3] transition-colors flex items-center gap-1 cursor-pointer"
          >
            <span>{language === 'ar' ? 'عرض السجل كاملاً' : 'View Full Ledger'}</span>
            <span className="material-symbols-outlined text-xs">arrow_forward</span>
          </button>
        </div>

        {recentOrders.length === 0 ? (
          <div className="py-8 text-center text-xs text-[#8f9194]">
            {language === 'ar' ? 'لا توجد أوامر مسجلة حتى الآن' : 'No transactions recorded yet'}
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider border-b border-white/5">
                  <th className="py-2 px-3">Order ID</th>
                  <th className="py-2 px-3">Customer / Counterparty</th>
                  <th className="py-2 px-3">Warehouse</th>
                  <th className="py-2 px-3 text-right">Items</th>
                  <th className="py-2 px-3 text-right">Amount</th>
                  <th className="py-2 px-3">Status</th>
                  <th className="py-2 px-3 text-center">Action</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/5 text-xs font-mono">
                {recentOrders.map(order => (
                  <tr key={order.id} className="hover:bg-[#282a2d]/50 transition-colors">
                    <td className="py-2.5 px-3 font-semibold text-white">
                      {order.orderNumber}
                    </td>
                    <td className="py-2.5 px-3 font-sans text-[#e2e2e6]">
                      {order.customerName || 'Direct Counterparty'}
                    </td>
                    <td className="py-2.5 px-3 font-sans text-[#8f9194]">
                      {order.warehouseName || 'Central Hub'}
                    </td>
                    <td className="py-2.5 px-3 text-right text-[#8f9194]">
                      {order.items?.length || 1}
                    </td>
                    <td className="py-2.5 px-3 text-right font-bold text-white">
                      {formatCurrency(Number(order.totalAmount) || 0)}
                    </td>
                    <td className="py-2.5 px-3 font-sans">
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
                    <td className="py-2.5 px-3 text-center">
                      <button
                        onClick={() => setCurrentPage('orders')}
                        className="px-2 py-1 rounded bg-[#282a2d] hover:bg-[#37393d] text-[#e2e2e6] text-[11px] font-sans transition-colors cursor-pointer"
                      >
                        Inspect
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};
