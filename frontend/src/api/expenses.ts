import api from './client';
import { type ExpenseResult, type RecordExpenseDto, type ExpenseCategoryDto } from '../types';

export const getExpensesByDate = async (date: string): Promise<ExpenseResult[]> => {
  const { data } = await api.get<ExpenseResult[]>(`/expenses/date/${date}`);
  return data;
};

export const getExpensesByMonth = async (year: number, month: number): Promise<ExpenseResult[]> => {
  const { data } = await api.get<ExpenseResult[]>(`/expenses/month/${year}/${month}`);
  return data;
};

export const getExpenseCategories = async (): Promise<ExpenseCategoryDto[]> => {
  const { data } = await api.get<ExpenseCategoryDto[]>('/expenses/categories');
  return data;
};

export const recordExpense = async (expense: RecordExpenseDto): Promise<ExpenseResult> => {
  const { data } = await api.post<ExpenseResult>('/expenses', expense);
  return data;
};
