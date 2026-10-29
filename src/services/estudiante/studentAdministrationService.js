import { request } from '../httpClient';

export const studentAdministrationService = {
  consultarEstudiante: (id) => request(`/api/estudiantes/${id}`),

  modificarEstudiante: (id, datos) =>
    request(`/api/estudiantes/${id}`, {
      method: 'PUT',
      body: JSON.stringify({
        nombre: datos.nombre,
        apellido: datos.apellido,
        dni: datos.dni,
      }),
    }),

  reasignarComision: (id, materiaId, seccion) =>
    request(`/api/estudiantes/${id}/comision`, {
      method: 'PUT',
      body: JSON.stringify({
        materiaId: Number(materiaId),
        seccion,
      }),
    }),
};
