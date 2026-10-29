import { lazy, Suspense } from 'react';
import { Navigate, Route } from 'react-router-dom';
import { withSuspense } from '../../core/router/withSuspense';

const HomePage = lazy(() => import('./pages/Inicio/HomePage'));
const PlanesEstudio = lazy(() => import('./pages/PlanesEstudio/PlanesEstudio'));
const InscripcionesContainer = lazy(() => import('./pages/Inscripciones/InscripcionesContainer'));
const Usuarios = lazy(() => import('./pages/Usuarios/Usuarios'));
const AppLayout = lazy(() => import('../../core/layouts/AppLayout'));
const ManageSlots = lazy(() => import('./pages/Inscripciones/ManageSlots'));
const ModificarEstudiante = lazy(() => import('./pages/Inscripciones/ModificarEstudiante'));
const VisualizarEstudiante = lazy(() => import('./pages/Inscripciones/VisualizarEstudiante'));
const Docentes = lazy(() => import('./pages/Docentes/ManageTeachers'));
const ModificarDocente = lazy(() => import('./pages/Docentes/ModificarDocente'));
const VisualizarDocente = lazy(() => import('./pages/Docentes/VisualizarDocente'));

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
    <Route path="manage-slots" element={withSuspense(ManageSlots)} />
    <Route path="estudiantes/:id/modificar" element={withSuspense(ModificarEstudiante)} />
    <Route path="estudiantes/:id/visualizar" element={withSuspense(VisualizarEstudiante)} />
    <Route path="docentes" element={withSuspense(Docentes)} />
    <Route path="docentes/:id/modificar" element={withSuspense(ModificarDocente)} />
    <Route path="docentes/:id/visualizar" element={withSuspense(VisualizarDocente)} />
  </Route>
);
