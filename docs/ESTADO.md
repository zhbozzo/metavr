# Estado real de ROOMBREAKERS

Actualización: 4 de octubre de 2026, hora de Chile (5 de octubre UTC). **Núcleo C# implementado y probado; no hay todavía APK ni app Quest validada.**

## Incremento actual: robustez y comprobación automática

Se corrigieron dos clases de problemas del núcleo:

- Una muestra de movimiento inválida/out-of-bounds dejaba la última pose válida sobre el destino y permitía premiar una devolución usando datos antiguos. Ahora la pose visual se conserva, pero la devolución queda invalidada hasta recibir otra muestra válida; una liberación inválida cancela la captura. Una muestra de otra mano no invalida al propietario.
- Poses canónicas finitas podían desbordarse al representarse a otra escala. Constructor, movimientos, devolución y recalibración verifican las vistas antes de cambiar el estado. Un fallo conserva el estado o marco anterior.

Se añadió `tools/run_unity_checks.py`: ejecuta los tests EditMode del paquete cuando existe un editor/proyecto real, valida el XML de NUnit y guarda reportes solo localmente. No acepta resultados ausentes, cero tests, casos omitidos o una suite ajena como PASS. Uso y límites en [AUTOCHECKS](AUTOCHECKS.md).

## Evidencia actual

| Comprobación | Resultado observado |
| --- | --- |
| C# compilado como .NET Standard 2.1; suite original | PASS: 49 tests |
| C# regresión y estrés | PASS: 15 tests |
| Total de casos C# | 64 aprobados, 0 fallidos |
| Python referencia + tests del verificador | PASS: 36 tests (20 + 16) |
| Integridad documental | PASS: 27 archivos requeridos y 20 enlaces locales simples en la ejecución indicada |
| Estrés determinista | 5.000 transformaciones y 20.000 comandos dentro de dos de los 15 tests nuevos |
| Comando Unity en entorno sin proyecto/editor | BLOCKED, código 2; no se ejecutó Unity |
| Import/compilación Unity y cuatro casos EditMode | NOT RUN |
| PlayMode / APK Android | NOT RUN |
| Passthrough, manos reales y room scan | No integrados |
| Confort, rendimiento y ejecución en Quest | NOT RUN |
| Canal Competition / candidatura | No creados ni enviados por esta implementación |

[CI C# 37249508028](https://github.com/zhbozzo/metavr/actions/runs/37249508028), job 111574063117: logs observados `RESULT: 49 passed, 0 failed` y `HARDENING: 15 passed, 0 failed`.

[CI Python/documentos 37249508162](https://github.com/zhbozzo/metavr/actions/runs/37249508162), job 111574063797: logs observados `Ran 36 tests` y `OK`.

Ambos corresponden al PR #2 sobre el head `0a4c675aec71c3c1b3748095cf398c1ec2447dc5`; GitHub comprobó el merge temporal `61b1fb043cf4d973cacc49598f2e93bbc59dde0f`. Actualizar este registro no modifica el núcleo ni los tests. La ejecución final del commit documental queda en los checks del PR.

Los 16 tests nuevos del verificador también se ejecutaron localmente con Python. Sus informes son sintéticos y sus lanzamientos están simulados para probar el script; **no son pruebas Unity**. Las iteraciones de estrés no equivalen a usuarios, habitaciones ni sensores reales.

## Evidencia de que los tests detectan el problema anterior

Antes de modificar el núcleo, [ejecución 37249267579](https://github.com/zhbozzo/metavr/actions/runs/37249267579), commit `500617ad26fdab5e864f048939eb4fd4ea3b76ff`: la suite antigua pasó 49/49; la nueva tuvo 9 casos aprobados y 6 fallidos. Esos seis casos ahora pasan. Son seis casos de regresión alrededor de dos clases de fallos, no seis problemas de hardware corregidos.

## Código existente conservado

`unity/Packages/com.zh.room-breakers` contiene SpatialMath, ScaleSession, ScaleLab, ScaleLabMenu y tests Unity. Una simulación y dos vistas; captura exclusiva y offset 6DoF, devolución única, pausa por motivos y recalibración. El laboratorio es sintético y con ratón; código de presentación escrito pero aún no ejecutado en Unity. El menú crea la escena con el editor real; no se fabricaron escenas, GUIDs o manifests Meta.

## Evidencia anterior conservada

[CI inicial C# 37248189333](https://github.com/zhbozzo/metavr/actions/runs/37248189333), commit `dafd74a1161fa59310817e711d202bef10ece667`: 49 tests. Uno contiene 300 round trips. Target netstandard2.1; tests net8.0; lenguaje C# 8.0; sin NuGet de terceros.

[Foundation inicial 37241476190](https://github.com/zhbozzo/metavr/actions/runs/37241476190), commit `a84e9b82b94cf61d8090e28a8f2325aafe5c3a6a`: documentación y referencia Python aprobadas. La sesión inicial registró 20 tests Python locales.

## Bloqueos reales

El entorno remoto no tiene Unity, dotnet, mono, csc ni un visor conectado. La conexión GitHub permite compilar y ejecutar C# en Actions. No se realizó clonación local porque el shell no resuelve github.com.

Unity/Meta Core/Interaction/MRUK/provider XR/Android tooling: combinación por verificar en un equipo real. El campo Unity 6000.0 del paquete expresa el objetivo de API, no una instalación probada. El script nuevo no instala software ni acepta acuerdos.

La lógica corregida detecta muestras explícitamente inválidas. El futuro adaptador de sensores aún debe detectar callbacks ausentes o atrasados y cancelar/pausar; no se afirma que eso ya esté implementado.

## Backlog y siguiente incremento

RB-001 continúa BLOCKED para editor, integración Meta y visor. RB-002: núcleo 6DoF implementado y probado en .NET; contraste Unity pendiente. RB-003: presentación escrita, observación pendiente. RB-004/RB-006: dominio reforzado; adaptación de manos y UX pendiente. Ningún ticket que exige visor se considera completo.

Siguiente paso ejecutable en una máquina con Unity: importar el paquete en un proyecto real, ejecutar `python3 tools/run_unity_checks.py`, luego observar Tools → RoomBreakers → Open Scale Lab → Play. Integrar después el adaptador Meta verificado, con watchdog de tracking. No se requiere aprobar cada modificación rutinaria; solo resolver accesos/acuerdos o pruebas físicas que el agente no puede realizar.
