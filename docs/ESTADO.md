# Estado real de ROOMBREAKERS

Actualización: 5 de octubre de 2026. **El juego implementado conserva Motes, reflector, Shell, guía de Pip y progreso local. Ahora hay un preparador reproducible de proyecto/escena y cuatro pruebas de integración para Unity PlayMode. Se han ejecutado las herramientas Python; el preparador C#, la presentación y las pruebas del motor siguen sin ejecutarse en Unity/Quest. No hay APK ni entrega final validada.**

## Incremento 008 — pasar del paquete a integración comprobable

`tools/setup_unity_project.py` prepara un proyecto con un Unity 6 ya instalado y activado. Interroga la versión real, la compara con ProjectVersion y bloquea discrepancias antes de importar. Solo permite crear un proyecto nuevo en un destino vacío mediante el editor. Conserva directorios no vacíos y proyectos existentes; no instala Unity, SDKs Meta, licencias o servicios.

El manifest se modifica preservando dependencias, registros y testables ajenos, con ruta local relativa al directorio Packages y una copia exacta previa. Se rechazan dependencias conflictivas, paquetes embebidos que ocultarían el repositorio, enlaces simbólicos del manifest y cambios de otro escritor. Un no-op conserva bytes y mtime. Test Framework se conserva como está instalado o se añade solo con versión exacta explícita, sin latest ni upgrades implícitos.

`DesktopProjectSetup.Prepare` está escrito para que Unity genere y guarde la escena DesktopEncounter. Comparte creación con el menú existente, valida un rig/cámara activos y sus referencias, y rechaza scripts perdidos. Una escena ya existente se inspecciona sin sobrescribirla. No cambia Build Settings ni el rig Meta. El comprobante de preparación tiene un nonce de la ejecución y debe coincidir con la versión y archivos generados. **Todavía no se ha ejecutado ese método en un editor real.**

`run_unity_checks.py` ya no acepta como validación completa solamente cuatro casos de transformaciones: exige también los ocho UnityInputFrameTests. Verifica la versión del ejecutable incluso con ruta explícita, rechaza XML antiguo y suites incompletas/fallidas, y admite una selección PlayMode separada. No usa quit durante pruebas; no transforma un fallo de motor en éxito .NET.

Las cuatro nuevas `EncounterIntegrationTests` ejercitan el FirstEncounterRig real, sus ciclos de vida y componentes visuales: pinza de arranque, captura representada en ambas escalas, recorrido hasta victoria/reinicio por controles e invalidación/deshabilitación. Sus manos y habitación son fuentes sintéticas explícitas. Los journals se aíslan en memoria antes de activar componentes, sin tocar el progreso del jugador. **Son pruebas escritas, no cuatro pruebas del motor aprobadas.**

Procedimiento, comandos y fuentes oficiales: [ARRANQUE_UNITY](ARRANQUE_UNITY.md). Alcances: [AUTOCHECKS](AUTOCHECKS.md). La política de una autoridad de juego y dos representaciones no cambia.

## Evidencia ejecutada durante este incremento

En el entorno local se escribieron y ejecutaron los archivos Python del preparador/verificador y sus tests. La ejecución final local mostró **48 tests aprobados, cero fallos**: 26 del verificador y 22 del preparador. El conjunto local no incluye los 20 tests de referencia previos; esos se vuelven a ejecutar en el checkout completo de GitHub Actions. No se afirma clonación local del repositorio.

Los tests de herramientas usan respuestas de proceso y XML sintéticos, más operaciones reales de archivos temporales. No son pruebas de Unity. Cubren conservación del manifest, backups, rutas con espacios, identidad de paquetes, versión incorrecta, destino ocupado, comprobantes viejos/incompletos, XML omitido/obsoleto y ausencia de editor. El caso de exit code fallido produce un XML nuevo dentro del proceso simulado para no quedar accidentalmente cubierto solo por la protección contra XML viejo.

Se ejecutaron además los comandos reales sin editor/proyecto en el entorno local:

- `python3 tools/setup_unity_project.py --create --verify`: **BLOCKED, código 2**, editor ausente; no creó proyecto ni ejecutó Unity.
- `python3 tools/run_unity_checks.py`: **BLOCKED, código 2**, proyecto ausente; cero casos del motor ejecutados.

La revisión del entorno no encontró Unity, dotnet o mono accesibles en PATH, ni visor conectado. Se usa Actions para comprobar el núcleo C#. La búsqueda de una integración de editor adicional no aportó una conexión Unity utilizable. No se ha obtenido acceso al Mac del usuario.

Las ocho suites .NET de dominio no fueron modificadas por este incremento. La evidencia previa es **265 casos C# aprobados**, registrada en PR #7; los checks de este PR vuelven a compilarlas. Los resultados finales, SHA exacto y jobs observados de la rama actual se registran en [PR #8](https://github.com/zhbozzo/metavr/pull/8) antes de integrar, sin inventar resultados mientras estén en ejecución.

## Qué sigue sin ejecutarse

| Verificación | Estado |
| --- | --- |
| Unity crea/importa proyecto y ejecuta el preparador C# | NOT RUN |
| Compilación Unity y SDK Meta/MRUK instalados | NOT RUN |
| 12 casos EditMode existentes | NOT RUN |
| 4 casos PlayMode de integración nuevos | NOT RUN |
| Demo observado en escritorio | NOT RUN |
| Build Android / instalación Quest | NOT RUN |
| Persistencia Android / sensores / confort / rendimiento | NOT RUN |
| Store, canal Competition o candidatura | No publicados/enviados |

`PREPARED` no significa pruebas del motor aprobadas. `PASS` de Python o .NET no significa Unity o Quest aprobado. El preparador solo puede informar PASS de integración con --verify después de recibir los informes completos del motor. Ni siquiera ese resultado evalúa calidad audiovisual o comodidad.

## Producto conservado y límites

El encuentro conecta tutorial de captura, tres Motes, orientación del reflector, pulso reflejado, vulnerabilidad de Shell y devolución final. Pip da ayudas contextuales por tiempo activo; el progreso cosmético del refugio usa checkpoints locales y no registra manos/planos. El guardado es de resultados, no de una partida en curso, ni cloud save o almacenamiento concurrente.

El reflector permanece fijo, admite un pulso y un rebote; la sala admite un piso horizontal y obstáculos conservadores. Un corredor no válido produce recuperación visible. Las manos/salas de pruebas no demuestran sensores reales. La mano ampliada sigue esquemática; no están calibrados los seis a ocho minutos, varios encuentros, accesibilidad completa o arte final.

No se añadieron compras, nube, telemetría, cambios de visibilidad/licencia, archivos domésticos o publicación. Los logs del preparador quedan locales bajo .validation-local y pueden contener rutas de máquina; no se suben automáticamente. Un import fallido conserva el proyecto y su manifest para diagnóstico, sin borrar o restaurar por encima de cambios posteriores.

## Historial y evidencia anterior

- [Estado completo del incremento 007](https://github.com/zhbozzo/metavr/blob/3bf7df72093875ce0078e9c3b06577d0ac18153c/docs/ESTADO.md), conservado en Git.
- [PR #7](https://github.com/zhbozzo/metavr/pull/7): guía, resultados y progreso. [CI detallado 37354563527](https://github.com/zhbozzo/metavr/actions/runs/37354563527): 265 C#. [CI final C#](https://github.com/zhbozzo/metavr/actions/runs/37354852843) y [Python](https://github.com/zhbozzo/metavr/actions/runs/37354852780): 265 C# y 36 Python previos. La regresión de checkpoints contradictorios se reprodujo y corrigió, sin relajar su test.
- [PR #6](https://github.com/zhbozzo/metavr/pull/6): Shell/reflector. [CI 37311959004](https://github.com/zhbozzo/metavr/actions/runs/37311959004): 218 C#.
- [PR #5](https://github.com/zhbozzo/metavr/pull/5): geometría, MRUK escrito y primer encuentro. [CI 37256138119](https://github.com/zhbozzo/metavr/actions/runs/37256138119): 183 C#.
- [PR #4](https://github.com/zhbozzo/metavr/pull/4): controles. [CI 37255395099](https://github.com/zhbozzo/metavr/actions/runs/37255395099): 148 C#.
- [PR #3](https://github.com/zhbozzo/metavr/pull/3): manos/watchdog. [CI 37250720836](https://github.com/zhbozzo/metavr/actions/runs/37250720836): 110 C#.
- [PR #2](https://github.com/zhbozzo/metavr/pull/2): poses antiguas/desbordamiento. [CI 37249508028](https://github.com/zhbozzo/metavr/actions/runs/37249508028): 64 C#.
- [PR #1](https://github.com/zhbozzo/metavr/pull/1): núcleo y Scale Lab. [CI 37248189333](https://github.com/zhbozzo/metavr/actions/runs/37248189333): 49 C#.

## Próxima aceptación

RB-001 sigue pendiente de editor/SDK/APK. El siguiente avance de integración es ejecutar el preparador con un editor real, resolver cualquier error de compilación, correr EditMode/PlayMode, abrir el demo y revisar la experiencia. Después, configurar el rig Meta y probar exactamente las mismas reglas en Quest. No se marca un hito de dispositivo completo por añadir código de preparación.
