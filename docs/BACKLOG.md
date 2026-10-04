# Backlog ejecutable

Todos los tickets empiezan pendientes. Solo ESTADO.md registra avance real. P0 = necesario; P1 = condicionado al funcionamiento de P0. Los tests de referencia existentes no completan un ticket de Unity.

## RB-001 · P0 · Entorno real

Depende: acceso al equipo de desarrollo. Ejecutar ENTORNO.md, crear proyecto Unity real, fijar combinación de SDKs y crear escena mínima. Aceptación: proyecto abre sin errores y APK mínimo arranca en el dispositivo declarado; registrar versiones. Sin visor, estado bloqueado, no completo.

## RB-002 · P0 · Marcos y pruebas

Depende: RB-001 para implementación Unity. Trasladar la matemática de ARQUITECTURA.md a un servicio testeable. Aceptación: round-trip posición/rotación, orígenes distintos, escala positiva, rechazo de valores no finitos y recalibración. Incluir rotaciones 6DoF en tests Unity; Python solo cubre yaw.

## RB-003 · P0 · Estado único, dos vistas

Depende: RB-002. Una entidad con ID y pose canónica, representación grande y pequeña. Aceptación: mover cualquiera mediante comando cambia ambas; no hay simulación duplicada; maqueta no se incluye recursivamente.

## RB-004 · P0 · Captura con manos

Depende: RB-003. Adaptador de input, hover, lock, offset, desplazamiento, destino y cancelación. Aceptación: 40 intentos registrados; selección inequívoca; liberar en destino inválido restaura pose; dos manos no se apropian de la misma entidad.

## RB-005 · P0 · Feedback de escala

Depende: RB-004. Mano ampliada visual y resaltado de correspondencia. Aceptación: persona nueva identifica relación sin explicación larga; visual no tapa objetivo, UI ni manos reales; se puede reducir intensidad.

## RB-006 · P0 · Colocación, pausa y recuperación

Depende: RB-004. Ajustar maqueta en pausa; manejar tracking y foco. Aceptación: cero daño o avance de timers en pausa; ningún lanzamiento al perder tracking; no salto al recalibrar. Prueba sentado obligatoria.

## RB-007 · P0 · Adaptador de habitación real

Depende: RB-001. Permisos, carga y normalización a RoomSnapshot. Aceptación: distinguir fixture sintético de datos reales; denegación y fallo visibles; no persistir geometría privada.

## RB-008 · P0 · Generador de configuración válida

Depende: RB-007 y RB-002. Sector frontal, ruta, grieta, refugio y obstáculo. Aceptación: dos disposiciones generan decisiones diferentes; detectar imposibilidad y ofrecer reubicar. No navegación arbitraria entre muebles.

## RB-009 · P0 · Portal anclado

Depende: RB-008. Portal visual simple sobre superficie válida. Aceptación: estable mientras se mueve la cabeza; relocalización recuperable; no requiere tocar pared. Rotura decorativa compleja es P1.

## RB-010 · P0 · Mote y cierre

Depende: RB-004, RB-008, RB-009. Spawn, ruta, captura, devolución, energía e integridad. Aceptación: recompensa solo una vez, no queda grieta imposible de cerrar, estados inválidos rechazados.

## RB-011 · P0 · Reflector y proyectil

Depende: RB-008 y RB-006. Herramienta orientable, preview y reflexión. Aceptación: normal válida, colisión única, vida/rebotes limitados y una solución alcanzable. Operación secuencial con una mano.

## RB-012 · P0 · Shell

Depende: RB-011 y RB-010. Protección, señal previa, proyectil, vulnerabilidad y captura. Aceptación: cada estado se entiende por forma/animación además de color; no vuelve a armadura estando sujeto; no ataques durante tutorial pausado.

## RB-013 · P0 · Sesión completa y Pip mínimo

Depende: RB-010 y RB-012. Tutorial, progresión, victoria, derrota, reinicio y guía contextual. Aceptación: jugador externo completa sesión y reinicia con manos. Variante final reutiliza reglas; no nueva física.

## RB-014 · P0 · Preferencias y modo asistido

Depende: RB-013. Mano dominante, velocidad, colocación, texto y efectos. Aceptación: modo una mano completo, no simultaneidad obligatoria, señales redundantes. No declarar cobertura universal.

## RB-015 · P0 · Guardado local

Depende: RB-013. Preferencias y progreso versionados, reset y recuperación. Aceptación: simular archivo corrupto y fallo de escritura sin bloquear sesión; no guardar room mesh ni datos identificables.

## RB-016 · P0 · Pruebas observadas

Depende: RB-013 y RB-014. Aplicar PRUEBAS.md a nuevos usuarios. Aceptación: resultados reales, límites de muestra y problemas priorizados; consentimiento para grabación; ninguna métrica inventada.

## RB-017 · P0 · Robustez de habitaciones

Depende: RB-008 y RB-013. Probar distribución estrecha, vacía, obstruida, modificada y no vista. Aceptación: gameplay válido o recuperación clara en cada caso; no prometer que todas funcionan.

## RB-018 · P0 · Perfil en hardware

Depende: RB-013. Medir CPU/GPU/fps y sesión repetida; limitar entidades, shaders y allocs. Aceptación: evidencia del APK con OS/SDK/dispositivo, tiempos y fallos; comparar con objetivo de 72 fps y requisitos actuales. Perfil con grabación separado del normal.

## RB-019 · P0 · Arte, audio y licencias

Depende: RB-005 y RB-013. Aplicar ARTE_AUDIO.md y auditoría de procedencia. Aceptación: selección, acierto, fallo, peligro y final legibles; assets autorizados; sin material privado ni logos ajenos en captura.

## RB-020 · P0 · Release y candidatura

Depende: todos los P0 previos. ENTREGAR solo con autorización del propietario. Preparar APK, canal Competition, hash, instrucciones, vídeo y formulario real. Aceptación: instalación por otra cuenta autorizada y coherencia entre lo mostrado y el build. Mantener evidencia y acceso según reglas.

## P1 — No comenzar antes de los hitos principales

Anclaje persistente del refugio; cosméticos adicionales; tercer layout; tutorial alternativo más elaborado; variante final especial. Cada uno requiere aceptación medible y presupuesto de rendimiento. Si amenaza el envío, se elimina.

## Plantilla para nuevos tickets

ID, objetivo de usuario, prioridad, dependencias, archivos/módulos afectados, comportamiento normal, casos de error, criterio de aceptación, pruebas previstas, evidencia real y decisión de alcance. No abrir un ticket sin explicar qué mejora del juego justifica su coste.
