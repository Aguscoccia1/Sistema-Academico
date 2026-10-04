import { createMockRepository } from '../../../../api/createMockRepository';

const initialStudents = [
  {
    id: 1,
    nombreCompleto: 'Juan Pérez',
    dni: '41.123.456',
    correo: 'juan.perez@gmail.com',
    estado: 'Pendiente',
  },

  {
    id: 2,
    nombreCompleto: 'María García',
    dni: '42.234.567',
    correo: 'maria.garcia@gmail.com',
    estado: 'Completa',
  },

  {
    id: 3,
    nombreCompleto: 'Carlos López',
    dni: '43.345.678',
    correo: 'carlos.lopez@gmail.com',
    estado: 'Completa',
  },

  {
    id: 4,
    nombreCompleto: 'Ana Martínez',
    dni: '44.456.789',
    correo: 'ana.martinez@gmail.com',
    estado: 'Pendiente',
  },
];

const mockRepository =
  createMockRepository(initialStudents);

export const studentsApi = {

  // Obtener todos los estudiantes
  obtenerEstudiantes: async () => {
    return mockRepository.list();
  },

  // Modificar estudiante
  actualizarEstudiante: async (id, estudiante) => {
    return mockRepository.update(
      id,
      estudiante
    );
  },

  // Eliminar estudiante
  eliminarEstudiante: async (id) => {
    return mockRepository.remove(id);
  },


};