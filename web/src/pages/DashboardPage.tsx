import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { apiGet } from '../api/client';
import { useAuth } from '../context/AuthContext';
import type { CarDto, OrderDto } from '../types';
import { Car, ClipboardList, Plus } from 'lucide-react';

export default function DashboardPage() {
  const { user } = useAuth();
  const [cars, setCars] = useState<CarDto[]>([]);
  const [orders, setOrders] = useState<OrderDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([
      apiGet<CarDto[]>('/cars').then(setCars),
      apiGet<OrderDto[]>('/orders').then(setOrders),
    ]).finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-8 space-y-6">
        {[1, 2, 3].map((i) => (
          <div key={i} className="card p-6 animate-pulse">
            <div className="h-4 bg-elevated rounded w-1/3 mb-4" />
            <div className="h-3 bg-elevated rounded w-2/3" />
          </div>
        ))}
      </div>
    );
  }

  const statusColor = (s: string) => {
    switch (s) {
      case 'Новый': return 'neutral';
      case 'В работе': return 'warning';
      case 'Готов': return 'success';
      default: return 'neutral';
    }
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <div className="mb-8">
        <h1 className="t-headline-lg text-2xl">
          Здравствуйте, {user?.fullName}
        </h1>
        <p className="text-muted mt-1 text-sm">Ваш личный кабинет</p>
      </div>

      <div className="grid md:grid-cols-3 gap-5 mb-8">
        <div className="stat-tile" data-accent="success">
          <div className="stat-head">
            <span className="stat-eyebrow">Автомобили</span>
            <Car className="w-5 h-5 text-muted" />
          </div>
          <span className="stat-value">{cars.length}</span>
          <div className="stat-meta">
            <Link to="/cars" className="text-sm text-muted hover:text-base font-medium">
              Управлять
            </Link>
          </div>
        </div>

        <div className="stat-tile">
          <div className="stat-head">
            <span className="stat-eyebrow">Заказы</span>
            <ClipboardList className="w-5 h-5 text-muted" />
          </div>
          <span className="stat-value">{orders.length}</span>
          <div className="stat-meta">
            <Link to="/orders" className="text-sm text-muted hover:text-base font-medium">
              Все заказы
            </Link>
          </div>
        </div>

        <div className="stat-tile" data-accent="warning">
          <div className="stat-head">
            <span className="stat-eyebrow">Новый заказ</span>
            <Plus className="w-5 h-5 text-muted" />
          </div>
          <span className="stat-value">&mdash;</span>
          <div className="stat-meta">
            <Link to="/orders/new" className="btn btn-primary btn-sm">
              <Plus size={16} /> Создать
            </Link>
          </div>
        </div>
      </div>

      <div className="grid md:grid-cols-2 gap-5">
        <div className="card">
          <div className="flex items-center justify-between mb-4">
            <h2 className="t-title-md">Мои автомобили</h2>
            <Link to="/cars" className="text-sm text-muted hover:text-base">Все</Link>
          </div>
          {cars.length === 0 ? (
            <div className="text-center py-8 text-faint">
              <Car size={40} className="mx-auto mb-2 opacity-50" />
              <p className="text-sm">Нет добавленных автомобилей</p>
              <Link to="/cars/new" className="text-base text-sm hover:text-white font-medium block mt-2">
                Добавить
              </Link>
            </div>
          ) : (
            <div className="space-y-2">
              {cars.slice(0, 3).map((c) => (
                <Link key={c.carId} to={`/cars/${c.carId}/edit`} className="block p-3 rounded-lg border border-border hover:border-border-strong hover:bg-surface transition-colors">
                  <p className="font-medium text-base">{c.brand} {c.model}</p>
                  <p className="text-sm text-faint">{c.licensePlate} &middot; {c.manufactureYear}</p>
                </Link>
              ))}
            </div>
          )}
        </div>

        <div className="card">
          <div className="flex items-center justify-between mb-4">
            <h2 className="t-title-md">Последние заказы</h2>
            <Link to="/orders" className="text-sm text-muted hover:text-base">Все</Link>
          </div>
          {orders.length === 0 ? (
            <div className="text-center py-8 text-faint">
              <ClipboardList size={40} className="mx-auto mb-2 opacity-50" />
              <p className="text-sm">Нет заказов</p>
              <Link to="/orders/new" className="text-base text-sm hover:text-white font-medium block mt-2">
                Записаться
              </Link>
            </div>
          ) : (
            <div className="space-y-2">
              {orders.slice(0, 3).map((o) => (
                <Link key={o.orderNumber} to={`/orders/${o.orderNumber}`} className="block p-3 rounded-lg border border-border hover:border-border-strong hover:bg-surface transition-colors">
                  <div className="flex items-center justify-between">
                    <p className="font-medium text-base">Заказ #{o.orderNumber}</p>
                    <span className="chip" data-tone={statusColor(o.status)}>
                      {o.status}
                    </span>
                  </div>
                  <p className="text-sm text-faint mt-1">
                    {o.car ? `${o.car.brand} ${o.car.model}` : ''} &middot; {new Date(o.orderDate).toLocaleDateString('ru-RU')}
                  </p>
                </Link>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
