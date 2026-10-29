import "../../../../styles/inscription.css";

import { useEffect, useMemo, useState } from 'react';
import { Edit, Eye } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

import { obtenerDocentes } from '../../../../services/secretario/teacherAdministrationService';

const ManageTeachers = () => {
  const navigate = useNavigate();
  const [teachers, setTeachers] = useState([]);
  const [search, setSearch] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const cargarDocentes = async () => {
      try {
        setIsLoading(true);
        setError('');
        setTeachers(await obtenerDocentes());
      } catch (err) {
        console.error(err);
        setError(
          err instanceof Error
            ? `No se pudieron cargar los docentes: ${err.message}`
            : 'No se pudieron cargar los docentes desde el servidor.'
        );
      } finally {
        setIsLoading(false);
      }
    };

    cargarDocentes();
  }, []);

  const docentesFiltrados = useMemo(() => {
    const texto = search.toLowerCase().trim();

    return teachers.filter((teacher) =>
      !texto ||
      teacher.nombreCompleto?.toLowerCase().includes(texto) ||
      teacher.dni?.toLowerCase().includes(texto) ||
      teacher.correo?.toLowerCase().includes(texto)
    );
  }, [teachers, search]);

  return (
    <div className="manage-slots-page">
      <div className="management-header">
        <div>
          <h1>Gestionar Docentes del Instituto</h1>
          <p>Modificá o visualizá la información de los docentes.</p>
        </div>
      </div>

      <div className="management-table-container">
        <div className="table-toolbar1">
          <input
            type="text"
            placeholder="Buscar docente..."
            aria-label="Buscar docente"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            className="search-input1"
          />
        </div>

        <br />

        {isLoading ? (
          <p role="status">Cargando docentes...</p>
        ) : (
          <table className="management-table">
            <thead>
              <tr>
                <th>Nombre completo</th>
                <th>DNI</th>
                <th>Correo</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>

            <tbody>
              {docentesFiltrados.length > 0 ? (
                docentesFiltrados.map((teacher) => (
                  <tr key={teacher.id}>
                    <td>{teacher.nombreCompleto}</td>
                    <td>{teacher.dni}</td>
                    <td>{teacher.correo}</td>
                    <td>{teacher.estado}</td>
                    <td>
                      <button
                        type="button"
                        title="Modificar docente"
                        aria-label={`Modificar docente ${teacher.nombreCompleto}`}
                        className="mod"
                        onClick={() =>
                          navigate(`/secretario/docentes/${teacher.id}/modificar`)
                        }
                      >
                        <Edit size={18} />
                      </button>
                      <button
                        type="button"
                        title="Visualizar docente"
                        aria-label={`Visualizar docente ${teacher.nombreCompleto}`}
                        className="vis"
                        onClick={() =>
                          navigate(`/secretario/docentes/${teacher.id}/visualizar`)
                        }
                      >
                        <Eye size={18} />
                      </button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={5}>No se encontraron docentes.</td>
                </tr>
              )}
            </tbody>
          </table>
        )}

        {error && (
          <p className="error" role="alert">
            {error}
          </p>
        )}
      </div>
    </div>
  );
};

export default ManageTeachers;
