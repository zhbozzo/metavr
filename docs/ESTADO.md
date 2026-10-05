# Estado real de ROOMBREAKERS

Actualización: sesión del 4 de octubre de 2026 en Chile (CI del 5 de octubre UTC). **Hay un primer encuentro implementado en código, con geometría de habitación, captura, criaturas móviles, victoria, derrota y reinicio. El núcleo C# está compilado y probado; los scripts Unity/Meta y su presentación siguen pendientes de compilación/ejecución dentro del motor. No hay APK ni app Quest validada.**

## Incremento 005: de laboratorio de controles a primer encuentro

- `RoomGeometry.cs`: suelo validado, paredes y obstáculos inmutables; pruebas de volumen barrido; planificador acotado de refugio, portal y ruta en un sector frontal. Una obstrucción cambia la configuración; una sala sin solución se rechaza, no se inventa.
- `FirstEncounter.cs`: una criatura a la vez sobre la misma `ScaleSession` y los mismos controladores. Tutorial sin ataque, movimiento autónomo por ruta, tres devoluciones para ganar, tres daños al refugio para perder, pausa y reinicio con confirmación. Invalidar la habitación no se puede arreglar pulsando Resume o Restart.
- `ScaleSession.TryAdvance` y restricción de movimiento compartida: el arrastre y el avance autónomo verifican geometría. No hay dos físicas; cada acierto se cuenta una vez. `HarnessControls.RestartSerial` comunica un reinicio explícito al encuentro.
- `RoomSource` y `MetaRoomSource`: carga explícita de datos locales del dispositivo, permiso espacial, validación de sala/anchor y estados recuperables. MRUK v207 documentado, NO instalado/compilado. La fuente real no usa fallback sintético; invalidación ante cambios o pérdida de localización.
- `FirstEncounterRig` y `FirstEncounterView`: código de maqueta derivada de geometría, portal, Motes, Pip/refugio, mano esquemática ampliada, zonas visibles de devolución, tutorial, resultado y audio espacial provisional generado en código. No se ha observado su renderizado. La mano no es una malla articulada rastreada.
- Dos rutas de editor preparadas: `Open First Encounter (Desktop)` con ratón/sala sintéticos etiquetados y `Add Device Room Encounter` sobre un rig Meta existente. Ambas requieren ejecutarse con Unity real. No se crea una cámara nueva en la ruta de dispositivo ni se configura passthrough silenciosamente.

Detalles y límites en [PRIMER_ENCUENTRO](PRIMER_ENCUENTRO.md). Se preservan los laboratorios anteriores, fuentes de manos, controles y herramientas de validación.

## Evidencia observada de este incremento

[CI 37256138119](https://github.com/zhbozzo/metavr/actions/runs/37256138119), commit `ee802654c33132d1cb7792fd4b8aff8a040059ca`, job `111593509678`. Los logs se leyeron y muestran:

| Suite | Resultado |
| --- | --- |
| Núcleo / transformaciones | 49 aprobados, 0 fallidos |
| Robustez / regresiones | 15 aprobados, 0 fallidos |
| Entrada de manos | 46 aprobados, 0 fallidos |
| Controles y coordinación | 38 aprobados, 0 fallidos |
| Habitación y primer encuentro | 35 aprobados, 0 fallidos |
| **Total C#** | **183 aprobados, 0 fallidos** |

Uno de los casos nuevos procesa cien disposiciones sintéticas: exige rutas libres para los planes aceptados y mensaje explícito para los rechazados. No son cien habitaciones reales ni cien pruebas en visor. La victoria, derrota y reinicio se recorren con entradas sintéticas sobre el mismo código de dominio usado por Unity.

El núcleo y las cinco suites no cambiaron después de esa ejecución. Los commits siguientes añaden y corrigen integración/presentación Unity y documentación. Los checks finales de la rama/PR deben revisarse antes de integrar.

La compilación usa .NET disponible en Actions (máximo SDK listado 10.0.401), núcleo netstandard2.1, tests net8.0 y C# 8.0. Sin NuGet de terceros. No se compilan UnityEngine, MRUK, IHand, shaders ni editor mediante estas pruebas y no se usan stubs para simular una validación del motor.

## Qué todavía no se ha ejecutado

| Comprobación | Estado |
| --- | --- |
| Import y compilación en Unity | NOT RUN |
| Compilación contra SDKs Meta reales | NOT RUN |
| 12 pruebas EditMode previamente escritas | NOT RUN |
| Escena de escritorio / Play Mode | NOT RUN |
| Solicitud real de permiso y carga MRUK en visor | NOT RUN |
| APK Android / ejecución Quest | NOT RUN |
| Calidad visual, comodidad y rendimiento | NOT RUN |
| Candidatura, Store o canal Competition | No publicados/enviados |

Python: última evidencia anterior son 36 tests y documentos aprobados en [37255395001](https://github.com/zhbozzo/metavr/actions/runs/37255395001), job `111591232074`. Sus archivos no fueron modificados por este incremento; el check final de esta rama vuelve a ejecutarlos.

## Restricciones concretas del prototipo

Una sala de un solo piso horizontal, paredes verticales rectangulares, obstáculos conservadores y criaturas flotando a una altura seleccionada. Sin navegación arbitraria por muebles, detección completa de objetos móviles ni promesas de seguridad física. Geometría fuera del alcance produce recuperación, no aproximaciones invisibles. Datos y geometría permanecen en memoria local, sin UUID o planos en logs públicos.

La primera criatura sirve de tutorial; después se reutiliza el mismo tipo de enemigo. No se han implementado todavía Shell, reflector, varios portales activos, guardado de progreso, arte final o la sesión completa de seis a ocho minutos. No se afirma que el código nuevo alcance una tasa de fps antes de perfilar el APK.

## Historial comprobado

- [PR #4](https://github.com/zhbozzo/metavr/pull/4): fichas hands-first, prioridad de UI, confirmación y MOVE. [CI final 37255395099](https://github.com/zhbozzo/metavr/actions/runs/37255395099): 148 C#; [37255395001](https://github.com/zhbozzo/metavr/actions/runs/37255395001): 36 Python y documentos.
- [PR #3](https://github.com/zhbozzo/metavr/pull/3): input de manos, watchdog y fuente IHand. [CI 37250720836](https://github.com/zhbozzo/metavr/actions/runs/37250720836): 110 C#.
- [PR #2](https://github.com/zhbozzo/metavr/pull/2): corrigió retorno con pose vieja y desbordamiento de vistas. [Antes del arreglo 37249267579](https://github.com/zhbozzo/metavr/actions/runs/37249267579): seis casos nuevos fallaban; [después 37249508028](https://github.com/zhbozzo/metavr/actions/runs/37249508028): 64 C# aprobados.
- [PR #1](https://github.com/zhbozzo/metavr/pull/1): núcleo 6DoF y Scale Lab. [CI 37248189333](https://github.com/zhbozzo/metavr/actions/runs/37248189333): 49 C#.
- [Foundation inicial 37241476190](https://github.com/zhbozzo/metavr/actions/runs/37241476190): referencia Python y documentación.

## Integración pendiente y próxima aceptación

El entorno remoto no tiene Unity ni visor conectado; la compilación C# ocurre en Actions. No se aceptaron licencias ni se inventaron ProjectSettings, escenas YAML, GUIDs o resultados de hardware. La familia de APIs MRUK/IHand v207 fue contrastada con documentación oficial, no con una instalación y build ya ejecutados.

RB-001 continúa pendiente de editor/SDK/APK. RB-007/008/009/010/013 ahora tienen incrementos concretos de carga, planificación, portal, criatura y sesión inicial; no cumplen todavía sus aceptaciones de dispositivo. El bloque siguiente de aceptación es importar el paquete, resolver errores reales de SDK/motor, ejecutar la escena de escritorio y la de dispositivo, y comprobar que la habitación real y el arrastre coincidan.

`tools/run_unity_checks.py` necesita editor/proyecto real. Sus tests Python son del verificador, no de Unity. Su requisito mínimo de presencia sigue centrado en los cuatro tests originales de transformaciones; revisar también los ocho UnityInputFrameTests. La ejecución previa sin editor devolvió BLOCKED, nunca PASS.
