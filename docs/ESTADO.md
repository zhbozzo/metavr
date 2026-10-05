# Estado real de ROOMBREAKERS

Actualización: 5 de octubre de 2026. **Encuentro ampliado implementado: Motes → reflector → pulso devuelto → Shell vulnerable → captura final. El núcleo C# está compilado y probado. Presentación Unity y adaptadores Meta/MRUK siguen sin import/compilación o ejecución en Unity/Quest. No hay APK ni entrega final validada.**

## Incremento 006 — Shell y reflector

`ScaleSession` incorpora orientación sobre soporte fijo sin romper la captura anterior: no traslada el soporte, una colocación no puntúa como criatura devuelta, una muestra inválida no confirma un ángulo antiguo y cancelar restaura la orientación inicial de la captura.

`ShellDuel.cs` incorpora un corredor validado en la habitación, pulso único con un rebote, previsualización limitada por geometría, aviso previo, vulnerabilidad, consumo único de un fallo y resolución final. No dispara durante la espera inicial; una colocación intencional inicia el ciclo. La armadura no se reactiva mientras Shell está sujeto.

`FirstEncounter` conecta los tres Motes con el tramo Shell, conserva la integridad restante y exige devolver la criatura final para ganar. Cambiar el objeto activo no arrastra una pinza antigua a la siguiente entidad. Reiniciar vuelve al tutorial; las pausas e invalidación de sala no se eluden.

`FirstEncounterRig` activa el recorrido ampliado por defecto en las dos entradas de escena existentes. Comprueba el corredor de reflexión antes de iniciar la partida. El modo básico de tres Motes sigue disponible como práctica/regresión, no como supuesto test de Shell.

Presentación escrita: `ShellDuelView`, mejoras a `FirstEncounterView` y selección del objetivo activo. Armadura segmentada, reflector sobre soporte, normal visible, trayectorias, pulso, anillo de aviso, tres luces de integridad del refugio y tonos procedurales para los eventos. `DesktopEncounterHand` añade Q/E al mantener pulsado el reflector. Ninguna de esas vistas aporta física o daño independiente.

[Detalles, procedimiento y límites de Shell/reflector](SHELL_REFLECTOR.md).

## Evidencia observada

[CI C# 37311959004](https://github.com/zhbozzo/metavr/actions/runs/37311959004), job `111769272682`. Head del PR: `99cb90e1796524aa24a5385bac0a7d53c9c4bd48`; merge temporal comprobado: `fefeabf13e83f30b6c87e9c9773d2ba031bd4156`. Logs leídos con:

| Suite | Resultado |
| --- | --- |
| Núcleo / transformaciones | 49 PASS |
| Robustez / regresiones | 15 PASS |
| Entrada de manos | 46 PASS |
| Controles y coordinación | 38 PASS |
| Habitación y Motes | 35 PASS |
| Shell / reflector / recorrido ampliado | 35 PASS |
| **Total C#** | **218 aprobados, cero fallidos** |

Incluye giro y liberación del reflector sin recompensa indebida, colisión barrida, proyección de trayectoria, pausa del proyectil, captura mantenida más allá de la ventana, pérdida de seguimiento, reinicio, invalidación de sala y un recorrido con entradas de una mano hasta victoria. Un caso recorre cien secuencias de rotación con semilla fija; no son cien usuarios o pruebas de visor.

El núcleo y esas seis suites no se modificaron después de ese CI. Los commits siguientes incorporan presentación/conexión Unity y documentos. Consultar los checks finales del PR #6 antes de integrar.

Compilación en Actions con .NET disponible en la imagen Ubuntu 24.04, máximo SDK listado 10.0.401; núcleo netstandard2.1, ejecutables de pruebas net8.0, lenguaje C# 8, sin paquetes NuGet de terceros. Las pruebas compilan el código real del núcleo, no stubs de UnityEngine. No incluyen scripts de presentación, SDKs, sensores ni medidas de rendimiento.

## Integración que no se ha ejecutado

| Comprobación | Estado |
| --- | --- |
| Unity import / compilación del motor | NOT RUN |
| Adaptadores Meta/MRUK compilados contra instalación real | NOT RUN |
| 12 casos EditMode anteriores | NOT RUN |
| Escena de escritorio / Play Mode | NOT RUN |
| APK Android / Quest | NOT RUN |
| Apariencia, audio, legibilidad y confort | NOT RUN |
| Rendimiento CPU/GPU o tasa de fps | NOT RUN |
| Canal Competition, Store o candidatura | No publicados/enviados |

El entorno local de esta sesión se inspeccionó: no hay Unity, dotnet, csc o mono disponibles en PATH; github.com, api.github.com y el host de distribución .NET no resuelven desde ese shell. No se afirma clonación ni ejecución local del motor. Las escrituras y los tests C# se ejecutan mediante GitHub/Actions.

Python: los 36 tests existentes no cambiaron. Su ejecución anterior comprobada es [37311958803](https://github.com/zhbozzo/metavr/actions/runs/37311958803), Foundation checks completed/success; la ejecución final de este PR vuelve a comprobarlos. Este registro no inventa una nueva ejecución local.

## Límites de producto

El reflector está fijo en un soporte y orienta un pulso que llega a su centro: no hay colocación libre, múltiples rebotes o simulación física universal. La sala admite un piso horizontal, paredes verticales y obstáculos conservadores; el corredor adicional puede rechazar un encuadre que servía para el modo de Motes. Se informa el fallo, sin reemplazar la sala real por una falsa.

Los umbrales, ventana de vulnerabilidad de 10 s, distancias y velocidades son tuning inicial, no validación de ergonomía. Todavía faltan perfil en hardware, guardado persistente, ajustes de accesibilidad completos, más encuentros, arte final y calibrar seis a ocho minutos de juego. La mano ampliada sigue siendo esquemática. No se garantiza compatibilidad ni calidad por el número de tests.

No se añadieron nube, cuentas, compras, licencias, telemetría ni exportación de datos domésticos. No se han cambiado visibilidad del repositorio o SDKs instalados.

## Evidencia histórica conservada

- [PR #5](https://github.com/zhbozzo/metavr/pull/5): geometría, MRUK escrito, primer encuentro Mote. [CI inicial 37256138119](https://github.com/zhbozzo/metavr/actions/runs/37256138119): 183 C#. Checks finales [37257349689](https://github.com/zhbozzo/metavr/actions/runs/37257349689) y [37257349704](https://github.com/zhbozzo/metavr/actions/runs/37257349704): núcleo y 36 Python/documentos aprobados.
- [PR #4](https://github.com/zhbozzo/metavr/pull/4): fichas espaciales y controles. [37255395099](https://github.com/zhbozzo/metavr/actions/runs/37255395099): 148 C#; [37255395001](https://github.com/zhbozzo/metavr/actions/runs/37255395001): 36 Python/documentos.
- [PR #3](https://github.com/zhbozzo/metavr/pull/3): input, watchdog y fuente IHand. [37250720836](https://github.com/zhbozzo/metavr/actions/runs/37250720836): 110 C#.
- [PR #2](https://github.com/zhbozzo/metavr/pull/2): retorno con pose antigua y desbordamiento. [Antes 37249267579](https://github.com/zhbozzo/metavr/actions/runs/37249267579) y [después 37249508028](https://github.com/zhbozzo/metavr/actions/runs/37249508028): seis regresiones detectadas y resueltas; 64 C# aprobados.
- [PR #1](https://github.com/zhbozzo/metavr/pull/1): núcleo 6DoF y Scale Lab. [37248189333](https://github.com/zhbozzo/metavr/actions/runs/37248189333): 49 C#.
- [Foundation inicial 37241476190](https://github.com/zhbozzo/metavr/actions/runs/37241476190): documentación y referencia Python.

## Próxima aceptación

RB-011/012 ahora tienen reflector y Shell implementados/pruebas de dominio, no aceptación de visor. RB-001 y la integración Unity/Meta siguen pendientes. El siguiente ensayo útil es recorrer el demo ampliado en Unity real, resolver errores del motor/SDK y luego probar el mismo flujo en Quest, incluyendo cambio de objeto activo, orientación de muñeca, pérdida de seguimiento y pausa.

`tools/run_unity_checks.py` requiere editor y proyecto reales. Sus tests Python no ejecutan Unity. El mínimo de presencia aún se centra en cuatro tests originales; revisar los ocho UnityInputFrameTests adicionales. Un entorno sin editor produce BLOCKED, no PASS.
