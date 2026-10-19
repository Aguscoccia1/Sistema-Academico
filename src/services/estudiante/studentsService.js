import { request } from '../httpClient';

// StudentsController: /api/students (solo Secretario)
export const studentsService = {
  obtenerTodos: () => request('/api/students'),

  obtenerPorId: (id) => request(`/api/students/${id}`),

  crear: (estudiante) =>
    request('/api/students', {
      method: 'POST',
      body: JSON.stringify(estudiante),
    }),

  actualizar: (id, estudiante) =>
    request(`/api/students/${id}`, {
      method: 'PUT',
      body: JSON.stringify(estudiante),
    }),

  eliminar: (id) => request(`/api/students/${id}`, { method: 'DELETE' }),
};
