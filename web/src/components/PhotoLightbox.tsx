import { useEffect, useCallback, useState } from 'react';
import { X, ChevronLeft, ChevronRight } from 'lucide-react';
import styles from './PhotoLightbox.module.css';

interface Props {
  photos: string[];
  initialIndex?: number;
  onClose: () => void;
}

export default function PhotoLightbox({ photos, initialIndex = 0, onClose }: Props) {
  const [idx, setIdx] = useState(initialIndex);

  const prev = useCallback(() => setIdx((i) => (i > 0 ? i - 1 : photos.length - 1)), [photos.length]);
  const next = useCallback(() => setIdx((i) => (i < photos.length - 1 ? i + 1 : 0)), [photos.length]);

  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
      if (e.key === 'ArrowLeft') prev();
      if (e.key === 'ArrowRight') next();
    };
    document.addEventListener('keydown', handler);
    document.body.style.overflow = 'hidden';
    return () => {
      document.removeEventListener('keydown', handler);
      document.body.style.overflow = '';
    };
  }, [onClose, prev, next]);

  return (
    <div className={styles.overlay} onClick={onClose}>
      <button
        type="button"
        onClick={(e) => { e.stopPropagation(); onClose(); }}
        className={styles.closeBtn}
      >
        <X size={20} strokeWidth={1.5} />
      </button>

      {photos.length > 1 && (
        <button
          type="button"
          onClick={(e) => { e.stopPropagation(); prev(); }}
          className={`${styles.navBtn} ${styles.navBtnLeft}`}
        >
          <ChevronLeft size={22} strokeWidth={1.5} />
        </button>
      )}

      {photos.length > 1 && (
        <button
          type="button"
          onClick={(e) => { e.stopPropagation(); next(); }}
          className={`${styles.navBtn} ${styles.navBtnRight}`}
        >
          <ChevronRight size={22} strokeWidth={1.5} />
        </button>
      )}

      <img
        src={photos[idx]}
        alt=""
        className={styles.image}
        onClick={(e) => e.stopPropagation()}
      />

      {photos.length > 1 && (
        <span className={styles.counter}>
          {idx + 1} / {photos.length}
        </span>
      )}
    </div>
  );
}
