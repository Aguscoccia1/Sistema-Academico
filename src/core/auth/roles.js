const ROL_FRONT = {
  Student: 'estudiante',
  AcademicSecretary: 'secretario',
};

export function rolFront(rolBackend) {
  return ROL_FRONT[rolBackend] ?? null;
}
export function rutaInicio(rol) {
  if (rol === 'secretario') return '/secretario/inicio';
  if (rol === 'estudiante') return '/estudiante/inicio';
  return '/login';
}