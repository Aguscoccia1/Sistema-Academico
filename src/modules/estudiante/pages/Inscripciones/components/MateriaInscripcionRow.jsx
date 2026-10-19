import { AlertCircle, CheckCircle2 } from 'lucide-react';

function MateriaInscripcionRow({ materia, periodoAbierto, onInscribir, onVerDetalle }) {
  const aprobada = materia.estado === 'A';
  const inscripta = materia.estado === 'R';
  const bloqueada = !!materia.correlativaBloqueante && !aprobada && !inscripta;

  const badge = aprobada
    ? { texto: 'Aprobada', clase: 'badge--habilitado' }
    : inscripta
      ? { texto: 'Inscripta', clase: 'badge--habilitado' }
      : bloqueada
        ? { texto: 'Bloqueado', clase: 'badge--bloqueado' }
        : { texto: 'Habilitado', clase: 'badge--habilitado' };

  return (
    <tr>
      <td className="tabla-materia">{materia.nombre}</td>
      <td className="tabla-centro">{materia.año}</td>
      <td className="tabla-centro">{materia.regimen}</td>
      <td>
        <span
          className={`correlativas-estado ${bloqueada ? 'correlativas-estado--no' : 'correlativas-estado--ok'}`}
          title={bloqueada ? `Requiere: ${materia.correlativaBloqueante}` : undefined}
        >
          {bloqueada ? <AlertCircle size={18} /> : <CheckCircle2 size={18} />}
          {bloqueada ? 'No cumple las correlatividades' : 'Cumple las correlatividades'}
        </span>
      </td>
      <td className="tabla-centro">
        <span className={`estado-badge ${badge.clase}`}>{badge.texto}</span>
      </td>
      <td className="tabla-centro">
        {bloqueada ? (
          <button type="button" className="tabla-boton" onClick={() => onVerDetalle(materia.id)}>
            Ver detalle
          </button>
        ) : (
          <button
            type="button"
            className="tabla-boton"
            disabled={!periodoAbierto || inscripta || aprobada}
            onClick={() => onInscribir(materia.id)}
          >
            {aprobada ? 'Aprobada' : inscripta ? 'Inscripta' : 'Inscribirme'}
          </button>
        )}
      </td>
    </tr>
  );
}
export default MateriaInscripcionRow;