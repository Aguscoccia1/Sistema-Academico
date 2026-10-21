import { request } from './httpClient';

// UsersController: /api/users (solo Secretario)
export const usersService = {
  obtenerTodos: () => request('/api/users'),

  obtenerPorId: (id) => request(`/api/users/${id}`),

  crear: (usuario) =>
    request('/api/users', {
      method: 'POST',
      body: JSON.stringify(usuario),
    }),

  actualizar: (id, usuario) =>
    request(`/api/users/${id}`, {
      method: 'PUT',
      body: JSON.stringify(usuario),
    }),

  eliminar: (id) => request(`/api/users/${id}`, { method: 'DELETE' }),
};
