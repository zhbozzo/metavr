# Plan de ejecución — simulator-first

Actualización: 5 de octubre de 2026, hora de Chile. [Decisión del propietario](SIMULATOR_FIRST.md): no compra, arriendo, préstamo ni acceso obligatorio a visor. El antiguo calendario que dependía de pruebas físicas queda sustituido por estos hitos de software. Fechas oficiales se reconsultan antes de enviar; no prometer horas ni fecha de acabado sin evidencia.

## Prioridad

Recuperación y estabilidad → interacción de manos → doble escala → espacio que modifica decisiones → partida completa → acabado → entrega. No ampliar mecánicas mientras el código existente siga sin ejecución en el motor.

## M0 — Editor reproducible · SIM-001 / RB-001–003

Preparar/importar con el script existente, fijar versiones compatibles y ejecutar 12 EditMode y cuatro PlayMode. Abrir el encuentro desktop y resolver errores reales. Conservar código y escenas; no rehacer el proyecto.

Aceptación: editor/compilación y recorridos observados, con logs propios de esa ejecución. Sin editor: BLOCKED de software, nunca dependencia de compra de hardware.

## M1 — Ruta XR sin hardware · SIM-002

Conectar Meta XR Simulator standalone, perfil Quest 3, SDK/rig e input de manos simuladas. Probar una interacción mínima y volver al encuentro. Validar la procedencia de los datos, no solo que aparezcan manos dibujadas.

Aceptación: sesión OpenXR real y recorrido por el adaptador Meta, sin controller físico ni mouse convertido en dependencia del APK. Registrar capacidades por plataforma. No depender de Environment Depth en Mac.

## M2 — Habitación y juego compartidos · SIM-003 / RB-004–013

Cargar habitación del runtime y fixtures Prefab/JSON explícitos cuando corresponda. Auditar el adaptador existente; completar la integración faltante sin duplicar la simulación. Recorrer Motes → reflector → Shell → resultado → reinicio.

Aceptación: las dos escalas coinciden y diferentes geometrías cambian rutas/solución o producen rechazo útil. Los datos simulados nunca se presentan como scans reales.

## M3 — Robustez, lectura y repetición · SIM-004 / RB-014–019

Matriz sintética de salas, orígenes, FoV, una/dos manos, pérdida de datos, foco, menús, reinicio y guardado. Casos reproducibles con semillas/fixtures; repetir en perfil 3S compatible. Observar usuarios de escritorio cuando sea viable, sin atribuir confort VR.

Aceptación: cero fallos conocidos bloqueantes del recorrido probado, límites documentados y problemas priorizados. Medir CPU/GPU/allocs del host solo como host. No convertir los fps del simulador en rendimiento de Quest.

## M4 — Artefacto Android · SIM-005 / RB-020

Generar APK real desde la misma versión fuente. Revisar configuración Android, escenas, permisos, firma, dependencias y separación de modos. El APK conserva manos y entorno del jugador; no depende de un PC, fixtures locales o un operador MCP.

Aceptación: compilación y archivo identificable con hash y versiones. Instalación, rendimiento y sensores de hardware: NO VALIDADOS, fuera del plan del equipo. No ocultar ese riesgo ni exigir comprar para continuar.

## M5 — Demostración y acceso · SIM-006 / RB-020

Grabar gameplay real desde XR Simulator, menor de tres minutos, indicando entorno y diferencias frente al APK. Preparar textos, disclosure y acceso Competition. Comprobar lo verificable del enlace sin llamarlo instalación probada.

Aceptación interna: evidencia de runtime, build producido, materiales coherentes y acceso configurado conforme a reglas vigentes. El propietario autoriza publicación/envío y revisa limitaciones. No enviar con fallos conocidos bloqueantes o declaraciones falsas.

## Recorte

Recortar más encuentros, cosméticos adicionales, persistencia espacial y efecto de mano complejo antes de interacción, recuperación o final. No compensar la falta de hardware con veinte sistemas nuevos. No migrar a WebXR para evitar integrar Unity sin autorización.

## Seguimiento

Cada sesión responde qué se ejecutó realmente, en qué nivel, qué fallo se corrigió y cuál es el siguiente paso de software. Historial anterior en Git; [ESTADO](ESTADO.md) registra evidencia actual. Los objetivos originales de calidad se conservan, las pruebas físicas no forman parte del camino crítico.
