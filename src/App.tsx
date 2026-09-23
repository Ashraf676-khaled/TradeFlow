import React, { useState, useEffect } from 'react';
import { TenantProvider, useTenant } from './context/TenantContext';
import { Header } from './components/layout/Header';
import { Sidebar } from './components/layout/Sidebar';
import { OverviewPage } from './components/dashboard/OverviewPage';
import { TradingTerminalPage } from './components/terminal/TradingTerminalPage';
import { OrdersPage } from './components/orders/OrdersPage';
import { InventoryPage } from './components/inventory/InventoryPage';
import { WarehousesPage } from './components/warehouses/WarehousesPage';
import { InvoicesPage } from './components/invoices/InvoicesPage';
import { CustomersPage } from './components/customers/CustomersPage';
import { AnalyticsPage } from './components/analytics/AnalyticsPage';
import { SettingsPage } from './components/settings/SettingsPage';
import { CreateOrderModal } from './components/orders/CreateOrderModal';
import { LoginPage } from './components/auth/LoginPage';
import { InvoiceDetailModal } from './components/invoices/InvoiceDetailModal';
import { Invoice } from './types';

const MainAppContent: React.FC = () => {
  const {
    isAuthenticated,
    loginSession,
    currentPage,
    apiError,
    language,
    openQuickOrderModal,
    setOpenQuickOrderModal,
  } = useTenant();

  const [isCreateOrderModalOpen, setIsCreateOrderModalOpen] = useState(false);
  const [createdInvoice, setCreatedInvoice] = useState<Invoice | null>(null);

  // Sync with context quick order trigger
  useEffect(() => {
    if (openQuickOrderModal) {
      setIsCreateOrderModalOpen(true);
      setOpenQuickOrderModal(false);
    }
  }, [openQuickOrderModal, setOpenQuickOrderModal]);

  // Authentication Guard: Render Login Page if not logged in
  if (!isAuthenticated) {
    return <LoginPage onLoginSuccess={loginSession} />;
  }

  const isRtl = language === 'ar';

  return (
    <div
      className="min-h-screen bg-[#0b0c0e] text-[#e2e2e6] font-sans antialiased selection:bg-[#4edea3]/20 selection:text-[#4edea3]"
      dir={isRtl ? 'rtl' : 'ltr'}
    >
      {/* Fixed Sidebar */}
      <Sidebar />

      {/* Main Content Area offset by sidebar */}
      <div className={`flex flex-col min-h-screen transition-all ${isRtl ? 'lg:pr-72' : 'lg:pl-72'}`}>
        {/* Sticky Header */}
        <Header onOpenNewOrder={() => setIsCreateOrderModalOpen(true)} />

        {/* Global API Error Alert Banner */}
        {apiError && (
          <div className="mx-4 md:mx-6 mt-3 p-3 rounded bg-rose-500/15 border border-rose-500/30 text-rose-400 text-xs flex items-center justify-between">
            <div className="flex items-center gap-2">
              <span className="material-symbols-outlined text-sm">error</span>
              <span>{apiError}</span>
            </div>
          </div>
        )}

        {/* Dynamic Page Views */}
        <main className="flex-1 p-4 md:p-6 max-w-7xl w-full mx-auto">
          {currentPage === 'overview' && (
            <OverviewPage onOpenCreateOrder={() => setIsCreateOrderModalOpen(true)} />
          )}

          {currentPage === 'terminal' && (
            <TradingTerminalPage />
          )}

          {currentPage === 'orders' && (
            <OrdersPage onOpenCreateOrder={() => setIsCreateOrderModalOpen(true)} />
          )}

          {currentPage === 'inventory' && (
            <InventoryPage />
          )}

          {currentPage === 'warehouses' && (
            <WarehousesPage />
          )}

          {currentPage === 'invoices' && (
            <InvoicesPage />
          )}

          {currentPage === 'customers' && (
            <CustomersPage />
          )}

          {currentPage === 'analytics' && (
            <AnalyticsPage />
          )}

          {currentPage === 'settings' && (
            <SettingsPage />
          )}
        </main>
      </div>

      {/* Global Order Creation Modal */}
      <CreateOrderModal
        isOpen={isCreateOrderModalOpen}
        onClose={() => setIsCreateOrderModalOpen(false)}
        onInvoiceCreated={(inv) => setCreatedInvoice(inv)}
      />

      {/* Invoice Detail / Print Modal */}
      <InvoiceDetailModal
        invoice={createdInvoice}
        onClose={() => setCreatedInvoice(null)}
      />
    </div>
  );
};

export function App() {
  return (
    <TenantProvider>
      <MainAppContent />
    </TenantProvider>
  );
}

export default App;
