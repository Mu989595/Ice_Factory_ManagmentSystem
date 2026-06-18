import axios from 'axios';
import api from './client';

export interface LoginRequestDto {
  password?: string;
}

export interface AuthResponseDto {
  token: string;
  expiration: string;
}

export function getAuthErrorMessage(err: unknown, fallback: string): string {
  if (axios.isAxiosError(err) && err.response?.data) {
    const data = err.response.data as { error?: string; message?: string };
    return data.error ?? data.message ?? fallback;
  }
  return fallback;
}

export const authApi = {
  login: async (request: LoginRequestDto): Promise<AuthResponseDto> => {
    const { data } = await api.post<AuthResponseDto>('/auth/login', request);
    return data;
  }
};
