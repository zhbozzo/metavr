# Estado real de ROOMBREAKERS

Actualización: 5 de octubre de 2026. **El encuentro Motes → reflector → Shell tiene ahora ayudas contextuales, resultados y progreso cosmético guardado localmente. El núcleo C# está compilado y probado. La presentación Unity y los adaptadores Meta/MRUK siguen pendientes de import/compilación y ejecución en Unity/Quest. No hay APK ni entrega final validada.**

## Incremento 007 — Pip y progreso entre partidas

`EncounterCoach` observa la misma autoridad de juego: da prioridad a interrupciones/recuperación, explica captura, retorno, giro y vulnerabilidad, e intensifica pistas tras seis/catorce segundos activos sin progreso del jugador. Moverse autónomamente no se considera progreso. Pausa y tracking perdido no consumen el reloj de ayuda; no se resuelve una acción por el jugador.

`PipGuidanceView` y `FirstEncounterView` añaden arcos de indicación en las dos escalas, énfasis de objetivos, pulsos de devolución, expresiones simples, títulos de fase y resumen final. El texto se distribuye en líneas y conserva la posición del marco UI confirmado. Los efectos son decoración, sin colisiones ni autoridad. Se retira la animación anterior del refugio basada en tiempo real aunque estuviera pausado.

`LocalProgress` conserva totales de partidas terminadas, victorias, defensas perfectas, mejor integridad y criaturas devueltas. Una, tres y cinco victorias desbloquean tres piezas cosméticas del faro de Pip. `FirstEncounter.RunId` identifica un intento local, cambia al reiniciar y se mantiene entre etapas. `EncounterExperience` observa y registra un resultado una sola vez; una invalidación de sala o un intento activo no se convierte en derrota. Completar los Motes del recorrido ampliado tampoco cuenta como victoria sin Shell.

`ProgressJournal` alterna dos checkpoints de tamaño acotado, valida versión, estructura, invariantes y checksum, y comprueba lectura después de escritura. Recupera un checkpoint anterior si es el único legible. Conserva archivos incompatibles/ambiguos sin sobrescribirlos y admite progreso solo en memoria. Un fallo de guardado se informa sin bloquear el juego. Los últimos 16 identificadores de resultado permiten reconocer reintentos recientes; no es un sistema antitrampas, concurrente o infalible frente a fallos físicos.

`FirstEncounterRig` conecta el observador y el journal a las dos rutas existentes. Práctica (editor, Development, datos sintéticos o modo básico) queda separada de release/dispositivo. Conserva journals entre recargas de sala y reintenta escrituras pendientes en límites de ciclo de vida, no cada frame. Prepara objetivos de menú antes del paso lógico y actualiza el conjunto visual una sola vez después.

**No se guardan manos, imágenes, planos, UUID de anclajes, cuentas o identificadores de visor.** Son resultados locales; no se añade backend, telemetría, pagos o autenticación. [Detalles y contrato de almacenamiento](EXPERIENCIA_PROGRESO.md).

## Evidencia observada

[CI C# 37354563527](https://github.com/zhbozzo/metavr/actions/runs/37354563527), job `111913568072`. Head del PR: `570a354f10d49de0796ee61d7bc6f90132a871a7`; merge temporal ejecutado: `231ffa55946edd1687f0dfa84fb56f5953978b5f`. Logs leídos:

| Suite | Resultado |
| --- | --- |
| Núcleo / transformaciones | 49 PASS |
| Robustez / regresiones | 15 PASS |
| Entrada de manos | 46 PASS |
| Controles y coordinación | 38 PASS |
| Habitación y Motes | 35 PASS |
| Shell / reflector | 35 PASS |
| Experience: ayudas, resultados y archivos | 39 PASS |
| ProgressRecovery: fallos y recuperación | 8 PASS |
| **Total C#** | **265 aprobados, cero fallidos** |

Las 47 pruebas nuevas incluyen recorrido de una mano hasta victoria y otra partida después de reiniciar, observaciones repetidas sin duplicación, prioridades de guía y congelación de sus relojes. Las pruebas de persistencia escriben/leen archivos temporales reales: reapertura, checkpoints truncados, UTF-8 inválido, versiones desconocidas, almacenamiento bloqueado, reintento y separación de directorios. Salas y manos siguen siendo entradas sintéticas, no datos de un visor.

### Fallo reproducido antes de corregir

[CI 37353904636](https://github.com/zhbozzo/metavr/actions/runs/37353904636), head `b28f258daf4ad4a26fb5290e57fdbb0e10932cba`, merge temporal `29f7d915ab912476a828c34df0f7670297dee1ea`, job `111911353300`: ProgressRecovery produjo siete aprobados y un fallo. Si dos checkpoints tenían el mismo número de resultados pero historias distintas, Flush podía dar éxito al coincidir uno con el contenido deseado. Ahora se comprueba la ambigüedad antes de reconocer un guardado previo. La prueba no se eliminó ni se debilitó; pasó en la ejecución verde indicada arriba.

El núcleo y las ocho suites no cambian en los commits documentales posteriores. Los checks finales del PR #7 deben consultarse antes de integrar. Las ejecuciones anteriores se conservan como evidencia de la evolución, no se describen como una prueba del commit final.

Compilación en Actions, imagen Ubuntu 24.04, SDK .NET disponible (máximo listado 10.0.401); núcleo netstandard2.1, ejecutables net8.0, C#8, sin NuGet de terceros. No se compilan UnityEngine, MRUK, IHand o shaders mediante estas pruebas; no se usan stubs del motor.

Python/documentos: Foundation checks del mismo head, [37354563589](https://github.com/zhbozzo/metavr/actions/runs/37354563589), fue observado completed/success. Las 36 pruebas existentes no se modificaron; consultar también los checks/logs del head final.

## Qué no se ha validado

| Comprobación | Estado |
| --- | --- |
| Import/compilación Unity y SDKs Meta instalados | NOT RUN |
| Los 12 tests EditMode anteriores | NOT RUN |
| Play Mode / escena desktop observada | NOT RUN |
| Almacenamiento de Android/Quest | NOT RUN |
| APK / instalación y ejecución en Quest | NOT RUN |
| Legibilidad, animaciones, audio y comodidad | NOT RUN |
| Rendimiento y coste de las escrituras en hardware | NOT RUN |
| Canal Competition, Store o candidatura | No publicados/enviados |

Se usa GitHub Actions para compilar el núcleo. No hay editor Unity ni visor accesible en este entorno; no se afirma clonación/ejecución local del motor. No se aceptan licencias ni se fabrican resultados de hardware. La integración escrita con persistentDataPath no demuestra que el guardado ya haya funcionado en Android.

## Límites del producto

El faro es una recompensa cosmética, no tres niveles nuevos. Pip usa reglas contextuales, no IA generativa. Los tiempos, tamaños y textos necesitan prueba en visor. No está calibrada la sesión final de seis a ocho minutos ni completados accesibilidad, arte final o varios encuentros. La mano ampliada sigue siendo esquemática.

El reflector sigue fijo en un soporte, recibe un único pulso por ciclo y permite un rebote; no hay física universal. La sala admite un piso horizontal, paredes y obstáculos conservadores. Un encuadre sin ruta/corredor válido se rechaza de forma visible, no se sustituye por uno sintético.

El guardado admite un escritor por perfil. Con una copia nueva dañada puede recuperar la anterior, perdiendo el último checkpoint. No garantiza persistencia ante cualquier fallo físico. Cuando un guardado pendiente falla y se cierra el proceso antes de recuperarlo, puede perderse ese progreso de sesión. No hay guardado del estado de una partida en curso, sincronización cloud o detección de fraude.

## Historial conservado

- [PR #6](https://github.com/zhbozzo/metavr/pull/6): Shell/reflector y final. [CI 37311959004](https://github.com/zhbozzo/metavr/actions/runs/37311959004): 218 C#. Checks finales [37313233627](https://github.com/zhbozzo/metavr/actions/runs/37313233627) y [37313234008](https://github.com/zhbozzo/metavr/actions/runs/37313234008): núcleo y 36 Python/documentos aprobados.
- [PR #5](https://github.com/zhbozzo/metavr/pull/5): geometría, MRUK escrito y primer encuentro. [CI 37256138119](https://github.com/zhbozzo/metavr/actions/runs/37256138119): 183 C#. [Checks finales C#](https://github.com/zhbozzo/metavr/actions/runs/37257349689) y [Python](https://github.com/zhbozzo/metavr/actions/runs/37257349704).
- [PR #4](https://github.com/zhbozzo/metavr/pull/4): controles dentro del visor. [CI 37255395099](https://github.com/zhbozzo/metavr/actions/runs/37255395099): 148 C#; [Python/documentos](https://github.com/zhbozzo/metavr/actions/runs/37255395001).
- [PR #3](https://github.com/zhbozzo/metavr/pull/3): manos, watchdog y fuente IHand. [CI 37250720836](https://github.com/zhbozzo/metavr/actions/runs/37250720836): 110 C#.
- [PR #2](https://github.com/zhbozzo/metavr/pull/2): retorno con pose antigua y desbordamiento. [Antes](https://github.com/zhbozzo/metavr/actions/runs/37249267579) / [después](https://github.com/zhbozzo/metavr/actions/runs/37249508028): seis regresiones detectadas y corregidas; 64 C#.
- [PR #1](https://github.com/zhbozzo/metavr/pull/1): núcleo 6DoF y Scale Lab. [CI 37248189333](https://github.com/zhbozzo/metavr/actions/runs/37248189333): 49 C#.
- [Foundation inicial](https://github.com/zhbozzo/metavr/actions/runs/37241476190): documentación y referencia Python.

## Próxima aceptación

La mejora avanza onboarding/feedback/persistencia en código, no cierra tickets que exijan pruebas en dispositivo. La siguiente aceptación es importar el paquete real, compilar SDKs, recorrer el encuentro, ganar, reiniciar y reabrir, y contrastar práctica/release por separado. Resolver errores del motor y medir interacción/legibilidad antes de ampliar mecánicas.

`tools/run_unity_checks.py` requiere editor/proyecto reales. Su presencia mínima aún se centra en los cuatro tests originales; revisar los ocho UnityInputFrameTests adicionales. Un entorno sin editor produce BLOCKED, no PASS.
