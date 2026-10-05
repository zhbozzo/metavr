# Estado real de ROOMBREAKERS

Actualización: sesión del 4 de octubre de 2026 en Chile (CI del 5 de octubre UTC). **Núcleo, controlador de manos y lógica de controles implementados y probados en .NET. Presentación Unity y adaptador Meta escritos, aún no compilados/ejecutados en Unity. No existe APK ni app Quest validada.**

## Incremento 004: controles del laboratorio dentro del visor

Se añadieron `NearControlDriver` y `HarnessControls`: fichas de pausa/reanudación, confirmación de reinicio y recolocación, cancelación, prioridades entre menú y captura, datos temporizados y un motivo de pausa Menu independiente. La selección se confirma al abrir los dedos sobre la misma ficha. Alejarse, perder datos o mover/deshabilitar el objetivo cancela. No se puede confirmar con una pose vieja ni utilizar el mismo gesto para pulsar un control y agarrar el objeto.

RESTART no borra el estado hasta una segunda elección explícita. CANCEL conserva el resultado y pausas previas. La mano libre puede pausar una captura; la que sostiene el objeto no activa controles al mismo tiempo. Los flujos están probados mediante entradas sintéticas sobre la composición real de controladores, no mediante sustitutos de la lógica.

`HandsControlPanel` crea la presentación espacial y `HandsScaleRig` la conecta a las fuentes de manos existentes. Esa parte es código escrito, no interfaz observada. Hay fichas PAUSE/RESUME, RESTART y MOVE, etiquetas y feedback de hover/pinza. Sus zonas tienen tamaño físico independiente de la escala de la maqueta y no cambian al resaltarse. La colocación normal de MOVE cambia solo la maqueta, no la cámara, el marco grande ni la pose canónica. Confirma primero y rechaza configuraciones inválidas. Los controles Inspector permanecen como herramientas de depuración; un fallo estructural del rig todavía puede requerir corregir su configuración.

Uso y pruebas manuales pendientes en [CONTROLES](CONTROLES.md). No se añadieron enemigos, nube, compras, telemetría ni publicación.

## Evidencia actual

| Comprobación | Resultado observado |
| --- | --- |
| Núcleo C# como .NET Standard 2.1 | PASS: compila |
| Suite original C# | PASS: 49 casos |
| Regresión/robustez C# | PASS: 15 casos |
| Controlador de manos C# | PASS: 46 casos |
| Controles y coordinación C# | PASS: 38 casos nuevos |
| Total C# | 148 aprobados, 0 fallidos |
| Nuevo estrés | 5.000 frames sintéticos en uno de los 38 casos, no pruebas de usuarios/dispositivo |
| Python/documentos | Última evidencia anterior: 36 tests, PASS; comprobar también checks finales de este incremento |
| Presentación del menú y conexión Unity escritas | Sí; no compiladas/observadas en el motor |
| Import/compilación Unity y adaptador Meta | NOT RUN |
| Tests Unity EditMode | 12 casos anteriores preparados (4 + 8); NOT RUN |
| Play Mode, APK, sensores Quest, confort/rendimiento | NOT RUN |
| Room scan / passthrough de la app | Pendientes de integración en un build |
| Canal Competition / Store / candidatura | No publicados/enviados |

[CI C# 37254868498](https://github.com/zhbozzo/metavr/actions/runs/37254868498), commit `5cf905a9491be9ca53f0e6491a916837e57e9fbd`, job `111589675965`: logs leídos con `RESULT: 49 passed, 0 failed`, `HARDENING: 15 passed, 0 failed`, `HAND INPUT: 46 passed, 0 failed` y `CONTROLS: 38 passed, 0 failed`. El commit posterior `f9fd70eb841a506ee95413495d5688c23d1f9d9d` conecta la UI Unity; no modifica esas suites/núcleo. Los checks finales de la rama/PR se revisan antes de integrar.

Runner Ubuntu 24.04; SDK .NET disponible en la imagen, máximo listado 10.0.401; núcleo netstandard2.1, runners net8.0, C# 8. Sin NuGet de terceros. La compilación NO incluye UnityEngine, fuentes Meta, shaders, fuentes tipográficas ni editor; no se usan stubs para fingir validación del motor.

En la regresión de motivo de pausa desconocido se actualizó el valor rechazado de 16 a 32, porque 16 ahora es el motivo Menu. Se conservaron las 49 pruebas anteriores y sus restantes afirmaciones. Las pruebas nuevas verifican además la separación de Menu, User, FocusLost, Placement y TrackingLost.

## Incrementos anteriores conservados

- [PR #3](https://github.com/zhbozzo/metavr/pull/3): controlador de manos, fuente Meta IHand opcional, watchdog, laboratorio y jerarquías. [CI C# 37250720836](https://github.com/zhbozzo/metavr/actions/runs/37250720836): 49 + 15 + 46 casos. Checks finales [37251398267](https://github.com/zhbozzo/metavr/actions/runs/37251398267) y [37251398282](https://github.com/zhbozzo/metavr/actions/runs/37251398282): C# y 36 Python aprobados. La representación ampliada sigue siendo una esfera, no una mano articulada.
- [PR #2](https://github.com/zhbozzo/metavr/pull/2): corrigió premios basados en poses antiguas y desbordamiento al proyectar vistas. [CI 37249508028](https://github.com/zhbozzo/metavr/actions/runs/37249508028): 49 + 15 casos. [Regresiones antes del arreglo 37249267579](https://github.com/zhbozzo/metavr/actions/runs/37249267579): seis casos fallidos que después pasaron; dos clases de defectos, no problemas de hardware.
- [PR #1](https://github.com/zhbozzo/metavr/pull/1): núcleo 6DoF y laboratorio sintético con ratón. [CI inicial C# 37248189333](https://github.com/zhbozzo/metavr/actions/runs/37248189333): 49 casos.
- [Foundation inicial 37241476190](https://github.com/zhbozzo/metavr/actions/runs/37241476190): documentación y referencia Python aprobadas.

`run_unity_checks.py` requiere editor/proyecto real y XML NUnit. Los tests Python del verificador usan XML sintético y procesos simulados, no ejecutan Unity. El mínimo de presencia que inspecciona sigue siendo la suite original de cuatro casos; la ejecución real debe revisar además los ocho casos UnityInputFrameTests. El lanzamiento previo sin editor/proyecto devolvió BLOCKED, no PASS.

## Bloqueos reales

Se volvió a inspeccionar el entorno de esta sesión: no hay ejecutable Unity ni dotnet disponible en PATH, ni visor conectado. La resolución local de github.com no produjo una dirección; no se afirma clonación local. El código se escribe y compila mediante la conexión GitHub y Actions. No se descargaron/instalaron SDKs, aceptaron licencias ni fabricaron ProjectSettings, escenas YAML o GUIDs.

El adaptador Meta previo sigue restringido a la familia IHand v207.x revisada, sin compilación instalada. Compatibilidad real de Unity, Meta Core/Interaction/MRUK/provider y Android tooling pendiente. El laboratorio usa distribución sintética; no reconoce mobiliario ni detecta obstáculos. Los umbrales, distancias y tamaños del menú son propuestas a contrastar en el visor. No se registran poses ni datos domésticos.

## Backlog y siguiente paso

RB-001 sigue bloqueado para editor/SDK y APK real. RB-002 tiene matemática .NET comprobada, no contraste motor. RB-003 tiene vistas escritas. RB-004/RB-006 avanzan con captura, recuperación y ahora controles integrados en código; su aceptación en visor sigue pendiente. Ningún hito de hardware se marca completo por pasar tests .NET.

La comprobación integrada prioritaria es importar el paquete, ejecutar los 12 casos Unity y recorrer captura → retorno → pausa/reanudar → reiniciar/cancelar → recolocar con una sola mano. Después, sustituir la distribución sintética por datos validados de habitación. No añadir un boss ni backend para ocultar el bloqueo de integración.
