import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { UserPlus, ArrowLeft } from 'lucide-react';
import { cleanPhone, formatPhone } from '../utils/phoneMask';

export default function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [fullName, setFullName] = useState('');
  const [phone, setPhone] = useState('');
  const [pin, setPin] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    if (pin.length !== 4 || !/^\d{4}$/.test(pin)) {
      setError('PIN-код должен состоять из 4 цифр');
      return;
    }
    setLoading(true);
    try {
      await register(fullName, cleanPhone(phone), pin);
      await new Promise(r => setTimeout(r, 50));
      const calcState = JSON.parse(sessionStorage.getItem('calcState') ?? 'null');
      if (calcState?.fromCalculator) {
        navigate('/orders/new');
      } else {
        navigate('/dashboard');
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Ошибка регистрации';
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
            <UserPlus size={40} className="mx-auto text-muted mb-2" />
            <h1 className="t-headline-lg text-2xl">Регистрация</h1>
            <p className="text-muted mt-1 text-sm">Создайте аккаунт для доступа к сервисам</p>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="card p-6 space-y-4">
          {error && (
            <div className="bg-elevated text-base text-sm rounded-md p-3">{error}</div>
          )}

          <div className="field">
            <label className="field-label">Имя</label>
            <input
              type="text"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              placeholder="Иван Иванов"
              className="input"
              required
            />
          </div>

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
            <label className="field-label">PIN-код (4 цифры)</label>
            <input
              type="password"
              value={pin}
              onChange={(e) => setPin(e.target.value)}
              placeholder="••••"
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
            {loading ? 'Регистрация...' : 'Зарегистрироваться'}
          </button>

          <p className="text-center text-sm text-muted">
            Уже есть аккаунт?{' '}
            <Link to="/login" className="text-base hover:text-white font-medium">
              Войти
            </Link>
          </p>
        </form>
      </div>
    </div>
  );
}
