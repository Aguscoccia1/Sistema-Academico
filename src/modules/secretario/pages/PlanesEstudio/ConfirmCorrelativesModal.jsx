import { Check, FileText, X } from 'lucide-react';
import '../../../../styles/modal.css';

const formatOrder = (order) => String(order).padStart(2, '0');

function ConfirmCorrelativesModal({ subject, correlatives, onClose, onConfirm }) {
  return (
    <div className="modal-overlay" role="dialog" aria-modal="true" aria-labelledby="confirm-correlatives-title">
      <div className="modal-content confirm-correlatives-modal">
        <div className="modal-header">
          <div className="modal-header-title">
            <FileText size={20} />
            <div>
              <h2 id="confirm-correlatives-title">Confirmar Correlatividades</h2>
              <p>Revisá las correlatividades seleccionadas antes de guardar los cambios</p>
            </div>
          </div>
          <button type="button" aria-label="Cerrar" onClick={onClose}><X size={20} /></button>
        </div>

        <div className="confirm-correlatives-body">
          <h3>Materia</h3>
          <p className="confirm-correlatives-subject">{subject.orden} - {subject.materia}</p>

          <h3>Correlativas seleccionadas ({correlatives.length})</h3>
          {correlatives.length === 0 ? (
            <p className="confirm-correlatives-empty">Esta materia no tiene correlativas.</p>
          ) : (
            <div className="confirm-correlatives-chips">
              {correlatives.map((correlative) => (
                <span className="confirm-correlatives-chip" key={correlative.id}>
                  <Check size={14} />
                  {formatOrder(correlative.orden)} - {correlative.materia}
                </span>
              ))}
            </div>
          )}

          <div className="modal-footer">
            <button type="button" className="modal-cancel-button" onClick={onClose}>Cancelar</button>
            <button type="button" className="modal-submit-button" onClick={onConfirm}>Confirmar</button>
          </div>
        </div>
      </div>
    </div>
  );
}

export default ConfirmCorrelativesModal;