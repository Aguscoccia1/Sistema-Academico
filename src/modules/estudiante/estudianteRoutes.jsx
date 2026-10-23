import { lazy } from 'react';
import { Navigate, Outlet, Route } from 'react-router-dom';
import { withSuspense } from '../../core/router/withSuspense';
import { InscripcionesProvider } from './pages/Inscripciones/context/InscripcionesContext';

const HomePageEstudiante = lazy(() => import('./pages/Inicio/HomePage'));
const InscripcionesEstudiante = lazy(() => import('./pages/Inscripciones/InscripcionesEstudiante'));
const MisInscripciones = lazy(() => import('./pages/Inscripciones/MisInscripciones'));
const AppLayout = lazy(() => import('../../core/layouts/AppLayout'));

function EstudianteProviders() {
  return (
    <InscripcionesProvider>
      <Outlet />
    </InscripcionesProvider>
  );
}

export const estudianteRoutes = (rolSimulado) => (
  <Route
    key="estudiante"
    path="/estudiante"
    element={
      rolSimulado === 'estudiante'
        ? withSuspense(AppLayout, { rolSimulado: 'estudiante' })
        : <Navigate to="/secretario/inicio" replace />
    }
  >
    <Route index element={<Navigate to="inicio" replace />} />
    <Route element={<EstudianteProviders />}>
      <Route path="inicio" element={withSuspense(HomePageEstudiante)} />
      <Route path="inscripciones" element={withSuspense(InscripcionesEstudiante)} />
      <Route path="mis-inscripciones" element={withSuspense(MisInscripciones)} />
    </Route>
  </Route>
);