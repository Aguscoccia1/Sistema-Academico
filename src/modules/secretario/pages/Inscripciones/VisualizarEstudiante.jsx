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


  // ==========================================
  // BUSCAR ESTUDIANTE
  // ==========================================

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
          Visualizar estudiante
        </h1>


        <div className="contenido">


          {/* LADO IZQUIERDO */}

          <div className="ladoIzq">

            <div className="student-edit-form">

              <h2>
                Datos personales
              </h2>


              <label>

                Nombre y apellido

                <input
                  type="text"
                  value={
                    student?.nombreCompleto ?? ''
                  }
                  readOnly
                />

              </label>


              <label>

                DNI

                <input
                  type="text"
                  value={
                    student?.dni ?? ''
                  }
                  readOnly
                />

              </label>


              <label>

                Correo electrónico

                <input
                  type="email"
                  value={
                    student?.correo ?? ''
                  }
                  readOnly
                />

              </label>


              <label>

                Estado

                <input
                  type="text"
                  value={
                    student?.estado ?? ''
                  }
                  readOnly
                />

              </label>


              <button
                type="button"
                onClick={() =>
                  navigate('/secretario/manage-slots')
                }
              >
                Volver
              </button>

            </div>

          </div>


          {/* LADO DERECHO */}

          <div className="ladoDer">

            <h3>
              Materias
            </h3>

            <p>
              Acá podés mostrar las materias
              correspondientes al estudiante.
            </p>

          </div>


        </div>

      </div>

    </section>

  );

}

export default VisualizarEstudiante;