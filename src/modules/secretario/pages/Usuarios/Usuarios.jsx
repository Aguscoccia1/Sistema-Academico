import { useCallback, useEffect, useMemo, useState } from 'react';
import { Plus, Users } from 'lucide-react';
import { usersService } from '../../../../services/usersService';
import { rolesService } from '../../../../services/rolesService';
import UsuarioForm from './UsuarioForm';

const NOMBRE_ROL = { Student: 'Estudiante', AcademicSecretary: 'Secretaría académica' };

function Usuarios() {
  const [usuarios, setUsuarios] = useState([]);
  const [roles, setRoles] = useState([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState('');
  const [aviso, setAviso] = useState('');
  const [busqueda, setBusqueda] = useState('');
  const [filtroRol, setFiltroRol] = useState('');
  const [filtroEstado, setFiltroEstado] = useState('');
  const [formulario, setFormulario] = useState(null); 
  const [aBajar, setABajar] = useState(null);
  const [errorBaja, setErrorBaja] = useState('');

  const cargar = useCallback(async () => {
    setCargando(true);
    setError('');
    try {
      const [listado, listadoRoles] = await Promise.all([usersService.obtenerTodos(), rolesService.obtenerTodos()]);
      setUsuarios(listado);
      setRoles(listadoRoles);
    } catch (loadError) {
      setError(loadError instanceof Error ? loadError.message : 'No se pudieron cargar los usuarios.');
    } finally {
      setCargando(false);
    }
  }, []);

  useEffect(() => {
    cargar();
  }, [cargar]);

  const filtrados = useMemo(() => {
    const texto = busqueda.trim().toLowerCase();
    return usuarios.filter((usuario) => {
      const coincideTexto =
        !texto || [usuario.firstName, usuario.lastName, usuario.email, usuario.cuil].some((valor) => String(valor ?? '').toLowerCase().includes(texto));
      const coincideRol = !filtroRol || String(usuario.roleId) === filtroRol;
      const coincideEstado = !filtroEstado || (filtroEstado === 'activo') === usuario.isActive;
      return coincideTexto && coincideRol && coincideEstado;
    });
  }, [usuarios, busqueda, filtroRol, filtroEstado]);

  const confirmarBaja = async () => {
    setErrorBaja('');
    try {
      await usersService.eliminar(aBajar.id);
      setAviso(`${aBajar.lastName}, ${aBajar.firstName} fue dado de baja.`);
      setABajar(null);
      cargar();
    } catch (bajaError) {
      setErrorBaja(bajaError instanceof Error ? bajaError.message : 'No se pudo dar de baja al usuario.');
    }
  };

  return (
    <section className="management-page">
      <div className="management-heading">
        <div>
          <span className="management-icon"><Users size={23} /></span>
          <div>
            <h1>Usuarios</h1>
            <p>Alta, edición y baja de los usuarios del sistema.</p>
          </div>
        </div>
        <button className="create-button" type="button" onClick={() => setFormulario({})}><Plus size={18} />Nuevo usuario</button>
      </div>

      {aviso && <div className="crud-notice">{aviso}<button onClick={() => setAviso('')} aria-label="Cerrar" type="button">×</button></div>}
      {error && <p role="alert">{error}</p>}

      <div className="usuarios-filtros">
        <input type="text" placeholder="Buscar por nombre, correo o CUIL" aria-label="Buscar usuarios" value={busqueda} onChange={(e) => setBusqueda(e.target.value)} />
        <select aria-label="Filtrar por rol" value={filtroRol} onChange={(e) => setFiltroRol(e.target.value)}>
          <option value="">Rol: todos</option>
          {roles.map((rol) => <option key={rol.id} value={rol.id}>{NOMBRE_ROL[rol.name] ?? rol.name}</option>)}
        </select>
        <select aria-label="Filtrar por estado" value={filtroEstado} onChange={(e) => setFiltroEstado(e.target.value)}>
          <option value="">Estado: todos</option>
          <option value="activo">Activo</option>
          <option value="baja">Baja</option>
        </select>
      </div>

      {cargando ? (
        <p>Cargando...</p>
      ) : (
        <div className="usuarios-caja">
          <table className="usuarios-tabla" aria-label="Tabla de usuarios">
            <thead>
              <tr>
                <th>Nombre y apellido</th>
                <th>Correo</th>
                <th>CUIL</th>
                <th>Rol</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {filtrados.map((usuario) => (
                <tr key={usuario.id} className={usuario.isActive ? '' : 'usuarios-fila-baja'}>
                  <td>{usuario.lastName}, {usuario.firstName}</td>
                  <td>{usuario.email}</td>
                  <td>{usuario.cuil}</td>
                  <td>{NOMBRE_ROL[usuario.roleName] ?? usuario.roleName}</td>
                  <td><span className={`usuarios-estado ${usuario.isActive ? 'usuarios-estado--activo' : 'usuarios-estado--baja'}`}>{usuario.isActive ? 'Activo' : 'Baja'}</span></td>
                  <td>
                    <div className="usuarios-acciones">
                      <button type="button" className="usuarios-accion" onClick={() => setFormulario(usuario)}>Editar</button>
                      {usuario.isActive && (
                        <button type="button" className="usuarios-accion usuarios-accion--peligro" onClick={() => { setErrorBaja(''); setABajar(usuario); }}>Dar de baja</button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
              {filtrados.length === 0 && (
                <tr><td colSpan={6} className="usuarios-vacio">No hay usuarios que coincidan con la búsqueda.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}
      {!cargando && <p className="usuarios-contador">Mostrando {filtrados.length} de {usuarios.length} usuarios</p>}

      {formulario && (
        <UsuarioForm
          usuario={formulario.id ? formulario : null}
          roles={roles}
          onCancelar={() => setFormulario(null)}
          onGuardado={(mensaje) => {
            setFormulario(null);
            setAviso(mensaje);
            cargar();
          }}
        />
      )}

      {aBajar && (
        <div className="usuarios-overlay" role="presentation">
          <div className="usuarios-modal usuarios-modal--chico" role="dialog" aria-modal="true" aria-labelledby="titulo-baja">
            <h2 id="titulo-baja">¿Dar de baja a este usuario?</h2>
            <p className="usuarios-sub">{aBajar.lastName}, {aBajar.firstName} ya no va a poder iniciar sesión. Sus datos se conservan y se puede reactivar desde Editar.</p>
            {errorBaja && <div className="usuarios-alerta" role="alert">{errorBaja}</div>}
            <div className="usuarios-botones">
              <button type="button" className="usuarios-secundario" onClick={() => setABajar(null)}>Cancelar</button>
              <button type="button" className="usuarios-peligro" onClick={confirmarBaja}>Dar de baja</button>
            </div>
          </div>
        </div>
      )}
    </section>
  );
}

export default Usuarios;