import { Link } from 'react-router-dom';
import { ChevronRight, ClipboardList, GraduationCap } from 'lucide-react';
import { obtenerSesion } from '../../../../core/auth/session';

const fechaDeHoy = () =>
  new Date().toLocaleDateString('es-AR', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }).toUpperCase();

function HomePage() {
  const sesion = obtenerSesion();

  return (
    <>
      <div className="welcome">
        <div>
          <p className="eyebrow">{fechaDeHoy()}</p>
          <h1>Hola, {sesion?.fullName ?? 'Secretaría'} <span>👋</span></h1>
          <p>Te damos la bienvenida al panel de Secretaría Académica.</p>
        </div>
      </div>

      <div className="dashboard-grid">
        <section className="panel shortcuts">
          <div className="panel-title"><div><h2>Accesos rápidos</h2><p>Gestiones frecuentes</p></div></div>
          <Link className="shortcut primary" to="/secretario/planes-de-estudio"><span><GraduationCap size={20} /></span><div><b>Planes de estudio</b><small>Crear y organizar planes y materias</small></div><ChevronRight size={18} /></Link>
          <Link className="shortcut" to="/secretario/inscripciones"><span><ClipboardList size={20} /></span><div><b>Inscripciones</b><small>Estado de las inscripciones por año</small></div><ChevronRight size={18} /></Link>
        </section>
      </div>
    </>
  );
}

export default HomePage;
