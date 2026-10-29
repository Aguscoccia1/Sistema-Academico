import { request } from './httpClient';

export const registrationService = {
  obtenerEstudiantes: async () => {
    const estudiantes = await request('/api/inscripciones/estudiantes');

    return estudiantes.map((estudiante) => ({
      id: estudiante.id,
      nombre: estudiante.nombre,
      apellido: estudiante.apellido,
      nombreCompleto: `${estudiante.nombre} ${estudiante.apellido}`,
      dni: estudiante.dni,
      legajo: estudiante.legajo,
      correo: estudiante.email,
    }));
  },

  consultarInscripcionesEstudiante: (id) =>
    request(`/api/inscripciones/estudiante/${id}`),

  consultarCantidadInscriptos: async () => {
    const resultado = await request('/api/inscripciones/cantidad');
    return resultado.totalInscriptos;
  },

  inscribirEstudiante: (estudianteId, materiaId, seccion) =>
    request('/api/inscripciones', {
      method: 'POST',
      body: JSON.stringify({
        estudianteId: Number(estudianteId),
        materiaId: Number(materiaId),
        seccion,
      }),
    }),

  consultarMaterias: (anio = 1) =>
    request(`/api/inscripciones/materias?anio=${encodeURIComponent(anio)}`),
};
