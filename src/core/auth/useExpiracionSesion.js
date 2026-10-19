import { useEffect } from 'react';
import { expirarSesion, obtenerSesion, tokenVencido } from './session';

const MINUTOS_INACTIVIDAD = 30;
const REVISAR_CADA_MS = 5000;
const EVENTOS = ['mousemove', 'mousedown', 'keydown', 'scroll', 'touchstart'];

export function useExpiracionSesion() {
  useEffect(() => {
    let ultimaActividad = Date.now();
    const registrarActividad = () => {
      ultimaActividad = Date.now();
    };

    const revisar = () => {
      if (!obtenerSesion()) return; 
      const inactivo = Date.now() - ultimaActividad >= MINUTOS_INACTIVIDAD * 60 * 1000;
      if (inactivo || tokenVencido()) expirarSesion();
    };

    EVENTOS.forEach((evento) => window.addEventListener(evento, registrarActividad));
    const temporizador = setInterval(revisar, REVISAR_CADA_MS);
    revisar();

    return () => {
      EVENTOS.forEach((evento) => window.removeEventListener(evento, registrarActividad));
      clearInterval(temporizador);
    };
  }, []);
}