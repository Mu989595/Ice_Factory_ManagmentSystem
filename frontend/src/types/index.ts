// Basin
export interface BasinState {
  currentStock: number;
  maxCapacity: number;
  freezeHours: number;
  lastUpdatedAt: string;
}

// Sales
export interface RecordSaleDto {
  ledgerDayId: number;
  blocksSold: number;
  unitPrice: number;
  customerName?: string;
  notes?: string;
}

export interface SaleResult {
  saleId: number;
  ledgerDayId: number;
  saleTime: string;
  blocksSold: number;
  unitPrice: number;
  totalAmount: number;
  customerName?: string;
  notes?: string;
}

// Expenses
export interface RecordExpenseDto {
  ledgerDayId: number;
  categoryId: number;
  amount: number;
  supplier?: string;
  invoiceRef?: string;
  notes?: string;
}

export interface ExpenseResult {
  expenseId: number;
  ledgerDayId: number;
  categoryId: number;
  categoryName: string;
  categoryType: string;
  amount: number;
  expenseTime: string;
  supplier?: string;
  invoiceRef?: string;
  notes?: string;
}

export interface ExpenseCategoryDto {
  id: number;
  name: string;
  categoryType: string;
  utilityType?: string;
  isActive: boolean;
}

// Workers
export interface CreateWorkerDto {
  fullName: string;
  role: 'WinchOperator' | 'IcePusher' | 'IceStacker';
  dailyWage: number;
  hiredAt: string;
}

export interface WorkerResult {
  id: number;
  fullName: string;
  role: string;
  roleArabic: string;
  dailyWage: number;
  hiredAt: string;
}

// Attendance
export interface AttendanceEntryDto {
  workerId: number;
  attended: boolean;
  notes?: string;
}

export interface AttendanceResultDto {
  attendanceId: number;
  workerId: number;
  workerName: string;
  attended: boolean;
  wagePaid: number;
  notes?: string;
}

// Monthly
export interface CloseMonthDto {
  year: number;
  month: number;
  splits: { partnerName: string; percentage: number }[];
}

export interface MonthlySummaryResult {
  id: number;
  year: number;
  month: number;
  totalIncome: number;
  totalExpenses: number;
  netProfit: number;
  isClosed: boolean;
  closedAt?: string;
  splits: { partnerName: string; percentage: number; amount: number }[];
}

// Production (Production Cycles / Production Log)
export interface ProductionCycle {
  id: number;
  cycleTime: string;
  trigger: 'Auto' | 'Manual' | 'Rollover';
  blocksAdded: number;
  stockBefore: number;
  stockAfter: number;
}
