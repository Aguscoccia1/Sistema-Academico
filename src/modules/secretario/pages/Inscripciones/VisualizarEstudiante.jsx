import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import "../../../../styles/visualizarEst.css";
import FileText from '../../../../assets/FileText.svg';



import { registrationService } from '../../../../services/registrationService';
import { studentAdministrationService } from '../../../../services/estudiante/studentAdministrationService';
import { subjectService } from '../../../../services/subjectService';
import SubjectScheduleList from '../../components/students/SubjectScheduleList';

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
                setError('');

                const [datos, inscripciones, materias] = await Promise.all([
                    studentAdministrationService.consultarEstudiante(id),
                    registrationService.consultarInscripcionesEstudiante(id),
                    subjectService.obtenerTodos(),
                ]);

                setStudent({
                    id: datos.id,
                    nombreCompleto: `${datos.nombre} ${datos.apellido}`,
                    dni: datos.dni,
                    legajo: datos.legajo,
                    correo: datos.email,
                    comision: [
                        ...new Set(inscripciones.map((item) => item.seccion)),
                    ].join(', '),
                    estado: inscripciones.length > 0
                        ? 'Inscripto'
                        : 'Sin inscripciones',
                    inscripciones,
                    materias: materias.map((materia) => ({
                        ...materia,
                        inscripcion: inscripciones.find(
                            (inscripcion) => inscripcion.materiaId === materia.id
                        ) ?? null,
                    })),
                });
            } catch (err) {
                console.error(err);
                setError('No se pudo consultar el estudiante.');
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

                <div className='ladoDer'>
                    <h2>
                        Materias y horarios
                    </h2>


                    <div className="student-edit-form1">
                        {student.materias.length > 0 ? (
                            student.materias.map((materia, index) => (
                                <div
                                    key={materia.id}
                                    className={index % 2 === 0 ? 'contV' : 'contB'}
                                >
                                    <label>{materia.name}</label>

                                    {materia.inscripcion ? (
                                        <SubjectScheduleList
                                            subjectId={materia.id}
                                            section={materia.inscripcion.seccion}
                                            subjectName={materia.name}
                                            readOnly
                                        />
                                    ) : (
                                        <input
                                            className="date"
                                            type="text"
                                            value="Sin inscripción"
                                            readOnly
                                        />
                                    )}
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
