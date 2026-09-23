import React, { useState, useEffect } from 'react';
import { useTenant, NavigationPage } from '../../context/TenantContext';

export const Sidebar: React.FC = () => {
  const {
    currentPage,
    setCurrentPage,
    orders,
    invoices,
    products,
    logoutSession,
    refreshAllData,
    isLoadingData,
    language,
  } = useTenant();

  const [utcTime, setUtcTime] = useState<string>('');
  const [refreshSuccess, setRefreshSuccess] = useState(false);

  useEffect(() => {
    const updateTime = () => {
      const now = new Date();
      setUtcTime(now.toTimeString().split(' ')[0]);
    };
    updateTime();
    const interval = setInterval(updateTime, 1000);
    return () => clearInterval(interval);
  }, []);

  const pendingOrdersCount = orders.filter(o => o.status === 'مسودة' || o.status === 'مؤكد').length;
  const unpaidInvoicesCount = invoices.filter(i => i.status === 'غير مدفوع' || i.status === 'متأخر').length;
  const lowStockCount = products.filter(p => p.status === 'مخزون منخفض' || p.status === 'نفد المخزون').length;

  const handleRefresh = async () => {
    await refreshAllData();
    setRefreshSuccess(true);
    setTimeout(() => setRefreshSuccess(false), 2000);
  };

  const navGroups = [
    {
      groupTitle: language === 'ar' ? 'العمليات الرئيسية' : 'Core Workspace',
      items: [
        {
          id: 'overview' as NavigationPage,
          label: language === 'ar' ? 'نظرة عامة تنفيذية' : 'Executive Overview',
          icon: 'dashboard',
          badge: undefined,
        },
        {
          id: 'terminal' as NavigationPage,
          label: language === 'ar' ? 'محطة التداول والتنفيذ' : 'Trading & Execution',
          icon: 'candlestick_chart',
          badge: undefined,
        },
        {
          id: 'orders' as NavigationPage,
          label: language === 'ar' ? 'سجل العمليات والأوامر' : 'Transactions & Orders',
          icon: 'receipt_long',
          badge: pendingOrdersCount > 0 ? pendingOrdersCount : undefined,
        },
        {
          id: 'inventory' as NavigationPage,
          label: language === 'ar' ? 'مخزون الأصول والمنتجات' : 'Asset Inventory',
          icon: 'token',
          badge: lowStockCount > 0 ? lowStockCount : undefined,
          badgeColor: 'amber',
        },
        {
          id: 'invoices' as NavigationPage,
          label: language === 'ar' ? 'الفواتير والتحصيل' : 'Billing & Invoices',
          icon: 'payments',
          badge: unpaidInvoicesCount > 0 ? unpaidInvoicesCount : undefined,
          badgeColor: 'rose',
        },
        {
          id: 'analytics' as NavigationPage,
          label: language === 'ar' ? 'تحليلات الأداء والمخاطر' : 'Performance Analytics',
          icon: 'insights',
          badge: undefined,
        },
      ],
    },
    {
      groupTitle: language === 'ar' ? 'الإدارة والخدمات اللوجستية' : 'Management & Logistics',
      items: [
        {
          id: 'customers' as NavigationPage,
          label: language === 'ar' ? 'الحسابات والعملاء' : 'Counterparties & Accounts',
          icon: 'lan',
          badge: undefined,
        },
        {
          id: 'warehouses' as NavigationPage,
          label: language === 'ar' ? 'المستودعات والمراكز' : 'Warehouses & Logistics',
          icon: 'warehouse',
          badge: undefined,
        },
      ],
    },
    {
      groupTitle: language === 'ar' ? 'النظام والامتثال' : 'Intelligence & System',
      items: [
        {
          id: 'settings' as NavigationPage,
          label: language === 'ar' ? 'إعدادات النظام' : 'System Settings',
          icon: 'settings',
          badge: undefined,
        },
      ],
    },
  ];

  return (
    <aside className={`fixed top-0 h-full w-72 bg-[#111316] z-50 flex flex-col justify-between select-none shadow-[0_1px_8px_rgba(0,0,0,0.5)] ${
      language === 'ar' ? 'right-0 border-l border-[#26292e]' : 'left-0 border-r border-[#26292e]'
    }`}>
      {/* Brand & Account Context */}
      <div className="flex flex-col min-h-0">
        <div className="px-4 py-3.5 bg-[#0c0e11]/90 border-b border-[#26292e]/80">
          <div className="flex items-center justify-between mb-2.5">
            {/* TradeFlow SVG Wordmark */}
            <div className="flex items-center gap-2 cursor-pointer" onClick={() => setCurrentPage('overview')}>
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 160 36" fill="none" className="h-8 w-auto">
                <rect x="2" y="4" width="28" height="28" rx="6" fill="#181A1E" stroke="#2E3238" strokeWidth="1.5" />
                <path d="M9 22L16 11L23 22" stroke="#EDEDEF" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round" />
                <path d="M12 18H20" stroke="#9CA3AF" strokeWidth="1.8" strokeLinecap="round" />
                <path d="M16 9V25" stroke="#10B981" strokeWidth="1.8" strokeLinecap="round" strokeDasharray="1 3" />
                <text x="38" y="23" fill="#EDEDEF" fontFamily="Inter, sans-serif" fontSize="16" fontWeight="700" letterSpacing="-0.03em">
                  Trade<tspan fill="#9CA3AF" fontWeight="400">Flow</tspan>
                </text>
              </svg>
            </div>
            <span className="px-1.5 py-0.5 rounded text-[10px] font-semibold tracking-wider uppercase bg-[#1e2023] text-[#4edea3] border border-[#4edea3]/20">
              PRIME
            </span>
          </div>

          <div className="flex items-center justify-between px-2.5 py-1.5 bg-[#1a1c1f] rounded border border-white/5">
            <div className="flex flex-col min-w-0 pr-2">
              <span className="text-xs text-[#e2e2e6] font-medium truncate">TradeFlow Global LLC</span>
              <div className="flex items-center gap-1.5 mt-0.5">
                <span className="w-1.5 h-1.5 rounded-full bg-[#10b981] animate-pulse"></span>
                <span className="text-[11px] text-[#8f9194]">Live Engine v2.4</span>
              </div>
            </div>
            <button
              onClick={handleRefresh}
              disabled={isLoadingData}
              title="Refresh System Data"
              className="p-1 rounded text-[#8f9194] hover:text-white hover:bg-[#282a2d] transition-colors"
            >
              <span className={`material-symbols-outlined text-sm ${isLoadingData ? 'animate-spin' : ''}`}>
                sync
              </span>
            </button>
          </div>
        </div>

        {/* Navigation Sections */}
        <nav className="flex-1 px-2.5 py-3 space-y-4 overflow-y-auto overflow-x-hidden">
          {navGroups.map((group, idx) => (
            <div key={idx} className="space-y-0.5">
              <div className="px-2.5 py-1 text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
                {group.groupTitle}
              </div>
              {group.items.map((item) => {
                const isActive = currentPage === item.id;
                return (
                  <button
                    key={item.id}
                    onClick={() => setCurrentPage(item.id)}
                    className={`w-full flex items-center justify-between px-2.5 py-2 rounded transition-all text-left ${
                      isActive
                        ? 'bg-[#282a2d] text-white border-l-2 border-[#4edea3] font-medium shadow-sm'
                        : 'text-[#c5c6c9] hover:bg-[#1a1c1f] hover:text-[#e2e2e6]'
                    }`}
                  >
                    <div className="flex items-center gap-2.5 min-w-0">
                      <span className={`material-symbols-outlined text-[19px] ${isActive ? 'text-[#4edea3]' : 'text-[#8f9194]'}`}>
                        {item.icon}
                      </span>
                      <span className="text-[13px] truncate">{item.label}</span>
                    </div>

                    {item.badge !== undefined && (
                      <span
                        className={`px-1.5 py-0.5 rounded-full text-[10px] font-bold tabular-nums ${
                          item.badgeColor === 'amber'
                            ? 'bg-amber-500/15 text-amber-400 border border-amber-500/30'
                            : item.badgeColor === 'rose'
                            ? 'bg-rose-500/15 text-rose-400 border border-rose-500/30'
                            : 'bg-[#4edea3]/15 text-[#4edea3] border border-[#4edea3]/30'
                        }`}
                      >
                        {item.badge}
                      </span>
                    )}
                  </button>
                );
              })}
            </div>
          ))}
        </nav>
      </div>

      {/* Footer System Status Strip */}
      <div className="p-3 bg-[#0c0e11]/95 border-t border-[#26292e] space-y-2">
        {refreshSuccess && (
          <div className="px-2 py-1 rounded bg-[#10b981]/15 border border-[#10b981]/30 text-[#4edea3] text-[11px] text-center">
            System data synchronized
          </div>
        )}

        <div className="flex items-center justify-between px-1 text-[11px] text-[#8f9194]">
          <div className="flex items-center gap-1.5">
            <span className="w-1.5 h-1.5 rounded-full bg-[#10b981]"></span>
            <span>REST API 12ms · Active</span>
          </div>
          <span className="font-mono text-[10px] text-[#c3c7cd]">UTC</span>
        </div>

        <div className="flex items-center justify-between px-1 text-[11px] text-[#8f9194]">
          <span className="font-mono text-white/80 tabular-nums">{utcTime}</span>
          <button
            onClick={logoutSession}
            className="flex items-center gap-1 text-[#8f9194] hover:text-rose-400 transition-colors"
          >
            <span className="material-symbols-outlined text-[15px]">logout</span>
            <span>{language === 'ar' ? 'خروج' : 'Sign Out'}</span>
          </button>
        </div>
      </div>
    </aside>
  );
};
