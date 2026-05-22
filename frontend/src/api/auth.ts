import axios from 'axios';
import api from './client';

export interface LoginRequestDto {
  username: string;
  password?: string;
}

export interface RegisterRequestDto {
  username: string;
  email: string;
  password?: string;
  fullName: string;
}

/** Flat auth payload — matches backend AuthResponseDto (camelCase JSON). */
export interface AuthResponseDto {
  token: string;
  expiration: string;
  username: string;
  fullName: string;
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
  },

  register: async (request: RegisterRequestDto): Promise<AuthResponseDto> => {
    const { data } = await api.post<AuthResponseDto>('/auth/register', request);
    return data;
  },
};
