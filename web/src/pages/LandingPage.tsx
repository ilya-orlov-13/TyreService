/// <reference types="yandex-maps" />

import { useState, useRef, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { motion } from 'motion/react';
import {
  Car, Shield, Clock, Award, Phone, MapPin, Mail, Star, Wrench,
  ArrowRight, Menu, X, ChevronRight, Gauge, Zap, TrendingUp
} from 'lucide-react';
import { INITIAL_SERVICES } from '../data';
import { apiGetPublic, SERVER_BASE } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { ADDRESS, YANDEX_MAPS_URL } from '../constants/location';
import type { PublicStatsDto, ReviewDto } from '../types';

const navLinks = [
  { label: 'Услуги', href: '#services' },
  { label: 'Калькулятор', href: '#calculator' },
  { label: 'Отзывы', href: '#reviews' },
  { label: 'Контакты', href: '#contacts' },
  { label: 'Карта', href: '#map' },
];

const benefits = [
  { icon: Clock, title: '35 минут', desc: 'Среднее время замены. Мы ценим ваше время.' },
  { icon: Shield, title: '12 месяцев', desc: 'Официальная гарантия на все работы.' },
  { icon: Award, title: '10+ лет', desc: 'На рынке, 3 филиала в Санкт-Петербурге.' },
  { icon: Car, title: 'Шиномонтаж у Бориса', desc: 'Профессиональное оборудование и мастера.' },
];

const sizeKeys = ['R13_14', 'R15_16', 'R17_18', 'R19_20', 'R21+'] as const;
const sizeLabels = ['R13–14', 'R15–16', 'R17–18', 'R19–20', 'R21+'];

const sparkline = (
  <svg className="stat-spark" viewBox="0 0 96 32" fill="none">
    <defs>
      <linearGradient id="spark" x1="0" y1="0" x2="0" y2="1">
        <stop offset="0%" stopColor="currentColor" stopOpacity="0.25" />
        <stop offset="100%" stopColor="currentColor" stopOpacity="0" />
      </linearGradient>
    </defs>
    <path d="M2 28 L12 22 L22 24 L32 16 L42 18 L52 10 L62 14 L72 6 L82 10 L94 4" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" fill="none" />
    <path d="M2 28 L12 22 L22 24 L32 16 L42 18 L52 10 L62 14 L72 6 L82 10 L94 4 L94 32 L2 32 Z" fill="url(#spark)" />
  </svg>
);

function getPrice(service: typeof INITIAL_SERVICES[0], key: string): number {
  switch (key) {
    case 'R13_14': return service.priceR13_14;
    case 'R15_16': return service.priceR15_16;
    case 'R17_18': return service.priceR17_18;
    case 'R19_20': return service.priceR19_20;
    case 'R21+': return service.priceR21_plus;
    default: return 0;
  }
}

export default function LandingPage() {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  const mapRef = useRef<HTMLDivElement>(null);
  const mapInstance = useRef<ymaps.Map | null>(null);
  const [menuOpen, setMenuOpen] = useState(false);
  const [scrolled, setScrolled] = useState(false);
  const [activeRadius, setActiveRadius] = useState<number>(2);
  const [calcRadius, setCalcRadius] = useState(2);
  const [calcWheels, setCalcWheels] = useState(4);
  const [calcServices, setCalcServices] = useState<string[]>(['1', '2']);
  const [stats, setStats] = useState<PublicStatsDto | null>(null);
  const [reviews, setReviews] = useState<ReviewDto[]>([]);

  useEffect(() => {
    apiGetPublic<PublicStatsDto>('/api/public/stats').then(setStats).catch(() => {});
    apiGetPublic<ReviewDto[]>('/api/public/reviews').then(setReviews).catch(() => {});
  }, []);

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 40);
    window.addEventListener('scroll', onScroll, { passive: true });
    return () => window.removeEventListener('scroll', onScroll);
  }, []);

  useEffect(() => {
    const container = mapRef.current;
    if (!container) return;

    function initMap() {
      if (!window.ymaps || !container) return;
      const map = new window.ymaps.Map(container, {
        center: [59.852798, 30.289399],
        zoom: 17,
        controls: [],
        type: 'yandex#map',
      }, {
        suppressMapOpenBlock: true,
        yandexMapDisablePoiInteractivity: true,
        minZoom: 10,
        maxZoom: 18,
      } as ymaps.IMapOptions);

      const marker = new window.ymaps.Placemark([59.852798, 30.289399], {
        iconCaption: 'Шиномонтаж у Бориса',
        balloonContent: [
          '<div style="font-family:Inter,system-ui,sans-serif;padding:6px 2px;min-width:220px">',
          '<div style="display:flex;align-items:center;gap:10px;margin-bottom:10px">',
          '<div style="width:36px;height:36px;border-radius:10px;background:#FC3F1D;display:flex;align-items:center;justify-content:center;flex-shrink:0">',
          '<svg viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="1.5" width="18" height="18"><path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"/></svg>',
          '</div>',
          '<div>',
          '<strong style="font-size:15px;color:#fff;display:block;line-height:1.3">Шиномонтаж у Бориса</strong>',
          '<span style="font-size:12px;color:#9AA0AE">Санкт-Петербург</span>',
          '</div>',
          '</div>',
          '<div style="display:flex;flex-direction:column;gap:5px">',
          '<div style="display:flex;align-items:center;gap:8px;font-size:13px;color:#C8CCD4">',
          '<span style="width:14px;text-align:center;flex-shrink:0;display:inline-flex;align-items:center;justify-content:center"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" width="14" height="14"><path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z"/><circle cx="12" cy="10" r="3"/></svg></span>',
          '<span>Ленинский просп., 146, корп. 1 (этаж 1)</span>',
          '</div>',
          '<div style="display:flex;align-items:center;gap:8px;font-size:13px;color:#C8CCD4">',
          '<span style="width:14px;text-align:center;flex-shrink:0;display:inline-flex;align-items:center;justify-content:center"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" width="14" height="14"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"/></svg></span>',
          '<span>+7 (812) 333-21-21</span>',
          '</div>',
          '<div style="display:flex;align-items:center;gap:8px;font-size:13px;color:#C8CCD4">',
          '<span style="width:14px;text-align:center;flex-shrink:0;display:inline-flex;align-items:center;justify-content:center"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" width="14" height="14"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg></span>',
          '<span>Круглосуточно, ежедневно</span>',
          '</div>',
          '</div>',
          '</div>',
        ].join(''),
        hintContent: 'Шиномонтаж у Бориса — Ленинский пр., 146к1',
      }, {
        preset: 'islands#redCircleDotIcon',
        iconColor: '#d33',
        hideIconOnBalloonOpen: false,
        openBalloonOnClick: true,
      });

      map.geoObjects.add(marker);
      map.events.add('click', function (e: ymaps.IEvent) {
        const target = e.get('target');
        if (target === map) {
          map.balloon.close();
        }
      });
      mapInstance.current = map;
    }

    function loadApi() {
      const apiKey = import.meta.env.VITE_YMAPS_API_KEY;
      const existingScript = document.querySelector<HTMLScriptElement>('script[src*="api-maps.yandex.ru/2.1"]');

      if (existingScript && window.ymaps) {
        window.ymaps.ready(initMap);
      } else if (existingScript) {
        existingScript.addEventListener('load', () => window.ymaps?.ready(initMap));
      } else {
        const script = document.createElement('script');
        script.src = `https://api-maps.yandex.ru/2.1/?apikey=${apiKey}&lang=ru_RU`;
        script.onload = () => window.ymaps?.ready(initMap);
        document.head.appendChild(script);
      }
    }

    loadApi();

    return () => {
      if (mapInstance.current) {
        mapInstance.current.destroy();
        mapInstance.current = null;
      }
    };
  }, []);

  const servicesToShow = INITIAL_SERVICES.filter(s => s.category !== 'storage');

  const calcTotal = () => {
    let total = 0;
    const multiplier = calcWheels / 4;
    const sizeKey = sizeKeys[calcRadius];
    for (const id of calcServices) {
      const service = INITIAL_SERVICES.find(s => s.id === id);
      if (!service) continue;
      const price = getPrice(service, sizeKey);
      if (service.category === 'tire-fitting' || service.category === 'balancing') {
        total += price * multiplier;
      } else {
        total += price;
      }
    }
    return total;
  };

  return (
    <div className="min-h-screen bg-canvas text-base">
      <header
        className={`fixed top-0 left-0 right-0 z-[1000] transition-all duration-300 ${
          scrolled ? 'bg-canvas/90 backdrop-blur-md border-b border-border' : 'bg-transparent'
        }`}
      >
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16 md:h-20">
            <Link to="/" className="flex items-center gap-3 font-semibold text-base">
              <img src={SERVER_BASE + '/images/logo.png'} alt="Шиномонтаж у Бориса" className="w-6 h-6 object-contain" />
              <span className={scrolled ? 'text-base' : 'text-white'}>
                Шиномонтаж у Бориса
              </span>
            </Link>

            <nav className="hidden md:flex items-center gap-8">
              {navLinks.map(link => (
                <a
                  key={link.href}
                  href={link.href}
                  className={`text-sm font-medium transition-colors hover:text-base ${
                    scrolled ? 'text-muted' : 'text-white/80'
                  }`}
                >
                  {link.label}
                </a>
              ))}
            </nav>

            <div className="hidden md:flex items-center gap-3">
              <Link
                to="/login"
                className={`text-sm font-medium transition-colors ${
                  scrolled ? 'text-muted hover:text-base' : 'text-white/80 hover:text-white'
                }`}
              >
                Войти
              </Link>
              <Link
                to="/register"
                className={`btn btn-sm ${scrolled ? 'btn-primary' : 'bg-white/10 text-white border-white/20 hover:bg-white/20'}`}
              >
                Записаться
              </Link>
            </div>

            <button
              onClick={() => setMenuOpen(!menuOpen)}
              className={`md:hidden p-2 rounded-md transition-colors ${
                scrolled ? 'text-muted' : 'text-white'
              }`}
            >
              {menuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
            </button>
          </div>
        </div>

        {menuOpen && (
          <motion.div
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            className="md:hidden bg-surface border-b border-border"
          >
            <div className="px-4 py-4 space-y-3">
              {navLinks.map(link => (
                <a
                  key={link.href}
                  href={link.href}
                  onClick={() => setMenuOpen(false)}
                  className="block py-2 text-sm font-medium text-muted hover:text-base"
                >
                  {link.label}
                </a>
              ))}
              <div className="pt-3 border-t border-border flex gap-3">
                <Link
                  to="/login"
                  onClick={() => setMenuOpen(false)}
                  className="flex-1 text-center py-2 text-sm font-medium text-muted border border-border rounded-md hover:text-base"
                >
                  Войти
                </Link>
                <Link
                  to="/register"
                  onClick={() => setMenuOpen(false)}
                  className="flex-1 text-center py-2 text-sm font-semibold text-white bg-primary rounded-md"
                >
                  Регистрация
                </Link>
              </div>
            </div>
          </motion.div>
        )}
      </header>

      <section className="relative min-h-[90vh] flex items-center overflow-hidden">
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,rgba(91,107,255,0.08),transparent_50%)]" />
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_bottom_left,rgba(61,215,229,0.04),transparent_50%)]" />
        <div className="absolute bottom-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-border to-transparent" />
        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-20 md:py-32">
          <motion.div
            initial={{ opacity: 0, y: 30 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
            className="max-w-3xl"
          >
            <div className="eyebrow mb-6">
              Шиномонтаж у Бориса в Санкт-Петербурге
            </div>
            <h1 className="t-display text-5xl sm:text-6xl md:text-7xl leading-[1.05] mb-6">
              Профессиональный
              <br />
              <span>
                шиномонтаж
              </span>
            </h1>
            <p className="text-lg md:text-xl text-muted mb-10 max-w-xl leading-relaxed">
              Быстрая и качественная замена резины, балансировка, правка дисков
              и сезонное хранение. Работаем круглосуточно.
            </p>
            <div className="flex flex-col sm:flex-row gap-4">
              <Link
                to="/register"
                className="btn btn-primary btn-lg gap-2"
              >
                Записаться
                <ArrowRight className="w-4 h-4" />
              </Link>
              <a
                href="#calculator"
                className="btn btn-secondary btn-lg gap-2"
              >
                Калькулятор
                <ChevronRight className="w-4 h-4" />
              </a>
            </div>

            <div className="flex flex-wrap gap-6 mt-12 pt-8 border-t border-border">
              <div>
                <span className="t-metric text-2xl">
                  {stats ? stats.carsServed.toLocaleString('ru-RU') : '—'}
                </span>
                <p className="t-label-sm mt-1">Обслужено авто</p>
              </div>
              <div>
                <span className="t-metric text-2xl">
                  {stats?.satisfactionPercent != null ? `${stats.satisfactionPercent}%` : '—'}
                </span>
                <p className="t-label-sm mt-1">Довольных клиентов</p>
              </div>
              <div>
                <span className="t-metric text-2xl">
                  {stats ? stats.branchesCount : '—'}
                </span>
                <p className="t-label-sm mt-1">Филиала</p>
              </div>
              <div>
                <span className="t-metric text-2xl">24/7</span>
                <p className="t-label-sm mt-1">Режим работы</p>
              </div>
            </div>
          </motion.div>
        </div>
      </section>

      <section className="py-20 md:py-28">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.5 }}
            className="text-center mb-16"
          >
            <div className="eyebrow justify-center mb-3">Преимущества</div>
            <h2 className="t-display text-4xl md:text-5xl mb-4">
              Почему выбирают нас
            </h2>
            <p className="text-muted max-w-xl mx-auto text-sm">
              Мы делаем всё, чтобы вы остались довольны сервисом
            </p>
          </motion.div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
            {benefits.map((item, i) => (
              <motion.div
                key={item.title}
                initial={{ opacity: 0, y: 30 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ duration: 0.5, delay: i * 0.1 }}
                className="stat-tile"
              >
                <div className="stat-head">
                  <span className="stat-eyebrow">{item.title}</span>
                  <item.icon className="w-5 h-5 text-muted" />
                </div>
                <p className="t-body-sm mt-1">{item.desc}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      <section id="services" className="py-20 md:py-28 border-t border-border">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.5 }}
            className="text-center mb-12"
          >
            <div className="eyebrow justify-center mb-3">Прайс-лист</div>
            <h2 className="t-display text-4xl md:text-5xl mb-4">
              Услуги и цены
            </h2>
            <p className="text-muted max-w-xl mx-auto text-sm">
              Прозрачные цены без скрытых платежей
            </p>
          </motion.div>

          <div className="flex justify-center mb-10">
            <div className="tabs">
              {sizeLabels.map((label, i) => (
                <button
                  key={sizeKeys[i]}
                  onClick={() => setActiveRadius(i)}
                  className={`tab ${activeRadius === i ? 'is-active' : ''}`}
                >
                  {label}
                </button>
              ))}
            </div>
          </div>

          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.5 }}
            className="card overflow-hidden p-0"
          >
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border">
                  <th className="text-left px-6 py-4 font-semibold text-base">Услуга</th>
                  <th className="text-right px-6 py-4 font-semibold text-base">Цена</th>
                </tr>
              </thead>
              <tbody>
                {servicesToShow.map((service, i) => (
                  <tr key={service.id} className={i < servicesToShow.length - 1 ? 'border-b border-border' : ''}>
                    <td className="px-6 py-4">
                      <div className="font-medium text-base">{service.name}</div>
                      <div className="t-mono-sm mt-0.5">{service.description}</div>
                    </td>
                    <td className="px-6 py-4 text-right">
                      <span className="t-metric text-lg">
                        {getPrice(service, sizeKeys[activeRadius]).toLocaleString('ru-RU')} ₽
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </motion.div>
          <p className="text-center text-faint text-xs mt-4">
            * Цены указаны для комплекта из 4 колёс. Для других типов кузова возможна корректировка.
          </p>
        </div>
      </section>

      <section id="calculator" className="py-20 md:py-28">
        <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.5 }}
            className="text-center mb-12"
          >
            <div className="eyebrow justify-center mb-3">Калькулятор</div>
            <h2 className="t-display text-4xl md:text-5xl mb-4">
              Калькулятор стоимости
            </h2>
            <p className="text-muted text-sm">
              Рассчитайте предварительную стоимость сезонной замены
            </p>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, y: 30 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.5, delay: 0.1 }}
            className="card p-6 md:p-8"
          >
            <div className="space-y-6">
              <div>
                <label className="field-label mb-3 block">Диаметр колёс</label>
                <div className="tabs">
                  {sizeLabels.map((label, i) => (
                    <button
                      key={sizeKeys[i]}
                      onClick={() => setCalcRadius(i)}
                      className={`tab ${calcRadius === i ? 'is-active' : ''}`}
                    >
                      {label}
                    </button>
                  ))}
                </div>
              </div>

              <div>
                <label className="field-label mb-3 block">
                  Количество колёс: <span className="text-base font-semibold">{calcWheels}</span>
                </label>
                <div className="tabs">
                  {[1, 2, 3, 4].map(n => (
                    <button
                      key={n}
                      onClick={() => setCalcWheels(n)}
                      className={`tab ${calcWheels === n ? 'is-active' : ''}`}
                    >
                      {n} шт.
                    </button>
                  ))}
                </div>
              </div>

              <div>
                <label className="field-label mb-3 block">Услуги</label>
                <div className="space-y-2">
                  {INITIAL_SERVICES.map(service => {
                    const price = getPrice(service, sizeKeys[calcRadius]);
                    const perWheel = service.category === 'tire-fitting' || service.category === 'balancing';
                    return (
                      <label key={service.id} className="check p-3 rounded-lg border border-border hover:border-border-strong transition-colors w-full">
                        <input
                          type="checkbox"
                          checked={calcServices.includes(service.id)}
                          onChange={() => setCalcServices(prev =>
                            prev.includes(service.id)
                              ? prev.filter(id => id !== service.id)
                              : [...prev, service.id]
                          )}
                        />
                        <div className="flex-1 flex items-center justify-between">
                          <span className="text-sm">{service.name.replace(/\([^)]*\)/, '').trim()}</span>
                          <span className="text-sm text-muted">
                            {(perWheel ? price * calcWheels / 4 : price).toLocaleString('ru-RU')} ₽
                          </span>
                        </div>
                      </label>
                    );
                  })}
                </div>
              </div>

              <div className="pt-4 border-t border-border flex items-center justify-between">
                <span className="t-title-md">Итого</span>
                <span className="t-metric text-3xl text-base">
                  {calcTotal().toLocaleString('ru-RU')} ₽
                </span>
              </div>

              <button
                onClick={() => {
                  const calcState = {
                    fromCalculator: true,
                    wheels: calcWheels,
                    radiusIndex: calcRadius,
                    radiusLabel: sizeLabels[calcRadius],
                    serviceIds: calcServices,
                  };
                  sessionStorage.setItem('calcState', JSON.stringify(calcState));
                  if (isAuthenticated) {
                    navigate('/orders/new', { replace: true });
                  } else {
                    navigate('/register', { replace: true });
                  }
                }}
                className="btn btn-primary w-full justify-center btn-lg"
              >
                Записаться по расчёту
              </button>
              <p className="text-xs text-faint text-center">
                Предварительный расчёт. Точная стоимость уточняется на месте.
              </p>
            </div>
          </motion.div>
        </div>
      </section>

      <section id="reviews" className="py-20 md:py-28 border-t border-border">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.5 }}
            className="text-center mb-16"
          >
            <div className="eyebrow justify-center mb-3">Отзывы</div>
            <h2 className="t-display text-4xl md:text-5xl mb-4">
              Отзывы клиентов
            </h2>
            <p className="text-muted text-sm">
              Что говорят о нас автомобилисты
            </p>
          </motion.div>

          {reviews.length === 0 ? (
            <p className="text-center text-muted text-sm mb-8">Пока нет отзывов. Будьте первым!</p>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-3 gap-5 mb-10">
              {reviews.map((review, i) => (
                <motion.div
                  key={review.reviewId}
                  initial={{ opacity: 0, y: 30 }}
                  whileInView={{ opacity: 1, y: 0 }}
                  viewport={{ once: true }}
                  transition={{ duration: 0.5, delay: i * 0.1 }}
                  className="card"
                >
                  <div className="flex items-center gap-1 mb-3">
                    {Array.from({ length: 5 }).map((_, j) => (
                      <Star
                        key={j}
                        className={`w-4 h-4 ${
                          j < review.rating ? 'text-muted fill-muted' : 'text-faint/30'
                        }`}
                      />
                    ))}
                  </div>
                  <p className="t-body-md mb-4 flex-1">
                    «{review.text}»
                  </p>
                  <div className="flex items-center justify-between pt-3 border-t border-border">
                    <span className="text-sm font-semibold text-base">{review.author}</span>
                    {review.carModel && <span className="t-mono-sm">{review.carModel}</span>}
                  </div>
                </motion.div>
              ))}
            </div>
          )}


        </div>
      </section>

      <section id="map" className="py-20 md:py-28">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.5 }}
            className="text-center mb-12"
          >
            <div className="eyebrow justify-center mb-3">Расположение</div>
            <h2 className="t-display text-4xl md:text-5xl mb-4">
              Как нас найти
            </h2>
            <p className="text-muted text-sm">
              Мы работаем в удобном районе Санкт-Петербурга
            </p>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, y: 30 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.5 }}
            className="card overflow-hidden p-0 relative"
          >
            <div className="h-[420px] w-full rounded-xl overflow-hidden">
              <div ref={mapRef} className="w-full h-full"></div>
            </div>
            <a
              href={YANDEX_MAPS_URL}
              target="_blank"
              rel="noopener noreferrer"
              className="ya-btn absolute bottom-12 left-1/2 -translate-x-1/2 z-10"
            >
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round">
                <path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z"/>
                <circle cx="12" cy="10" r="3"/>
              </svg>
              <span>Открыть в Яндекс Картах</span>
            </a>
          </motion.div>
        </div>
      </section>

      <section id="contacts" className="py-20 md:py-28 border-t border-border">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.5 }}
            className="text-center mb-16"
          >
            <div className="eyebrow justify-center mb-3">Связь</div>
            <h2 className="t-display text-4xl md:text-5xl mb-4">
              Контакты
            </h2>
            <p className="text-muted text-sm">
              Свяжитесь с нами удобным способом
            </p>
          </motion.div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-5 max-w-4xl mx-auto">
            {[
              { icon: Phone, label: 'Телефон', value: '+7 (812) 333-21-21', href: 'tel:+78123332121' },
              { icon: MapPin, label: 'Адрес', value: ADDRESS, href: YANDEX_MAPS_URL },
              { icon: Mail, label: 'Email', value: 'info@premium-tyre.ru', href: 'mailto:info@premium-tyre.ru' },
              { icon: Clock, label: 'Режим работы', value: 'Ежедневно, круглосуточно' },
            ].map((item, i) => (
              <motion.div
                key={item.label}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ duration: 0.4, delay: i * 0.1 }}
                className="stat-tile items-center text-center pt-7"
              >
                <item.icon className="w-6 h-6 text-muted mb-3" />
                <p className="stat-eyebrow mb-1">{item.label}</p>
                {item.href ? (
                  <a
                    href={item.href}
                    className="text-sm font-medium text-base hover:text-white transition-colors"
                    {...(item.href.startsWith('http') ? { target: '_blank', rel: 'noopener noreferrer' } : {})}
                  >
                    {item.value}
                  </a>
                ) : (
                  <p className="text-sm text-base">{item.value}</p>
                )}
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      <footer className="border-t border-border py-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex flex-col md:flex-row items-center justify-between gap-4">
            <div className="flex items-center gap-3">
              <img src={SERVER_BASE + '/images/logo.png'} alt="Шиномонтаж у Бориса" className="w-6 h-6 object-contain" />
              <span className="text-base font-semibold">Шиномонтаж у Бориса</span>
            </div>
            <p className="text-sm text-faint">
              &copy; {new Date().getFullYear()} Шиномонтаж у Бориса. Все права защищены.
            </p>
          </div>
        </div>
      </footer>
    </div>
  );
}
