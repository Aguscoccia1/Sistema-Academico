import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import {
    consultarEstudiante,
    modificarEstudiante,
    consultarInscripcionesEstudiante,
    reasignarComision,
    inscribirEstudiante,
} from '../../../../services/secretario/studentAdministrationService';
import { subjectService } from '../../../../services/subjectService';

import "../../../../styles/modificarEst.css";
import FileText from '../../../../assets/FileText.svg';
import SubjectScheduleList from '../../components/students/SubjectScheduleList';

const COMISIONES = ['A', 'B'];

const ModificarEstudiante = () => {
    const { id } = useParams();
    const navigate = useNavigate();

    const [editingStudent, setEditingStudent] = useState(null);

    const [isLoading, setIsLoading] = useState(true);
    const [notice, setNotice] = useState('');
    const [error, setError] = useState('');
    const [comisionGeneral, setComisionGeneral] = useState('');

    useEffect(() => {

        const cargarEstudiante = async () => {
            try {
                setIsLoading(true);
                setError('');

                const [datos, inscripciones, materias] = await Promise.all([
                    consultarEstudiante(id),
                    consultarInscripcionesEstudiante(id),
                    subjectService.obtenerTodos(),
                ]);

                const materiasDisponibles = materias.map((materia) => {
                    const inscripcion = inscripciones.find(
                        (item) => item.materiaId === materia.id
                    ) ?? null;

                    return {
                        ...materia,
                        comisionesDisponibles: COMISIONES,
                        inscripcion,
                        seccionSeleccionada: inscripcion?.seccion ?? '',
                    };
                });

                setEditingStudent({
                    id: datos.id,
                    nombre: datos.nombre,
                    apellido: datos.apellido,
                    nombreCompleto: `${datos.nombre} ${datos.apellido}`,
                    dni: datos.dni,
                    legajo: datos.legajo,
                    correo: datos.email,
                    inscripciones,
                    materiasDisponibles,
                });
                const seccionesIniciales = [
                    ...new Set(inscripciones.map((item) => item.seccion).filter(Boolean)),
                ];
                setComisionGeneral(
                    seccionesIniciales.length === 1 &&
                        COMISIONES.includes(seccionesIniciales[0])
                        ? seccionesIniciales[0]
                        : ''
                );
            } catch (err) {
                console.error(err);
                setError(
                    `No se pudo cargar el estudiante. ${err.message || 'Revisá la conexión con la API.'}`
                );
            } finally {
                setIsLoading(false);
            }
        };

        cargarEstudiante();

    }, [id]);

    const handleChange = (event) => {
        const { name, value } = event.target;

        setEditingStudent((current) => ({
            ...current,
            [name]: value,
        }));
    };

    const handleComisionMateriaChange = (materiaId, seccion) => {
        setEditingStudent((current) => {
            const materiasDisponibles = current.materiasDisponibles.map((materia) => (
                materia.id === materiaId
                    ? { ...materia, seccionSeleccionada: seccion }
                    : materia
            ));

            return {
                ...current,
                materiasDisponibles,
            };
        });
        const secciones = editingStudent.materiasDisponibles.map((materia) => (
            materia.id === materiaId ? seccion : materia.seccionSeleccionada
        ));
        setComisionGeneral(
            secciones.length > 0 &&
                secciones.every((item) => item && item === secciones[0])
                ? secciones[0]
                : ''
        );
        setError('');
    };

    const handleComisionGeneralChange = (event) => {
        const seccion = event.target.value;
        setError('');
        setComisionGeneral(seccion);
        setEditingStudent((current) => ({
            ...current,
            materiasDisponibles: current.materiasDisponibles.map((materia) => ({
                ...materia,
                seccionSeleccionada: seccion,
            })),
        }));
    };

    const handleSave = async (event) => {
        event.preventDefault();

        try {
            setError('');
            setNotice('');

            const nombreCompleto = editingStudent.nombreCompleto.trim();
            const partesNombre = nombreCompleto.split(/\s+/);

            if (partesNombre.length < 2) {
                setError('Ingresá el nombre y el apellido del estudiante.');
                return;
            }

            const nombre = partesNombre[0];
            const apellido = partesNombre.slice(1).join(' ');

            // Primero actualizamos los datos personales.
            await modificarEstudiante(editingStudent.id, {
                nombre,
                apellido,
                dni: editingStudent.dni.trim(),
            });

            for (const materia of editingStudent.materiasDisponibles) {
                if (!materia.seccionSeleccionada) {
                    continue;
                }

                if (!materia.inscripcion) {
                    await inscribirEstudiante(
                        editingStudent.id,
                        materia.id,
                        materia.seccionSeleccionada
                    );
                } else if (materia.seccionSeleccionada !== materia.inscripcion.seccion) {
                    await reasignarComision(
                        editingStudent.id,
                        materia.id,
                        materia.seccionSeleccionada
                    );
                }
            }

            setNotice('Los cambios se guardaron correctamente.');

            // Volvemos a consultar los datos persistidos.
            const [datosActualizados, inscripcionesActualizadas] =
                await Promise.all([
                    consultarEstudiante(editingStudent.id),
                    consultarInscripcionesEstudiante(editingStudent.id),
                ]);

            const materiasActualizadas = editingStudent.materiasDisponibles.map((materia) => {
                const inscripcion = inscripcionesActualizadas.find(
                    (item) => item.materiaId === materia.id
                ) ?? null;

                return {
                    ...materia,
                    inscripcion,
                    seccionSeleccionada: inscripcion?.seccion ?? '',
                };
            });

            setEditingStudent((actual) => ({
                ...actual,
                nombre: datosActualizados.nombre,
                apellido: datosActualizados.apellido,
                nombreCompleto:
                    `${datosActualizados.nombre} ${datosActualizados.apellido}`,
                dni: datosActualizados.dni,
                inscripciones: inscripcionesActualizadas,
                materiasDisponibles: materiasActualizadas,
            }));
            const seccionesActualizadas = [
                ...new Set(
                    inscripcionesActualizadas
                        .map((item) => item.seccion)
                        .filter(Boolean)
                ),
            ];
            setComisionGeneral(
                seccionesActualizadas.length === 1 &&
                    COMISIONES.includes(seccionesActualizadas[0])
                    ? seccionesActualizadas[0]
                    : ''
            );
        } catch (err) {
            console.error(err);
            setError(
                'No se pudieron guardar todos los cambios. Revisá los datos y la respuesta de la API.'
            );
        }
    };


    if (isLoading) {
        return (
            <section className="contenedor">

                <p>
                    Cargando estudiante...
                </p>

            </section>

        );
    }

    if (!editingStudent) {
        return (
            <div>
                <h2>{error || 'Estudiante no encontrado.'}</h2>

                <button onClick={() => navigate('/secretario/manage-slots')}>
                    Volver
                </button>
            </div>
        );
    }

    return (
        <div className='contenedor'>

            <div className='cab'>
                <div className='logo'>
                    <img src={FileText} alt="" />
                </div>
                <div className='space'></div>
                <h1>
                    Alumnos de 1er Año
                </h1>
            </div>

            <hr />


            <form onSubmit={handleSave}>
                <div className='contenido'>

                    <div className="ladoIzq">
                        <div className='student-edit-form'>

                            <h2>
                                Datos personales
                            </h2>

                            <div>
                                <label>
                                    Nombre completo
                                </label>

                                <input
                                    className='date'
                                    type="text"
                                    name="nombreCompleto"
                                    value={editingStudent.nombreCompleto ?? ''}
                                    onChange={handleChange}
                                />
                            </div>

                            <div>
                                <label>
                                    DNI
                                </label>

                                <input
                                    className='date'
                                    type="text"
                                    name="dni"
                                    value={editingStudent.dni ?? ''}
                                    onChange={handleChange}
                                />
                            </div>

                            <div>
                                <label>
                                    Correo
                                </label>

                                <input
                                    className='date'
                                    type="email"
                                    name="correo"
                                    value={editingStudent.correo ?? ''}
                                    onChange={handleChange}
                                />
                            </div>


                            <div>
                                <label htmlFor="comision-general">Comisión</label>
                                <select
                                    id="comision-general"
                                    className="date"
                                    value={comisionGeneral}
                                    onChange={handleComisionGeneralChange}
                                >
                                    <option value="">Seleccionar comisión</option>
                                    {COMISIONES.map((seccion) => (
                                        <option key={seccion} value={seccion}>
                                            Comisión {seccion}
                                        </option>
                                    ))}
                                </select>
                                
                            </div>
                        </div>
                    </div>

                    <div className='ladoDer'>
                        <h2>Materias y horarios</h2>


                        <div className="student-edit-form1">
                            {editingStudent.materiasDisponibles.length > 0 ? (
                                editingStudent.materiasDisponibles.map((materia, index) => (
                                    <div
                                        key={materia.id}
                                        className={index % 2 === 0 ? 'contV' : 'contB'}
                                    >
                                        <label>{materia.name}</label>

                                        <SubjectScheduleList
                                            subjectId={materia.id}
                                            section={materia.seccionSeleccionada}
                                            subjectName={materia.name}
                                            onChange={(seccion) => handleComisionMateriaChange(
                                                materia.id,
                                                seccion
                                            )}
                                        />

                                    </div>
                                ))
                            ) : (
                                <p>No hay materias cargadas en la base de datos.</p>
                            )}
                        </div>

                    </div>
                </div>

                <br />

                <div className="student-edit-actions">
                    <button
                        className='cancel'
                        type="button"
                        onClick={() => navigate('/secretario/manage-slots')}
                    >
                        Cancelar
                    </button>

                    <div className='space'></div>

                    <button
                        className='save'
                        type="submit"
                    >
                        Guardar cambios
                    </button>

                    <br />
                    <br />

                    {notice && (
                        <p className="crud-notice">
                            {notice}
                        </p>
                    )}

                    {error && (
                        <p role="alert">
                            {error}
                        </p>
                    )}
                </div>
            </form>
        </div>
    );
};

export default ModificarEstudiante;
