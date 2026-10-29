import { request } from '../httpClient';

export async function obtenerDocentes() {
    const usuarios = await request('/api/users');

    return usuarios
        .filter((usuario) => usuario.roleName === 'Teacher')
        .map((docente) => ({
            id: docente.id,
            nombreCompleto: `${docente.firstName} ${docente.lastName}`,
            dni: docente.cuil,
            correo: docente.email,
            estado: docente.isActive ? 'Activo' : 'Inactivo',
        }));
}

export async function consultarMateriasDocente(teacherId) {
    return request(`/api/teachers/${teacherId}/subjects`);
}

export async function guardarMateriasDocente(teacherId, assignments) {
    return request(`/api/teachers/${teacherId}/subjects`, {
        method: 'PUT',
        body: JSON.stringify({ assignments }),
    });
}
