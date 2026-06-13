import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { apiGet, apiDelete, apiPost } from '../api/client';
import type { OrderDto, ReviewDto, CreateReviewRequest } from '../types';
import { ArrowLeft, Trash2, Star } from 'lucide-react';

export default function OrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [order, setOrder] = useState<OrderDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [reviewRating, setReviewRating] = useState(5);
  const [reviewText, setReviewText] = useState('');
  const [reviewSubmitting, setReviewSubmitting] = useState(false);
  const [reviewError, setReviewError] = useState('');
  const [reviewSuccess, setReviewSuccess] = useState(false);

  useEffect(() => {
    if (!id) return;
    apiGet<OrderDto>(`/orders/${id}`)
      .then(setOrder)
      .finally(() => setLoading(false));
  }, [id]);

  const canReview = order?.status === 'Готов' || order?.status === 'Оплачено';

  const handleSubmitReview = async () => {
    if (!order) return;
    if (reviewText.trim().length < 10) {
      setReviewError('Текст отзыва должен содержать не менее 10 символов');
      return;
    }
    setReviewError('');
    setReviewSubmitting(true);
    try {
      const body: CreateReviewRequest = {
        rating: reviewRating,
        text: reviewText.trim(),
        orderNumber: order.orderNumber,
      };
      await apiPost<ReviewDto>('/reviews', body);
      setReviewSuccess(true);
    } catch (err) {
      setReviewError(err instanceof Error ? err.message : 'Ошибка при отправке');
    } finally {
      setReviewSubmitting(false);
    }
  };

  const handleCancel = async () => {
    if (!confirm('Отменить заказ?')) return;
    try {
      await apiDelete(`/orders/${id}`);
      navigate('/orders');
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Ошибка');
    }
  };

  if (loading) {
    return (
      <div className="max-w-3xl mx-auto px-4 py-8">
        <div className="card p-6 animate-pulse space-y-4">
          <div className="h-6 bg-elevated rounded w-48" />
          {[1, 2, 3].map((i) => <div key={i} className="h-4 bg-elevated rounded w-2/3" />)}
        </div>
      </div>
    );
  }

  if (!order) {
    return (
      <div className="max-w-3xl mx-auto px-4 py-8 text-center">
        <p className="text-faint">Заказ не найден</p>
        <Link to="/orders" className="text-base hover:text-white text-sm mt-2 inline-block">Вернуться к заказам</Link>
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
    <div className="max-w-3xl mx-auto px-4 py-8">
      <button onClick={() => navigate('/orders')} className="flex items-center gap-1 text-sm text-muted hover:text-base mb-4">
        <ArrowLeft size={16} /> Назад к заказам
      </button>

      <div className="card p-0 overflow-hidden">
        <div className="p-6 border-b border-border">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="t-headline-lg text-2xl">Заказ #{order.orderNumber}</h1>
              <div className="flex items-center gap-2 mt-1">
                <span className={`chip`} data-tone={statusTone(order.status)}>
                  {order.status}
                </span>
                <span className="t-mono-sm">{order.paymentStatus}</span>
              </div>
            </div>
            {order.status === 'Новый' && (
              <div className="flex gap-2">
                <button onClick={() => navigate(`/orders/${id}/edit`)} className="btn btn-secondary btn-sm">Редактировать</button>
                <button onClick={handleCancel} className="btn btn-danger btn-sm">
                  <Trash2 size={14} /> Отменить
                </button>
              </div>
            )}
          </div>
        </div>

        <div className="p-6 space-y-5">
          <div className="grid md:grid-cols-2 gap-4">
            <div>
              <p className="t-label-sm mb-1">Дата заказа</p>
              <p className="font-medium text-base">{new Date(order.orderDate).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric', hour: '2-digit', minute: '2-digit' })}</p>
            </div>
            {order.scheduledAt && (
              <div>
                <p className="t-label-sm mb-1">Запланировано</p>
                <p className="font-medium text-base">{new Date(order.scheduledAt).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', hour: '2-digit', minute: '2-digit' })}</p>
              </div>
            )}
          </div>

          {order.car && (
            <div>
              <p className="t-label-sm mb-1">Автомобиль</p>
              <p className="font-medium text-base">{order.car.brand} {order.car.model}</p>
              <p className="text-sm text-muted">{order.car.licensePlate} &middot; {order.car.manufactureYear} г.</p>
            </div>
          )}

          {order.masterName && (
            <div>
              <p className="t-label-sm mb-1">Мастер</p>
              <p className="font-medium text-base">{order.masterName}</p>
            </div>
          )}

          <div>
            <p className="t-label-sm mb-2">Услуги</p>
            {order.services.length === 0 ? (
              <p className="text-sm text-faint">Нет услуг</p>
            ) : (
              <div className="space-y-2">
                {order.services.map((s) => (
                  <div key={s.workId} className="flex items-center justify-between p-3 bg-surface rounded-lg border border-border">
                    <span className="text-sm text-base">{s.serviceName}</span>
                    <span className="text-sm font-medium text-muted">{s.workTotal.toLocaleString('ru-RU')} ₽</span>
                  </div>
                ))}
              </div>
            )}
          </div>

          {order.clientTotal != null && (
            <div className="pt-4 border-t border-border flex justify-between">
              <span className="t-title-md">Итого</span>
              <span className="t-metric text-lg text-base">{order.clientTotal.toLocaleString('ru-RU')} ₽</span>
            </div>
          )}
        </div>
      </div>

      {canReview && !reviewSuccess && (
        <div className="card p-6 mt-6">
          <h2 className="t-headline-md text-lg mb-4">Оставить отзыв</h2>
          <div className="space-y-4">
            <div>
              <p className="t-label-sm mb-2">Оценка</p>
              <div className="flex gap-1">
                {[1, 2, 3, 4, 5].map((s) => (
                  <button
                    key={s}
                    type="button"
                    onClick={() => setReviewRating(s)}
                    className="p-1 transition-colors"
                  >
                    <Star
                      size={24}
                      className={s <= reviewRating ? 'text-muted fill-muted' : 'text-faint/30'}
                    />
                  </button>
                ))}
              </div>
            </div>
            <div>
              <textarea
                value={reviewText}
                onChange={(e) => setReviewText(e.target.value)}
                placeholder="Расскажите о качестве обслуживания..."
                rows={4}
                className="w-full bg-surface border border-border rounded-lg p-3 text-sm focus:outline-none focus:border-base resize-none"
                maxLength={1000}
              />
              <p className="t-mono-sm text-faint mt-1 text-right">{reviewText.length}/1000</p>
            </div>
            {reviewError && <p className="text-sm text-red-400">{reviewError}</p>}
            <button
              type="button"
              onClick={handleSubmitReview}
              disabled={reviewSubmitting}
              className="btn btn-primary w-full justify-center"
            >
              {reviewSubmitting ? 'Отправка...' : 'Отправить отзыв'}
            </button>
          </div>
        </div>
      )}

      {reviewSuccess && (
        <div className="card p-6 mt-6 text-center">
          <p className="text-green-400 text-lg font-medium">Спасибо! Отзыв отправлен.</p>
          <p className="text-sm text-muted mt-1">После проверки он появится на сайте.</p>
        </div>
      )}
    </div>
  );
}
