import React, { useState, useEffect } from 'react';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import styles from './CalendarWidget.module.css';

interface CalendarWidgetProps {
  selectedDate: string;
  onChange: (date: string) => void;
}

const MONTHS = [
  'Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь',
  'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь',
];

const DAYS = ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс'];

function toLocalDateStr(year: number, month: number, day: number): string {
  const m = String(month + 1).padStart(2, '0');
  const d = String(day).padStart(2, '0');
  return `${year}-${m}-${d}`;
}

export default function CalendarWidget({ selectedDate, onChange }: CalendarWidgetProps) {
  const today = new Date();
  const todayStr = toLocalDateStr(today.getFullYear(), today.getMonth(), today.getDate());

  const parsed = selectedDate ? new Date(selectedDate + 'T00:00:00') : today;
  const [viewYear, setViewYear] = useState(parsed.getFullYear());
  const [viewMonth, setViewMonth] = useState(parsed.getMonth());

  useEffect(() => {
    const d = selectedDate ? new Date(selectedDate + 'T00:00:00') : today;
    setViewYear(d.getFullYear());
    setViewMonth(d.getMonth());
  }, [selectedDate]);

  const firstDay = new Date(viewYear, viewMonth, 1);
  const startOffset = (firstDay.getDay() + 6) % 7;
  const daysInMonth = new Date(viewYear, viewMonth + 1, 0).getDate();

  const prevMonth = () => {
    if (viewMonth === 0) {
      setViewYear(viewYear - 1);
      setViewMonth(11);
    } else {
      setViewMonth(viewMonth - 1);
    }
  };

  const nextMonth = () => {
    if (viewMonth === 11) {
      setViewYear(viewYear + 1);
      setViewMonth(0);
    } else {
      setViewMonth(viewMonth + 1);
    }
  };

  const isPrevDisabled =
    viewYear < today.getFullYear() ||
    (viewYear === today.getFullYear() && viewMonth <= today.getMonth());

  const cells: React.ReactNode[] = [];
  for (let i = 0; i < startOffset; i++) {
    cells.push(<div key={`empty-${i}`} />);
  }
  for (let day = 1; day <= daysInMonth; day++) {
    const dateStr = toLocalDateStr(viewYear, viewMonth, day);
    const isPast = dateStr < todayStr;
    const isSelected = dateStr === selectedDate;
    const isToday = dateStr === todayStr;

    cells.push(
      <button
        key={day}
        type="button"
        disabled={isPast}
        onClick={() => onChange(dateStr)}
        className={[
          'flex items-center justify-center h-10 rounded-lg text-sm font-medium transition-all duration-100',
          isPast
            ? 'text-faint opacity-30 cursor-not-allowed'
            : isSelected
              ? 'bg-primary text-canvas font-semibold shadow-sm'
              : isToday
                ? 'bg-primary-soft text-base border border-border-strong'
                : 'text-muted hover:bg-elevated hover:text-base',
        ].join(' ')}
      >
        {day}
      </button>,
    );
  }

  return (
    <div className="card p-5 select-none">
      <div className="flex items-center justify-between mb-5">
        <button
          type="button"
          onClick={prevMonth}
          disabled={isPrevDisabled}
          className="btn btn-ghost btn-icon"
        >
          <ChevronLeft size={18} />
        </button>
        <span className="t-title-md text-base">
          {MONTHS[viewMonth]} {viewYear}
        </span>
        <button
          type="button"
          onClick={nextMonth}
          className="btn btn-ghost btn-icon"
        >
          <ChevronRight size={18} />
        </button>
      </div>

      <div className={`grid gap-1 ${styles.calendarGrid}`}>
        {DAYS.map((d) => (
          <div
            key={d}
            className="flex items-center justify-center h-8 text-xs font-medium text-faint tracking-wider uppercase"
          >
            {d}
          </div>
        ))}
        {cells}
      </div>
    </div>
  );
}
