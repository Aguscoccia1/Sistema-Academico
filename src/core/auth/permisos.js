
const MENU_POR_ROL = {
  secretario: [
    { label: 'Inicio', path: '/secretario/inicio', icon: 'Grid2X2', habilitado: true },
    { label: 'Planes de Estudio', path: '/secretario/planes-de-estudio', icon: 'GraduationCap', habilitado: true },
    { label: 'Inscripciones', path: '/secretario/inscripciones', icon: 'ClipboardList', habilitado: true },
    { label: 'Docentes', path: '/secretario/docentes', icon: 'UserRound', habilitado: true },
    { label: 'Materias', path: '/secretario/materias', icon: 'BookOpen', habilitado: false },
    { label: 'Usuarios', path: '/secretario/usuarios', icon: 'Users', habilitado: false },
    { label: 'Auditoría', path: '/secretario/auditoria', icon: 'ShieldCheck', habilitado: false },
  ],
  estudiante: [
    { label: 'Inicio', path: '/estudiante/inicio', icon: 'Grid2X2', habilitado: true },
    { label: 'Inscripción a Materias', path: '/estudiante/inscripciones', icon: 'ClipboardList', habilitado: true },
    { label: 'Mis Inscripciones', path: '/estudiante/mis-inscripciones', icon: 'ClipboardList', habilitado: true },
  ],
};

export function menuParaRol(rol) {
  return MENU_POR_ROL[rol] ?? [];
}