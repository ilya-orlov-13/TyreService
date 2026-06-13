import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiGet, apiDelete } from '../api/client';
import type { TireDto } from '../types';
import { Plus, Trash2, Edit } from 'lucide-react';
import ConfirmModal from '../components/ConfirmModal';

export default function TiresPage() {
  const [tires, setTires] = useState<TireDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [deleteTarget, setDeleteTarget] = useState<number | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    apiGet<TireDto[]>('/tires')
      .then(setTires)
      .finally(() => setLoading(false));
  }, []);

  const confirmDelete = async () => {
    if (deleteTarget === null) return;
    setDeleteTarget(null);
    try {
      await apiDelete(`/tires/${deleteTarget}`);
      setTires((prev) => prev.filter((t) => t.tireId !== deleteTarget));
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
        <h1 className="t-headline-lg text-2xl">Мои шины</h1>
        <button onClick={() => navigate('/tires/new')} className="btn btn-primary btn-sm">
          <Plus size={18} /> Добавить
        </button>
      </div>

      {tires.length === 0 ? (
        <div className="text-center py-16 text-faint">
          <p className="text-lg mb-2 text-muted">Нет шин</p>
          <p className="text-sm mb-4 text-faint">Добавьте шины для хранения</p>
          <button onClick={() => navigate('/tires/new')} className="btn btn-primary">
            <Plus size={18} /> Добавить шины
          </button>
        </div>
      ) : (
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-4">
          {tires.map((tire) => (
            <div key={tire.tireId} className="card p-5">
              <h3 className="font-semibold text-base">{tire.manufacturer} {tire.tireModel}</h3>
              <div className="text-sm text-muted mt-2 space-y-1">
                <p>Размер: {tire.size}</p>
                <p>Сезонность: {tire.seasonality}</p>
              </div>
              <div className="flex gap-3 mt-3 pt-3 border-t border-border">
                <button
                  onClick={() => navigate(`/tires/${tire.tireId}/edit`)}
                  className="flex items-center gap-1 text-sm text-muted hover:text-base font-medium"
                >
                  <Edit size={14} /> Редактировать
                </button>
                <button
                  onClick={() => setDeleteTarget(tire.tireId)}
                  className="flex items-center gap-1 text-sm text-faint hover:text-base font-medium ml-auto"
                >
                  <Trash2 size={14} /> Удалить
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
      {deleteTarget !== null && (
        <ConfirmModal
          message="Удалить шину?"
          onConfirm={confirmDelete}
          onCancel={() => setDeleteTarget(null)}
        />
      )}
    </div>
  );
}
