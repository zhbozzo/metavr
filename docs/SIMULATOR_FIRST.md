# Decisión vigente: desarrollo sin visor físico

Decisión del propietario: 5 de octubre de 2026, hora de Chile. Referencias técnicas reconsultadas durante esta actualización (6 de octubre UTC).

## Restricción que gobierna el trabajo

**ROOMBREAKERS se desarrolla y se demuestra en computador, sin comprar, arrendar, pedir prestado ni depender de acceso a un Quest físico.** No es una etapa provisional a la espera de comprar uno. No volver a introducir la compra o una prueba de visor como requisito del siguiente hito sin una nueva decisión explícita del propietario.

Esta decisión sustituye los requisitos internos antiguos de hardware de los planes, prompts y documentos históricos del repositorio. No sustituye las reglas del concurso, no convierte pruebas pendientes en aprobadas y no cambia automáticamente el código. Los registros de PRs anteriores se conservan como historia, no como instrucciones vigentes de adquirir hardware.

## Producto y ambición que se conservan

Juego individual de realidad mixta, sentado y hands-first: una maqueta de la habitación y una representación grande consumen una sola simulación. El jugador protege a Pip devolviendo Motes y orientando un reflector para exponer y capturar Shell. Se mantienen recuperación de input, progreso local y la consecuencia visual de la mano ampliada. No volvemos a boxeo o productividad, ni migramos a WebXR por comodidad.

La tesis competitiva sigue siendo mostrar una interacción espacial clara y original, con un recorrido completo y cuidado. No se promete ganar ni se asigna una probabilidad. Bajo la restricción elegida, priorizar compatibilidad del runtime, comprensión, recuperación y presentación antes que nuevas mecánicas.

## Tres entornos distintos

| Entorno | Función | Lo que NO demuestra |
| --- | --- | --- |
| Unity desktop + ratón / muestras sintéticas | Iterar reglas, escenas y pruebas deterministas | Integración OpenXR/Meta o tracking real |
| Unity + Meta XR Simulator | Probar la ruta XR, manos simuladas, perfiles y entornos sintéticos admitidos | Ejecución del APK Android, sensores, confort físico o fps del Quest |
| APK Android para Meta VR | Artefacto de entrega que debe conservar la ruta real de manos y habitación | Instalación/ejecución comprobada por el equipo, mientras no haya evidencia |

Meta documenta XR Simulator como runtime OpenXR a nivel de API: no incluye una imagen de sistema operativo ni una capa Android. Por tanto, correr la app de escritorio dentro del simulador NO equivale a haber ejecutado el APK. [Referencia técnica R1](https://developers.meta.com/vr/documentation/unity/xrsim-intro/).

El plan completo no depende de un visor del autor, pero el destinatario final sigue siendo Meta VR. No entregar un juego exclusivo de mouse disfrazado de app hands-first.

## Host y compatibilidad por comprobar

Ruta preferida: Unity 6 en macOS Apple Silicon. La guía Unity de Meta contempla macOS ARM y OpenXR Plugin 1.13.0 o posterior; estos mínimos no prueban una combinación instalada. El simulador actual se distribuye como aplicación standalone. Verificar editor, OS, Core/Interaction/MRUK, provider, API gráfica, extensiones y versión del runtime antes de fijarlos. No mezclar instrucciones archivadas con el standalone ni desinstalar paquetes silenciosamente. [R2](https://developers.meta.com/vr/documentation/unity/unity-simulate-xrsim/).

**Environment Depth del simulador está documentado para Windows, no para esta ruta Mac.** No hacerlo indispensable para el gameplay. La geometría simplificada de escena y las colisiones conservadoras son la base; una oclusión avanzada no disponible debe declararse como tal, no fingirse con datos del sensor. [R1](https://developers.meta.com/vr/documentation/unity/xrsim-intro/).

No usar Link, Air Link, USB, ADB hacia un visor, capturas domésticas o data forwarding como dependencias de desarrollo. Sí se conserva el tooling Android necesario para compilar el APK, sin instalación en hardware propio.

## Contrato de entrada y habitación

1. Conservar dominio único y adaptadores intercambiables; no dos juegos distintos.
2. El laboratorio desktop conserva su fuente sintética explícita. No renombrarla como Meta XR Simulator ni afirmar que valida IHand.
3. La ruta XR debe recibir manos desde el runtime/SDK elegido, con teclado y ratón controlando las manos simuladas. Verificar la capacidad exacta del perfil; probar también pérdida de datos, reapertura, izquierda y derecha.
4. MRUK ofrece fuentes Device, Prefab y JSON. La ruta de runtime puede recibir datos del simulador en lugar de un sensor físico. Los fixtures Prefab/JSON son una alternativa de prueba explícita, no un scan real. [R3](https://developers.meta.com/vr/documentation/unity/unity-mr-utility-kit-manage-scene-data/).
5. Escribir los adaptadores que falten y comprobar sus APIs instaladas. La existencia de esas funciones en MRUK no significa que nuestro MetaRoomSource ya las soporte.
6. Registrar origen de datos como synthetic/runtime-simulated/device cuando la integración lo permita. No llamar real a una habitación de prueba porque una API se llame Device.
7. En la entrega Android conservar la carga de habitación consentida y la interacción real de manos. No quitar protecciones de release para activar el mouse o una sala falsa por defecto. No esconder fallos con un fallback sintético.

La guía oficial distingue manos simuladas por teclado/ratón de tracking real. [R4](https://developers.meta.com/vr/design/prototype-start-here/).

## Prioridades ejecutables

- **SIM-001:** abrir el proyecto real con el preparador existente, compilar y ejecutar EditMode/PlayMode. Registrar errores y versiones; no rehacer el núcleo ya implementado.
- **SIM-002:** conectar el runtime standalone compatible, seleccionar perfil Quest 3, verificar las extensiones necesarias y recorrer una escena mínima con manos simuladas, sin controller obligatorio.
- **SIM-003:** conectar fuentes de habitación del runtime y fixtures explícitos a RoomSnapshot; probar el encuentro completo, no solo un sample de Meta.
- **SIM-004:** ampliar la matriz de salas/input/fallos y grabar regresiones repetibles. Revisar perfil Quest 3S, FoV y escalas sin declarar hardware probado.
- **SIM-005:** generar el APK real y revisar configuración Android, dependencias, permisos, entrada de manos y separación de modos. Compilación exitosa no acredita arranque en dispositivo.
- **SIM-006:** grabar gameplay ejecutándose en XR Simulator, preparar vídeo, instrucciones y acceso de evaluación; publicar/enviar solo con autorización.

Estos IDs son tareas de integración pendientes, no funcionalidades que esta actualización documental haya construido.

## Matriz mínima propuesta de simulación

Vacía; pequeña/estrecha; escritorio; sofá/obstáculo central; habitación amplia dentro de límites; origen rotado; suelo cóncavo; paredes/superficies faltantes; sector frontal bloqueado; sin corredor reflectante; sala modificada durante una captura; ausencia temporal de datos. Cada fixture debe estar identificado, versionado y tener resultado esperado: recorrido jugable o rechazo recuperable.

En input: una mano; ambas; pinza ya cerrada al iniciar; muestras repetidas/atrasadas/inválidas; pérdida de mano propietaria; foco; pausa; reinicio; cambio de etapa; recolocación. Un caso sintético no es una prueba de usuario. No anunciar que existe esta matriz hasta crearla y ejecutarla.

## Criterios de aceptación sin hardware

Separar evidencia de documentación, .NET, Unity EditMode, Unity PlayMode, XR Simulator, compilación Android, acceso del canal y hardware. Marcar PASS, FAIL, BLOCKED o NOT RUN por nivel. Añadir 'hardware fuera del plan por decisión del propietario' sin traducirlo a PASS ni condicionar el trabajo a conseguir un visor.

El hito de simulación exige escena XR ejecutada, controles de manos simuladas de principio a fin, fixtures verificables, recuperación y una grabación del recorrido real. No basta la escena desktop con ratón. El hito de APK exige archivo producido y verificaciones de configuración; su ejecución en Quest permanece NO VALIDADA.

Medir rendimiento del host solo como rendimiento del host. No extrapolar batería, temperatura, fps, latencia o precisión de tracking al Quest. No afirmar comodidad física, oclusión real ni registro espacial comprobados. El riesgo residual de hardware permanece explícito; más tests no lo convierten en certeza.

## Entrega y vídeo

La sección What to Submit de Devpost admite vídeo del proyecto 'via XR Simulator, or another equivalent emulator'. Para Unity pide además APK en canal Competition e Invite URL. La autorización de vídeo en simulación no exime del build ni de hands-first. [R5, requisitos reconsultados con el conector Devpost](https://start-developer-competition-26.devpost.com/).

Grabar gameplay de la misma versión de código y configuración documentada, declarar entorno/perfil/fixtures y diferencias relevantes frente al APK. No usar vídeo generado por IA como evidencia ni presentar un vídeo desktop genérico como runtime XR sin justificar equivalencia. La demostración no depende de capturar una habitación privada.

Texto de transparencia previsto, a completar solo tras ejecutar las pruebas: 'Developed and demonstrated using Meta XR Simulator. Physical-headset validation has not been performed by the team.' No indicar dispositivos probados donde solo existan perfiles simulados. No enviar candidatos con fallos conocidos bloqueantes. La decisión de envío pertenece al propietario.

## Autonomía y límites del agente

Completar cambios rutinarios y pruebas accesibles sin pedir permiso por cada archivo. Usar GitHub/Actions y, cuando esté realmente disponible, el editor local. Un chat con acceso a GitHub no tiene por ello acceso al Mac o a un runtime local. Si falta Unity, registrar ese bloqueo de software y continuar trabajo útil; no sustituirlo por la exigencia de un visor.

Meta XR Operator puede investigarse como herramienta local opcional y experimental. Sus capacidades deben verificarse en la versión instalada; no basar la aceptación en supuestas herramientas de manos que no se hayan descubierto/ejecutado. No activar servicios pagos, aceptar licencias, exponer puertos o publicar credenciales. Nunca incluir el operador de depuración como requisito del jugador.

## Alcance de esta decisión

Cambia la estrategia, los documentos de trabajo y los criterios internos. No instala Unity, MRUK o XR Simulator; no crea un APK; no ejecuta un motor. El estado real y la evidencia histórica están en [ESTADO](ESTADO.md). Las instrucciones vigentes de agentes están en [AGENTS](../AGENTS.md).
