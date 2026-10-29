import { useEffect } from 'react';
import {
  expirarSesion,
  marcarActividad,
  obtenerSesion,
  obtenerUltimaActividad,
  tokenVencido,
} from './session';
import { renovarSesion } from '../../services/httpClient';

const MINUTOS_INACTIVIDAD = 30;
const REVISAR_CADA_MS = 5000;
const GUARDAR_CADA_MS = 15000;
const RENOVAR_CADA_MS = 5000;
const EVENTOS = ['mousemove', 'mousedown', 'keydown', 'wheel', 'touchstart'];

export function useExpiracionSesion() {
  useEffect(() => {
    const actividadGuardada = obtenerUltimaActividad();
    let ultimaActividad = actividadGuardada ?? Date.now();
    let ultimoGuardado = ultimaActividad;
    let ultimaRenovacion = 0;

    const sincronizarActividad = () => {
      const guardada = obtenerUltimaActividad();
      if (guardada !== null && guardada > ultimaActividad) {
        ultimaActividad = guardada;
      }
    };

    const renovar = () => {
      if (!obtenerSesion()) return;

      const ahora = Date.now();
      if (ahora - ultimaRenovacion < RENOVAR_CADA_MS) return;

      ultimaRenovacion = ahora;
      renovarSesion()
        .then((token) => {
          if (!token && tokenVencido()) expirarSesion();
        })
        .catch(() => {});
    };

    const registrarActividad = () => {
      if (!obtenerSesion()) return;

      const ahora = Date.now();
      sincronizarActividad();

      if (ahora - ultimaActividad >= MINUTOS_INACTIVIDAD * 60 * 1000) {
        expirarSesion();
        return;
      }

      ultimaActividad = ahora;

      if (ahora - ultimoGuardado >= GUARDAR_CADA_MS) {
        ultimoGuardado = ahora;
        marcarActividad(ahora);
      }

      renovar();
    };

    const revisar = () => {
      if (!obtenerSesion()) return;

      sincronizarActividad();
      if (Date.now() - ultimaActividad >= MINUTOS_INACTIVIDAD * 60 * 1000) {
        expirarSesion();
      }
    };

    const guardarAlSalir = () => marcarActividad(ultimaActividad);

    EVENTOS.forEach((evento) => window.addEventListener(evento, registrarActividad, { passive: true }));
    window.addEventListener('pagehide', guardarAlSalir);
    const temporizador = setInterval(revisar, REVISAR_CADA_MS);

    if (obtenerSesion()) {
      sincronizarActividad();

      if (Date.now() - ultimaActividad >= MINUTOS_INACTIVIDAD * 60 * 1000) {
        expirarSesion();
      } else if (actividadGuardada === null) {
        // Sesión vieja sin marca de actividad: recién ahí usamos el momento actual.
        marcarActividad(ultimaActividad);
      }
      // Recargar no reinicia el contador: se conserva la última actividad real.
    }

    return () => {
      EVENTOS.forEach((evento) => window.removeEventListener(evento, registrarActividad));
      window.removeEventListener('pagehide', guardarAlSalir);
      clearInterval(temporizador);
    };
  }, []);
}
