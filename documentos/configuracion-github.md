# Configuración del repositorio en GitHub

Guía para dejar el repositorio con la estructura de ramas que pide la cátedra. **La ejecuta el E1** (Acceso e Integración), que es quien administra el repositorio.

> **Estado actual.** El repositorio ya existe en https://github.com/gonzaleztomi1978-design/Sistema-Academico, es público, y las 7 ramas ya están publicadas con `production` como rama por defecto. **Falta hacer desde la web:** invitar a los integrantes (sección 5) y activar las protecciones de rama (sección 6). Los pasos 1 a 4 quedan escritos como referencia, por si alguna vez hay que rehacer el repositorio.

## Qué queremos dejar armado

```text
e1 ─┐
e2 ─┤
e3 ─┼─ PR ─> development ─ PR ─> testing ─ PR ─> production
e4 ─┘
```

| Rama | Para qué | ¿Quién pushea directo? |
|---|---|---|
| `production` | Versión estable y presentable. Rama por defecto. | Nadie. Solo PR desde `testing`. |
| `testing` | Versión candidata: se prueba completa. | Nadie. Solo PR desde `development`. |
| `development` | Integración diaria de los 4 equipos. | Nadie. Solo PR desde `e1`..`e4`. |
| `e1` a `e4` | Trabajo de cada equipo. | Los integrantes de ese equipo. |

## 1. Crear el repositorio en GitHub (vacío)

Entrá a [github.com](https://github.com) > botón **New** y completá:

| Campo | Qué poner |
|---|---|
| Owner | `gonzaleztomi1978-design` (o la organización de la cátedra, si corresponde). |
| Repository name | `Sistema-Academico` |
| Public / Private | **Public** (ver la nota de abajo). |
| Add a README file | **Desmarcado** |
| Add .gitignore | **None** |
| Choose a license | **None** |

> **Por qué vacío.** Si tildás README o `.gitignore`, GitHub crea un commit propio del lado remoto. Ese commit no tiene nada que ver con el local, y el primer push se rechaza porque son dos historias sin ancestro común.

> **Público o privado.** Las reglas de protección de rama están disponibles gratis en repositorios **públicos**. En privados de cuenta Free pueden no estarlo. Como no subimos claves ni datos personales, conviene **Public**.

## 2. Agregar el remoto al repositorio local

```bash
git remote add origin https://github.com/gonzaleztomi1978-design/Sistema-Academico.git
git remote -v
```

Tiene que devolver dos líneas, `fetch` y `push`, apuntando a la misma URL. Si te equivocaste:

```bash
git remote set-url origin https://github.com/gonzaleztomi1978-design/Sistema-Academico.git
```

## 3. Publicar todas las ramas

`git push -u origin <rama>` sube la rama y le deja configurado el **upstream**, así después alcanza con `git push` y `git pull` a secas.

**Empezá por `production`**: GitHub toma como rama por defecto la primera que recibe.

```bash
git push -u origin production
git push -u origin testing
git push -u origin development
git push -u origin e1
git push -u origin e2
git push -u origin e3
git push -u origin e4
```

Comprobá que todas quedaron con su remota entre corchetes:

```bash
git branch -vv
```

## 4. Poner `production` como rama por defecto

**Settings > General > Default branch > Switch to another branch > production > Update.**

> **Por qué importa.** La rama por defecto es contra la que GitHub abre los Pull Request por descuido: el campo **base** viene precargado con ella. Si la default fuera `development`, cualquier distraído mete código sin pasar por `testing`. Además, es lo que ve cualquiera que abre el repositorio: queremos que vea la versión estable.

Ojo: por eso mismo, en los PR hacia `development` hay que **cambiar la base a mano**.

## 5. Cómo trabajan los integrantes: forks

En este proyecto **no se invita a nadie como colaborador**. Cada integrante hace un **fork**, que es su propia copia del repositorio en GitHub. Sube su trabajo ahí y abre un Pull Request hacia `development`. El único que puede mergear es el dueño del repositorio, así que todo lo que entra pasa por el E1.

Qué tiene que hacer cada integrante está en el README, en la sección "Primera vez". Lo que más traba:

- **Al crear el fork, hay que destildar "Copy the production branch only".** Si queda tildado, el fork no trae las ramas de los equipos.
- **Se clona el repositorio del proyecto, no el fork.** En su computadora, `origin` apunta acá y `fork` apunta a su copia.
- **Se sube con `git push fork eX`.** Un `git push origin` da error 403, porque no tienen permiso sobre este repositorio.

Lo que le toca al E1 con cada Pull Request que llega desde un fork:

1. La primera vez que alguien abre un PR desde su fork, GitHub no corre el CI solo. En el PR aparece **Approve and run workflows**: hay que tocarlo.
2. Esperar a que los checks queden en verde, revisar los cambios y mergear con **Create a merge commit**.

Para ver quién ya hizo su fork: pestaña **Insights > Forks** del repositorio.

## 6. Reglas de protección de rama

**Settings > Rules > Rulesets > New ruleset > New branch ruleset.** Se crea **un ruleset por rama protegida**: uno para `production`, uno para `testing` y uno para `development`.

### Configuración recomendada

| Opción | `production` | `testing` | `development` | `e1`..`e4` |
|---|---|---|---|---|
| Requiere Pull Request | Sí | Sí | Sí | No |
| Aprobaciones requeridas | 1 | 1 | 1 | — |
| Prohibir push directo | Sí | Sí | Sí | No |
| Prohibir force push | Sí | Sí | Sí | No |
| Prohibir borrado de la rama | Sí | Sí | Sí | No |
| Requiere que pase el CI | Sí | Sí | Sí | No |
| Origen permitido del merge | Solo `testing` | Solo `development` | Solo `e1`..`e4` | — |

**`e1`, `e2`, `e3` y `e4` no llevan ninguna regla.** Son las ramas de trabajo: cada equipo pushea ahí directo y se equivoca tranquilo.

Hay dos excepciones de emergencia al origen del merge, y las maneja solo el E1: un `hotfix/...` puede entrar directo a `production`, y una rama `revert/...` o `promocion/reintegra-...` puede entrar a `testing`. Están en [`integracion-y-releases.md`](integracion-y-releases.md), secciones 8 y 9.

### Cómo se traduce en la pantalla de GitHub

| Campo | Qué poner |
|---|---|
| Ruleset Name | `proteccion-production` (o `-testing` / `-development`) |
| Enforcement status | **Active** |
| Bypass list | Vacía (nadie se saltea la regla, el E1 tampoco) |
| Target branches > Include by pattern | El nombre exacto de la rama: `production` |

Y en **Branch rules**, tildá:

- **Restrict deletions** — que nadie borre la rama.
- **Block force pushes** — que nadie reescriba la historia.
- **Require a pull request before merging**, con **Required approvals: 1**.
- **Require status checks to pass**, y agregá el check del CI.

> **Sobre el check de CI.** El workflow está en `.github/workflows/ci.yml` y define dos jobs: **`Compilar el proyecto (React + Vite)`** y **`Compilar el backend (.NET 10)`**. Agregá los dos, buscándolos por ese nombre exacto. **Pero ojo:** GitHub solo lista los checks que ya vio correr al menos una vez, así que si todavía no aparece, dejá la opción destildada, terminá el resto del ruleset y volvé después.

> **No podés aprobar tu propio PR.** GitHub no te deja dar *Approve* en un PR que abriste vos. Como las promociones las abre el E1, con **Required approvals: 1** necesitás que otra persona apruebe. Resolvelo así: que las abra un integrante del E1 y las apruebe otro. Si el E1 es una sola persona, tenés dos opciones honestas: que apruebe la profesora, o bajar **Required approvals a 0** dejando igual el *Require a pull request*. Lo que **no** hay que hacer es meterse en la *Bypass list*, porque eso desactiva todas las reglas para vos.

> **Sobre "permitir merge solo desde `testing`".** GitHub **no** tiene una opción que limite desde qué rama se puede mergear. Es un **acuerdo del equipo** y se hace cumplir en la revisión: antes de aprobar, mirá el encabezado del PR (`base: production` ← `compare: testing`) y, si el `compare` no es el que corresponde, cerralo y pedí que se rehaga. Es la regla que más se rompe por descuido.

### Revisión automática del E1 (CODEOWNERS)

El repositorio trae `.github/CODEOWNERS`, que dice qué partes revisa sí o sí el E1 (la configuración del proyecto y todo `src/core`, `src/api`, `src/hooks`). Para que sirva:

1. **Tener el usuario correcto.** Hoy asigna todo a `@gonzaleztomi1978-design`. Si hay más integrantes del E1 que revisan, agregá sus usuarios en la misma línea separados por espacio. Cada persona listada tiene que ser colaboradora con rol **Write** como mínimo.
2. **Activar la opción en el ruleset.** Adentro de *Require a pull request before merging*, tildá **Require review from Code Owners**.

Un detalle que confunde: GitHub lee el `CODEOWNERS` **de la rama base del PR**. Para que funcione en los PR hacia `development`, el archivo tiene que estar en `development`.

### Qué está disponible gratis

| Situación | Reglas de protección |
|---|---|
| Repositorio **público** (cuenta Free) | Disponibles y gratis. |
| Repositorio **privado** (cuenta Free) | Limitadas; varias opciones piden plan pago. |
| Organización con plan pago o educativo | Disponibles. |

Si la opción aparece deshabilitada, tenés dos caminos: pasar el repositorio a público, o sostenerlo por acuerdo del equipo (nadie pushea a las tres ramas, solo el E1 mergea, y el E1 revisa cada tanto `git log --oneline development` buscando commits que hayan entrado sin PR).

## 7. Si el repositorio es de la cátedra o de una organización

Cuando el repositorio no es tuyo sino de la cátedra, cambia **quién puede tocar Settings**:

- La pestaña **Settings** la ven solo quienes tienen rol **Admin**. Con rol Write ni siquiera aparece.
- Los pasos 4, 5 y 6 **son todos de Settings**.

Qué hacer:

1. Pedile a la profesora rol **Admin** sobre el repositorio. Es lo más práctico: el E1 es el integrador y necesita administrar las ramas.
2. Si no se puede, pasale esta guía con el pedido concreto: rama por defecto `production`, colaboradores con rol Write, y protecciones en `production`, `testing` y `development` según la tabla de la sección 6.
3. Los pasos 2 y 3 (remoto y publicar ramas) **sí** se pueden hacer con rol Write.

## 8. Verificar que quedó bien

| # | Qué hacés | Qué tenés que ver |
|---|---|---|
| 1 | `git remote -v` | Dos líneas apuntando a `https://github.com/gonzaleztomi1978-design/Sistema-Academico.git`. |
| 2 | `git branch -a` | Las 7 ramas locales **y** las 7 `remotes/origin/...`. |
| 3 | `git branch -vv` | Cada rama local con su `[origin/<rama>]` entre corchetes. |
| 4 | Abrir el repositorio en GitHub | El desplegable muestra las 7 ramas, y arriba dice **production** con la etiqueta `default`. |
| 5 | Insights > Forks | Un fork por cada integrante que ya empezó a trabajar. |
| 6 | Settings > Rules > Rulesets | Tres rulesets en **Active**. Ninguno sobre `e1`..`e4`. |
| 7 | Probar un push directo a `development` | Que **lo rechace**. Ver abajo. |

### Prueba 7: que el push directo a `development` rebote

Esta es la comprobación que de verdad importa, porque es la que sostiene todo el flujo:

```bash
git checkout development
git commit --allow-empty -m "E1: prueba de proteccion de rama"
git push origin development
```

Tiene que **fallar**:

```text
remote: error: GH006: Protected branch update failed for refs/heads/development.
remote: error: Changes must be made through a pull request.
```

**Si fue rechazado** (que es lo que esperamos), borrá el commit de prueba, que quedó solo en tu copia local:

```bash
git reset --hard HEAD~1
git checkout e1
```

**Si el push entra**, la protección no quedó aplicada: revisá que el ruleset esté en **Active**, que el target sea exactamente `development` y que la *Bypass list* esté vacía. El commit vacío ya subió: es inofensivo, dejalo, **no lo borres con `git push --force`**, y seguí con `git pull origin development`.

## 9. Problemas frecuentes

### `error: remote origin already exists.`

Ya habías agregado un remoto `origin`. No lo agregues de nuevo, corregí la URL:

```bash
git remote set-url origin https://github.com/gonzaleztomi1978-design/Sistema-Academico.git
```

### `remote: Permission to ... denied` (error 403)

Revisá a qué repositorio estabas subiendo:

1. **Si dice `gonzaleztomi1978-design/Sistema-Academico`**, quisiste subir al repositorio del proyecto, y eso solo lo puede hacer el E1. Subí a tu fork con `git push fork eX`. Si `fork` no existe, conectalo como dice el README, en "Primera vez", paso 4.
2. **Si dice tu propio usuario**, estás autenticado con otra cuenta. Mirá el punto del Administrador de credenciales, más abajo.
3. **Si el mensaje dice `protected branch`**, no es un problema de permisos: es la regla de protección funcionando.

### `remote: Support for password authentication was removed`

GitHub **ya no acepta la contraseña de tu cuenta** para pushear por HTTPS. Va un **token de acceso personal**:

1. En GitHub: foto de perfil > **Settings** (los de tu cuenta, no los del repositorio) > abajo de todo **Developer settings** > **Personal access tokens** > **Tokens (classic)** > **Generate new token (classic)**.
2. Completá: **Note** `sistema-academico`, **Expiration** lo que dure la cursada, **Scopes** tildá **`repo`**.
3. **Generate token** y **copialo en ese momento**: GitHub no te lo vuelve a mostrar.
4. Cuando Git te pida credenciales, poné tu usuario y **pegá el token en el campo de la contraseña**.

> El token es una clave: **no se sube al repositorio, no se pega en el grupo y no se comparte.** Cada uno genera el suyo. Si se te filtró, borralo desde esa misma pantalla y generá otro.

Si en vez del prompt se te abre una ventana del navegador para autorizar, también sirve.

### Windows te sigue autenticando con la cuenta vieja

Windows guarda el usuario y el token en el **Administrador de credenciales** y los reusa sin preguntarte:

1. Menú Inicio > **Administrador de credenciales** > **Credenciales de Windows**.
2. Buscá la entrada **`git:https://github.com`**.
3. Desplegala > **Quitar**.

La próxima vez que hagas `git push`, Git te vuelve a pedir usuario y token.

### `error: src refspec <rama> does not match any`

El nombre de la rama está mal escrito o no existe en tu copia local. Verificá con `git branch`: las ramas son exactamente `production`, `testing`, `development`, `e1`, `e2`, `e3` y `e4`, todas en minúscula.

## Checklist final del E1

- [ ] Repositorio creado y remoto configurado.
- [ ] Las 7 ramas publicadas, empezando por `production`.
- [ ] `production` confirmada como rama por defecto.
- [ ] Todos los integrantes avisados de que trabajan con fork, con el link al README.
- [ ] Rulesets activos en `production`, `testing` y `development`; `e1`..`e4` sin protección.
- [ ] El check del CI agregado como obligatorio, una vez que corrió la primera vez.
- [ ] Push directo a `development` probado y **rechazado**.
- [ ] Link del repositorio compartido con los 4 equipos.
