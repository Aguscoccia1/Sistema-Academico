import { request } from '../../../../api/httpClient';
 
// Régimen de una materia en el front <-> cuatrimestre (term) en el backend: vacío = anual
const REGIMEN_A_TERM = { 'Anual': null, '1.º cuatrimestre': 1, '2.º cuatrimestre': 2 };
 
const termARegimen = (term) => {
  if (term === 1) return '1.º cuatrimestre';
  if (term === 2) return '2.º cuatrimestre';
  return 'Anual';
};
 
// plan: backend -> frontend
const adaptarPlan = (plan) => ({
  id: plan.id,
  nombre: plan.name,
  codigo: plan.planCode,
  añoInicio: plan.startYear,
  añoFin: plan.endYear,
  duracion: plan.careerDurationYears != null ? `${plan.careerDurationYears} años` : null,
  modalidad: plan.attendanceMode,
  cargaHoraria: plan.totalWorkloadHours != null ? `${plan.totalWorkloadHours} horas reloj` : null,
  activo: plan.isActive,
});
 
// plan: frontend -> backend
const prepararPlan = (plan) => ({
  name: plan.nombre,
  planCode: plan.codigo,
  careerDurationYears: Number.parseInt(plan.duracion, 10) || null,
  attendanceMode: plan.modalidad,
  totalWorkloadHours: Number.parseInt(plan.cargaHoraria, 10) || null,
  startYear: Number(plan.añoInicio),
  endYear: plan.añoFin ? Number(plan.añoFin) : null,
  isActive: Boolean(plan.activo),
});
 
// materia de un plan: backend -> frontend
const adaptarMateria = (planSubject) => ({
  id: planSubject.id,
  orden: planSubject.subject.orderNumber,
  formato: planSubject.subject.format,
  materia: planSubject.subject.name,
  anio: `${planSubject.courseYear}°`,
  regimen: termARegimen(planSubject.term),
  horasCatedra: planSubject.subject.lectureHours,
  horasReloj: planSubject.subject.totalHours,
});
 
export const plansApi = {
  obtenerPlanes: async () => {
    const planes = await request('/api/StudyPlan');
    return planes.map(adaptarPlan);
  },
 
  // devuelve el plan con sus materias (ordenadas por año y cuatrimestre desde el backend)
  obtenerPlanPorId: async (id) => {
    const plan = await request(`/api/StudyPlan/${id}`);
    return { ...adaptarPlan(plan), materias: plan.subjects.map(adaptarMateria) };
  },
 
  crearPlan: async (plan) => {
    const nuevoPlan = await request('/api/StudyPlan', {
      method: 'POST',
      body: JSON.stringify(prepararPlan(plan)),
    });
    return adaptarPlan(nuevoPlan);
  },
 
  // el PUT reemplaza todos los campos: siempre hay que enviar el plan completo
  actualizarPlan: async (id, plan) => {
    const planActualizado = await request(`/api/StudyPlan/${id}`, {
      method: 'PUT',
      body: JSON.stringify(prepararPlan(plan)),
    });
    return adaptarPlan(planActualizado);
  },
 
  // el backend todavía no tiene DELETE; ManagementPage.jsx (Docentes) usa esta función
  eliminarPlan: async () => {
    throw new Error('Eliminar planes todavía no está disponible.');
  },
 
  // una fila de la grilla = crear la materia + agregarla al plan
  agregarMateria: async (planId, fila) => {
    const materia = await request('/api/Subject', {
      method: 'POST',
      body: JSON.stringify({
        name: fila.materia,
        orderNumber: Number(fila.orden),
        format: fila.formato,
        lectureHours: Number(fila.horasCatedra),
        totalHours: Number(fila.horasReloj),
      }),
    });
 
    await request(`/api/StudyPlan/${planId}/subjects`, {
      method: 'POST',
      body: JSON.stringify({
        subjectId: materia.id,
        courseYear: Number.parseInt(fila.anio, 10),
        term: REGIMEN_A_TERM[fila.regimen],
      }),
    });
  },
};
