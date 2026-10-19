import { useState } from 'react';
import { AlertCircle, ArrowRight, Clock, Eye, EyeOff, Lock, User } from 'lucide-react';
import { login } from '../../../api/authApi';
import { guardarSesion } from '../../../core/auth/session';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { rolFront, rutaInicio } from '../../../core/auth/roles';

function LoginPage({ onLogin }) {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const sesionExpirada = searchParams.get('motivo') === 'expirada';
  const [usuario, setUsuario] = useState('');
  const [password, setPassword] = useState('');
  const [verPassword, setVerPassword] = useState(false);
  const [errores, setErrores] = useState({});
  const [errorGeneral, setErrorGeneral] = useState('');
  const [cargando, setCargando] = useState(false);
  const [recordar, setRecordar] = useState(false);

  const validar = () => {
    const nuevos = {};
    if (!usuario.trim()) nuevos.usuario = 'Ingresá tu correo.';
    if (!password) nuevos.password = 'Ingresá tu contraseña.';
    setErrores(nuevos);
    return Object.keys(nuevos).length === 0;
  };

  const handleSubmit = async (e) => {
  e.preventDefault();
  setErrorGeneral('');
  if (!validar()) return;

  setCargando(true);
  try {
    const sesion = await login(usuario.trim(), password);
    const rol = rolFront(sesion.role);
    if (!rol) {
        setErrorGeneral('Tu usuario no tiene un rol habilitado para ingresar.');
        return;
    }
    guardarSesion(sesion, recordar);
    onLogin(rol);
    navigate(rutaInicio(rol), { replace: true });
  } catch (error) {
    if (error.status === 401) {
      setErrorGeneral('Correo o contraseña incorrectos.');
    } else {
      setErrorGeneral('No pudimos conectar con el servidor. Intentá de nuevo.');
    }
  } finally {
    setCargando(false);
  }
};

  return (
    <div className="login">
      <aside className="login__marca">
        <h2>Sistema Académico</h2>
        <p>Inscripciones, planes de estudio y correlatividades del instituto, en un solo lugar.</p>
      </aside>

      <div className="login__contenido">
        <form className="login__form" onSubmit={handleSubmit} noValidate>
          <h1>Iniciar sesión</h1>
          <p className="login__sub">Ingresá con tu correo institucional para continuar.</p>

          {sesionExpirada && !errorGeneral && (
            <div className="login__aviso" role="status">
              <Clock size={18} /> Tu sesión expiró. Ingresá de nuevo para continuar.
            </div>
          )}
          
          {errorGeneral && (
            <div className="login__alerta" role="alert">
              <AlertCircle size={18} /> {errorGeneral}
            </div>
          )}

          <label htmlFor="usuario">Correo</label>
          <div className={`login__campo ${errores.usuario ? 'login__campo--error' : ''}`}>
            <User size={18} />
            <input id="usuario" type="email" autoComplete="email" value={usuario} onChange={(e) => setUsuario(e.target.value)} placeholder="Ej: nombre@uni.edu" />
          </div>
          {errores.usuario && <small className="login__error">{errores.usuario}</small>}

          <label htmlFor="password">Contraseña</label>
          <div className={`login__campo ${errores.password ? 'login__campo--error' : ''}`}>
            <Lock size={18} />
            <input id="password" type={verPassword ? 'text' : 'password'} value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Ingresá tu contraseña" />
            <button type="button" onClick={() => setVerPassword((v) => !v)} aria-label="Mostrar contraseña">
              {verPassword ? <EyeOff size={18} /> : <Eye size={18} />}
            </button>
          </div>
          {errores.password && <small className="login__error">{errores.password}</small>}
        <div className="login__opciones">
            <label className="login__recordarme">
                <input type="checkbox" checked={recordar} onChange={(e) => setRecordar(e.target.checked)} />
                Recordarme
        </label>
        </div>
          <button className="login__boton" type="submit" disabled={cargando}>
            {cargando ? 'Ingresando...' : <>Ingresar <ArrowRight size={18} /></>}
          </button>
        </form>
      </div>
    </div>
  );
}

export default LoginPage;