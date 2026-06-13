import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LogOut, Menu, X } from 'lucide-react';
import { useState } from 'react';
import { SERVER_BASE } from '../api/client';

export default function Layout() {
  const { user, logout, isAuthenticated } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [menuOpen, setMenuOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  const navLinks = [
    { to: '/dashboard', label: 'Главная' },
    { to: '/cars', label: 'Автомобили' },
    { to: '/tires', label: 'Шины' },
    { to: '/orders', label: 'Заказы' },
  ];

  return (
    <div className="min-h-screen bg-canvas">
      <header className="bg-surface border-b border-border sticky top-0 z-[1000]">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            <Link to={isAuthenticated ? '/dashboard' : '/'} className="flex items-center gap-2 text-xl font-bold text-base">
              <img src={SERVER_BASE + '/images/logo.png'} alt="Шиномонтаж у Бориса" className="w-6 h-6 object-contain" />
              Шиномонтаж у Бориса
            </Link>

            <nav className="hidden md:flex items-center gap-6">
              {navLinks.map((l) => (
                <Link
                  key={l.to}
                  to={l.to}
                  className="text-sm font-medium text-muted hover:text-base transition-colors"
                >
                  {l.label}
                </Link>
              ))}
              {isAuthenticated ? (
                <div className="flex items-center gap-3 ml-4 pl-4 border-l border-border">
                  <span className="text-sm text-muted">{user?.fullName}</span>
                  <button
                    onClick={handleLogout}
                    className="flex items-center gap-1 text-sm text-faint hover:text-base transition-colors"
                  >
                    <LogOut size={16} /> Выйти
                  </button>
                </div>
              ) : (
                <Link
                  to="/login"
                  className="text-sm font-medium text-muted hover:text-base"
                >
                  Войти
                </Link>
              )}
            </nav>

            <button className="md:hidden p-2 text-muted" onClick={() => setMenuOpen(!menuOpen)}>
              {menuOpen ? <X size={24} /> : <Menu size={24} />}
            </button>
          </div>
        </div>

        {menuOpen && (
          <div className="md:hidden border-t border-border bg-surface">
            <div className="px-4 py-3 space-y-2">
              {navLinks.map((l) => (
                <Link
                  key={l.to}
                  to={l.to}
                  onClick={() => setMenuOpen(false)}
                  className="block text-sm font-medium text-muted hover:text-base py-2"
                >
                  {l.label}
                </Link>
              ))}
              {isAuthenticated ? (
                <button
                  onClick={() => { handleLogout(); setMenuOpen(false); }}
                  className="flex items-center gap-1 text-sm text-faint hover:text-base py-2"
                >
                  <LogOut size={16} /> Выйти
                </button>
              ) : (
                <Link to="/login" onClick={() => setMenuOpen(false)} className="block text-sm font-medium text-base py-2">
                  Войти
                </Link>
              )}
            </div>
          </div>
        )}
      </header>

      <main>
        <Outlet />
      </main>

      <footer className="border-t border-border text-faint py-8">
        <div className="max-w-7xl mx-auto px-4 text-center text-sm">
          &copy; {new Date().getFullYear()} Шиномонтаж у Бориса. Все права защищены.
        </div>
      </footer>
    </div>
  );
}
