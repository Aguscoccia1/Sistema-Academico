import { request } from '../httpClient';
import { usersService } from '../usersService';

export const teachersService = {
  obtenerDocentes: async () => {
    const usuarios = await usersService.obtenerTodos();

    return usuarios
      .filter((usuario) => usuario.roleName === 'Teacher')
      .map((docente) => ({
        id: docente.id,
        nombreCompleto: `${docente.firstName} ${docente.lastName}`,
        dni: docente.cuil,
        correo: docente.email,
        estado: docente.isActive ? 'Activo' : 'Inactivo',
      }));
  },

  consultarMateriasDocente: (teacherId) =>
    request(`/api/teachers/${teacherId}/subjects`),

  guardarMateriasDocente: (teacherId, assignments) =>
    request(`/api/teachers/${teacherId}/subjects`, {
      method: 'PUT',
      body: JSON.stringify({ assignments }),
    }),
};
