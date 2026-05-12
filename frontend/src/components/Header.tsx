import { format } from 'date-fns';
import { ar } from 'date-fns/locale';
import { useLocation } from 'react-router-dom';
import { Activity, Droplets } from 'lucide-react';
import { cn } from '../lib/utils';
import { useQuery } from '@tanstack/react-query';
import { getBasinState } from '../api/basin';

const pageTitles: Record<string, string> = {
  '/': 'Dashboard',
  '/basin': 'Basin Management',
  '/sales': 'Sales Ledger',
  '/expenses': 'Expense Tracking',
  '/workers': 'Worker Directory',
  '/attendance': 'Attendance Log',
  '/monthly': 'Monthly Summary',
  '/production': 'Production Log',
};

export function Header() {
  const location = useLocation();
  const title = pageTitles[location.pathname] || 'Dashboard';
  
  const { data: basin } = useQuery({
    queryKey: ['basin'],
    queryFn: getBasinState,
    refetchInterval: 30000,
  });

  const basinPercentage = basin ? (basin.currentStock / basin.maxCapacity) * 100 : 0;
  
  const getBasinColor = (pct: number) => {
    if (pct > 60) return 'text-green-500 bg-green-500/10 border-green-500/20';
    if (pct > 30) return 'text-amber-500 bg-amber-500/10 border-amber-500/20';
    return 'text-red-500 bg-red-500/10 border-red-500/20';
  };

  return (
    <header className="h-16 border-b border-slate-200 bg-white/80 backdrop-blur-md sticky top-0 z-40 px-8 flex items-center justify-between">
      <div className="flex items-center gap-4">
        <h2 className="text-lg font-semibold text-slate-900">{title}</h2>
        <div className="h-4 w-px bg-slate-200 mx-2" />
        <p className="text-sm text-slate-500">
          {format(new Date(), 'EEEE, d MMMM yyyy', { locale: ar })}
        </p>
      </div>

      <div className="flex items-center gap-6">
        <div className={cn(
          "flex items-center gap-2 px-3 py-1.5 rounded-full border text-xs font-medium transition-colors",
          getBasinColor(basinPercentage)
        )}>
          <Droplets className="w-3.5 h-3.5" />
          <span>Basin: {Math.round(basinPercentage)}%</span>
        </div>

        <div className="flex items-center gap-2 px-3 py-1.5 rounded-full border border-green-500/20 bg-green-500/10 text-green-600 text-xs font-medium">
          <Activity className="w-3.5 h-3.5" />
          <span>System Online</span>
        </div>
      </div>
    </header>
  );
}
