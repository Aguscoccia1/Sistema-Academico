import { Link } from 'react-router-dom';
import { BookOpen, ChevronRight, ClipboardList } from 'lucide-react';
import { obtenerSesion } from '../../../../core/auth/session';

const fechaDeHoy = () =>
  new Date().toLocaleDateString('es-AR', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }).toUpperCase();

function HomePage() {
  const sesion = obtenerSesion();

  return (
    <section className="estudiante-dashboard student-homepage">
      <div className="welcome">
        <div>
          <p className="eyebrow">{fechaDeHoy()}</p>
          <h1>Hola, {sesion?.fullName ?? 'estudiante'} <span>👋</span></h1>
          <p>Te damos la bienvenida a tu espacio académico.</p>
        </div>
      </div>

      {sesion?.recordNumber && <div className="student-home-meta"><span>Legajo {sesion.recordNumber}</span></div>}

      <div className="dashboard-grid">
        <section className="panel shortcuts">
          <div className="panel-title"><div><h2>Accesos rápidos</h2><p>Gestioná tu cursada</p></div></div>
          <Link className="shortcut primary" to="/estudiante/inscripciones"><span><BookOpen size={20} /></span><div><b>Inscripción a materias</b><small>Consultar materias disponibles</small></div><ChevronRight size={17} /></Link>
          <Link className="shortcut" to="/estudiante/mis-inscripciones"><span><ClipboardList size={20} /></span><div><b>Mis inscripciones</b><small>Ver las materias en las que estás inscripto</small></div><ChevronRight size={17} /></Link>
        </section>
      </div>
    </section>
  );
}

export default HomePage;
