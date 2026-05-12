import api from './client';
import { type MonthlySummaryResult, type CloseMonthDto } from '../types';

export const getMonthlySummary = async (year: number, month: number): Promise<MonthlySummaryResult> => {
  const { data } = await api.get<MonthlySummaryResult>(`/monthly/${year}/${month}`);
  return data;
};

export const closeMonth = async (dto: CloseMonthDto): Promise<MonthlySummaryResult> => {
  const { data } = await api.post<MonthlySummaryResult>('/monthly/close', dto);
  return data;
};
