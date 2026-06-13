import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { apiGet, apiPut } from '../api/client';
import type { OrderDto, ServiceDto, TimeSlotDto } from '../types';
import { Clock } from 'lucide-react';
import CalendarWidget from '../components/CalendarWidget';

export default function OrderEditPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [order, setOrder] = useState<OrderDto | null>(null);
  const [services, setServices] = useState<ServiceDto[]>([]);
  const [slots, setSlots] = useState<TimeSlotDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [selectedServices, setSelectedServices] = useState<number[]>([]);
  const [hasOther, setHasOther] = useState(false);
  const [selectedDate, setSelectedDate] = useState('');
  const [selectedTime, setSelectedTime] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!id) return;
    Promise.all([
      apiGet<OrderDto>(`/orders/${id}`),
      apiGet<ServiceDto[]>('/services'),
    ]).then(([orderData, servicesData]) => {
      setOrder(orderData);
      setServices(servicesData);

      const codes = orderData.services.map((s) => s.serviceCode);
      setSelectedServices(codes);

      if (orderData.scheduledAt) {
        const d = new Date(orderData.scheduledAt);
        setSelectedDate(d.toISOString().split('T')[0]);
        setSelectedTime(d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' }));
      }
    }).finally(() => setLoading(false));
  }, [id]);

  useEffect(() => {
    if (selectedDate) {
      apiGet<TimeSlotDto[]>(`/slots?date=${selectedDate}`)
        .then((s) => setSlots(s.filter((s) => s.available)))
        .catch(() => {});
    }
  }, [selectedDate]);

  const toggleService = (code: number) => {
    setSelectedServices((prev) =>
      prev.includes(code) ? prev.filter((c) => c !== code) : [...prev, code],
    );
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!selectedServices.length && !hasOther) { setError('Выберите услугу или укажите "Другое"'); return; }

    setSubmitting(true);
    try {
      const scheduledAt = selectedTime
        ? new Date(`${selectedDate}T${selectedTime}`).toISOString()
        : null;

      await apiPut(`/orders/${id}`, {
        serviceCodes: selectedServices.length > 0 ? selectedServices : null,
        hasOther,
        description: null,
        scheduledAt,
      });
      navigate(`/orders/${id}`);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Ошибка обновления заказа');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="max-w-2xl mx-auto px-4 py-8 space-y-4">
        {[1, 2, 3].map((i) => (
          <div key={i} className="card p-6 animate-pulse">
            <div className="h-4 bg-elevated rounded w-1/2 mb-3" />
            <div className="h-10 bg-elevated rounded" />
          </div>
        ))}
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      <h1 className="t-headline-lg text-2xl mb-6">Редактировать заказ #{id}</h1>

      <form onSubmit={handleSubmit} className="space-y-5">
        {error && <div className="bg-elevated text-base text-sm rounded-md p-3">{error}</div>}

        <div className="card p-6">
          <label className="t-title-md block mb-3">Услуги</label>
          <div className="space-y-2 mb-3">
            {services.map((s) => (
              <label key={s.serviceCode} className={`check p-3 rounded-lg border w-full justify-between ${selectedServices.includes(s.serviceCode) ? 'border-base bg-primary-soft' : 'border-border hover:border-border-strong'}`}>
                <div className="flex items-center gap-3">
                  <input type="checkbox" checked={selectedServices.includes(s.serviceCode)} onChange={() => toggleService(s.serviceCode)} />
                  <span className="text-sm text-base">{s.serviceName}</span>
                </div>
                <span className="text-sm font-medium text-muted">{s.serviceCost.toLocaleString('ru-RU')} ₽</span>
              </label>
            ))}
          </div>
          <label className="check gap-2 text-sm text-muted">
            <input type="checkbox" checked={hasOther} onChange={(e) => setHasOther(e.target.checked)} />
            Другое (консультация)
          </label>
        </div>

        <div className="card p-6">
          <label className="t-title-md block mb-4">Дата и время</label>
          <div className="grid md:grid-cols-2 gap-6">
            <CalendarWidget
              selectedDate={selectedDate}
              onChange={(d) => { setSelectedDate(d); setSelectedTime(''); }}
            />
            <div>
              <label className="field-label mb-3 block">
                <Clock size={14} className="inline mr-1" /> Время
              </label>
              {!selectedDate ? (
                <p className="text-sm text-faint">Выберите дату</p>
              ) : slots.length === 0 ? (
                <p className="text-sm text-faint">Нет свободных слотов</p>
              ) : (
                <div className="grid grid-cols-3 gap-1.5">
                  {slots.map((s) => {
                    const timeStr = new Date(s.time).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
                    return (
                      <button
                        key={s.time}
                        type="button"
                        onClick={() => setSelectedTime(timeStr)}
                        className={`tab justify-center text-xs ${selectedTime === timeStr ? 'is-active' : ''}`}
                      >
                        {timeStr}
                      </button>
                    );
                  })}
                </div>
              )}
            </div>
          </div>
        </div>

        <button type="submit" disabled={submitting} className="btn btn-primary w-full justify-center btn-lg">
          {submitting ? 'Сохранение...' : 'Сохранить'}
        </button>
      </form>
    </div>
  );
}
