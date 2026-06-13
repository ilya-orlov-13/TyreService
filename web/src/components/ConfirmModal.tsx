import { useEffect } from 'react';
import styles from './ConfirmModal.module.css';

interface Props {
  message: string;
  onConfirm: () => void;
  onCancel: () => void;
  confirmLabel?: string;
  cancelLabel?: string;
}

export default function ConfirmModal({ message, onConfirm, onCancel, confirmLabel = 'Удалить', cancelLabel = 'Отмена' }: Props) {
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onCancel();
    };
    document.addEventListener('keydown', handler);
    return () => document.removeEventListener('keydown', handler);
  }, [onCancel]);

  return (
    <div className={styles.backdrop} onClick={onCancel}>
      <div className={styles.dialog} onClick={(e) => e.stopPropagation()}>
        <p className={styles.message}>{message}</p>
        <div className={styles.actions}>
          <button type="button" onClick={onCancel} className={styles.btnCancel}>
            {cancelLabel}
          </button>
          <button type="button" onClick={onConfirm} className={styles.btnConfirm}>
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}
