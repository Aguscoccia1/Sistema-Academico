import "../../../../styles/inscription.css";

import { useEffect, useMemo, useState } from 'react';
import { Edit, Eye } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

import { studentsApi } from '../../components/students/studentApi';
import GraduationCap from '../../../../assets/GraduationCap.svg';


const ManageSlots = () => {
  const navigate = useNavigate();

  const [students, setStudents] = useState([]);
  const [search, setSearch] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [commission, setCommission] = useState('');

  const cargarEstudiantes = async () => {
    try {
      setIsLoading(true);
      setError('');

      const data =
        await studentsApi.obtenerEstudiantes();

      setStudents(data);

    } catch (err) {
      console.error(err);

      setError(
        'No se pudieron cargar los estudiantes.'
      );
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    cargarEstudiantes();
  }, []);

  const estudiantesFiltrados = useMemo(() => {
    const texto = search.toLowerCase().trim();

    return students.filter((student) => {

      // FILTRO DEL BUSCADOR
      const coincideBusqueda =
        !texto ||
        student.nombreCompleto
          ?.toLowerCase()
          .includes(texto) ||
        student.dni
          ?.toLowerCase()
          .includes(texto) ||
        student.correo
          ?.toLowerCase()
          .includes(texto);

      // FILTRO DEL SELECT
      const coincideComision =
        !commission ||
        student.comision === commission;

      // Tiene que cumplir ambos filtros
      return coincideBusqueda && coincideComision;
    });

  }, [students, search, commission]);

  if (isLoading) {
    return (
      <div>
        <h2>
          Cargando estudiantes...
        </h2>
      </div>
    );
  }



  return (
    <div className="manage-slots-page">

      <div className="management-header">

        <div>
          <h1>
            Gestionar estudiantes de 1er año
          </h1>

          <p>
            Modificá o visualizá la información de
            los estudiantes.
          </p>
        </div>

        <div className="ladoDer">

          <select
            className="filtro"
            name="comision"
            value={commission}
            onChange={(event) => setCommission(event.target.value)}
          >
            <option value="">
              Todas las comisiones
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


      {error && (
        <div className="error">
          {error}
        </div>
      )}



      <div className="management-table-container">

        <table className="management-table">

          <thead>

            <tr>

              <th>
                Nombre completo
              </th>

              <th>
                DNI
              </th>

              <th>
                Correo
              </th>

              <th>
                Estado
              </th>

              <th>
                Acciones
              </th>

            </tr>

          </thead>


          <tbody>

            {estudiantesFiltrados.map(
              (student) => (

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

                    <button
                      type="button"
                      title="Modificar estudiante"
                      onClick={() =>
                        navigate(
                          `/secretario/estudiantes/${student.id}/modificar`
                        )
                      }
                      className="mod"
                    >
                      <Edit size={18} />
                    </button>


                    <button
                      type="button"
                      title="Visualizar estudiante"
                      onClick={() =>
                        navigate(
                          `/secretario/estudiantes/${student.id}/visualizar`
                        )
                      }
                      className="vis"
                    >
                      <Eye size={18} />
                    </button>

                  </td>

                </tr>

              )
            )}

          </tbody>

        </table>

      </div>
      <br />



      <div className="table-toolbar1">

        <input
          type="text"
          placeholder="Buscar estudiante..."
          value={search}
          onChange={(event) =>
            setSearch(event.target.value)
          }
          className="search-input1"
        />

      </div>

    </div>
  );
};

export default ManageSlots;