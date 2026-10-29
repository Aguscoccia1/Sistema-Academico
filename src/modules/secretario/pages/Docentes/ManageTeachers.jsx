
import "../../../../styles/inscription.css";

import { useEffect, useMemo, useState } from 'react';
import { Edit, Eye, Shuffle, Users } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

import { studentsApi } from '../../components/students/studentApi';

const ManageTeachers = () => {
  const navigate = useNavigate();

  const [students, setStudents] = useState([]);
  const [search, setSearch] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [commission, setCommission] = useState('');

  const [totalEstudiantes, setTotalEstudiantes] = useState(0);
  const [asignando, setAsignando] = useState(false);
  const [mensaje, setMensaje] = useState('');


  if (isLoading) {
    return (
      <div className="manage-slots-page">
        <h2>Cargando estudiantes...</h2>
      </div>
    );
  }

  return (
    <div className="manage-slots-page">

      {/* ENCABEZADO */}
      <div className="management-header">
        <div>
          <h1>Gestionar estudiantes de 1er año</h1>

          <p>
            Modificá o visualizá la información de los estudiantes.
          </p>
        </div>

        {/* FILTRO POR COMISIÓN */}
        <div className="ladoDer1">

          <div className="asignar-aleatorio-container">
            <button
              type="button"
              className="asignar-aleatorio"
              onClick={handleAsignacionAleatoria}
              disabled={asignando}
            >
              {asignando
                ? 'Asignando...'
                : 'Asignar comisiones aleatoriamente'}
            </button>
          </div>

          <select
            className="filtro"
            name="comision"
            value={commission}
            onChange={(event) => setCommission(event.target.value)}
          >
            <option value="">Todas las comisiones</option>
            <option value="A">Comisión A</option>
            <option value="B">Comisión B</option>
          </select>

        </div>
      </div>




      {/* TABLA Y BUSCADOR */}
      <div className="management-table-container">

        <div className="table-toolbar1">
          <input
            type="text"
            placeholder="Buscar estudiante..."
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            className="search-input1"
          />
        </div>

        <br />

        <table className="management-table">
          <thead>
            <tr>
              <th>Nombre completo</th>
              <th>DNI</th>
              <th>Correo</th>
              <th>Comisión</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>

          <tbody>
            {estudiantesFiltrados.length > 0 ? (
              estudiantesFiltrados.map((student) => (
                <tr key={student.id}>
                  <td>{student.nombreCompleto}</td>
                  <td>{student.dni}</td>
                  <td>{student.correo}</td>
                  <td>
                    {student.comision
                      ? `Comisión ${student.comision}`
                      : 'Sin asignar'}
                  </td>
                  <td>{student.estado}</td>

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
              ))
            ) : (
              <tr>
                <td colSpan={6}>
                  No se encontraron estudiantes.
                </td>
              </tr>
            )}
          </tbody>
        </table>
        <div className="total-estudiantes">

          <div className="ladoDer1">
            <h3>Total de alumnos inscriptos:</h3>
            <p>{totalEstudiantes}</p>
            
          </div>
        </div>

        <br />
        
        {/* MENSAJES */}
        {mensaje && (
          <p className="crud-notice" role="status">
            {mensaje}
          </p>
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
