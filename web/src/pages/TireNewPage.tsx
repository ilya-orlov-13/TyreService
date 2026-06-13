import { useState, type ChangeEvent, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiPost } from '../api/client';
import { ArrowLeft } from 'lucide-react';
import TireForm, { type TireFormState } from '../components/TireForm';

const INITIAL_FORM: TireFormState = {
  manufacturer: '',
  tireModel: '',
  size: '',
  seasonality: '',
};

export default function TireNewPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState<TireFormState>(INITIAL_FORM);
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const handleChange = (e: ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value, ...(name === 'manufacturer' ? { tireModel: '' } : {}) }));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!form.manufacturer || !form.tireModel || !form.size || !form.seasonality) {
      setError('Заполните все поля');
      return;
    }
    setSubmitting(true);
    try {
      await apiPost('/tires', {
        tireType: 'Легковая',
        seasonality: form.seasonality,
        manufacturer: form.manufacturer,
        tireModel: form.tireModel,
        size: form.size,
        loadIndex: 0,
        wearPercentage: 0,
        pressure: 2.2,
      });
      navigate('/tires');
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Ошибка создания шины';
      setError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      <div className="flex items-center gap-3 mb-6">
        <button type="button" onClick={() => navigate('/tires')} className="btn btn-tertiary btn-sm p-0 w-9">
          <ArrowLeft size={18} />
        </button>
        <h1 className="t-headline-lg text-2xl">Добавить шину</h1>
      </div>

      <TireForm
        form={form}
        error={error}
        submitting={submitting}
        submitLabel="Добавить шину"
        submittingLabel="Сохранение..."
        onChange={handleChange}
        onSubmit={handleSubmit}
      />
    </div>
  );
}
