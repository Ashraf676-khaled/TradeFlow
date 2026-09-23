import React from 'react';

interface BrandLogoProps {
  language?: 'en' | 'ar';
  size?: 'sm' | 'md' | 'lg';
  showBadge?: boolean;
  showSubtitle?: boolean;
  onClick?: () => void;
  className?: string;
}

export const BrandLogo: React.FC<BrandLogoProps> = ({
  language = 'ar',
  size = 'md',
  showBadge = true,
  showSubtitle = true,
  onClick,
  className = '',
}) => {
  const isAr = language === 'ar';

  const iconSizes = {
    sm: 'w-7 h-7',
    md: 'w-8 h-8',
    lg: 'w-10 h-10',
  };

  const titleSizes = {
    sm: 'text-sm',
    md: 'text-base',
    lg: 'text-xl',
  };

  return (
    <div
      onClick={onClick}
      className={`flex items-center gap-2.5 select-none ${onClick ? 'cursor-pointer' : ''} ${className}`}
      dir={isAr ? 'rtl' : 'ltr'}
    >
      {/* Institutional Geometric Emblem */}
      <div
        className={`${iconSizes[size]} shrink-0 rounded-lg bg-gradient-to-br from-[#1c1e22] via-[#141619] to-[#0d0e11] border border-[#2e3238] flex items-center justify-center shadow-[0_2px_8px_rgba(0,0,0,0.4)] relative group transition-transform hover:scale-105`}
      >
        <svg viewBox="0 0 24 24" fill="none" className="w-5 h-5">
          {/* Hexagonal trading node */}
          <path
            d="M12 3L19.5 7.33V16.67L12 21L4.5 16.67V7.33L12 3Z"
            stroke="#4EDEA3"
            strokeWidth="1.5"
            strokeLinejoin="round"
            className="opacity-90"
          />
          {/* Dynamic trade flow arrows */}
          <path
            d="M8.5 14L12 9.5L15.5 14"
            stroke="#FFFFFF"
            strokeWidth="1.8"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M10 12.5H14"
            stroke="#9CA3AF"
            strokeWidth="1.5"
            strokeLinecap="round"
          />
          <circle cx="12" cy="12" r="1.5" fill="#10B981" />
        </svg>
        <span className="absolute -top-0.5 -right-0.5 w-2 h-2 rounded-full bg-[#10b981] ring-2 ring-[#111316] animate-pulse" />
      </div>

      {/* Brand Title & Regional Localization */}
      <div className="flex flex-col min-w-0 leading-tight">
        <div className="flex items-center gap-1.5 flex-wrap">
          {isAr ? (
            <>
              <span className={`${titleSizes[size]} font-extrabold text-white tracking-tight`}>
                تريد<span className="text-[#4edea3]">فلو</span>
              </span>
              <span className="text-[10px] font-mono text-[#8f9194] px-1 bg-[#1a1c1f] rounded border border-white/5">
                TradeFlow
              </span>
            </>
          ) : (
            <>
              <span className={`${titleSizes[size]} font-extrabold text-white tracking-tight`}>
                Trade<span className="text-[#4edea3]">Flow</span>
              </span>
              <span className="text-[10px] font-mono text-[#8f9194] px-1 bg-[#1a1c1f] rounded border border-white/5">
                ERP
              </span>
            </>
          )}

          {showBadge && (
            <span className="px-1.5 py-0.5 rounded text-[9px] font-bold tracking-wider uppercase bg-[#1e2023] text-[#4edea3] border border-[#4edea3]/25 shadow-sm">
              {isAr ? 'مصر PRIME' : 'PRIME'}
            </span>
          )}
        </div>

        {showSubtitle && (
          <span className="text-[11px] text-[#8f9194] truncate font-medium mt-0.5">
            {isAr ? 'منظومة التجارة والمخازن المتكاملة' : 'Enterprise Trading & Logistics'}
          </span>
        )}
      </div>
    </div>
  );
};
