import { request } from './httpClient';

// PrerequisiteValidationController: /api/prerequisites
export const prerequisiteValidationService = {
  // El backend identifica al estudiante mediante el token.
  // datos: { studyPlanId, subjectId }
  validar: (datos) =>
    request('/api/prerequisites/validate', {
      method: 'POST',
      body: JSON.stringify(datos),
    }),
};
