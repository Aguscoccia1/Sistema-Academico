# Integración y releases — runbook del E1

Este documento es para **la persona del E1 que hace de integrador**: la que junta lo que hacen los cuatro equipos, lo prueba y deja `production` presentable.

Si sos de E2, E3 o E4, lo que te toca está en [`flujo-git.md`](flujo-git.md). Igual leelo: vas a entender por qué a veces te pedimos cambios en un PR en vez de mergearlo.

## Ramas del repositorio

```
e1 ─┐
e2 ─┤
e3 ─┼─ PR ─> development ─ PR ─> testing ─ PR ─> production
e4 ─┘
```

| Rama | Para qué es | ¿Quién pushea directo? |
|---|---|---|
| `e1` a `e4` | Trabajo diario de cada equipo. | Los integrantes de ese equipo. |
| `development` | Integración diaria: acá se junta todo. | **Nadie.** Solo PR desde `e1`..`e4`. |
| `testing` | Versión candidata, se prueba completa. | **Nadie.** Solo PR desde `development`. |
| `production` | Versión estable. Rama por defecto del repositorio. | **Nadie.** Solo PR desde `testing` (o de un `hotfix/`). |

Regla de oro: **a `development`, `testing` y `production` se entra únicamente por Pull Request**, aunque tengas permiso técnico para pushear.

---

## 1. Rol del integrador

El integrador **no** es el que más código escribe: es el que garantiza que lo que está en `production` se pueda mostrar sin sorpresas.

### Qué decide

- Si un PR hacia `development` se mergea, se devuelve con cambios pedidos, o espera.
- Cuándo se corta una versión candidata (`development` → `testing`).
- Cuándo se publica (`testing` → `production`) y qué número de versión lleva.
- Cuándo algo amerita un `hotfix/` y cuándo puede esperar.
- Si hay que revertir un merge que rompió algo.

### Qué NO decide

- **Cómo resuelve cada equipo su módulo por dentro.**
- **Estructura, dependencias y modelo de datos**: eso se acuerda en la Mesa Técnica.
- **Arreglar el código de otro equipo por afuera.** Si un PR viene roto, se piden cambios en el PR. Si lo acomodás en silencio, el equipo nunca se entera de lo que hizo mal y vuelve a pasar.

### De qué es responsable

| Rama | Responsabilidad del integrador |
|---|---|
| `development` | Que siempre compile. Si alguien la rompe, o se arregla en el día o se revierte el merge. |
| `testing` | Que lo que está ahí haya sido probado a mano de punta a punta con la checklist de la sección 5. |
| `production` | Que sea mostrable en cualquier momento, etiquetada con `git tag` y con notas de versión. |

Si tenés que elegir entre "mergear rápido para no frenar al equipo" y "que `production` quede sana", elegí siempre lo segundo.

---

## 2. Recibir un PR de un equipo hacia `development`

### Checklist de revisión

- [ ] **La base del PR es `development`.** No `testing`, no `production`.
- [ ] **El CI está en verde.** Es el primer filtro y es gratis.
- [ ] **Compila**: `npm run build` sin errores (ver sección 3 para verificarlo local).
- [ ] **Respeta la estructura de carpetas.** Pantallas en `src/modules/<rol>/pages/`, componentes propios en el `components/` de ese módulo, nada de carpetas nuevas inventadas al costado.
- [ ] **No trae basura versionada.** Nada de `node_modules/`, `dist/`, `.env`, `.vite/`.
- [ ] **No agrega dependencias sin avisar.** Si el diff toca `package.json`, tiene que estar acordado en la Mesa Técnica. Una librería nueva la terminan sufriendo los cuatro equipos.
- [ ] **No pisa archivos de otro equipo.** Excepción normal: los compartidos (`Sidebar.jsx`, los archivos de rutas, `managementData.js`), donde es esperable un agregado chico.
- [ ] **Prefijo de commit correcto**, y que coincida con el equipo dueño de la rama.
- [ ] **Tiene evidencia.** El template pide capturas; si la funcionalidad es visible y no hay nada, pedila.
- [ ] **No hay conflictos con `development`.** Si GitHub marca conflicto, lo resuelve el equipo autor en su rama.

### Qué hacer si no cumple

1. **Pedí cambios en el PR**, con un comentario concreto por cada punto: qué falla, en qué archivo, y qué esperás. Nada de "no anda".
2. **No lo mergees igual pensando en arreglarlo después.** Un merge que rompe `development` frena a los cuatro equipos.
3. **No lo arregles por afuera.** El equipo corrige en su rama y pushea: el PR se actualiza solo.
4. Si estamos sobre una entrega y no hay tiempo, arreglalo vos **en la rama del equipo** (`eX`), nunca en `development`, y escribilo en el PR para que quede constancia.

Comentario tipo para pedir cambios:

```
No lo mergeo todavía, faltan dos cosas:

1. `npm run build` falla en src/modules/secretario/pages/Docentes/Docentes.jsx:42
   (el import de ManagementTable está mal escrito).

2. El PR trae node_modules/. Sacalo con:
   git rm -r --cached node_modules
   git commit -m "E2: saca node_modules del control de versiones"
   git push fork e2

Cuando lo pushees, este PR se actualiza solo y lo vuelvo a mirar.
```

---

## 3. Verificar un PR localmente antes de aprobarlo

La revisión en GitHub sirve para leer el diff. Para saber si **funciona**, hay que bajarlo y levantarlo.

> Todos los comandos se corren **parados en la raíz del repositorio** (la carpeta `Sistema-Academico` que te quedó al clonar). Si no sabés dónde estás, corré `pwd`.

Los PR llegan desde los forks de cada integrante. Para traer uno a tu computadora se usa su número, el que aparece en el título del PR (por ejemplo `#7`):

```bash
# 1. Traer lo que hay en GitHub y el PR número 7 como una rama local
git fetch origin
git fetch origin pull/7/head:pr-7

# 2. Pasarte a esa rama
git checkout pr-7
```

Si querés ver cómo quedaría `development` con ese PR adentro, probá el merge en una rama descartable:

```bash
git checkout development
git pull origin development
git checkout -b prueba/pr-7      # rama temporal, no se sube nunca
git merge pr-7
```

### Compilar y probar

```bash
npm install       # va siempre: si el PR agregó una dependencia, sin esto no compila
npm run build
npm run dev
```

Y ahí sí:

- Entrá a http://localhost:5173 y probá **la funcionalidad del PR**, no solo que la pantalla abra.
- Cambiá de rol con el simulador y probá que **lo que ya andaba siga andando**.
- Mirá la consola del navegador: no tiene que haber errores rojos.

`npm run lint` es aparte: **avisa pero no bloquea**, igual que en el CI. Si tira algo, pedile al equipo que lo mire, pero no frenes el PR por eso.

### Limpiar la rama temporal

```bash
git checkout development
git branch -D prueba/pr-7 pr-7
```

---

## 4. Estrategia de merge: siempre merge commit

Al mergear un PR, usá **"Create a merge commit"**. Ni *Squash and merge*, ni *Rebase and merge*.

### Por qué

- **No reescribe historia que otros ya tienen.** Con squash o rebase, los commits cambian de identificador y a todos los que ya tenían esa rama les queda un historial que no coincide. El síntoma típico es alguien que hace `git pull` y le aparecen treinta commits duplicados.
- **Queda claro qué entró y cuándo.** Si algo se rompe, mirás los merges recientes y sabés por dónde empezar.
- **Se puede deshacer de una** con `git revert -m 1` (sección 9).
- **Sirve para la defensa del proyecto**: el historial muestra quién hizo qué y cómo se integró.

Desde la consola el equivalente es el comando de abajo, pero **solo para probar en una rama temporal**. Hacia `development`, `testing` y `production` el merge se hace siempre con el botón del PR en GitHub.

```bash
git merge --no-ff e2
```

---

## 5. Promoción `development` → `testing`

### Cuándo

Una vez por semana, día fijo, o cuando hay una entrega cerca. La idea es cortar una **versión candidata**.

### Cómo se prepara

- [ ] Están mergeados todos los PR que tenían que entrar. Los que quedaron a medias esperan al próximo corte.
- [ ] El último run del CI sobre `development` terminó en verde.
- [ ] `development` compila también en tu máquina.
- [ ] Avisaste a los cuatro equipos que se corta.

```bash
git checkout development
git pull origin development
npm install
npm run build
```

### Cómo se abre el PR

En GitHub: **New pull request**, `base: testing` ← `compare: development`. Título sugerido: `Release candidata v1.1.0`.

En el cuerpo, listá qué entra y de qué equipo:

```
Entra en esta candidata:

- E2: alta de docentes + listado de estudiantes por comisión
- E3: ABM de materias y carga de correlatividades
- E4: consulta de materias disponibles con validación de correlativas
- E1: pantalla de login
```

Mergealo con **merge commit**.

### Pruebas manuales obligatorias sobre `testing`

Después de mergear, bajá `testing` y pasá toda la lista. Si algo falla, no se promueve a `production`.

```bash
git checkout testing
git pull origin testing
npm install
npm run dev
```

| # | Prueba | Resultado esperado |
|---|---|---|
| 1 | Cambiar a perfil Secretario | Se ve su menú completo. |
| 2 | Cambiar a perfil Estudiante | Se ve su menú completo. |
| 3 | Recorrer todos los ítems del menú, en los dos perfiles | Cada pantalla abre y muestra sus datos. |
| 4 | Entrar a una URL del otro rol a mano | Redirige al inicio del rol activo, no queda en blanco. |
| 5 | Recargar la página estando en una pantalla interna | Vuelve a cargar bien, sin romperse. |
| 6 | E1 — acceso | Login, usuarios, roles y permisos. |
| 7 | E2 — secretaría | Inscribir a 1.º año, ver estudiantes por comisión, asignar materias a un docente. |
| 8 | E3 — gestión académica | Crear un plan, cargar materias, definir una correlatividad. |
| 9 | E4 — inscripciones | Consultar materias disponibles e inscribirse a una de 2.º/3.º. |
| 10 | E4 — correlativas | Intentar inscribirse sin la correlativa aprobada: se rechaza con mensaje claro. |
| 11 | Altas, bajas y ediciones | La lista se actualiza sola después de cada operación. |
| 12 | Consola del navegador | Sin errores rojos al recorrer las pantallas principales. |

Anotá lo que falle en el mismo PR de promoción o en un issue, asignado al equipo dueño. Los arreglos entran por el flujo normal: el equipo corrige en `eX`, PR a `development`, y se vuelve a promover.

---

## 6. Promoción `testing` → `production`

### Cuándo

Solo por **entrega o hito**: presentación de avance, entrega de cuatrimestre, defensa final. No se promueve "porque sí", ni un rato antes de mostrarlo.

- [ ] Toda la checklist de la sección 5 pasó sobre `testing`.
- [ ] No quedan errores conocidos que afecten la demo.
- [ ] `testing` estuvo al menos un día sin cambios.

### Quién aprueba

**El E1 organiza y aprueba.** Lo ideal es que, además de quien abre el PR, otra persona del E1 lo revise. Si la profesora quiere dar el visto bueno, se espera.

### Cómo se hace

En GitHub: **New pull request**, `base: production` ← `compare: testing`. Título: `Release v1.0.0`. Merge con **merge commit**.

### Después del merge: etiquetar la versión

```bash
git checkout production
git pull origin production

git tag -a v1.0.0 -m "Primera entrega: acceso, secretaria, plan de estudio e inscripciones"
git push origin v1.0.0        # sin esto el tag queda solo en tu máquina
```

Numeración, simplificada para el proyecto:

| Cambio | Cuándo se usa |
|---|---|
| `v1.0.0` → `v2.0.0` | Entrega grande o cambio de alcance. |
| `v1.0.0` → `v1.1.0` | Funcionalidad nueva de algún equipo. |
| `v1.0.0` → `v1.0.1` | Solo arreglos, típicamente un hotfix. |

### Después del tag: notas de versión

En GitHub, **Releases → Draft a new release**, elegís el tag y escribís las notas:

```markdown
## v1.0.0 — Primera entrega

### Funcionalidades
- E1: pantalla de acceso y gestión de usuarios, roles y permisos.
- E2: inscripción a 1.º año, estudiantes por comisión, asignación de materias a docentes.
- E3: planes de estudio, ABM de materias, carga de correlatividades.
- E4: consulta de materias e inscripción a 2.º/3.º con validación de correlativas.

### Cómo levantarlo
Ver README: npm install y npm run dev.

### Pendientes conocidos
- Los datos son simulados en memoria: se pierden al recargar.
- Falta el reporte de inscriptos por comisión (queda para v1.1.0).
```

Los "pendientes conocidos" no son una vergüenza: escribirlos muestra que sabés dónde está parado el proyecto. Ocultarlos y que aparezcan en plena demo, eso sí.

---

## 7. Volver a bajar lo integrado a las ramas de los equipos

**Después de cada promoción**, los cuatro equipos tienen que traerse lo integrado. Si no, cada uno sigue trabajando sobre una base vieja y los PR siguientes llegan llenos de conflictos.

La rama de referencia para bajar es **`development`**. Como integrador:

```bash
git fetch origin
git checkout development
git pull origin development

# repetir para e1, e2, e3 y e4
git checkout e1
git pull origin e1
git merge development
git push origin e1

git checkout development
```

Si alguna tira conflicto, **no lo resuelvas solo**: es código del equipo. Pero ojo, un merge con conflictos deja la rama a medio mergear, así que cancelalo antes de seguir:

```bash
git merge --abort
git checkout development
```

Después avisale al equipo que lo corra en su rama y lo suba a su fork:

```bash
git checkout e3
git fetch origin
git merge origin/development
git push fork e3
```

Avisá siempre que lo hiciste: "Promoví a testing y bajé development a e1..e4. Antes de seguir, hagan `git fetch origin` y `git merge origin/development` en su rama".

---

## 8. Hotfix: algo roto en `production` a mitad de la presentación

Un hotfix es para **algo roto en `production` que no puede esperar** el ciclo normal. No es para "aprovecho y agrego una cosita".

El hotfix sale de `production` y vuelve a `production`, y **después** se reintegra hacia `testing` y `development` para que el arreglo no se pierda.

### Paso 1 — Crear la rama desde `production`

```bash
git fetch origin
git checkout production
git pull origin production
git checkout -b hotfix/menu-estudiante-no-abre
```

Nombre: `hotfix/<descripcion-corta-con-guiones>`.

### Paso 2 — Arreglar, compilar y probar

```bash
npm install
npm run build
npm run dev          # probá que el problema esté resuelto Y que no rompiste otra cosa

git add .
git commit -m "E1: corrige el menu del estudiante que no abria"
git push origin hotfix/menu-estudiante-no-abre
```

### Paso 3 — PR a `production`

`base: production` ← `compare: hotfix/menu-estudiante-no-abre`. Merge con **merge commit**.

### Paso 4 — Tag de la versión corregida

```bash
git checkout production
git pull origin production
git tag -a v1.0.1 -m "Hotfix: el menu del estudiante no abria"
git push origin v1.0.1
```

### Paso 5 — Reintegrar hacia `testing` y `development`

**Este paso no es opcional.** Si lo salteás, el próximo release vuelve a pisar `production` con el bug adentro.

- PR `base: testing` ← `compare: production`, título `Reintegra hotfix v1.0.1 a testing`.
- PR `base: development` ← `compare: testing`, título `Reintegra hotfix v1.0.1 a development`.

Si estás en el medio de la presentación y no da para abrir dos PR, dejalos abiertos y mergealos apenas termine, pero **no te vayas del día sin hacerlo**.

### Paso 6 — Borrar la rama de hotfix

```bash
git branch -d hotfix/menu-estudiante-no-abre
git push origin --delete hotfix/menu-estudiante-no-abre
```

---

## 9. `testing` quedó roto y hay que revertir un merge

Revertir **no borra historia**: crea un commit nuevo que deshace lo que hizo el merge. Por eso es seguro en ramas compartidas.

### Paso 1 — Encontrar el identificador del merge

```bash
git checkout testing
git pull origin testing
git log --oneline --merges -10
```

Copiá el identificador del merge que rompió todo, por ejemplo `a1b2c3d`. Los de tu repositorio van a ser otros: usá el que te muestre a vos el `git log`.

### Paso 2 — Revertir, pero en una rama aparte

`testing` no acepta push directo, así que el revert **no se hace parado en `testing`**: si lo hacés ahí, el commit te queda trabado en tu máquina. Va en una rama nueva que después entra por PR.

```bash
git checkout -b revert/merge-17
git revert -m 1 a1b2c3d
```

**Qué es ese `-m 1`, en criollo.** Un merge commit tiene dos padres: por un lado la historia de la rama donde estabas (`testing`), por el otro la de la rama que entró (`development`). Git no puede adivinar cuál querés conservar, así que se lo decís vos. `-m 1` significa "dejame `testing` como estaba antes del merge". En un merge de PR el padre 1 es **siempre** la rama destino, así que en la práctica **siempre va `-m 1`**.

### Paso 3 — Subir el revert y abrir el PR

```bash
git push origin revert/merge-17
```

PR: `base: testing` ← `compare: revert/merge-17`. Merge con **merge commit**. Avisale al equipo responsable.

> Si te apuraste y corriste el `git revert` parado en `testing`, ese commit está solo en tu máquina. Sacalo con `git checkout testing` y `git reset --hard origin/testing`, y volvé a empezar desde el paso 2.

### Paso 4 — Volver a meter el trabajo, ya arreglado

Ojo con esto, que es la trampa clásica. El revert quedó vivo en `testing`, pero `development` sigue teniendo el código original. Para Git esos commits **ya están mergeados** en `testing`, así que en la próxima promoción va a traer **solo los commits nuevos del arreglo**, no lo que revertiste. Resultado: `testing` queda con el arreglo pero sin la funcionalidad que el arreglo corrige. Si al promover "no aparece" la funcionalidad, no está fallando Git: es exactamente esto.

La solución es **revertir el revert** antes de volver a promover.

1. El equipo corrige en su rama `eX` y abre PR a `development`, como siempre. En `development` **no** se revierte nada.
2. Cuando el arreglo ya está en `development`, en vez del PR de promoción normal armás esta rama:

```bash
git fetch origin
git checkout -b promocion/reintegra-merge-17 origin/testing

# Buscá el commit del revert del paso 2: es el que empieza con "Revert"
git log --oneline -10 origin/testing

# Deshacé ese revert (va el identificador del commit "Revert ...", NO el del merge original)
git revert SHA-DEL-COMMIT-REVERT

# Traé el arreglo que ya está integrado en development
git merge origin/development

git push origin promocion/reintegra-merge-17
```

3. PR: `base: testing` ← `compare: promocion/reintegra-merge-17`. Merge commit, y pasá la checklist de pruebas de la sección 5.

Ese `git revert` no lleva `-m 1`, porque el commit del revert es un commit común, no un merge.

---

## 10. Ritmo sugerido para el cuatrimestre

| Cuándo | Qué pasa | Quién |
|---|---|---|
| Cuando una funcionalidad está lista y probada | El equipo abre PR `eX` → `development`. No se acumulan tres semanas en un PR gigante. | Cada equipo |
| Todos los días, o cuando llega un PR | El integrador revisa los PR abiertos: pide cambios o mergea. | E1 |
| Semanal, día fijo | Promoción `development` → `testing` y checklist de pruebas manuales. | E1 |
| Semanal, después de promover | Bajar `development` a `e1`..`e4` y avisar a los equipos. | E1 |
| Por entrega o hito | Promoción `testing` → `production`, tag y notas de versión. | E1 |
| Cuando se rompe algo en `production` | `hotfix/<descripcion>` y reintegración. | E1 |
| Antes de cada hito grande | Revisar los documentos de la Mesa Técnica. | Mesa Técnica |

Dos consejos que valen más que el calendario:

1. **PR chicos y seguido.** Un PR de cuarenta archivos no lo revisa nadie de verdad, y los conflictos que genera se comen dos días.
2. **No promuevas nada el mismo día de la entrega.** Si la demo es el jueves, `production` tiene que estar lista el martes.

---

## 11. Tabla resumen

| Acción | Origen | Destino | Quién | Cómo |
|---|---|---|---|---|
| Trabajo diario del equipo | — | `eX` en el fork de cada integrante | Cada integrante | `git push fork eX` |
| Traer lo integrado antes de seguir | `development` | `eX` | Cada integrante | `git merge origin/development` y `git push fork eX` |
| Integrar una funcionalidad | `eX` del fork | `development` | Lo abre el integrante, lo mergea el E1 | PR con merge commit |
| Cortar versión candidata | `development` | `testing` | E1 | PR con merge commit + pruebas manuales |
| Publicar versión estable | `testing` | `production` | E1 | PR con merge commit + tag + notas de versión |
| Bajar lo integrado a los equipos | `development` | `e1` a `e4` | E1, o cada equipo | `git merge development` en cada rama y push |
| Arreglo urgente | `production` | `hotfix/<desc>` y vuelta a `production` | E1 | PR con merge commit + tag de patch |
| Reintegrar un hotfix | `production` | `testing`, y después `development` | E1 | Dos PR con merge commit |
| Deshacer un merge que rompió todo | `testing` | la misma rama | E1 | `git revert -m 1 <sha>` en una rama `revert/...` + PR |
| Volver a meter lo revertido | `testing` + `development` | `testing` | E1 | Rama `promocion/reintegra-...` (sección 9, paso 4) |

---

## Recordatorios finales

- Nunca `git push` directo a `development`, `testing` ni `production`.
- Siempre merge commit. Nunca squash ni rebase en ramas compartidas.
- Los tags se pushean aparte: `git push origin v1.0.0`.
- Si un PR no cumple, se piden cambios en el PR. No se arregla por afuera ni se mergea "por ahora".
- Después de cada promoción, bajá `development` a las cuatro ramas de equipo y avisá.
- Si dudás entre frenar un merge o dejar `production` en riesgo, frená el merge.
