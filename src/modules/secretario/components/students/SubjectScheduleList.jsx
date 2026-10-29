import { useEffect, useState } from 'react';
import { obtenerHorariosMateria } from '../../../../services/subjectScheduleService';

const DIAS_SEMANA = {
  Monday: 'Lunes',
  Tuesday: 'Martes',
  Wednesday: 'Miércoles',
  Thursday: 'Jueves',
  Friday: 'Viernes',
  Saturday: 'Sábado',
  Sunday: 'Domingo',
};

function SubjectScheduleList({
  subjectId,
  section,
  subjectName,
  onChange,
  readOnly = false,
}) {
  const [resultado, setResultado] = useState({
    section: '',
    cargando: false,
    horarios: [],
    error: '',
  });

  useEffect(() => {
    if (!subjectId || !section) {
      setResultado({ section: '', cargando: false, horarios: [], error: '' });
      return undefined;
    }

    let cancelado = false;
    setResultado({ section, cargando: true, horarios: [], error: '' });

    obtenerHorariosMateria(subjectId, section)
      .then((horarios) => {
        if (!cancelado) {
          setResultado({ section, cargando: false, horarios, error: '' });
        }
      })
      .catch((err) => {
        console.error(err);
        if (!cancelado) {
          const detalle = err.status === 404
            ? `${err.message} Verificá que la materia exista y reiniciá el backend para cargar el endpoint de horarios.`
            : err.message || 'Revisá la conexión con la API.';
          setResultado({
            section,
            cargando: false,
            horarios: [],
            error: `No se pudieron cargar los horarios. ${detalle}`,
          });
        }
      });

    return () => {
      cancelado = true;
    };
  }, [subjectId, section]);

  const resultadoActual = resultado.section === section ? resultado : null;
  const horariosTexto = resultadoActual?.horarios.map((horario) =>
    `${DIAS_SEMANA[horario.day] ?? horario.day} ${horario.startTime.slice(0, 5)} hs a ${horario.endTime.slice(0, 5)} hs`
  ) ?? [];
  const textoSeleccionado = !section
    ? readOnly
      ? 'Sin comisión asignada'
      : 'Elegir comisión'
    : resultadoActual?.cargando || !resultadoActual
      ? `Comisión ${section} - Cargando horario...`
      : resultadoActual.error
        ? `Comisión ${section} - No se pudo cargar el horario`
        : horariosTexto.length === 0
          ? `Comisión ${section} - Sin horarios configurados`
          : `Comisión ${section} - ${
              horariosTexto.length === 1
                ? horariosTexto[0]
                : `${horariosTexto.slice(0, -1).join(', ')} y ${horariosTexto.at(-1)}`
            }`;

  if (readOnly) {
    return (
      <>
        <div
          className="date"
          aria-label={`Comisión y horario de ${subjectName}`}
        >
          {textoSeleccionado}
        </div>
        {section && resultadoActual?.error && (
          <small className="schedule-error" role="alert">
            {resultadoActual.error}
          </small>
        )}
      </>
    );
  }

  return (
    <>
      <select
        className="date"
        aria-label={`Comisión y horario de ${subjectName}`}
        value={section}
        onChange={(event) => onChange(event.target.value)}
      >
        <option value="">{textoSeleccionado}</option>
        {['A', 'B'].map((comision) => (
          <option key={comision} value={comision}>
            {section === comision
              ? textoSeleccionado
              : `Comisión ${comision}`}
          </option>
        ))}
      </select>
      {section && resultadoActual?.error && (
        <small className="schedule-error" role="alert">
          {resultadoActual.error}
        </small>
      )}
    </>
  );
}

export default SubjectScheduleList;
