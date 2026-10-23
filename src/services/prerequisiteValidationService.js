import { request } from './httpClient';

// PrerequisiteValidationController: /api/prerequisites
export const prerequisiteValidationService = {
  // datos: { studentId, studyPlanId, subjectId }
  validar: (datos) =>
    request('/api/prerequisites/validate', {
      method: 'POST',
      body: JSON.stringify(datos),
    }),
};
