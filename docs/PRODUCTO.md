# Producto y alcance

Estado: diseño propuesto, no funcionalidad implementada. Las cifras son objetivos por validar, no resultados de pruebas.

## Visión

ROOMBREAKERS es un juego MR individual de acción táctica. Se juega sentado, con manos, en sesiones objetivo de 6–8 minutos. Manipulas una maqueta simplificada de tu habitación y ves las consecuencias a escala real. Proteges el refugio de Pip y devuelves criaturas por grietas virtuales.

La promesa es acción grande con movimientos pequeños. La geometría aporta decisiones; la maqueta permite actuar a distancia; las manos hacen tangible la intervención; Pip explica el objetivo; audio y efectos unen las escalas.

## Bucle

Observar amenaza → seleccionar miniatura o herramienta → intervenir → reconocer consecuencia grande → proteger refugio → cerrar grieta → siguiente encuentro → final y reinicio.

## Requisitos funcionales

- FR-01. Todo el recorrido propio de la app funciona con manos: inicio, colocación, juego, ajustes, pausa, final y reinicio.
- FR-02. Explicar y solicitar acceso al entorno; denegación y datos insuficientes tienen recuperación o salida claras.
- FR-03. Escoger una zona frontal válida, sin exigir caminar ni aceptar encuentros imposibles.
- FR-04. Maqueta reconocible, legible y ajustable en pausa. No es una copia fotográfica.
- FR-05. Una autoridad para estado y poses. Ambas escalas comparten IDs; no duplican daño o entidades.
- FR-06. Captura estable, destino previsualizado y liberación controlada; no se exige lanzar con fuerza.
- FR-07. Mano espectral ampliada o feedback equivalente enlaza escalas sin tapar lo importante.
- FR-08. Criatura básica con ruta válida y devolución a grieta.
- FR-09. Criatura acorazada que solo queda capturable tras reflejar su ataque.
- FR-10. Reflector orientable con trayectoria anticipada y posición válida visible.
- FR-11. Encuentro con objetivo, presión, victoria, derrota y reinicio.
- FR-12. Pip guía mediante estados y pocas animaciones, sin conversación generativa.
- FR-13. Pausa ante fallos críticos; nunca daño ni cuenta atrás durante recuperación.
- FR-14. Ajustes de mano dominante, altura, distancia, velocidad, texto, audio y confort.
- FR-15. Guardado local de progreso y preferencias, recuperación de corrupción y opción de borrado.
- FR-16. Build de entrega accesible y materiales que describen exactamente esa versión.

## Requisitos de calidad

Fiabilidad: sin saltos entre escalas. Pérdida de manos no origina un lanzamiento. Pérdida de localización pausa la partida. Evitar bloqueos entre pausa, tutorial y captura.

Rendimiento: objetivo inicial de ingeniería de 72 fps estables en Quest 3/3S. Verificar estándar vigente y medir CPU/GPU, frames perdidos y sesión prolongada. No basta el promedio. Ver PRUEBAS.md.

Privacidad: sin backend, envío de geometría, cámaras o manos; sin micrófono para el MVP. La geometría real no se guarda por defecto.

Accesibilidad: modo asistido secuencial con una mano, velocidad reducida, texto legible, contraste y señales redundantes. No afirmar accesibilidad universal.

Mantenibilidad: dominio separado del SDK, dependencias fijadas después de validarlas, pruebas de estados y transformación, logs sin datos personales.

## Diferenciación demostrable

Una disposición u obstáculo reales deben cambiar una ruta, trayectoria o lugar de herramienta. Encender passthrough alrededor de un tablero idéntico no demuestra valor del entorno.

El primer prototipo prueba sensación de escala. El siguiente prueba que la geometría cambia una decisión. Nunca ocultar información en una escala para forzar giros de cuello en la otra.

## Alcance de entrega

Obligatorio: un sector frontal adaptable, captura, un reflector, dos arquetipos, Pip mínimo, sesión completa, recuperación con manos y pruebas reales.

Condicional: tercera grieta, variante final elaborada, progreso visual entre sesiones y anclaje persistente. Solo después de validar fiabilidad y diversión.

Fuera: multiplayer, mundo abierto, navegación arbitraria entre muebles, IA generativa, pagos, login, backend, editor de niveles, fotogrametría, mover muebles físicos, seguimiento ocular obligatorio y lanzamientos violentos.

## Objetivos de validación

Después de configurar el entorno, 4 de 5 personas nuevas deberían comprender ambas escalas y completar una captura antes de 60 segundos con guía mínima. Objetivo de captura: 95% de intentos deliberados correctos sobre un protocolo de 40; registrar falsos positivos aparte.

Ninguna interacción debe exigir levantarse o alcanzar superficies reales. Probar dos habitaciones no usadas para desarrollar: generar configuración válida o explicar cómo reubicar, sin fallos silenciosos.

Preguntar qué decisión cambió por la habitación y qué harían en otra partida, no solo si les gustó. Estas muestras detectan fallos grandes; no prueban calidad universal.

Si dos iteraciones no resuelven la fatiga entre escalas, ajustar tamaño, posición, pausas o sector; si persiste, decidir un cambio con Lorenzo antes de producir contenido. Sin acceso a un Quest, no declarar validado el concepto.
