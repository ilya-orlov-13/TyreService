import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { apiGet, apiPost } from '../api/client';
import type { CarDto, ServiceDto, TimeSlotDto, TireDto } from '../types';
import { Clock, Info, Car, Truck } from 'lucide-react';
import CalendarWidget from '../components/CalendarWidget';

type OrderMode = 'car' | 'tire';

export default function OrderNewPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const calcState = (location.state as Record<string, unknown> | null)
    ?? JSON.parse(sessionStorage.getItem('calcState') ?? 'null');
  const [cars, setCars] = useState<CarDto[]>([]);
  const [tires, setTires] = useState<TireDto[]>([]);
  const [services, setServices] = useState<ServiceDto[]>([]);
  const [slots, setSlots] = useState<TimeSlotDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [orderMode, setOrderMode] = useState<OrderMode>('car');
  const [selectedCarId, setSelectedCarId] = useState<number | ''>('');
  const [selectedTireId, setSelectedTireId] = useState<number | ''>('');
  const [selectedServices, setSelectedServices] = useState<number[]>([]);
  const [hasOther, setHasOther] = useState(false);
  const [selectedDate, setSelectedDate] = useState(new Date().toISOString().split('T')[0]);
  const [selectedTime, setSelectedTime] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const isStorage = orderMode === 'tire';
  const filteredServices = isStorage
    ? services.filter(s => s.serviceName.toLowerCase().includes('хранен'))
    : services;

  useEffect(() => {
    Promise.all([
      apiGet<CarDto[]>('/cars'),
      apiGet<TireDto[]>('/tires'),
      apiGet<ServiceDto[]>('/services'),
    ]).then(([carsData, tiresData, servicesData]) => {
      setCars(carsData);
      setTires(tiresData);
      setServices(servicesData);
      if (calcState?.fromCalculator) {
        const ids = (calcState.serviceIds as (string | number)[]) ?? [];
        const keywordMap: Record<string, string> = {
          '1': 'шиномонтаж',
          '2': 'балансировк',
          '3': 'правк',
          '4': 'вулканизаци',
          '5': 'хранен',
          '6': 'чернен',
        };
        const matched = servicesData.filter(s =>
          ids.some(id => {
            const kw = keywordMap[String(id)];
            return kw && s.serviceName.toLowerCase().includes(kw);
          })
        ).map(s => s.serviceCode);
        if (matched.length > 0) setSelectedServices(matched);
        const hasStorage = ids.some(id => String(id) === '5');
        if (hasStorage && tiresData.length > 0) {
          setOrderMode('tire');
          setSelectedTireId(tiresData[0].tireId);
        }
        sessionStorage.removeItem('calcState');
      }
    }).finally(() => setLoading(false));
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

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
    if (isStorage) {
      if (!selectedTireId) { setError('Выберите шину'); return; }
    } else {
      if (!selectedCarId) { setError('Выберите автомобиль'); return; }
    }
    if (!selectedServices.length && !hasOther) { setError('Выберите услугу или укажите "Другое"'); return; }

    setSubmitting(true);
    try {
      const scheduledAt = selectedTime
        ? new Date(`${selectedDate}T${selectedTime}`).toISOString()
        : null;

      await apiPost('/orders', {
        carId: isStorage ? null : selectedCarId,
        tireId: isStorage ? selectedTireId : null,
        serviceCodes: selectedServices.length > 0 ? selectedServices : null,
        hasOther,
        description: null,
        scheduledAt,
        wheelCount: (calcState?.wheels as number) ?? undefined,
      });
      navigate('/orders');
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { error?: string } } };
      setError(axiosErr?.response?.data?.error ?? (err instanceof Error ? err.message : 'Ошибка создания заказа'));
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
      <h1 className="t-headline-lg text-2xl mb-6">Новый заказ</h1>

      {calcState?.fromCalculator && (
        <div className="card p-4 mb-6 flex items-start gap-3 border-primary/30 bg-primary-soft/40">
          <Info size={18} className="text-primary mt-0.5 shrink-0" />
          <div className="text-sm">
            <p className="font-medium text-base">Расчёт из калькулятора</p>
            <p className="text-muted mt-1">{calcState.wheels} колеса, диаметр {calcState.radiusLabel}</p>
            <p className="text-muted">{services.filter(s => selectedServices.includes(s.serviceCode)).map(s => s.serviceName).join(', ') || 'услуги не выбраны'}</p>
          </div>
        </div>
      )}

      {/* Mode toggle */}
      <div className="flex gap-2 mb-5">
        <button
          type="button"
          onClick={() => { setOrderMode('car'); setSelectedTireId(''); setSelectedServices([]); }}
          className={`tab flex items-center gap-2 px-4 py-2 ${orderMode === 'car' ? 'is-active' : ''}`}
        >
          <Car size={16} /> Обслуживание авто
        </button>
        <button
          type="button"
          onClick={() => { setOrderMode('tire'); setSelectedCarId(''); setSelectedServices([]); }}
          className={`tab flex items-center gap-2 px-4 py-2 ${orderMode === 'tire' ? 'is-active' : ''}`}
        >
          <Truck size={16} /> Хранение шин
        </button>
      </div>

      <form onSubmit={handleSubmit} className="space-y-5">
        {error && <div className="bg-elevated text-base text-sm rounded-md p-3">{error}</div>}

        {isStorage ? (
          <div className="card p-6">
            <label className="t-title-md block mb-3">Шина для хранения</label>
            {tires.length === 0 ? (
              <p className="text-sm text-faint">Сначала добавьте шину</p>
            ) : (
              <div className="space-y-2">
                {tires.map((t) => (
                  <label key={t.tireId} className={`check p-3 rounded-lg border w-full ${selectedTireId === t.tireId ? 'border-base bg-primary-soft' : 'border-border hover:border-border-strong'}`}>
                    <input type="radio" name="tire" value={t.tireId} checked={selectedTireId === t.tireId} onChange={() => setSelectedTireId(t.tireId)} />
                    <div>
                      <p className="font-medium text-base">{t.manufacturer} {t.tireModel}</p>
                      <p className="text-sm text-muted">{t.size} &middot; {t.seasonality} &middot; {t.wearPercentage}%</p>
                    </div>
                  </label>
                ))}
              </div>
            )}
          </div>
        ) : (
          <div className="card p-6">
            <label className="t-title-md block mb-3">Автомобиль</label>
            {cars.length === 0 ? (
              <p className="text-sm text-faint">Сначала добавьте автомобиль</p>
            ) : (
              <div className="space-y-2">
                {cars.map((c) => (
                  <label key={c.carId} className={`check p-3 rounded-lg border w-full ${selectedCarId === c.carId ? 'border-base bg-primary-soft' : 'border-border hover:border-border-strong'}`}>
                    <input type="radio" name="car" value={c.carId} checked={selectedCarId === c.carId} onChange={() => setSelectedCarId(c.carId)} />
                    <div>
                      <p className="font-medium text-base">{c.brand} {c.model}</p>
                      <p className="text-sm text-muted">{c.licensePlate} &middot; {c.manufactureYear}</p>
                    </div>
                  </label>
                ))}
              </div>
            )}
          </div>
        )}

        <div className="card p-6">
          <label className="t-title-md block mb-3">Услуги</label>
          {filteredServices.length === 0 ? (
            <p className="text-sm text-faint">Нет доступных услуг</p>
          ) : (
            <div className="space-y-2 mb-3">
              {filteredServices.map((s) => (
                <label key={s.serviceCode} className={`check p-3 rounded-lg border w-full justify-between ${selectedServices.includes(s.serviceCode) ? 'border-base bg-primary-soft' : 'border-border hover:border-border-strong'}`}>
                  <div className="flex items-center gap-3">
                    <input type="checkbox" checked={selectedServices.includes(s.serviceCode)} onChange={() => toggleService(s.serviceCode)} />
                    <span className="text-sm text-base">{s.serviceName}</span>
                  </div>
                  <span className="text-sm font-medium text-muted">{s.serviceCost.toLocaleString('ru-RU')} ₽</span>
                </label>
              ))}
            </div>
          )}
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
          {submitting ? 'Создание...' : 'Записаться'}
        </button>
      </form>
    </div>
  );
}
