import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import { Calendar, CheckCircle2, XCircle, Save, Search } from 'lucide-react';
import { getWorkers } from '../api/workers';
import { getAttendanceByDate, recordAttendance } from '../api/attendance';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { cn, formatCurrency } from '../lib/utils';

export function Attendance() {
  const queryClient = useQueryClient();
  const [selectedDate, setSelectedDate] = useState(format(new Date(), 'yyyy-MM-dd'));
  const [attendanceMap, setAttendanceMap] = useState<Record<number, { attended: boolean, notes: string }>>({});

  const { data: workers } = useQuery({ queryKey: ['workers'], queryFn: getWorkers });
  const { data: existingAttendance } = useQuery({
    queryKey: ['attendance', selectedDate],
    queryFn: () => getAttendanceByDate(selectedDate),
  });

  useEffect(() => {
    if (existingAttendance && workers) {
      const map: Record<number, { attended: boolean, notes: string }> = {};
      workers.forEach(w => {
        const record = existingAttendance.find(a => a.workerId === w.id);
        map[w.id] = {
          attended: record ? record.attended : false,
          notes: record?.notes || ''
        };
      });
      setAttendanceMap(map);
    } else if (workers) {
      const map: Record<number, { attended: boolean, notes: string }> = {};
      workers.forEach(w => {
        map[w.id] = { attended: false, notes: '' };
      });
      setAttendanceMap(map);
    }
  }, [existingAttendance, workers]);

  const recordMutation = useMutation({
    mutationFn: () => {
      const entries = Object.entries(attendanceMap).map(([id, data]) => ({
        workerId: Number(id),
        attended: data.attended,
        notes: data.notes
      }));
      return recordAttendance(1, entries); // simplified ledgerDayId
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['attendance', selectedDate] });
      alert('Attendance saved successfully!');
    },
  });

  const toggleAttendance = (workerId: number) => {
    setAttendanceMap(prev => ({
      ...prev,
      [workerId]: { ...prev[workerId], attended: !prev[workerId].attended }
    }));
  };

  const updateNotes = (workerId: number, notes: string) => {
    setAttendanceMap(prev => ({
      ...prev,
      [workerId]: { ...prev[workerId], notes }
    }));
  };

  const presentCount = Object.values(attendanceMap).filter(v => v.attended).length;

  return (
    <div className="space-y-6">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Worker Attendance</h1>
          <p className="text-sm text-slate-500 mt-1">Track daily presence and automated wage calculation</p>
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
          <Button onClick={() => recordMutation.mutate()} className="gap-2 shrink-0">
            <Save className="w-4 h-4" /> Save Attendance
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
        <Card className="lg:col-span-1 border-none shadow-sm h-fit sticky top-24">
          <CardHeader>
            <CardTitle className="text-lg font-bold">Daily Summary</CardTitle>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="flex justify-between items-center pb-4 border-b">
              <span className="text-sm text-slate-500 font-medium">Total Staff</span>
              <span className="text-lg font-bold text-slate-900">{workers?.length || 0}</span>
            </div>
            <div className="flex justify-between items-center pb-4 border-b">
              <span className="text-sm text-slate-500 font-medium">Present Today</span>
              <Badge variant="success" className="text-sm px-3 py-1">{presentCount}</Badge>
            </div>
            <div className="flex justify-between items-center pb-4 border-b">
              <span className="text-sm text-slate-500 font-medium">Absent</span>
              <Badge variant="destructive" className="text-sm px-3 py-1">{(workers?.length || 0) - presentCount}</Badge>
            </div>
            <div className="pt-2">
              <p className="text-xs text-slate-400 leading-relaxed italic">
                * Wages are automatically added to today's expenses based on recorded attendance.
              </p>
            </div>
          </CardContent>
        </Card>

        <Card className="lg:col-span-3 border-none shadow-sm">
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-lg font-bold">Worker List</CardTitle>
            <div className="relative w-64">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
              <input 
                placeholder="Search worker..." 
                className="pl-9 pr-4 py-1.5 border rounded-md text-sm bg-slate-50 w-full outline-none focus:bg-white transition-all"
              />
            </div>
          </CardHeader>
          <CardContent className="p-0">
            <div className="divide-y divide-slate-100">
              {workers?.map((worker) => {
                const status = attendanceMap[worker.id];
                return (
                  <div key={worker.id} className="p-4 flex flex-col md:flex-row items-start md:items-center justify-between gap-4 group hover:bg-slate-50/50 transition-colors">
                    <div className="flex items-center gap-4 flex-1">
                      <div className={cn(
                        "w-12 h-12 rounded-full flex items-center justify-center font-bold transition-all duration-300",
                        status?.attended 
                          ? "bg-green-500 text-white shadow-lg shadow-green-200" 
                          : "bg-slate-100 text-slate-400"
                      )}>
                        {worker.fullName.charAt(0)}
                      </div>
                      <div>
                        <h4 className="font-bold text-slate-900">{worker.fullName}</h4>
                        <div className="flex items-center gap-2 mt-0.5">
                          <span className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">{worker.role}</span>
                          <span className="w-1 h-1 bg-slate-300 rounded-full" />
                          <span className="text-xs font-bold text-primary">{formatCurrency(worker.dailyWage)}/day</span>
                        </div>
                      </div>
                    </div>

                    <div className="flex items-center gap-4 w-full md:w-auto">
                      <input 
                        placeholder="Add note..."
                        className="text-xs px-3 py-2 border rounded-lg flex-1 md:w-48 outline-none focus:border-primary/50"
                        value={status?.notes || ''}
                        onChange={(e) => updateNotes(worker.id, e.target.value)}
                      />
                      <button 
                        onClick={() => toggleAttendance(worker.id)}
                        className={cn(
                          "flex items-center gap-2 px-4 py-2 rounded-lg font-bold text-sm transition-all duration-200 shrink-0",
                          status?.attended 
                            ? "bg-green-500 text-white hover:bg-green-600" 
                            : "bg-slate-100 text-slate-500 hover:bg-slate-200"
                        )}
                      >
                        {status?.attended ? <CheckCircle2 className="w-4 h-4" /> : <XCircle className="w-4 h-4" />}
                        {status?.attended ? 'Present' : 'Absent'}
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
