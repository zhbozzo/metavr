# Estado real de ROOMBREAKERS

Actualización: 5 de octubre de 2026, hora de Chile (consulta/escritura del 6 de octubre UTC). **Decisión vigente: desarrollo y demostración en computador, sin comprar ni depender de visor físico.** Ver [SIMULATOR_FIRST](SIMULATOR_FIRST.md).

## Incremento 009 — contexto y plan sin hardware

Se actualizan instrucciones de agentes, prompts, contexto, entorno, plan, backlog, pruebas y entrega. Se elimina la exigencia interna de obtener un Quest o probarlo antes de continuar. Los IDs RB se conservan y se añade cola SIM-001–006 para editor, runtime, fuentes de sala, regresiones, APK y vídeo.

Esta es una actualización documental. No cambia gameplay, SDKs instalados, firmas, visibilidad o licencia. No instala Unity/XR Simulator, no conecta el Mac, no crea un APK y no ejecuta el motor. No convierte los adaptadores actuales en integración del simulador por el solo cambio de plan.

La política vigente sustituye los antiguos gates internos de hardware; los registros de implementación anteriores permanecen en Git como evidencia histórica. Hardware queda fuera del plan y NO VALIDADO, no falsamente aprobado.

## Producto y código conservados

Una simulación canónica y dos vistas. Captura/menús con watchdog, geometría/planificador, tutorial y Motes, reflector sobre soporte fijo, Shell vulnerable y devolución final. Pip tiene guía contextual y progreso cosmético con dos checkpoints locales. Sin backend, cuentas o IA generativa en runtime.

El paquete contiene rutas desktop y adaptadores Meta/MRUK escritos. La escena desktop usa ratón y datos sintéticos; no es Meta XR Simulator. El bootstrap de PR #8 crea/importa con un editor real cuando exista y exige pruebas reales; no fabrica metadata.

## Evidencia histórica confirmada, no nueva ejecución

[PR #8](https://github.com/zhbozzo/metavr/pull/8), integrado en `aead3358d922e6d1e532818e3d1c55b824bbefe5`, registró:

- [Foundation 37363316766](https://github.com/zhbozzo/metavr/actions/runs/37363316766): 68 Python aprobados y comprobación documental.
- [C# 37363316830](https://github.com/zhbozzo/metavr/actions/runs/37363316830): ocho suites completas. Logs detallados previos del mismo núcleo en [37362619274](https://github.com/zhbozzo/metavr/actions/runs/37362619274): 265 aprobados, cero fallos.
- Bootstrap y verificador ejecutados sin editor en el entorno remoto: BLOCKED/código 2, no motor ejecutado.

Los checks de este incremento documental se registran en su PR después de observarlos. No presentar estas ejecuciones históricas como nuevas. No se agregan tests de gameplay ni se cambia su número.

## Estado por nivel

| Verificación | Estado al registrar la decisión |
| --- | --- |
| Dominio C# / herramientas Python | Evidencia previa PASS, enlaces arriba |
| Unity crea/importa y ejecuta preparador C# | NOT RUN |
| Compilación del motor y SDKs instalados | NOT RUN |
| 12 EditMode / 4 PlayMode escritos | NOT RUN |
| Demo desktop observado | NOT RUN |
| Meta XR Simulator + SDK + manos simuladas | NOT RUN |
| Fuentes MRUK del runtime / Prefab / JSON conectadas y recorridas | Pendientes de auditoría/integración |
| Matriz simulator-first completa | Planificada, no creada/ejecutada íntegramente |
| Compilación APK Android | NOT RUN |
| Instalación / sensores / confort / fps Quest | NO VALIDADO, fuera del plan por decisión del propietario |
| Canal Competition, vídeo publicado o candidatura | No realizados por esta actualización |

No hay editor ni runtime XR accesibles desde esta sesión de GitHub. Esto es una limitación del entorno actual, no evidencia de que el Mac no pueda instalarlos. No se ha obtenido acceso remoto al equipo. Los comandos futuros requieren herramientas reales y licencia del usuario cuando corresponda.

## Restricciones técnicas adoptadas

Unity/C# permanece; no migración WebXR. Los perfiles de simulador no prueban hardware. XR Simulator es runtime de API sin imagen Android; el APK se produce por separado. En Mac no hacer indispensable Environment Depth del simulador, documentado para Windows. Referencias y detalles en SIMULATOR_FIRST.

La ruta del APK mantiene manos reales y carga consentida de habitación para jueces. Los fixtures son explícitos en desarrollo; no quitar protecciones de release ni sustituir fallos silenciosamente. La separación de progreso práctica/release permanece.

El reflector sigue fijo, un pulso y un rebote; sala de piso horizontal/obstáculos conservadores; arte/mano provisionales. No se ha calibrado duración, accesibilidad completa, presentación o rendimiento. La guía y las recompensas no equivalen a más niveles.

## Próxima tarea

**SIM-001:** ejecutar el preparador con editor real, resolver errores y correr EditMode/PlayMode. **Después SIM-002:** runtime Meta XR Simulator y una interacción de manos simuladas por SDK. No pedir compra, arriendo, préstamo o USB como paso previo.

Un agente debe registrar exactamente el nivel observado, conservar cambios ajenos y no publicar/envíar sin autorización. Si no dispone del editor, avanzar trabajo verificable e informar la dependencia de software, sin inventar pruebas.

## Historia conservada

[Estado íntegro anterior, incremento 008](https://github.com/zhbozzo/metavr/blob/aead3358d922e6d1e532818e3d1c55b824bbefe5/docs/ESTADO.md) conserva detalles de implementación y enlaces de PR #1–#8. No se borra ni se reescribe su evidencia en retrospectiva. Las obligaciones operativas de hardware que allí aparezcan están sustituidas por la decisión 009.
