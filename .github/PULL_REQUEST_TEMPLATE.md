## ¿Qué hace este PR?

<!-- Descripción breve y concreta de la funcionalidad o el arreglo.
     Si agrega pantallas o rutas nuevas, nombralas. -->

## Equipo

<!-- Marcá el equipo que abre el PR -->

- [ ] E1 — Acceso e Integración
- [ ] E2 — Secretaría
- [ ] E3 — Gestión Académica
- [ ] E4 — Inscripciones

## Rama origen y destino

| | Repositorio | Rama |
|---|---|---|
| **head** (de dónde vienen los cambios) | tu fork, `TU-USUARIO/Sistema-Academico` | `e1` / `e2` / `e3` / `e4` |
| **base** (a dónde van) | `gonzaleztomi1978-design/Sistema-Academico` | `development` |

Ejemplo: tu fork, rama **`e2`** → **`development`** del proyecto.

> Los PR de equipo van **siempre** a `development`. Las promociones a `testing` y a `production` las abre el E1 con las otras plantillas (mirá la nota del final).

## Issues o tareas relacionadas

<!-- Ej: Closes #12 / Relacionado con #7. Si no hay issue, escribí a qué tarea del equipo corresponde. -->

## Checklist

- [ ] `npm run build` pasa sin errores
- [ ] Probé la funcionalidad a mano en el navegador, con los dos roles si corresponde
- [ ] No subí `node_modules/`, `dist/`, `bin/`, `obj/`, `.env` ni claves
- [ ] No cambié `appsettings.json` con mi conexión (la mía va con `dotnet user-secrets`)
- [ ] Mis cambios están en la carpeta de mi equipo; si toqué un archivo compartido, solo agregué lo mío
- [ ] Mergeé `development` en mi rama y resolví los conflictos antes de abrir el PR
- [ ] El PR apunta a `development` (nunca a `testing` ni a `production`)
- [ ] Mis commits usan el prefijo del equipo (`E1:` / `E2:` / `E3:` / `E4:`)

Para el punto del merge (el ejemplo usa `e2`: cambiá la rama por la de tu equipo):

```bash
git checkout e2               # tu rama: e1, e2, e3 o e4
git fetch origin              # actualizás la foto local de development (sin esto mergeás algo viejo)
git merge origin/development  # traés lo ya integrado de los otros equipos
# Si hay conflictos: resolvelos en el editor y después
#   git add ARCHIVO-QUE-RESOLVISTE
#   git commit
git push fork e2              # se sube a tu fork; este PR se actualiza solo
```

## Capturas / evidencia

<!-- Arrastrá acá una captura de la pantalla nueva o del cambio funcionando. -->

---

### Otras plantillas (solo E1)

Las promociones entre ramas tienen su propia plantilla, guardada en `.github/PULL_REQUEST_TEMPLATE/`.
Ojo con esto: GitHub **no** muestra un selector de plantillas al abrir un Pull Request (eso existe solo
para los issues). La única forma de elegirla es abrir el PR con el parámetro `template=` en la URL.

| PR | URL para abrirlo |
|---|---|
| `development` → `testing` | `https://github.com/gonzaleztomi1978-design/Sistema-Academico/compare/testing...development?expand=1&template=promocion-a-testing.md` |
| `testing` → `production` | `https://github.com/gonzaleztomi1978-design/Sistema-Academico/compare/production...testing?expand=1&template=promocion-a-production.md` |

La URL de comparación se lee `compare/<base>...<compare>`: primero la rama de destino y después la rama
de donde vienen los cambios. Si ya abriste el PR desde la interfaz y querés sumarle la plantilla,
agregale `&template=promocion-a-testing.md` al final de la URL y recargá la página. Cuidado: al recargar
se pierde lo que hayas escrito en la descripción, así que elegí la plantilla antes de completarla.
