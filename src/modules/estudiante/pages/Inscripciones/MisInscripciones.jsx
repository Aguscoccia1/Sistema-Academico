import { Link, useNavigate } from 'react-router-dom';
import { CalendarDays, ChevronRight, FileText } from 'lucide-react';
import { useInscripciones, PERIODO_TEXTO } from './context/InscripcionesContext';
import MisInscripcionesRow from './components/MisInscripcionesRow';

function MisInscripciones() {
  const navigate = useNavigate();
  const { materiasPreparadas } = useInscripciones();
  const inscriptas = materiasPreparadas.filter((materia) => materia.estado === 'R');

  return (
    <section className="estudiante-dashboard">
      <nav className="breadcrumb" aria-label="Ruta de navegación">
        <Link to="/estudiante/inicio">Inicio</Link>
        <ChevronRight size={14} />
        <span>Mis Inscripciones</span>
      </nav>

      <header className="inscripciones-header">
        <div>
          <h1>Mis Inscripciones</h1>
          <p>Consulta las materias a las que te has inscripto</p>
        </div>
        <div className="periodo-card">
          <CalendarDays size={38} />
          <div>
            <small>Período de Inscripción</small>
            <strong>{PERIODO_TEXTO}</strong>
          </div>
        </div>
      </header>

      {inscriptas.length === 0 ? (
        <div className="estado-vacio" role="status">
          <div className="estado-vacio__icono">
            <FileText size={32} />
          </div>
          <h2>Todavía no te inscribiste a ninguna materia</h2>
          <p>Primero debes inscribirte a una materia para que se puedan visualizar</p>
          <button
            type="button"
            className="boton-primario"
            onClick={() => navigate('/estudiante/inscripciones')}
          >
            Ir a inscribirse
          </button>
        </div>
      ) : (
        <div className="tabla-wrapper">
          <table className="tabla-materias">
            <thead>
              <tr>
                <th>Materias</th>
                <th>Año</th>
                <th>Régimen</th>
                <th>Correlativas</th>
                <th>Estado</th>
              </tr>
            </thead>
            <tbody>
              {inscriptas.map((materia) => (
                <MisInscripcionesRow key={materia.id} materia={materia} />
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}

export default MisInscripciones;