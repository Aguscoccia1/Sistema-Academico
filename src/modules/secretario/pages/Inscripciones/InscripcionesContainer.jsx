import { useState } from 'react';
import YearCard from './components/YearCard';

function InscripcionesContainer() {
  const [planId, setPlanId] = useState('');
  const [notice, setNotice] = useState('');
  // los años, los planes y los resúmenes van a venir de la API
  const years = [];
  const plans = [];
  const summaries = [];

  const handleManagePlaces = (idAño) => {
    const selectedYear = years.find((year) => year.id === idAño);
    if (selectedYear) {
      setNotice(`Gestionar cupos de ${selectedYear.etiqueta} Año.`);
    }
  };

  return (
    <section className="enrollments-page">
      <div className="enrollments-heading">
        <span className="enrollments-eyebrow">GESTIÓN ACADÉMICA</span>
        <h1>Inscripciones</h1>
        <p>Consultá el estado de las inscripciones y asigná comisiones por año.</p>
      </div>

      {notice && (
        <div className="enrollments-notice" role="status">
          {notice}
          <button type="button" onClick={() => setNotice('')} aria-label="Cerrar aviso">×</button>
        </div>
      )}

      <div className="enrollment-filters" aria-label="Filtros de inscripciones">
        <label>
          <span>Plan de estudio</span>
          <select aria-label="Plan de estudio" value={planId} onChange={(event) => setPlanId(event.target.value)}>
            <option value="">Todos los planes</option>
            {plans.map((plan) => (
              <option key={plan.id} value={plan.id}>{plan.nombre}</option>
            ))}
          </select>
        </label>
      </div>

      {years.length === 0 && <p className="enrollments-empty">Todavía no hay inscripciones cargadas.</p>}

      <div className="years-grid">
        {years.map((year) => {
          const summary = summaries.find((item) => item.añoAcademico === year.id) ?? {};
          return (
            <YearCard
              key={year.id}
              id={year.id}
              año={year.etiqueta}
              totalEstudiantes={summary.totalEstudiantes ?? 0}
              sinAsignacion={summary.estudiantesSinAsignar ?? 0}
              onManage={handleManagePlaces}
            />
          );
        })}
      </div>
    </section>
  );
}

export default InscripcionesContainer;
