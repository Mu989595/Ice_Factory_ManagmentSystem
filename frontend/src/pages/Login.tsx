import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { authApi, getAuthErrorMessage } from '../api/auth';
import { useAuth } from '../hooks/useAuth';
import { Lock, UserPlus } from 'lucide-react';

export const Login = () => {
  const [password, setPassword] = useState('');
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
      setError(getAuthErrorMessage(err, needsSetup ? 'Failed to setup PIN.' : 'Failed to login. Please check your PIN.'));
    } finally {
      setIsLoading(false);
    }
  };

  if (isInitializing) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-900 px-4">
        <div className="text-white text-lg animate-pulse">Loading system...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-900 px-4">
      <div className="max-w-md w-full bg-gray-800 rounded-xl shadow-2xl p-8 border border-gray-700">
        <div className="text-center mb-8">
          <div className="mx-auto bg-blue-500/10 w-16 h-16 rounded-full flex items-center justify-center mb-4">
            {needsSetup ? <UserPlus className="h-8 w-8 text-blue-400" /> : <Lock className="h-8 w-8 text-blue-400" />}
          </div>
          <h2 className="text-3xl font-bold text-white">Ice Factory ERP</h2>
          <p className="text-gray-400 mt-2">
            {needsSetup ? 'Welcome! Create a PIN to secure your system.' : 'System Locked. Enter PIN to unlock.'}
          </p>
        </div>

        {error && (
          <div className="bg-red-500/10 border border-red-500/50 text-red-400 p-3 rounded-lg mb-6 text-sm text-center">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-6">
          <div>
            <label className="block text-sm font-medium text-gray-300 mb-2">
              {needsSetup ? 'New System PIN' : 'System PIN'}
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <Lock className="h-5 w-5 text-gray-500" />
              </div>
              <input
                type="password"
                required
                className="block w-full pl-10 pr-3 py-2 border border-gray-600 rounded-lg bg-gray-700 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                placeholder={needsSetup ? 'Enter 4+ characters' : 'Enter PIN'}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
            </div>
          </div>

          <button
            type="submit"
            disabled={isLoading}
            className="w-full flex justify-center py-3 px-4 border border-transparent rounded-lg shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {isLoading ? 'Processing...' : needsSetup ? 'Set PIN & Enter' : 'Unlock System'}
          </button>
        </form>
      </div>
    </div>
  );
};
