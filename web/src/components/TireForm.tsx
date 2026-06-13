import type { ChangeEvent, FormEvent } from 'react';
import SeasonalitySelect from './SeasonalitySelect';
import { getTireModelSuggestions, TIRE_BRANDS, TIRE_SIZES } from '../utils/tireData';

export interface TireFormState {
  manufacturer: string;
  tireModel: string;
  size: string;
  seasonality: string;
}

interface TireFormProps {
  form: TireFormState;
  error: string;
  submitting: boolean;
  submitLabel: string;
  submittingLabel: string;
  onChange: (event: ChangeEvent<HTMLInputElement | HTMLSelectElement>) => void;
  onSubmit: (event: FormEvent) => void;
}

export default function TireForm({
  form,
  error,
  submitting,
  submitLabel,
  submittingLabel,
  onChange,
  onSubmit,
}: TireFormProps) {
  const modelSuggestions = getTireModelSuggestions(form.manufacturer);

  return (
    <form onSubmit={onSubmit} className="card p-6 space-y-4">
      {error && <div className="bg-elevated text-base text-sm rounded-md p-3">{error}</div>}

      <div className="grid md:grid-cols-2 gap-4">
        <div className="field">
          <label className="field-label">Производитель</label>
          <input name="manufacturer" value={form.manufacturer} onChange={onChange} className="input" placeholder="Michelin" list="brandList" required autoComplete="off" />
          <datalist id="brandList">
            {TIRE_BRANDS.map((brand) => <option key={brand} value={brand} />)}
          </datalist>
        </div>
        <div className="field">
          <label className="field-label">Модель</label>
          <input name="tireModel" value={form.tireModel} onChange={onChange} className="input" placeholder="X-Ice North" list="modelList" required autoComplete="off" />
          <datalist id="modelList">
            {modelSuggestions.map((model) => <option key={model} value={model} />)}
          </datalist>
        </div>
      </div>

      <div className="grid md:grid-cols-2 gap-4">
        <div className="field">
          <label className="field-label">Размер</label>
          <input name="size" value={form.size} onChange={onChange} className="input" placeholder="205/55R16" list="sizeList" required autoComplete="off" />
          <datalist id="sizeList">
            {TIRE_SIZES.map((size) => <option key={size} value={size} />)}
          </datalist>
        </div>
        <div className="field">
          <label className="field-label">Сезонность</label>
          <SeasonalitySelect value={form.seasonality} onChange={onChange} required />
        </div>
      </div>

      <button type="submit" disabled={submitting} className="btn btn-primary w-full justify-center">
        {submitting ? submittingLabel : submitLabel}
      </button>
    </form>
  );
}
