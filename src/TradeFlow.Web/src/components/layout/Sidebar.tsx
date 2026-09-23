import React, { useState } from 'react';
import { LayoutDashboard, ShoppingCart, Package, FileText, Users, Layers, LogOut, RefreshCw, Settings } from 'lucide-react';
import { useTenant, NavigationPage } from '../../context/TenantContext';

interface SidebarProps {
}

interface NavItem {
  id: NavigationPage;
  label: string;
  icon: React.ElementType;
  badge?: number;
}

export const Sidebar: React.FC<SidebarProps> = () => {
  const { currentPage, setCurrentPage, orders, products, invoices, logoutSession, refreshAllData, isLoadingData, apiError } = useTenant();
  const [refreshMessage, setRefreshMessage] = useState('');

  const pendingOrdersCount = orders.filter(o => o.status === 'مسودة' || o.status === 'مؤكد').length;
  const unpaidInvoicesCount = invoices.filter(i => i.status === 'غير مدفوع' || i.status === 'متأخر').length;

  const navItems: NavItem[] = [
    {
      id: 'overview',
      label: 'لوحة التحكم الرئيسية',
      icon: LayoutDashboard,
    },
    {
      id: 'orders',
      label: 'أوامر المبيعات',
      icon: ShoppingCart,
      badge: pendingOrdersCount > 0 ? pendingOrdersCount : undefined,
    },
    {
      id: 'inventory',
      label: 'إدارة المخزون والمنتجات',
      icon: Package,
    },
    {
      id: 'invoices',
      label: 'الفواتير والتحصيل',
      icon: FileText,
      badge: unpaidInvoicesCount > 0 ? unpaidInvoicesCount : undefined,
    },
    {
      id: 'customers',
      label: 'دليل العملاء',
      icon: Users,
    },
    {
      id: 'settings',
      label: 'إعدادات النظام',
      icon: Settings,
    },
  ];

  const handleNavClick = (pageId: NavigationPage) => {
    setCurrentPage(pageId);
  };

  return (
    <nav className="sticky top-16 z-20 w-full border-b border-slate-200 bg-white shadow-sm" dir="rtl">
      <div className="mx-auto flex max-w-7xl items-center gap-3 overflow-x-auto px-4 py-2 lg:px-8">
        <div className="hidden shrink-0 items-center gap-2 border-l border-slate-200 pl-4 md:flex">
          <div className="flex items-center gap-3">
            <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-blue-700 text-white shadow-md">
              <Layers className="w-5 h-5" />
            </div>
            <div>
              <span className="font-black text-lg tracking-tight text-slate-900">
                  تريد<span className="text-blue-700">فلو</span>
              </span>
            </div>
          </div>
        </div>
        <div className="flex min-w-max flex-1 items-center gap-1">
              {navItems.map((item) => {
                const Icon = item.icon;
                const isActive = currentPage === item.id;
                return (
                  <button
                    key={item.id}
                    onClick={() => handleNavClick(item.id)}
                    className={`flex items-center gap-2 rounded-xl px-3 py-2.5 font-bold text-xs transition-all ${
                      isActive
                        ? 'bg-blue-600 text-white shadow-md shadow-blue-600/20'
                        : 'text-slate-600 hover:text-slate-900 hover:bg-slate-100/80'
                    }`}
                  >
                    <div className="flex items-center gap-2">
                      <Icon className={`w-4 h-4 ${isActive ? 'text-white' : 'text-slate-500'}`} />
                      <span>{item.label}</span>
                    </div>

                    {item.badge !== undefined && (
                      <span
                        className={`text-[10px] font-bold px-2 py-0.5 rounded-full ${
                          isActive ? 'bg-white/20 text-white' : 'bg-blue-100 text-blue-800'
                        }`}
                      >
                        {item.badge}
                      </span>
                    )}
                  </button>
                );
              })}
        </div>
        <button
          onClick={async () => {
            setRefreshMessage('جارٍ التحديث');
            const refreshed = await refreshAllData();
            setRefreshMessage(refreshed ? 'تم التحديث' : 'تعذر التحديث');
            window.setTimeout(() => setRefreshMessage(''), 2500);
          }}
          disabled={isLoadingData}
          title="تحديث البيانات"
          className="shrink-0 rounded-lg p-2 text-blue-700 hover:bg-blue-50 disabled:opacity-50"
        >
          <RefreshCw className={`h-4 w-4 ${isLoadingData ? 'animate-spin' : ''}`} />
        </button>
        {refreshMessage && <span className="shrink-0 text-[10px] font-bold text-slate-500">{refreshMessage}</span>}
        <div className="shrink-0 border-r border-slate-200 pr-2">
          <button
            onClick={logoutSession}
            title="تسجيل الخروج"
            className="rounded-lg p-2 text-rose-600 hover:bg-rose-50"
          >
            <LogOut className="w-4 h-4" />
          </button>
        </div>
      </div>
    </nav>
  );
};
