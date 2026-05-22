import api from './client';
import { type BasinState, type ProductionCycle } from '../types';

export const getBasinState = async (): Promise<BasinState> => {
  const { data } = await api.get<BasinState>('/basin');
  return data;
};

export const replenishBasin = async (blocks: number): Promise<{ currentStock: number }> => {
  const { data } = await api.post<{ currentStock: number }>('/basin/replenish', { blocksToAdd: blocks });
  return data;
};

export const updateFreezeHours = async (hours: number): Promise<{ freezeHours: number }> => {
  const { data } = await api.patch<{ freezeHours: number }>('/basin/freeze-hours', { hours });
  return data;
};

// Assuming there's a production log endpoint based on the requirements
export const getProductionLog = async (startDate?: string, endDate?: string): Promise<ProductionCycle[]> => {
  const { data } = await api.get<ProductionCycle[]>('/basin/production-log', {
    params: { startDate, endDate },
  });
  return data;
};
