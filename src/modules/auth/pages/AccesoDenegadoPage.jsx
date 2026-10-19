import { ShieldAlert } from 'lucide-react';
import { Link } from 'react-router-dom';
import { rutaInicio } from '../../../core/auth/roles';


function AccesoDenegadoPage({ rol }) {
  return (
    <div className="acceso-denegado">
      <div className="acceso-denegado__tarjeta" role="alert">
        <div className="acceso-denegado__icono">
          <ShieldAlert size={30} />
        </div>
        <div className="acceso-denegado__codigo">403</div>
        <h1>No tenés permiso para ver esta sección</h1>
        <p>Tu usuario no tiene acceso a esta pantalla. Si creés que es un error, consultá en Secretaría.</p>
        <Link className="acceso-denegado__boton" to={rutaInicio(rol)} replace>
          {rol ? 'Volver a mi inicio' : 'Ir al inicio de sesión'}
        </Link>
      </div>
    </div>
  );
}

export default AccesoDenegadoPage;