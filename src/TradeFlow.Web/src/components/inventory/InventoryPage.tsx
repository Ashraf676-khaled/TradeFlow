import React, { useEffect, useMemo, useState } from 'react';
import { 
  Package, 
  Search, 
  Warehouse as WarehouseIcon, 
  AlertTriangle, 
  RefreshCw,
  LayoutGrid,
  List,
  MapPin,
  CheckCircle2,
  XCircle
  ,FileSpreadsheet
  ,Plus
} from 'lucide-react';
import { useTenant } from '../../context/TenantContext';
import { Product } from '../../types';
import { StockAdjustModal } from './StockAdjustModal';
import { WarehouseModal } from './WarehouseModal';
import { stockService } from '../../services/stockService';
import { getApiErrorMessage } from '../../services/apiClient';
import * as XLSX from 'xlsx';

export const InventoryPage: React.FC = () => {
  const { products, warehouses, currencySymbol, createProduct, settings } = useTenant();

  const [selectedWarehouse, setSelectedWarehouse] = useState<string>('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [viewMode, setViewMode] = useState<'table' | 'grid'>('table');
  const [isAdjustOpen, setIsAdjustOpen] = useState(false);
  const [activeAdjustProduct, setActiveAdjustProduct] = useState<Product | null>(null);
  const [isProductFormOpen, setIsProductFormOpen] = useState(false);
  const [isImporting, setIsImporting] = useState(false);
  const [stockProductIds, setStockProductIds] = useState<string[] | null>(null);
  const [productForm, setProductForm] = useState({ sku: '', name: '', unitPrice: '', costPrice: '', minimumStock: '5', openingStock: '0', warehouseId: '' });
  const [formError, setFormError] = useState('');
  const [openGroups, setOpenGroups] = useState<Record<string, boolean>>({});
  const [isWarehouseFormOpen, setIsWarehouseFormOpen] = useState(false);

  useEffect(() => {
    let cancelled = false;
    if (selectedWarehouse === 'all') {
      setStockProductIds(null);
      return;
    }
    stockService.getStockByWarehouse(selectedWarehouse, 1, 500)
      .then(items => {
        if (!cancelled) setStockProductIds(items.map((item: { productId: string }) => item.productId));
      })
      .catch(() => { if (!cancelled) setStockProductIds([]); });
    return () => { cancelled = true; };
  }, [selectedWarehouse]);

  useEffect(() => {
    if (selectedWarehouse === 'all' && warehouses.length > 0) {
      const defaultWarehouse = warehouses.find(warehouse => warehouse.name?.includes('رئيسي')) || warehouses[0];
      setSelectedWarehouse(defaultWarehouse.id);
    }
  }, [warehouses, selectedWarehouse]);

  const filteredProducts = products.filter(p => {
    const matchesWh = selectedWarehouse === 'all' || p.warehouseId === selectedWarehouse || Boolean(stockProductIds?.includes(p.id));
    const q = searchTerm.toLowerCase();
    const name = (p.name ?? '').toLowerCase();
    const sku = (p.sku ?? '').toLowerCase();
    const category = (p.category ?? '').toLowerCase();
    return matchesWh && (name.includes(q) || sku.includes(q) || category.includes(q));
  });

  const productGroups = useMemo(() => {
    return filteredProducts.reduce<Record<string, Product[]>>((groups, product) => {
      const sku = (product.sku || '').trim().toUpperCase();
      const groupName = sku.includes('-') ? sku.split('-')[0] : (sku.charAt(0) || 'أصناف أخرى');
      (groups[groupName] ||= []).push(product);
      return groups;
    }, {});
  }, [filteredProducts]);

  const sortedGroups = Object.entries(productGroups).sort(([first], [second]) => first.localeCompare(second));

  const saveProduct = async (event: React.FormEvent) => {
    event.preventDefault();
    setFormError('');
    try {
      const unitPrice = Number(productForm.unitPrice);
      const costPrice = Number(productForm.costPrice);
      const minimumStock = Number(productForm.minimumStock);
      const openingStock = Number(productForm.openingStock || 0);
      if (!productForm.sku.trim() || !productForm.name.trim() || !Number.isFinite(unitPrice) || unitPrice <= 0 || !Number.isFinite(costPrice) || costPrice < 0 || !Number.isFinite(minimumStock) || minimumStock < 0 || !Number.isFinite(openingStock) || openingStock < 0) {
        throw new Error('يرجى إدخال رمز الصنف والبيانات الرقمية بشكل صحيح.');
      }
      await createProduct({
        sku: productForm.sku.trim(),
        name: productForm.name.trim(),
        unitPrice,
        costPrice,
        minimumStock,
        // Optional opening stock: booked into the selected warehouse (or the
        // auto-created default warehouse when none is chosen) in the same save.
        openingStockQuantity: openingStock,
        openingStockWarehouseId: productForm.warehouseId || undefined,
      });
      setProductForm({ sku: '', name: '', unitPrice: '', costPrice: '', minimumStock: '5', openingStock: '0', warehouseId: '' });
      setIsProductFormOpen(false);
    } catch (error) {
      setFormError(getApiErrorMessage(error, 'تعذر حفظ الصنف.'));
    }
  };

  const importProducts = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    event.target.value = '';
    if (!file) return;
    setIsImporting(true);
    setFormError('');
    try {
      const workbook = XLSX.read(await file.arrayBuffer(), { type: 'array' });
      const rows = XLSX.utils.sheet_to_json<Record<string, unknown>>(workbook.Sheets[workbook.SheetNames[0]], { defval: '' });
      for (const row of rows) {
        const value = (keys: string[]) => keys.map(key => row[key]).find(item => item !== undefined && item !== '') as string | number | undefined;
        await createProduct({
          sku: String(value(['SKU', 'sku', 'رمز الصنف']) || '').trim(),
          name: String(value(['Name', 'name', 'اسم الصنف']) || '').trim(),
          unitPrice: Number(value(['SellingPrice', 'sellingPrice', 'سعر البيع']) || 0),
          costPrice: Number(value(['Cost', 'cost', 'التكلفة']) || 0),
          minimumStock: Number(value(['MinimumStock', 'minimumStock', 'حد إعادة الطلب']) || 5),
        });
      }
    } catch (error) {
      setFormError(getApiErrorMessage(error, 'تعذر استيراد ملف المنتجات.'));
    } finally {
      setIsImporting(false);
    }
  };

  const safetyThreshold = (p: Product) =>
    (p.minStockLevel && p.minStockLevel > 0 ? p.minStockLevel : (settings.lowStockThreshold > 0 ? settings.lowStockThreshold : 5));

  // Live safety-threshold state based on the AVAILABLE quantity.
  const stockState = (p: Product): 'out' | 'low' | 'ok' => {
    const available = p.availableStock ?? p.currentStock ?? 0;
    if (available <= 0) return 'out';
    return available <= safetyThreshold(p) ? 'low' : 'ok';
  };

  const lowStockCount = filteredProducts.filter(p => stockState(p) !== 'ok').length;
  const totalValuation = products.reduce((sum, p) => sum + ((p.currentStock ?? 0) * (p.unitPrice ?? 0)), 0);

  const getStatusBadge = (status: Product['status']) => {
    switch (status) {
      case 'متوفر':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-emerald-100 text-emerald-800 border border-emerald-200">
            <CheckCircle2 className="w-3 h-3" /> متوفر
          </span>
        );
      case 'مخزون منخفض':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-amber-100 text-amber-800 border border-amber-200">
            <AlertTriangle className="w-3 h-3 text-amber-600" /> مخزون منخفض
          </span>
        );
      case 'نفد المخزون':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-rose-100 text-rose-800 border border-rose-200">
            <XCircle className="w-3 h-3" /> نفد المخزون
          </span>
        );
      default:
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-slate-100 text-slate-700">
            {status || 'متوفر'}
          </span>
        );
    }
  };

  return (
    <div className="space-y-6 animate-in fade-in duration-300">
      
      {/* Title Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Package className="w-5 h-5 text-blue-700" /> إدارة المخزون والمستودعات
          </h1>
          <p className="text-xs text-slate-500">
            متابعة فورية لقيمة المخزون، الحدود الدنيا للأمان، وأماكن التخزين عبر الفروع.
          </p>
        </div>

        <div className="flex flex-wrap gap-2">
          <button onClick={() => { setProductForm(current => ({ ...current, warehouseId: current.warehouseId || warehouses.find(w => w.name?.includes('رئيسي'))?.id || warehouses[0]?.id || '', minimumStock: current.minimumStock === '5' || current.minimumStock === '' ? String(settings.lowStockThreshold > 0 ? settings.lowStockThreshold : 5) : current.minimumStock })); setIsProductFormOpen(true); }} className="flex items-center gap-2 rounded-xl bg-blue-600 px-4 py-2.5 text-xs font-bold text-white hover:bg-blue-700"><Plus className="h-4 w-4" /> إضافة صنف</button>
          <button
            type="button"
            onClick={() => setIsWarehouseFormOpen(true)}
            className="flex items-center gap-2 rounded-xl border border-indigo-200 bg-white px-4 py-2.5 text-xs font-bold text-indigo-700 hover:bg-indigo-50"
          >
            <WarehouseIcon className="h-4 w-4" /> إضافة مخزن
          </button>
          <label className="flex cursor-pointer items-center gap-2 rounded-xl border border-blue-200 bg-white px-4 py-2.5 text-xs font-bold text-blue-700 hover:bg-blue-50"><FileSpreadsheet className="h-4 w-4" /> {isImporting ? 'جارٍ الاستيراد' : 'استيراد المنتجات'}<input type="file" accept=".xlsx,.xls,.csv" className="hidden" onChange={importProducts} disabled={isImporting} /></label>
          <button onClick={() => { setActiveAdjustProduct(null); setIsAdjustOpen(true); }} className="flex items-center justify-center gap-2 rounded-xl bg-slate-100 px-4 py-2.5 text-xs font-bold text-slate-700 hover:bg-slate-200"><RefreshCw className="h-4 w-4" /> توريد مخزني</button>
        </div>
      </div>

      {isProductFormOpen && <form onSubmit={saveProduct} className="space-y-3 rounded-2xl border border-blue-200 bg-white p-5 shadow-sm">
        <div className="flex items-center justify-between"><h2 className="font-bold">إضافة صنف جديد</h2><button type="button" onClick={() => setIsProductFormOpen(false)}>إغلاق</button></div>
        <div className="grid grid-cols-1 gap-3 md:grid-cols-4">
          <div className="space-y-1"><label className="block text-[10px] font-bold text-slate-600">رمز الصنف (SKU)</label><input required placeholder="مثال: BR-001" value={productForm.sku} onChange={e => setProductForm({ ...productForm, sku: e.target.value })} className="w-full rounded-xl border border-slate-200 px-3 py-2 text-xs" /></div>
          <div className="space-y-1"><label className="block text-[10px] font-bold text-slate-600">اسم الصنف</label><input required placeholder="مثال: شيبسى" value={productForm.name} onChange={e => setProductForm({ ...productForm, name: e.target.value })} className="w-full rounded-xl border border-slate-200 px-3 py-2 text-xs" /></div>
          <div className="space-y-1"><label className="block text-[10px] font-bold text-slate-600">سعر البيع</label><input required min="0" type="number" placeholder="0.00" value={productForm.unitPrice} onChange={e => setProductForm({ ...productForm, unitPrice: e.target.value })} className="w-full rounded-xl border border-slate-200 px-3 py-2 text-xs" /></div>
          <div className="space-y-1"><label className="block text-[10px] font-bold text-slate-600">التكلفة</label><input required min="0" type="number" placeholder="0.00" value={productForm.costPrice} onChange={e => setProductForm({ ...productForm, costPrice: e.target.value })} className="w-full rounded-xl border border-slate-200 px-3 py-2 text-xs" /></div>
          <div className="space-y-1"><label className="block text-[10px] font-bold text-slate-600">حد إعادة الطلب (عدد الوحدات)</label><input required min="0" type="number" placeholder="مثال: 5" value={productForm.minimumStock} onChange={e => setProductForm({ ...productForm, minimumStock: e.target.value })} className="w-full rounded-xl border border-slate-200 px-3 py-2 text-xs" /></div>
          <div className="space-y-1"><label className="block text-[10px] font-bold text-slate-600">الكمية الافتتاحية (اختياري)</label><input min="0" type="number" placeholder="0" value={productForm.openingStock} onChange={e => setProductForm({ ...productForm, openingStock: e.target.value })} className="w-full rounded-xl border border-slate-200 px-3 py-2 text-xs" /></div>
          <div className="space-y-1"><label className="block text-[10px] font-bold text-slate-600">مستودع الرصيد الافتتاحي</label><select value={productForm.warehouseId} onChange={e => setProductForm({ ...productForm, warehouseId: e.target.value })} className="w-full rounded-xl border border-slate-200 bg-white px-3 py-2 text-xs">{warehouses.length === 0 ? <option value="">سيتم استخدام المستودع الافتراضي تلقائيًا</option> : warehouses.map(w => (<option key={w.id} value={w.id}>{w.name}</option>))}</select></div>
        </div>
        {formError && <p className="text-xs font-bold text-rose-600">{formError}</p>}
        <button type="submit" className="rounded-xl bg-blue-600 px-4 py-2 text-xs font-bold text-white">حفظ الصنف</button>
      </form>}

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="p-4 rounded-2xl bg-white border border-slate-200 shadow-2xs flex items-center justify-between">
          <div>
            <p className="text-xs text-slate-500 font-bold">إجمالي قيمة المخزون الحالية</p>
            <h3 className="text-xl font-extrabold text-slate-900 mt-1">
              {totalValuation.toLocaleString(undefined, { minimumFractionDigits: 2 })} <span className="text-xs font-normal text-slate-500">{currencySymbol}</span>
            </h3>
          </div>
          <div className="p-3 rounded-xl bg-blue-50 text-blue-700">
            <Package className="w-5 h-5" />
          </div>
        </div>

        <div className="p-4 rounded-2xl bg-white border border-slate-200 shadow-2xs flex items-center justify-between">
          <div>
            <p className="text-xs text-slate-500 font-bold">عدد المستودعات النشطة</p>
            <h3 className="text-xl font-extrabold text-slate-900 mt-1">
              {warehouses.length} <span className="text-xs font-normal text-slate-500">مخازن</span>
            </h3>
          </div>
          <div className="p-3 rounded-xl bg-indigo-50 text-indigo-700">
            <WarehouseIcon className="w-5 h-5" />
          </div>
        </div>

        <div className="p-4 rounded-2xl bg-white border border-slate-200 shadow-2xs flex items-center justify-between">
          <div>
            <p className="text-xs text-slate-500 font-bold">تنبيهات نواقص المخزون</p>
            <h3 className={`text-xl font-extrabold mt-1 ${lowStockCount > 0 ? 'text-rose-700' : 'text-emerald-700'}`}>
              {lowStockCount} <span className="text-xs font-normal text-slate-500">أصناف تحتاج إعادة طلب</span>
            </h3>
            <p className={`text-[10px] mt-1 font-bold ${lowStockCount > 0 ? 'text-rose-600' : 'text-emerald-600'}`}>
              {lowStockCount > 0 ? '⚠ أصناف بلغت حد الأمان أو نفدت' : 'كل الأصناف أعلى من حد الأمان'}
            </p>
          </div>
          <div className={`p-3 rounded-xl ${lowStockCount > 0 ? 'bg-amber-50 text-amber-600' : 'bg-emerald-50 text-emerald-600'}`}>
            <AlertTriangle className="w-5 h-5" />
          </div>
        </div>
      </div>

      {/* Filter Toolbar */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 p-4 rounded-2xl bg-white border border-slate-200 shadow-sm">
        
        {/* Warehouse Dropdown */}
        <div className="flex items-center gap-2">
          <label className="text-xs font-bold text-slate-600 flex items-center gap-1">
            <WarehouseIcon className="w-3.5 h-3.5" /> المستودع:
          </label>
          <select
            value={selectedWarehouse}
            onChange={(e) => setSelectedWarehouse(e.target.value)}
            className="px-3 py-1.5 text-xs rounded-xl bg-slate-50 border border-slate-200 text-slate-900 font-bold"
          >
            <option value="all">جميع المستودعات ({products.length} أصناف)</option>
            {warehouses.map(w => (
              <option key={w.id} value={w.id}>{w.name}</option>
            ))}
          </select>
        </div>

        {/* Search & Layout Toggle */}
        <div className="flex items-center gap-3">
          <div className="relative w-full md:w-64">
            <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="البحث باسم الصنف أو الفئة..."
              className="w-full pr-9 pl-3 py-1.5 text-xs rounded-xl bg-slate-50 border border-slate-200 text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div className="flex items-center p-1 rounded-xl bg-slate-100">
            <button
              onClick={() => setViewMode('table')}
              className={`p-1.5 rounded-lg transition ${
                viewMode === 'table' ? 'bg-white text-blue-700 shadow-xs' : 'text-slate-500'
              }`}
              title="عرض كجدول"
            >
              <List className="w-4 h-4" />
            </button>
            <button
              onClick={() => setViewMode('grid')}
              className={`p-1.5 rounded-lg transition ${
                viewMode === 'grid' ? 'bg-white text-blue-700 shadow-xs' : 'text-slate-500'
              }`}
              title="عرض كبطاقات"
            >
              <LayoutGrid className="w-4 h-4" />
            </button>
          </div>
        </div>

      </div>

      {/* Data Table View */}
      {viewMode === 'table' ? (
        <div className="rounded-2xl bg-white border border-slate-200 shadow-sm overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-right text-xs">
              <thead>
                <tr className="border-b border-slate-200 text-slate-500 uppercase text-[10px] font-bold bg-slate-50">
                  <th className="py-3 px-4">رمز الصنف / اسم الصنف</th>
                  <th className="py-3 px-4">الفئة</th>
                  <th className="py-3 px-4">المستودع والموقع</th>
                  <th className="py-3 px-4 text-center">مستوى المخزون الحالي</th>
                  <th className="py-3 px-4 text-left">سعر الوحدة</th>
                  <th className="py-3 px-4 text-center">الحالة</th>
                  <th className="py-3 px-4 text-left">إجراء</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {sortedGroups.flatMap(([, groupProducts]) => groupProducts).map((prod) => {
                  const currentStock = prod.currentStock ?? 0;
                  const maxStock = prod.maxStockLevel || 100;
                  const minStock = safetyThreshold(prod);
                  const state = stockState(prod);
                  const stockPct = Math.min(100, Math.round((currentStock / maxStock) * 100));
                  return (
                    <tr key={prod.id} className={`transition hover:bg-slate-50 ${state === 'ok' ? '' : state === 'out' ? 'bg-rose-50/60' : 'bg-amber-50/60'}`}>
                      <td className="py-3.5 px-4 font-bold text-slate-900">
                        <div>{prod.name ?? '—'}</div>
                        <div className="text-[10px] font-mono text-blue-700">رمز: {prod.sku ?? '—'}</div>
                      </td>
                      <td className="py-3.5 px-4 text-slate-600">
                        {prod.category ?? '—'}
                      </td>
                      <td className="py-3.5 px-4 text-slate-600">
                        <div className="flex items-center gap-1">
                          <MapPin className="w-3 h-3 text-slate-400" /> {prod.warehouseName ?? 'المستودع الرئيسي'}
                        </div>
                        <div className="text-[10px] font-mono text-slate-400">{prod.binLocation ?? '—'}</div>
                      </td>
                      <td className="py-3.5 px-4">
                        <div className="w-32 mx-auto space-y-1">
                          <div className="flex justify-between text-[10px] font-mono">
                            <span className="font-bold text-slate-900">
                              {currentStock} {prod.unitOfMeasure ?? 'قطعة'}
                            </span>
                            <span className="text-slate-400">حد الأمان: {minStock}</span>
                          </div>
                          <div className="w-full bg-slate-200 h-1.5 rounded-full overflow-hidden">
                            <div 
                              className={`h-full rounded-full ${
                                state === 'out' ? 'bg-rose-500' : state === 'low' ? 'bg-amber-500' : 'bg-emerald-500'
                              }`}
                              style={{ width: `${stockPct}%` }}
                            />
                          </div>
                          {state !== 'ok' && (
                            <p className={`flex items-center gap-1 text-[10px] font-bold ${state === 'out' ? 'text-rose-600' : 'text-amber-600'}`}>
                              <AlertTriangle className="w-3 h-3" />
                              {state === 'out' ? 'نفد المخزون — أعد الطلب الآن' : `تحت حد الأمان (المتاح ${currentStock} / الحد ${minStock})`}
                            </p>
                          )}
                        </div>
                      </td>
                      <td className="py-3.5 px-4 text-left font-mono font-extrabold text-slate-900">
                        {(prod.unitPrice ?? 0).toFixed(2)} {currencySymbol}
                      </td>
                      <td className="py-3.5 px-4 text-center">
                        {getStatusBadge(state === 'out' ? 'نفد المخزون' : state === 'low' ? 'مخزون منخفض' : 'متوفر')}
                      </td>
                      <td className="py-3.5 px-4 text-left">
                        <button
                          onClick={() => {
                            setActiveAdjustProduct(prod);
                            setIsAdjustOpen(true);
                          }}
                          className="px-2.5 py-1 rounded-lg text-[11px] font-bold text-blue-700 hover:bg-blue-50 border border-blue-200 transition"
                        >
                          تعديل
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      ) : (
        /* Grid View */
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {sortedGroups.map(([groupName, groupProducts]) => (
            <section key={groupName} className="space-y-3">
              <button type="button" onClick={() => setOpenGroups(current => ({ ...current, [groupName]: !(current[groupName] ?? true) }))} className="flex w-full items-center justify-between rounded-xl border border-slate-200 bg-white px-4 py-3 text-right shadow-sm">
                <span className="font-bold text-slate-800">مجموعة {groupName} <span className="text-xs font-normal text-slate-500">({groupProducts.length} أصناف)</span></span>
                <span className="text-xs text-blue-700">{(openGroups[groupName] ?? true) ? 'إخفاء' : 'عرض'}</span>
              </button>
              {(openGroups[groupName] ?? true) && <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {groupProducts.map((prod) => {
            const state = stockState(prod);
            const available = prod.availableStock ?? prod.currentStock ?? 0;
            return (
            <div key={prod.id} className={`p-5 rounded-2xl bg-white border shadow-sm space-y-4 transition ${state === 'ok' ? 'border-slate-200 hover:border-blue-400' : state === 'out' ? 'border-rose-300 bg-rose-50/40' : 'border-amber-300 bg-amber-50/40'}`}>
              <div className="flex items-start justify-between">
                <div>
                  <span className="text-[10px] font-mono font-bold px-2 py-0.5 rounded bg-blue-50 text-blue-700">
                    رمز: {prod.sku ?? '—'}
                  </span>
                  <h3 className="font-bold text-slate-900 text-sm mt-1.5">
                    {prod.name ?? '—'}
                  </h3>
                </div>
                {getStatusBadge(state === 'out' ? 'نفد المخزون' : state === 'low' ? 'مخزون منخفض' : 'متوفر')}
              </div>

              <p className="text-xs text-slate-500 line-clamp-2">
                {prod.description ?? '—'}
              </p>

              <div className="p-3 rounded-xl bg-slate-50 border border-slate-100 space-y-2 text-xs">
                <div className="flex justify-between text-slate-600">
                  <span>المستودع</span>
                  <span className="font-bold text-slate-900">{prod.warehouseName ?? 'المستودع الرئيسي'}</span>
                </div>
                <div className="flex justify-between text-slate-600">
                  <span>مكان التخزين</span>
                  <span className="font-mono text-slate-700">{prod.binLocation ?? '—'}</span>
                </div>
                <div className="flex justify-between text-slate-600">
                  <span>المخزون المتاح</span>
                  <span className={`font-mono font-bold ${state === 'ok' ? 'text-emerald-700' : state === 'low' ? 'text-amber-700' : 'text-rose-700'}`}>{available} {prod.unitOfMeasure ?? 'قطعة'}</span>
                </div>
                {state !== 'ok' && (
                  <p className={`flex items-center gap-1 text-[10px] font-bold ${state === 'out' ? 'text-rose-600' : 'text-amber-600'}`}>
                    <AlertTriangle className="w-3 h-3" />
                    {state === 'out' ? 'نفد المخزون — أعد الطلب الآن' : 'تحت حد الأمان'}
                  </p>
                )}
              </div>

              <div className="flex items-center justify-between pt-2 border-t border-slate-100">
                <span className="font-mono font-extrabold text-slate-900 text-base">
                  {(prod.unitPrice ?? 0).toFixed(2)} {currencySymbol}
                </span>
                <button
                  onClick={() => {
                    setActiveAdjustProduct(prod);
                    setIsAdjustOpen(true);
                  }}
                  className="px-3 py-1.5 rounded-xl text-xs font-bold text-white bg-blue-600 hover:bg-blue-700 transition shadow-xs"
                >
                  تعديل الكمية
                </button>
              </div>
            </div>
            );
          })}
              </div>}
            </section>
          ))}
        </div>
      )}

      {/* Stock Adjust Modal */}
      <StockAdjustModal
        isOpen={isAdjustOpen}
        onClose={() => setIsAdjustOpen(false)}
        initialProduct={activeAdjustProduct}
      />

      {/* Add Warehouse Modal */}
      <WarehouseModal
        isOpen={isWarehouseFormOpen}
        onClose={() => setIsWarehouseFormOpen(false)}
      />

    </div>
  );
};
