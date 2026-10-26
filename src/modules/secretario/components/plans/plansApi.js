import { studyPlanService } from '../../../../services/studyPlanService';
import { subjectService } from '../../../../services/subjectService';
 
// Nombres de los campos: { nombre en el front: nombre en el backend }
const CAMPOS_PLAN = {
  nombre: 'name',
  codigo: 'planCode',
  añoInicio: 'startYear',
  añoFin: 'endYear',
  modalidad: 'attendanceMode',
  activo: 'isActive',
};
 
const CAMPOS_MATERIA = {
  orden: 'orderNumber',
  formato: 'format',
  materia: 'name',
  horasCatedra: 'lectureHours',
  horasReloj: 'totalHours',
  course: 'course',
};
 
// Régimen en el front -> cuatrimestre (term) en el backend: vacío = anual
const REGIMENES = { 'Anual': null, '1.º cuatrimestre': 1, '2.º cuatrimestre': 2 };
 
// copian un objeto cambiando los nombres de los campos según el mapa
const aFront = (datos, campos) => Object.fromEntries(Object.entries(campos).map(([front, api]) => [front, datos[api]]));
const aApi = (datos, campos) => Object.fromEntries(Object.entries(campos).map(([front, api]) => [api, datos[front]]));
 
// Los campos con otro formato (texto, números) se completan aparte.
const adaptarPlan = (plan) => ({
  id: plan.id,
  ...aFront(plan, CAMPOS_PLAN),
  duracion: plan.careerDurationYears != null ? `${plan.careerDurationYears} años` : null,
  cargaHoraria: plan.totalWorkloadHours != null ? `${plan.totalWorkloadHours} horas reloj` : null,
});
 
const prepararPlan = (plan) => ({
  ...aApi(plan, CAMPOS_PLAN),
  careerDurationYears: Number.parseInt(plan.duracion, 10) || null,
  totalWorkloadHours: Number.parseInt(plan.cargaHoraria, 10) || null,
});
 
// una materia de un plan llega como { courseYear, term, subject: {...} }
const adaptarMateria = (planSubject) => ({
  id: planSubject.id,
  subjectId: planSubject.subjectId,
  ...aFront(planSubject.subject, CAMPOS_MATERIA),
  anio: `${planSubject.courseYear}°`,
  regimen: Object.keys(REGIMENES).find((nombre) => REGIMENES[nombre] === (planSubject.term ?? null)) ?? 'Anual',
});
 
const prepararMateria = (materia) => ({
  ...aApi(materia, CAMPOS_MATERIA),
  orderNumber: Number(materia.orden),
  lectureHours: Number(materia.horasCatedra),
  totalHours: Number(materia.horasReloj),
});
 
// una fila de la grilla = crear la materia + agregarla al plan
const agregarMateria = async (planId, fila) => {
  const materia = await subjectService.crear(prepararMateria(fila));
 
  await studyPlanService.agregarMateria(planId, {
    subjectId: materia.id,
    courseYear: Number.parseInt(fila.anio, 10),
    term: REGIMENES[fila.regimen] ?? null,
  });
};
 
export const plansApi = {
  obtenerPlanes: async () => (await studyPlanService.obtenerTodos()).map(adaptarPlan),
 
  crearPlan: async (plan) => adaptarPlan(await studyPlanService.crear(prepararPlan(plan))),
 
  // el PUT reemplaza todos los campos: siempre hay que enviar el plan completo
  actualizarPlan: async (id, plan) => adaptarPlan(await studyPlanService.actualizar(id, prepararPlan(plan))),
 
  // materias del plan, ordenadas por año y cuatrimestre desde el backend
  obtenerMaterias: async (planId) => {
    const plan = await studyPlanService.obtenerPorId(planId);
    return plan.subjects.map(adaptarMateria);
  },
 
  // agrega las filas de a una y en orden; si una falla, avisa cuántas ya quedaron guardadas
  agregarMaterias: async (planId, filas) => {
    let guardadas = 0;
    try {
      for (const fila of filas) {
        await agregarMateria(planId, fila);
        guardadas += 1;
      }
    } catch (error) {
      if (guardadas === 0) throw error;
      throw new Error(`Se guardaron ${guardadas} de ${filas.length} materias antes del error: ${error.message} Cancelá y revisá la lista antes de reintentar, para no duplicar materias.`);
    }
  },
 
  // solo cambia los datos propios de la materia (nombre, orden, formato, horas)
  actualizarMateria: (materia) => subjectService.actualizar(materia.subjectId, prepararMateria(materia)),
};
