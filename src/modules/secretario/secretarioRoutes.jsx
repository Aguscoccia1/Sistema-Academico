import { lazy, Suspense } from 'react';
import { Navigate, Route } from 'react-router-dom';
import { withSuspense } from '../../core/router/withSuspense';

const HomePage = lazy(() => import('./pages/Inicio/HomePage'));
const PlanesEstudio = lazy(() => import('./pages/PlanesEstudio/PlanesEstudio'));
const InscripcionesContainer = lazy(() => import('./pages/Inscripciones/InscripcionesContainer'));
const Usuarios = lazy(() => import('./pages/Usuarios/Usuarios'));
const AppLayout = lazy(() => import('../../core/layouts/AppLayout'));

export const secretarioRoutes = (rolSimulado) => (
  <Route
    key="secretario"
    path="/secretario"
    element={
      rolSimulado === 'secretario'
        ? withSuspense(AppLayout, { rolSimulado: 'secretario' })
        : <Navigate to="/estudiante/inicio" replace />
    }
  >
    <Route index element={<Navigate to="inicio" replace />} />
    <Route path="inicio" element={withSuspense(HomePage)} />
    <Route path="planes-de-estudio" element={withSuspense(PlanesEstudio)} />
    <Route path="inscripciones" element={withSuspense(InscripcionesContainer)} />
    <Route path="usuarios" element={withSuspense(Usuarios)} />
  </Route>
);
