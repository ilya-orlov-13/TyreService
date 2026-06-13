import { useEffect, useState, type ChangeEvent, type FormEvent } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { api, apiGet, SERVER_BASE } from '../api/client';
import type { CarDto } from '../types';
import { formatLicensePlate } from '../utils/phoneMask';
import { BRANDS, getModelSuggestions, getCarIconUrl, getAllPhotos } from '../utils/carData';
import PhotoLightbox from '../components/PhotoLightbox';
import { ArrowLeft } from 'lucide-react';
import styles from './CarEditPage.module.css';

export default function CarEditPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [car, setCar] = useState<CarDto | null>(null);
  const [brand, setBrand] = useState('');
  const [model, setModel] = useState('');
  const [manufactureYear, setManufactureYear] = useState(new Date().getFullYear());
  const [licensePlate, setLicensePlate] = useState('');
  const [vin, setVin] = useState('');
  const [photos, setPhotos] = useState<File[]>([]);
  const [previews, setPreviews] = useState<string[]>([]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(true);
  const [lightboxOpen, setLightboxOpen] = useState(false);
  const [lightboxIndex, setLightboxIndex] = useState(0);
  const [deletedExistingIndices, setDeletedExistingIndices] = useState<number[]>([]);

  useEffect(() => {
    if (!id) return;
    apiGet<CarDto>(`/cars/${id}`)
      .then((c) => {
        setCar(c);
        setBrand(c.brand);
        setModel(c.model);
        setManufactureYear(c.manufactureYear);
        setLicensePlate(formatLicensePlate(c.licensePlate));
        setVin(c.vin);
      })
      .finally(() => setFetching(false));
  }, [id]);

  const handlePhotoAdd = (e: ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files ?? []) as File[];
    e.target.value = '';
    setPhotos((prev) => [...prev, ...files]);
    setPreviews((prev) => [...prev, ...files.map((f) => URL.createObjectURL(f))]);
  };

  const removeNewPhoto = (index: number) => {
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
      if (deletedExistingIndices.length > 0)
        formData.append('deletePhotoIndices', JSON.stringify(deletedExistingIndices));
      photos.forEach((p) => formData.append('photos', p));

      await api.put(`/cars/${id}`, formData);
      navigate('/cars');
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Ошибка сохранения';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  if (fetching) {
    return (
      <div className="max-w-2xl mx-auto px-4 py-8">
        <div className="card p-6 animate-pulse space-y-4">
          <div className="h-6 bg-elevated rounded w-48" />
          {[1, 2, 3].map((i) => <div key={i} className="h-10 bg-elevated rounded" />)}
        </div>
      </div>
    );
  }

  if (!car) {
    return (
      <div className="max-w-2xl mx-auto px-4 py-8 text-center text-faint">
        <p>Автомобиль не найден</p>
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      <div className="flex items-center gap-3 mb-6">
        <button type="button" onClick={() => navigate('/cars')} className={`btn btn-tertiary btn-sm ${styles.backBtn}`}>
          <ArrowLeft size={18} />
        </button>
        <h1 className="t-headline-lg text-2xl">Редактировать автомобиль</h1>
      </div>

      <form onSubmit={handleSubmit} className="card p-6 space-y-4">
        {error && <div className="bg-elevated text-base text-sm rounded-md p-3">{error}</div>}

        <div>
          <label className="field-label mb-2 block">
            Фотографии ({getAllPhotos(car.photoUrl, car.additionalPhotosJson).filter((_, i) => !deletedExistingIndices.includes(i)).length + previews.length})
          </label>
          {(() => {
            const existing = getAllPhotos(car.photoUrl, car.additionalPhotosJson);
            const visibleExisting = existing.filter((_, i) => !deletedExistingIndices.includes(i));
            const allPhotos = [...visibleExisting, ...previews];
            const placeholder = !car.photoUrl ? getCarIconUrl(car.brand, car.model) : null;
            const firstSrc = visibleExisting[0] || (previews.length > 0 ? previews[0] : (placeholder ? SERVER_BASE + placeholder : null));
            const mapExistingIdx = (visIdx: number) => {
              let cnt = -1;
              for (let i = 0; i < existing.length; i++) {
                if (!deletedExistingIndices.includes(i)) cnt++;
                if (cnt === visIdx) return i;
              }
              return -1;
            };
            return (
              <div className={styles.gallery}>
                {firstSrc && (() => {
                  const isExistingPhoto = visibleExisting.length > 0;
                  const isPreviewPhoto = previews.length > 0 && !isExistingPhoto;
                  const ei = isExistingPhoto ? mapExistingIdx(0) : -1;
                  return (
                    <div
                      onClick={() => { if (isExistingPhoto || isPreviewPhoto) { setLightboxIndex(0); setLightboxOpen(true); } }}
                      className={`${styles.mainPhoto} ${isExistingPhoto || isPreviewPhoto ? 'cursor-pointer' : ''}`}
                    >
                      <img
                        src={firstSrc}
                        alt=""
                        className={styles.mainPhotoImg}
                        onError={(e) => { (e.target as HTMLImageElement).style.display = 'none'; }}
                      />
                      {ei >= 0 && (
                        <button
                          type="button"
                          onClick={(e) => { e.stopPropagation(); setDeletedExistingIndices((prev) => [...prev, ei]); }}
                          className={styles.mainPhotoDeleteBtn}
                        >&#10005;</button>
                      )}
                    </div>
                  );
                })()}
                {(allPhotos.length > 1 || previews.length < 5) && (
                  <div className={styles.thumbRow}>
                    {visibleExisting.map((url, vi) => {
                      const ei = mapExistingIdx(vi);
                      return (
                        <div key={`e-${ei}`} className={styles.thumbWrapper}>
                          <img
                            src={url}
                            alt=""
                            onClick={() => { setLightboxIndex(vi); setLightboxOpen(true); }}
                            className={`${styles.thumbImg} ${vi === 0 ? styles.thumbImgFirst : ''}`}
                          />
                          <button
                            type="button"
                            onClick={() => setDeletedExistingIndices((prev) => [...prev, ei])}
                            className={styles.thumbDeleteBtn}
                          >&#10005;</button>
                        </div>
                      );
                    })}
                    {previews.map((url, i) => (
                      <div key={`n-${i}`} className={styles.thumbWrapper}>
                        <img
                          src={url}
                          alt=""
                          onClick={() => { setLightboxIndex(visibleExisting.length + i); setLightboxOpen(true); }}
                          className={styles.thumbImg}
                        />
                        <button
                          type="button"
                          onClick={() => removeNewPhoto(i)}
                          className={styles.thumbDeleteBtn}
                        >&#10005;</button>
                      </div>
                    ))}
                    {previews.length < 5 && (
                      <label className={styles.addBtn}>
                        +
                        <input type="file" accept="image/*" multiple onChange={handlePhotoAdd} className="hidden" />
                      </label>
                    )}
                  </div>
                )}

              </div>
            );
          })()}
        </div>

        {lightboxOpen && (() => {
          const existing = getAllPhotos(car.photoUrl, car.additionalPhotosJson);
          const visibleExisting = existing.filter((_, i) => !deletedExistingIndices.includes(i));
          return (
            <PhotoLightbox
              photos={[...visibleExisting, ...previews]}
              initialIndex={lightboxIndex}
              onClose={() => setLightboxOpen(false)}
            />
          );
        })()}

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
            <input type="number" value={manufactureYear} onChange={(e) => setManufactureYear(parseInt(e.target.value))} min={1990} max={2030} className="input" required />
          </div>
          <div className="field">
            <label className="field-label">Госномер</label>
            <input type="text" value={licensePlate} onChange={(e) => setLicensePlate(formatLicensePlate(e.target.value))} className="input" required />
          </div>
        </div>

        <div className="field">
          <label className="field-label">VIN</label>
          <input type="text" value={vin} onChange={(e) => setVin(e.target.value.toUpperCase())} maxLength={17} className="input font-mono" required />
        </div>

        <button type="submit" disabled={loading} className="btn btn-primary w-full justify-center">
          {loading ? 'Сохранение...' : 'Сохранить'}
        </button>
      </form>
    </div>
  );
}
