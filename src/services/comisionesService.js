import { request } from './httpClient';

async function consultarComisiones(materiaId) {
  const query = materiaId
    ? `?materiaId=${encodeURIComponent(materiaId)}`
    : '';

  return request(`/api/comisiones${query}`);
}

export const comisionesService = {
  consultarComisiones,

  asignarComisionesAleatoriamente: async () => {
    const comisiones = await consultarComisiones();
    const materias = [
      ...new Map(
        comisiones
          .filter((comision) =>
            ['A', 'B'].includes(comision.seccion?.trim().toUpperCase())
          )
          .map((comision) => [
            comision.materiaId,
            {
              id: comision.materiaId,
              nombre: comision.materia,
            },
          ])
      ).values(),
    ];

    if (materias.length === 0) {
      return {
        mensaje: 'No hay materias con comisiones configuradas para asignar.',
        errores: [],
      };
    }

    const errores = [];
    let totalAsignado = 0;
    let materiasProcesadas = 0;

    for (const materia of materias) {
      try {
        const resultado = await request(
          '/api/comisiones/asignacion-aleatoria-masiva',
          {
            method: 'POST',
            body: JSON.stringify({ materiaId: Number(materia.id) }),
          }
        );

        materiasProcesadas++;
        totalAsignado += resultado.totalAsignado;
      } catch (error) {
        console.error(
          `No se pudieron asignar comisiones para ${materia.nombre}:`,
          error
        );
        errores.push(`${materia.nombre}: ${error.message}`);
      }
    }

    const mensaje = totalAsignado > 0
      ? `Se asignaron ${totalAsignado} inscripciones en ${materiasProcesadas} materias.`
      : 'Todos los alumnos ya tienen una comisión asignada.';

    return {
      mensaje: errores.length > 0
        ? `${mensaje} Hubo problemas en ${errores.length} materias.`
        : mensaje,
      errores,
    };
  },

  buscarEstudianteEnComision: (seccion, estudianteId, materiaId) =>
    request(
      `/api/comisiones/${encodeURIComponent(seccion)}/estudiantes/${estudianteId}?materiaId=${encodeURIComponent(materiaId)}`
    ),
};
