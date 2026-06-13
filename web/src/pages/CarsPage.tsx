import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { apiGet, apiDelete, SERVER_BASE } from '../api/client';
import type { CarDto } from '../types';
import { Car, Plus, Trash2, Edit } from 'lucide-react';
import { getCarIconUrl, getAllPhotos } from '../utils/carData';
import PhotoLightbox from '../components/PhotoLightbox';
import ConfirmModal from '../components/ConfirmModal';
import styles from './CarsPage.module.css';

export default function CarsPage() {
  const [cars, setCars] = useState<CarDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [lightboxPhotos, setLightboxPhotos] = useState<string[]>([]);
  const [lightboxIndex, setLightboxIndex] = useState(0);
  const [deleteTarget, setDeleteTarget] = useState<number | null>(null);
  const [hoveredIdx, setHoveredIdx] = useState<Record<number, number>>({});
  const navigate = useNavigate();

  useEffect(() => {
    apiGet<CarDto[]>('/cars')
      .then(setCars)
      .finally(() => setLoading(false));
  }, []);

  const handleDelete = async (id: number) => {
    setDeleteTarget(id);
  };

  const confirmDelete = async () => {
    if (deleteTarget === null) return;
    setDeleteTarget(null);
    try {
      await apiDelete(`/cars/${deleteTarget}`);
      setCars((prev) => prev.filter((c) => c.carId !== deleteTarget));
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Ошибка удаления');
    }
  };

  if (loading) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-8">
        <div className="h-8 bg-elevated rounded w-48 mb-6 animate-pulse" />
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-4">
          {[1, 2, 3].map((i) => (
            <div key={i} className="card p-6 animate-pulse">
              <div className="h-4 bg-elevated rounded w-3/4 mb-3" />
              <div className="h-3 bg-elevated rounded w-1/2" />
            </div>
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="t-headline-lg text-2xl">Мои автомобили</h1>
        <Link
          to="/cars/new"
          className="btn btn-primary btn-sm"
        >
          <Plus size={18} /> Добавить
        </Link>
      </div>

      {cars.length === 0 ? (
        <div className="text-center py-16 text-faint">
          <Car size={64} className="mx-auto mb-4 opacity-30" />
          <p className="text-lg mb-2 text-muted">Нет автомобилей</p>
          <p className="text-sm mb-4 text-faint">Добавьте автомобиль для записи на шиномонтаж</p>
          <Link to="/cars/new" className="btn btn-primary">
            <Plus size={18} /> Добавить автомобиль
          </Link>
        </div>
      ) : (
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-4">
          {cars.map((car) => (
            <div key={car.carId} className="card p-0 overflow-hidden">
              {(() => {
                const all = getAllPhotos(car.photoUrl, car.additionalPhotosJson);
                const iconUrl = !car.photoUrl ? getCarIconUrl(car.brand, car.model) : null;
                const hasPhoto = all.length > 0;
                const activeIdx = hoveredIdx[car.carId] ?? 0;
                return hasPhoto ? (
                  <>
                    <div
                      className="w-full h-40 overflow-hidden cursor-pointer"
                      onClick={() => { setLightboxPhotos(all); setLightboxIndex(activeIdx); }}
                    >
                      <img src={all[activeIdx]} alt="" className="w-full h-full object-contain" />
                    </div>
                    {all.length > 1 && (
                      <div
                        className={styles.thumbStrip}
                        onMouseLeave={() => setHoveredIdx((prev) => { const n = { ...prev }; delete n[car.carId]; return n; })}
                      >
                        {all.slice(0, 4).map((url, i) => (
                          <img
                            key={i}
                            src={url}
                            alt=""
                            onClick={() => { setLightboxPhotos(all); setLightboxIndex(i); }}
                            onMouseEnter={() => setHoveredIdx((prev) => ({ ...prev, [car.carId]: i }))}
                            className={`${styles.thumb} ${i === activeIdx ? styles.thumbActive : ''}`}
                          />
                        ))}
                        {all.length > 4 && (
                          <span
                            onClick={() => { setLightboxPhotos(all); setLightboxIndex(4); }}
                            className={styles.moreBadge}
                          >+{all.length - 4}</span>
                        )}
                      </div>
                    )}
                  </>
                ) : iconUrl ? (
                  <div className="w-full h-40 flex items-center justify-center p-4">
                    <img src={SERVER_BASE + iconUrl} alt="" className="w-full h-full object-contain" onError={(e) => { (e.target as HTMLImageElement).style.display = 'none'; }} />
                  </div>
                ) : null;
              })()}
              <div className="p-4">
                <h3 className="font-semibold text-base">{car.brand} {car.model}</h3>
                <p className="text-sm text-faint mt-1">{car.licensePlate} &middot; {car.manufactureYear} г.</p>
                <p className="t-mono-sm mt-1">VIN: {car.vin}</p>
                <div className="flex gap-3 mt-3 pt-3 border-t border-border">
                  <button
                    onClick={() => navigate(`/cars/${car.carId}/edit`)}
                    className="flex items-center gap-1 text-sm text-muted hover:text-base font-medium"
                  >
                    <Edit size={14} /> Редактировать
                  </button>
                  <button
                    onClick={() => handleDelete(car.carId)}
                    className="flex items-center gap-1 text-sm text-faint hover:text-base font-medium ml-auto"
                  >
                    <Trash2 size={14} /> Удалить
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
      {deleteTarget !== null && (
        <ConfirmModal
          message="Удалить автомобиль?"
          onConfirm={confirmDelete}
          onCancel={() => setDeleteTarget(null)}
        />
      )}
      {lightboxPhotos.length > 0 && (
        <PhotoLightbox
          photos={lightboxPhotos}
          initialIndex={lightboxIndex}
          onClose={() => setLightboxPhotos([])}
        />
      )}
    </div>
  );
}
