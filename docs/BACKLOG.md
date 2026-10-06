# Backlog ejecutable — sin visor físico

Actualizado por [SIMULATOR_FIRST](SIMULATOR_FIRST.md). Se conservan IDs RB, pero las aceptaciones que exigían hardware se sustituyen por niveles explícitos de simulación. No se marcan tickets completos por cambiar este documento. [ESTADO](ESTADO.md) manda sobre avance real. P0 necesario; P1 solo después de integración.

## Cola inmediata de integración

**SIM-001 · P0:** ejecutar bootstrap, importar/compilar editor y los tests reales existentes. Depende de software/acceso al host, no de un visor. Aceptación: 12 EditMode, cuatro PlayMode y escena observada; registrar fallos en vez de reescribir tests para ocultarlos.

**SIM-002 · P0:** conectar XR Simulator standalone con rig/provider/SDK y manos simuladas. Depende de SIM-001. Aceptación: captura/menú mediante datos de runtime en perfil Quest 3, sin controller físico ni DesktopEncounterHand como sustituto.

**SIM-003 · P0:** habitación del runtime y fixtures explícitos, misma simulación. Depende de SIM-002. Aceptación: encuentro completo y recuperación, procedencia de datos visible. Adaptadores faltantes se implementan y prueban, no se dan por hechos.

**SIM-004 · P0:** matriz de salas e input, perfil 3S/FoV compatible, replay y regresiones. Depende de SIM-003. Aceptación: fixtures esperados, resultados por caso, fallos corregidos y medición del host sin atribuirla al Quest.

**SIM-005 · P0:** generar y revisar APK Android sin instalación propia. Depende de SIM-003 y revisión de configuración. Aceptación: archivo, hash, versiones, entrada real de manos/entorno y ausencia de dependencias de laboratorio en release. Hardware no validado.

**SIM-006 · P0:** vídeo XR Simulator, instrucciones transparentes y acceso Competition autorizado. Depende de SIM-004/005. Aceptación: materiales del mismo candidato, diferencias declaradas y cumplimiento de requisitos vigentes; envío solo autorizado.

## RB-001 · P0 · Entorno real de software

Crear/importar con ENTORNO y ARRANQUE_UNITY; fijar combinación compatible. Aceptación de editor en SIM-001 y de XR en SIM-002. Separar APK en SIM-005. Se elimina la antigua condición de poseer/probar visor.

## RB-002 · P0 · Marcos y pruebas

Depende de editor para contraste Unity. Mantener núcleo 6DoF implementado y tests .NET; comprobar correspondencia con Transform, escalas, rotaciones, valores inválidos y recalibración. Python no sustituye los tests del motor.

## RB-003 · P0 · Estado único, dos vistas

Depende de RB-002. Una entidad canónica, dos representaciones; movimiento y rotación coinciden sin física duplicada ni maqueta recursiva. Aceptación observada en PlayMode/XR Simulator, no inferida de código.

## RB-004 · P0 · Captura con manos

Depende de RB-003/SIM-002. Conservar hover, lock, offset, destino, arbitraje y watchdog. Probar intentos deliberados mediante manos simuladas izquierda/derecha y errores de input. No llamar a su tasa de éxito precisión de tracking físico.

## RB-005 · P0 · Feedback de escala

Depende de RB-004. Mano ampliada y correspondencia visibles sin tapar objetivo/UI. Revisar ambos ojos/FoV del perfil. Observación externa de escritorio cuando sea viable; sin afirmar confort en visor.

## RB-006 · P0 · Colocación, pausa y recuperación

Depende de RB-004. Recolocar solo en estado seguro, congelar daño/timers, cancelar tracking/foco sin lanzamiento. Aceptación con poses sentadas simuladas y secuencias de interrupción; no requiere ensayo físico.

## RB-007 · P0 · Fuentes de habitación

Depende de SIM-002. Runtime simulado/fixtures explícitos para desarrollo; conservar adaptador de datos consentidos para Android. Distinguir origen, permiso/fallo y revisión de sala. No guardar geometría privada ni activar fallback falso.

## RB-008 · P0 · Configuración válida

Depende de RB-007/002. Sector frontal, ruta, grieta, refugio y obstáculos. Dos fixtures cambian decisiones; imposibilidad produce recuperación antes de jugar. No navegación universal por muebles.

## RB-009 · P0 · Portal anclado

Depende de RB-008. Ubicación coherente bajo poses de cabeza y cambios de localización simulados. No pide tocar paredes. Estabilidad física no validada; rotura decorativa compleja P1.

## RB-010 · P0 · Mote y cierre

Depende de RB-004/008/009. Mantener spawn, ruta, captura, devolución, energía e integridad; validar una sola resolución y estados imposibles. Reutilizar el código existente.

## RB-011 · P0 · Reflector y proyectil

Depende de RB-008/006. Orientación sobre soporte fijo, preview, rebote único y geometría. Una mano simulada completa la acción. No ampliar a colocación libre antes de probar integración.

## RB-012 · P0 · Shell

Depende de RB-011/010. Protección, aviso, pulso, vulnerabilidad y captura. Estados distinguibles además de color; no rearme durante Held ni daño durante aprendizaje pausado.

## RB-013 · P0 · Sesión y Pip

Depende de RB-010/012. Recorrido completo y reinicio con manos simuladas, más guía de tiempo activo. Captura del runtime real y pruebas de estados; no convertir autoplay en demostración.

## RB-014 · P0 · Accesibilidad

Depende de RB-013. Operación secuencial con una mano, mano dominante, colocación, texto y efectos. Revisar en perfiles/escenarios, sin prometer cobertura física universal. Ajustes faltantes permanecen pendientes.

## RB-015 · P0 · Progreso local

Depende de RB-013. Conservar checkpoints y separación práctica/release. Probar archivos corruptos, formatos futuros y errores sin bloquear juego. Almacenamiento del host no acredita Android; no guardar manos/room mesh.

## RB-016 · P0 · Pruebas observadas en computador

Depende de RB-013/014. Revisión del runtime y, cuando sea posible, usuarios nuevos de escritorio. Registrar muestra, ayuda y errores, no testimonios inventados ni mediciones de comodidad VR.

## RB-017 · P0 · Robustez de habitaciones simuladas

Depende de RB-008/013. Matriz SIM-004 con fixtures reservados y casos inválidos. Resultado: configuración jugable o rechazo claro; no todos los espacios deben aceptarse.

## RB-018 · P0 · Rendimiento del host y presupuesto Android

Depende de RB-013. Medir el editor/runtime: memoria, tiempos, allocs y acumulación en sesiones repetidas. Presupuesto conservador de objetos/shaders. No presentar esos datos como fps Quest. Perfil físico queda fuera del plan y NO VALIDADO, no bloquea SIM-005.

## RB-019 · P0 · Arte, audio y procedencia

Depende de RB-005/013. Feedback y final legibles en ejecución, recursos autorizados y capturas sin datos privados. Audio reproducido en host no certifica espacialización/confort de headset.

## RB-020 · P0 · Release candidato y candidatura

Depende de integración, pruebas de alcance simulado y SIM-005/006. APK, canal, hash, vídeo y formulario, con autorización. No exigir ensayo físico por otra cuenta como paso obligatorio; registrar acceso web revisado por separado de instalación no verificada. No enviar con bugs bloqueantes conocidos.

## P1

Más encuentros, anclaje persistente, cosméticos extra, tutorial alternativo y variante final espectacular. Tercer layout no es P1 si hace falta para la matriz de prueba: fixtures de robustez son P0. Ningún P1 puede reintroducir compra/acceso obligatorio a visor.

## Plantilla

ID, objetivo, dependencia de software, archivos, ruta normal/error, nivel de aceptación, pruebas previstas, evidencia y límites. Nunca sustituir 'sin visor' por 'probado en visor'.
