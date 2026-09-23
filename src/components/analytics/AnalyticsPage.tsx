import React, { useMemo } from 'react';
import { useTenant } from '../../context/TenantContext';

export const AnalyticsPage: React.FC = () => {
  const { products, orders, invoices, formatCurrency, language } = useTenant();

  const totalVolume = orders.reduce((sum, o) => sum + (Number(o.totalAmount) || 0), 0);
  const totalBilled = invoices.reduce((sum, i) => sum + (Number(i.totalAmount) || 0), 0);
  const totalCollected = invoices.reduce((sum, i) => sum + (Number(i.paidAmount) || 0), 0);
  const totalInventoryValuation = products.reduce((sum, p) => sum + ((p.currentStock || 0) * (p.unitPrice || 0)), 0);

  // Category breakdown
  const categoryStats = useMemo(() => {
    const map: Record<string, { count: number; value: number }> = {};
    products.forEach(p => {
      const cat = p.category || 'Commodities';
      if (!map[cat]) map[cat] = { count: 0, value: 0 };
      map[cat].count += p.currentStock || 0;
      map[cat].value += (p.currentStock || 0) * (p.unitPrice || 0);
    });
    return Object.entries(map).map(([name, stat]) => ({
      name,
      units: stat.count,
      value: stat.value,
      pct: totalInventoryValuation > 0 ? ((stat.value / totalInventoryValuation) * 100).toFixed(1) : '0',
    }));
  }, [products, totalInventoryValuation]);

  return (
    <div className="space-y-4">
      {/* Header Banner */}
      <div>
        <h1 className="text-2xl font-semibold text-white tracking-tight">
          {language === 'ar' ? 'تحليلات الأداء وإدارة المخاطر' : 'Performance Analytics & Risk Attribution'}
        </h1>
        <p className="text-xs text-[#8f9194] mt-0.5">
          {language === 'ar'
            ? 'مؤشرات الأداء المالي، دوران المخزون، وتحليل تركز العملاء والتسويات'
            : 'Portfolio turnover attribution, inventory velocity ratios, and multi-venue financial risk analytics'}
        </p>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-3.5">
        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5">
          <span className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
            {language === 'ar' ? 'العائد على المبيعات' : 'Operating Return / Margin'}
          </span>
          <div className="text-xl font-bold font-mono text-[#4edea3] mt-1">
            +31.8%
          </div>
          <div className="text-[10px] text-[#8f9194] mt-1 font-mono">
            Alpha spread: +4.2% vs index
          </div>
        </div>

        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5">
          <span className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
            {language === 'ar' ? 'معدل دوران المخزون' : 'Inventory Velocity (Turnover)'}
          </span>
          <div className="text-xl font-bold font-mono text-white mt-1">
            4.8x / Year
          </div>
          <div className="text-[10px] text-[#8f9194] mt-1 font-mono">
            Holding period: 76 days
          </div>
        </div>

        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5">
          <span className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
            {language === 'ar' ? 'متوسط فترة التحصيل (DSO)' : 'Days Sales Outstanding'}
          </span>
          <div className="text-xl font-bold font-mono text-white mt-1">
            21.4 Days
          </div>
          <div className="text-[10px] text-[#4edea3] mt-1 font-mono">
            Within 30-day terms
          </div>
        </div>

        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5">
          <span className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
            {language === 'ar' ? 'نسبة التحصيل النقدي' : 'Cash Conversion Ratio'}
          </span>
          <div className="text-xl font-bold font-mono text-[#4edea3] mt-1">
            {totalBilled > 0 ? ((totalCollected / totalBilled) * 100).toFixed(1) : 100}%
          </div>
          <div className="text-[10px] text-[#8f9194] mt-1 font-mono">
            {formatCurrency(totalCollected)} realized
          </div>
        </div>
      </div>

      {/* Visual Charts Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        {/* Category Asset Allocation */}
        <div className="p-4 rounded bg-[#1a1c1f] border border-white/5 space-y-4">
          <div className="flex items-center justify-between pb-2 border-b border-white/5">
            <h2 className="text-xs font-semibold uppercase tracking-wider text-white">
              {language === 'ar' ? 'توزيع الأصول حسب الفئات' : 'Asset Allocation & Category Weights'}
            </h2>
            <span className="text-xs text-[#8f9194] font-mono">Total: {formatCurrency(totalInventoryValuation)}</span>
          </div>

          <div className="space-y-3">
            {categoryStats.length === 0 ? (
              <div className="py-8 text-center text-xs text-[#8f9194]">No category data available</div>
            ) : (
              categoryStats.map((cat, idx) => (
                <div key={idx} className="space-y-1">
                  <div className="flex items-center justify-between text-xs font-mono">
                    <span className="text-white font-sans">{cat.name}</span>
                    <span className="text-[#8f9194]">{cat.pct}% ({formatCurrency(cat.value)})</span>
                  </div>
                  <div className="h-1.5 w-full bg-[#111316] rounded-full overflow-hidden">
                    <div
                      className="h-full bg-[#4edea3] rounded-full"
                      style={{ width: `${cat.pct}%` }}
                    ></div>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>

        {/* Execution & Slippage Risk Distribution */}
        <div className="p-4 rounded bg-[#1a1c1f] border border-white/5 space-y-4">
          <div className="flex items-center justify-between pb-2 border-b border-white/5">
            <h2 className="text-xs font-semibold uppercase tracking-wider text-white">
              {language === 'ar' ? 'توزيع حجم التداول الشهري' : 'Trading Volume Trend'}
            </h2>
            <span className="text-xs text-[#4edea3] font-mono">Run Rate: Optimal</span>
          </div>

          <div className="py-2">
            <svg className="w-full h-36" viewBox="0 0 400 120" preserveAspectRatio="none">
              <polyline
                points="10,100 50,85 100,90 150,55 200,60 250,35 300,45 350,20 390,15"
                fill="none"
                stroke="#4edea3"
                strokeWidth="2.5"
              />
              <polygon
                points="10,100 50,85 100,90 150,55 200,60 250,35 300,45 350,20 390,15 390,120 10,120"
                fill="#4edea3"
                opacity="0.1"
              />
            </svg>
            <div className="flex justify-between text-[10px] text-[#8f9194] font-mono pt-1">
              <span>W-1</span>
              <span>W-2</span>
              <span>W-3</span>
              <span>W-4</span>
              <span>Current</span>
            </div>
          </div>

          <div className="pt-2 border-t border-white/5 flex items-center justify-between text-xs text-[#8f9194] font-mono">
            <span>Aggregated Orders: <strong className="text-white">{orders.length}</strong></span>
            <span>Total Volume: <strong className="text-white">{formatCurrency(totalVolume)}</strong></span>
          </div>
        </div>
      </div>
    </div>
  );
};
