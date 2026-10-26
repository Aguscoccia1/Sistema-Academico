import { useState } from 'react';
import { ChevronRight, FileText, Pencil, Plus, Trash2 } from 'lucide-react';
import CargarMaterias from './LoadSubjects';
import EditSubjectModal from './EditSubjectModal';
import ConfirmCorrelativesModal from './ConfirmCorrelativesModal';

const tabs = ['Resumen', 'Materias', 'Materias por año', 'Correlatividades'];
const YEARS = ['1°', '2°', '3°'];
const CORRELATIVE_YEARS = ['2°', '3°'];

const yearLabel = (year) => year.replace('°', '.° Año');
const sortByOrder = (a, b) => Number(a.orden) - Number(b.orden);

function Field({ label, value }) {
  return (
    <div className="plan-detalle-field">
      <span className="field-label">{label}</span>
      <span className="field-value">{value ?? '—'}</span>
    </div>
  );
}

function PlanDetalle({ plan, onBack }) {
  const [activeTab, setActiveTab] = useState('Resumen');
  const [isLoadingSubjects, setIsLoadingSubjects] = useState(false);
  const [materias, setMaterias] = useState([]);
  const [selectedYear, setSelectedYear] = useState('1°');
  const [editingSubject, setEditingSubject] = useState(null);

  // Correlatividades: { [idMateria]: [idsCorrelativas] }
  const [correlatives, setCorrelatives] = useState({});
  const [draftCorrelatives, setDraftCorrelatives] = useState({});
  const [correlativeYear, setCorrelativeYear] = useState('2°');
  const [selectedSubjectId, setSelectedSubjectId] = useState(null);
  const [correlativeNotice, setCorrelativeNotice] = useState('');
  const [isConfirmingCorrelatives, setIsConfirmingCorrelatives] = useState(false);

  const handleSaveSubjects = (newSubjects) => {
    setMaterias((current) => [...current, ...newSubjects]);
    setIsLoadingSubjects(false);
  };

  const handleUpdateSubject = (updatedSubject) => {
    setMaterias((current) => current.map((materia) => (materia.id === updatedSubject.id ? updatedSubject : materia)));
    setEditingSubject(null);
  };

  const handleDeleteSubject = (subject) => {
    if (!window.confirm(`¿Eliminar la materia "${subject.materia}"?`)) return;
    setMaterias((current) => current.filter((materia) => materia.id !== subject.id));

    // Se limpian las correlativas que apuntaban a la materia eliminada
    const cleanUp = (source) => {
      const result = {};
      Object.entries(source).forEach(([id, list]) => {
        if (String(id) === String(subject.id)) return;
        result[id] = list.filter((correlativeId) => correlativeId !== subject.id);
      });
      return result;
    };
    setCorrelatives(cleanUp);
    setDraftCorrelatives(cleanUp);
    if (selectedSubjectId === subject.id) setSelectedSubjectId(null);
  };

  const handleTabChange = (tab) => {
    setActiveTab(tab);
    if (tab === 'Correlatividades') {
      setDraftCorrelatives(correlatives);
      setCorrelativeNotice('');
    }
  };

  const toggleCorrelative = (correlativeId) => {
    setDraftCorrelatives((current) => {
      const currentList = current[selectedSubjectId] ?? [];
      const nextList = currentList.includes(correlativeId)
        ? currentList.filter((id) => id !== correlativeId)
        : [...currentList, correlativeId];
      return { ...current, [selectedSubjectId]: nextList };
    });
  };

  const requestSaveCorrelatives = () => {
    if (!selectedSubjectId) {
      setCorrelativeNotice('Seleccioná una materia para guardar sus correlatividades.');
      return;
    }
    setCorrelativeNotice('');
    setIsConfirmingCorrelatives(true);
  };

  const confirmSaveCorrelatives = () => {
    setCorrelatives(draftCorrelatives);
    setIsConfirmingCorrelatives(false);
    setCorrelativeNotice('Las correlatividades se guardaron correctamente.');
  };

  const cancelCorrelatives = () => {
    setDraftCorrelatives(correlatives);
    setSelectedSubjectId(null);
    setCorrelativeNotice('');
  };

  if (isLoadingSubjects) {
    return (
      <CargarMaterias
        plan={plan}
        onBack={onBack}
        onCancel={() => setIsLoadingSubjects(false)}
        onSave={handleSaveSubjects}
      />
    );
  }

  const renderCorrelatives = () => {
    const countByYear = (year) => materias.filter((materia) => materia.anio === year).length;
    const subjectsOfYear = materias.filter((materia) => materia.anio === correlativeYear).sort(sortByOrder);
    const selectedSubject = materias.find((materia) => materia.id === selectedSubjectId) ?? null;
    const selectedList = draftCorrelatives[selectedSubjectId] ?? [];

    // Solo se puede ser correlativa de una materia de un año anterior
    const previousYears = YEARS.slice(0, YEARS.indexOf(correlativeYear));

    return (
      <div className="plan-materias">
        <h2>Correlatividades</h2>

        <div className="year-tabs correlative-year-tabs">
          {CORRELATIVE_YEARS.map((year) => {
            const count = countByYear(year);
            return (
              <button
                key={year}
                type="button"
                className={`year-tab ${correlativeYear === year ? 'active' : ''}`}
                onClick={() => {
                  setCorrelativeYear(year);
                  setSelectedSubjectId(null);
                }}
              >
                <strong>{yearLabel(year)}</strong>
                <span>{count} {count === 1 ? 'materia' : 'materias'}</span>
              </button>
            );
          })}
        </div>

        <h3 className="correlative-title">Materias de {correlativeYear.replace('°', '.° año')}</h3>
        <p className="correlative-subtitle">Seleccioná una materia y definí sus correlativas.</p>

        {subjectsOfYear.length === 0 ? (
          <p className="year-subjects-empty">No hay materias cargadas para este año.</p>
        ) : (
          <div className="correlative-subjects">
            {subjectsOfYear.map((materia) => (
              <button
                key={materia.id}
                type="button"
                className={`correlative-subject ${selectedSubjectId === materia.id ? 'selected' : ''}`}
                onClick={() => setSelectedSubjectId(materia.id)}
              >
                <span className="correlative-number">{materia.orden}</span>
                <span className="correlative-name">{materia.materia}</span>
              </button>
            ))}
          </div>
        )}

        {selectedSubject && (
          <>
            <h3 className="correlative-title">Correlativas disponibles</h3>
            <p className="correlative-subtitle">
              Selecciona las materias que son correlativas de {selectedSubject.orden} · {selectedSubject.materia}.
            </p>

            {previousYears.map((year) => {
              const available = materias.filter((materia) => materia.anio === year).sort(sortByOrder);
              return (
                <div key={year} className="correlative-group">
                  <span className="correlative-group-title">{yearLabel(year)}</span>
                  {available.length === 0 ? (
                    <p className="year-subjects-empty">No hay materias cargadas para este año.</p>
                  ) : (
                    <div className="correlative-options">
                      {available.map((materia) => (
                        <button
                          key={materia.id}
                          type="button"
                          className={`correlative-option ${selectedList.includes(materia.id) ? 'checked' : ''}`}
                          aria-pressed={selectedList.includes(materia.id)}
                          onClick={() => toggleCorrelative(materia.id)}
                        >
                          <span className="correlative-number">{materia.orden}</span>
                          <span className="correlative-name">{materia.materia}</span>
                        </button>
                      ))}
                    </div>
                  )}
                </div>
              );
            })}
          </>
        )}

        {correlativeNotice && <p className="correlative-notice" role="status">{correlativeNotice}</p>}

        <div className="correlative-footer">
          <button type="button" className="plan-detalle-button" onClick={cancelCorrelatives}>Cancelar cambios</button>
          <button type="button" className="create-plan-button" onClick={requestSaveCorrelatives}>Guardar cambios</button>
        </div>
      </div>
    );
  };

  const renderContent = () => {
    if (activeTab === 'Resumen') {
      return (
        <div className="plan-detalle-card">
          <h2>Información del Plan</h2>
          <div className="plan-detalle-grid">
            <Field label="Nombre del plan" value={plan.nombre} />
            <Field label="Código" value={plan.codigo} />
            <Field label="Año de inicio del plan de estudio" value={plan.añoInicio} />
            <Field label="Año de finalización del plan de estudio" value={plan.añoFin ?? 'Sin definir'} />
            <Field label="Duración de la carrera" value={plan.duracion} />
            <Field label="Modalidad de la carrera" value={plan.modalidad} />
            <Field label="Carga horaria total" value={plan.cargaHoraria} />
            <Field label="Régimen de cursada" value={plan.regimen} />
          </div>
        </div>
      );
    }

    if (activeTab === 'Materias') {
      return (
        <div className="plan-materias">
          <div className="plan-materias-header">
            <div>
              <h2>Materias</h2>
              <p>Gestioná las materias asociadas al plan de estudio.</p>
            </div>
            {materias.length > 0 && (
              <button type="button" className="create-plan-button" onClick={() => setIsLoadingSubjects(true)}>
                <Plus size={14} /> Agregar materias
              </button>
            )}
          </div>

          {materias.length === 0 ? (
            <div className="plan-materias-empty">
              <div className="plan-materias-icon"><FileText size={32} /></div>
              <h3>Todavía no hay materias cargadas</h3>
              <p>Agrega las materias para comenzar a organizar la estructura académica.</p>
              <button type="button" className="create-plan-button" onClick={() => setIsLoadingSubjects(true)}>
                <Plus size={14} /> Agregar materias
              </button>
            </div>
          ) : (
            <div className="plan-materias-list">
              <div className="plan-materias-list-row plan-materias-list-head">
                <span>N.º</span>
                <span>Formato</span>
                <span>Materia</span>
                <span>Año</span>
                <span>Régimen</span>
                <span>Hs. cátedra</span>
                <span>Hs. reloj</span>
                <span>Acciones</span>
              </div>
              {[...materias].sort(sortByOrder).map((materia) => (
                <div className="plan-materias-list-row" key={materia.id}>
                  <span>{materia.orden}</span>
                  <span>{materia.formato}</span>
                  <span>{materia.materia}</span>
                  <span>{materia.anio}</span>
                  <span>{materia.regimen}</span>
                  <span>{materia.horasCatedra}</span>
                  <span>{materia.horasReloj}</span>
                  <span className="plan-materias-actions">
                    <button type="button" className="subject-edit-button" aria-label={`Editar ${materia.materia}`} onClick={() => setEditingSubject(materia)}>
                      <Pencil size={16} />
                    </button>
                    <button type="button" className="subject-delete-button" aria-label={`Eliminar ${materia.materia}`} onClick={() => handleDeleteSubject(materia)}>
                      <Trash2 size={16} />
                    </button>
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>
      );
    }

    if (activeTab === 'Materias por año') {
      const countByYear = (year) => materias.filter((materia) => materia.anio === year).length;
      const subjectsOfYear = materias.filter((materia) => materia.anio === selectedYear).sort(sortByOrder);

      return (
        <div className="plan-materias">
          <h2>Materias por año</h2>
          <p className="plan-materias-subtitle">Visualiza las materias que conforman cada año del plan de estudio.</p>

          <div className="year-tabs">
            {YEARS.map((year) => {
              const count = countByYear(year);
              return (
                <button
                  key={year}
                  type="button"
                  className={`year-tab ${selectedYear === year ? 'active' : ''}`}
                  onClick={() => setSelectedYear(year)}
                >
                  <strong>{yearLabel(year)}</strong>
                  <span>{count} {count === 1 ? 'materia' : 'materias'}</span>
                </button>
              );
            })}
          </div>

          <div className="year-subjects">
            <div className="year-subjects-row year-subjects-head">
              <span>N.º orden</span>
              <span>Formato</span>
              <span>Materia</span>
              <span>Régimen</span>
            </div>
            {subjectsOfYear.length === 0 ? (
              <p className="year-subjects-empty">No hay materias cargadas para este año.</p>
            ) : (
              subjectsOfYear.map((materia) => (
                <div className="year-subjects-row" key={materia.id}>
                  <span>{materia.orden}</span>
                  <span>{materia.formato}</span>
                  <span>{materia.materia}</span>
                  <span>{materia.regimen}</span>
                </div>
              ))
            )}
          </div>
        </div>
      );
    }

    return renderCorrelatives();
  };

  return (
    <section className="plan-detalle-page">
      <nav className="plan-detalle-breadcrumb" aria-label="Breadcrumb">
        <button type="button" onClick={onBack}>Planes de Estudio</button>
        <ChevronRight size={14} />
        <span>{plan.codigo} – {plan.nombre}</span>
      </nav>

      <div className="plan-detalle-heading">
        <div className="plan-detalle-title">
          <h1>{plan.codigo} – {plan.nombre}</h1>
          <span className={`plan-badge ${plan.activo ? 'plan-badge-active' : 'plan-badge-inactive'}`}>
            {plan.activo ? 'Activo' : 'Archivado'}
          </span>
        </div>
        {activeTab === 'Resumen' && (
          <div className="plan-detalle-actions">
            <button type="button" className="plan-detalle-button">Editar</button>
            <button type="button" className="plan-detalle-button">Archivar</button>
          </div>
        )}
      </div>

      <div className="plan-detalle-tabs" role="tablist">
        {tabs.map((tab) => (
          <button
            key={tab}
            type="button"
            role="tab"
            aria-selected={activeTab === tab}
            className={activeTab === tab ? 'active' : ''}
            onClick={() => handleTabChange(tab)}
          >
            {tab}
          </button>
        ))}
      </div>

      {renderContent()}

      {editingSubject && (
        <EditSubjectModal
          subject={editingSubject}
          onClose={() => setEditingSubject(null)}
          onSave={handleUpdateSubject}
        />
      )}

      {isConfirmingCorrelatives && (
        <ConfirmCorrelativesModal
          subject={materias.find((materia) => materia.id === selectedSubjectId)}
          correlatives={(draftCorrelatives[selectedSubjectId] ?? [])
            .map((id) => materias.find((materia) => materia.id === id))
            .filter(Boolean)
            .sort(sortByOrder)}
          onClose={() => setIsConfirmingCorrelatives(false)}
          onConfirm={confirmSaveCorrelatives}
        />
      )}
    </section>
  );
}

export default PlanDetalle;