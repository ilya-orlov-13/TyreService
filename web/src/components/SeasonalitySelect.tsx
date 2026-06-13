import type { ChangeEvent } from 'react';

interface SeasonalitySelectProps {
  name?: string;
  value: string;
  onChange: (e: ChangeEvent<HTMLSelectElement>) => void;
  required?: boolean;
}

const selectClassName =
  'h-10 w-full rounded-xl bg-[var(--color-surface)] px-[14px] text-sm text-[var(--color-base)] outline-none border border-[var(--color-border)] hover:border-[var(--color-border-strong)] focus:border-[var(--color-primary)] focus:shadow-[0_0_0_3px_rgba(91,107,255,0.35)] appearance-none transition-[border-color] duration-150 ease-out';

export default function SeasonalitySelect({
  name = 'seasonality',
  value,
  onChange,
  required,
}: SeasonalitySelectProps) {
  return (
    <div className="relative">
      <select
        name={name}
        value={value}
        onChange={onChange}
        className={selectClassName}
        required={required}
      >
        <option value="">--</option>
        <option value="Зимняя">Зимняя</option>
        <option value="Летняя">Летняя</option>
        <option value="Всесезонная">Всесезонная</option>
      </select>
      <svg
        className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 h-4 w-4 text-[var(--color-faint)]"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
        aria-hidden
      >
        <polyline points="6 9 12 15 18 9" />
      </svg>
    </div>
  );
}
