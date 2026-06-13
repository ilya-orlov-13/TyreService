import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { apiGet, apiDelete } from '../api/client';
import type { OrderDto } from '../types';
import { ClipboardList, Plus, X } from 'lucide-react';

export default function OrdersPage() {
  const [orders, setOrders] = useState<OrderDto[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    apiGet<OrderDto[]>('/orders')
      .then(setOrders)
      .finally(() => setLoading(false));
  }, []);

  const handleCancel = async (id: number) => {
    if (!confirm('Отменить заказ?')) return;
    try {
      await apiDelete(`/orders/${id}`);
      setOrders((prev) => prev.filter((o) => o.orderNumber !== id));
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Ошибка');
    }
  };

  if (loading) {
    return (
      <div className="max-w-4xl mx-auto px-4 py-8 space-y-4">
        {[1, 2, 3].map((i) => (
          <div key={i} className="card p-6 animate-pulse">
            <div className="h-4 bg-elevated rounded w-1/3 mb-3" />
            <div className="h-3 bg-elevated rounded w-1/2" />
          </div>
        ))}
      </div>
    );
  }

  const statusTone = (s: string) => {
    switch (s) {
      case 'Новый': return 'neutral';
      case 'В работе': return 'warning';
      case 'Готов': return 'success';
      case 'Оплачено': return 'success';
      default: return 'neutral';
    }
  };

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="t-headline-lg text-2xl">Мои заказы</h1>
        <Link to="/orders/new" className="btn btn-primary btn-sm">
          <Plus size={18} /> Новый заказ
        </Link>
      </div>

      {orders.length === 0 ? (
        <div className="text-center py-16 text-faint">
          <ClipboardList size={64} className="mx-auto mb-4 opacity-30" />
          <p className="text-lg mb-2 text-muted">Нет заказов</p>
          <p className="text-sm mb-4 text-faint">Запишитесь на шиномонтаж</p>
          <Link to="/orders/new" className="btn btn-primary">
            <Plus size={18} /> Записаться
          </Link>
        </div>
      ) : (
        <div className="space-y-3">
          {orders.map((o) => (
            <div key={o.orderNumber} className="card p-5">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-1">
                    <h3 className="font-semibold text-base">Заказ #{o.orderNumber}</h3>
                    <span className={`chip`} data-tone={statusTone(o.status)}>
                      {o.status}
                    </span>
                    <span className="t-mono-sm">{o.paymentStatus}</span>
                  </div>
                  <p className="text-sm text-muted">
                    {new Date(o.orderDate).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', hour: '2-digit', minute: '2-digit' })}
                  </p>
                  {o.car && (
                    <p className="text-sm text-muted mt-1">{o.car.brand} {o.car.model} &middot; {o.car.licensePlate}</p>
                  )}
                  {o.services.length > 0 && (
                    <div className="flex flex-wrap gap-1 mt-2">
                      {o.services.map((s) => (
                        <span key={s.workId} className="chip" data-tone="neutral">{s.serviceName}</span>
                      ))}
                    </div>
                  )}
                  {o.scheduledAt && (
                    <p className="text-sm text-faint mt-2">
                      Запланировано: {new Date(o.scheduledAt).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', hour: '2-digit', minute: '2-digit' })}
                    </p>
                  )}
                </div>
              </div>
              <div className="flex gap-3 mt-3 pt-3 border-t border-border">
                <button onClick={() => navigate(`/orders/${o.orderNumber}`)} className="text-sm text-muted hover:text-base font-medium">
                  Подробнее
                </button>
                {o.status === 'Новый' && (
                  <>
                    <button onClick={() => navigate(`/orders/${o.orderNumber}/edit`)} className="text-sm text-muted hover:text-base font-medium">
                      Редактировать
                    </button>
                    <button onClick={() => handleCancel(o.orderNumber)} className="flex items-center gap-1 text-sm text-faint hover:text-base font-medium ml-auto">
                      <X size={14} /> Отменить
                    </button>
                  </>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
