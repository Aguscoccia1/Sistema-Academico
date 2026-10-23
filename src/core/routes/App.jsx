import { BrowserRouter, Navigate, Outlet, Route, Routes, useLocation } from 'react-router-dom';
import { useState } from 'react';
import { secretarioRoutes } from '../../modules/secretario/secretarioRoutes';
import { estudianteRoutes } from '../../modules/estudiante/estudianteRoutes';
import LoginPage from '../../modules/auth/pages/LoginPage';
import AccesoDenegadoPage from '../../modules/auth/pages/AccesoDenegadoPage';
import { obtenerSesion } from '../auth/session';
import { rolFront, rutaInicio } from '../auth/roles';
import { useExpiracionSesion } from '../auth/useExpiracionSesion';

function RutaProtegida() {
  useLocation();
  useExpiracionSesion();
  return obtenerSesion() ? <Outlet /> : <Navigate to="/login" replace />;
}

function RutaPorRol({ rolRequerido, rolActual }) {
  return rolActual === rolRequerido ? <Outlet /> : <AccesoDenegadoPage rol={rolActual} />;
}

function App() {
  // El rol sale de la sesión: hasta que alguien inicia sesión es null.
  const [rol, setRol] = useState(() => rolFront(obtenerSesion()?.role));

  const redireccionPorDefecto = <Navigate to={rutaInicio(rol)} replace />;

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage onLogin={setRol} />} />
        <Route element={<RutaProtegida />}>
          <Route element={<RutaPorRol rolRequerido="secretario" rolActual={rol} />}>
            {secretarioRoutes(rol)}
          </Route>
          <Route element={<RutaPorRol rolRequerido="estudiante" rolActual={rol} />}>
            {estudianteRoutes(rol)}
          </Route>
          <Route path="/403" element={<AccesoDenegadoPage rol={rol} />} />
        </Route>
        <Route path="*" element={redireccionPorDefecto} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
