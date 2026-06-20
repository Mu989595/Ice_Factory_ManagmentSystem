import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Layout } from './components/Layout';
import { ProtectedRoute } from './components/ProtectedRoute';
import { AuthProvider } from './hooks/useAuth';
import {
  Dashboard,
  Basin,
  Sales,
  Expenses,
  Workers,
  MonthlySummary,
  ProductionLog,
  Login
} from './pages';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 0,
      retry: 1,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/login" element={<Login />} />
            <Route element={<ProtectedRoute />}>
              <Route path="/" element={<Layout />}>
                <Route index element={<Dashboard />} />
                <Route path="basin" element={<Basin />} />
                <Route path="sales" element={<Sales />} />
                <Route path="expenses" element={<Expenses />} />
                <Route path="workers" element={<Workers />} />
                <Route path="monthly" element={<MonthlySummary />} />
                <Route path="production" element={<ProductionLog />} />
              </Route>
            </Route>
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
