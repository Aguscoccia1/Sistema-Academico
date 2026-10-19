import { request } from './httpClient';

// PermissionsController: /api/auth/permissions (cualquier usuario con sesión)
export const permissionsService = {
  obtenerMisPermisos: () => request('/api/auth/permissions'),
};
