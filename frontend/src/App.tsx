import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Layout } from './components/Layout';
import { 
  Dashboard, 
  Basin, 
  Sales, 
  Expenses, 
  Workers, 
  Attendance, 
  MonthlySummary, 
  ProductionLog 
} from './pages';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30000,
      retry: 1,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Layout />}>
            <Route index element={<Dashboard />} />
            <Route path="basin" element={<Basin />} />
            <Route path="sales" element={<Sales />} />
            <Route path="expenses" element={<Expenses />} />
            <Route path="workers" element={<Workers />} />
            <Route path="attendance" element={<Attendance />} />
            <Route path="monthly" element={<MonthlySummary />} />
            <Route path="production" element={<ProductionLog />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
