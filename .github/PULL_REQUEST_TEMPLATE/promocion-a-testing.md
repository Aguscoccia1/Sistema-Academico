## Promoción a `testing`

**compare `development`** → **base `testing`**

Este PR arma la **versión candidata**: lo que está integrado en `development` pasa a `testing` para
probarse completo antes de darlo por bueno. Lo abre y lo aprueba el E1.

## Qué entra en esta candidata

<!-- Listá lo que se mergeó desde cada rama de equipo desde la última promoción.
     Si un equipo no metió nada en este período, escribí "sin cambios". -->

| Equipo | Rama | Qué entra en esta candidata | PR |
|---|---|---|---|
| E1 — Acceso e Integración | `e1` | | #  |
| E2 — Secretaría | `e2` | | #  |
| E3 — Gestión Académica | `e3` | | #  |
| E4 — Inscripciones | `e4` | | #  |

Para ver rápido qué commits entran:

```bash
git fetch origin
git log --oneline origin/testing..origin/development
```

## Estado del CI

- [ ] El workflow **CI** de GitHub Actions terminó en verde sobre este PR

Si el CI falló o quedó pendiente, verificá a mano y contá acá qué pasó.
Parado en la raíz del repositorio, con `development` actualizada:

```bash
npm ci
npm run build
```

Resultado: <!-- verde / rojo + link al run o pegá el error -->

## Pruebas manuales

Se prueban con el proyecto levantado (`npm run dev`) en http://localhost:5173.
Si un módulo todavía no existe en esta candidata, no borres el ítem: marcalo y escribile al lado
"N/A — sin pantallas todavía", así queda claro que no se olvidó de probar.

- [ ] **Perfil Secretario**: inicio sesión con `maria@uni.edu` y se ve su menú
- [ ] **Perfil Estudiante**: inicio sesión con `loko@uni.edu` y se ve su menú
- [ ] **Navegación**: todos los ítems del menú abren su pantalla, en los dos perfiles
- [ ] **Rutas cruzadas**: entrar a una URL del otro rol redirige, no rompe la pantalla
- [ ] **Pantallas del E1**: acceso, usuarios, roles y permisos
- [ ] **Pantallas del E2**: inscripción a 1.º año, docentes, estudiantes por comisión
- [ ] **Pantallas del E3**: planes de estudio, materias, correlatividades
- [ ] **Pantallas del E4**: consulta de materias e inscripción a 2.º/3.º
- [ ] **Altas y bajas**: crear, editar y eliminar funcionan y la lista se actualiza sola
- [ ] **Recarga de página**: recargar estando en una pantalla interna no rompe la navegación
- [ ] **Consola del navegador**: sin errores rojos al recorrer las pantallas principales

## Riesgos conocidos

<!-- Qué quedó a medias, qué se sabe que puede fallar, qué no se pudo probar y por qué.
     Si no hay nada, escribí "ninguno detectado". No lo dejes vacío. -->

## Capturas / evidencia

<!-- Capturas de las pantallas probadas. -->
