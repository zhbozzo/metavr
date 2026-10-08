# Estado real de ROOMBREAKERS

## Incremento 010 — encargo integral a Codex local

Actualización documental: 7 de octubre de 2026. El propietario ha delegado a Codex ejecutar el proyecto en este computador y encargarse también de la entrega del concurso. [Encargo completo](../prompts/CODEX_LOCAL_ENTREGA.md). Se conserva la restricción de desarrollo/demostración sin visor físico.

El handoff reúne estado recibido, mapa del código, auditoría del equipo y Git, preparación de Unity, integración de XR Simulator, calidad/pruebas, APK, vídeo, canal Competition, formulario y confirmación efectiva del envío. AGENTS y README apuntan a ese encargo. La autorización incluye las operaciones de entrega de este proyecto; no sustituye login, 2FA, permisos del cliente, acuerdos o declaraciones que requieran confirmación del titular. No incluye compras ni publicación comercial en Store.

**No se accedió a este computador desde ChatGPT ni se lanzó Codex en él.** Se preparó el encargo en GitHub para que el propietario lo pase a una sesión local. La revisión del contenedor remoto no encontró Unity/dotnet en PATH ni resolución DNS de GitHub; eso no describe el Mac. El conector GitHub sí permitió leer/escribir el repositorio.

El conector Devpost pidió reautenticación (401); no se le atribuye al usuario o al futuro Codex una sesión válida o inválida por ese resultado. Se reconsultaron por web las páginas públicas del concurso y la documentación oficial de Codex/Meta. Ningún formulario, APK o vídeo se publicó en este incremento.

Solo se modifican documentos de contexto e instrucciones. La evidencia de checks de esta rama se registra en su PR después de observarla. No se vuelven a presentar los resultados históricos como pruebas nuevas del motor.

**Próxima ejecución de Codex:** confirmar entorno local y carpeta correcta, auditar cambios existentes, correr el bootstrap y resolver los errores reales de Unity. Seguir SIM-001–006 hasta las operaciones de entrega autorizadas, dejando evidencia y pasos humanos exactos cuando existan. No responder solo con otro plan ni pedir un visor.

## Decisión conservada del incremento 009

**Desarrollo y demostración en computador, sin comprar ni depender de visor físico.** Ver [SIMULATOR_FIRST](SIMULATOR_FIRST.md). Se actualizaron instrucciones de agentes, prompts, contexto, entorno, plan, backlog, pruebas y entrega. Se eliminó la exigencia interna de obtener un Quest o probarlo antes de continuar. Los IDs RB se conservan y se añadió la cola SIM-001–006 para editor, runtime, fuentes de sala, regresiones, APK y vídeo.

La política sustituye los antiguos gates internos de hardware; los registros de implementación anteriores permanecen en Git como evidencia histórica. Hardware queda fuera del plan y NO VALIDADO, no falsamente aprobado. Ni el cambio de plan ni este handoff conectan los adaptadores al simulador por sí solos.

## Producto y código conservados

Una simulación canónica y dos vistas. Captura/menús con watchdog, geometría/planificador, tutorial y Motes, reflector sobre soporte fijo, Shell vulnerable y devolución final. Pip tiene guía contextual y progreso cosmético con dos checkpoints locales. Sin backend, cuentas o IA generativa en runtime.

El paquete contiene rutas desktop y adaptadores Meta/MRUK escritos. La escena desktop usa ratón y datos sintéticos; no es Meta XR Simulator. El bootstrap de PR #8 crea/importa con un editor real cuando exista y exige pruebas reales; no fabrica metadata.

## Evidencia histórica confirmada, no nueva ejecución

[PR #8](https://github.com/zhbozzo/metavr/pull/8), integrado en `aead3358d922e6d1e532818e3d1c55b824bbefe5`, registró:

- [Foundation 37363316766](https://github.com/zhbozzo/metavr/actions/runs/37363316766): 68 Python aprobados y comprobación documental.
- [C# 37363316830](https://github.com/zhbozzo/metavr/actions/runs/37363316830): ocho suites completas. Logs detallados previos del mismo núcleo en [37362619274](https://github.com/zhbozzo/metavr/actions/runs/37362619274): 265 aprobados, cero fallos.
- Bootstrap y verificador ejecutados sin editor en el entorno remoto: BLOCKED/código 2, no motor ejecutado.

[PR #9](https://github.com/zhbozzo/metavr/pull/9), integrado en `a142054d9b6c48dadefe96c33d4cf4eedda07df7`, documentó la decisión simulator-first. [Foundation 37397666467](https://github.com/zhbozzo/metavr/actions/runs/37397666467): 68 Python y enlaces/archivos comprobados. No cambió código ni ejecutó Unity.

## Estado por nivel al traspasar

| Verificación | Estado recibido |
| --- | --- |
| Dominio C# / herramientas Python | Evidencia histórica PASS, enlaces arriba |
| Unity crea/importa y ejecuta preparador C# | NOT RUN |
| Compilación del motor y SDKs instalados | NOT RUN |
| 12 EditMode / 4 PlayMode escritos | NOT RUN |
| Demo desktop observado | NOT RUN |
| Meta XR Simulator + SDK + manos simuladas | NOT RUN |
| Fuentes MRUK del runtime / Prefab / JSON conectadas y recorridas | Pendientes de auditoría/integración |
| Matriz simulator-first completa | Planificada, no creada/ejecutada íntegramente |
| Compilación APK Android | NOT RUN |
| Instalación / sensores / confort / fps Quest | NO VALIDADO, fuera del plan por decisión del propietario |
| Canal Competition, vídeo publicado o candidatura | No realizados por este traspaso |

No se ha obtenido acceso remoto al equipo desde esta sesión de GitHub. Codex debe auditar sus propias herramientas/permisos y no heredar los bloqueos del contenedor remoto como si describieran su entorno local. Los comandos futuros requieren software real y activación/licencia del usuario cuando corresponda.

## Restricciones técnicas adoptadas

Unity/C# permanece; no migración WebXR. Los perfiles de simulador no prueban hardware. XR Simulator es runtime de API sin imagen Android; el APK se produce por separado. En Mac no hacer indispensable Environment Depth del simulador, documentado para Windows. Referencias y detalles en SIMULATOR_FIRST y el encargo local.

La ruta del APK mantiene manos reales y carga consentida de habitación para jueces. Los fixtures son explícitos en desarrollo; no quitar protecciones de release ni sustituir fallos silenciosamente. La separación de progreso práctica/release permanece.

El reflector sigue fijo, un pulso y un rebote; sala de piso horizontal/obstáculos conservadores; arte/mano provisionales. No se ha calibrado duración, accesibilidad completa, presentación o rendimiento. La guía y las recompensas no equivalen a más niveles.

## Historia conservada

[Estado íntegro del incremento 009](https://github.com/zhbozzo/metavr/blob/a142054d9b6c48dadefe96c33d4cf4eedda07df7/docs/ESTADO.md) y [estado del incremento 008](https://github.com/zhbozzo/metavr/blob/aead3358d922e6d1e532818e3d1c55b824bbefe5/docs/ESTADO.md) conservan el detalle y los enlaces de PR #1–#9. No se modifica su evidencia retrospectiva. Las obligaciones antiguas de hardware están sustituidas por simulator-first; la delegación local de entrega queda precisada en AGENTS y el nuevo encargo.
