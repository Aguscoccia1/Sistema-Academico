import {
  actualizarTokens,
  expirarSesion,
  obtenerSesion,
  obtenerToken,
} from '../core/auth/session';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
const RUTAS_SIN_RENOVACION = ['/api/auth/login', '/api/auth/refresh'];

export class ApiError extends Error {
  constructor(message, status) {
    super(message);
    this.status = status;
  }
}

async function leerMensajeDeError(response) {
  try {
    const texto = await response.text();
    if (!texto) return null;
    try {
      const datos = JSON.parse(texto);
      return datos.message ?? datos.title ?? null;
    } catch {
      return texto.length < 200 ? texto : null;
    }
  } catch {
    return null;
  }
}

async function enviar(path, options, token) {
  return fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
      ...options.headers,
    },
  });
}

let renovacionEnCurso = null;

export async function renovarSesion() {
  if (!API_BASE_URL) return null;

  const sesion = obtenerSesion();
  if (!sesion?.refreshToken) return null;

  renovacionEnCurso ??= (async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken: sesion.refreshToken }),
      });

      if (!response.ok) return null;

      const datos = await response.json();
      if (!datos?.token || !datos?.refreshToken) return null;

      actualizarTokens(datos);
      return datos.token;
    } catch {
      return null;
    } finally {
      renovacionEnCurso = null;
    }
  })();

  return renovacionEnCurso;
}

export async function request(path, options = {}) {
  if (!API_BASE_URL) {
    throw new Error('No hay una API configurada. Definí VITE_API_BASE_URL para usar el backend.');
  }

  const token = obtenerToken();
  let response = await enviar(path, options, token);

  if (response.status === 401 && token && !RUTAS_SIN_RENOVACION.includes(path)) {
    const nuevoToken = await renovarSesion();

    if (!nuevoToken) {
      expirarSesion();
      throw new ApiError('La sesión expiró. Iniciá sesión de nuevo.', 401);
    }

    response = await enviar(path, options, nuevoToken);
  }

  if (!response.ok) {
    if (response.status === 401 && token && !RUTAS_SIN_RENOVACION.includes(path)) {
      expirarSesion();
    }
    const mensaje = await leerMensajeDeError(response);
    throw new ApiError(mensaje ?? `La API respondió con ${response.status}.`, response.status);
  }

  return response.status === 204 ? undefined : response.json();
}
