# Estado real de ROOMBREAKERS

Actualización: 4 de octubre de 2026, hora de Chile (ejecución CI del 5 de octubre UTC). **Ahora existe núcleo C# implementado y compilado; no existe todavía APK ni app Quest validada.**

## Incremento implementado

Código en `unity/Packages/com.zh.room-breakers`:

- `SpatialMath.cs`: poses 6DoF, marcos, quaternions, correspondencia de escalas, límites de interacción y reflexión.
- `ScaleSession.cs`: una entidad autoritativa, captura exclusiva, offset de posición y orientación, movimientos validados, cancelación, devolución única, pausa por motivos y recalibración.
- `ScaleLab.cs`: laboratorio Unity sintético, dos vistas, selección con ratón, proxy ampliado, destino, reset y simulación de pérdida de tracking. Es código escrito, pendiente de ejecutar en Unity.
- `ScaleLabMenu.cs`: crea la escena mediante el editor real; no se fabricaron escenas YAML ni GUIDs.
- `SpatialFrameUnityTests.cs`: cuatro casos EditMode para contrastar con el motor; NOT RUN.

El proyecto Unity principal y los SDKs Meta todavía no se han instalado. El paquete local es una forma de incorporar código al proyecto real, no un sustituto de ese proyecto. La referencia Python y toda la especificación anterior permanecen.

## Evidencia de ejecución

| Comprobación | Estado |
| --- | --- |
| Núcleo C# compilado como .NET Standard 2.1 | PASS en GitHub Actions |
| Tests del núcleo real C# | PASS: 49 tests, 0 fallidos |
| Casos de ida/vuelta 6DoF | 300 iteraciones con semilla fija dentro de uno de esos 49 tests |
| Archivos de laboratorio/editor Unity escritos | Sí; import y ejecución pendientes |
| Import/compilación Unity | NOT RUN |
| Cuatro tests EditMode Unity | NOT RUN |
| Observación de Play Mode | NOT RUN |
| APK Android | NOT RUN |
| Passthrough, manos reales y room scan | No integrados todavía |
| Ejecución/confort/rendimiento en Quest | NOT RUN |
| Canal Competition y candidatura | No creados/enviados por esta implementación |

## Evidencia C#

[CI: Dual-scale CSharp core, ejecución 37248189333](https://github.com/zhbozzo/metavr/actions/runs/37248189333), commit `dafd74a1161fa59310817e711d202bef10ece667`, job `111570191619`. Resultado observado en logs: `RESULT: 49 passed, 0 failed`. Runner ubuntu-24.04; compilación con SDK .NET disponible en la imagen (lista máxima: 10.0.401), target del núcleo netstandard2.1, runner de tests net8.0. No hay paquetes NuGet de terceros. C# limitado a versión 8.0.

Es compilación y ejecución del código del núcleo, no del adaptador Unity. No se usaron stubs de UnityEngine para fingir validación del editor. El test de tracking prueba una señal lógica de fallo, no sensores físicos.

## Evidencia anterior conservada

[Foundation checks 37241476190](https://github.com/zhbozzo/metavr/actions/runs/37241476190), commit `a84e9b82b94cf61d8090e28a8f2325aafe5c3a6a`: comprobación documental y modelo de referencia aprobados. La sesión previa registró 20 tests Python locales; esta sesión no los describe como una nueva ejecución local. Su nueva ejecución, cuando corresponda, queda en los checks del commit/PR.

## Bloqueos y versiones

En este entorno no están instalados Unity, dotnet, mono o csc; no hay visor conectado. La conexión GitHub permitió compilar/probar C# en Actions. El shell local no resuelve github.com, por lo que no se afirma una clonación local.

Unity/Meta Core/Interaction/MRUK/provider XR/Android tooling: combinación por validar en el equipo real. El campo Unity 6000.0 del paquete indica objetivo de API, no versión instalada o compatibilidad Meta certificada. Acceso a Quest: pendiente de confirmar.

## Backlog

RB-001: BLOCKED para editor, integración Meta y dispositivo. RB-002: núcleo 6DoF implementado y probado en .NET; prueba Unity pendiente. RB-003: presentaciones escritas; observación pendiente. RB-004/RB-006: mecanismos de dominio parciales; manos, UX y dispositivo pendientes. Ningún ticket de aceptación en visor se considera completado por estos resultados.

## Próximo paso

Importar el paquete local en un proyecto Unity real; ejecutar Tools → RoomBreakers → Open Scale Lab → Play. Registrar versión completa del editor, errores de import/compilación y ejecución de los tests EditMode. Después integrar el adaptador Meta elegido y probar la misma interacción con manos y passthrough.

No construir un jefe, backend o más planificación estratégica para eludir el bloqueo del editor/visor. Consultar `docs/IMPLEMENTACION_001.md` y el README del paquete.
