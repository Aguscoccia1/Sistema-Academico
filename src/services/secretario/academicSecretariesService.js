import { request } from '../httpClient';

// AcademicSecretariesController: /api/academic-secretaries (solo Secretario)
export const academicSecretariesService = {
  obtenerTodos: () => request('/api/academic-secretaries'),

  obtenerPorId: (id) => request(`/api/academic-secretaries/${id}`),

  crear: (secretario) =>
    request('/api/academic-secretaries', {
      method: 'POST',
      body: JSON.stringify(secretario),
    }),

  actualizar: (id, secretario) =>
    request(`/api/academic-secretaries/${id}`, {
      method: 'PUT',
      body: JSON.stringify(secretario),
    }),

  eliminar: (id) => request(`/api/academic-secretaries/${id}`, { method: 'DELETE' }),
};
