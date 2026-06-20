import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { authApi, getAuthErrorMessage } from '../api/auth';
import { useAuth } from '../hooks/useAuth';
import { Eye, EyeOff, Lock, UserPlus } from 'lucide-react';

export const Login: React.FC = () => {
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isInitializing, setIsInitializing] = useState(true);
  const [needsSetup, setNeedsSetup] = useState(false);
  
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  
  const from = location.state?.from?.pathname || '/';

  useEffect(() => {
    const checkStatus = async () => {
      try {
        const { isInitialized } = await authApi.status();
        setNeedsSetup(!isInitialized);
      } catch (err) {
        console.error("Failed to check auth status", err);
      } finally {
        setIsInitializing(false);
      }
    };
    checkStatus();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      const response = needsSetup 
        ? await authApi.setup({ password })
        : await authApi.login({ password });
        
      login(response);
      navigate(from, { replace: true });
    } catch (err: unknown) {
      setError(getAuthErrorMessage(err, needsSetup ? 'Failed to setup PIN.' : 'Invalid PIN.'));
    } finally {
      setIsLoading(false);
    }
  };

  if (isInitializing) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
        <div className="text-gray-500 text-lg animate-pulse">Loading system...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8 font-sans">
      <div className="max-w-md w-full space-y-8 bg-white p-8 rounded-2xl shadow-xl border border-gray-100">
        <div className="text-center">
          <div className="mx-auto bg-blue-50 w-16 h-16 rounded-full flex items-center justify-center mb-6 shadow-sm">
            {needsSetup ? <UserPlus className="h-8 w-8 text-blue-600" /> : <Lock className="h-8 w-8 text-blue-600" />}
          </div>
          <h2 className="text-3xl font-extrabold text-gray-900">
            {needsSetup ? 'Set System PIN' : 'System Locked'}
          </h2>
          <p className="mt-2 text-sm text-gray-500">
            {needsSetup ? 'Create a secure PIN to initialize your ERP system.' : 'Enter your secure PIN to access the dashboard.'}
          </p>
        </div>

        <form onSubmit={handleSubmit} className="mt-8 space-y-6">
          {error && (
            <div className="rounded-lg bg-red-50 p-4 border border-red-100 text-sm text-red-700 text-center font-medium">
              {error}
            </div>
          )}

          <div className="space-y-4">
            <div>
              <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1">
                {needsSetup ? 'New PIN' : 'System PIN'}
              </label>
              <div className="relative">
                <input
                  id="password"
                  name="password"
                  type={showPassword ? 'text' : 'password'}
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="appearance-none block w-full px-4 py-3 border border-gray-300 rounded-xl shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent sm:text-sm transition-all"
                  placeholder={needsSetup ? "Enter at least 4 characters" : "••••••••"}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600 transition-colors"
                >
                  {showPassword ? (
                    <EyeOff className="h-5 w-5" />
                  ) : (
                    <Eye className="h-5 w-5" />
                  )}
                </button>
              </div>
            </div>
          </div>

          <button
            type="submit"
            disabled={isLoading}
            className="w-full flex justify-center py-3 px-4 border border-transparent rounded-xl shadow-md text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200"
          >
            {isLoading ? 'Processing...' : needsSetup ? 'Set PIN & Enter' : 'Unlock System'}
          </button>
        </form>
      </div>
    </div>
  );
};
