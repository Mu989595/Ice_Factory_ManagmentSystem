import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { format } from 'date-fns';
import { 
  TrendingUp, 
  TrendingDown, 
  Wallet, 
  Droplets,
  Plus,
} from 'lucide-react';
import { getBasinState } from '../api/basin';
import { getSalesByDate } from '../api/sales';
import { getExpensesByDate } from '../api/expenses';
import { formatCurrency, getArabicRole, cn } from '../lib/utils';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';

export function Dashboard() {
  const navigate = useNavigate();
  const today = format(new Date(), 'yyyy-MM-dd');

  const { data: basin } = useQuery({ queryKey: ['basin'], queryFn: getBasinState });
  const { data: sales } = useQuery({ queryKey: ['sales', today], queryFn: () => getSalesByDate(today) });
  const { data: expenses } = useQuery({ queryKey: ['expenses', today], queryFn: () => getExpensesByDate(today) });

  const todayIncome = sales?.reduce((acc, s) => acc + s.totalAmount, 0) || 0;
  const todayExpenses = expenses?.reduce((acc, e) => acc + e.amount, 0) || 0;
  const netProfit = todayIncome - todayExpenses;
  const basinLevel = basin ? Math.round((basin.currentStock / basin.maxCapacity) * 100) : 0;

  const metrics = [
    { title: "Today's Income", value: formatCurrency(todayIncome), icon: TrendingUp, color: "text-green-600", bg: "bg-green-50", trend: "+12%" },
    { title: "Today's Expenses", value: formatCurrency(todayExpenses), icon: TrendingDown, color: "text-red-600", bg: "bg-red-50", trend: "-5%" },
    { title: "Net Profit", value: formatCurrency(netProfit), icon: Wallet, color: "text-blue-600", bg: "bg-blue-50", trend: "+8%" },
    { title: "Basin Level", value: `${basinLevel}%`, icon: Droplets, color: "text-primary", bg: "bg-primary/10", trend: "Normal" },
  ];

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-slate-900">System Overview</h1>
        <div className="flex gap-3">
          <Button variant="outline" className="gap-2" onClick={() => navigate('/expenses')}>
            <Plus className="w-4 h-4" /> Add Expense
          </Button>
          <Button className="gap-2" onClick={() => navigate('/sales')}>
            <Plus className="w-4 h-4" /> New Sale
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {metrics.map((m) => (
          <Card key={m.title} className="overflow-hidden border-none shadow-sm hover:shadow-md transition-shadow">
            <CardContent className="p-6">
              <div className="flex justify-between items-start">
                <div className={cn("p-2 rounded-lg", m.bg)}>
                  <m.icon className={cn("w-5 h-5", m.color)} />
                </div>
                <Badge variant={m.trend.startsWith('+') ? 'success' : m.trend.startsWith('-') ? 'destructive' : 'info'}>
                  {m.trend}
                </Badge>
              </div>
              <div className="mt-4">
                <p className="text-sm font-medium text-slate-500">{m.title}</p>
                <h3 className="text-2xl font-bold text-slate-900 mt-1">{m.value}</h3>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <Card className="lg:col-span-2">
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-lg font-bold">Today's Sales</CardTitle>
            <Button variant="ghost" size="sm" className="text-primary">View All</Button>
          </CardHeader>
          <CardContent>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm">
                <thead>
                  <tr className="text-slate-500 border-b">
                    <th className="pb-3 font-medium">Time</th>
                    <th className="pb-3 font-medium">Customer</th>
                    <th className="pb-3 font-medium">Blocks</th>
                    <th className="pb-3 font-medium text-right">Total</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {sales?.map((sale) => (
                    <tr key={sale.saleId} className="group hover:bg-slate-50/50 transition-colors">
                      <td className="py-3 text-slate-500">{format(new Date(sale.saleTime), 'hh:mm a')}</td>
                      <td className="py-3 font-medium text-slate-900">{sale.customerName || 'Walk-in'}</td>
                      <td className="py-3 text-slate-600">{sale.blocksSold} units</td>
                      <td className="py-3 text-right font-bold text-slate-900">{formatCurrency(sale.totalAmount)}</td>
                    </tr>
                  ))}
                  {(!sales || sales.length === 0) && (
                    <tr>
                      <td colSpan={4} className="py-8 text-center text-slate-400">No sales recorded today</td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>

        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg font-bold">Basin Inventory</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="h-4 w-full bg-slate-100 rounded-full overflow-hidden border border-slate-200">
                <div 
                  className={cn(
                    "h-full transition-all duration-500",
                    basinLevel > 60 ? "bg-green-500" : basinLevel > 30 ? "bg-amber-500" : "bg-red-500"
                  )}
                  style={{ width: `${basinLevel}%` }}
                />
              </div>
              <div className="flex justify-between items-center text-sm font-medium">
                <span className="text-slate-500">Current Stock</span>
                <span className="text-slate-900">{basin?.currentStock || 0} / {basin?.maxCapacity || 0} Blocks</span>
              </div>
            </CardContent>
          </Card>

        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-lg font-bold">Recent Expenses</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="text-slate-500 border-b">
                  <th className="pb-3 font-medium">Time</th>
                  <th className="pb-3 font-medium">Category</th>
                  <th className="pb-3 font-medium">Type</th>
                  <th className="pb-3 font-medium">Supplier</th>
                  <th className="pb-3 font-medium text-right">Amount</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {expenses?.map((exp) => (
                  <tr key={exp.expenseId} className="group hover:bg-slate-50/50 transition-colors">
                    <td className="py-3 text-slate-500">{format(new Date(exp.expenseTime), 'hh:mm a')}</td>
                    <td className="py-3 font-medium text-slate-900">{exp.categoryName}</td>
                    <td className="py-3">
                      <Badge variant={exp.categoryType === 'Utility' ? 'warning' : 'info'}>
                        {exp.categoryType}
                      </Badge>
                    </td>
                    <td className="py-3 text-slate-600">{exp.supplier || 'N/A'}</td>
                    <td className="py-3 text-right font-bold text-slate-900">{formatCurrency(exp.amount)}</td>
                  </tr>
                ))}
                {(!expenses || expenses.length === 0) && (
                  <tr>
                    <td colSpan={5} className="py-8 text-center text-slate-400">No expenses recorded today</td>
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


