
import { useState } from 'react';
import YearCard from './components/YearCard';
import { useNavigate } from 'react-router-dom';
import { useEnrollments } from './useEnrollments';

const academicYears = [
  { id: 1, etiqueta: '1ro' },
  { id: 2, etiqueta: '2do' },
  { id: 3, etiqueta: '3ro' },
];

function InscripcionesContainer() {
  const navigate = useNavigate();

  const [planId, setPlanId] = useState('');
  const [notice, setNotice] = useState('');

  const {
    plans,
    summaries,
  } = useEnrollments(planId);

  // Ir a gestionar cupos solamente de 1er año.
  const handleManagePlaces = (id) => {
    if (String(id) !== '1') {
      return;
    }

    navigate('/secretario/manage-slots');
  };

  return (
    <section className="enrollments-page">

      {/* Encabezado */}
      <div className="enrollments-heading">

        <span className="enrollments-eyebrow">
          GESTIÓN ACADÉMICA
        </span>

        <h1>
          Inscripciones
        </h1>

        <p>
          Consultá el estado de las inscripciones y asigná
          comisiones por año.
        </p>

      </div>

      {/* Aviso */}
      {notice && (
        <div
          className="enrollments-notice"
          role="status"
        >
          {notice}

          <button
            type="button"
            onClick={() => setNotice('')}
            aria-label="Cerrar aviso"
          >
            ×
          </button>
        </div>
      )}

      <div
        className="enrollment-filters"
        aria-label="Filtros de inscripciones"
      >

        <label>

          <span>
            Plan de estudio
          </span>

          <select
            aria-label="Plan de estudio"
            value={planId}
            onChange={(event) =>
              setPlanId(event.target.value)
            }
          >

            <option value="">
              Todos los planes
            </option>

            {plans.map((plan) => (
              <option
                key={plan.id}
                value={plan.id}
              >
                {plan.nombre}
              </option>
            ))}

          </select>

        </label>

      </div>

      {summaries.length === 0 && <p className="enrollments-empty">Todavía no hay inscripciones cargadas.</p>}

      <div className="years-grid">

        {academicYears.map((year) => {

          const summary =
            summaries.find(
              (item) =>
                item.añoAcademico === year.id
            ) ?? {};

          return (
            <YearCard
              key={year.id}
              id={year.id}
              año={year.etiqueta}
              totalEstudiantes={
                summary.totalEstudiantes ?? 0
              }
              sinAsignacion={
                summary.estudiantesSinAsignar ?? 0
              }
              onManage={handleManagePlaces}
            />
          );
        })}

      </div>

    </section>
  );
}

export default InscripcionesContainer;
