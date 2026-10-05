import { useEffect, useState } from 'react';
import { Save, X } from 'lucide-react';
import { useNavigate, useParams } from 'react-router-dom';

import { studentsApi } from '../../components/students/studentApi';

import "../../../../styles/modificarEst.css";


function ModificarEstudiante() {

    const { id } = useParams();

    const navigate = useNavigate();

    const [editingStudent, setEditingStudent] = useState(null);

    const [isLoading, setIsLoading] = useState(true);

    const [notice, setNotice] = useState('');

    const [error, setError] = useState('');

    const [hover, setHover] = useState(false);




    useEffect(() => {

        const loadStudent = async () => {

            try {

                setIsLoading(true);

                const students =
                    await studentsApi.obtenerEstudiantes();

                const student =
                    students.find(
                        (student) =>
                            String(student.id) === String(id)
                    );

                if (!student) {

                    setError('No se encontró el estudiante.');

                    return;

                }

                setEditingStudent({
                    ...student,


                    comision:
                        student.comision ??
                        student.estado ??
                        ''
                });

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



    const handleChange = (event) => {

        const { name, value } = event.target;

        setEditingStudent((current) => ({

            ...current,

            [name]: value

        }));

    };



    const handleSave = async () => {

        if (!editingStudent) {
            return;
        }

        try {

            const updatedStudent =
                await studentsApi.actualizarEstudiante(

                    editingStudent.id,

                    {
                        nombreCompleto:
                            editingStudent.nombreCompleto,

                        dni:
                            editingStudent.dni,

                        correo:
                            editingStudent.correo,

                        comision:
                            editingStudent.comision
                    }

                );

            setEditingStudent(updatedStudent);

            setNotice(
                'Estudiante modificado correctamente.'
            );

            setError('');

        } catch (error) {

            setError(
                'No fue posible modificar el estudiante.'
            );

        }

    };



    const handleCancel = () => {

        navigate('/secretario/manage-slots');

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


    if (error && !editingStudent) {

        return (

            <section className="contenedor">

                <p role="alert">
                    {error}
                </p>

                <button
                    type="button"
                    onClick={handleCancel}
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
                                Datos del estudiante
                            </h2>


                            <label>

                                Nombre y apellido
                                <br />

                                <input
                                    className='date'
                                    type="text"
                                    name="nombreCompleto"
                                    value={
                                        editingStudent?.nombreCompleto ?? ''
                                    }
                                    onChange={handleChange}
                                />

                            </label>
                            <br />


                            <label>

                                DNI
                                <br />

                                <input
                                    className='date'
                                    type="text"
                                    name="dni"
                                    value={
                                        editingStudent?.dni ?? ''
                                    }
                                    onChange={handleChange}
                                />

                            </label>
                            <br />


                            <label>

                                Correo electrónico

                                <br />

                                <input
                                    className='date'
                                    type="email"
                                    name="correo"
                                    value={
                                        editingStudent?.correo ?? ''
                                    }
                                    onChange={handleChange}
                                />

                            </label>
                            <br />


                            <label>

                                Comisión

                                <br />

                                <select
                                    className='date'
                                    name="comision"
                                    value={
                                        editingStudent?.comision ?? ''
                                    }
                                    onChange={handleChange}
                                >

                                    <option value="">
                                        Seleccionar
                                    </option>

                                    <option value="A">
                                        Comisión A
                                    </option>

                                    <option value="B">
                                        Comisión B
                                    </option>

                                </select>

                            </label>


                        </div>

                    </div>

                    <div className="ladoDer">

                        <h2>
                            Materias
                        </h2>

                        <div className="student-edit-form1">


                            <div className='contV'>
                                <label>

                                    Programacion I

                                    <select
                                        className='date1'
                                        name="comision"
                                        value={
                                            editingStudent?.comision ?? ''
                                        }
                                        onChange={handleChange}
                                    >

                                        <option value="">
                                            Seleccionar
                                        </option>

                                        <option value="A">
                                            Lunes 14:00 hs a 16:00 hs y Miercoles 16:20 hs a 17:30 hs
                                        </option>

                                        <option value="B">
                                            Comisión B
                                        </option>

                                    </select>

                                </label>
                            </div>
                            


                            <div className='contB'>
                                <label>

                                    Sistemas y Organizaciones

                                    <select
                                        className='date1'
                                        name="comision"
                                        value={
                                            editingStudent?.comision ?? ''
                                        }
                                        onChange={handleChange}
                                    >

                                        <option value="">
                                            Seleccionar
                                        </option>

                                        <option value="A">
                                            Lunes 16:10 hs a 18:50 hs
                                        </option>

                                        <option value="B">
                                            Comisión B
                                        </option>

                                    </select>

                                </label>
                            </div>
                            


                            <div className='contV'>
                                <label>

                                    Competencias Comunicacionales I

                                    <select
                                        className='date1'
                                        name="comision"
                                        value={
                                            editingStudent?.comision ?? ''
                                        }
                                        onChange={handleChange}
                                    >

                                        <option value="">
                                            Seleccionar
                                        </option>

                                        <option value="A">
                                            Miercoles 14:00 hs a 16:00 hs
                                        </option>

                                        <option value="B">
                                            Comisión B
                                        </option>

                                    </select>

                                </label>
                            </div>
                            


                            <div className='contB'>
                                <label>

                                    Base de Datos

                                    <select
                                        className='date1'
                                        name="comision"
                                        value={
                                            editingStudent?.comision ?? ''
                                        }
                                        onChange={handleChange}
                                    >

                                        <option value="">
                                            Seleccionar
                                        </option>

                                        <option value="A">
                                            Jueves 14:00 hs a 16:40 hs
                                        </option>

                                        <option value="B">
                                            Comisión B
                                        </option>

                                    </select>

                                </label>
                            </div>

                            <div className='contV'>
                                <label>

                                    Aproximaciones al Mundo del Trabajo

                                    <select
                                        className='date1'
                                        name="comision"
                                        value={
                                            editingStudent?.comision ?? ''
                                        }
                                        onChange={handleChange}
                                    >

                                        <option value="">
                                            Seleccionar
                                        </option>

                                        <option value="A">
                                            Martes 16:10 hs a 18:10 hs
                                        </option>

                                        <option value="B">
                                            Comisión B
                                        </option>

                                    </select>

                                </label>
                            </div>
                            


                            <div className='contB'>
                                <label>

                                    Ingles

                                    <select
                                        className='date1'
                                        name="comision"
                                        value={
                                            editingStudent?.comision ?? ''
                                        }
                                        onChange={handleChange}
                                    >

                                        <option value="">
                                            Seleccionar
                                        </option>

                                        <option value="A">
                                            Jueves 17:00 hs a 18:20 hs
                                        </option>

                                        <option value="B">
                                            Comisión B
                                        </option>

                                    </select>

                                </label>
                            </div>

                            <div className='contV'>
                                <label>

                                   Elementos de Matematica y Logica

                                    <select
                                        className='date1'
                                        name="comision"
                                        value={
                                            editingStudent?.comision ?? ''
                                        }
                                        onChange={handleChange}
                                    >

                                        <option value="">
                                            Seleccionar
                                        </option>

                                        <option value="A">
                                            Martes 14:00 hs a 16:00 hs
                                        </option>

                                        <option value="B">
                                            Comisión B
                                        </option>

                                    </select>

                                </label>
                            </div>
                            


                            <div className='contB'>
                                <label>

                                    Arquitectura de las Computadoras

                                    <select
                                        className='date1'
                                        name="comision"
                                        value={
                                            editingStudent?.comision ?? ''
                                        }
                                        onChange={handleChange}
                                    >

                                        <option value="">
                                            Seleccionar
                                        </option>

                                        <option value="A">
                                            Martes 16:10 hs a 18:10 hs
                                        </option>

                                        <option value="B">
                                            Comisión B
                                        </option>

                                    </select>

                                </label>
                            </div>

                            <div className='contV'>
                                <label>

                                   Competencias Comunicacionales II

                                    <select
                                        className='date1'
                                        name="comision"
                                        value={
                                            editingStudent?.comision ?? ''
                                        }
                                        onChange={handleChange}
                                    >

                                        <option value="">
                                            Seleccionar
                                        </option>

                                        <option value="A">
                                            Miercoles 14:00 hs a 16:00 hs
                                        </option>

                                        <option value="B">
                                            Comisión B
                                        </option>

                                    </select>

                                </label>
                            </div>
                            


                            <div className='contB'>
                                <label>

                                    Etica y Deontologia Profesional

                                    <select
                                        className='date1'
                                        name="comision"
                                        value={
                                            editingStudent?.comision ?? ''
                                        }
                                        onChange={handleChange}
                                    >

                                        <option value="">
                                            Seleccionar
                                        </option>

                                        <option value="A">
                                            Miercoles 17:40 hs a 19:40 hs
                                        </option>

                                        <option value="B">
                                            Comisión B
                                        </option>

                                    </select>

                                </label>
                            </div>

                        </div>

                    </div>




                </div>
                <br />
                <br />

                <div className="student-edit-actions">

                    <button
                        className='save'
                        type="button"
                        onClick={handleSave}
                        onMouseEnter={() => setHover(true)} onMouseLeave={() => setHover(false)}

                    >

                        <Save size={16} />

                        Confirmar Inscripcion

                    </button>

                    <div className='space'></div>

                    <button
                        className='cancel'
                        type="button"
                        onClick={handleCancel}
                        onMouseEnter={() => setHover(true)} onMouseLeave={() => setHover(false)}

                    >

                        <X size={16} />

                        Cancelar

                    </button>

                </div>

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

        </section>

    );

}

export default ModificarEstudiante;