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

export interface AuthResponseDto {
  token: string;
  expiration: string;
  username: string;
  fullName: string;
}

export const authApi = {
  login: async (request: LoginRequestDto): Promise<AuthResponseDto> => {
    const response = await api.post<AuthResponseDto>('/Auth/login', request);
    return response.data;
  },

  register: async (request: RegisterRequestDto): Promise<AuthResponseDto> => {
    const response = await api.post<AuthResponseDto>('/Auth/register', request);
    return response.data;
  }
};
