import { request } from './httpClient';

// HealthController: /api/health (público)
export const healthService = {
  obtenerEstado: () => request('/api/health'),
};
