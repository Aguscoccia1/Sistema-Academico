import { BrowserRouter, Navigate, Outlet, Route, Routes, useLocation } from 'react-router-dom';
import { useState } from 'react';
import SimuladorRoles from '../components/SimuladorRoles';
import { secretarioRoutes } from '../../modules/secretario/secretarioRoutes';
import { estudianteRoutes } from '../../modules/estudiante/estudianteRoutes';
import LoginPage from '../../modules/auth/pages/LoginPage';
import AccesoDenegadoPage from '../../modules/auth/pages/AccesoDenegadoPage';
import { obtenerSesion } from '../auth/session';
import { rolFront, rutaInicio } from '../auth/roles';
import { useExpiracionSesion } from '../auth/useExpiracionSesion';

const simuladorActivo = import.meta.env.DEV && import.meta.env.VITE_ENABLE_ROLE_SIMULATOR === 'true';

function RutaProtegida() {
  useLocation();
  useExpiracionSesion();
  return obtenerSesion() || simuladorActivo ? <Outlet /> : <Navigate to="/login" replace />;
}

function RutaPorRol({ rolRequerido, rolActual }) {
  return rolActual === rolRequerido ? <Outlet /> : <AccesoDenegadoPage rol={rolActual} />;
}

function App() {
  // El rol sale de la sesión. Solo con el simulador activo (desarrollo) se parte de "secretario".
  const [rolSimulado, setRolSimulado] = useState(
    () => rolFront(obtenerSesion()?.role) ?? (simuladorActivo ? 'secretario' : null),
  );

  const redireccionPorDefecto = <Navigate to={rutaInicio(rolSimulado)} replace />;

  return (
    <BrowserRouter>
      <SimuladorRoles rolSimulado={rolSimulado} onChange={setRolSimulado} />
      <Routes>
        <Route path="/login" element={<LoginPage onLogin={setRolSimulado} />} />
        <Route element={<RutaProtegida />}>
          <Route element={<RutaPorRol rolRequerido="secretario" rolActual={rolSimulado} />}>
            {secretarioRoutes(rolSimulado)}
          </Route>
          <Route element={<RutaPorRol rolRequerido="estudiante" rolActual={rolSimulado} />}>
            {estudianteRoutes(rolSimulado)}
          </Route>
          <Route path="/403" element={<AccesoDenegadoPage rol={rolSimulado} />} />
        </Route>
        <Route path="*" element={redireccionPorDefecto} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
