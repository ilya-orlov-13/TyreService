import { useState, type ChangeEvent, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { api, apiPost } from '../api/client';
import { Upload, ArrowLeft } from 'lucide-react';
import type { OcrResult } from '../types';
import { formatLicensePlate } from '../utils/phoneMask';
import { BRANDS, getModelSuggestions } from '../utils/carData';
import '@lottiefiles/dotlottie-wc';

export default function CarNewPage() {
  const navigate = useNavigate();
  const [brand, setBrand] = useState('');
  const [model, setModel] = useState('');
  const [manufactureYear, setManufactureYear] = useState(new Date().getFullYear());
  const [licensePlate, setLicensePlate] = useState('');
  const [vin, setVin] = useState('');
  const [photos, setPhotos] = useState<File[]>([]);
  const [previews, setPreviews] = useState<string[]>([]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [ocrLoading, setOcrLoading] = useState(false);
  const [ocrDocPreview, setOcrDocPreview] = useState<string | null>(null);
  const [ocrDone, setOcrDone] = useState(false);

  const handleOcr = async (file: File) => {
    setOcrLoading(true);
    setOcrDone(false);
    try {
      const formData = new FormData();
      formData.append('photo', file);
      const res = await api.post('/ocr/scan', formData);
      const data: OcrResult = res.data.data;
      if (data.brand) setBrand(data.brand);
      if (data.model) setModel(data.model);
      if (data.year) setManufactureYear(parseInt(data.year));
      if (data.licensePlate) setLicensePlate(formatLicensePlate(data.licensePlate));
      if (data.vin) setVin(data.vin);
      setOcrDone(true);
    } catch {
      // OCR may fail silently
    } finally {
      setOcrLoading(false);
    }
  };

  const handleOcrDocUpload = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    e.target.value = '';
    if (ocrDocPreview) URL.revokeObjectURL(ocrDocPreview);
    setOcrDocPreview(URL.createObjectURL(file));
    handleOcr(file);
  };

  const handlePhotoAdd = (e: ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files ?? []) as File[];
    e.target.value = '';
    const newPhotos = [...photos, ...files];
    setPhotos(newPhotos);
    setPreviews(newPhotos.map((f) => URL.createObjectURL(f)));
  };

  const removePhoto = (index: number) => {
    URL.revokeObjectURL(previews[index]);
    setPhotos((prev) => prev.filter((_, i) => i !== index));
    setPreviews((prev) => prev.filter((_, i) => i !== index));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const formData = new FormData();
      formData.append('brand', brand);
      formData.append('model', model);
      formData.append('manufactureYear', manufactureYear.toString());
      formData.append('licensePlate', licensePlate);
      formData.append('vin', vin);
      photos.forEach((p) => formData.append('photos', p));

      await api.post('/cars', formData);
      navigate('/cars');
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Ошибка сохранения';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      <div className="flex items-center gap-3 mb-6">
        <button type="button" onClick={() => navigate('/cars')} className="btn btn-tertiary btn-sm p-0 w-9">
          <ArrowLeft size={18} />
        </button>
        <h1 className="t-headline-lg text-2xl">Добавить автомобиль</h1>
      </div>

      <form onSubmit={handleSubmit} className="card p-6 space-y-4">
        {error && <div className="bg-elevated text-base text-sm rounded-md p-3">{error}</div>}

        {/* OCR Document Upload */}
        <div className="border-2 border-dashed border-border rounded-lg p-4">
          <label className="field-label mb-1">Распознавание СТС</label>
          <p className="text-sm text-faint mb-3">Загрузите фото свидетельства о регистрации ТС для автозаполнения полей</p>

          {ocrDocPreview ? (
            <div className="relative mb-2">
              <img src={ocrDocPreview} alt="" className="w-full h-32 object-contain rounded-lg" />
              {ocrLoading && (
                <div className="absolute inset-0 bg-black/55 flex flex-col items-center justify-center rounded-lg">
                  <dotlottie-wc src="/Document-OCR-Scan.lottie" autoplay loop style={{width:120,height:120}} />
                  <p className="text-white text-sm font-medium mt-2">Идёт распознавание...</p>
                  <p className="text-blue-200 text-xs mt-1">Обычно 3–4 минуты</p>
                </div>
              )}
              {ocrDone && !ocrLoading && (
                <div className="absolute top-1 right-1 bg-green-600 text-white text-xs px-2 py-0.5 rounded">Распознано</div>
              )}
            </div>
          ) : (
            <label className="flex items-center justify-center h-24 border-2 border-dashed border-border rounded-lg cursor-pointer hover:border-border-strong transition-colors">
              <div className="text-center">
                <Upload size={24} className="mx-auto mb-1 text-faint" />
                <span className="text-sm text-faint">Нажмите для загрузки</span>
              </div>
              <input type="file" accept="image/*" onChange={handleOcrDocUpload} className="hidden" />
            </label>
          )}
          {ocrDocPreview && (
            <button type="button" onClick={() => { setOcrDocPreview(null); setOcrDone(false); }} className="text-sm text-faint hover:text-base mt-1">
              Удалить
            </button>
          )}
        </div>

        <div className="grid md:grid-cols-2 gap-4">
          <div className="field">
            <label className="field-label">Марка</label>
            <input type="text" value={brand} onChange={(e) => setBrand(e.target.value)} list="brandList" className="input" required autoComplete="off" />
            <datalist id="brandList">
              {BRANDS.map(b => <option key={b} value={b} />)}
            </datalist>
          </div>
          <div className="field">
            <label className="field-label">Модель</label>
            <input type="text" value={model} onChange={(e) => setModel(e.target.value)} list="modelList" className="input" required autoComplete="off" />
            <datalist id="modelList">
              {getModelSuggestions(brand).map(m => <option key={m} value={m} />)}
            </datalist>
          </div>
        </div>

        <div className="grid md:grid-cols-2 gap-4">
          <div className="field">
            <label className="field-label">Год выпуска</label>
            <input type="number" value={manufactureYear} onChange={(e) => setManufactureYear(parseInt(e.target.value) || new Date().getFullYear())} min={1990} max={2030} className="input" />
          </div>
          <div className="field">
            <label className="field-label">Госномер</label>
            <input type="text" value={licensePlate} onChange={(e) => setLicensePlate(formatLicensePlate(e.target.value))} placeholder="А123ВВ777" className="input" required />
          </div>
        </div>

        <div className="field">
          <label className="field-label">VIN <span className="text-faint font-normal">(необязательно)</span></label>
          <input type="text" value={vin} onChange={(e) => setVin(e.target.value.toUpperCase())} placeholder="17 символов" maxLength={17} className="input font-mono" />
        </div>

        <div>
          <label className="field-label mb-2 block">Фотографии автомобиля</label>
          <div className="grid grid-cols-3 gap-2 mb-2">
            {previews.map((p, i) => (
              <div key={i} className="relative">
                <img src={p} alt="" className="w-full h-24 object-cover rounded-lg" />
                <button type="button" onClick={() => removePhoto(i)} className="absolute -top-1 -right-1 bg-base text-canvas w-5 h-5 rounded-full text-xs">&#10005;</button>
              </div>
            ))}
            {previews.length < 5 && (
              <label className="flex items-center justify-center h-24 border-2 border-dashed border-border rounded-lg cursor-pointer hover:border-border-strong transition-colors">
                <Upload size={20} className="text-faint" />
                <input type="file" accept="image/*" multiple onChange={handlePhotoAdd} className="hidden" />
              </label>
            )}
          </div>
        </div>

        <button type="submit" disabled={loading} className="btn btn-primary w-full justify-center">
          {loading ? 'Сохранение...' : 'Добавить автомобиль'}
        </button>
      </form>
    </div>
  );
}
