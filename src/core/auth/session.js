const CLAVE = 'sesion';

export function guardarSesion(sesion, recordar = false) {
  cerrarSesion();
  const almacen = recordar ? localStorage : sessionStorage;
  almacen.setItem(CLAVE, JSON.stringify(sesion));
}

export function obtenerSesion() {
  const guardada = sessionStorage.getItem(CLAVE) ?? localStorage.getItem(CLAVE);
  return guardada ? JSON.parse(guardada) : null;
}

export function obtenerToken() {
  return obtenerSesion()?.token ?? null;
}

export function cerrarSesion() {
  sessionStorage.removeItem(CLAVE);
  localStorage.removeItem(CLAVE);
}

function leerVencimiento(token) {
  try {
    const payload = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    return JSON.parse(atob(payload)).exp ?? null;
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