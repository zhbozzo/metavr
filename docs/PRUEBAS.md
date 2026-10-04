# Pruebas y evidencia

Un test verde demuestra solo lo que ese test ejecutó. La validación documental y Python no prueba Unity, Android, sensores, comodidad ni elegibilidad.

## Niveles de comprobación

1. Repositorio: archivos esperados, enlaces relativos y configuración coherente.
2. Referencia Python: transformación yaw/escala, reflexión y autoridad de captura simplificada.
3. Unity EditMode: transformaciones 6DoF, dominio, estados, rutas, guardado y eventos.
4. Unity PlayMode: jerarquías, selección, sincronización, pausa, escenas y UI.
5. APK en Quest: instalación, permisos, manos, registro espacial, rendimiento y recuperación.
6. Usuarios y salas no vistas: comprensión, comodidad, decisiones y repetición.
7. Entrega: otro usuario autorizado instala desde el canal y sigue las instrucciones.

No saltar del nivel 2 a afirmar el nivel 5. Los niveles 3–7 están pendientes hasta que exista evidencia real.

## Automatización de dominio prevista

- Round-trip posición y rotación con orígenes distintos, escala y orientación.
- Rechazar escala cero/negativa/no finita y coordenadas inválidas.
- Respetar offset inicial de captura; no saltar al seleccionar.
- Solo una mano puede poseer una entidad y una entidad por mano según configuración.
- Un evento duplicado no duplica energía ni daño.
- Miniatura no produce colisiones de gameplay.
- Pausa congela timers, spawns y daño; reanudar no incorpora tiempo ausente.
- Captura cancelada por tracking vuelve a lugar válido, sin velocidad residual.
- No restaurar armadura durante Held; no devolver una entidad Resolved.
- Reflector normalizado, número máximo de rebotes, trayectoria y tiempo de vida.
- Una plantilla imposible se rechaza antes de entrar a Playing.
- Guardado corrupto y versión desconocida recuperan un estado seguro.

## Protocolo de captura

Registrar 40 intentos deliberados repartidos entre posiciones y manos. Separar aciertos, fallos de detección y selecciones falsas. Objetivo inicial: al menos 95% de capturas deliberadas correctas; registrar además cancelaciones y errores de destino. Una cifra sin denominador no se acepta.

Probar sin agitar las manos ni optimizar artificialmente el ambiente para el vídeo. Registrar iluminación aproximada y limitaciones sin capturar datos biométricos. No publicar coordenadas de manos.

## Protocolo de comprensión

Cinco personas nuevas, cuando sea viable, con consentimiento para observación. Tras setup, no dar instrucciones verbales adicionales durante el primer intento. Medir tiempo hasta entender escalas y primera captura; registrar ayudas necesarias. Objetivo de diseño: 4 de 5 completan la interacción antes de 60 segundos con las pistas de la app.

Preguntas: ¿qué cambió cuando moviste la miniatura?, ¿qué decisión tomaste por la habitación?, ¿qué repetirías?, ¿qué fue incómodo? No inducir respuestas positivas. Cualquier incomodidad permite detener la prueba.

## Matriz de habitaciones

Sintéticas en Git: rectangular vacía, estrecha, con obstáculo central, superficies faltantes, sector obstruido y origen rotado. Incluir casos inválidos deliberados.

Reales fuera de Git: al menos dos espacios de desarrollo y dos no usados para ajustar el generador cuando se disponga de ellos. Probar datos incompletos, permiso denegado, cambio de muebles y pérdida de localización. Si no se alcanzó la muestra, declararlo; no inventar pruebas.

Resultado válido puede ser una configuración jugable o una recuperación clara. Rechazar una sala de forma útil es mejor que fingir detección y fallar durante partida. No todas las habitaciones tienen que ser compatibles con el MVP.

## Matriz hands-first y confort

Comprobar inicio, configuración, juego, ajustes, pausa, reinicio y salida sin mandos. Mano derecha, izquierda y modo asistido de una mano. Variar tamaño/altura de maqueta. Ocultar temporalmente la mano activa y recuperar sin daño. Menú de sistema, foco perdido y relocalización.

Comprobar que ninguna interacción invita a caminar, golpear muebles o alcanzar una pared. No mover cámara automáticamente. Probar lectura de UI y detección de amenazas con FoV reducido mediante herramienta oficial cuando esté disponible; no llamar a eso prueba en hardware distinto.

## Rendimiento

Objetivo propio: 72 fps sostenidos en dispositivo objetivo, con configuración registrada. La documentación de Meta consultada establece mínimo de rendering y frecuencias permitidas por separado; ver FUENTES.md [S06]. No confundir Hz de pantalla con fps de la app.

Medir CPU/GPU, tiempos de frame y periodos de caída; observar desde arranque hasta final, repetir sesiones para detectar acumulación, calor o memoria. Usar herramientas oficiales como OVR Metrics Tool si están disponibles. Medir gameplay sin grabación y con grabación por separado.

Registrar versión del APK, dispositivo, OS, SDK, frecuencia, duración y circunstancias. No deducir rendimiento del editor o del fps promedio. A 72 fps el presupuesto nominal es 13,89 ms por frame; margen y colas deben evaluarse realmente.

## Severidades

P0: riesgo de seguridad, caída, pérdida de progreso grave, no instalar, bloqueo de sesión o necesidad de mandos. Detiene entrega.
P1: selección inestable, desalineación, sala imposible sin recuperación, caídas persistentes de rendimiento. Corregir antes de presentación.
P2: problemas de feedback/arte que no bloquean. Priorizar por impacto observado.

## Registro de una sesión de prueba

```text
Fecha / ticket / commit:
Tipo: Python | Unity EditMode | Unity PlayMode | APK | usuario | entrega
Dispositivo y versiones (sin serial):
Configuración y fixture anónimo:
Pasos:
Esperado:
Observado:
Mediciones y denominadores:
Resultado: PASS | FAIL | BLOCKED | NOT RUN
Evidencia no sensible:
Limitaciones:
Acción siguiente:
```

Los vídeos domésticos y scans no se suben al repo. Guardar referencias privadas autorizadas sin enlaces que otorguen acceso público. ESTADO.md puede contener resultados agregados anónimos.
