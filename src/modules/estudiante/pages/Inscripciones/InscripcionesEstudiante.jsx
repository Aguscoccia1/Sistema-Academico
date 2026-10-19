import { useState } from 'react';
import { Link } from 'react-router-dom';
import { CalendarDays, ChevronRight, FileText, Lock, CheckCircle2, ListChecks } from 'lucide-react';
import MateriaInscripcionRow from './components/MateriaInscripcionRow';
import ModalConfirmarInscripcion from './components/ModalConfirmarInscripcion';
import ModalDetalleMateria from './components/ModalDetalleMateria';
import { useInscripciones, PERIODO_TEXTO } from './context/InscripcionesContext';

const MES_ACTUAL_SIMULADO = 'Marzo';
const PERIODO_DE_INSCRIPCION = 'Marzo';

const FILTROS = [
  { id: 'todas', label: 'Todas' },
  { id: 'habilitadas', label: 'Habilitadas' },
  { id: 'bloqueadas', label: 'Bloqueadas' },
  { id: 'aprobadas', label: 'Aprobadas' },
];

const ESTADOS_VACIOS = {
  todas: {
    icono: FileText,
    titulo: 'Todavía no hay materias',
    texto: 'Cuando las materias estén cargadas aparecerán acá',
  },
  habilitadas: {
    icono: ListChecks,
    titulo: 'No hay materias habilitadas',
    texto: 'Cuando hayan materias disponibles aparecerán acá',
  },
  bloqueadas: {
    icono: Lock,
    titulo: 'No tenés materias bloqueadas',
    texto: 'Cumpliste las correlatividades de todas tus materias',
  },
  aprobadas: {
    icono: CheckCircle2,
    titulo: 'Todavía no tenés materias aprobadas',
    texto: 'Cuando apruebes una materia aparecerá acá',
  },
};

const ORDINALES = { 1: '1ro', 2: '2do', 3: '3ro', 4: '4to', 5: '5to' };
const formatearAnio = (anio) => ORDINALES[anio] ?? `${anio}°`;

function InscripcionesEstudiante() {
  const { materiasPreparadas, inscribir } = useInscripciones();
  const [aviso, setAviso] = useState('');
  const [filtro, setFiltro] = useState('todas');
  const [modal, setModal] = useState(null); // { tipo: 'confirmar' | 'detalle', materiaId }
  const periodoAbierto = MES_ACTUAL_SIMULADO === PERIODO_DE_INSCRIPCION;

  // Las materias inscriptas (estado 'R') se ven en "Mis Inscripciones", no acá
  const materiasDisponibles = materiasPreparadas.filter((materia) => materia.estado !== 'R');

  const materiasFiltradas = materiasDisponibles.filter((materia) => {
    const aprobada = materia.estado === 'A';
    const bloqueada = !!materia.correlativaBloqueante && !aprobada;

    if (filtro === 'aprobadas') return aprobada;
    if (filtro === 'habilitadas') return !bloqueada && !aprobada;
    if (filtro === 'bloqueadas') return bloqueada;
    return true;
  });

  const estadoVacio = ESTADOS_VACIOS[filtro];
  const IconoVacio = estadoVacio.icono;

  const materiaModal = modal ? materiasPreparadas.find(({ id }) => id === modal.materiaId) : null;

  const cerrarModal = () => setModal(null);
  const abrirConfirmacion = (materiaId) => setModal({ tipo: 'confirmar', materiaId });
  const abrirDetalle = (materiaId) => setModal({ tipo: 'detalle', materiaId });

  const handleConfirmarInscripcion = () => {
    if (!materiaModal) return;
    inscribir(materiaModal.id);
    setAviso(`Te inscribiste a ${materiaModal.nombre}. La podés ver en Mis Inscripciones.`);
    cerrarModal();
  };

  return (
    <section className="estudiante-dashboard">
      <nav className="breadcrumb" aria-label="Ruta de navegación">
        <Link to="/estudiante/inicio">Inicio</Link>
        <ChevronRight size={14} />
        <span>Inscripciones a materias</span>
      </nav>

      <header className="inscripciones-header">
        <div>
          <h1>Inscripciones a materias</h1>
          <p>Consulta las materias disponibles para inscribirse</p>
        </div>
        <div className="periodo-card">
          <CalendarDays size={38} />
          <div>
            <small>Período de Inscripción</small>
            <strong>{PERIODO_TEXTO}</strong>
          </div>
        </div>
      </header>

      {!periodoAbierto && (
        <div className="estudiante-periodo-cerrado" role="status">
          El período de inscripciones ({PERIODO_DE_INSCRIPCION}) se encuentra cerrado.
        </div>
      )}

      {aviso && (
        <div className="estudiante-aviso" role="status">
          {aviso}
          <button type="button" onClick={() => setAviso('')} aria-label="Cerrar aviso">
            ×
          </button>
        </div>
      )}

      <div className="tabs-bar">
        <div className="tabs" role="tablist">
          {FILTROS.map(({ id, label }) => (
            <button
              key={id}
              type="button"
              role="tab"
              aria-selected={filtro === id}
              className={`tab ${filtro === id ? 'tab--activa' : ''}`}
              onClick={() => setFiltro(id)}
            >
              {label}
            </button>
          ))}
        </div>
        <span className="tabs-contador">{materiasFiltradas.length} Materias</span>
      </div>

      {materiasFiltradas.length === 0 ? (
        <div className="estado-vacio" role="status">
          <div className="estado-vacio__icono">
            <IconoVacio size={32} />
          </div>
          <h2>{estadoVacio.titulo}</h2>
          <p>{estadoVacio.texto}</p>
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
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {materiasFiltradas.map((materia) => (
                <MateriaInscripcionRow
                  key={materia.id}
                  materia={materia}
                  periodoAbierto={periodoAbierto}
                  onInscribir={abrirConfirmacion}
                  onVerDetalle={abrirDetalle}
                />
              ))}
            </tbody>
          </table>
        </div>
      )}

      {materiaModal && modal.tipo === 'confirmar' && (
        <ModalConfirmarInscripcion
          materia={materiaModal}
          anioTexto={formatearAnio(materiaModal.año)}
          onConfirmar={handleConfirmarInscripcion}
          onCancelar={cerrarModal}
        />
      )}

      {materiaModal && modal.tipo === 'detalle' && (
        <ModalDetalleMateria
          materia={materiaModal}
          anioTexto={formatearAnio(materiaModal.año)}
          onCerrar={cerrarModal}
        />
      )}
    </section>
  );
}

export default InscripcionesEstudiante;