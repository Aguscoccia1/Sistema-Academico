import { AlertCircle, FileText } from 'lucide-react';
import ModalBase from './ModalBase';

function ModalDetalleMateria({ materia, anioTexto, onCerrar }) {
  return (
    <ModalBase
      icono={FileText}
      variante="error"
      titulo="Detalle de materia"
      subtitulo="Revisá porque la materia no está habilitada"
      onClose={onCerrar}
    >
      <div className="modal__body">
        <span className="modal__label">Materia</span>
        <p className="modal__valor">
          {materia.nombre} - Año: {anioTexto}
        </p>

        <span className="modal__label modal__label--seccion">Correlatividades</span>
        <div className="modal__alerta" role="alert">
          <AlertCircle size={28} />
          <div>
            <strong>Falta aprobar correlativas</strong>
            <p>Para inscribirte necesitás aprobar:</p>
            <ul>
              {materia.correlativasPendientes.map((nombre) => (
                <li key={nombre}>{nombre}</li>
              ))}
            </ul>
          </div>
        </div>
      </div>
    </ModalBase>
  );
}

export default ModalDetalleMateria;