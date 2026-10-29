import { useCallback } from 'react';
import { useAsyncResource } from '../../../../hooks/useAsyncResource';
import { registrationService } from '../../../../services/registrationService';
import { studyPlanService } from '../../../../services/studyPlanService';

export function useEnrollments(planId) {
  const loadEnrollments = useCallback(async () => {
    const plans = await studyPlanService.obtenerTodos();
    const selectedPlans = planId
      ? plans.filter((plan) => String(plan.id) === planId)
      : plans;
    const detailedPlans = await Promise.all(
      selectedPlans.map((plan) => studyPlanService.obtenerPorId(plan.id)),
    );
    const students = await registrationService.obtenerEstudiantes();
    const enrollmentsByStudent = await Promise.all(
      students.map(async (student) => ({
        student,
        enrollments: await registrationService.consultarInscripcionesEstudiante(
          student.id
        ),
      })),
    );

    const subjectsByYear = new Map();
    detailedPlans.forEach((plan) => {
      plan.subjects.forEach((subject) => {
        const subjectIds = subjectsByYear.get(subject.courseYear) ?? new Set();
        subjectIds.add(subject.subjectId);
        subjectsByYear.set(subject.courseYear, subjectIds);
      });
    });

    const years = [...subjectsByYear.keys()].sort(
      (first, second) => first - second,
    );
    const summaries = years.map((year) => {
      const subjectIds = subjectsByYear.get(year);
      const studentsInYear = new Set();
      const studentsWithoutSection = new Set();

      enrollmentsByStudent.forEach(({ student, enrollments }) => {
        const yearEnrollments = enrollments.filter((enrollment) =>
          subjectIds.has(enrollment.materiaId),
        );

        if (yearEnrollments.length > 0) {
          studentsInYear.add(student.id);
          if (yearEnrollments.some((enrollment) => !enrollment.seccion)) {
            studentsWithoutSection.add(student.id);
          }
        }
      });

      return {
        añoAcademico: year,
        totalEstudiantes: studentsInYear.size,
        estudiantesSinAsignar: studentsWithoutSection.size,
      };
    });

    return {
      years: years.map((year) => ({
        id: year,
        etiqueta: `${year}°`,
      })),
      plans: plans.map(({ id, name }) => ({ id, nombre: name })),
      summaries,
    };
  }, [planId]);

  const { data } = useAsyncResource(loadEnrollments, [loadEnrollments]);

  return {
    years: data.years ?? [],
    plans: data.plans ?? [],
    summaries: data.summaries ?? [],
  };
}
