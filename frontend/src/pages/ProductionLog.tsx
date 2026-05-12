import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { format, subDays } from 'date-fns';
import { ClipboardList, Calendar, Search, Filter, Box, RefreshCw } from 'lucide-react';
import { getProductionLog } from '../api/basin';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { cn } from '../lib/utils';

export function ProductionLog() {
  const [startDate, setStartDate] = useState(format(subDays(new Date(), 30), 'yyyy-MM-dd'));
  const [endDate, setEndDate] = useState(format(new Date(), 'yyyy-MM-dd'));

  const { data: logs, isLoading, refetch } = useQuery({
    queryKey: ['production-log', startDate, endDate],
    queryFn: () => getProductionLog(startDate, endDate),
  });

  const totalProduced = logs?.reduce((acc, log) => acc + log.blocksAdded, 0) || 0;

  return (
    <div className="space-y-6">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Production Log</h1>
          <p className="text-sm text-slate-500 mt-1">Monitor historical ice block production cycles</p>
        </div>
        <div className="flex items-center gap-3 w-full md:w-auto">
          <div className="flex items-center gap-2 bg-white border rounded-lg px-3 py-1.5 shadow-sm">
            <Calendar className="w-4 h-4 text-slate-400" />
            <input 
              type="date" 
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              className="text-xs border-none outline-none focus:ring-0 w-28"
            />
            <span className="text-slate-300">|</span>
            <input 
              type="date" 
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              className="text-xs border-none outline-none focus:ring-0 w-28"
            />
          </div>
          <Button size="icon" variant="outline" onClick={() => refetch()}>
            <RefreshCw className={cn("w-4 h-4", isLoading && "animate-spin")} />
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card className="border-none shadow-sm relative overflow-hidden">
          <div className="absolute inset-y-0 left-0 w-1 bg-primary" />
          <CardContent className="p-6">
            <div className="flex justify-between items-center">
              <div>
                <p className="text-slate-500 text-xs font-bold uppercase tracking-widest">Total Production</p>
                <h3 className="text-2xl font-bold mt-1 text-slate-900">{totalProduced} <span className="text-sm font-medium text-slate-400">Blocks</span></h3>
              </div>
              <div className="w-12 h-12 bg-primary/10 rounded-full flex items-center justify-center text-primary">
                <Box className="w-6 h-6" />
              </div>
            </div>
            <p className="mt-4 text-xs text-slate-400">In the selected period</p>
          </CardContent>
        </Card>
      </div>

      <Card className="border-none shadow-sm">
        <CardHeader className="flex flex-row items-center justify-between border-b">
          <CardTitle className="text-lg font-bold">Production History</CardTitle>
          <div className="flex items-center gap-2">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
              <input 
                placeholder="Filter by trigger..." 
                className="pl-9 pr-4 py-1.5 border rounded-md text-sm bg-slate-50 focus:bg-white transition-all outline-none"
              />
            </div>
            <Button variant="outline" size="sm" className="gap-2">
              <Filter className="w-4 h-4" /> Filter
            </Button>
          </div>
        </CardHeader>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="text-slate-500 border-b bg-slate-50/50">
                  <th className="py-4 px-6 font-semibold">Date & Time</th>
                  <th className="py-4 font-semibold">Trigger Method</th>
                  <th className="py-4 font-semibold text-center">Blocks Added</th>
                  <th className="py-4 font-semibold text-center">Stock Before</th>
                  <th className="py-4 font-semibold text-center">Stock After</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {isLoading ? (
                  Array.from({ length: 8 }).map((_, i) => (
                    <tr key={i} className="animate-pulse">
                      <td colSpan={5} className="py-4 px-6"><div className="h-10 bg-slate-50 rounded" /></td>
                    </tr>
                  ))
                ) : logs?.map((log) => (
                  <tr key={log.id} className="hover:bg-slate-50/50 transition-colors">
                    <td className="py-4 px-6 font-medium text-slate-900">
                      {format(new Date(log.cycleTime), 'MMM dd, yyyy - hh:mm a')}
                    </td>
                    <td className="py-4">
                      <Badge 
                        variant={log.trigger === 'Auto' ? 'success' : log.trigger === 'Manual' ? 'warning' : 'info'}
                        className="font-bold uppercase tracking-widest text-[10px]"
                      >
                        {log.trigger}
                      </Badge>
                    </td>
                    <td className="py-4 text-center">
                      <span className="font-bold text-primary">+{log.blocksAdded}</span>
                    </td>
                    <td className="py-4 text-center text-slate-500">
                      {log.stockBefore}
                    </td>
                    <td className="py-4 text-center font-bold text-slate-900">
                      {log.stockAfter}
                    </td>
                  </tr>
                ))}
                {!isLoading && logs?.length === 0 && (
                  <tr>
                    <td colSpan={5} className="py-24 text-center text-slate-400">
                      <ClipboardList className="w-16 h-16 mx-auto text-slate-100 mb-4" />
                      <p className="text-lg font-medium">No production logs found</p>
                      <p className="text-sm">Try expanding the date range</p>
                    </td>
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


