import { lazy } from 'react';
import { Navigate, Route } from 'react-router-dom';
import { withSuspense } from '../../core/router/withSuspense';

const HomePage = lazy(() =>
  import('./pages/Inicio/HomePage')
);

const PlanesEstudio = lazy(() =>
  import('./pages/PlanesEstudio/PlanesEstudio')
);

const InscripcionesContainer = lazy(() =>
  import('./pages/Inscripciones/InscripcionesContainer')
);

const ManageSlots = lazy(() =>
  import('./pages/Inscripciones/ManageSlots')
);

const Docentes = lazy(() =>
  import('./pages/Docentes/Docentes')
);

const AppLayout = lazy(() =>
  import('../../core/layouts/AppLayout')
);

export const secretarioRoutes = (rolSimulado) => (

  <Route
    key="secretario"
    path="/secretario"
    element={
      rolSimulado === 'secretario'
        ? withSuspense(
            AppLayout,
            { rolSimulado: 'secretario' }
          )
        : (
          <Navigate
            to="/estudiante/inicio"
            replace
          />
        )
    }
  >

    {/* Ruta principal */}
    <Route
      index
      element={
        <Navigate
          to="inicio"
          replace
        />
      }
    />

    {/* Inicio */}
    <Route
      path="inicio"
      element={withSuspense(HomePage)}
    />

    {/* Planes de estudio */}
    <Route
      path="planes-de-estudio"
      element={withSuspense(PlanesEstudio)}
    />

    {/* Inscripciones */}
    <Route
      path="inscripciones"
      element={
        withSuspense(InscripcionesContainer)
      }
    />

    {/* Gestionar cupos */}
    <Route
      path="manage-slots"
      element={
        withSuspense(ManageSlots)
      }
    />

    {/* Docentes */}
    <Route
      path="docentes"
      element={withSuspense(Docentes)}
    />

  </Route>
);