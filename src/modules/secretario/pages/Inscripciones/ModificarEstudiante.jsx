import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import { studentsApi } from '../../components/students/studentApi';

import "../../../../styles/modificarEst.css";

import {
    MATERIAS,
    CLAVES_MATERIAS,
} from '../../components/students/materiasHorarios';

const ModificarEstudiante = () => {
    const { id } = useParams();
    const navigate = useNavigate();

    const [editingStudent, setEditingStudent] = useState(null);

    const [isLoading, setIsLoading] = useState(true);
    const [notice, setNotice] = useState('');
    const [error, setError] = useState('');

    useEffect(() => {
        const cargarEstudiante = async () => {
            try {
                setIsLoading(true);

                const estudiantes = await studentsApi.obtenerEstudiantes();

                const student = estudiantes.find(
                    (item) => String(item.id) === String(id)
                );

                if (!student) {
                    setError('No se encontró el estudiante.');
                    return;
                }

                const materiasIniciales = Object.fromEntries(
                    CLAVES_MATERIAS.map((materia) => [
                        materia,
                        student.materias?.[materia] ?? student.comision ?? '',
                    ])
                );

                setEditingStudent({
                    ...student,

                    comision: student.comision ?? '',

                    materias: materiasIniciales,
                });
            } catch (err) {
                console.error(err);
                setError('Ocurrió un error al cargar el estudiante.');
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


    const handleCommissionChange = (event) => {
        const nuevaComision = event.target.value;

        setEditingStudent((current) => ({
            ...current,

            comision: nuevaComision,

            materias: Object.fromEntries(
                CLAVES_MATERIAS.map((materia) => [
                    materia,
                    nuevaComision,
                ])
            ),
        }));
    };


    const handleMateriaChange = (materia, nuevaComision) => {
        setEditingStudent((current) => ({
            ...current,

            materias: {
                ...current.materias,

                [materia]: nuevaComision,
            },
        }));
    };


    const handleSave = async (event) => {
        event.preventDefault();

        try {
            setError('');
            setNotice('');


            const todasLasMateriasCompletas = CLAVES_MATERIAS.every(
                (materia) => editingStudent.materias?.[materia]
            );


            const nuevoEstado =
                editingStudent.comision && todasLasMateriasCompletas
                    ? 'Completa'
                    : 'Pendiente';

            const updatedStudent = await studentsApi.actualizarEstudiante(
                editingStudent.id,
                {
                    id: editingStudent.id,

                    nombreCompleto: editingStudent.nombreCompleto,

                    dni: editingStudent.dni,

                    correo: editingStudent.correo,

                    comision: editingStudent.comision,

                    materias: editingStudent.materias,

                    estado: nuevoEstado,
                }
            );

            setEditingStudent(updatedStudent);

            setNotice(
                nuevoEstado === 'Completa'
                    ? 'Estudiante modificado y completado correctamente.'
                    : 'Estudiante modificado correctamente, pero todavía quedan materias pendientes.'
            );
        } catch (err) {
            console.error(err);

            setError('No se pudieron guardar los cambios.');
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
                <div className='logo'></div>
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
                                <label>
                                    Comisión
                                </label>

                                <select
                                    className='date'
                                    name="comision"
                                    value={editingStudent.comision ?? ''}
                                    onChange={handleCommissionChange}
                                >
                                    <option value="">
                                        Seleccionar comisión
                                    </option>

                                    <option value="A">
                                        Comisión A
                                    </option>

                                    <option value="B">
                                        Comisión B
                                    </option>
                                </select>
                            </div>
                        </div>
                    </div>

                    <div className='ladoDer'>
                        <h2>Materias y horarios</h2>

                        <div className="student-edit-form1">



                            {CLAVES_MATERIAS.map((materia, index) => {

                                const comisionSeleccionada =
                                    editingStudent.materias?.[materia] ?? '';

                                const horarioSeleccionado =
                                    comisionSeleccionada
                                        ? MATERIAS[materia].horarios[comisionSeleccionada]
                                        : '';

                                return (
                                    <div
                                        key={materia}
                                        className={index % 2 === 0 ? 'contV' : 'contB'}
                                    >

                                        <label>
                                            {MATERIAS[materia].nombre}
                                        </label>


                                        <select
                                            className='date1'
                                            value={comisionSeleccionada}
                                            onChange={(event) =>
                                                handleMateriaChange(
                                                    materia,
                                                    event.target.value
                                                )
                                            }
                                        >

                                            <option value="">
                                                Seleccionar comisión
                                            </option>

                                            <option value="A">
                                                Comisión A - {MATERIAS[materia].horarios.A}
                                            </option>

                                            <option value="B">
                                                Comisión B - {MATERIAS[materia].horarios.B}
                                            </option>

                                        </select>




                                    </div>
                                );
                            })}

                        </div>
                    </div>
                </div>

                <br />

                <div className="student-edit-actions">

                    <button
                        className='save'
                        type="submit"
                    >
                        Guardar cambios
                    </button>

                    <div className='space'></div>

                    <button
                        className='cancel'
                        type="button"
                        onClick={() => navigate('/secretario/manage-slots')}
                    >
                        Cancelar y Volver
                    </button>
                    <br />
                    <br />

                    {/* MENSAJE */}

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