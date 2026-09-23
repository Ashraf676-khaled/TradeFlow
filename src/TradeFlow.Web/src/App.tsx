import React, { useState } from 'react';
import { TenantProvider, useTenant } from './context/TenantContext';
import { Header } from './components/layout/Header';
import { Sidebar } from './components/layout/Sidebar';
import { OverviewPage } from './components/dashboard/OverviewPage';
import { OrdersPage } from './components/orders/OrdersPage';
import { InventoryPage } from './components/inventory/InventoryPage';
import { InvoicesPage } from './components/invoices/InvoicesPage';
import { CustomersPage } from './components/customers/CustomersPage';
import { SettingsPage } from './components/settings/SettingsPage';
import { CreateOrderModal } from './components/orders/CreateOrderModal';
import { LoginPage } from './components/auth/LoginPage';
import { InvoiceDetailModal } from './components/invoices/InvoiceDetailModal';
import { Invoice } from './types';

const MainAppContent: React.FC = () => {
  const { isAuthenticated, loginSession, currentPage, apiError } = useTenant();
  const [isCreateOrderModalOpen, setIsCreateOrderModalOpen] = useState(false);
  const [createdInvoice, setCreatedInvoice] = useState<Invoice | null>(null);

  // Authentication Guard: Render Login Page if not logged in
  if (!isAuthenticated) {
    return <LoginPage onLoginSuccess={loginSession} />;
  }

  return (
    <div className="min-h-screen bg-slate-50 text-slate-800 font-sans antialiased" dir="rtl">
        
        {/* Sticky Header */}
        <Header />
        <Sidebar />

        {apiError && (
          <div className="mx-6 mt-4 p-3 rounded-xl bg-rose-50 border border-rose-200 text-rose-700 text-xs font-bold text-right">
            {apiError}
          </div>
        )}

        {/* Dynamic Page Views */}
        <main className="flex-1 p-4 sm:p-6 lg:p-8 max-w-7xl w-full mx-auto">
          {currentPage === 'overview' && (
            <OverviewPage 
              onOpenCreateOrder={() => setIsCreateOrderModalOpen(true)}
            />
          )}

          {currentPage === 'orders' && (
            <OrdersPage 
              onOpenCreateOrder={() => setIsCreateOrderModalOpen(true)}
            />
          )}

          {currentPage === 'inventory' && (
            <InventoryPage />
          )}

          {currentPage === 'invoices' && (
            <InvoicesPage />
          )}

          {currentPage === 'customers' && (
            <CustomersPage />
          )}

          {currentPage === 'settings' && (
            <SettingsPage />
          )}
        </main>
      {/* Global Order Creation Modal */}
      <CreateOrderModal 
        isOpen={isCreateOrderModalOpen}
        onClose={() => setIsCreateOrderModalOpen(false)}
        onInvoiceCreated={setCreatedInvoice}
      />
      <InvoiceDetailModal invoice={createdInvoice} onClose={() => setCreatedInvoice(null)} />

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
