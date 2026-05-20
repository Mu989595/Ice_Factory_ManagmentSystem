import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import { Receipt, Plus, Calendar, Download, CreditCard } from 'lucide-react';
import { getExpensesByDate, getExpenseCategories, recordExpense } from '../api/expenses';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { formatCurrency } from '../lib/utils';

export function Expenses() {
  const queryClient = useQueryClient();
  const [selectedDate, setSelectedDate] = useState(format(new Date(), 'yyyy-MM-dd'));
  const [showModal, setShowModal] = useState(false);
  const [categoryFilter, setCategoryFilter] = useState('all');

  const { data: expenses, isLoading } = useQuery({
    queryKey: ['expenses', selectedDate],
    queryFn: () => getExpensesByDate(selectedDate),
  });

  const { data: categories } = useQuery({
    queryKey: ['expense-categories'],
    queryFn: getExpenseCategories,
  });

  const [formData, setFormData] = useState({
    categoryId: 0,
    amount: '' as number | string,
    supplier: ''
  });

  const expenseMutation = useMutation({
    mutationFn: (data: any) => recordExpense({ ...data, ledgerDayId: 1 }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['expenses', selectedDate] });
      setShowModal(false);
      setFormData({ categoryId: 0, amount: '', supplier: '' });
      alert('Expense recorded successfully!');
    },
    onError: (error: any) => {
      alert(`Failed to record expense: ${error.response?.data?.Error || error.message}`);
    }
  });

  const filteredExpenses = categoryFilter === 'all' 
    ? expenses 
    : expenses?.filter(e => e.categoryId.toString() === categoryFilter);

  const totalAmount = filteredExpenses?.reduce((acc, e) => acc + e.amount, 0) || 0;

  return (
    <div className="space-y-6">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Expense Tracking</h1>
          <p className="text-sm text-slate-500 mt-1">Track and categorize factory operational costs</p>
        </div>
        <div className="flex items-center gap-3 w-full md:w-auto">
          <div className="relative">
            <Calendar className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input 
              type="date" 
              value={selectedDate}
              onChange={(e) => setSelectedDate(e.target.value)}
              className="pl-10 pr-4 py-2 border rounded-lg text-sm bg-white outline-none focus:ring-2 focus:ring-primary/20"
            />
          </div>
          <Button onClick={() => setShowModal(true)} className="gap-2 shrink-0">
            <Plus className="w-4 h-4" /> Add Expense
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="md:col-span-1 bg-slate-900 text-white border-none shadow-lg">
          <CardContent className="p-6">
            <p className="text-slate-400 text-xs font-bold uppercase tracking-widest">Selected Day Total</p>
            <h2 className="text-2xl font-bold mt-2 text-white">{formatCurrency(totalAmount)}</h2>
            <div className="mt-4 flex items-center gap-2 text-xs text-slate-400">
              <CreditCard className="w-3.5 h-3.5" />
              <span>{filteredExpenses?.length || 0} Transactions</span>
            </div>
          </CardContent>
        </Card>
        
        <div className="md:col-span-3 flex items-end gap-3 pb-2">
          <div className="flex-1 space-y-1.5">
            <label className="text-[10px] font-bold text-slate-500 uppercase tracking-wider px-1">Filter by Category</label>
            <select 
              value={categoryFilter}
              onChange={(e) => setCategoryFilter(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white outline-none focus:ring-2 focus:ring-primary/20 appearance-none cursor-pointer"
            >
              <option value="all">All Categories</option>
              {categories?.map(c => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>
          <Button variant="outline" className="gap-2 h-[38px]">
            <Download className="w-4 h-4" /> Export
          </Button>
        </div>
      </div>

      <Card className="border-none shadow-sm overflow-hidden">
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 border-b border-slate-100">
                <tr className="text-slate-500">
                  <th className="py-4 px-6 font-semibold">Time</th>
                  <th className="py-4 font-semibold">Category</th>
                  <th className="py-4 font-semibold">Type</th>
                  <th className="py-4 font-semibold">Supplier / Ref</th>
                  <th className="py-4 px-6 text-right font-semibold">Amount</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {isLoading ? (
                  Array.from({ length: 5 }).map((_, i) => (
                    <tr key={i} className="animate-pulse">
                      <td colSpan={5} className="py-4 px-6"><div className="h-10 bg-slate-50 rounded" /></td>
                    </tr>
                  ))
                ) : filteredExpenses?.map((exp) => (
                  <tr key={exp.expenseId} className="hover:bg-slate-50/50 transition-colors">
                    <td className="py-4 px-6 text-slate-500">{format(new Date(exp.expenseTime), 'hh:mm a')}</td>
                    <td className="py-4">
                      <span className="font-bold text-slate-900">{exp.categoryName}</span>
                    </td>
                    <td className="py-4">
                      <Badge variant={exp.categoryType === 'Utility' ? 'warning' : 'info'} className="capitalize">
                        {exp.categoryType}
                      </Badge>
                    </td>
                    <td className="py-4">
                      <div className="flex flex-col">
                        <span className="text-slate-700 font-medium">{exp.supplier || 'N/A'}</span>
                        <span className="text-[10px] text-slate-400 font-mono">{exp.invoiceRef || 'No Ref'}</span>
                      </div>
                    </td>
                    <td className="py-4 px-6 text-right font-bold text-slate-900">{formatCurrency(exp.amount)}</td>
                  </tr>
                ))}
                {!isLoading && filteredExpenses?.length === 0 && (
                  <tr>
                    <td colSpan={5} className="py-20 text-center text-slate-400">
                      <Receipt className="w-16 h-16 mx-auto text-slate-100 mb-4" />
                      <p className="text-lg font-medium">No expenses found</p>
                      <p className="text-sm">Try changing the date or category filter</p>
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      {showModal && (
        <div className="fixed inset-0 bg-slate-900/40 backdrop-blur-sm z-[60] flex items-center justify-center p-4">
          <Card className="w-full max-w-md shadow-2xl animate-in zoom-in-95 duration-200">
            <CardHeader>
              <CardTitle>Record Expense</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-1.5">
                <label className="text-sm font-medium text-slate-700">Category</label>
                <select 
                  className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary/20 outline-none bg-white"
                  value={formData.categoryId}
                  onChange={(e) => setFormData({...formData, categoryId: Number(e.target.value)})}
                >
                  <option value={0}>Select Category</option>
                  {categories?.map(c => (
                    <option key={c.id} value={c.id}>{c.name}</option>
                  ))}
                </select>
              </div>
              <div className="space-y-1.5">
                <label className="text-sm font-medium text-slate-700">Amount (EGP)</label>
                <input 
                  type="number"
                  className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary/20 outline-none"
                  value={formData.amount}
                  onChange={(e) => setFormData({...formData, amount: e.target.value === '' ? '' : Number(e.target.value)})}
                />
              </div>
              <div className="space-y-1.5">
                <label className="text-sm font-medium text-slate-700">Supplier</label>
                <input 
                  type="text"
                  className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary/20 outline-none"
                  value={formData.supplier}
                  onChange={(e) => setFormData({...formData, supplier: e.target.value})}
                />
              </div>
            </CardContent>
            <div className="p-6 pt-0 flex gap-3">
              <Button variant="outline" className="flex-1" onClick={() => setShowModal(false)}>Cancel</Button>
              <Button className="flex-1" onClick={() => expenseMutation.mutate(formData)}>Save Expense</Button>
            </div>
          </Card>
        </div>
      )}
    </div>
  );
}
