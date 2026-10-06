# Decisiones y riesgos

Registro inicial: 4 de octubre de 2026. Actualización: 5 de octubre de 2026, hora de Chile. Diseño aceptado no equivale a implementación o validación.

## ADR-008 · Desarrollo y demostración sin visor físico — VIGENTE

Decisión explícita del propietario: todo en entorno de computador, sin comprar, arrendar, pedir prestado ni depender de un Quest. No es una fase de espera para comprar hardware. [Política completa y fuentes](SIMULATOR_FIRST.md).

Alternativas consideradas en la conversación: compra, préstamo y desarrollo inicial con simulador seguido de pruebas físicas. Se descartan como dependencias del plan. Se mantiene Unity, no se migra a WebXR. Los criterios de trabajo pasan a editor real → XR Simulator/manos/entornos sintéticos → APK Android → vídeo/acceso.

Consecuencias: los antiguos requisitos internos de hardware en ADR-001/007, planes y prompts quedan sustituidos. La evidencia física se declara NO VALIDADA y fuera de alcance; no bloquea hitos de simulación ni se transforma en aprobado. Las reglas externas de APK y manos siguen aplicando. El runtime simulado no contiene Android.

No cambiar el código o las protecciones de release por inferencia. Los adaptadores de datos sintéticos y SDK tienen que integrarse y probarse; no afirmar que existen por esta decisión. En Mac no depender de Environment Depth del simulador, documentado para Windows. Ver referencias en SIMULATOR_FIRST.

Riesgo aceptado en la planificación: discrepancias de sensores, comodidad, registro espacial, almacenamiento y rendimiento en hardware siguen sin comprobar. La entrega se autoriza separadamente y debe ser transparente. Esta decisión solo se reabre por solicitud explícita del propietario o un cambio externo verificado que requiera replantear el alcance; no por preferencia de otro agente.

## ADR-001 · Unity como ruta principal

Se mantiene para interacción de manos, utilidades del entorno y presentación nativa. La selección inicial WebXR del registro no describe la entrega actual. Consecuencia vigente: entorno real Unity/Android y XR Simulator; la antigua exigencia de visor fue sustituida por ADR-008.

## ADR-002 · Una autoridad, dos representaciones

Pose y eventos canónicos previenen divergencia, daño duplicado y física incoherente entre escalas. La mano grande es visual. Transformación y sincronización se prueban primero; no reparar una vista con offsets secretos.

## ADR-003 · Sector frontal y configuraciones validadas

Se mantiene por diseño sentado/FoV y alcance. No navegar por cualquier geometría. Datos insuficientes producen reconfiguración explícita. Los fixtures son de desarrollo; no un reemplazo silencioso del entorno del jugador en release.

## ADR-004 · MVP offline

Sin servidor, cuentas, latencia cloud ni exposición de geometría. No Terraform, APIs generativas, rankings globales, multiplayer o telemetría automática. Reabrir exige necesidad, presupuesto, privacidad y autorización.

## ADR-005 · Manos y movimientos controlados

Soltar en destino validado en vez de lanzamiento fuerte. Sin eye tracking obligatorio; operación secuencial con una mano, destinos claros y pruebas de falsa activación/pérdida. Se valida la ruta simulada sin afirmar precisión física.

## ADR-006 · Evidencia antes que promesas

El objetivo es competir, no aparentar producto acabado. Estados, métricas, vídeos y niveles de prueba explícitos. No garantías de premio ni plazos sin datos. Simulator-first no reduce esta exigencia.

## ADR-007 · Núcleo C# y paquete antes del editor disponible

Decisión del 4 de octubre: avanzar mediante C# .NET Standard 2.1, paquete Unity y GitHub Actions sin inventar ProjectSettings, GUIDs o APIs. La compilación de dominio no sustituye importar el motor. El bloqueo original de acceso físico se sustituye por ADR-008; el bloqueo de software que no se haya ejecutado permanece.

Detalle histórico: [versión previa del registro](https://github.com/zhbozzo/metavr/blob/aead3358d922e6d1e532818e3d1c55b824bbefe5/docs/DECISIONES.md) y [IMPLEMENTACION_001](IMPLEMENTACION_001.md).

## Registro de riesgos vigente

| Riesgo | Señal | Respuesta |
| --- | --- | --- |
| Confundir documentación con ejecución | Se anuncia XR sin sesión de runtime | Exigir evidencia por nivel y no reescribir tests para fingirla |
| Atención dividida | Alternancia poco clara entre vistas | Observar la ejecución, ajustar maqueta/sector y guías |
| Jitter amplificado | Inestabilidad en pruebas de input | Probar perturbaciones sintéticas, límites y filtros; hardware sigue no validado |
| Selección diminuta | Objetivos ambiguos | Tamaño/separación/lock medidos en el perfil simulado |
| Sala sin solución | Ruta/corredor bloqueados | Validar antes de jugar y ofrecer recuperación |
| Tablero genérico | Geometría no cambia decisiones | Comparar fixtures con consecuencias distintas |
| API/host incompatible | Runtime o extensión falla | Auditar versiones oficiales e instalación, no exigir compra de Quest |
| Depth ausente en Mac | Función depende de capacidad no disponible | No hacerla esencial; geometría simplificada y limitación explícita |
| Coste de doble render | Frame time/memoria crecen | Límites, materiales compartidos y perfil del host, sin atribuirlo al Quest |
| Sin validación física | Solo datos simulados/host | Riesgo declarado; pruebas conservadoras y candidato transparente, no hardware como gate |
| APK distinto del demo | Dependencias de laboratorio en release | Revisar configuración Android y misma revisión fuente, sin autoplay ni fallback falso |
| Acceso de jueces incorrecto | Enlace privado/admin | Comprobar canal e invitación; no afirmar instalación no realizada |
| Alcance excesivo | Más features antes de integrar | Seguir cola SIM y recortar P1 |
| Datos o assets indebidos | Sin procedencia/autorización | No publicar hasta resolver derechos/privacidad |

## Plantilla

Fecha, ID, contexto, alternativas, decisión, consecuencias, fuentes, nivel de prueba y condición de revisión. Las reglas oficiales prevalecen sobre decisiones internas; no cambiar seguridad/privacidad sin dejar rastro.
