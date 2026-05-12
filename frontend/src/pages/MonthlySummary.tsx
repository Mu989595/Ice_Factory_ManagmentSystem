import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { 
  TrendingUp, 
  TrendingDown, 
  Calculator, 
  Lock, 
  ChevronRight,
  PieChart,
  CalendarDays
} from 'lucide-react';
import { getMonthlySummary, closeMonth } from '../api/monthly';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { formatCurrency } from '../lib/utils';
import { PieChart as RePieChart, Pie, Cell, ResponsiveContainer, Tooltip, Legend } from 'recharts';

export function MonthlySummary() {
  const queryClient = useQueryClient();
  const now = new Date();
  const [year, setYear] = useState(now.getFullYear());
  const [month, setMonth] = useState(now.getMonth() + 1);

  const { data: summary } = useQuery({
    queryKey: ['monthly-summary', year, month],
    queryFn: () => getMonthlySummary(year, month),
  });

  const [splits, setSplits] = useState([
    { partnerName: 'Partner A', percentage: 50 },
    { partnerName: 'Partner B', percentage: 50 },
  ]);

  const closeMutation = useMutation({
    mutationFn: (dto: any) => closeMonth(dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['monthly-summary', year, month] }),
  });

  const years = Array.from({ length: 5 }, (_, i) => now.getFullYear() - i);
  const months = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];

  const chartData = summary?.splits.map(s => ({
    name: s.partnerName,
    value: s.amount
  })) || [];

  const COLORS = ['#3b82f6', '#8b5cf6', '#ec4899', '#f97316'];

  return (
    <div className="space-y-6">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Monthly Performance</h1>
          <p className="text-sm text-slate-500 mt-1">Review financial performance and close fiscal periods</p>
        </div>
        <div className="flex items-center gap-3 w-full md:w-auto">
          <select 
            value={month} 
            onChange={(e) => setMonth(Number(e.target.value))}
            className="px-3 py-2 border rounded-lg text-sm bg-white outline-none focus:ring-2 focus:ring-primary/20"
          >
            {months.map((m, i) => <option key={m} value={i + 1}>{m}</option>)}
          </select>
          <select 
            value={year} 
            onChange={(e) => setYear(Number(e.target.value))}
            className="px-3 py-2 border rounded-lg text-sm bg-white outline-none focus:ring-2 focus:ring-primary/20"
          >
            {years.map(y => <option key={y} value={y}>{y}</option>)}
          </select>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card className="border-none shadow-sm relative overflow-hidden group">
          <div className="absolute top-0 right-0 p-4 opacity-10 group-hover:scale-110 transition-transform">
            <TrendingUp className="w-12 h-12 text-green-500" />
          </div>
          <CardContent className="p-6">
            <p className="text-slate-500 text-xs font-bold uppercase tracking-widest">Monthly Income</p>
            <h2 className="text-3xl font-bold mt-2 text-slate-900">{formatCurrency(summary?.totalIncome || 0)}</h2>
            <div className="mt-4 flex items-center gap-2 text-xs text-green-600 font-bold bg-green-50 w-fit px-2 py-1 rounded-full">
              <TrendingUp className="w-3 h-3" />
              <span>Verified Sales</span>
            </div>
          </CardContent>
        </Card>
        
        <Card className="border-none shadow-sm relative overflow-hidden group">
          <div className="absolute top-0 right-0 p-4 opacity-10 group-hover:scale-110 transition-transform">
            <TrendingDown className="w-12 h-12 text-red-500" />
          </div>
          <CardContent className="p-6">
            <p className="text-slate-500 text-xs font-bold uppercase tracking-widest">Monthly Expenses</p>
            <h2 className="text-3xl font-bold mt-2 text-slate-900">{formatCurrency(summary?.totalExpenses || 0)}</h2>
            <div className="mt-4 flex items-center gap-2 text-xs text-red-600 font-bold bg-red-50 w-fit px-2 py-1 rounded-full">
              <TrendingDown className="w-3 h-3" />
              <span>Operational Cost</span>
            </div>
          </CardContent>
        </Card>

        <Card className="border-none shadow-sm bg-slate-900 text-white relative overflow-hidden group">
          <CardContent className="p-6">
            <p className="text-slate-400 text-xs font-bold uppercase tracking-widest">Net Profit</p>
            <h2 className="text-3xl font-bold mt-2 text-white">{formatCurrency(summary?.netProfit || 0)}</h2>
            <div className="mt-4 flex items-center gap-2 text-xs text-primary-foreground font-bold bg-white/10 w-fit px-2 py-1 rounded-full">
              <Calculator className="w-3 h-3" />
              <span>Disposable Income</span>
            </div>
          </CardContent>
        </Card>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card className="border-none shadow-sm">
          <CardHeader>
            <CardTitle className="text-lg font-bold flex items-center gap-2">
              <PieChart className="w-5 h-5 text-primary" /> Profit Split Breakdown
            </CardTitle>
          </CardHeader>
          <CardContent>
            {summary?.isClosed ? (
              <div className="h-[300px] w-full">
                <ResponsiveContainer width="100%" height="100%">
                  <RePieChart>
                    <Pie
                      data={chartData}
                      cx="50%"
                      cy="50%"
                      innerRadius={60}
                      outerRadius={80}
                      paddingAngle={5}
                      dataKey="value"
                    >
                      {chartData.map((_, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip 
                      formatter={(value: any) => formatCurrency(Number(value))}
                      contentStyle={{ borderRadius: '12px', border: 'none', boxShadow: '0 10px 15px -3px rgb(0 0 0 / 0.1)' }}
                    />
                    <Legend />
                  </RePieChart>
                </ResponsiveContainer>
              </div>
            ) : (
              <div className="flex flex-col items-center justify-center h-[300px] text-slate-400 space-y-4">
                <Lock className="w-12 h-12 opacity-20" />
                <p className="text-sm font-medium">Profit splits visible after month closure</p>
              </div>
            )}
          </CardContent>
        </Card>

        <Card className="border-none shadow-sm overflow-hidden">
          <CardHeader className="bg-slate-50/50">
            <CardTitle className="text-lg font-bold flex items-center gap-2">
              <CalendarDays className="w-5 h-5 text-slate-400" /> 
              {summary?.isClosed ? 'Closure Details' : 'Close This Month'}
            </CardTitle>
          </CardHeader>
          <CardContent className="p-6">
            {summary?.isClosed ? (
              <div className="space-y-6">
                <div className="flex items-center gap-3 p-4 bg-green-50 text-green-700 rounded-xl border border-green-100">
                  <Lock className="w-5 h-5" />
                  <span className="font-bold">This period is locked and finalized</span>
                </div>
                <div className="space-y-4">
                  {summary.splits.map((split, i) => (
                    <div key={split.partnerName} className="flex justify-between items-center p-4 rounded-lg bg-slate-50 border border-slate-100 group hover:border-primary/30 transition-colors">
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-lg bg-white border flex items-center justify-center font-bold text-slate-400 group-hover:text-primary transition-colors">
                          {i + 1}
                        </div>
                        <span className="font-bold text-slate-900">{split.partnerName}</span>
                      </div>
                      <div className="text-right">
                        <p className="text-sm font-bold text-slate-900">{formatCurrency(split.amount)}</p>
                        <p className="text-[10px] font-bold text-slate-400 uppercase">{split.percentage}% Split</p>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            ) : (
              <div className="space-y-6">
                <p className="text-sm text-slate-500 leading-relaxed">
                  Finalize the month by defining the profit distribution between partners. 
                  Once closed, no further transactions can be added to this period.
                </p>
                <div className="space-y-4">
                  {splits.map((split, index) => (
                    <div key={index} className="flex items-center gap-4">
                      <input 
                        className="flex-1 px-3 py-2 border rounded-lg text-sm outline-none focus:ring-2 focus:ring-primary/20"
                        value={split.partnerName}
                        onChange={(e) => {
                          const newSplits = [...splits];
                          newSplits[index].partnerName = e.target.value;
                          setSplits(newSplits);
                        }}
                      />
                      <div className="relative w-24">
                        <input 
                          type="number"
                          className="w-full px-3 py-2 border rounded-lg text-sm outline-none focus:ring-2 focus:ring-primary/20 pr-8"
                          value={split.percentage}
                          onChange={(e) => {
                            const newSplits = [...splits];
                            newSplits[index].percentage = Number(e.target.value);
                            setSplits(newSplits);
                          }}
                        />
                        <span className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 text-xs">%</span>
                      </div>
                    </div>
                  ))}
                </div>
                <Button 
                  className="w-full h-12 gap-2 text-base shadow-xl shadow-primary/20"
                  onClick={() => closeMutation.mutate({ year, month, splits })}
                >
                  <Lock className="w-4 h-4" /> Finalize & Close Month
                </Button>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-lg font-bold">Historical Records</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead className="border-b">
                <tr className="text-slate-500">
                  <th className="py-4 font-semibold">Period</th>
                  <th className="py-4 font-semibold">Total Income</th>
                  <th className="py-4 font-semibold">Total Expenses</th>
                  <th className="py-4 font-semibold">Net Profit</th>
                  <th className="py-4 font-semibold">Status</th>
                  <th className="py-4 text-right font-semibold">Action</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                <tr className="hover:bg-slate-50/50 transition-colors cursor-pointer group">
                  <td className="py-4 font-bold text-slate-900">April 2026</td>
                  <td className="py-4 text-slate-600 font-medium">{formatCurrency(45000)}</td>
                  <td className="py-4 text-slate-600 font-medium">{formatCurrency(12000)}</td>
                  <td className="py-4 font-bold text-green-600">{formatCurrency(33000)}</td>
                  <td className="py-4">
                    <Badge variant="success">Closed</Badge>
                  </td>
                  <td className="py-4 text-right">
                    <Button variant="ghost" size="sm" className="group-hover:translate-x-1 transition-transform">
                      <ChevronRight className="w-4 h-4" />
                    </Button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
