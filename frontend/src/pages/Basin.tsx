import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { 
  Droplets, 
  Settings2, 
  History, 
  PlusCircle, 
  Timer,
  LayoutGrid,
  Unlock
} from 'lucide-react';
import { getBasinState, replenishBasin, updateFreezeHours, getProductionLog } from '../api/basin';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { cn } from '../lib/utils';
import { format } from 'date-fns';

export function Basin() {
  const queryClient = useQueryClient();
  const [isEditingFreeze, setIsEditingFreeze] = useState(false);
  const [newFreezeHours, setNewFreezeHours] = useState<number>(0);

  const { data: basin } = useQuery({ 
    queryKey: ['basin'], 
    queryFn: getBasinState 
  });

  const { data: productionLog } = useQuery({ 
    queryKey: ['production-log'], 
    queryFn: () => getProductionLog() 
  });

  const replenishMutation = useMutation({
    mutationFn: (blocks: number) => replenishBasin(blocks),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['basin'] }),
  });

  const freezeMutation = useMutation({
    mutationFn: (hours: number) => updateFreezeHours(hours),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['basin'] });
      setIsEditingFreeze(false);
    },
  });

  const basinPercentage = basin ? (basin.currentStock / basin.maxCapacity) * 100 : 0;
  const freeSlots = basin ? basin.maxCapacity - basin.currentStock : 0;

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-slate-900">Basin Inventory & Production</h1>
        <Button onClick={() => replenishMutation.mutate(50)} className="gap-2">
          <PlusCircle className="w-4 h-4" /> Manual Replenish (50)
        </Button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <Card className="lg:col-span-2 overflow-hidden border-none shadow-sm relative">
          <div className="absolute inset-0 bg-gradient-to-br from-primary/5 to-transparent pointer-events-none" />
          <CardContent className="p-8 space-y-8 relative">
            <div className="flex justify-between items-end">
              <div>
                <p className="text-sm font-medium text-slate-500">Current Stock Status</p>
                <h2 className="text-4xl font-bold text-slate-900 mt-1">{Math.round(basinPercentage)}%</h2>
              </div>
              <div className="text-right">
                <p className="text-sm font-medium text-slate-500">Last Updated</p>
                <p className="text-sm text-slate-900 font-bold">{basin?.lastUpdatedAt ? format(new Date(basin.lastUpdatedAt), 'hh:mm a') : 'N/A'}</p>
              </div>
            </div>

            <div className="relative h-12 w-full bg-slate-100 rounded-2xl p-1 border border-slate-200 overflow-hidden shadow-inner">
              <div 
                className={cn(
                  "h-full rounded-xl transition-all duration-1000 ease-out flex items-center justify-end px-4 relative",
                  basinPercentage > 60 ? "bg-gradient-to-r from-green-400 to-green-500 shadow-[0_0_20px_rgba(34,197,94,0.3)]" : 
                  basinPercentage > 30 ? "bg-gradient-to-r from-amber-400 to-amber-500 shadow-[0_0_20px_rgba(245,158,11,0.3)]" : 
                  "bg-gradient-to-r from-red-400 to-red-500 shadow-[0_0_20px_rgba(239,68,68,0.3)]"
                )}
                style={{ width: `${basinPercentage}%` }}
              >
                <span className="text-white text-xs font-bold whitespace-nowrap">{basin?.currentStock} Blocks</span>
              </div>
            </div>

            <div className="grid grid-cols-3 gap-8 pt-4">
              <div className="space-y-1">
                <p className="text-xs text-slate-500 flex items-center gap-1.5 uppercase tracking-wider font-bold">
                  <LayoutGrid className="w-3 h-3" /> Max Capacity
                </p>
                <p className="text-xl font-bold text-slate-900">{basin?.maxCapacity} <span className="text-sm font-medium text-slate-400">Blocks</span></p>
              </div>
              <div className="space-y-1">
                <p className="text-xs text-slate-500 flex items-center gap-1.5 uppercase tracking-wider font-bold">
                  <Unlock className="w-3 h-3 text-primary" /> Free Slots
                </p>
                <p className="text-xl font-bold text-slate-900">{freeSlots} <span className="text-sm font-medium text-slate-400">Blocks</span></p>
              </div>
              <div className="space-y-1">
                <p className="text-xs text-slate-500 flex items-center gap-1.5 uppercase tracking-wider font-bold">
                  <Timer className="w-3 h-3 text-amber-500" /> Freeze Time
                </p>
                <div className="flex items-center gap-2">
                  {isEditingFreeze ? (
                    <div className="flex items-center gap-2">
                      <input 
                        type="number" 
                        value={newFreezeHours} 
                        onChange={(e) => setNewFreezeHours(Number(e.target.value))}
                        className="w-16 px-2 py-1 border rounded text-sm outline-primary"
                        autoFocus
                      />
                      <Button size="sm" className="h-7" onClick={() => freezeMutation.mutate(newFreezeHours)}>Save</Button>
                      <Button size="sm" variant="ghost" className="h-7" onClick={() => setIsEditingFreeze(false)}>X</Button>
                    </div>
                  ) : (
                    <div className="flex items-center gap-2 group cursor-pointer" onClick={() => { setIsEditingFreeze(true); setNewFreezeHours(basin?.freezeHours || 0); }}>
                      <p className="text-xl font-bold text-slate-900">{basin?.freezeHours} <span className="text-sm font-medium text-slate-400">Hours</span></p>
                      <Settings2 className="w-3.5 h-3.5 text-slate-300 group-hover:text-primary transition-colors" />
                    </div>
                  )}
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-lg font-bold flex items-center gap-2">
              <Timer className="w-5 h-5 text-primary" /> Production Health
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="p-4 bg-slate-50 rounded-xl border border-slate-100 flex items-center gap-4">
              <div className="w-12 h-12 bg-white rounded-full border flex items-center justify-center shadow-sm">
                <Droplets className="w-6 h-6 text-primary" />
              </div>
              <div>
                <p className="text-xs font-bold text-slate-400 uppercase tracking-wide">Optimization</p>
                <p className="text-sm font-semibold text-slate-900">Optimal stock levels maintained</p>
              </div>
            </div>
            <p className="text-sm text-slate-500 leading-relaxed">
              System is currently configured to replenish automatically when stock falls below 20%.
            </p>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <CardTitle className="text-lg font-bold flex items-center gap-2">
              <History className="w-5 h-5 text-slate-400" /> Production Cycles
            </CardTitle>
            <Button variant="ghost" size="sm">Export CSV</Button>
          </div>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="text-slate-500 border-b">
                  <th className="pb-3 font-medium">Date & Time</th>
                  <th className="pb-3 font-medium">Trigger</th>
                  <th className="pb-3 font-medium">Blocks Added</th>
                  <th className="pb-3 font-medium">Before</th>
                  <th className="pb-3 font-medium">After</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {productionLog?.map((cycle) => (
                  <tr key={cycle.id} className="group hover:bg-slate-50/50 transition-colors">
                    <td className="py-3 font-medium text-slate-900">
                      {format(new Date(cycle.cycleTime), 'MMM dd, hh:mm a')}
                    </td>
                    <td className="py-3">
                      <Badge 
                        variant={cycle.trigger === 'Auto' ? 'success' : cycle.trigger === 'Manual' ? 'warning' : 'info'}
                      >
                        {cycle.trigger}
                      </Badge>
                    </td>
                    <td className="py-3 font-bold text-primary">+{cycle.blocksAdded}</td>
                    <td className="py-3 text-slate-500">{cycle.stockBefore}</td>
                    <td className="py-3 text-slate-900 font-medium">{cycle.stockAfter}</td>
                  </tr>
                ))}
                {(!productionLog || productionLog.length === 0) && (
                  <tr>
                    <td colSpan={5} className="py-8 text-center text-slate-400">No production logs found</td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
