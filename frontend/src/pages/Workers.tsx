import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Users, UserPlus, Edit3 } from 'lucide-react';
import { getWorkers, createWorker, updateWorkerWage } from '../api/workers';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { formatCurrency } from '../lib/utils';
import { format } from 'date-fns';

export function Workers() {
  const queryClient = useQueryClient();
  const [showModal, setShowModal] = useState(false);
  const [editingWageId, setEditingWageId] = useState<number | null>(null);
  const [newWage, setNewWage] = useState(0);

  const { data: workers, isLoading } = useQuery({
    queryKey: ['workers'],
    queryFn: getWorkers,
  });

  const [formData, setFormData] = useState({
    fullName: '',
    role: 'WinchOperator' as any,
    dailyWage: 0,
    hiredAt: format(new Date(), 'yyyy-MM-dd')
  });

  const createMutation = useMutation({
    mutationFn: createWorker,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workers'] });
      setShowModal(false);
      setFormData({ fullName: '', role: 'WinchOperator', dailyWage: 0, hiredAt: format(new Date(), 'yyyy-MM-dd') });
    },
  });

  const wageMutation = useMutation({
    mutationFn: ({ id, wage }: { id: number, wage: number }) => updateWorkerWage(id, wage),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workers'] });
      setEditingWageId(null);
    },
  });

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Workers Directory</h1>
          <p className="text-sm text-slate-500 mt-1">Manage factory staff and payroll</p>
        </div>
        <Button onClick={() => setShowModal(true)} className="gap-2">
          <UserPlus className="w-4 h-4" /> New Worker
        </Button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card className="border-none shadow-sm">
          <CardContent className="p-6 flex items-center gap-4">
            <div className="w-12 h-12 rounded-2xl bg-primary/10 flex items-center justify-center text-primary">
              <Users className="w-6 h-6" />
            </div>
            <div>
              <p className="text-sm font-medium text-slate-500">Total Workers</p>
              <h3 className="text-2xl font-bold text-slate-900">{workers?.length || 0}</h3>
            </div>
          </CardContent>
        </Card>
        {/* Additional stats could go here */}
      </div>

      <Card className="border-none shadow-sm">
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 border-b border-slate-100">
                <tr className="text-slate-500">
                  <th className="py-4 px-6 font-semibold">Name</th>
                  <th className="py-4 font-semibold">Role</th>
                  <th className="py-4 font-semibold">Hired Date</th>
                  <th className="py-4 font-semibold">Daily Wage</th>
                  <th className="py-4 px-6 text-right font-semibold">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {isLoading ? (
                  Array.from({ length: 3 }).map((_, i) => (
                    <tr key={i} className="animate-pulse">
                      <td colSpan={5} className="py-4 px-6"><div className="h-10 bg-slate-50 rounded" /></td>
                    </tr>
                  ))
                ) : workers?.map((worker) => (
                  <tr key={worker.id} className="hover:bg-slate-50/50 transition-colors group">
                    <td className="py-4 px-6">
                      <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-full bg-slate-100 flex items-center justify-center text-slate-500 font-bold">
                          {worker.fullName.charAt(0)}
                        </div>
                        <span className="font-bold text-slate-900">{worker.fullName}</span>
                      </div>
                    </td>
                    <td className="py-4">
                      <div className="flex flex-col">
                        <span className="text-slate-700 text-xs font-medium uppercase tracking-wider">{worker.role}</span>
                        <span className="font-arabic text-primary font-bold text-sm">{worker.roleArabic}</span>
                      </div>
                    </td>
                    <td className="py-4 text-slate-500">
                      {format(new Date(worker.hiredAt), 'MMM dd, yyyy')}
                    </td>
                    <td className="py-4">
                      {editingWageId === worker.id ? (
                        <div className="flex items-center gap-2">
                          <input 
                            type="number"
                            className="w-20 px-2 py-1 border rounded text-sm outline-primary"
                            value={newWage}
                            onChange={(e) => setNewWage(Number(e.target.value))}
                            autoFocus
                          />
                          <Button size="sm" className="h-7 px-2" onClick={() => wageMutation.mutate({ id: worker.id, wage: newWage })}>Save</Button>
                        </div>
                      ) : (
                        <div className="flex items-center gap-2 group/wage">
                          <span className="font-bold text-slate-900">{formatCurrency(worker.dailyWage)}</span>
                          <button 
                            onClick={() => { setEditingWageId(worker.id); setNewWage(worker.dailyWage); }}
                            className="opacity-0 group-hover/wage:opacity-100 transition-opacity p-1 hover:text-primary"
                          >
                            <Edit3 className="w-3.5 h-3.5" />
                          </button>
                        </div>
                      )}
                    </td>
                    <td className="py-4 px-6 text-right">
                      <Button variant="ghost" size="sm">Details</Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      {showModal && (
        <div className="fixed inset-0 bg-slate-900/40 backdrop-blur-sm z-[60] flex items-center justify-center p-4">
          <Card className="w-full max-w-md shadow-2xl animate-in zoom-in-95 duration-200">
            <CardHeader>
              <CardTitle>Add New Worker</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-1.5">
                <label className="text-sm font-medium text-slate-700">Full Name</label>
                <input 
                  type="text"
                  className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary/20 outline-none"
                  value={formData.fullName}
                  onChange={(e) => setFormData({...formData, fullName: e.target.value})}
                />
              </div>
              <div className="space-y-1.5">
                <label className="text-sm font-medium text-slate-700">Role</label>
                <select 
                  className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary/20 outline-none bg-white"
                  value={formData.role}
                  onChange={(e) => setFormData({...formData, role: e.target.value as any})}
                >
                  <option value="WinchOperator">Winch Operator (وناش)</option>
                  <option value="IcePusher">Ice Pusher (بيزق التلج)</option>
                  <option value="IceStacker">Ice Stacker (بيرص التلج)</option>
                </select>
              </div>
              <div className="space-y-1.5">
                <label className="text-sm font-medium text-slate-700">Daily Wage (EGP)</label>
                <input 
                  type="number"
                  className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary/20 outline-none"
                  value={formData.dailyWage}
                  onChange={(e) => setFormData({...formData, dailyWage: Number(e.target.value)})}
                />
              </div>
              <div className="space-y-1.5">
                <label className="text-sm font-medium text-slate-700">Hired Date</label>
                <input 
                  type="date"
                  className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary/20 outline-none"
                  value={formData.hiredAt}
                  onChange={(e) => setFormData({...formData, hiredAt: e.target.value})}
                />
              </div>
            </CardContent>
            <div className="p-6 pt-0 flex gap-3">
              <Button variant="outline" className="flex-1" onClick={() => setShowModal(false)}>Cancel</Button>
              <Button className="flex-1" onClick={() => createMutation.mutate(formData)}>Add Worker</Button>
            </div>
          </Card>
        </div>
      )}
    </div>
  );
}
