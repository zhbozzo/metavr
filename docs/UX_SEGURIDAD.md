# Interacción, comodidad y seguridad

Esta es una especificación, no una certificación de seguridad. Fuentes de plataforma: `FUENTES.md` [S04, S05, S07]. Las dimensiones y tiempos son parámetros a validar con usuarios reales.

## Recorrido y estados

`Boot → ExplainEnvironment → Permission → RoomLoad → Placement → Tutorial → Playing → Result`.

Desde cualquier estado pertinente: `Paused`, `TrackingRecovery`, `RoomRecovery`, `Settings`, `Exit`. Las transiciones tienen un único responsable; se conserva el estado previo para regresar sin duplicar spawns o recompensas.

No entrar a Playing hasta confirmar que la maqueta es cómoda, los datos de habitación sirven y las manos se detectan. El overlay de sistema y los permisos deben probarse realmente sin controles; no asumir que un menú propio resuelve el recorrido del sistema.

## Colocación

El usuario ajusta altura, distancia y tamaño desde pausa. Maqueta frente al torso, dentro de alcance cómodo y sin obligar a levantar los brazos permanentemente. No fijar una única estatura, postura o mano dominante.

Seleccionar un sector frontal real. Evitar amenazas esenciales detrás, giros repetidos y objetos por debajo de muebles. La maqueta puede ocultar paredes frontales visualmente para permitir ver dentro, pero conserva su geometría lógica.

No colocar botones sobre las manos ni depender de mirar directamente a un objetivo. Quest 3/3S no proporcionan eye tracking. Un indicador basado en orientación de cabeza nunca se etiqueta como mirada ocular.

## Legibilidad y atención

Mantener información suficiente en ambas escalas. Resaltar pares correspondientes al hover y selección. No exigir mirar arriba para saber si el destino pequeño es válido. El mundo grande ofrece confirmación y espectáculo, no información escondida.

Probar tamaño y ángulo de la maqueta; si alternar causa fatiga, subirla, reducir sector y usar pausa táctica opcional. No obligar a seguir simultáneamente dos focos alejados.

Un enemigo pequeño puede tener un volumen de selección mayor que su malla. Resolver solapamientos por proximidad, prioridad visual y objetivo fijado. Mostrar claramente qué se va a seleccionar antes de la pinza.

## Captura y liberación

`Idle → Hover → Selected → Dragging → ValidTarget/InvalidTarget → Released`.

Objetivo bloqueado durante captura; conservar offset inicial para no producir salto. Una operación por entidad y una por mano. No reemplazar el objetivo al cruzar otras miniaturas.

Destino válido: feedback de forma, animación y sonido; soltar confirma. Destino inválido: restaurar última pose válida con feedback. Soltar por pérdida de tracking no es una acción deliberada.

No interpretar gestos ambiguos como golpes o lanzamientos. No pedir que se toque una pared o se alcance la posición física del enemigo. Nunca recompensar movimientos fuera del espacio de manos confirmado.

## Fallos y recuperación

| Situación | Comportamiento requerido |
| --- | --- |
| Mano seleccionadora perdida | Suspender interacción y daño; conservar pose segura; indicar cómo recuperar; confirmar antes de seguir |
| Mano no activa perdida | Degradar según modo; no interrumpir innecesariamente si una mano basta |
| Localización de sala perdida | Pausa completa, ocultar/atenuar contenido mal registrado y volver a colocación cuando sea necesario |
| Usuario abre menú del sistema | Pausar timers, audio de combate y simulación; restaurar sin salto de tiempo |
| Permiso denegado | Explicación breve, reintentar por vía oficial o salir; no falsificar sala detectada |
| No hay superficie/ruta válida | Ofrecer otro sector o instrucciones de configuración; no comenzar partida imposible |
| Mobiliario se movió | Pedir revisar/recargar configuración; no suponer que la geometría guardada sigue correcta |
| Guardado corrupto | Cargar valores seguros, informar y permitir reiniciar progreso |

El timeout de recuperación puede cancelar una captura restaurando pose válida. Nunca aplicar velocidad estimada a una mano que reaparece. Reiniciar filtros tras interrupción larga.

## Accesibilidad objetivo

Modo una mano secuencial; no sostener un escudo con una mano mientras la otra sea obligatoria. Posibilidad de cambiar mano dominante. Velocidad y presión reducidas; pausa táctica accesible. Texto ampliable, contraste reforzado, información por forma además de color, subtítulos para contenido hablado, indicadores visuales de audio direccional.

Reducir flashes, vibraciones de cámara y tamaño/intensidad de la mano espectral. No mover la cámara del jugador. No usar sobresaltos, fallos catastróficos ni amenazas acercándose a la cara como instrucciones de juego.

No se prometen alternativas por voz, ojos o adaptación a cualquier condición. Registrar qué perfiles y modos se probaron y sus límites.

## Textos de interfaz propuestos en inglés

- "Make yourself comfortable. Place the miniature within easy reach."
- "This miniature follows your room. Only virtual objects can move."
- "Pinch to pick up. Move gently. Release inside the glowing gate."
- "Hands not detected. The game is paused."
- "Your room position changed. Let's place the miniature again."
- "We couldn't find a suitable play area. Try another direction or update your room setup."
- "Continue", "Retry", "Reposition", "Comfort", "Reset progress", "Exit".

## Revisión obligatoria en visor

Comprobar alcance sentado, ausencia de colisiones físicas incentivadas, estabilidad de objetivo, recuperaciones, iluminación variada, mano izquierda/derecha, una mano, lentes con FoV distinto o simulación oficial. Registrar molestias y detener una prueba cuando alguien reporte incomodidad. Un error potencialmente peligroso bloquea el hito aunque el resto funcione.
