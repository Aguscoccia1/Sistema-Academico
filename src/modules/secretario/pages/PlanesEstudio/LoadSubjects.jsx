import { useRef, useState } from 'react';
import { ChevronRight, Plus, Trash2 } from 'lucide-react';

const FORMATOS = ['Asignatura', 'Taller'];
const ANIOS = ['1°', '2°', '3°'];
const REGIMENES = ['Anual', 'Cuatrimestral'];
const INITIAL_ROWS = 5;

function CargarMaterias({ plan, onBack, onCancel, onSave }) {
  const nextId = useRef(INITIAL_ROWS);
  const createRow = (id) => ({
    id,
    orden: '',
    formato: '',
    materia: '',
    anio: '',
    regimen: '',
    horasCatedra: '',
    horasReloj: '',
  });

  const [rows, setRows] = useState(() => Array.from({ length: INITIAL_ROWS }, (_, i) => createRow(i)));
  const [error, setError] = useState('');

  const updateRow = (id, field, value) => {
    setRows((current) => current.map((row) => (row.id === id ? { ...row, [field]: value } : row)));
  };

  const addRow = () => {
    nextId.current += 1;
    setRows((current) => [...current, createRow(nextId.current)]);
  };

  const removeRow = (id) => {
    setRows((current) => current.filter((row) => row.id !== id));
  };

  const handleSubmit = () => {
    const filledRows = rows.filter((row) => Object.entries(row).some(([key, value]) => key !== 'id' && value !== ''));

    if (filledRows.length === 0) {
      setError('Completá al menos una materia.');
      return;
    }

    const incomplete = filledRows.some((row) => !row.orden || !row.formato || !row.materia.trim() || !row.anio || !row.regimen || !row.horasCatedra || !row.horasReloj);
    if (incomplete) {
      setError('Completá todos los campos de cada fila (o eliminá las filas que no uses).');
      return;
    }

    onSave(filledRows.map((row) => ({ ...row, materia: row.materia.trim() })));
  };

  const renderSelect = (row, field, options) => (
    <select value={row[field]} onChange={(event) => updateRow(row.id, field, event.target.value)} aria-label={field}>
      <option value="" />
      {options.map((option) => <option key={option} value={option}>{option}</option>)}
    </select>
  );

  return (
    <section className="plan-detalle-page">
      <nav className="plan-detalle-breadcrumb" aria-label="Breadcrumb">
        <button type="button" onClick={onBack}>Planes de Estudio</button>
        <ChevronRight size={14} />
        <span>{plan.codigo} – {plan.nombre}</span>
      </nav>

      <div className="cargar-materias-heading">
        <h1>Cargar materias</h1>
        <p>Completá los datos de las materias que forman parte del plan de estudio.</p>
      </div>

      <div className="cargar-materias-scroll">
        <div className="cargar-materias-table">
          <div className="cargar-materias-row cargar-materias-head">
            <span>N.º orden</span>
            <span>Formato</span>
            <span>Materia</span>
            <span>Año</span>
            <span>Régimen</span>
            <span>Hs. cátedra semanales</span>
            <span>Hs. reloj totales</span>
            <span>Acciones</span>
          </div>

          {rows.map((row) => (
            <div className="cargar-materias-row" key={row.id}>
              <input type="number" min="1" placeholder="Ej: 1" aria-label="N.º orden" value={row.orden} onChange={(event) => updateRow(row.id, 'orden', event.target.value)} />
              {renderSelect(row, 'formato', FORMATOS)}
              <input type="text" placeholder="Ej: Programación I" aria-label="Materia" value={row.materia} onChange={(event) => updateRow(row.id, 'materia', event.target.value)} />
              {renderSelect(row, 'anio', ANIOS)}
              {renderSelect(row, 'regimen', REGIMENES)}
              <input type="number" min="1" placeholder="Ej: 4" aria-label="Horas cátedra semanales" value={row.horasCatedra} onChange={(event) => updateRow(row.id, 'horasCatedra', event.target.value)} />
              <input type="number" min="1" placeholder="Ej: 1060" aria-label="Horas reloj totales" value={row.horasReloj} onChange={(event) => updateRow(row.id, 'horasReloj', event.target.value)} />
              <button type="button" className="cargar-materias-delete" aria-label="Eliminar fila" onClick={() => removeRow(row.id)}>
                <Trash2 size={16} />
              </button>
            </div>
          ))}
        </div>
      </div>

      <button type="button" className="plan-outline-button cargar-materias-add" onClick={addRow}>
        <Plus size={12} /> Agregar fila
      </button>

      {error && <p className="field-error cargar-materias-error" role="alert">{error}</p>}

      <div className="cargar-materias-footer">
        <button type="button" className="plan-detalle-button" onClick={onCancel}>Cancelar</button>
        <button type="button" className="create-plan-button" onClick={handleSubmit}>Agregar materias</button>
      </div>
    </section>
  );
}

export default CargarMaterias;