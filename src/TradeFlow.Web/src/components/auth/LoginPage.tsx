import React, { useState } from 'react';
import { Layers, Lock, Mail, AlertCircle, ArrowLeft, User, UserPlus, LogIn } from 'lucide-react';
import { authService, UserSession } from '../../services/authService';

interface LoginPageProps {
  onLoginSuccess: (session: UserSession) => void;
}

type AuthMode = 'login' | 'register';

export const LoginPage: React.FC<LoginPageProps> = ({ onLoginSuccess }) => {
  const [mode, setMode] = useState<AuthMode>('login');

  // Login state
  const [loginEmail, setLoginEmail] = useState('admin@tradeflow.io');
  const [loginPassword, setLoginPassword] = useState('Password123!');

  // Register state
  const [regName, setRegName] = useState('');
  const [regCompanyName, setRegCompanyName] = useState('');
  const [regEmail, setRegEmail] = useState('');
  const [regPassword, setRegPassword] = useState('');
  const [regConfirmPassword, setRegConfirmPassword] = useState('');

  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setErrorMessage(null);

    try {
      const session = await authService.login(loginEmail, loginPassword);
      onLoginSuccess(session);
    } catch (err: any) {
      if (err.response?.data?.title) {
        setErrorMessage(err.response.data.title);
      } else if (err.response?.status === 401) {
        setErrorMessage('البريد الإلكتروني أو كلمة المرور غير صحيحة.');
      } else {
        setErrorMessage('تعذر الاتصال بالنظام. يرجى التحقق من اتصال الشبكة وإعادة المحاولة.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);
    setSuccessMessage(null);

    if (!regName.trim()) {
      setErrorMessage('يرجى إدخال الاسم الكامل.');
      return;
    }
    if (!regCompanyName.trim()) {
      setErrorMessage('يرجى إدخال اسم الشركة أو المؤسسة.');
      return;
    }
    if (regPassword !== regConfirmPassword) {
      setErrorMessage('كلمة المرور وتأكيدها غير متطابقتان.');
      return;
    }
    if (regPassword.length < 8) {
      setErrorMessage('يجب أن تتكون كلمة المرور من 8 أحرف على الأقل.');
      return;
    }
    if (!/[A-Z]/.test(regPassword) || !/[0-9]/.test(regPassword)) {
      setErrorMessage('يجب أن تحتوي كلمة المرور على حرف كبير (A-Z) ورقم (0-9) على الأقل.');
      return;
    }

    setIsLoading(true);

    try {
      const session = await authService.register(regEmail, regPassword, regName, regCompanyName);
      onLoginSuccess(session);
    } catch (err: any) {
      if (err.response?.data?.errors) {
        const errs = Object.values(err.response.data.errors as Record<string, string[]>).flat();
        setErrorMessage(errs.join(' | '));
      } else if (err.response?.data?.title) {
        setErrorMessage(err.response.data.title);
      } else if (err.response?.data?.message) {
        setErrorMessage(err.response.data.message);
      } else if (err.response?.status === 400) {
        setErrorMessage('البيانات المدخلة غير صحيحة. يرجى مراجعة المعلومات وإعادة المحاولة.');
      } else if (err.response?.status === 409) {
        setErrorMessage('هذا البريد الإلكتروني مسجل بالفعل في النظام. يرجى تسجيل الدخول.');
      } else {
        setErrorMessage('تعذر الاتصال بالنظام. يرجى التحقق من اتصال الشبكة وإعادة المحاولة.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  const switchMode = (newMode: AuthMode) => {
    setMode(newMode);
    setErrorMessage(null);
    setSuccessMessage(null);
  };

  return (
    <div
      className="min-h-screen w-full bg-slate-50 flex items-center justify-center p-4 font-sans text-right"
      dir="rtl"
    >
      <div className="w-full max-w-md">

        {/* Brand Header */}
        <div className="text-center mb-6 space-y-2">
          <div className="w-14 h-14 rounded-2xl bg-blue-700 text-white flex items-center justify-center mx-auto shadow-lg">
            <Layers className="w-8 h-8" />
          </div>
          <h1 className="text-2xl font-black text-slate-900 tracking-tight">
              تريد<span className="text-blue-700">فلو</span>
          </h1>
          <p className="text-xs text-slate-500 font-semibold">
            نظام إدارة المبيعات والمخزون والفواتير للشركات والمؤسسات
          </p>
        </div>

        {/* Auth Card */}
        <div className="bg-white border border-slate-200 rounded-3xl shadow-xl overflow-hidden">

          {/* Tab Switcher */}
          <div className="flex border-b border-slate-100">
            <button
              type="button"
              onClick={() => switchMode('login')}
              className={`flex-1 flex items-center justify-center gap-2 py-3.5 text-xs font-bold transition-all ${
                mode === 'login'
                  ? 'bg-white text-blue-700 border-b-2 border-blue-600'
                  : 'bg-slate-50 text-slate-500 hover:text-slate-700'
              }`}
            >
              <LogIn className="w-4 h-4" />
              تسجيل الدخول
            </button>
            <button
              type="button"
              onClick={() => switchMode('register')}
              className={`flex-1 flex items-center justify-center gap-2 py-3.5 text-xs font-bold transition-all ${
                mode === 'register'
                  ? 'bg-white text-blue-700 border-b-2 border-blue-600'
                  : 'bg-slate-50 text-slate-500 hover:text-slate-700'
              }`}
            >
              <UserPlus className="w-4 h-4" />
              إنشاء حساب جديد
            </button>
          </div>

          <div className="p-8 space-y-5">

            {/* Alert Area */}
            {errorMessage && (
              <div className="p-3.5 rounded-xl bg-rose-50 border border-rose-200 text-rose-700 text-xs flex items-start gap-2 font-bold">
                <AlertCircle className="w-4 h-4 text-rose-600 shrink-0 mt-0.5" />
                <span>{errorMessage}</span>
              </div>
            )}
            {successMessage && (
              <div className="p-3.5 rounded-xl bg-emerald-50 border border-emerald-200 text-emerald-700 text-xs font-bold">
                {successMessage}
              </div>
            )}

            {/* ─── LOGIN FORM ─── */}
            {mode === 'login' && (
              <form onSubmit={handleLogin} className="space-y-4 text-xs">
                <div>
                  <label className="block font-bold text-slate-700 mb-1.5">البريد الإلكتروني</label>
                  <div className="relative">
                    <Mail className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                    <input
                      type="email"
                      required
                      value={loginEmail}
                      onChange={(e) => setLoginEmail(e.target.value)}
                      placeholder="name@company.com"
                      className="w-full pr-9 pl-4 py-2.5 text-xs rounded-xl bg-slate-50 border border-slate-200 text-slate-900 font-medium focus:outline-none focus:ring-2 focus:ring-blue-500 focus:bg-white transition"
                    />
                  </div>
                </div>

                <div>
                  <label className="block font-bold text-slate-700 mb-1.5">كلمة المرور</label>
                  <div className="relative">
                    <Lock className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                    <input
                      type="password"
                      required
                      value={loginPassword}
                      onChange={(e) => setLoginPassword(e.target.value)}
                      placeholder="••••••••"
                      className="w-full pr-9 pl-4 py-2.5 text-xs rounded-xl bg-slate-50 border border-slate-200 text-slate-900 font-medium focus:outline-none focus:ring-2 focus:ring-blue-500 focus:bg-white transition"
                    />
                  </div>
                </div>

                <button
                  type="submit"
                  disabled={isLoading}
                  className="w-full py-3 px-4 rounded-xl bg-blue-600 hover:bg-blue-700 disabled:opacity-60 text-white font-bold text-xs shadow-md transition flex items-center justify-center gap-2 mt-2"
                >
                  {isLoading ? (
                    <>
                      <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                      <span>جاري تسجيل الدخول...</span>
                    </>
                  ) : (
                    <>
                      <span>تسجيل الدخول للنظام</span>
                      <ArrowLeft className="w-4 h-4" />
                    </>
                  )}
                </button>
              </form>
            )}

            {/* ─── REGISTER FORM ─── */}
            {mode === 'register' && (
              <form onSubmit={handleRegister} className="space-y-4 text-xs">
                <div>
                  <label className="block font-bold text-slate-700 mb-1.5">الاسم الكامل</label>
                  <div className="relative">
                    <User className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                    <input
                      type="text"
                      required
                      value={regName}
                      onChange={(e) => setRegName(e.target.value)}
                      placeholder="مثال: أحمد محمد الشريف"
                      className="w-full pr-9 pl-4 py-2.5 text-xs rounded-xl bg-slate-50 border border-slate-200 text-slate-900 font-medium focus:outline-none focus:ring-2 focus:ring-blue-500 focus:bg-white transition"
                    />
                  </div>
                </div>

                <div>
                  <label className="block font-bold text-slate-700 mb-1.5">اسم الشركة / المؤسسة</label>
                  <div className="relative">
                    <svg xmlns="http://www.w3.org/2000/svg" className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>
                    <input
                      type="text"
                      required
                      value={regCompanyName}
                      onChange={(e) => setRegCompanyName(e.target.value)}
                      placeholder="مثال: شركة تريد فلو للتجارة"
                      className="w-full pr-9 pl-4 py-2.5 text-xs rounded-xl bg-slate-50 border border-slate-200 text-slate-900 font-medium focus:outline-none focus:ring-2 focus:ring-blue-500 focus:bg-white transition"
                    />
                  </div>
                </div>

                <div>
                  <label className="block font-bold text-slate-700 mb-1.5">البريد الإلكتروني</label>
                  <div className="relative">
                    <Mail className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                    <input
                      type="email"
                      required
                      value={regEmail}
                      onChange={(e) => setRegEmail(e.target.value)}
                      placeholder="name@company.com"
                      className="w-full pr-9 pl-4 py-2.5 text-xs rounded-xl bg-slate-50 border border-slate-200 text-slate-900 font-medium focus:outline-none focus:ring-2 focus:ring-blue-500 focus:bg-white transition"
                    />
                  </div>
                </div>

                <div>
                  <label className="block font-bold text-slate-700 mb-1.5">كلمة المرور</label>
                  <div className="relative">
                    <Lock className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                    <input
                      type="password"
                      required
                      minLength={8}
                      value={regPassword}
                      onChange={(e) => setRegPassword(e.target.value)}
                      placeholder="8 أحرف على الأقل (تشمل A-Z و 0-9)"
                      className="w-full pr-9 pl-4 py-2.5 text-xs rounded-xl bg-slate-50 border border-slate-200 text-slate-900 font-medium focus:outline-none focus:ring-2 focus:ring-blue-500 focus:bg-white transition"
                    />
                  </div>
                </div>

                <div>
                  <label className="block font-bold text-slate-700 mb-1.5">تأكيد كلمة المرور</label>
                  <div className="relative">
                    <Lock className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                    <input
                      type="password"
                      required
                      value={regConfirmPassword}
                      onChange={(e) => setRegConfirmPassword(e.target.value)}
                      placeholder="أعد إدخال كلمة المرور"
                      className="w-full pr-9 pl-4 py-2.5 text-xs rounded-xl bg-slate-50 border border-slate-200 text-slate-900 font-medium focus:outline-none focus:ring-2 focus:ring-blue-500 focus:bg-white transition"
                    />
                  </div>
                </div>

                <button
                  type="submit"
                  disabled={isLoading}
                  className="w-full py-3 px-4 rounded-xl bg-blue-600 hover:bg-blue-700 disabled:opacity-60 text-white font-bold text-xs shadow-md transition flex items-center justify-center gap-2 mt-2"
                >
                  {isLoading ? (
                    <>
                      <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                      <span>جاري إنشاء الحساب...</span>
                    </>
                  ) : (
                    <>
                      <UserPlus className="w-4 h-4" />
                      <span>إنشاء الحساب والدخول للنظام</span>
                    </>
                  )}
                </button>
              </form>
            )}

          </div>
        </div>

        <p className="text-center text-[11px] text-slate-400 mt-5">
          منصة موحدة لإدارة عمليات التجارة
        </p>

      </div>
    </div>
  );
};
