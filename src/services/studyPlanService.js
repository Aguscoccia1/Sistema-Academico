import { request } from './httpClient';

// StudyPlanController: /api/StudyPlan (solo Secretario)
export const studyPlanService = {
  obtenerTodos: () => request('/api/StudyPlan'),

  // devuelve el plan con sus materias
  obtenerPorId: (id) => request(`/api/StudyPlan/${id}`),

  crear: (plan) =>
    request('/api/StudyPlan', {
      method: 'POST',
      body: JSON.stringify(plan),
    }),

  // el PUT reemplaza todos los campos: siempre hay que enviar el plan completo
  actualizar: (id, plan) =>
    request(`/api/StudyPlan/${id}`, {
      method: 'PUT',
      body: JSON.stringify(plan),
    }),

  // materiaDelPlan: { subjectId, courseYear, term }
  agregarMateria: (planId, materiaDelPlan) =>
    request(`/api/StudyPlan/${planId}/subjects`, {
      method: 'POST',
      body: JSON.stringify(materiaDelPlan),
    }),
};
