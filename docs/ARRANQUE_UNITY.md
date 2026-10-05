# Arranque reproducible en Unity — incremento 008

Estado: herramientas Python ejecutadas con pruebas; preparador C# y cuatro pruebas PlayMode escritos, **todavía NOT RUN en Unity**. Este procedimiento no es evidencia de que exista un APK o una escena ya ejecutada. La implementación de gameplay anterior se conserva.

## Preparar el escritorio desde el repositorio

Requiere Python 3.10+, un editor Unity 6 instalado/activado y el proyecto cerrado. No se instala ni activa Unity automáticamente. Elegir el ejecutable real mediante `UNITY_EDITOR` o `--unity`; no se selecciona una versión más nueva por defecto.

```bash
# En macOS: sustituir VERSION_INSTALADA por la versión realmente instalada.
export UNITY_EDITOR="/Applications/Unity/Hub/Editor/VERSION_INSTALADA/Unity.app/Contents/MacOS/Unity"
python3 tools/setup_unity_project.py --create
```

El destino por defecto es `unity/RoomBreakers`. También se acepta `--project /ruta/al/proyecto`. Cuando falta el proyecto, `--create` permite que **Unity** lo genere en un destino vacío. Una carpeta no vacía que no sea un proyecto válido se conserva y el comando se bloquea; no borra para volver a empezar.

El flujo comprueba la versión con `Unity -version`, la compara con `ProjectVersion.txt`, conecta el paquete local mediante una ruta relativa al directorio `Packages`, preserva otras dependencias y registros, y añade `com.zh.room-breakers` a `testables`. Un paquete diferente o una copia embebida que pueda ocultar la copia del repositorio se rechaza. No se inventan ProjectSettings, GUID, escenas YAML ni lockfiles. Unity conserva la autoridad sobre su metadata.

El método de editor `DesktopProjectSetup.Prepare` utiliza la misma creación de escena que el menú anterior. Guarda una escena nueva en:

```text
Assets/RoomBreakersGenerated/DesktopEncounter.unity
```

Si ya existe, la inspecciona sin sobrescribirla. Exige un rig y una cámara activos, ausencia de scripts perdidos y referencias coherentes de fuente sintética, ratón, cámara y shader. No modifica Build Settings ni reemplaza la escena XR existente. El menú interactivo conserva la confirmación previa para guardar o descartar cambios.

Tras una preparación correcta, abrir el proyecto en Unity, abrir esa escena y pulsar Play. Sala y ratón están identificados como sintéticos. Capturar con ratón; orientar el reflector con Q/E mientras se mantiene pulsado. **El recorrido aún requiere una ejecución real; no se anuncia como validado.**

## Preparar y comprobar el motor

Con Test Framework ya instalado en el proyecto:

```bash
python3 tools/setup_unity_project.py --create --verify
```

Si falta `com.unity.test-framework`, se detiene antes de verificar. Instalar una versión compatible elegida en Package Manager, o pasar `--test-framework-version X.Y.Z` con una versión exacta. No se elige `latest` ni se cambia silenciosamente una versión fijada. Los valores usados en fixtures Python son datos de prueba, no una recomendación de versión.

También se pueden ejecutar las suites por separado:

```bash
python3 tools/run_unity_checks.py --platform EditMode
python3 tools/run_unity_checks.py --platform PlayMode
```

EditMode exige los cuatro casos originales de transformaciones y **los ocho UnityInputFrameTests**; antes el mínimo solo cubría los cuatro primeros. PlayMode exige sus cuatro casos concretos, no un XML de otra suite con igual número de tests. No se usa `-quit` al correr tests. PlayMode mantiene un dispositivo gráfico, por lo que una máquina remota sin sesión gráfica puede necesitar configuración propia; no se sustituye por una prueba de dominio.

## Qué ejercitan las cuatro pruebas PlayMode preparadas

`EncounterIntegrationTests` crea un rig real en una escena Unity temporal y fuentes explícitamente sintéticas. Ejecuta Start/Update/LateUpdate y genera los componentes visuales reales.

- Pinza de arranque → carga de habitación → primer encuentro.
- Captura/movimiento → correspondencia de posiciones en ambas representaciones.
- Motes → orientación del reflector → Shell → victoria → reinicio mediante controles espaciales, sin mover o duplicar la cámara.
- Deshabilitar/reanudar el rig e invalidar la habitación → pausa/recuperación y limpieza.

Antes de activar cualquier componente, el fixture inyecta journals de memoria. Si cambia ese contrato, falla antes del arranque. No lee/escribe el progreso del jugador. No instala un proveedor falso como si fuera IHand. Estas pruebas están escritas, pero **NO ejecutadas** en el entorno de desarrollo de esta conversación.

## Evidencia y recuperación

Cada ejecución utiliza un directorio nuevo en `.validation-local`. El comprobante del editor incluye versión, escena y un nonce de esa ejecución; se exige además la escena y su metadata generada. Un exit code cero sin comprobante no basta. Un XML anterior, pruebas omitidas/fallidas, versión distinta o informe ilegible no pasan.

`PREPARED` significa solo escena preparada; `PASS` con `--verify` exige además ambas suites del motor; `FAIL` indica una ejecución fallida y `BLOCKED` un requisito faltante. Ninguno certifica Quest, APK, confort o calidad gráfica. Ante un timeout, revisar logs y cerrar procesos Unity restantes antes de volver a intentar. Los logs no se publican automáticamente y pueden contener rutas locales; revisarlos antes de compartir.

La modificación del manifest guarda una copia exacta local antes de reemplazarlo. Si el import posterior falla, se conserva el proyecto y el cambio para diagnosticar: no se borran archivos ni se revierte por encima de posibles cambios posteriores. Es un flujo de un escritor con el editor cerrado, no una transacción concurrente.

## Fuentes oficiales contrastadas el 5 de octubre de 2026

- [CLI de Unity 6](https://docs.unity3d.com/6000.0/Documentation/Manual/EditorCommandLineArguments.html): `-version`, `-createProject`, `-executeMethod`, `-projectPath` y limitación de `-quit` con pruebas.
- [Dependencias locales](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-localpath.html): rutas relativas a `Packages`, no al directorio raíz del proyecto.
- [Manifest y testables](https://docs.unity.com/en-us/engine/6000.0/manual/packages-list/managing-packages-manifest/upm-manifest-prj).
- [Test Framework CLI](https://docs.unity3d.com/Packages/com.unity.test-framework@1.4/manual/reference-command-line.html): EditMode, PlayMode, assemblyNames y resultados.
- [EditorSceneManager.SaveScene](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/SceneManagement.EditorSceneManager.SaveScene.html): guardado por el editor.

Esto no instala ni valida una combinación Unity/Meta. La siguiente aceptación real es importar, ejecutar ambas suites, abrir el demo, revisar errores del motor y luego configurar/probar el rig Meta en Quest.
