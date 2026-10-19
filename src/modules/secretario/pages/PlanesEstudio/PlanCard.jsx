import { CalendarDays, Clock, FileText, GraduationCap, CalendarRange } from 'lucide-react';

function PlanCard({ plan, onToggleStatus, onViewSubjects }) {
  const { id, nombre, codigo, añoInicio, añoFin, duracion, modalidad, cargaHoraria, regimen, activo } = plan;

  return (
    <article className="plan-card">
      <div className="plan-card-top">
        <div className="plan-card-icon"><FileText size={20} /></div>
        <div>
          <span className={`plan-badge ${activo ? 'plan-badge-active' : 'plan-badge-inactive'}`}>
            {activo ? 'Activo' : 'Archivado'}
          </span>
          <h3 className="plan-card-title">{codigo} – {nombre}</h3>
          <p className="plan-card-years">{añoFin ? `${añoInicio} - ${añoFin}` : `Desde ${añoInicio}`}</p>
        </div>
      </div>

      {activo && (
        <div className="plan-card-details">
          <div className="plan-card-detail">
            <CalendarDays size={18} />
            <div><span className="detail-label">Duración</span><strong className="detail-value">{duracion ?? '—'}</strong></div>
          </div>
          <div className="plan-card-detail">
            <GraduationCap size={18} />
            <div><span className="detail-label">Modalidad</span><strong className="detail-value">{modalidad ?? '—'}</strong></div>
          </div>
          <div className="plan-card-detail">
            <Clock size={18} />
            <div><span className="detail-label">Carga Horaria</span><strong className="detail-value">{cargaHoraria ?? '—'}</strong></div>
          </div>
          <div className="plan-card-detail">
            <CalendarRange size={18} />
            <div><span className="detail-label">Régimen de cursado</span><strong className="detail-value">{regimen ?? '—'}</strong></div>
          </div>
        </div>
      )}

      <div className={`plan-card-footer ${activo ? '' : 'is-archived'}`}>
        <button type="button" className="plan-outline-button" onClick={() => onViewSubjects(id)}>Ver plan</button>
        <button type="button" className="plan-outline-button" onClick={() => onToggleStatus(id)}>
          {activo ? 'Archivar' : 'Restaurar'}
        </button>
      </div>
    </article>
  );
}

export default PlanCard;