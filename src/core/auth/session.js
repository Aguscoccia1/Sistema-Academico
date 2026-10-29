const CLAVE = 'sesion';
const CLAVE_ACTIVIDAD = 'sesion:ultimaActividad';

function almacenSesion() {
  return sessionStorage.getItem(CLAVE) ? sessionStorage : localStorage;
}

export function guardarSesion(sesion, recordar = false) {
  cerrarSesion();
  const almacen = recordar ? localStorage : sessionStorage;
  almacen.setItem(CLAVE, JSON.stringify(sesion));
  almacen.setItem(CLAVE_ACTIVIDAD, String(Date.now()));
}

export function obtenerSesion() {
  const guardada = sessionStorage.getItem(CLAVE) ?? localStorage.getItem(CLAVE);
  if (!guardada) return null;

  try {
    const sesion = JSON.parse(guardada);
    if (!sesion || typeof sesion !== 'object' || Array.isArray(sesion) || !sesion.token) {
      throw new Error('Sesión inválida.');
    }
    return sesion;
  } catch {
    cerrarSesion();
    return null;
  }
}

export function obtenerToken() {
  return obtenerSesion()?.token ?? null;
}

export function actualizarTokens({ token, refreshToken }) {
  const sesion = obtenerSesion();
  if (!sesion) return;

  almacenSesion().setItem(CLAVE, JSON.stringify({ ...sesion, token, refreshToken }));
}

export function marcarActividad(ahora = Date.now()) {
  if (!obtenerSesion()) return;

  const almacen = almacenSesion();
  const actual = Number(almacen.getItem(CLAVE_ACTIVIDAD));

  // No retrocedemos la marca: una pestaña vieja no puede acortar la sesión de otra.
  if (!Number.isFinite(actual) || ahora > actual) {
    almacen.setItem(CLAVE_ACTIVIDAD, String(ahora));
  }
}

export function obtenerUltimaActividad() {
  if (!obtenerSesion()) return null;

  const valor = Number(almacenSesion().getItem(CLAVE_ACTIVIDAD));
  return Number.isFinite(valor) && valor > 0 ? valor : null;
}

export function cerrarSesion() {
  for (const almacen of [sessionStorage, localStorage]) {
    almacen.removeItem(CLAVE);
    almacen.removeItem(CLAVE_ACTIVIDAD);
  }
}

function leerVencimiento(token) {
  try {
    const payload = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    const conPadding = payload.padEnd(payload.length + (4 - (payload.length % 4)) % 4, '=');
    return JSON.parse(atob(conPadding)).exp ?? null;
  } catch {
    return null;
  }
}

export function tokenVencido() {
  const token = obtenerToken();
  if (!token) return false;
  const vencimiento = leerVencimiento(token);
  return vencimiento !== null && vencimiento * 1000 <= Date.now();
}

export function expirarSesion() {
  cerrarSesion();
  window.location.assign('/login?motivo=expirada');
}
