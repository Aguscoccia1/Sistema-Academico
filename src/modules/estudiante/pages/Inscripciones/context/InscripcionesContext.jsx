import { createContext, useContext, useState } from 'react';

// el período de inscripción y las materias van a venir de la API
export const PERIODO_TEXTO = 'Sin definir';

const MATERIAS_INICIALES = [];

const InscripcionesContext = createContext(null);

export function InscripcionesProvider({ children }) {
  const [materias, setMaterias] = useState(MATERIAS_INICIALES);

  const materiasPreparadas = materias.map((materia) => {
    const correlativasPendientes = materia.correlativas
      .map((correlativaId) => materias.find(({ id }) => id === correlativaId))
      .filter((correlativa) => correlativa && correlativa.estado !== 'A')
      .map((correlativa) => correlativa.nombre);

    return {
      ...materia,
      correlativasPendientes,
      correlativaBloqueante: correlativasPendientes[0],
    };
  });

  const inscribir = (materiaId) => {
    setMaterias((actuales) =>
      actuales.map((materia) => (materia.id === materiaId ? { ...materia, estado: 'R' } : materia))
    );
  };

  return (
    <InscripcionesContext.Provider value={{ materiasPreparadas, inscribir }}>
      {children}
    </InscripcionesContext.Provider>
  );
}

export function useInscripciones() {
  const contexto = useContext(InscripcionesContext);
  if (!contexto) throw new Error('useInscripciones debe usarse dentro de InscripcionesProvider');
  return contexto;
}