import { request } from './httpClient';

// RolesController: /api/roles (solo Secretario). Los roles son fijos: solo se pueden listar.
export const rolesService = {
  obtenerTodos: () => request('/api/roles'),
};
