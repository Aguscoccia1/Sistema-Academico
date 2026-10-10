import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import "../../../../styles/visualizarEst.css";
import FileText from '../../../../assets/FileText.svg';



import { studentsApi } from '../../components/students/studentApi';
import {
    MATERIAS,
    CLAVES_MATERIAS,
} from '../../components/students/materiasHorarios';

const VisualizarEstudiante = () => {
    const { id } = useParams();
    const navigate = useNavigate();

    const [student, setStudent] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        const cargarEstudiante = async () => {
            try {
                setIsLoading(true);

                const estudiantes =
                    await studentsApi.obtenerEstudiantes();

                const encontrado = estudiantes.find(
                    (item) => String(item.id) === String(id)
                );

                if (!encontrado) {
                    setError('No se encontró el estudiante.');
                    return;
                }

                setStudent(encontrado);

            } catch (err) {
                console.error(err);

                setError(
                    'Ocurrió un error al cargar el estudiante.'
                );
            } finally {
                setIsLoading(false);
            }
        };

        cargarEstudiante();
    }, [id]);

    if (isLoading) {
        return (
            <div>
                <h2>Cargando estudiante...</h2>
            </div>
        );
    }

    if (!student) {
        return (
            <div>
                <h2>
                    {error || 'Estudiante no encontrado.'}
                </h2>

                <button
                    onClick={() =>
                        navigate('/secretario/manage-slots')
                    }
                >
                    Volver
                </button>
            </div>
        );
    }


    const obtenerHorario = (materia) => {

        const comision =
            student.materias?.[materia];

        if (!comision) {
            return 'Sin comisión asignada';
        }

        const horario =
            MATERIAS[materia].horarios[comision];

        if (!horario) {
            return 'Horario no disponible';
        }

        return `Comisión ${comision} - ${horario}`;
    };

    return (
        <div className="contenedor">




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

            <div className='contenido'>

                <div className='ladoIzq'>
                    <h2>
                        Datos personales
                    </h2>

                    <div className="student-edit-form">

                        <div>
                            <label>
                                Nombre completo
                            </label>

                            <input
                                className='date'
                                type="text"
                                value={student.nombreCompleto ?? ''}
                                readOnly
                            />
                        </div>


                        <div>
                            <label>
                                DNI
                            </label>

                            <input
                                className='date'
                                type="text"
                                value={student.dni ?? ''}
                                readOnly
                            />
                        </div>


                        <div>
                            <label>
                                Correo
                            </label>

                            <input
                                className='date'
                                type="email"
                                value={student.correo ?? ''}
                                readOnly
                            />
                        </div>


                        <div>
                            <label>
                                Comisión general
                            </label>

                            <input
                                className='date'
                                type="text"
                                value={student.comision ?? 'Sin comisión'}
                                readOnly
                            />
                        </div>


                        <div>
                            <label>
                                Estado
                            </label>

                            <input
                                className='date'
                                type="text"
                                value={student.estado ?? 'Pendiente'}
                                readOnly
                            />
                        </div>

                    </div>
                </div>


                {/* ========================= */}
                {/* MATERIAS */}
                {/* ========================= */}

                <div className='ladoDer'>
                    <h2>
                        Materias y horarios
                    </h2>


                    <div className="student-edit-form1">

                        {CLAVES_MATERIAS.map((materia, index) => {

                            return (
                                <div
                                    key={materia}
                                    className={
                                        index % 2 === 0
                                            ? 'contV'
                                            : 'contB'
                                    }
                                >

                                    <label>
                                        {MATERIAS[materia].nombre}
                                    </label>

                                    <input
                                        className='date'
                                        type="text"
                                        value={obtenerHorario(materia)}
                                        readOnly
                                    />

                                </div>
                            );

                        })}

                    </div>
                </div>



            </div>
            <br />
            <div className="student-edit-actions">
                <button
                    className='cancel'
                    type="button"
                    onClick={() =>
                        navigate('/secretario/manage-slots')
                    }
                >
                    Volver
                </button>

            </div>

        </div>
    );
};

export default VisualizarEstudiante;