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


const ModificarEstudiante = lazy(() =>
  import('./pages/Inscripciones/ModificarEstudiante')
);


const VisualizarEstudiante = lazy(() =>
  import('./pages/Inscripciones/VisualizarEstudiante')
);


const Docentes = lazy(() =>
  import('./pages/Docentes/ManageTeachers')
);

const ModificarDocente = lazy(() =>
  import('./pages/Docentes/ModificarDocente')
);

const VisualizarDocente = lazy(() =>
  import('./pages/Docentes/VisualizarDocente')
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
            {
              rolSimulado: 'secretario'
            }
          )

        : (
          <Navigate
            to="/estudiante/inicio"
            replace
          />
        )

    }
  >


    {/* =========================
        RUTA PRINCIPAL
    ========================= */}

    <Route
      index
      element={
        <Navigate
          to="inicio"
          replace
        />
      }
    />


    {/* =========================
        INICIO
    ========================= */}

    <Route
      path="inicio"
      element={
        withSuspense(HomePage)
      }
    />


    {/* =========================
        PLANES DE ESTUDIO
    ========================= */}

    <Route
      path="planes-de-estudio"
      element={
        withSuspense(PlanesEstudio)
      }
    />


    {/* =========================
        INSCRIPCIONES
    ========================= */}

    <Route
      path="inscripciones"
      element={
        withSuspense(
          InscripcionesContainer
        )
      }
    />


    {/* =========================
        GESTIONAR CUPOS
    ========================= */}

    <Route
      path="manage-slots"
      element={
        withSuspense(
          ManageSlots
        )
      }
    />


    {/* =========================
        MODIFICAR ESTUDIANTE
    ========================= */}

    <Route
      path="estudiantes/:id/modificar"
      element={
        withSuspense(
          ModificarEstudiante
        )
      }
    />


    {/* =========================
        VISUALIZAR ESTUDIANTE
    ========================= */}

    <Route
      path="estudiantes/:id/visualizar"
      element={
        withSuspense(
          VisualizarEstudiante
        )
      }
    />


    {/* =========================
        DOCENTES
    ========================= */}

    <Route
      path="docentes"
      element={
        withSuspense(
          Docentes
        )
      }
    />

    <Route
      path="docentes/:id/modificar"
      element={
        withSuspense(
          ModificarDocente
        )
      }
    />

    <Route
      path="docentes/:id/visualizar"
      element={
        withSuspense(
          VisualizarDocente
        )
      }
    />

  </Route>

);