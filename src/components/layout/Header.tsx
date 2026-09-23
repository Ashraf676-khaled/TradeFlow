import React, { useState, useRef, useEffect } from 'react';
import { useTenant } from '../../context/TenantContext';
import { BrandLogo } from './BrandLogo';

interface HeaderProps {
  onOpenNewOrder?: () => void;
  onToggleMobileSidebar?: () => void;
}

export const Header: React.FC<HeaderProps> = ({ onOpenNewOrder, onToggleMobileSidebar }) => {
  const {
    userSession,
    logoutSession,
    currentPage,
    setCurrentPage,
    activeCurrency,
    setActiveCurrency,
    language,
    setLanguage,
    setOpenQuickOrderModal,
    products,
    orders,
    customers,
  } = useTenant();

  const [isProfileOpen, setIsProfileOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [isSearchFocused, setIsSearchFocused] = useState(false);
  const profileRef = useRef<HTMLDivElement>(null);
  const searchRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (profileRef.current && !profileRef.current.contains(e.target as Node)) {
        setIsProfileOpen(false);
      }
      if (searchRef.current && !searchRef.current.contains(e.target as Node)) {
        setIsSearchFocused(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const pageTitles: Record<string, { en: string; ar: string; sectionEn: string; sectionAr: string }> = {
    overview: { en: 'Executive Overview', ar: 'نظرة عامة تنفيذية', sectionEn: 'Prime Portfolio', sectionAr: 'المحفظة الرئيسية' },
    terminal: { en: 'Trading & Execution', ar: 'محطة التداول والتنفيذ', sectionEn: 'Market Desks', sectionAr: 'مكاتب التداول' },
    orders: { en: 'Transactions & Orders', ar: 'سجل العمليات والأوامر', sectionEn: 'Ledger', sectionAr: 'دفتر الأستاذ' },
    inventory: { en: 'Asset Inventory', ar: 'مخزون الأصول', sectionEn: 'Supply Chain', sectionAr: 'سلاسل الإمداد' },
    invoices: { en: 'Billing & Invoices', ar: 'الفواتير والتحصيل', sectionEn: 'Financial Ops', sectionAr: 'العمليات المالية' },
    analytics: { en: 'Performance Analytics', ar: 'تحليلات الأداء والمخاطر', sectionEn: 'Intelligence', sectionAr: 'ذكاء الأعمال' },
    customers: { en: 'Counterparties & Accounts', ar: 'الحسابات والعملاء', sectionEn: 'CRM & Credit', sectionAr: 'العملاء والائتمان' },
    warehouses: { en: 'Warehouses & Logistics', ar: 'المستودعات والخدمات اللوجستية', sectionEn: 'Facilities', sectionAr: 'المراكز اللوجستية' },
    settings: { en: 'System Settings', ar: 'إعدادات النظام', sectionEn: 'Configuration', sectionAr: 'التهيئة العامة' },
  };

  const pageInfo = pageTitles[currentPage] || pageTitles.overview;

  // Filter items for quick search popover
  const filteredProducts = searchQuery.trim()
    ? products.filter(p =>
        (p.name && p.name.toLowerCase().includes(searchQuery.toLowerCase())) ||
        (p.sku && p.sku.toLowerCase().includes(searchQuery.toLowerCase()))
      ).slice(0, 3)
    : [];

  const filteredOrders = searchQuery.trim()
    ? orders.filter(o =>
        (o.orderNumber && o.orderNumber.toLowerCase().includes(searchQuery.toLowerCase())) ||
        (o.customerName && o.customerName.toLowerCase().includes(searchQuery.toLowerCase()))
      ).slice(0, 3)
    : [];

  const isAr = language === 'ar';

  return (
    <header className="sticky top-0 z-40 h-16 w-full bg-[#111316]/95 backdrop-blur-xl border-b border-[#26292e] shadow-[0_1px_8px_rgba(0,0,0,0.4)]">
      <div className="h-16 w-full px-3 sm:px-4 md:px-6 flex items-center justify-between gap-3 md:gap-4" dir={isAr ? 'rtl' : 'ltr'}>
        {/* Start / Left: Mobile toggle + BrandLogo or Breadcrumbs */}
        <div className="flex items-center gap-3 min-w-0">
          {/* Mobile Hamburger Button */}
          {onToggleMobileSidebar && (
            <button
              onClick={onToggleMobileSidebar}
              className="lg:hidden p-1.5 rounded-lg bg-[#1a1c1f] text-[#8f9194] hover:text-white border border-white/5"
              title={isAr ? 'فتح القائمة الرئيسية' : 'Toggle navigation'}
            >
              <span className="material-symbols-outlined text-xl">menu</span>
            </button>
          )}

          {/* Mobile Brand Logo */}
          <div className="lg:hidden">
            <BrandLogo
              language={language}
              size="sm"
              showBadge={false}
              showSubtitle={false}
              onClick={() => setCurrentPage('overview')}
            />
          </div>

          {/* Desktop Breadcrumbs */}
          <div className="hidden sm:flex items-center gap-2 text-xs text-[#8f9194] whitespace-nowrap uppercase tracking-wider">
            <span
              onClick={() => setCurrentPage('overview')}
              className="hover:text-[#4edea3] cursor-pointer transition-colors font-bold text-white flex items-center gap-1"
            >
              <span className="w-1.5 h-1.5 rounded-full bg-[#10b981]" />
              TradeFlow
            </span>
            <span className="text-[#55585f]">/</span>
            <span className="hover:text-white cursor-pointer transition-colors">
              {isAr ? pageInfo.sectionAr : pageInfo.sectionEn}
            </span>
            <span className="text-[#55585f]">/</span>
            <span className="text-[#e2e2e6] font-semibold">
              {isAr ? pageInfo.ar : pageInfo.en}
            </span>
          </div>

          {/* Quick Search Bar */}
          <div ref={searchRef} className="relative hidden xl:block w-80">
            <div className="flex items-center gap-2 px-3 py-1.5 bg-[#1a1c1f] rounded-lg border border-white/5 text-[#8f9194] focus-within:border-[#4edea3]/50 focus-within:text-[#e2e2e6] transition-all">
              <span className="material-symbols-outlined text-base">search</span>
              <input
                type="text"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onFocus={() => setIsSearchFocused(true)}
                placeholder={isAr ? 'بحث سريع عن الأصناف، العملاء، أو الفواتير...' : 'Search tickers, orders, accounts...'}
                className="bg-transparent border-none outline-none text-xs text-[#e2e2e6] placeholder-[#8f9194] w-full"
              />
              <kbd className="text-[10px] px-1.5 py-0.5 bg-[#282a2d] rounded text-[#8f9194] font-mono">⌘K</kbd>
            </div>

            {/* Quick search dropdown */}
            {isSearchFocused && searchQuery.trim() && (
              <div className="absolute left-0 right-0 mt-1 bg-[#1a1c1f] border border-[#26292e] rounded-lg shadow-xl p-2 z-50 space-y-2 max-h-80 overflow-y-auto">
                {filteredProducts.length > 0 && (
                  <div>
                    <div className="text-[10px] font-semibold text-[#8f9194] uppercase px-1.5 py-0.5">
                      {isAr ? 'الأصناف والمخزون' : 'Products'}
                    </div>
                    {filteredProducts.map(p => (
                      <div
                        key={p.id}
                        onClick={() => {
                          setCurrentPage('inventory');
                          setIsSearchFocused(false);
                          setSearchQuery('');
                        }}
                        className="px-2 py-1.5 hover:bg-[#282a2d] rounded cursor-pointer text-xs flex justify-between items-center"
                      >
                        <span className="text-[#e2e2e6] truncate pr-2">{p.name}</span>
                        <span className="text-[#4edea3] font-mono text-[11px] shrink-0">{p.sku}</span>
                      </div>
                    ))}
                  </div>
                )}

                {filteredOrders.length > 0 && (
                  <div>
                    <div className="text-[10px] font-semibold text-[#8f9194] uppercase px-1.5 py-0.5">
                      {isAr ? 'أوامر البيع' : 'Orders'}
                    </div>
                    {filteredOrders.map(o => (
                      <div
                        key={o.id}
                        onClick={() => {
                          setCurrentPage('orders');
                          setIsSearchFocused(false);
                          setSearchQuery('');
                        }}
                        className="px-2 py-1.5 hover:bg-[#282a2d] rounded cursor-pointer text-xs flex justify-between items-center"
                      >
                        <span className="text-[#e2e2e6]">{o.orderNumber}</span>
                        <span className="text-[#8f9194] truncate">{o.customerName}</span>
                      </div>
                    ))}
                  </div>
                )}

                {filteredProducts.length === 0 && filteredOrders.length === 0 && (
                  <div className="p-2 text-xs text-[#8f9194] text-center">
                    {isAr ? 'لم يتم العثور على نتائج' : 'No results found'}
                  </div>
                )}
              </div>
            )}
          </div>
        </div>

        {/* End / Right: Controls & Actions */}
        <div className="flex items-center gap-2 sm:gap-3 shrink-0">
          {/* Live System Status Pill */}
          <div className="hidden 2xl:flex items-center gap-1.5 px-2.5 py-1 bg-[#1a1c1f] rounded-full border border-white/5">
            <span className="w-1.5 h-1.5 rounded-full bg-[#10b981] animate-pulse"></span>
            <span className="text-xs text-[#e2e2e6] font-medium">
              {isAr ? 'الفاتورة الإلكترونية والضرائب المصرية · متصل' : 'ETA E-Invoice · Live'}
            </span>
          </div>

          {/* Currency Toggle (EGP Primary) */}
          <div className="flex items-center bg-[#1a1c1f] p-0.5 rounded-lg border border-white/5">
            {(['EGP', 'USD', 'SAR', 'EUR'] as const).map(cur => {
              const labelMap: Record<string, string> = {
                EGP: isAr ? 'ج.م' : 'EGP',
                USD: '$',
                SAR: isAr ? 'ر.س' : 'SAR',
                EUR: '€',
              };
              return (
                <button
                  key={cur}
                  onClick={() => setActiveCurrency(cur)}
                  className={`px-2 py-1 text-[11px] font-bold rounded transition-colors ${
                    activeCurrency === cur
                      ? 'bg-[#333538] text-white shadow-sm'
                      : 'text-[#8f9194] hover:text-[#e2e2e6]'
                  }`}
                  title={cur}
                >
                  {labelMap[cur] || cur}
                </button>
              );
            })}
          </div>

          {/* Language Toggle */}
          <div className="flex items-center bg-[#1a1c1f] p-0.5 rounded-lg border border-white/5">
            <button
              onClick={() => setLanguage('ar')}
              className={`px-2 py-1 text-[11px] font-bold rounded transition-colors ${
                language === 'ar' ? 'bg-[#333538] text-[#4edea3] shadow-sm' : 'text-[#8f9194] hover:text-[#e2e2e6]'
              }`}
            >
              عربي
            </button>
            <button
              onClick={() => setLanguage('en')}
              className={`px-2 py-1 text-[11px] font-medium rounded transition-colors ${
                language === 'en' ? 'bg-[#333538] text-white shadow-sm' : 'text-[#8f9194] hover:text-[#e2e2e6]'
              }`}
            >
              EN
            </button>
          </div>

          {/* High-Contrast "New Order" Action */}
          <button
            onClick={() => {
              if (onOpenNewOrder) {
                onOpenNewOrder();
              } else {
                setOpenQuickOrderModal(true);
              }
            }}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-[#ffffff] hover:bg-[#e2e2e4] text-[#111316] text-xs font-bold rounded-lg shadow-[0_1px_4px_rgba(0,0,0,0.3)] transition-all cursor-pointer"
          >
            <span className="material-symbols-outlined text-base font-bold">add</span>
            <span className="hidden xs:inline">{isAr ? 'أمر جديد' : 'New Order'}</span>
          </button>

          {/* Profile Dropdown */}
          <div ref={profileRef} className="relative">
            <div
              onClick={() => setIsProfileOpen(!isProfileOpen)}
              className="flex items-center gap-2 pl-1 cursor-pointer select-none"
            >
              <div className="w-8 h-8 rounded-full bg-[#282a2d] border border-white/10 flex items-center justify-center text-xs font-bold text-[#e2e2e6]">
                {userSession?.username ? userSession.username.substring(0, 2).toUpperCase() : 'TF'}
              </div>
              <span className="material-symbols-outlined text-[#8f9194] text-sm">
                expand_more
              </span>
            </div>

            {isProfileOpen && (
              <div className="absolute right-0 mt-2 w-48 bg-[#1e2023] border border-[#26292e] rounded shadow-2xl py-1 z-50">
                <div className="px-3 py-2 border-b border-[#26292e]">
                  <p className="text-xs font-semibold text-white truncate">
                    {userSession?.username || 'Trader Executive'}
                  </p>
                  <p className="text-[11px] text-[#8f9194] truncate">
                    {userSession?.role || 'Administrator'}
                  </p>
                </div>
                <button
                  onClick={() => {
                    setCurrentPage('settings');
                    setIsProfileOpen(false);
                  }}
                  className="w-full text-left px-3 py-2 text-xs text-[#c5c6c9] hover:bg-[#282a2d] hover:text-white flex items-center gap-2"
                >
                  <span className="material-symbols-outlined text-sm">settings</span>
                  <span>{language === 'ar' ? 'إعدادات النظام' : 'System Settings'}</span>
                </button>
                <button
                  onClick={logoutSession}
                  className="w-full text-left px-3 py-2 text-xs text-rose-400 hover:bg-[#282a2d] flex items-center gap-2"
                >
                  <span className="material-symbols-outlined text-sm">logout</span>
                  <span>{language === 'ar' ? 'تسجيل الخروج' : 'Sign Out'}</span>
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
    </header>
  );
};
