import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import Layout from './components/Layout';
import ProtectedRoute from './components/ProtectedRoute';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import DashboardPage from './pages/DashboardPage';
import CarsPage from './pages/CarsPage';
import CarNewPage from './pages/CarNewPage';
import CarEditPage from './pages/CarEditPage';
import TiresPage from './pages/TiresPage';
import TireNewPage from './pages/TireNewPage';
import TireEditPage from './pages/TireEditPage';
import OrdersPage from './pages/OrdersPage';
import OrderNewPage from './pages/OrderNewPage';
import OrderDetailPage from './pages/OrderDetailPage';
import OrderEditPage from './pages/OrderEditPage';
import LandingPage from './pages/LandingPage';

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          {/* Public landing page without Layout wrapper */}
          <Route path="/" element={<LandingPage />} />

          {/* Auth pages */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* Protected routes with Layout */}
          <Route
            element={
              <ProtectedRoute>
                <Layout />
              </ProtectedRoute>
            }
          >
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/cars" element={<CarsPage />} />
            <Route path="/cars/new" element={<CarNewPage />} />
            <Route path="/cars/:id/edit" element={<CarEditPage />} />
            <Route path="/tires" element={<TiresPage />} />
            <Route path="/tires/new" element={<TireNewPage />} />
            <Route path="/tires/:id/edit" element={<TireEditPage />} />
            <Route path="/orders" element={<OrdersPage />} />
            <Route path="/orders/new" element={<OrderNewPage />} />
            <Route path="/orders/:id" element={<OrderDetailPage />} />
            <Route path="/orders/:id/edit" element={<OrderEditPage />} />
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
