import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import '../../../../styles/modificarEst.css';
import FileText from '../../../../assets/FileText.svg';
import { subjectService } from '../../../../services/subjectService';
import {
  consultarMateriasDocente,
  guardarMateriasDocente,
} from '../../../../services/secretario/teacherAdministrationService';
import { usersService } from '../../../../services/usersService';

const COMISIONES = ['A', 'B'];

const ModificarDocente = () => {
  const { id } = useParams();
  const navigate = useNavigate();

  const [teacher, setTeacher] = useState(null);
  const [user, setUser] = useState(null);
  const [editingTeacher, setEditingTeacher] = useState({
    fullName: '',
    email: '',
  });
  const [subjects, setSubjects] = useState([]);
  const [assignments, setAssignments] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [notice, setNotice] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    const loadTeacherAndSubjects = async () => {
      try {
        setIsLoading(true);
        setError('');

        const [teacherData, userData, allSubjects] = await Promise.all([
          consultarMateriasDocente(id),
          usersService.obtenerPorId(id),
          subjectService.obtenerTodos(),
        ]);

        setTeacher(teacherData);
        setUser(userData);
        setEditingTeacher({
          fullName: `${teacherData.firstName} ${teacherData.lastName}`.trim(),
          email: teacherData.email,
        });
        setSubjects(allSubjects);
        setAssignments(
          teacherData.assignments.map((assignment) => ({
            subjectId: assignment.subjectId,
            section: assignment.section,
          }))
        );
      } catch (loadError) {
        console.error(loadError);
        setError(
          loadError instanceof Error
            ? `No se pudieron cargar los datos del docente. ${loadError.message}`
            : 'No se pudieron cargar los datos del docente.'
        );
      } finally {
        setIsLoading(false);
      }
    };

    loadTeacherAndSubjects();
  }, [id]);

  const toggleSection = (subjectId, section) => {
    setAssignments((current) => {
      const isAssigned = current.some(
        (assignment) =>
          assignment.subjectId === subjectId && assignment.section === section
      );

      if (isAssigned) {
        return current.filter(
          (assignment) =>
            assignment.subjectId !== subjectId || assignment.section !== section
        );
      }

      return [...current, { subjectId, section }];
    });
    setNotice('');
    setError('');
  };

  const handleTeacherChange = (event) => {
    const { name, value } = event.target;
    setEditingTeacher((current) => ({ ...current, [name]: value }));
    setNotice('');
    setError('');
  };

  const handleSave = async (event) => {
    event.preventDefault();
    setError('');
    setNotice('');

    const nameParts = editingTeacher.fullName.trim().split(/\s+/);
    if (nameParts.length < 2) {
      setError('Ingresá el nombre y el apellido del docente.');
      return;
    }

    if (!editingTeacher.email.trim() || !editingTeacher.email.includes('@')) {
      setError('Ingresá un correo válido.');
      return;
    }

    let profileUpdated = false;
    try {
      setIsSaving(true);
      await usersService.actualizar(id, {
        firstName: nameParts[0],
        lastName: nameParts.slice(1).join(' '),
        cuil: user.cuil,
        birthDate: user.birthDate,
        address: user.address,
        roleId: user.roleId,
        email: editingTeacher.email.trim(),
        phone: user.phone,
        emergencyContactPhone: user.emergencyContactPhone,
        password: null,
        isActive: user.isActive,
      });
      profileUpdated = true;

      const result = await guardarMateriasDocente(id, assignments);
      setTeacher((current) => ({
        ...current,
        firstName: nameParts[0],
        lastName: nameParts.slice(1).join(' '),
        email: editingTeacher.email.trim(),
      }));
      setNotice(result.message);
    } catch (saveError) {
      console.error(saveError);
      setError(
        profileUpdated
          ? `Se actualizaron los datos personales, pero no se pudieron guardar las materias. ${
              saveError instanceof Error ? saveError.message : ''
            }`
          : `No se pudieron guardar los datos del docente. ${
              saveError instanceof Error ? saveError.message : ''
            }`
      );
    } finally {
      setIsSaving(false);
    }
  };

  if (isLoading) {
    return (
      <section className="contenedor">
        <p role="status">Cargando datos del docente...</p>
      </section>
    );
  }

  if (!teacher) {
    return (
      <section className="contenedor">
        <p role="alert">{error || 'No se encontró el docente.'}</p>
        <button type="button" onClick={() => navigate('/secretario/docentes')}>
          Volver a docentes
        </button>
      </section>
    );
  }

  return (
    <div className="contenedor">
      <div className="cab">
        <div className="logo">
          <img src={FileText} alt="" />
        </div>
        <div className="space" />
        <h1>Modificar docente</h1>
      </div>

      <hr />

      <form onSubmit={handleSave}>
        <div className="contenido">
          <div className="ladoIzq">
            <div className="student-edit-form">
              <h2>Datos del docente</h2>
              <div>
                <label htmlFor="nombreCompleto">Nombre completo</label>
                <input
                  className="date"
                  type="text"
                  id="nombreCompleto"
                  name="fullName"
                  value={editingTeacher.fullName}
                  onChange={handleTeacherChange}
                  required
                />
              </div>
              <div>
                <label htmlFor="correoDocente">Correo</label>
                <input
                  className="date"
                  type="email"
                  id="correoDocente"
                  name="email"
                  value={editingTeacher.email}
                  onChange={handleTeacherChange}
                  required
                />
              </div>
            </div>
          </div>

          <div className="ladoDer">
            <h2>Seleccionar materias y comisiones</h2>
            <div className="student-edit-form1 materias-docente">
  {subjects.length > 0 ? (
    subjects.map((subject, index) => (
      <div
        key={subject.id}
        className={`materia-docente ${index % 2 === 0 ? 'contV' : 'contB'}`}
      >
        <strong className="materia-docente__nombre">{subject.name}</strong>

        <div className="materia-docente__comisiones">
          {COMISIONES.map((section) => (
            <label className="materia-docente__comision" key={section}>
              <input
                type="checkbox"
                checked={assignments.some(
                  (assignment) =>
                    assignment.subjectId === subject.id &&
                    assignment.section === section
                )}
                onChange={() => toggleSection(subject.id, section)}
              />
              Comisión {section}
            </label>
          ))}
        </div>
      </div>
    ))
  ) : (
    <p>No hay materias cargadas en la base de datos.</p>
  )}
</div>
          </div>
        </div>
        
        <br />

        {notice && (
          <p className="crud-notice" role="status">
            {notice}
          </p>
        )}
        {error && (
          <p className="error" role="alert">
            {error}
          </p>
        )}

        <br />

        <div className="student-edit-actions">
          <button
            className="cancel"
            type="button"
            onClick={() => navigate('/secretario/docentes')}
          >
            Cancelar
          </button>
          <div className="space" />
          <button className="save" type="submit" disabled={isSaving}>
            {isSaving ? 'Guardando...' : 'Guardar cambios'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default ModificarDocente;
