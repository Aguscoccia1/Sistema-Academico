import { CheckCircle2, FileText, Info } from 'lucide-react';
import ModalBase from './ModalBase';

function ModalConfirmarInscripcion({ materia, anioTexto, onConfirmar, onCancelar }) {
  return (
    <ModalBase
      icono={FileText}
      variante="ok"
      titulo="Confirmar Inscripciones"
      subtitulo="Revisá la inscripción seleccionada antes de guardar los cambios"
      onClose={onCancelar}
    >
      <div className="modal__body">
        <span className="modal__label">Materia</span>
        <p className="modal__valor">
          {materia.nombre} - Año: {anioTexto}
        </p>

        <div className="modal__correlativas-ok">
          <CheckCircle2 size={18} />
          <span>Cumple las correlatividades</span>
        </div>
      </div>

      <div className="modal__footer">
        <span className="modal__nota">
          <Info size={16} />
          Al confirmar, se registrará tu inscripción
        </span>
        <div className="modal__acciones">
          <button type="button" className="modal-boton modal-boton--secundario" onClick={onCancelar}>
            Cancelar
          </button>
          <button type="button" className="modal-boton modal-boton--primario" onClick={onConfirmar}>
            Confirmar
          </button>
        </div>
      </div>
    </ModalBase>
  );
}

export default ModalConfirmarInscripcion;