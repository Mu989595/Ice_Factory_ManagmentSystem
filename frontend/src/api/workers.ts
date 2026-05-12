import api from './client';
import { type WorkerResult, type CreateWorkerDto } from '../types';

export const getWorkers = async (): Promise<WorkerResult[]> => {
  const { data } = await api.get<WorkerResult[]>('/workers');
  return data;
};

export const createWorker = async (worker: CreateWorkerDto): Promise<{ id: number }> => {
  const { data } = await api.post<{ id: number }>('/workers', worker);
  return data;
};

export const updateWorkerWage = async (id: number, dailyWage: number): Promise<{ id: number; dailyWage: number }> => {
  const { data } = await api.patch<{ id: number; dailyWage: number }>(`/workers/${id}/wage`, dailyWage);
  return data;
};
