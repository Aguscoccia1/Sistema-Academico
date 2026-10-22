import { createContext, useContext, useEffect, useState } from 'react';
import { enrollmentService } from '../../../../../services/enrollmentService';
import { prerequisiteValidationService } from '../../../../../services/prerequisiteValidationService';

// el período de inscripción y las materias van a venir de la API
export const PERIODO_TEXTO = 'Sin definir';

const InscripcionesContext = createContext(null);

export function InscripcionesProvider({ children }) {
  const [materiasPreparadas, setMateriasPreparadas] = useState([]);
  const [errorCarga, setErrorCarga] = useState('');

  useEffect(() => {
    let cancelado = false;

    async function cargarMaterias() {
      try {
        setErrorCarga('');
        const respuesta = await enrollmentService.obtenerMateriasDisponibles();
        const materias = await Promise.all(
          (respuesta.subjects ?? []).map(async (materia) => {
            const validacion = await prerequisiteValidationService.validar({
              studyPlanId: materia.studyPlanId,
              subjectId: materia.id,
            });
            const correlativasPendientes = (validacion.missingPrerequisites ?? [])
              .map((correlativa) => correlativa.name);

            return {
              id: materia.id,
              studyPlanId: materia.studyPlanId,
              section: materia.section,
              nombre: materia.name,
              año: materia.courseYear,
              regimen: materia.term ?? '',
              correlativasPendientes,
              correlativaBloqueante: correlativasPendientes[0],
            };
          })
        );

        if (!cancelado) setMateriasPreparadas(materias);
      } catch (error) {
        console.error('No se pudieron cargar las materias de la API.', error);
        if (!cancelado) {
          setMateriasPreparadas([]);
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
    if (!materia.section) {
      throw new Error('El servidor todavía no informa la comisión asignada. No se realizó la inscripción.');
    }

    await enrollmentService.inscribir({
      studyPlanId: materia.studyPlanId,
      subjectId: materia.id,
      section: materia.section,
    });

    setMateriasPreparadas((actuales) =>
      actuales.map((item) => item.id === materiaId ? { ...item, estado: 'R' } : item)
    );
  };

  return (
    <InscripcionesContext.Provider value={{ materiasPreparadas, inscribir, errorCarga }}>
      {children}
    </InscripcionesContext.Provider>
  );
}

export function useInscripciones() {
  const contexto = useContext(InscripcionesContext);
  if (!contexto) throw new Error('useInscripciones debe usarse dentro de InscripcionesProvider');
  return contexto;
}
