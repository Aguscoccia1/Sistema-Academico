import { request } from './httpClient';

// SubjectController: /api/Subject (solo Secretario)
export const subjectService = {
  obtenerTodos: () => request('/api/Subject'),

  obtenerPorId: (id) => request(`/api/Subject/${id}`),

  crear: (materia) =>
    request('/api/Subject', {
      method: 'POST',
      body: JSON.stringify(materia),
    }),

  actualizar: (id, materia) =>
    request(`/api/Subject/${id}`, {
      method: 'PUT',
      body: JSON.stringify(materia),
    }),
};
