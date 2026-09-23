import React, { useState } from 'react';
import { useTenant } from '../../context/TenantContext';
import { Customer } from '../../types';

export const CustomersPage: React.FC = () => {
  const {
    customers,
    formatCurrency,
    createCustomer,
    updateCustomer,
    changeCustomerCreditLimit,
    setCustomerStatus,
    deleteCustomer,
    language,
  } = useTenant();

  const [searchTerm, setSearchTerm] = useState('');
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingCustomerId, setEditingCustomerId] = useState<string | null>(null);
  const [form, setForm] = useState({ name: '', phone: '', email: '', creditLimit: '5000' });
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Credit limit adjustment modal
  const [creditModalCustomer, setCreditModalCustomer] = useState<Customer | null>(null);
  const [newCreditLimitValue, setNewCreditLimitValue] = useState<number>(0);

  const filteredCustomers = customers.filter(c =>
    (c.name || '').toLowerCase().includes(searchTerm.toLowerCase()) ||
    (c.company || '').toLowerCase().includes(searchTerm.toLowerCase()) ||
    (c.phone || '').toLowerCase().includes(searchTerm.toLowerCase()) ||
    (c.email || '').toLowerCase().includes(searchTerm.toLowerCase())
  );

  const totalCreditOffered = customers.reduce((sum, c) => sum + (c.creditLimit || 0), 0);
  const totalBalanceUtilized = customers.reduce((sum, c) => sum + (c.currentBalance || 0), 0);
  const activeCount = customers.filter(c => c.isActive !== false).length;

  const handleOpenCreate = () => {
    setEditingCustomerId(null);
    setForm({ name: '', phone: '', email: '', creditLimit: '5000' });
    setError('');
    setIsFormOpen(true);
  };

  const handleOpenEdit = (c: Customer) => {
    setEditingCustomerId(c.id);
    setForm({
      name: c.name,
      phone: c.phone || '',
      email: c.email || '',
      creditLimit: (c.creditLimit || 0).toString(),
    });
    setError('');
    setIsFormOpen(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    if (!form.name.trim() || !form.phone.trim()) {
      setError(language === 'ar' ? 'اسم العميل ورقم الهاتف مطلوبان.' : 'Customer name and phone number are required.');
      return;
    }

    try {
      setIsSubmitting(true);
      const creditLimit = parseFloat(form.creditLimit) || 0;
      if (editingCustomerId) {
        await updateCustomer(editingCustomerId, {
          name: form.name.trim(),
          phone: form.phone.trim(),
          email: form.email.trim() || undefined,
          creditLimit,
        });
      } else {
        await createCustomer({
          name: form.name.trim(),
          phone: form.phone.trim(),
          email: form.email.trim() || undefined,
          creditLimit,
        });
      }
      setIsFormOpen(false);
    } catch (err: any) {
      setError(err.response?.data?.detail || err.response?.data?.title || err.message || 'Failed to save customer account.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleSaveCreditLimit = async () => {
    if (!creditModalCustomer) return;
    try {
      await changeCustomerCreditLimit(creditModalCustomer.id, newCreditLimitValue);
      setCreditModalCustomer(null);
    } catch (err) {
      console.error(err);
    }
  };

  const handleToggleStatus = async (c: Customer) => {
    try {
      await setCustomerStatus(c.id, !c.isActive);
    } catch (err) {
      console.error(err);
    }
  };

  const handleDelete = async (c: Customer) => {
    if (!window.confirm(language === 'ar' ? 'هل أنت متأكد من حذف حساب هذا العميل؟' : 'Are you sure you want to delete this customer account?')) {
      return;
    }
    try {
      await deleteCustomer(c.id);
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <div className="space-y-4">
      {/* Header Banner */}
      <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-white tracking-tight">
            {language === 'ar' ? 'الحسابات والعملاء التجاريين' : 'Counterparties & Accounts'}
          </h1>
          <p className="text-xs text-[#8f9194] mt-0.5">
            {language === 'ar'
              ? 'دليل العملاء والشركات والحدود الائتمانية والتعرض المالي'
              : 'Institutional client directory, active credit limits, settlement risk exposure, and balances'}
          </p>
        </div>

        <button
          onClick={handleOpenCreate}
          className="flex items-center gap-1.5 px-3.5 py-1.5 bg-[#ffffff] hover:bg-[#e2e2e4] text-[#111316] font-semibold rounded text-xs transition-all shadow-md cursor-pointer"
        >
          <span className="material-symbols-outlined text-sm font-bold">add</span>
          <span>{language === 'ar' ? 'إضافة عميل' : 'New Counterparty'}</span>
        </button>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-3.5">
        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5">
          <span className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
            {language === 'ar' ? 'إجمالي الحسابات المسجلة' : 'Total Client Accounts'}
          </span>
          <div className="text-xl font-bold font-mono text-white mt-1">
            {customers.length} Accounts
          </div>
          <div className="text-[10px] text-[#4edea3] mt-1 font-mono">
            {activeCount} active in system
          </div>
        </div>

        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5">
          <span className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
            {language === 'ar' ? 'إجمالي التسهيلات الائتمانية' : 'Approved Credit Facilities'}
          </span>
          <div className="text-xl font-bold font-mono text-white mt-1">
            {formatCurrency(totalCreditOffered)}
          </div>
          <div className="text-[10px] text-[#8f9194] mt-1">
            Revolving credit lines
          </div>
        </div>

        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5">
          <span className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
            {language === 'ar' ? 'إجمالي الرصيد القائم' : 'Current Exposure / Utilized'}
          </span>
          <div className={`text-xl font-bold font-mono mt-1 ${totalBalanceUtilized > 0 ? 'text-amber-400' : 'text-white'}`}>
            {formatCurrency(totalBalanceUtilized)}
          </div>
          <div className="text-[10px] text-[#8f9194] mt-1">
            {totalCreditOffered > 0 ? ((totalBalanceUtilized / totalCreditOffered) * 100).toFixed(1) : 0}% utilization
          </div>
        </div>
      </div>

      {/* Search Bar */}
      <div className="p-3 rounded bg-[#1a1c1f] border border-white/5 flex items-center justify-between">
        <div className="relative w-full max-w-sm">
          <span className="material-symbols-outlined absolute left-2.5 top-1/2 -translate-y-1/2 text-sm text-[#8f9194]">
            search
          </span>
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder={language === 'ar' ? 'بحث بالاسم، الشركة، الهاتف...' : 'Search Counterparty Name, Company, Phone...'}
            className="w-full pl-8 pr-3 py-1.5 bg-[#111316] border border-white/10 rounded text-xs text-white placeholder-[#8f9194] outline-none focus:border-[#4edea3]"
          />
        </div>
      </div>

      {/* Customers Table */}
      <div className="rounded bg-[#1a1c1f] border border-white/5 overflow-hidden shadow-sm">
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-[#111316] text-[#8f9194] text-[11px] font-semibold uppercase tracking-wider border-b border-white/5">
                <th className="py-2.5 px-3">Counterparty Name</th>
                <th className="py-2.5 px-3">Contact Details</th>
                <th className="py-2.5 px-3 text-right">Credit Facility</th>
                <th className="py-2.5 px-3 text-right">Current Exposure</th>
                <th className="py-2.5 px-3">Utilization</th>
                <th className="py-2.5 px-3">Status</th>
                <th className="py-2.5 px-3 text-center">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-white/5 text-xs font-mono">
              {filteredCustomers.length === 0 ? (
                <tr>
                  <td colSpan={7} className="py-12 text-center text-xs text-[#8f9194] font-sans">
                    {language === 'ar' ? 'لم يتم العثور على عملاء' : 'No counterparties found.'}
                  </td>
                </tr>
              ) : (
                filteredCustomers.map(customer => {
                  const limit = customer.creditLimit || 0;
                  const balance = customer.currentBalance || 0;
                  const utilPct = limit > 0 ? Math.min(100, Math.round((balance / limit) * 100)) : 0;
                  const isActive = customer.isActive !== false;

                  return (
                    <tr key={customer.id} className="hover:bg-[#282a2d]/50 transition-colors">
                      <td className="py-3 px-3">
                        <div className="font-bold text-white font-sans">{customer.name}</div>
                        <div className="text-[10px] text-[#8f9194] font-sans">{customer.company || 'Enterprise Counterparty'}</div>
                      </td>

                      <td className="py-3 px-3 font-sans text-[#8f9194]">
                        <div>{customer.phone}</div>
                        {customer.email && <div className="text-[10px] text-[#8f9194]">{customer.email}</div>}
                      </td>

                      <td className="py-3 px-3 text-right">
                        <div
                          onClick={() => {
                            setCreditModalCustomer(customer);
                            setNewCreditLimitValue(customer.creditLimit || 0);
                          }}
                          className="font-bold text-white hover:text-[#4edea3] cursor-pointer flex items-center justify-end gap-1"
                          title="Click to adjust credit facility"
                        >
                          <span>{formatCurrency(limit)}</span>
                          <span className="material-symbols-outlined text-[11px] text-[#8f9194]">tune</span>
                        </div>
                      </td>

                      <td className="py-3 px-3 text-right">
                        <span className={balance > 0 ? 'text-amber-400 font-bold' : 'text-[#8f9194]'}>
                          {formatCurrency(balance)}
                        </span>
                      </td>

                      <td className="py-3 px-3 font-sans">
                        <div className="w-24 space-y-1">
                          <div className="flex justify-between text-[10px] text-[#8f9194] font-mono">
                            <span>{utilPct}%</span>
                          </div>
                          <div className="h-1.5 w-full bg-[#282a2d] rounded-full overflow-hidden">
                            <div
                              className={`h-full rounded-full ${utilPct > 80 ? 'bg-rose-500' : utilPct > 50 ? 'bg-amber-400' : 'bg-[#4edea3]'}`}
                              style={{ width: `${utilPct}%` }}
                            ></div>
                          </div>
                        </div>
                      </td>

                      <td className="py-3 px-3 font-sans">
                        <span className={`px-2 py-0.5 rounded-full text-[10px] font-semibold ${
                          isActive
                            ? 'bg-[#10b981]/15 text-[#4edea3] border border-[#10b981]/30'
                            : 'bg-rose-500/15 text-rose-400 border border-rose-500/30'
                        }`}>
                          {isActive ? 'Active' : 'Inactive'}
                        </span>
                      </td>

                      <td className="py-3 px-3 text-center">
                        <div className="flex items-center justify-center gap-1 font-sans">
                          <button
                            onClick={() => handleOpenEdit(customer)}
                            title="Edit Account"
                            className="p-1 rounded bg-[#282a2d] hover:bg-[#333538] text-white transition-colors"
                          >
                            <span className="material-symbols-outlined text-[15px]">edit</span>
                          </button>

                          <button
                            onClick={() => handleToggleStatus(customer)}
                            title={isActive ? 'Deactivate' : 'Activate'}
                            className={`p-1 rounded transition-colors ${
                              isActive ? 'bg-[#282a2d] hover:bg-amber-900/30 text-amber-400' : 'bg-[#282a2d] hover:bg-emerald-900/30 text-[#4edea3]'
                            }`}
                          >
                            <span className="material-symbols-outlined text-[15px]">
                              {isActive ? 'block' : 'check_circle'}
                            </span>
                          </button>

                          <button
                            onClick={() => handleDelete(customer)}
                            title="Delete"
                            className="p-1 rounded bg-[#282a2d] hover:bg-rose-900/30 text-[#8f9194] hover:text-rose-400 transition-colors"
                          >
                            <span className="material-symbols-outlined text-[15px]">delete</span>
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Customer Form Modal */}
      {isFormOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/75 backdrop-blur-xs">
          <form
            onSubmit={handleSubmit}
            className="w-full max-w-md bg-[#1a1c1f] border border-[#26292e] rounded shadow-2xl p-5 space-y-4"
          >
            <div className="flex items-center justify-between pb-3 border-b border-white/5">
              <div className="flex items-center gap-2">
                <span className="material-symbols-outlined text-[#4edea3]">person</span>
                <div>
                  <h3 className="text-sm font-bold text-white">
                    {editingCustomerId ? 'Edit Counterparty' : 'New Counterparty Account'}
                  </h3>
                  <p className="text-[11px] text-[#8f9194]">Client entity and credit facility</p>
                </div>
              </div>
              <button
                type="button"
                onClick={() => setIsFormOpen(false)}
                className="p-1 rounded text-[#8f9194] hover:text-white"
              >
                <span className="material-symbols-outlined text-base">close</span>
              </button>
            </div>

            {error && (
              <div className="p-2.5 rounded bg-rose-500/15 border border-rose-500/30 text-rose-400 text-xs">
                {error}
              </div>
            )}

            <div className="space-y-3 font-sans">
              <div>
                <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                  Entity / Customer Name *
                </label>
                <input
                  type="text"
                  required
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                  placeholder="e.g. Apex Trading Corp"
                  className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-xs text-white focus:border-[#4edea3] outline-none"
                />
              </div>

              <div>
                <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                  Phone Number *
                </label>
                <input
                  type="text"
                  required
                  value={form.phone}
                  onChange={(e) => setForm({ ...form, phone: e.target.value })}
                  placeholder="+20 100 000 0000"
                  className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-xs text-white focus:border-[#4edea3] outline-none"
                />
              </div>

              <div>
                <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                  Email Address
                </label>
                <input
                  type="email"
                  value={form.email}
                  onChange={(e) => setForm({ ...form, email: e.target.value })}
                  placeholder="contact@apextrading.com"
                  className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-xs text-white focus:border-[#4edea3] outline-none"
                />
              </div>

              <div>
                <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                  Credit Facility Limit
                </label>
                <input
                  type="number"
                  step="100"
                  value={form.creditLimit}
                  onChange={(e) => setForm({ ...form, creditLimit: e.target.value })}
                  className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-xs font-mono text-white focus:border-[#4edea3] outline-none"
                />
              </div>
            </div>

            <div className="flex items-center justify-end gap-2 pt-2 border-t border-white/5">
              <button
                type="button"
                onClick={() => setIsFormOpen(false)}
                className="px-3 py-1.5 rounded bg-[#282a2d] hover:bg-[#333538] text-[#8f9194] hover:text-white text-xs cursor-pointer"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={isSubmitting}
                className="px-4 py-1.5 rounded bg-[#ffffff] hover:bg-[#e2e2e4] text-[#111316] font-bold text-xs uppercase tracking-wider transition-all cursor-pointer"
              >
                {isSubmitting ? 'Saving...' : 'Save Account'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Credit Limit Modal */}
      {creditModalCustomer && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/75 backdrop-blur-xs">
          <div className="w-full max-w-sm bg-[#1a1c1f] border border-[#26292e] rounded shadow-2xl p-5 space-y-4">
            <div className="flex items-center justify-between pb-3 border-b border-white/5">
              <h3 className="text-sm font-bold text-white">Adjust Credit Limit</h3>
              <button onClick={() => setCreditModalCustomer(null)} className="text-[#8f9194] hover:text-white">
                <span className="material-symbols-outlined text-base">close</span>
              </button>
            </div>

            <div className="space-y-3 text-xs">
              <div className="text-[#8f9194]">
                Account: <strong className="text-white">{creditModalCustomer.name}</strong>
              </div>

              <div>
                <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                  New Credit Limit
                </label>
                <input
                  type="number"
                  step="500"
                  value={newCreditLimitValue}
                  onChange={(e) => setNewCreditLimitValue(parseFloat(e.target.value) || 0)}
                  className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-xs font-mono text-white focus:border-[#4edea3] outline-none"
                />
              </div>
            </div>

            <div className="flex items-center justify-end gap-2 pt-2 border-t border-white/5">
              <button
                type="button"
                onClick={() => setCreditModalCustomer(null)}
                className="px-3 py-1.5 rounded bg-[#282a2d] text-[#8f9194] hover:text-white text-xs"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleSaveCreditLimit}
                className="px-4 py-1.5 rounded bg-[#10b981] hover:bg-[#059669] text-white font-bold text-xs uppercase"
              >
                Update Limit
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
