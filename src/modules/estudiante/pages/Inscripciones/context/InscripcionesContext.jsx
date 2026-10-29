import { createContext, useContext, useEffect, useState } from 'react';
import { enrollmentService } from '../../../../../services/enrollmentService';
import { prerequisiteValidationService } from '../../../../../services/prerequisiteValidationService';

// el período de inscripción y las materias van a venir de la API
export const PERIODO_TEXTO = 'Sin definir';

const InscripcionesContext = createContext(null);

const obtenerRegimen = (term) => {
  if (term === 1) return '1.º cuatrimestre';
  if (term === 2) return '2.º cuatrimestre';
  return 'Anual';
};

const normalizarEstadoFinal = (finalStatus) =>
  typeof finalStatus === 'string' ? finalStatus.trim().toLowerCase() : '';

export function InscripcionesProvider({ children }) {
  const [materiasPreparadas, setMateriasPreparadas] = useState([]);
  const [misInscripciones, setMisInscripciones] = useState([]);
  const [errorCarga, setErrorCarga] = useState('');

  useEffect(() => {
    let cancelado = false;

    async function cargarMaterias() {
      try {
        setErrorCarga('');
        const [respuesta, inscripciones] = await Promise.all([
          enrollmentService.obtenerMateriasDisponibles(),
          enrollmentService.obtenerMisInscripciones(),
        ]);

        const materiasDeLaApi = respuesta.subjects ?? [];
        if (materiasDeLaApi.length === 0 && respuesta.message) {
          setErrorCarga(respuesta.message);
        }

        const estadosPorMateria = new Map();
        const materiasDesaprobadas = new Set();
        const inscripcionesActivas = [];

        inscripciones.forEach((inscripcion) => {
          const estadoFinal = normalizarEstadoFinal(inscripcion.finalStatus);

          if (!estadoFinal) {
            estadosPorMateria.set(inscripcion.subjectId, 'R');
            inscripcionesActivas.push(inscripcion);
          } else if (estadoFinal === 'aprobado') {
            estadosPorMateria.set(inscripcion.subjectId, 'A');
          } else {
            // TODO: las desaprobadas quedan ocultas hasta que Soria y el PO definan cómo mostrarlas.
            materiasDesaprobadas.add(inscripcion.subjectId);
          }
        });

        const materiasDisponibles = materiasDeLaApi
          .filter((materia) => !materiasDesaprobadas.has(materia.id));
        const materiasParaValidar = materiasDisponibles
          .filter((materia) => !estadosPorMateria.has(materia.id));
        const resultadosValidacion = await Promise.allSettled(
          materiasParaValidar.map((materia) =>
            prerequisiteValidationService.validar({
              studyPlanId: materia.studyPlanId,
              subjectId: materia.id,
            })
          )
        );
        const validacionesPorMateria = new Map(
          materiasParaValidar.map((materia, index) => [materia.id, resultadosValidacion[index]])
        );

        const materias = materiasDisponibles.map((materia) => {
          const estado = estadosPorMateria.get(materia.id);
          const resultadoValidacion = estado ? null : validacionesPorMateria.get(materia.id);
          let correlativasPendientes = [];
          let validacionFallida = false;

          if (resultadoValidacion?.status === 'fulfilled') {
            const validacion = resultadoValidacion.value;
            correlativasPendientes = (validacion?.missingPrerequisites ?? [])
              .map((correlativa) => correlativa.name);
            validacionFallida = validacion?.canEnroll === false && correlativasPendientes.length === 0;
          } else if (resultadoValidacion?.status === 'rejected') {
            validacionFallida = true;
            console.error(
              `No se pudieron validar las correlativas de la materia ${materia.id}.`,
              resultadoValidacion.reason
            );
          }

          if (validacionFallida) {
            correlativasPendientes = ['No se pudo verificar las correlatividades'];
          }

          return {
            id: materia.id,
            studyPlanId: materia.studyPlanId,
            nombre: materia.name,
            año: materia.courseYear,
            regimen: obtenerRegimen(materia.term),
            correlativasPendientes,
            correlativaBloqueante: correlativasPendientes[0],
            estado,
            validacionFallida,
          };
        });

        const materiasPorId = new Map(materias.map((materia) => [materia.id, materia]));
        const inscripcionesPreparadas = inscripcionesActivas.map((inscripcion) => {
          const materiaDisponible = materiasPorId.get(inscripcion.subjectId);

          return {
            id: inscripcion.subjectId,
            enrollmentId: inscripcion.enrollmentId,
            nombre: inscripcion.subjectName,
            año: inscripcion.courseYear ?? materiaDisponible?.año ?? '',
            regimen: inscripcion.courseYear == null
              ? materiaDisponible?.regimen ?? ''
              : obtenerRegimen(inscripcion.term),
            estado: 'R',
          };
        });

        if (!cancelado) {
          setMateriasPreparadas(materias);
          setMisInscripciones(inscripcionesPreparadas);
        }
      } catch (error) {
        console.error('No se pudieron cargar las materias de la API.', error);
        if (!cancelado) {
          setMateriasPreparadas([]);
          setMisInscripciones([]);
          setErrorCarga(error.message || 'No se pudieron cargar las materias disponibles.');
        }
      }
    }

    cargarMaterias();
    return () => {
      cancelado = true;
    };
  }, []);

  const inscribir = async (materiaId) => {
    const materia = materiasPreparadas.find((item) => item.id === materiaId);
    if (!materia) throw new Error('No se encontró la materia seleccionada.');
    if (materia.validacionFallida) {
      throw new Error('No se pudieron validar las correlativas. Intentá nuevamente más tarde.');
    }

    const inscripcionCreada = await enrollmentService.inscribir({
      studyPlanId: materia.studyPlanId,
      subjectId: materia.id,
    });

    setMateriasPreparadas((actuales) =>
      actuales.map((item) => item.id === materiaId ? { ...item, estado: 'R' } : item)
    );
    setMisInscripciones((actuales) => [
      ...actuales,
      {
        id: materia.id,
        enrollmentId: inscripcionCreada.id,
        nombre: materia.nombre,
        año: materia.año,
        regimen: materia.regimen,
        estado: 'R',
        finalStatus: null,
      },
    ]);
  };

  return (
    <InscripcionesContext.Provider value={{ materiasPreparadas, misInscripciones, inscribir, errorCarga }}>
      {children}
    </InscripcionesContext.Provider>
  );
}

export function useInscripciones() {
  const contexto = useContext(InscripcionesContext);
  if (!contexto) throw new Error('useInscripciones debe usarse dentro de InscripcionesProvider');
  return contexto;
}
