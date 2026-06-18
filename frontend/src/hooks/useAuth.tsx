import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import type { AuthResponseDto } from '../api/auth';

interface AuthContextType {
  isAuthenticated: boolean;
  user: AuthResponseDto | null;
  login: (userData: AuthResponseDto) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

/** Check if a JWT token is expired by decoding its payload */
function isTokenExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    // exp is in seconds, Date.now() in milliseconds
    return payload.exp * 1000 < Date.now();
  } catch {
    return true; // If we can't decode it, treat as expired
  }
}

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<AuthResponseDto | null>(null);

  const logout = useCallback(() => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
  }, []);

  useEffect(() => {
    const token = localStorage.getItem('token');
    const storedUser = localStorage.getItem('user');
    
    if (token && storedUser) {
      if (isTokenExpired(token)) {
        // Token expired — clear storage silently
        logout();
      } else {
        setUser(JSON.parse(storedUser));
      }
    }
  }, [logout]);

  // Listen for 401 events dispatched by the API client interceptor
  useEffect(() => {
    const handleUnauthorized = () => logout();
    window.addEventListener('auth:unauthorized', handleUnauthorized);
    return () => window.removeEventListener('auth:unauthorized', handleUnauthorized);
  }, [logout]);

  const login = (userData: AuthResponseDto) => {
    localStorage.setItem('token', userData.token);
    localStorage.setItem('user', JSON.stringify(userData));
    setUser(userData);
  };

  return (
    <AuthContext.Provider value={{ isAuthenticated: !!user, user, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
