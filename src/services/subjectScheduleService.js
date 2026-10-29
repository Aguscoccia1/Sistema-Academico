import { request } from './httpClient';

export async function obtenerHorariosMateria(subjectId, section) {
  const query = new URLSearchParams({
    materiaId: String(subjectId),
    seccion: section,
  });

  return request(`/api/horarios-materias?${query}`);
}
