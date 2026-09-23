import React, { useState, useMemo } from 'react';
import { useTenant } from '../../context/TenantContext';
import { Product } from '../../types';
import { StockAdjustModal } from './StockAdjustModal';
import { StockTransferModal } from './StockTransferModal';
import { WarehouseModal } from './WarehouseModal';
import * as XLSX from 'xlsx';

export const InventoryPage: React.FC = () => {
  const {
    products,
    warehouses,
    formatCurrency,
    createProduct,
    updateProductPrice,
    deleteProduct,
    settings,
    language,
    refreshAllData,
  } = useTenant();

  const [selectedWarehouseFilter, setSelectedWarehouseFilter] = useState<string>('all');
  const [selectedStatusFilter, setSelectedStatusFilter] = useState<string>('all');
  const [searchQuery, setSearchQuery] = useState('');
  const [activeAdjustProduct, setActiveAdjustProduct] = useState<Product | null>(null);
  const [isAdjustOpen, setIsAdjustOpen] = useState(false);
  const [isTransferOpen, setIsTransferOpen] = useState(false);
  const [isWarehouseModalOpen, setIsWarehouseModalOpen] = useState(false);
  const [isAddProductOpen, setIsAddProductOpen] = useState(false);

  // New product form state
  const [newProduct, setNewProduct] = useState({
    sku: '',
    name: '',
    description: '',
    unitPrice: '',
    costPrice: '',
    minimumStock: '5',
    openingStockQuantity: '10',
    openingStockWarehouseId: '',
  });
  const [productError, setProductError] = useState('');
  const [isSubmittingProduct, setIsSubmittingProduct] = useState(false);

  // Quick edit price state
  const [editingPriceProductId, setEditingPriceProductId] = useState<string | null>(null);
  const [editPriceValue, setEditPriceValue] = useState<string>('');

  const filteredProducts = useMemo(() => {
    return products.filter(p => {
      const matchesWarehouse = selectedWarehouseFilter === 'all' || p.warehouseId === selectedWarehouseFilter;
      const matchesStatus =
        selectedStatusFilter === 'all' ||
        (selectedStatusFilter === 'low' && (p.status === 'مخزون منخفض' || (p.currentStock ?? 0) <= (p.minStockLevel || 5))) ||
        (selectedStatusFilter === 'depleted' && (p.status === 'نفد المخزون' || (p.currentStock ?? 0) <= 0)) ||
        (selectedStatusFilter === 'instock' && (p.currentStock ?? 0) > (p.minStockLevel || 5));

      const q = searchQuery.toLowerCase();
      const matchesSearch =
        (p.name || '').toLowerCase().includes(q) ||
        (p.sku || '').toLowerCase().includes(q) ||
        (p.category || '').toLowerCase().includes(q);

      return matchesWarehouse && matchesStatus && matchesSearch;
    });
  }, [products, selectedWarehouseFilter, selectedStatusFilter, searchQuery]);

  // Aggregate metrics
  const totalValuation = products.reduce((acc, p) => acc + ((p.currentStock || 0) * (p.unitPrice || 0)), 0);
  const totalCost = products.reduce((acc, p) => acc + ((p.currentStock || 0) * (p.costPrice || 0)), 0);
  const totalUnits = products.reduce((acc, p) => acc + (p.currentStock || 0), 0);
  const lowStockCount = products.filter(p => (p.currentStock ?? 0) <= (p.minStockLevel || 5)).length;

  const handleExport = () => {
    const data = filteredProducts.map(p => ({
      SKU: p.sku,
      Name: p.name,
      Category: p.category || 'General',
      Warehouse: warehouses.find(w => w.id === p.warehouseId)?.name || 'Central',
      CostPrice: p.costPrice || 0,
      SellingPrice: p.unitPrice || 0,
      CurrentStock: p.currentStock || 0,
      AvailableStock: p.availableStock || p.currentStock || 0,
      Status: p.status,
    }));
    const worksheet = XLSX.utils.json_to_sheet(data);
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Inventory');
    XLSX.writeFile(workbook, `tradeflow_inventory_${new Date().toISOString().slice(0, 10)}.xlsx`);
  };

  const handleCreateProductSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setProductError('');
    if (!newProduct.sku.trim() || !newProduct.name.trim()) {
      setProductError('SKU and Product Name are required.');
      return;
    }
    const unitPrice = parseFloat(newProduct.unitPrice);
    const costPrice = parseFloat(newProduct.costPrice);
    if (isNaN(unitPrice) || isNaN(costPrice) || unitPrice < 0 || costPrice < 0) {
      setProductError('Valid selling and cost prices are required.');
      return;
    }

    try {
      setIsSubmittingProduct(true);
      await createProduct({
        sku: newProduct.sku.trim().toUpperCase(),
        name: newProduct.name.trim(),
        description: newProduct.description.trim() || undefined,
        unitPrice,
        costPrice,
        minimumStock: parseInt(newProduct.minimumStock) || 5,
        openingStockQuantity: parseInt(newProduct.openingStockQuantity) || 0,
        openingStockWarehouseId: newProduct.openingStockWarehouseId || warehouses[0]?.id,
      });

      setIsAddProductOpen(false);
      setNewProduct({
        sku: '',
        name: '',
        description: '',
        unitPrice: '',
        costPrice: '',
        minimumStock: '5',
        openingStockQuantity: '10',
        openingStockWarehouseId: '',
      });
    } catch (err: any) {
      setProductError(err.response?.data?.detail || err.response?.data?.title || err.message || 'Failed to create product.');
    } finally {
      setIsSubmittingProduct(false);
    }
  };

  const handleSavePrice = async (productId: string) => {
    const val = parseFloat(editPriceValue);
    if (isNaN(val) || val <= 0) return;
    try {
      await updateProductPrice(productId, val);
      setEditingPriceProductId(null);
    } catch (err) {
      console.error(err);
    }
  };

  const handleDelete = async (productId: string) => {
    if (!window.confirm(language === 'ar' ? 'هل أنت متأكد من حذف هذا المنتج؟' : 'Are you sure you want to delete this product?')) {
      return;
    }
    try {
      await deleteProduct(productId);
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <div className="space-y-4">
      {/* Top Banner & High-Density Stat Cards */}
      <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-white tracking-tight">
            {language === 'ar' ? 'مخزون الأصول والمنتجات' : 'Asset Inventory & Multi-Warehouse Stock'}
          </h1>
          <p className="text-xs text-[#8f9194] mt-0.5">
            {language === 'ar'
              ? 'مراقبة فورية للمخزون والتوريد وتوزيع البضائع عبر المراكز اللوجستية'
              : 'Real-time multi-location inventory ledger, stock valuation, and supply chain controls'}
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <button
            onClick={handleExport}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-[#1a1c1f] hover:bg-[#282a2d] text-[#e2e2e6] rounded text-xs transition-colors border border-white/5 cursor-pointer"
          >
            <span className="material-symbols-outlined text-sm text-[#8f9194]">file_download</span>
            <span>{language === 'ar' ? 'تصدير إكسيل' : 'Export Excel'}</span>
          </button>

          <button
            onClick={() => {
              setActiveAdjustProduct(null);
              setIsTransferOpen(true);
            }}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-[#1a1c1f] hover:bg-[#282a2d] text-amber-400 rounded text-xs transition-colors border border-amber-500/20 cursor-pointer"
          >
            <span className="material-symbols-outlined text-sm">swap_horiz</span>
            <span>{language === 'ar' ? 'تحويل مخزون' : 'Transfer Stock'}</span>
          </button>

          <button
            onClick={() => {
              setActiveAdjustProduct(null);
              setIsAdjustOpen(true);
            }}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-[#1a1c1f] hover:bg-[#282a2d] text-[#4edea3] rounded text-xs transition-colors border border-[#10b981]/20 cursor-pointer"
          >
            <span className="material-symbols-outlined text-sm">add_box</span>
            <span>{language === 'ar' ? 'توريد واستلام' : 'Receive Stock'}</span>
          </button>

          <button
            onClick={() => setIsAddProductOpen(true)}
            className="flex items-center gap-1.5 px-3.5 py-1.5 bg-[#ffffff] hover:bg-[#e2e2e4] text-[#111316] font-semibold rounded text-xs transition-all shadow-md cursor-pointer"
          >
            <span className="material-symbols-outlined text-sm font-bold">add</span>
            <span>{language === 'ar' ? 'إضافة صنف جديد' : 'New Product'}</span>
          </button>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-3.5">
        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5 flex items-center justify-between">
          <div>
            <div className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
              {language === 'ar' ? 'إجمالي الأصناف المسجلة' : 'Registered Instruments'}
            </div>
            <div className="text-xl font-bold font-mono text-white mt-1">
              {products.length} SKUs
            </div>
          </div>
          <span className="material-symbols-outlined text-2xl text-[#8f9194]">token</span>
        </div>

        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5 flex items-center justify-between">
          <div>
            <div className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
              {language === 'ar' ? 'تقييم المخزون الإجمالي' : 'Total Inventory Valuation'}
            </div>
            <div className="text-xl font-bold font-mono text-[#4edea3] mt-1">
              {formatCurrency(totalValuation)}
            </div>
          </div>
          <span className="material-symbols-outlined text-2xl text-[#4edea3]">payments</span>
        </div>

        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5 flex items-center justify-between">
          <div>
            <div className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
              {language === 'ar' ? 'إجمالي الوحدات الجاهزة' : 'Total Units In Hand'}
            </div>
            <div className="text-xl font-bold font-mono text-white mt-1">
              {totalUnits} Units
            </div>
          </div>
          <span className="material-symbols-outlined text-2xl text-[#8f9194]">inventory_2</span>
        </div>

        <div className="p-3.5 rounded bg-[#1a1c1f] border border-white/5 flex items-center justify-between">
          <div>
            <div className="text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider">
              {language === 'ar' ? 'تنبيهات حد الطلب' : 'Low Stock Reorders'}
            </div>
            <div className={`text-xl font-bold font-mono mt-1 ${lowStockCount > 0 ? 'text-amber-400' : 'text-white'}`}>
              {lowStockCount} items
            </div>
          </div>
          <span className={`material-symbols-outlined text-2xl ${lowStockCount > 0 ? 'text-amber-400' : 'text-[#8f9194]'}`}>
            warning
          </span>
        </div>
      </div>

      {/* Filter and Search Ribbon */}
      <div className="p-3 rounded bg-[#1a1c1f] border border-white/5 flex flex-col md:flex-row md:items-center justify-between gap-3">
        <div className="flex flex-wrap items-center gap-2">
          {/* Status Tabs */}
          <div className="flex items-center bg-[#111316] p-0.5 rounded border border-white/5">
            {[
              { id: 'all', label: language === 'ar' ? 'الكل' : 'All' },
              { id: 'instock', label: language === 'ar' ? 'متوفر' : 'In Stock' },
              { id: 'low', label: language === 'ar' ? 'منخفض' : 'Low Stock' },
              { id: 'depleted', label: language === 'ar' ? 'نافد' : 'Depleted' },
            ].map(tab => (
              <button
                key={tab.id}
                onClick={() => setSelectedStatusFilter(tab.id)}
                className={`px-2.5 py-1 text-xs font-medium rounded transition-colors ${
                  selectedStatusFilter === tab.id
                    ? 'bg-[#282a2d] text-white shadow-xs'
                    : 'text-[#8f9194] hover:text-[#e2e2e6]'
                }`}
              >
                {tab.label}
              </button>
            ))}
          </div>

          {/* Warehouse Selector */}
          <select
            value={selectedWarehouseFilter}
            onChange={(e) => setSelectedWarehouseFilter(e.target.value)}
            className="px-2.5 py-1 bg-[#111316] border border-white/10 rounded text-xs text-[#e2e2e6] outline-none"
          >
            <option value="all">{language === 'ar' ? 'جميع المستودعات' : 'All Warehouses'}</option>
            {warehouses.map(w => (
              <option key={w.id} value={w.id}>{w.name}</option>
            ))}
          </select>
        </div>

        {/* Search Input */}
        <div className="relative w-full md:w-64">
          <span className="material-symbols-outlined absolute left-2.5 top-1/2 -translate-y-1/2 text-sm text-[#8f9194]">
            search
          </span>
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder={language === 'ar' ? 'بحث بالاسم، الرمز SKU...' : 'Search Name, SKU, Category...'}
            className="w-full pl-8 pr-3 py-1.5 bg-[#111316] border border-white/10 rounded text-xs text-white placeholder-[#8f9194] outline-none focus:border-[#4edea3]"
          />
        </div>
      </div>

      {/* Inventory Table */}
      <div className="rounded bg-[#1a1c1f] border border-white/5 overflow-hidden shadow-sm">
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-[#111316] text-[#8f9194] text-[11px] font-semibold uppercase tracking-wider border-b border-white/5">
                <th className="py-2.5 px-3">SKU</th>
                <th className="py-2.5 px-3">Instrument / Name</th>
                <th className="py-2.5 px-3">Warehouse Hub</th>
                <th className="py-2.5 px-3 text-right">Cost Price</th>
                <th className="py-2.5 px-3 text-right">Selling Price</th>
                <th className="py-2.5 px-3 text-right">Margin</th>
                <th className="py-2.5 px-3 text-right">Stock On Hand</th>
                <th className="py-2.5 px-3">Status</th>
                <th className="py-2.5 px-3 text-center">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-white/5 text-xs font-mono">
              {filteredProducts.length === 0 ? (
                <tr>
                  <td colSpan={9} className="py-12 text-center text-xs text-[#8f9194] font-sans">
                    {language === 'ar' ? 'لا توجد منتجات مسجلة' : 'No inventory items found matching your filters.'}
                  </td>
                </tr>
              ) : (
                filteredProducts.map(product => {
                  const marginPct = product.unitPrice && product.unitPrice > 0
                    ? (((product.unitPrice - (product.costPrice || 0)) / product.unitPrice) * 100).toFixed(1)
                    : '0';

                  const isLow = (product.currentStock ?? 0) <= (product.minStockLevel || 5);
                  const isDepleted = (product.currentStock ?? 0) <= 0;

                  return (
                    <tr key={product.id} className="hover:bg-[#282a2d]/50 transition-colors">
                      <td className="py-3 px-3 font-bold text-white">
                        <span className="px-1.5 py-0.5 rounded bg-[#282a2d] text-[#e2e2e6] border border-white/5">
                          {product.sku}
                        </span>
                      </td>

                      <td className="py-3 px-3 font-sans text-white">
                        <div className="font-medium text-[#e2e2e6]">{product.name}</div>
                        <div className="text-[10px] text-[#8f9194]">{product.category || 'General Commodity'}</div>
                      </td>

                      <td className="py-3 px-3 font-sans text-[#8f9194]">
                        {warehouses.find(w => w.id === product.warehouseId)?.name || 'Central Distribution'}
                      </td>

                      <td className="py-3 px-3 text-right text-[#8f9194]">
                        {formatCurrency(product.costPrice || 0)}
                      </td>

                      <td className="py-3 px-3 text-right">
                        {editingPriceProductId === product.id ? (
                          <div className="flex items-center justify-end gap-1">
                            <input
                              type="number"
                              step="0.01"
                              value={editPriceValue}
                              onChange={(e) => setEditPriceValue(e.target.value)}
                              className="w-20 px-1 py-0.5 bg-[#111316] border border-[#4edea3] rounded text-xs text-white text-right"
                            />
                            <button
                              onClick={() => handleSavePrice(product.id)}
                              className="p-1 rounded bg-[#10b981] text-white"
                            >
                              <span className="material-symbols-outlined text-[13px]">check</span>
                            </button>
                            <button
                              onClick={() => setEditingPriceProductId(null)}
                              className="p-1 rounded bg-[#282a2d] text-[#8f9194]"
                            >
                              <span className="material-symbols-outlined text-[13px]">close</span>
                            </button>
                          </div>
                        ) : (
                          <div
                            onClick={() => {
                              setEditingPriceProductId(product.id);
                              setEditPriceValue(product.unitPrice.toString());
                            }}
                            className="font-bold text-white hover:text-[#4edea3] cursor-pointer flex items-center justify-end gap-1"
                            title="Click to edit price"
                          >
                            <span>{formatCurrency(product.unitPrice || 0)}</span>
                            <span className="material-symbols-outlined text-[11px] text-[#8f9194]">edit</span>
                          </div>
                        )}
                      </td>

                      <td className="py-3 px-3 text-right text-[#4edea3]">
                        {marginPct}%
                      </td>

                      <td className="py-3 px-3 text-right font-bold text-white">
                        <span className={isDepleted ? 'text-rose-400' : isLow ? 'text-amber-400' : 'text-white'}>
                          {product.currentStock ?? 0}
                        </span>
                      </td>

                      <td className="py-3 px-3 font-sans">
                        <span className={`px-2 py-0.5 rounded-full text-[10px] font-semibold ${
                          isDepleted
                            ? 'bg-rose-500/15 text-rose-400 border border-rose-500/30'
                            : isLow
                            ? 'bg-amber-500/15 text-amber-400 border border-amber-500/30'
                            : 'bg-[#10b981]/15 text-[#4edea3] border border-[#10b981]/30'
                        }`}>
                          {isDepleted ? 'Depleted' : isLow ? 'Low Stock' : 'Active'}
                        </span>
                      </td>

                      <td className="py-3 px-3 text-center">
                        <div className="flex items-center justify-center gap-1 font-sans">
                          <button
                            onClick={() => {
                              setActiveAdjustProduct(product);
                              setIsAdjustOpen(true);
                            }}
                            title="Receive Inbound Goods"
                            className="p-1 rounded bg-[#282a2d] hover:bg-[#333538] text-[#4edea3] transition-colors"
                          >
                            <span className="material-symbols-outlined text-[15px]">add_box</span>
                          </button>

                          <button
                            onClick={() => {
                              setActiveAdjustProduct(product);
                              setIsTransferOpen(true);
                            }}
                            title="Transfer Stock"
                            className="p-1 rounded bg-[#282a2d] hover:bg-[#333538] text-amber-400 transition-colors"
                          >
                            <span className="material-symbols-outlined text-[15px]">swap_horiz</span>
                          </button>

                          <button
                            onClick={() => handleDelete(product.id)}
                            title="Delete Product"
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

      {/* Add New Product Modal */}
      {isAddProductOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/75 backdrop-blur-xs">
          <div className="bg-[#1a1c1f] border border-[#26292e] rounded shadow-2xl w-full max-w-lg p-5 space-y-4">
            <div className="flex items-center justify-between pb-3 border-b border-white/5">
              <div className="flex items-center gap-2">
                <span className="material-symbols-outlined text-[#4edea3]">add_circle</span>
                <div>
                  <h3 className="text-sm font-bold text-white">
                    {language === 'ar' ? 'إضافة صنف جديد للنظام' : 'Register New Instrument / Product'}
                  </h3>
                  <p className="text-[11px] text-[#8f9194]">Create SKU and initial warehouse allocation</p>
                </div>
              </div>
              <button
                onClick={() => setIsAddProductOpen(false)}
                className="p-1 rounded text-[#8f9194] hover:text-white hover:bg-[#282a2d]"
              >
                <span className="material-symbols-outlined text-base">close</span>
              </button>
            </div>

            {productError && (
              <div className="p-2.5 rounded bg-rose-500/15 border border-rose-500/30 text-rose-400 text-xs">
                {productError}
              </div>
            )}

            <form onSubmit={handleCreateProductSubmit} className="space-y-3 font-sans text-xs">
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                    SKU Code *
                  </label>
                  <input
                    type="text"
                    required
                    value={newProduct.sku}
                    onChange={(e) => setNewProduct({ ...newProduct, sku: e.target.value })}
                    placeholder="e.g. COMM-GOLD-01"
                    className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-white font-mono focus:border-[#4edea3] outline-none"
                  />
                </div>

                <div>
                  <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                    Product Name *
                  </label>
                  <input
                    type="text"
                    required
                    value={newProduct.name}
                    onChange={(e) => setNewProduct({ ...newProduct, name: e.target.value })}
                    placeholder="e.g. Premium Arabica Coffee"
                    className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-white focus:border-[#4edea3] outline-none"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                    Cost Price *
                  </label>
                  <input
                    type="number"
                    step="0.01"
                    required
                    value={newProduct.costPrice}
                    onChange={(e) => setNewProduct({ ...newProduct, costPrice: e.target.value })}
                    placeholder="0.00"
                    className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-white font-mono focus:border-[#4edea3] outline-none"
                  />
                </div>

                <div>
                  <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                    Selling Price *
                  </label>
                  <input
                    type="number"
                    step="0.01"
                    required
                    value={newProduct.unitPrice}
                    onChange={(e) => setNewProduct({ ...newProduct, unitPrice: e.target.value })}
                    placeholder="0.00"
                    className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-white font-mono focus:border-[#4edea3] outline-none"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                    Opening Stock Quantity
                  </label>
                  <input
                    type="number"
                    value={newProduct.openingStockQuantity}
                    onChange={(e) => setNewProduct({ ...newProduct, openingStockQuantity: e.target.value })}
                    className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-white font-mono focus:border-[#4edea3] outline-none"
                  />
                </div>

                <div>
                  <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                    Minimum Reorder Threshold
                  </label>
                  <input
                    type="number"
                    value={newProduct.minimumStock}
                    onChange={(e) => setNewProduct({ ...newProduct, minimumStock: e.target.value })}
                    className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-white font-mono focus:border-[#4edea3] outline-none"
                  />
                </div>
              </div>

              <div>
                <label className="block text-[11px] font-semibold text-[#8f9194] uppercase tracking-wider mb-1">
                  Initial Warehouse Location
                </label>
                <select
                  value={newProduct.openingStockWarehouseId}
                  onChange={(e) => setNewProduct({ ...newProduct, openingStockWarehouseId: e.target.value })}
                  className="w-full px-2.5 py-1.5 bg-[#111316] border border-[#26292e] rounded text-white focus:border-[#4edea3] outline-none"
                >
                  {warehouses.map(w => (
                    <option key={w.id} value={w.id}>{w.name}</option>
                  ))}
                </select>
              </div>

              <div className="flex items-center justify-end gap-2 pt-3 border-t border-white/5">
                <button
                  type="button"
                  onClick={() => setIsAddProductOpen(false)}
                  className="px-3 py-1.5 rounded bg-[#282a2d] hover:bg-[#333538] text-[#8f9194] hover:text-white"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={isSubmittingProduct}
                  className="px-4 py-1.5 rounded bg-[#ffffff] hover:bg-[#e2e2e4] text-[#111316] font-bold uppercase tracking-wider shadow-md"
                >
                  {isSubmittingProduct ? 'Registering...' : 'Register Product'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Stock Adjust Modal */}
      <StockAdjustModal
        isOpen={isAdjustOpen}
        onClose={() => setIsAdjustOpen(false)}
        initialProduct={activeAdjustProduct}
      />

      {/* Stock Transfer Modal */}
      <StockTransferModal
        isOpen={isTransferOpen}
        onClose={() => setIsTransferOpen(false)}
        initialProduct={activeAdjustProduct}
      />

      {/* Warehouse Modal */}
      <WarehouseModal
        isOpen={isWarehouseModalOpen}
        onClose={() => setIsWarehouseModalOpen(false)}
      />
    </div>
  );
};
