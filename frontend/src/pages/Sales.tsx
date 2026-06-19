import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import { ShoppingCart, Plus, Calendar, Search, Filter } from 'lucide-react';
import { getSalesByDate, recordSale } from '../api/sales';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { formatCurrency } from '../lib/utils';
import { Badge } from '../components/ui/Badge';

export function Sales() {
  const queryClient = useQueryClient();
  const [selectedDate, setSelectedDate] = useState(format(new Date(), 'yyyy-MM-dd'));
  const [showModal, setShowModal] = useState(false);

  const { data: sales, isLoading } = useQuery({
    queryKey: ['sales', selectedDate],
    queryFn: () => getSalesByDate(selectedDate),
  });

  const [formData, setFormData] = useState({
    blocksSold: '' as number | string,
    unitPrice: '' as number | string,
    customerName: ''
  });

  const saleMutation = useMutation({
    mutationFn: (data: any) => recordSale({ ...data, ledgerDayId: 1 }), // simplified ledgerDayId
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sales'] });
      queryClient.invalidateQueries({ queryKey: ['basin'] }); // Update basin stock
      setShowModal(false);
      setFormData({ blocksSold: '', unitPrice: '', customerName: '' });
      alert('Sale recorded successfully!');
    },
    onError: (error: any) => {
      console.error('Sale recording failed:', error);
      alert(`Failed to record sale: ${error.response?.data?.Error || error.message}`);
    }
  });

  const totalSalesAmount = sales?.reduce((acc, s) => acc + s.totalAmount, 0) || 0;
  const totalBlocks = sales?.reduce((acc, s) => acc + s.blocksSold, 0) || 0;

  return (
    <div className="space-y-6">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Sales Ledger</h1>
          <p className="text-sm text-slate-500 mt-1">Manage and record ice block sales</p>
        </div>
        <div className="flex items-center gap-3 w-full md:w-auto">
          <div className="relative flex-1 md:flex-none">
            <Calendar className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400 pointer-events-none" />
            <input
              type="date"
              value={selectedDate}
              onChange={(e) => setSelectedDate(e.target.value)}
              className="pl-10 pr-4 py-2 border rounded-lg text-sm bg-white focus:ring-2 focus:ring-primary/20 outline-none w-full"
            />
          </div>
          <Button onClick={() => setShowModal(true)} className="gap-2 shrink-0">
            <Plus className="w-4 h-4" /> New Sale
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card className="bg-primary text-white border-none shadow-lg shadow-primary/20">
          <CardContent className="p-6">
            <p className="text-primary-foreground/80 text-sm font-medium uppercase tracking-wider">Total Revenue</p>
            <h2 className="text-3xl font-bold mt-1">{formatCurrency(totalSalesAmount)}</h2>
          </CardContent>
        </Card>
        <Card className="border-none shadow-sm">
          <CardContent className="p-6">
            <p className="text-slate-500 text-sm font-medium uppercase tracking-wider">Blocks Sold</p>
            <h2 className="text-3xl font-bold mt-1 text-slate-900">{totalBlocks} <span className="text-sm font-medium text-slate-400">Units</span></h2>
          </CardContent>
        </Card>
        <Card className="border-none shadow-sm">
          <CardContent className="p-6">
            <p className="text-slate-500 text-sm font-medium uppercase tracking-wider">Average Price</p>
            <h2 className="text-3xl font-bold mt-1 text-slate-900">{formatCurrency(totalBlocks > 0 ? totalSalesAmount / totalBlocks : 0)}</h2>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader className="flex flex-row items-center justify-between py-4">
          <CardTitle className="text-lg font-bold">Transaction History</CardTitle>
          <div className="flex items-center gap-2">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
              <input
                placeholder="Search customer..."
                className="pl-9 pr-4 py-1.5 border rounded-md text-sm bg-slate-50 focus:bg-white transition-all outline-none"
              />
            </div>
            <Button variant="outline" size="sm" className="gap-2">
              <Filter className="w-4 h-4" /> Filter
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="text-slate-500 border-b">
                  <th className="pb-4 font-medium px-2">Time</th>
                  <th className="pb-4 font-medium">Customer</th>
                  <th className="pb-4 font-medium text-center">Blocks</th>
                  <th className="pb-4 font-medium text-right">Unit Price</th>
                  <th className="pb-4 font-medium text-right">Total Amount</th>
                  <th className="pb-4 font-medium text-center">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {isLoading ? (
                  Array.from({ length: 5 }).map((_, i) => (
                    <tr key={i} className="animate-pulse">
                      <td colSpan={6} className="py-4"><div className="h-8 bg-slate-100 rounded" /></td>
                    </tr>
                  ))
                ) : sales?.map((sale) => (
                  <tr key={sale.saleId} className="group hover:bg-slate-50/50 transition-colors">
                    <td className="py-4 px-2 text-slate-500">{format(new Date(sale.saleTime), 'hh:mm a')}</td>
                    <td className="py-4">
                      <div className="flex flex-col">
                        <span className="font-bold text-slate-900">{sale.customerName || 'Walk-in Customer'}</span>
                        {sale.notes && <span className="text-[10px] text-slate-400 italic mt-0.5">{sale.notes}</span>}
                      </div>
                    </td>
                    <td className="py-4 text-center font-medium text-slate-600">{sale.blocksSold}</td>
                    <td className="py-4 text-right text-slate-500">{formatCurrency(sale.unitPrice)}</td>
                    <td className="py-4 text-right font-bold text-slate-900">{formatCurrency(sale.totalAmount)}</td>
                    <td className="py-4 text-center">
                      <Badge variant="success">Completed</Badge>
                    </td>
                  </tr>
                ))}
                {!isLoading && sales?.length === 0 && (
                  <tr>
                    <td colSpan={6} className="py-12 text-center text-slate-400">
                      <ShoppingCart className="w-12 h-12 mx-auto text-slate-200 mb-3" />
                      <p className="font-medium">No sales recorded for this date</p>
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      {/* Simple Modal Backdrop */}
      {showModal && (
        <div className="fixed inset-0 bg-slate-900/40 backdrop-blur-sm z-[60] flex items-center justify-center p-4">
          <Card className="w-full max-w-md shadow-2xl animate-in zoom-in-95 duration-200">
            <CardHeader>
              <CardTitle>Record New Sale</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-1.5">
                <label className="text-sm font-medium text-slate-700">Blocks Sold</label>
                <input
                  type="number"
                  className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary/20 outline-none"
                  value={formData.blocksSold}
                  onChange={(e) => setFormData({ ...formData, blocksSold: e.target.value === '' ? '' : Number(e.target.value) })}
                />
              </div>
              <div className="space-y-1.5">
                <label className="text-sm font-medium text-slate-700">Unit Price (EGP)</label>
                <input
                  type="number"
                  className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary/20 outline-none"
                  value={formData.unitPrice}
                  onChange={(e) => setFormData({ ...formData, unitPrice: e.target.value === '' ? '' : Number(e.target.value) })}
                />
              </div>
              <div className="space-y-1.5">
                <label className="text-sm font-medium text-slate-700">Customer Name</label>
                <input
                  type="text"
                  placeholder="Optional"
                  className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary/20 outline-none"
                  value={formData.customerName}
                  onChange={(e) => setFormData({ ...formData, customerName: e.target.value })}
                />
              </div>
            </CardContent>
            <div className="p-6 pt-0 flex gap-3">
              <Button variant="outline" className="flex-1" onClick={() => setShowModal(false)}>Cancel</Button>
              <Button className="flex-1" onClick={() => saleMutation.mutate(formData)}>Confirm Sale</Button>
            </div>
          </Card>
        </div>
      )}
    </div>
  );
}
