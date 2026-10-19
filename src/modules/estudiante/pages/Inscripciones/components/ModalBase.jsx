import { useEffect } from 'react';
import { X } from 'lucide-react';

function ModalBase({ icono: Icono, variante = 'ok', titulo, subtitulo, onClose, children }) {
  useEffect(() => {
    const handleKey = (e) => {
      if (e.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', handleKey);
    return () => document.removeEventListener('keydown', handleKey);
  }, [onClose]);

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div
        className="modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="modal-titulo"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="modal__header">
          <div className={`modal__icono modal__icono--${variante}`}>
            <Icono size={22} />
          </div>
          <div className="modal__titulos">
            <h2 id="modal-titulo">{titulo}</h2>
            <p>{subtitulo}</p>
          </div>
          <button type="button" className="modal__cerrar" onClick={onClose} aria-label="Cerrar">
            <X size={20} />
          </button>
        </div>
        {children}
      </div>
    </div>
  );
}

export default ModalBase;