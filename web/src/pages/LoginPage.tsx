import { useState, type FormEvent } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LogIn, ArrowLeft } from 'lucide-react';
import { cleanPhone, formatPhone } from '../utils/phoneMask';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [phone, setPhone] = useState('');
  const [pin, setPin] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await login(cleanPhone(phone), pin);
      await new Promise(r => setTimeout(r, 0));
      const calcState = JSON.parse(sessionStorage.getItem('calcState') ?? 'null');
      if (calcState?.fromCalculator) {
        navigate('/orders/new');
      } else {
        const from = (location.state as { from?: { pathname: string } } | null)?.from;
        navigate(from?.pathname ?? '/dashboard');
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Ошибка входа';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-canvas flex items-center justify-center px-4">
      <div className="w-full max-w-md">
        <div className="flex items-center justify-center mb-6 relative">
          <button type="button" onClick={() => navigate('/')} className="btn btn-tertiary btn-sm p-0 w-9 absolute left-0 top-1/2 -translate-y-1/2">
            <ArrowLeft size={18} />
          </button>
          <div className="text-center">
            <LogIn size={40} className="mx-auto text-muted mb-2" />
            <h1 className="t-headline-lg text-2xl">Вход</h1>
            <p className="text-muted mt-1 text-sm">Введите телефон и PIN-код</p>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="card p-6 space-y-4">
          {error && (
            <div className="bg-elevated text-base text-sm rounded-md p-3">{error}</div>
          )}

          <div className="field">
            <label className="field-label">Телефон</label>
            <input
              type="tel"
              value={phone}
              onChange={(e) => setPhone(cleanPhone(e.target.value))}
              onBlur={() => setPhone((prev) => formatPhone(prev))}
              placeholder="+7 (999) 123-45-67"
              className="input"
              required
            />
          </div>

          <div className="field">
            <label className="field-label">PIN-код</label>
            <input
              type="password"
              value={pin}
              onChange={(e) => setPin(e.target.value)}
              placeholder="4 цифры"
              maxLength={4}
              pattern="\d{4}"
              className="input"
              required
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            className="btn btn-primary w-full justify-center"
          >
            {loading ? 'Вход...' : 'Войти'}
          </button>

          <p className="text-center text-sm text-muted">
            Нет аккаунта?{' '}
            <Link to="/register" className="text-base hover:text-white font-medium">
              Зарегистрироваться
            </Link>
          </p>
        </form>
      </div>
    </div>
  );
}
