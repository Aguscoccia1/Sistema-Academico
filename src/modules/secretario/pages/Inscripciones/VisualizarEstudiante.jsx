import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import { studentsApi } from '../../components/students/studentApi';

import "../../../../styles/modificarEst.css";


function VisualizarEstudiante() {

    const { id } = useParams();

    const navigate = useNavigate();

    const [student, setStudent] = useState(null);

    const [isLoading, setIsLoading] = useState(true);

    const [error, setError] = useState('');



    useEffect(() => {

        const loadStudent = async () => {

            try {

                setIsLoading(true);

                const students =
                    await studentsApi.obtenerEstudiantes();

                const selectedStudent =
                    students.find(
                        (student) =>
                            String(student.id) === String(id)
                    );

                if (!selectedStudent) {

                    setError(
                        'No se encontró el estudiante.'
                    );

                    return;

                }

                setStudent(selectedStudent);

                setError('');

            } catch (error) {

                setError(
                    'No fue posible cargar el estudiante.'
                );

            } finally {

                setIsLoading(false);

            }

        };

        loadStudent();

    }, [id]);


    if (isLoading) {

        return (

            <section className="contenedor">

                <p>
                    Cargando estudiante...
                </p>

            </section>

        );

    }


    if (error) {

        return (

            <section className="contenedor">

                <p role="alert">
                    {error}
                </p>

                <button
                    type="button"
                    onClick={() =>
                        navigate('/secretario/manage-slots')
                    }
                >
                    Volver
                </button>

            </section>

        );

    }


    return (

        <section>

            <div className="contenedor">

                <h1>
                    Alumnos de 1er Año
                </h1>


                <div className="contenido">



                    <div className="ladoIzq">

                        <div className="student-edit-form">

                            <h2>
                                Datos personales
                            </h2>


                            <label>

                                Nombre y apellido
                                <br />

                                <input
                                    className='date'
                                    type="text"
                                    value={
                                        student?.nombreCompleto ?? ''
                                    }
                                    readOnly
                                />

                            </label>


                            <label>

                                DNI
                                <br />

                                <input
                                    className='date'
                                    type="text"
                                    value={
                                        student?.dni ?? ''
                                    }
                                    readOnly
                                />

                            </label>


                            <label>

                                Correo electrónico
                                <br />

                                <input
                                    className='date'
                                    type="email"
                                    value={
                                        student?.correo ?? ''
                                    }
                                    readOnly
                                />

                            </label>


                            <label>

                                Comision
                                <br />

                                <input
                                    className='date'
                                    type="text"
                                    value={
                                        student?.estado ?? ''
                                    }
                                    readOnly
                                />

                            </label>




                        </div>

                    </div>



                    <div className="ladoDer">

                        <h2>
                            Materias
                        </h2>

                        <label>

                            Programacion
                            <br />

                            <input
                                className='date'
                                type="text"
                                value={
                                    student?.estado ?? ''
                                }
                                readOnly
                            />

                        </label>

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

        </section>

    );

}

export default VisualizarEstudiante;