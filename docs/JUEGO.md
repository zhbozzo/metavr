# Diseño del juego

Todo lo descrito es especificación pendiente de implementación. Valores de balance son hipótesis iniciales configurables.

## Fantasía y reglas

El jugador protege el refugio luminoso de Pip en un sector de su habitación. Una maqueta flotante representa ese mismo sector. Al manipular una criatura pequeña, una mano espectral grande ejecuta la intervención sobre su equivalente real. No hay armas ni daño físico; el jugador devuelve invasores por grietas virtuales.

Solo hay una criatura lógica por ID. El enemigo diminuto y el grande son representaciones del mismo estado. Ninguna entidad dentro de la maqueta genera una maqueta recursiva: se excluyen la maqueta, la cámara, el jugador y los auxiliares visuales del conjunto replicado.

## Tutorial de primer encuentro

Separar tiempo de permisos/configuración del tiempo de gameplay. No prometer inicio instantáneo en una habitación sin configurar.

1. Pip presenta la maqueta y señala dos superficies correspondientes. Resaltarlas brevemente en ambas escalas.
2. Una única criatura sale de una grieta frontal. No produce daño durante la enseñanza inicial.
3. Resaltar su miniatura y mostrar una demostración sencilla de selección. No una pared de texto.
4. Al capturar, fijar el objetivo y mostrar la mano ampliada. La criatura reacciona en ambas vistas.
5. Resaltar una zona receptora junto a la grieta en la maqueta. Soltar dentro devuelve la criatura; fuera vuelve al último lugar válido.
6. Celebración corta de Pip, primera grieta sellada y transición a juego normal.

Si no hay acción, ayuda progresiva visual y texto breve en inglés. Repetir pista contextual, no reiniciar la aplicación. Tutorial repetible desde ajustes.

## Input único

Selección por proximidad/rayo de mano según se valide el SDK y la ergonomía. Una pinza estable inicia captura; un movimiento corto desplaza; soltar confirma. Elegir un recorrido principal coherente, no varios gestos sin enseñar.

Al empezar captura guardar offset mano-objeto para evitar saltos. Mantener lock del objetivo mientras la selección continúe. Dos manos no pueden apropiarse de la misma entidad. Una mano puede operar toda la partida en modo asistido.

No se usa velocidad de lanzamiento para puntuar. Capturar no puede eliminar un enemigo sin una devolución válida. Los destinos tienen tolerancia visible y restricciones de capacidad configurables.

## Arquetipos

### Mote — invasor básico

Aparece, recorre una ruta simple validada y ataca el refugio al llegar. Es capturable salvo durante estados de transición. Mientras está sujeto, su movimiento y ataques se suspenden. Una devolución válida aporta energía de cierre a la grieta de origen. No se permite producir energía capturando una misma entidad repetidas veces.

### Shell — invasor acorazado

Protección visible por forma y animación, además de color. Al seleccionarlo protegido responde con feedback claro y no consume una acción oculta. Detiene su marcha, anuncia una trayectoria lenta y dispara una esfera virtual hacia un objetivo de juego, nunca hacia una zona que invite al jugador a esquivar físicamente.

La herramienta reflectante devuelve el proyectil. Un impacto reflejado correcto desactiva la protección durante una ventana generosa. Entonces puede capturarse como Mote. Si la ventana acaba, reactivar protección solo cuando no esté sujeto.

### Variante final

Reutiliza Shell con escala visual mayor y dos ciclos de protección configurables. Sus reglas no cambian por sorpresa. El gran cierre consiste en capturar su versión pequeña y devolver su equivalente grande a la grieta. No implementar anatomía/física nueva ni exigir ambas manos.

## Reflector

Existe uno activo en el MVP. Se toma desde un soporte claro en la maqueta y se coloca en un punto válido. La orientación modifica una normal; la trayectoria anticipada muestra el efecto antes de confirmar. No genera proyectiles por sí mismo.

Regla matemática: dirección reflejada = d - 2(d·n)n, con d y n normalizados. La colisión se resuelve una sola vez en el dominio. No duplicar reflejos en ambas representaciones.

Al reflejar, limitar el número de rebotes y consumir el proyectil al alcanzar su objetivo, salir del volumen válido o superar su tiempo de vida. Trayectorias que atraviesan geometría sólida no cuentan como solución.

## Grietas, refugio y victoria

Cada grieta tiene posición válida, energía requerida y lista de entidades originadas. Una devolución aumenta energía una sola vez. Al llegar al umbral y completar las entidades activas de ese encuentro, la grieta se cierra. No permitir oleadas infinitas ni estado imposible sin enemigos que aporten energía.

El refugio tiene integridad visible, no una UI numérica obligatoria. Los atacantes dañan en intervalos claros; durante onboarding, pausa o tracking perdido no hacen daño. Al llegar a cero: congelar juego, mostrar consecuencia no agresiva, ofrecer reintentar o salir con manos.

Victoria: todas las grietas previstas cerradas y final resuelto. Celebración breve, resultado y una mejora cosmética del refugio guardada localmente. Un fallo al guardar no impide terminar ni reiniciar.

## Partida objetivo de 6–8 minutos

- Primer minuto: correspondencia entre escalas y captura tutorial.
- Siguiente tramo: varios Mote con entradas espaciadas, sin saturar la maqueta.
- Introducción Shell: pausa de enseñanza y reflector. Sin castigo mientras se aprende.
- Combinación: elegir entre capturar una amenaza o orientar el reflector. Limitar concurrencia para manos y legibilidad.
- Final: variante Shell y cierre. Resultado y reinicio.

Punto de partida de balance: máximo 3 amenazas activas, proyectiles lentos y 3 hitos de cierre. Reducir a 2 encuentros si hace falta completar el producto. Los números definitivos requieren pruebas.

## Cómo importa la habitación

Un obstáculo real simplificado cambia una ruta o bloquea una línea. La disposición de superficies determina posiciones válidas de grieta, refugio y reflector. Elegir entre plantillas cortas verificadas; no generar navegación universal.

El generador debe comprobar antes de empezar: entrada alcanzable por ruta virtual, refugio válido, lugar alcanzable en la maqueta, solución reflectante existente y visibilidad frontal suficiente. Si falla, probar otra plantilla o pedir recolocación. No cambiar silenciosamente a un tablero genérico y llamarlo adaptación real.

## Rejugabilidad contenida

Variar secuencia y distribución válidas con una semilla local; pequeñas recompensas cosméticas; dificultad opcional después de victoria. Un modo fácil sigue siendo partida completa. No monetización, rachas obligatorias ni generación de contenido con modelos.

## Parámetros que se exponen a diseño

Escala y posición de maqueta; tamaño mínimo de collider de selección; tolerancia del destino; tiempo de ayuda; límite de enemigos; velocidades; integridad; energía por grieta; ventana de vulnerabilidad; pausa asistida; duración y transparencia de mano espectral.

Mantenerlos en configuración versionada. Cambiar un valor no exige reescribir lógica. Todo balance inicial se etiqueta como experimental.
