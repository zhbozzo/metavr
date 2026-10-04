# Dirección de arte y audio

Dirección propuesta, todavía sin assets finales. Prioridad: legibilidad y respuesta de interacción antes que cantidad de contenido.

## Identidad

Aventura táctica luminosa y ligeramente traviesa. La habitación real permanece reconocible. Maqueta arquitectónica simplificada, grietas de luz, criaturas de silueta clara y herramientas como pequeños mecanismos. No terror, realismo militar, marcas comerciales ni parecido deliberado a personajes existentes.

El nombre ROOMBREAKERS y Pip son provisionales. No generar un logo definitivo antes de validar interacción y revisar disponibilidad del nombre.

## Lenguaje de dos escalas

Usar mismo contorno y estado reconocible para ambas versiones de una entidad. La miniatura puede exagerar rasgos legibles, sin representar otra posición o regla. La geometría pequeña es deliberadamente simple: no prometer texturas reales del espacio.

Hover: contorno en ambas escalas. Selección: pequeño acento visual y reacción del objeto. Destino válido: forma receptora y animación. Error: rechazo breve y retorno seguro; nunca solo rojo/verde.

La mano espectral aparece al intervenir, no permanece bloqueando la escena. Ajustar opacidad, duración y detalle. Su escala debe explicar la acción, no competir visualmente con el enemigo. Ofrecer modo de efecto reducido.

## Pip

Personaje pequeño no humano: cuerpo mecánico simple, ojos expresivos y luz. Animaciones mínimas: reposo/curiosidad, señalar, alarma, alivio y celebración. Puede enseñar sin diálogo largo. No lip-sync, conversación generativa ni animación cinematográfica costosa.

## Lista mínima de assets

- Pip con cinco respuestas reutilizables.
- Mote y Shell; variante final reutiliza Shell.
- Refugio y una mejora cosmética opcional.
- Un reflector con soporte.
- Un portal/grieta con estados abrir, recibir y cerrar.
- Proyectil simple y efectos de selección/impacto.
- Mano espectral compatible con el rig validado y sus términos de licencia.
- Iconos claros de pausa, reubicar, ajustes, repetir y salir.

Cada asset debe tener origen/licencia registrados. Usar primitivas hasta que el gameplay funcione. Evitar librerías comerciales completas en este repositorio público.

## Audio funcional

Crear señales diferenciadas para hover, selección, destino válido, devolución, protección, reflejo, daño al refugio, pausa y cierre. Sonidos cercanos para la miniatura; consecuencia espacial desde la ubicación grande. No reproducir dos veces el mismo impacto por tener dos vistas.

Priorizar eventos; limitar voces simultáneas. La alarma importante no queda oculta por música. Si un mensaje se transmite por sonido, ofrecer equivalente visual. Texto inglés/subtítulos para voz. Música original o licenciada y volumen independiente.

## VFX y rendimiento

Una grieta emisiva anclada es suficiente para el primer portal. Rotura de pared y partículas avanzadas son mejoras, no dependencia. No muestrear cámaras ni fabricar texturas del hogar para ahorrar trabajo artístico.

Limitar transparencias, partículas, luces dinámicas y materiales únicos. Medir doble representación en APK. No sacrificar fluidez para una captura bonita. Efectos sin flashes agresivos ni movimiento de cámara.

## Revisión de acabado

El jugador distingue qué puede tocar, qué sostiene, dónde puede soltar, qué amenaza es prioritaria y cuándo termina la partida. Revisar a tamaño real en visor, no solo una imagen en monitor. Un personaje debe resultar legible desde la posición de maqueta acordada antes de aumentar detalle.

Ver PRIVACIDAD_LICENCIAS.md para publicación y material de terceros.
