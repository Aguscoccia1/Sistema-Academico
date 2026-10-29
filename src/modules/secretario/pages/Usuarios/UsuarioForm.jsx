import { useState } from 'react';
import { usersService } from '../../../../services/usersService';

const VACIO = {
  firstName: '',
  lastName: '',
  cuil: '',
  birthDate: '',
  address: '',
  phone: '',
  emergencyContactPhone: '',
  email: '',
  roleId: '',
  password: '',
  isActive: true,
};

function valoresIniciales(usuario) {
  if (!usuario) return VACIO;
  return {
    firstName: usuario.firstName ?? '',
    lastName: usuario.lastName ?? '',
    cuil: usuario.cuil ?? '',
    birthDate: usuario.birthDate ?? '',
    address: usuario.address ?? '',
    phone: usuario.phone ?? '',
    emergencyContactPhone: usuario.emergencyContactPhone ?? '',
    email: usuario.email ?? '',
    roleId: String(usuario.roleId),
    password: '',
    isActive: usuario.isActive,
  };
}

function validar(valores, esEdicion) {
  const errores = {};
  if (!valores.firstName.trim()) errores.firstName = 'Ingresá el nombre.';
  if (!valores.lastName.trim()) errores.lastName = 'Ingresá el apellido.';
  if (!/^\d{11}$/.test(valores.cuil.trim())) errores.cuil = 'El CUIL debe tener 11 dígitos, sin guiones.';
  if (!valores.email.trim() || !valores.email.includes('@')) errores.email = 'Ingresá un correo válido.';
  if (!valores.roleId) errores.roleId = 'Seleccioná un rol.';
  if (!esEdicion && !valores.password) errores.password = 'Ingresá una contraseña.';
  return errores;
}

function Campo({ id, etiqueta, error, ayuda, children }) {
  return (
    <div className="usuarios-campo">
      <label htmlFor={id}>{etiqueta}</label>
      {children}
      {ayuda && <small className="usuarios-ayuda">{ayuda}</small>}
      {error && <small className="usuarios-error" role="alert">{error}</small>}
    </div>
  );
}

function UsuarioForm({ usuario, roles, onCancelar, onGuardado }) {
  const esEdicion = Boolean(usuario);
  const [valores, setValores] = useState(() => valoresIniciales(usuario));
  const [errores, setErrores] = useState({});
  const [errorApi, setErrorApi] = useState('');
  const [guardando, setGuardando] = useState(false);

  const cambiar = (campo) => (evento) => {
    const valor = evento.target.type === 'checkbox' ? evento.target.checked : evento.target.value;
    setValores((actuales) => ({ ...actuales, [campo]: valor }));
  };

  const guardar = async (evento) => {
    evento.preventDefault();
    setErrorApi('');
    const nuevosErrores = validar(valores, esEdicion);
    setErrores(nuevosErrores);
    if (Object.keys(nuevosErrores).length > 0) return;

    const payload = {
      firstName: valores.firstName.trim(),
      lastName: valores.lastName.trim(),
      cuil: valores.cuil.trim(),
      birthDate: valores.birthDate || null,
      address: valores.address.trim() || null,
      roleId: Number(valores.roleId),
      email: valores.email.trim(),
      phone: valores.phone.trim() || null,
      emergencyContactPhone: valores.emergencyContactPhone.trim() || null,
      password: valores.password || null,
      isActive: valores.isActive,
    };

    setGuardando(true);
    try {
      if (esEdicion) await usersService.actualizar(usuario.id, payload);
      else await usersService.crear(payload);
      onGuardado(esEdicion ? 'Usuario actualizado correctamente.' : 'Usuario creado correctamente.');
    } catch (error) {
      setErrorApi(error instanceof Error ? error.message : 'No se pudo guardar el usuario.');
    } finally {
      setGuardando(false);
    }
  };

  return (
    <div className="usuarios-overlay" role="presentation">
      <form className="usuarios-modal" role="dialog" aria-modal="true" aria-labelledby="titulo-usuario" onSubmit={guardar} noValidate>
        <h2 id="titulo-usuario">{esEdicion ? 'Editar usuario' : 'Nuevo usuario'}</h2>
        <p className="usuarios-sub">Los campos con * son obligatorios.</p>

        {errorApi && <div className="usuarios-alerta" role="alert">{errorApi}</div>}

        <div className="usuarios-grid">
          <Campo id="firstName" etiqueta="Nombre *" error={errores.firstName}>
            <input id="firstName" type="text" value={valores.firstName} onChange={cambiar('firstName')} />
          </Campo>
          <Campo id="lastName" etiqueta="Apellido *" error={errores.lastName}>
            <input id="lastName" type="text" value={valores.lastName} onChange={cambiar('lastName')} />
          </Campo>
          <Campo id="cuil" etiqueta="CUIL * (11 dígitos)" error={errores.cuil}>
            <input id="cuil" type="text" inputMode="numeric" maxLength={11} placeholder="20123456789" value={valores.cuil} onChange={cambiar('cuil')} />
          </Campo>
          <Campo id="birthDate" etiqueta="Fecha de nacimiento">
            <input id="birthDate" type="date" value={valores.birthDate} onChange={cambiar('birthDate')} />
          </Campo>
          <Campo id="address" etiqueta="Domicilio">
            <input id="address" type="text" value={valores.address} onChange={cambiar('address')} />
          </Campo>
          <Campo id="phone" etiqueta="Teléfono">
            <input id="phone" type="text" value={valores.phone} onChange={cambiar('phone')} />
          </Campo>
          <Campo id="emergencyContactPhone" etiqueta="Teléfono de emergencia">
            <input id="emergencyContactPhone" type="text" value={valores.emergencyContactPhone} onChange={cambiar('emergencyContactPhone')} />
          </Campo>
          <Campo id="email" etiqueta="Correo *" error={errores.email}>
            <input id="email" type="text" placeholder="correo@dominio.edu" value={valores.email} onChange={cambiar('email')} />
          </Campo>
          <Campo id="roleId" etiqueta="Rol *" error={errores.roleId}>
            <select id="roleId" value={valores.roleId} onChange={cambiar('roleId')}>
              <option value="">Seleccioná un rol</option>
              {roles.map((rol) => (
                <option key={rol.id} value={rol.id}>{rol.name === 'Student' ? 'Estudiante' : rol.name === 'AcademicSecretary' ? 'Secretaría académica' : rol.name}</option>
              ))}
            </select>
          </Campo>
          <Campo id="password" etiqueta={esEdicion ? 'Contraseña' : 'Contraseña *'} error={errores.password} ayuda={esEdicion ? 'Dejala vacía para no cambiarla.' : null}>
            <input id="password" type="password" autoComplete="new-password" value={valores.password} onChange={cambiar('password')} />
          </Campo>
        </div>

        {esEdicion && (
          <label className="usuarios-activo">
            <input type="checkbox" checked={valores.isActive} onChange={cambiar('isActive')} />
            Usuario activo
          </label>
        )}

        <div className="usuarios-botones">
          <button type="button" className="usuarios-secundario" onClick={onCancelar} disabled={guardando}>Cancelar</button>
          <button type="submit" className="usuarios-primario" disabled={guardando}>{guardando ? 'Guardando...' : 'Guardar'}</button>
        </div>
      </form>
    </div>
  );
}

export default UsuarioForm;