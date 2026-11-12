import { request } from './httpClient';
import { obtenerHorariosMateria } from './subjectScheduleService';

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
    const materiasCandidatas = [
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
    const errores = [];
    const resultadosHorarios = await Promise.all(
      materiasCandidatas.map(async (materia) => {
          try {
            const [horariosA, horariosB] = await Promise.all([
              obtenerHorariosMateria(materia.id, 'A'),
              obtenerHorariosMateria(materia.id, 'B'),
            ]);

            if (horariosA.length > 0 && horariosB.length > 0) {
              return materia;
            }

            if (horariosA.length > 0 || horariosB.length > 0) {
              errores.push(
                `${materia.nombre}: faltan horarios configurados para las comisiones A y B.`
              );
            }

            return null;
          } catch (error) {
            errores.push(`${materia.nombre}: ${error.message}`);
            return null;
          }
      })
    );
    const materias = resultadosHorarios.filter(Boolean);

    if (materias.length === 0) {
      return {
          mensaje: 'No hay materias con horarios configurados para A y B.',
          errores,
      };
    }

    const resultado = await request(
      '/api/comisiones/asignacion-aleatoria-masiva',
      {
        method: 'POST',
        body: JSON.stringify({
          materiaIds: materias.map((materia) => Number(materia.id)),
        }),
      }
    );

    return {
      mensaje: resultado.mensaje,
      errores,
    };
  },

  buscarEstudianteEnComision: (seccion, estudianteId, materiaId) =>
    request(
      `/api/comisiones/${encodeURIComponent(seccion)}/estudiantes/${estudianteId}?materiaId=${encodeURIComponent(materiaId)}`
    ),
};
