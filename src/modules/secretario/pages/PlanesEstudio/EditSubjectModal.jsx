import { useState } from 'react';
import { FileText, X } from 'lucide-react';
import '../../../../styles/modal.css';

const FORMATOS = ['Asignatura', 'Taller'];
const ANIOS = ['1°', '2°', '3°'];
const REGIMENES = ['Anual', 'Cuatrimestral'];

function EditSubjectModal({ subject, onClose, onSave }) {
  const [form, setForm] = useState({
    orden: subject.orden,
    materia: subject.materia,
    formato: subject.formato,
    regimen: subject.regimen,
    anio: subject.anio,
    horasCatedra: subject.horasCatedra,
    horasReloj: subject.horasReloj,
  });
  const [errors, setErrors] = useState({});

  const handleChange = (field) => (event) => {
    setForm((current) => ({ ...current, [field]: event.target.value }));
  };

  const validate = () => {
    const newErrors = {};
    if (!String(form.orden).trim()) newErrors.orden = 'Este campo es obligatorio';
    if (!form.materia.trim()) newErrors.materia = 'Este campo es obligatorio';
    if (!form.formato) newErrors.formato = 'Este campo es obligatorio';
    if (!form.regimen) newErrors.regimen = 'Este campo es obligatorio';
    if (!form.anio) newErrors.anio = 'Este campo es obligatorio';
    if (!String(form.horasCatedra).trim()) newErrors.horasCatedra = 'Este campo es obligatorio';
    if (!String(form.horasReloj).trim()) newErrors.horasReloj = 'Este campo es obligatorio';
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    if (!validate()) return;
    onSave({ ...subject, ...form, materia: form.materia.trim() });
  };

  const errorClass = (field) => (errors[field] ? 'input-error' : '');

  return (
    <div className="modal-overlay" role="dialog" aria-modal="true" aria-labelledby="edit-subject-title">
      <div className="modal-content">
        <div className="modal-header">
          <div className="modal-header-title">
            <FileText size={20} />
            <div>
              <h2 id="edit-subject-title">Editar materia</h2>
              <p>Modifica la información de la materia seleccionada.</p>
            </div>
          </div>
          <button type="button" aria-label="Cerrar" onClick={onClose}><X size={20} /></button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="modal-row">
            <div className="modal-field">
              <label htmlFor="edit-orden">N.º Orden *</label>
              <input id="edit-orden" type="number" min="1" value={form.orden} onChange={handleChange('orden')} className={errorClass('orden')} />
              {errors.orden && <span className="field-error">{errors.orden}</span>}
            </div>
            <div className="modal-field">
              <label htmlFor="edit-materia">Materia *</label>
              <input id="edit-materia" type="text" value={form.materia} onChange={handleChange('materia')} className={errorClass('materia')} />
              {errors.materia && <span className="field-error">{errors.materia}</span>}
            </div>
          </div>

          <div className="modal-field" style={{ marginBottom: 16 }}>
            <label htmlFor="edit-formato">Formato *</label>
            <select id="edit-formato" value={form.formato} onChange={handleChange('formato')} className={errorClass('formato')}>
              <option value="" />
              {FORMATOS.map((option) => <option key={option} value={option}>{option}</option>)}
            </select>
            {errors.formato && <span className="field-error">{errors.formato}</span>}
          </div>

          <div className="modal-row">
            <div className="modal-field">
              <label htmlFor="edit-regimen">Régimen *</label>
              <select id="edit-regimen" value={form.regimen} onChange={handleChange('regimen')} className={errorClass('regimen')}>
                <option value="" />
                {REGIMENES.map((option) => <option key={option} value={option}>{option}</option>)}
              </select>
              {errors.regimen && <span className="field-error">{errors.regimen}</span>}
            </div>
            <div className="modal-field">
              <label htmlFor="edit-anio">Año *</label>
              <select id="edit-anio" value={form.anio} onChange={handleChange('anio')} className={errorClass('anio')}>
                <option value="" />
                {ANIOS.map((option) => <option key={option} value={option}>{option}</option>)}
              </select>
              {errors.anio && <span className="field-error">{errors.anio}</span>}
            </div>
          </div>

          <div className="modal-row">
            <div className="modal-field">
              <label htmlFor="edit-horas-catedra">Hs. cátedra semanales *</label>
              <input id="edit-horas-catedra" type="number" min="1" value={form.horasCatedra} onChange={handleChange('horasCatedra')} className={errorClass('horasCatedra')} />
              {errors.horasCatedra && <span className="field-error">{errors.horasCatedra}</span>}
            </div>
            <div className="modal-field">
              <label htmlFor="edit-horas-reloj">Hs. reloj *</label>
              <input id="edit-horas-reloj" type="number" min="1" value={form.horasReloj} onChange={handleChange('horasReloj')} className={errorClass('horasReloj')} />
              {errors.horasReloj && <span className="field-error">{errors.horasReloj}</span>}
            </div>
          </div>

          <div className="modal-footer">
            <button type="button" className="modal-cancel-button" onClick={onClose}>Cancelar</button>
            <button type="submit" className="modal-submit-button">Guardar cambios</button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default EditSubjectModal;