import { useState } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import Sidebar from './Sidebar';
import Header from './Header';

function AppLayout({ rolSimulado = 'secretario' }) {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const [showNotifications, setShowNotifications] = useState(false);
  const location = useLocation();
  const section = location.pathname.includes('inicio') || location.pathname === '/' ? 'Inicio' : location.pathname.includes('planes') ? 'Planes de Estudio' : location.pathname.includes('manage-slots') ? 'Gestionar cupos' : location.pathname.includes('docentes') && location.pathname.endsWith('/modificar') ? 'Modificar docente' : location.pathname.includes('docentes') && location.pathname.endsWith('/visualizar') ? 'Visualizar docente' : location.pathname.includes('modificar') ? 'Modificar estudiante' : location.pathname.includes('visualizar') ? 'Visualizar estudiante' : location.pathname.includes('inscripciones') ? 'Inscripciones' : location.pathname.includes('docentes') ? 'Docentes' : 'Estudiantes';
  const closeSidebar = () => setIsSidebarOpen(false);

  return (
    <div className="app-shell">
      <Sidebar rolSimulado={rolSimulado} isOpen={isSidebarOpen} onClose={closeSidebar} />
      {isSidebarOpen && <button aria-label="Cerrar menú" className="overlay" onClick={closeSidebar} type="button" />}

      <main>
        <Header activeSection={section} rolSimulado={rolSimulado} showNotifications={showNotifications} onToggleNotifications={() => setShowNotifications((current) => !current)} onOpenSidebar={() => setIsSidebarOpen(true)} />
        <section className="content"><Outlet /></section>
      </main>
    </div>
  );
}
export default AppLayout;
