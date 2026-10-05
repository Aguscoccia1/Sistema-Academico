import { useEffect, useState } from 'react';
import { Save, X } from 'lucide-react';
import { useNavigate, useParams } from 'react-router-dom';

import { studentsApi } from '../../components/students/studentApi';

import "../../../../styles/modificarEst.css";


function ModificarEstudiante() {

  // ID que viene desde la URL
  const { id } = useParams();

  // Para volver a la lista
  const navigate = useNavigate();

  const [editingStudent, setEditingStudent] = useState(null);

  const [isLoading, setIsLoading] = useState(true);

  const [notice, setNotice] = useState('');

  const [error, setError] = useState('');


  // ==========================================
  // BUSCAR ESTUDIANTE
  // ==========================================

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

          // Si existe comision la usamos.
          // Si no, usamos estado.
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


  // ==========================================
  // CAMBIAR DATOS
  // ==========================================

  const handleChange = (event) => {

    const { name, value } = event.target;

    setEditingStudent((current) => ({

      ...current,

      [name]: value

    }));

  };


  // ==========================================
  // GUARDAR
  // ==========================================

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


  // ==========================================
  // CANCELAR
  // ==========================================

  const handleCancel = () => {

    navigate('/secretario/manage-slots');

  };


  // ==========================================
  // CARGANDO
  // ==========================================

  if (isLoading) {

    return (

      <section className="contenedor">

        <p>
          Cargando estudiante...
        </p>

      </section>

    );

  }


  // ==========================================
  // ERROR
  // ==========================================

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
          Modificar estudiante
        </h1>


        <div className="contenido">


          {/* =================================
              LADO IZQUIERDO
          ================================= */}

          <div className="ladoIzq">

            <div className="student-edit-form">

              <h2>
                Datos del estudiante
              </h2>


              {/* NOMBRE */}

              <label>

                Nombre y apellido

                <input
                  type="text"
                  name="nombreCompleto"
                  value={
                    editingStudent?.nombreCompleto ?? ''
                  }
                  onChange={handleChange}
                />

              </label>


              {/* DNI */}

              <label>

                DNI

                <input
                  type="text"
                  name="dni"
                  value={
                    editingStudent?.dni ?? ''
                  }
                  onChange={handleChange}
                />

              </label>


              {/* CORREO */}

              <label>

                Correo electrónico

                <input
                  type="email"
                  name="correo"
                  value={
                    editingStudent?.correo ?? ''
                  }
                  onChange={handleChange}
                />

              </label>


              {/* COMISIÓN */}

              <label>

                Comisión

                <select
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


              {/* BOTONES */}

              <div className="student-edit-actions">

                <button
                  type="button"
                  onClick={handleSave}
                >

                  <Save size={16} />

                  Guardar

                </button>


                <button
                  type="button"
                  onClick={handleCancel}
                >

                  <X size={16} />

                  Cancelar

                </button>

              </div>


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

          </div>


          {/* =================================
              LADO DERECHO
          ================================= */}

          <div className="ladoDer">

            <h3>
              Materias
            </h3>

            <p>
              Acá podés agregar posteriormente
              las materias del estudiante.
            </p>

          </div>


        </div>

      </div>

    </section>

  );

}

export default ModificarEstudiante;