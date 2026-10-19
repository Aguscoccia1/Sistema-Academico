
import { useState } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import Sidebar from './Sidebar';
import Header from './Header';

function AppLayout({ rolSimulado = 'secretario' }) {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const [showNotifications, setShowNotifications] = useState(false);
  const location = useLocation();
<<<<<<< HEAD

  const section = location.pathname.includes('inicio') || location.pathname === '/' ? 'Inicio' : location.pathname.includes('planes') ? 'Planes de Estudio' : location.pathname.includes('manage-slots') ? 'Gestionar cupos' : location.pathname.includes('modificar') ? 'Modificar estudiante' : location.pathname.includes('visualizar') ? 'Visualizar estudiante' : location.pathname.includes('inscripciones') ? 'Inscripciones' : location.pathname.includes('docentes') ? 'Docentes' : 'Estudiantes';

  const hideNotifications =
    location.pathname === '/secretario/manage-slots' ||
    (
      location.pathname.startsWith('/secretario/estudiantes/') &&
      (
        location.pathname.endsWith('/modificar') ||
        location.pathname.endsWith('/visualizar')
      )
    );

=======
  const section = location.pathname.includes('mis-inscripciones') ? 'Mis Inscripciones' : location.pathname.includes('inicio') || location.pathname === '/' ? 'Inicio' : location.pathname.includes('planes') ? 'Planes de Estudio' : location.pathname.includes('inscripciones') ? 'Inscripciones' : location.pathname.includes('docentes') ? 'Docentes' : 'Estudiantes';
>>>>>>> 8e032f25645166d7dc5245e9d31310f5accbef4d
  const closeSidebar = () => setIsSidebarOpen(false);

  return (
    <div className="app-shell">
      <Sidebar
        rolSimulado={rolSimulado}
        isOpen={isSidebarOpen}
        onClose={closeSidebar}
      />

      {isSidebarOpen && (
        <button
          aria-label="Cerrar menú"
          className="overlay"
          onClick={closeSidebar}
          type="button"
        />
      )}

      <main>
        <Header
          activeSection={section}
          rolSimulado={rolSimulado}
          showNotifications={showNotifications}
          showNotificationButton={!hideNotifications}
          onToggleNotifications={() =>
            setShowNotifications((current) => !current)
          }
          onOpenSidebar={() => setIsSidebarOpen(true)}
        />

        <section className="content">
          <Outlet />
        </section>
      </main>
    </div>
  );
}

<<<<<<< HEAD
export default AppLayout;

=======
export default AppLayout;
>>>>>>> 8e032f25645166d7dc5245e9d31310f5accbef4d
