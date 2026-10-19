import { BookOpen, ChevronRight, ClipboardList, GraduationCap, Grid2X2, LogOut, Settings, ShieldCheck, UserRound, Users, X } from 'lucide-react';
import { NavLink, useNavigate } from 'react-router-dom';
import icgbLogo from '../../assets/icgb-logo.svg';
import { cerrarSesion } from '../auth/session';
import { menuParaRol } from '../auth/permisos';

const ICONS = { Grid2X2, GraduationCap, ClipboardList, Users, UserRound, BookOpen, ShieldCheck };

function Sidebar({ rolSimulado = 'secretario', isOpen, onClose }) {
  const menuItems = menuParaRol(rolSimulado);
  const esEstudiante = rolSimulado === 'estudiante';
  const navigate = useNavigate();
  const handleCerrarSesion = () => {
    cerrarSesion();
    navigate('/login', { replace: true });
  };

  return (
    <aside className={`sidebar ${isOpen ? 'open' : ''}`}>
      <div className="brand"><img className="brand-logo" src={icgbLogo} alt="ICGB" /><button className="close" onClick={onClose} type="button"><X size={20} /></button></div>
      <div className="role-label">{esEstudiante ? 'PORTAL DEL ESTUDIANTE' : 'SECRETARÍA ACADÉMICA'}</div>
      <nav>
        {menuItems.map(({ label, path, icon, habilitado }) => {
          const Icon = ICONS[icon];
          // Módulo todavía no desarrollado: se ve, pero deshabilitado.
          if (!habilitado) {
            return (
              <span key={path} className="nav-item nav-item--disabled" aria-disabled="true" title="Próximamente">
                <Icon size={19} /><span>{label}</span><small className="nav-dot">Pronto</small>
              </span>
            );
          }
          return <NavLink key={path} to={path} end className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`} onClick={onClose}><Icon size={19} /><span>{label}</span></NavLink>;
        })}
      </nav>
      <div className="sidebar-bottom"><button className="nav-item" type="button"><Settings size={19} /><span>Configuración</span></button><button className="nav-item" type="button" onClick={handleCerrarSesion}><LogOut size={19} /><span>Cerrar sesión</span></button><div className="help-card"><div className="help-icon">?</div><div><strong>¿Necesitás ayuda?</strong><span>Consultá el centro de ayuda</span></div><ChevronRight size={16} /></div></div>
    </aside>
  );
}

export default Sidebar;
