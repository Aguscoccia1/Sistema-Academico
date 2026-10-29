import { request } from './httpClient';

// EnrollmentController: /api/enrollments (solo Estudiante)
export const enrollmentService = {
  obtenerMateriasDisponibles: () => request('/api/enrollments/available-subjects'),
  obtenerMisInscripciones: () => request('/api/enrollments/me'),
  inscribir: (datos) =>
    request('/api/enrollments', {
      method: 'POST',
      body: JSON.stringify(datos),
    }),
};
