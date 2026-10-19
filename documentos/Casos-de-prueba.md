Criterio HU-E1-02	Resultado
Cierre manual	Pasó. El botón «Cerrar sesión» llevó a /login.

Botón «Atrás» después del cierre	Pasó. Al volver atrás, el portal no reapareció: siguió en el login.

Expiración por inactividad	Pasó. Adelanté(copilot) el reloj de prueba 30 minutos; la app redirigió a /login?motivo=expirada, mostró el aviso de sesión expirada y limpió la sesión guardada.

Pruebas adicionales

- API: `GET http://localhost:5000/api/health` respondió `{"status":"ok","database":"AcademicSystem","roles":4}`.

- Inicio de sesión como Secretaría (`maria@uni.edu`): pasó; redirigió a `/secretario/inicio` y mostró el portal de Secretaría.
- Cierre de sesión y botón «Atrás» como Secretaría: el cierre llevó a `/login`; al volver atrás, el portal protegido no reapareció y se mantuvo el login. La navegación mostró `?motivo=expirada` porque seguía activo el reloj acelerado usado en la prueba anterior; conviene repetir este caso con el reloj restablecido para verificarlo sin esa condición.
