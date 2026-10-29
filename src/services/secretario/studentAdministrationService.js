
import { request } from '../httpClient';

// 1. Consultar estudiantes.
// El controller delega en InscripcionesService.
export async function obtenerEstudiantes() {
    const estudiantes = await request('/api/inscripciones/estudiantes');

    return estudiantes.map((estudiante) => ({
        id: estudiante.id,
        nombre: estudiante.nombre,
        apellido: estudiante.apellido,
        nombreCompleto: `${estudiante.nombre} ${estudiante.apellido}`,
        dni: estudiante.dni,
        legajo: estudiante.legajo,
        correo: estudiante.email,
    }));
}

// 2. Consultar un estudiante por su ID.
// Controller: StudentController.
// Business: StudentService.
export async function consultarEstudiante(id) {
    return request(`/api/estudiantes/${id}`);
}

// 3. Modificar los datos personales.
// El DTO espera Nombre, Apellido y Dni.
export async function modificarEstudiante(id, datos) {
    return request(`/api/estudiantes/${id}`, {
        method: 'PUT',
        body: JSON.stringify({
            nombre: datos.nombre,
            apellido: datos.apellido,
            dni: datos.dni,
        }),
    });
}

// 4. Reasignar comisión de una materia.
// El backend recibe MateriaId y Seccion.
export async function reasignarComision(id, materiaId, seccion) {
    return request(`/api/estudiantes/${id}/comision`, {
        method: 'PUT',
        body: JSON.stringify({
            materiaId: Number(materiaId),
            seccion,
        }),
    });
}

// 5. Consultar comisiones.
// Se puede filtrar por materia.
export async function consultarComisiones(materiaId) {
    const query = materiaId
        ? `?materiaId=${encodeURIComponent(materiaId)}`
        : '';

    return request(`/api/comisiones${query}`);
}

export async function asignarComisionesAleatoriamente() {
    const comisiones = await consultarComisiones();
    const materias = [
        ...new Map(
            comisiones
                .filter((comision) =>
                    ['A', 'B'].includes(comision.seccion?.trim().toUpperCase())
                )
                .map((comision) => [
                    comision.materiaId,
                    {
                        id: comision.materiaId,
                        nombre: comision.materia,
                    },
                ])
        ).values(),
    ];

    if (materias.length === 0) {
        return {
            mensaje: 'No hay materias con comisiones configuradas para asignar.',
            errores: [],
        };
    }

    const errores = [];
    let totalAsignado = 0;
    let materiasProcesadas = 0;

    for (const materia of materias) {
        try {
            const resultado = await request(
                '/api/comisiones/asignacion-aleatoria-masiva',
                {
                    method: 'POST',
                    body: JSON.stringify({
                        materiaId: Number(materia.id),
                    }),
                }
            );

            materiasProcesadas++;
            totalAsignado += resultado.totalAsignado;
        } catch (error) {
            console.error(
                `No se pudieron asignar comisiones para ${materia.nombre}:`,
                error
            );
            errores.push(`${materia.nombre}: ${error.message}`);
        }
    }

    const mensaje = totalAsignado > 0
        ? `Se asignaron ${totalAsignado} inscripciones en ${materiasProcesadas} materias.`
        : 'Todos los alumnos ya tienen una comisión asignada.';

    return {
        mensaje: errores.length > 0
            ? `${mensaje} Hubo problemas en ${errores.length} materias.`
            : mensaje,
        errores,
    };
}

// 6. Buscar un estudiante dentro de una comisión.
export async function buscarEstudianteEnComision(
    seccion,
    estudianteId,
    materiaId
) {
    return request(
        `/api/comisiones/${encodeURIComponent(seccion)}/estudiantes/${estudianteId}?materiaId=${encodeURIComponent(materiaId)}`
    );
}

// Cantidad total de alumnos inscriptos.
// El backend responde: { totalInscriptos: número }.
export async function consultarCantidadInscriptos() {
    const resultado = await request('/api/inscripciones/cantidad');

    return resultado.totalInscriptos;
}

// Consultar las inscripciones de un estudiante.
// Sirve para conocer sus materias y comisiones actuales.
export async function consultarInscripcionesEstudiante(id) {
    return request(`/api/inscripciones/estudiante/${id}`);
}

export async function inscribirEstudiante(estudianteId, materiaId, seccion) {
    return request('/api/inscripciones', {
        method: 'POST',
        body: JSON.stringify({
            estudianteId: Number(estudianteId),
            materiaId: Number(materiaId),
            seccion,
        }),
    });
}

// Consultar materias disponibles del primer año.
export async function consultarMaterias() {
    return request('/api/inscripciones/materias?anio=1');
}
