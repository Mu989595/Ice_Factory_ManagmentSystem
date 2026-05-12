import api from './client';
import { type SaleResult, type RecordSaleDto } from '../types';

export const getSalesByDate = async (date: string): Promise<SaleResult[]> => {
  const { data } = await api.get<SaleResult[]>(`/sales/date/${date}`);
  return data;
};

export const recordSale = async (sale: RecordSaleDto): Promise<SaleResult> => {
  const { data } = await api.post<SaleResult>('/sales', sale);
  return data;
};
