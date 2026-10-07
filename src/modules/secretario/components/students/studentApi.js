import { createMockRepository } from '../../../../api/createMockRepository';

const initialStudents = [
  {
    id: 1,
    nombreCompleto: 'Juan Pérez',
    dni: '41.123.456',
    correo: 'juan.perez@gmail.com',

    estado: 'Pendiente',

    comision: '',
    
    materias: {},
  },

  {
    id: 2,
    nombreCompleto: 'María García',
    dni: '42.234.567',
    correo: 'maria.garcia@gmail.com',

    estado: 'Completa',

    comision: 'A',

    materias: {
      programacionI: 'A',
      sistemasOrganizaciones: 'A',
      competenciasComunicacionalesI: 'A',
      baseDeDatos: 'A',
      aproximacionesMundoTrabajo: 'A',
      ingles: 'A',
      elementosMatematicaLogica: 'A',
      arquitecturaComputadoras: 'A',
      competenciasComunicacionalesII: 'A',
      eticaDeontologiaProfesional: 'A',
    },
  },

  {
    id: 3,
    nombreCompleto: 'Carlos López',
    dni: '43.345.678',
    correo: 'carlos.lopez@gmail.com',

    estado: 'Completa',

    comision: 'B',

    materias: {
      programacionI: 'B',
      sistemasOrganizaciones: 'B',
      competenciasComunicacionalesI: 'B',
      baseDeDatos: 'B',
      aproximacionesMundoTrabajo: 'B',
      ingles: 'B',
      elementosMatematicaLogica: 'B',
      arquitecturaComputadoras: 'B',
      competenciasComunicacionalesII: 'B',
      eticaDeontologiaProfesional: 'B',
    },
  },

  {
    id: 4,
    nombreCompleto: 'Ana Martínez',
    dni: '44.456.789',
    correo: 'ana.martinez@gmail.com',

    estado: 'Pendiente',

    comision: '',

    materias: {},
  },
];

const mockRepository = createMockRepository(initialStudents);

export const studentsApi = {
  obtenerEstudiantes: async () => {
    return mockRepository.list();
  },

  actualizarEstudiante: async (id, estudiante) => {
    return mockRepository.update(id, estudiante);
  },

  eliminarEstudiante: async (id) => {
    return mockRepository.remove(id);
  },
};