import api from './client';
import { type AttendanceResultDto, type AttendanceEntryDto } from '../types';

export const getAttendanceByDate = async (date: string): Promise<AttendanceResultDto[]> => {
  const { data } = await api.get<AttendanceResultDto[]>(`/attendance/date/${date}`);
  return data;
};

export const recordAttendance = async (ledgerDayId: number, entries: AttendanceEntryDto[]): Promise<AttendanceResultDto[]> => {
  const { data } = await api.post<AttendanceResultDto[]>(`/attendance/record/${ledgerDayId}`, entries);
  return data;
};
