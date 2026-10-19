import { expirarSesion, obtenerToken } from '../core/auth/session';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export class ApiError extends Error {
  constructor(message, status) {
    super(message);
    this.status = status;
  }
}

export async function request(path, options = {}) {
  if (!API_BASE_URL) {
    throw new Error('No hay una API configurada. Definí VITE_API_BASE_URL para usar el backend.');
  }

  const token = obtenerToken();

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
      ...options.headers,
    },
  });

  if (!response.ok) {
    if (response.status === 401 && token && path !== '/api/auth/login') {
      expirarSesion();
    }
    throw new ApiError(`La API respondió con ${response.status}.`, response.status);
  }

  return response.status === 204 ? undefined : response.json();
}