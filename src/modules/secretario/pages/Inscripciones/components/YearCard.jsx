function YearCard({
  id,
  año,
  totalEstudiantes,
  sinAsignacion,
  onManage
}) {
  const hayBajoCupo = sinAsignacion > 0;

  return (
    <article className="year-card">

      {/* Encabezado */}
      <div className="year-card-header">

        <div className="year-card-icon">
          📚
        </div>

        <div className="year-card-label">
          Año académico
        </div>

      </div>

      {/* Año */}
      <h2>
        {año}
      </h2>

      {/* Cantidad de estudiantes */}
      <p className="year-students">

        <strong>
          {totalEstudiantes}
        </strong>

        {' '}
        estudiantes

      </p>

      {/* Estado */}
      <p
        className={`year-assignment ${
          hayBajoCupo
            ? 'year-assignment-warning'
            : 'year-assignment-success'
        }`}
      >

        {hayBajoCupo
          ? `${sinAsignacion} sin asignación`
          : 'Cupos asignados'}

      </p>

      {/* Gestionar */}
      <button
        type="button"
        className="manage-places-button"
        onClick={() => onManage(id)}
      >
        Gestionar cupos
      </button>

    </article>
  );
}

export default YearCard;