import { useEffect, useMemo, useState } from 'react';
import { Edit, Eye } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

import { studentsApi } from '../../components/students/studentApi';

import "../../../../styles/inscription.css";

function ManageSlots() {

  const navigate = useNavigate();

  const [students, setStudents] = useState([]);
  const [search, setSearch] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  // =========================
  // CARGAR ESTUDIANTES
  // =========================

  useEffect(() => {

    const loadStudents = async () => {

      try {

        setIsLoading(true);

        const data = await studentsApi.obtenerEstudiantes();

        setStudents(data);
        setError('');

      } catch (error) {

        setError('No fue posible cargar los estudiantes.');

      } finally {

        setIsLoading(false);

      }

    };

    loadStudents();

  }, []);

  // =========================
  // BUSCADOR
  // =========================

  const filteredStudents = useMemo(() => {

    const text = search.toLowerCase().trim();

    if (!text) {
      return students;
    }

    return students.filter((student) =>
      Object.values(student).some((value) =>
        String(value)
          .toLowerCase()
          .includes(text)
      )
    );

  }, [students, search]);

  // =========================
  // MODIFICAR
  // =========================

  const handleEdit = (id) => {

    navigate(`/secretario/estudiantes/${id}/modificar`);

  };

  // =========================
  // VISUALIZAR
  // =========================

  const handleView = (id) => {

    navigate(`/secretario/estudiantes/${id}/visualizar`);

  };

  return (

    <section className="management-page">

      <div className="management-heading">

        <div>

          <h1>
            Alumnos de 1er Año
          </h1>

          <p>
            Administrá los estudiantes inscriptos.
          </p>

        </div>

      </div>

      {error && (
        <p role="alert">
          {error}
        </p>
      )}

      <div className="management-table">

        {isLoading ? (

          <p>
            Cargando estudiantes...
          </p>

        ) : (

          <table aria-label="Tabla de estudiantes">

            <thead>

              <tr>

                <th className="f1">
                  ESTUDIANTE
                </th>

                <th className="f1">
                  DNI
                </th>

                <th className="f1">
                  CORREO ELECTRÓNICO
                </th>

                <th className="f1">
                  ESTADO
                </th>

                <th className="f1">
                  ACCIONES
                </th>

              </tr>

            </thead>

            <tbody>

              {filteredStudents.length === 0 ? (

                <tr>

                  <td colSpan="5">
                    No se encontraron estudiantes.
                  </td>

                </tr>

              ) : (

                filteredStudents.map((student) => (

                  <tr key={student.id}>

                    <td>
                      {student.nombreCompleto}
                    </td>

                    <td>
                      {student.dni}
                    </td>

                    <td>
                      {student.correo}
                    </td>

                    <td>
                      {student.estado}
                    </td>

                    <td>

                      <div className="actions">

                        {/* MODIFICAR */}
                        <button
                          type="button"
                          onClick={() => handleEdit(student.id)}
                          aria-label={`Modificar ${student.nombreCompleto}`}
                        >
                          <Edit size={16} />
                        </button>

                        {/* VISUALIZAR */}
                        <button
                          type="button"
                          onClick={() => handleView(student.id)}
                          aria-label={`Visualizar ${student.nombreCompleto}`}
                        >
                          <Eye size={16} />
                        </button>

                      </div>

                    </td>

                  </tr>

                ))

              )}

            </tbody>

          </table>

        )}

        <br />
        <br />

        <div className="table-toolbar1">

          <input
            type="text"
            placeholder="Buscar Alumno"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            className="search-input1"
            aria-label="Buscar estudiante"
          />

        </div>

      </div>

    </section>

  );

}

export default ManageSlots;