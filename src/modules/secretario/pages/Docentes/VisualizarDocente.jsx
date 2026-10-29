import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import '../../../../styles/visualizarEst.css';
import FileText from '../../../../assets/FileText.svg';
import { consultarMateriasDocente } from '../../../../services/secretario/teacherAdministrationService';
import { usersService } from '../../../../services/usersService';
import SubjectScheduleList from '../../components/students/SubjectScheduleList';

const VisualizarDocente = () => {
  const { id } = useParams();
  const navigate = useNavigate();

  const [teacher, setTeacher] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    let cancelled = false;

    const loadTeacher = async () => {
      try {
        setIsLoading(true);
        setError('');

        const [teacherData, userData] = await Promise.all([
          consultarMateriasDocente(id),
          usersService.obtenerPorId(id),
        ]);

        if (cancelled) return;

        const assignmentsBySubject = teacherData.assignments.reduce(
          (grouped, assignment) => {
            const existing = grouped.find(
              (subject) => subject.subjectId === assignment.subjectId
            );

            if (existing) {
              existing.sections.push(assignment.section);
            } else {
              grouped.push({
                subjectId: assignment.subjectId,
                subjectName: assignment.subjectName,
                sections: [assignment.section],
              });
            }

            return grouped;
          },
          []
        );

        setTeacher({
          fullName: `${teacherData.firstName} ${teacherData.lastName}`.trim(),
          cuil: userData.cuil,
          email: teacherData.email,
          sections: [
            ...new Set(teacherData.assignments.map((item) => item.section)),
          ].join(', '),
          status: userData.isActive ? 'Activo' : 'Inactivo',
          subjects: assignmentsBySubject,
        });
      } catch (loadError) {
        console.error(loadError);
        if (!cancelled) {
          setError(
            loadError instanceof Error
              ? `No se pudo consultar el docente. ${loadError.message}`
              : 'No se pudo consultar el docente.'
          );
        }
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    };

    loadTeacher();

    return () => {
      cancelled = true;
    };
  }, [id]);

  if (isLoading) {
    return (
      <section className="contenedor">
        <p role="status">Cargando docente...</p>
      </section>
    );
  }

  if (!teacher) {
    return (
      <section className="contenedor">
        <p role="alert">{error || 'Docente no encontrado.'}</p>
        <button
          className="cancel"
          type="button"
          onClick={() => navigate('/secretario/docentes')}
        >
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
        <h1>Visualizar docente</h1>
      </div>

      <hr />

      <div className="contenido">
        <div className="ladoIzq">
          <h2>Datos personales</h2>
          <div className="student-edit-form">
            <div>
              <label>Nombre completo</label>
              <input
                className="date"
                type="text"
                value={teacher.fullName}
                readOnly
              />
            </div>
            <div>
              <label>CUIL</label>
              <input
                className="date"
                type="text"
                value={teacher.cuil ?? ''}
                readOnly
              />
            </div>
            <div>
              <label>Correo</label>
              <input
                className="date"
                type="email"
                value={teacher.email}
                readOnly
              />
            </div>
            <div>
              <label>Comisiones asignadas</label>
              <input
                className="date"
                type="text"
                value={teacher.sections || 'Sin comisión'}
                readOnly
              />
            </div>
            <div>
              <label>Estado</label>
              <input
                className="date"
                type="text"
                value={teacher.status}
                readOnly
              />
            </div>
          </div>
        </div>

        <div className="ladoDer">
          <h2>Materias y horarios</h2>
          <div className="student-edit-form1">
            {teacher.subjects.length > 0 ? (
              teacher.subjects.map((subject, index) => (
                <div
                  key={subject.subjectId}
                  className={index % 2 === 0 ? 'contV' : 'contB'}
                >
                  <label>{subject.subjectName}</label>
                  {subject.sections.map((section) => (
                    <SubjectScheduleList
                      key={`${subject.subjectId}-${section}`}
                      subjectId={subject.subjectId}
                      section={section}
                      subjectName={subject.subjectName}
                      readOnly
                    />
                  ))}
                </div>
              ))
            ) : (
              <p>El docente todavía no tiene materias asignadas.</p>
            )}
          </div>
        </div>
      </div>

      <br />
      <div className="student-edit-actions">
        <button
          className="cancel"
          type="button"
          onClick={() => navigate('/secretario/docentes')}
        >
          Volver
        </button>
      </div>
    </div>
  );
};

export default VisualizarDocente;
