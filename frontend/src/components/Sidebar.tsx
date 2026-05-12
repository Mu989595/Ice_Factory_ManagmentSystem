import { NavLink } from 'react-router-dom';
import { 
  LayoutDashboard, 
  Droplets, 
  ShoppingCart, 
  Receipt, 
  Users, 
  CalendarCheck, 
  BarChart3, 
  ClipboardList,
  LogOut
} from 'lucide-react';
import { cn } from '../lib/utils';

const navGroups = [
  {
    title: 'Main',
    links: [
      { name: 'Dashboard', href: '/', icon: LayoutDashboard },
      { name: 'Basin', href: '/basin', icon: Droplets },
      { name: 'Sales', href: '/sales', icon: ShoppingCart },
      { name: 'Expenses', href: '/expenses', icon: Receipt },
    ]
  },
  {
    title: 'HR',
    links: [
      { name: 'Workers', href: '/workers', icon: Users },
      { name: 'Attendance', href: '/attendance', icon: CalendarCheck },
    ]
  },
  {
    title: 'Reports',
    links: [
      { name: 'Monthly Summary', href: '/monthly', icon: BarChart3 },
      { name: 'Production Log', href: '/production', icon: ClipboardList },
    ]
  }
];

export function Sidebar() {
  return (
    <aside className="fixed left-0 top-0 h-screen w-[220px] bg-slate-900 text-slate-300 flex flex-col border-r border-slate-800 z-50">
      <div className="p-6 flex items-center gap-3">
        <div className="w-8 h-8 bg-primary rounded-lg flex items-center justify-center">
          <Droplets className="text-white w-5 h-5" />
        </div>
        <h1 className="text-xl font-bold text-white tracking-tight">IcePlant ERP</h1>
      </div>

      <nav className="flex-1 overflow-y-auto px-4 py-4 space-y-8">
        {navGroups.map((group) => (
          <div key={group.title} className="space-y-2">
            <h2 className="px-2 text-xs font-semibold text-slate-500 uppercase tracking-wider">
              {group.title}
            </h2>
            <div className="space-y-1">
              {group.links.map((link) => (
                <NavLink
                  key={link.href}
                  to={link.href}
                  className={({ isActive }) => cn(
                    "flex items-center gap-3 px-3 py-2 rounded-md transition-all duration-200 group",
                    isActive 
                      ? "bg-primary text-white" 
                      : "hover:bg-slate-800 hover:text-white"
                  )}
                >
                  <link.icon className={cn(
                    "w-4 h-4 transition-colors",
                    "group-hover:text-white"
                  )} />
                  <span className="text-sm font-medium">{link.name}</span>
                </NavLink>
              ))}
            </div>
          </div>
        ))}
      </nav>

      <div className="p-4 border-t border-slate-800 mt-auto">
        <div className="flex items-center gap-3 px-3 py-2">
          <div className="w-8 h-8 rounded-full bg-slate-700 flex items-center justify-center text-xs font-medium text-white">
            AD
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-white truncate">Admin User</p>
            <p className="text-xs text-slate-500 truncate">System Manager</p>
          </div>
          <button className="text-slate-500 hover:text-white transition-colors">
            <LogOut className="w-4 h-4" />
          </button>
        </div>
      </div>
    </aside>
  );
}
