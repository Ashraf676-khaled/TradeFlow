import React, { useState, useRef, useEffect } from 'react';
import { 
  Search, 
  ShieldCheck,
  LogOut,
  User
} from 'lucide-react';
import { useTenant } from '../../context/TenantContext';

interface HeaderProps {
}

export const Header: React.FC<HeaderProps> = () => {
  const { 
    userSession,
    logoutSession
  } = useTenant();

  const [isProfileOpen, setIsProfileOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const profileRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (profileRef.current && !profileRef.current.contains(e.target as Node)) {
        setIsProfileOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  return (
    <header className="sticky top-0 z-30 h-16 w-full border-b border-slate-200 bg-white/95 backdrop-blur-md shadow-2xs">
      <div className="flex items-center justify-between h-full px-4 md:px-6">
        
        {/* Right Section: Mobile Menu & Active Session Title */}
        <div className="flex items-center gap-3">
          <div className="hidden sm:block">
            <h2 className="text-sm font-bold text-slate-900">
              نظام تريد فلو لإدارة التجارة
            </h2>
            <p className="text-[10px] text-slate-500 font-semibold">
              جلسة عمل نشطة • بيئة الأعمال المعتمدة
            </p>
          </div>
        </div>

        {/* Center: Global Search Bar */}
        <div className="hidden md:flex items-center flex-1 max-w-md mx-6">
          <div className="relative w-full">
            <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="البحث الفوري في الأوامر والمنتجات..."
              className="w-full pr-9 pl-4 py-2 text-xs rounded-xl bg-slate-100 border border-slate-200 text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:bg-white transition"
            />
          </div>
        </div>

        {/* Left Section: User Profile & Logout */}
        <div className="flex items-center gap-3">
          <div className="relative" ref={profileRef}>
            <button
              onClick={() => setIsProfileOpen(!isProfileOpen)}
              className="flex items-center gap-2 p-1.5 rounded-xl hover:bg-slate-100 transition border border-slate-200"
            >
              <div className="w-8 h-8 rounded-xl bg-blue-700 text-white font-bold text-xs flex items-center justify-center shadow-xs">
                {userSession?.name?.substring(0, 1).toUpperCase() || 'U'}
              </div>
              <div className="text-right hidden lg:block pl-1">
                <p className="text-xs font-bold text-slate-900 leading-tight">
                  {userSession?.name || 'مدير النظام'}
                </p>
                <p className="text-[10px] text-slate-500 flex items-center gap-1">
                  <ShieldCheck className="w-3 h-3 text-blue-600" /> جلسة موثقة
                </p>
              </div>
            </button>

            {isProfileOpen && (
              <div className="absolute left-0 mt-2 w-56 rounded-2xl bg-white border border-slate-200 shadow-xl py-2 z-50 text-xs text-right animate-in fade-in">
                <div className="px-4 py-2 border-b border-slate-100">
                  <p className="font-bold text-slate-900">{userSession?.name}</p>
                  <p className="text-[10px] text-slate-500 font-mono">{userSession?.email}</p>
                </div>
                <div className="py-1">
                  <button 
                    onClick={logoutSession}
                    className="w-full text-right px-4 py-2 text-rose-600 hover:bg-rose-50 font-bold flex items-center gap-2"
                  >
                    <LogOut className="w-4 h-4" />
                    <span>تسجيل الخروج</span>
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>

      </div>
    </header>
  );
};
