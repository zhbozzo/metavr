# Estado real de ROOMBREAKERS

Actualización: sesión del 4 de octubre de 2026 en Chile (ejecuciones CI del 5 de octubre UTC). **Núcleo y controlador de manos C# implementados y probados. Adaptador Meta y laboratorios Unity escritos, todavía no compilados/ejecutados en Unity. No existe APK ni app Quest validada.**

## Incremento 003: interacción de manos

Se implementó `Runtime/Core/HandCaptureDriver.cs`: muestras temporizadas, selección cercana con pinza e histéresis, arbitraje entre dos manos, operación con una sola mano, vigilancia de datos ausentes/atrasados, cancelación y recuperación con una nueva muestra de mano abierta. La liberación aplica primero la pose actual. No se captura automáticamente al iniciar con dedos cerrados, recuperar tracking o barrer sobre el objeto después de una pinza fallida.

Código Unity añadido: `HandSampleSource`, `UnitySpatialFrame`, `HandsScaleRig` y `HandsHarnessPlacement`. Representa una entidad en dos escalas y mantiene una esfera de feedback en la posición ampliada de la pinza. No es aún una mano articulada. Configura un espacio de prueba sintético, no un scan. Detecta cambios de marcos y solicita confirmación; rechaza jerarquías no uniformes o reflejadas; conserva pausas previas. Fallos de inicialización deshabilitan el harness y registran el error, sin continuar a medias.

`Runtime/Meta/MetaHandSampleSource.cs` lee la interfaz oficial `IHand`: datos/confianza, puntas de pulgar e índice, pose de muñeca y fuerza de pinza. Usa `CurrentDataVersion` para no renovar artificialmente datos almacenados. Su assembly y el asistente de editor solo se habilitan para el paquete Interaction SDK en `[207.0.0,208.0.0)`. Se revisó la documentación v207; no se instaló el SDK ni se comprobó su compilación. Fuentes y límites en [MANOS](MANOS.md).

El comando de editor **Tools → RoomBreakers → Add Meta Hands Harness** requiere una escena XR ya configurada y referencias explícitas de cámara/manos. No reemplaza la cámara ni instala paquetes. Los botones Inspector permiten pruebas de pausa/reinicio/colocación, pero **no son un menú hands-first dentro del visor**. Esa UI, passthrough, room scan y la malla de mano ampliada siguen pendientes de integración.

## Evidencia de este incremento

| Comprobación | Resultado observado |
| --- | --- |
| Núcleo C# como .NET Standard 2.1, suite original | PASS: 49 casos |
| Suite C# de regresión/robustez | PASS: 15 casos |
| Nuevo controlador de manos C# | PASS: 46 casos |
| Total C# | 110 aprobados, 0 fallidos |
| Iteración de nuevas entradas | 5.000 frames con semilla fija dentro de un caso; no usuarios ni sensores |
| Import/compilación de assemblies Unity/Meta | NOT RUN |
| Tests Unity EditMode | 12 casos escritos (4 anteriores + 8 nuevos), NOT RUN |
| Observación visual de ambos laboratorios / PlayMode | NOT RUN |
| APK Android, Quest, confort y rendimiento | NOT RUN |
| Room scan / passthrough / menú XR completo | No integrados en un build probado |
| Store / canal Competition / candidatura | No publicados/enviados por este incremento |

[CI C# 37250720836](https://github.com/zhbozzo/metavr/actions/runs/37250720836), commit `180d80197f1b1d7a7e961afd17b022e49aac05bb`, job `111577587660`: se leyeron logs con `RESULT: 49 passed, 0 failed`, `HARDENING: 15 passed, 0 failed` y `HAND INPUT: 46 passed, 0 failed`. El núcleo/controlador y esas suites no cambiaron después de esa ejecución; los commits siguientes ajustan Unity/editor y documentación. Los checks finales de la rama/PR deben consultarse antes de integrar.

La compilación usa .NET disponible en Actions, núcleo netstandard2.1, runners net8.0, C# 8, sin paquetes NuGet externos. El adaptador Unity y la fuente Meta NO participan en esa compilación. No se usan stubs de UnityEngine para fingir una prueba del motor.

## Evidencia anterior conservada

PR #2 corrigió dos clases de defectos: premio de una devolución basado en una pose válida antigua tras una muestra rechazada; y poses finitas que desbordaban al proyectarse en otra escala. Se comprueban ambas vistas antes de aceptar cambios; fallos conservan estado/marco anterior.

- [CI previa C# 37249508028](https://github.com/zhbozzo/metavr/actions/runs/37249508028), job 111574063117: 49 + 15 casos aprobados.
- [CI previa Python/documentos 37249508162](https://github.com/zhbozzo/metavr/actions/runs/37249508162), job 111574063797: 36 tests (20 referencia + 16 verificador) y comprobación documental aprobados.
- Ambas usaron el merge temporal `61b1fb043cf4d973cacc49598f2e93bbc59dde0f` del head `0a4c675aec71c3c1b3748095cf398c1ec2447dc5`.
- [Regresiones antes del arreglo 37249267579](https://github.com/zhbozzo/metavr/actions/runs/37249267579), commit `500617ad26fdab5e864f048939eb4fd4ea3b76ff`: 49/49 anteriores aprobados, seis casos nuevos fallidos que ahora pasan. Dos clases de errores, no seis problemas de hardware.
- [CI inicial C# 37248189333](https://github.com/zhbozzo/metavr/actions/runs/37248189333), commit `dafd74a1161fa59310817e711d202bef10ece667`: 49 casos, uno con 300 round trips.
- [Foundation inicial 37241476190](https://github.com/zhbozzo/metavr/actions/runs/37241476190), commit `a84e9b82b94cf61d8090e28a8f2325aafe5c3a6a`: documentación y referencia Python aprobadas. La sesión inicial registró 20 tests locales.

`tools/run_unity_checks.py` exige un proyecto/editor real y XML de NUnit completo. Los 16 tests Python del verificador usan informes sintéticos y procesos simulados: no acreditan ejecución Unity. El lanzamiento real previo sin editor/proyecto devolvió BLOCKED, código 2. Historial en [AUTOCHECKS](AUTOCHECKS.md).

## Bloqueos y alcance pendiente

Este entorno no dispone de editor Unity ni visor conectado. Se auditó la ausencia de compiladores .NET locales; se usa Actions para C#. La clonación GitHub desde el shell local falla por resolución DNS; no se afirma clonación o ejecución Unity local.

La combinación real de Unity, Meta Core/Interaction/MRUK/provider y Android tooling sigue por instalar/comprobar. El adaptador escrito no sustituye configurar esos paquetes y el rig. No se fabricaron ProjectSettings, escenas YAML, GUIDs ni resultados de hardware. Los umbrales de pinza, timeout y distancias son valores propuestos, no mediciones de comodidad.

El watchdog detecta falta de nuevas muestras del proveedor; no puede detectar que un proveedor avance su versión entregando datos incorrectos. La selección de proveedores reales y la recuperación del espacio del visor requieren prueba integrada. No se guardan poses privadas ni datos del entorno.

## Backlog y siguiente paso

RB-001: continúa pendiente/bloqueado para instalación de editor/SDKs y APK en visor. RB-002: matemática .NET probada, contraste con motor pendiente. RB-003: dos presentaciones escritas, observación pendiente. RB-004/RB-006: controlador de manos y recuperación avanzados; fuente Meta/glue escritos, aceptación en visor pendiente. No hay tickets de hardware completados por estos resultados.

Siguiente incremento prioritario: importar el paquete en Unity real, ejecutar los 12 casos EditMode y probar el harness con el rig de manos. Conectar después controles de pausa/reinicio a UI XR y adaptar el marco a datos reales de habitación, antes de añadir enemigos finales. El flujo y el comando están en MANOS. No hace falta aprobar cada modificación rutinaria; sí resolver licencias, acceso a dispositivo y pruebas físicas que no pueden ejecutarse aquí.
