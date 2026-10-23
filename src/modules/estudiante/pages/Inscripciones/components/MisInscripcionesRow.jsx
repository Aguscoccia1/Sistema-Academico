import { CheckCircle2 } from 'lucide-react';

function MisInscripcionRow({ materia }) {
  return (
    <tr>
      <td className="tabla-materia">{materia.nombre}</td>
      <td className="tabla-centro">{materia.año}</td>
      <td className="tabla-centro">{materia.regimen}</td>
      <td>
        <span className="correlativas-estado correlativas-estado--ok">
          <CheckCircle2 size={18} />
          Cumple las correlatividades
        </span>
      </td>
      <td className="tabla-centro">
        <span className="estado-badge badge--inscripta">Inscripta</span>
      </td>
    </tr>
  );
}

export default MisInscripcionRow;