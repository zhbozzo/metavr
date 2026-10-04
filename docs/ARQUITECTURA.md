# Arquitectura técnica

Diseño pendiente de implementar en Unity. Los nombres de módulos e interfaces aquí son propios del proyecto, no APIs que se afirme que existen en Meta. Verificar los nombres reales del SDK antes de integrar.

## Principio central

**Una simulación, dos representaciones.** El estado canónico vive en coordenadas de habitación, en metros. La vista grande representa ese estado en el espacio del visor; la maqueta lo representa mediante una transformación uniforme. El input en maqueta se convierte a coordenadas canónicas antes de modificar el estado.

La mano gigante es feedback visual. No tiene autoridad de daño ni un rigidbody que compita con la simulación. Las miniaturas no se simulan físicamente como un segundo mundo.

## Capas y responsabilidades

- `Domain`: entidades, estados, oleadas, integridad, captura, reflejos, victoria y derrota. Sin dependencia del SDK de Meta.
- `Room`: normalización de datos del entorno, superficies, obstáculos, sector válido, rutas y colocación. Adaptador real y fixture sintético con el mismo contrato.
- `Input`: muestras de mano y comandos semánticos. Adaptador Meta para release; mouse solo para desarrollo.
- `Presentation`: vistas grande/miniatura, Pip, audio, VFX y UI. Consume eventos y poses; no decide daño.
- `Infrastructure`: progreso local, preferencias, reloj y logs privados mínimos. No significa nube.
- `Bootstrap`: composición de dependencias, estados de app y selección explícita de modo de desarrollo.

Flujo: HandAdapter → InteractionController → comando de dominio → Simulation → eventos → ambas vistas. Los callbacks del SDK no escriben directamente en todos los GameObjects.

## Contratos propios previstos

`IRoomProvider`: estado de permiso/carga/localización y `RoomSnapshot` validado.
`IHandInput`: identidad de mano, pose válida, inicio/fin de selección y validez de tracking. No inventar un campo de confianza numérica si el SDK solo expone flags.
`ISimulationClock`: delta de simulación que queda en cero durante pausa.
`IProgressStore`: carga, escritura versionada, reset y recuperación.
`ISessionDiagnostics`: mediciones agregadas sin geometría ni identificadores privados.

Comandos: `BeginCapture(entityId, handId)`, `MoveCapture(entityId, pose)`, `CommitCapture(entityId, targetId)`, `CancelCapture`, `PlaceReflector`, `Pause`, `Resume`, `Restart`. Validar cada comando en el dominio; un callback duplicado debe ser inocuo.

## Entidades y datos

`EntityState`: ID estable, tipo, pose canónica, estado, grieta de origen, propietario de captura opcional, timestamps de simulación y banderas de recompensa consumida.
`RoomSnapshot`: versión local de sesión, marco de referencia, superficies simplificadas, obstáculos y sector permitido. No serializar a Git ni enviarlo por red.
`EncounterConfig`: semilla, plantillas válidas, límites de spawn, energía, integridad y parámetros de balance.
`ProgressData`: versión de esquema, preferencias, encuentros completados y cosméticos. No necesita incluir room mesh ni posiciones domésticas.

Estados de criatura: `Spawning`, `Advancing`, `Telegraphing`, `Armored`, `Vulnerable`, `Held`, `Returning`, `Resolved`. Separar estado de combate y presentación cuando convenga; evitar combinaciones imposibles.

## Matemática de correspondencia

Sean R la rotación del marco grande, o su origen; Q la rotación de la maqueta, a su origen; s una escala uniforme positiva; p una posición en coordenadas canónicas.

```text
world(p) = o + R * p
mini(p)  = a + Q * (s * p)

canonicalFromWorld(w) = inverse(R) * (w - o)
canonicalFromMini(m)  = inverse(Q) * (m - a) / s
```

Por tanto una mano situada en la maqueta se mapea al espacio grande pasando por el mismo marco canónico. Las orientaciones no se multiplican por s. Los vectores de desplazamiento sí cambian de escala; los tamaños visuales usan s en la maqueta y 1 en el mundo.

Requisitos: escala finita y mayor que cero; rotaciones válidas; sin escala no uniforme ni padres con escala heredada inesperada; rechazar NaN/infinito; no mezclar metros con centímetros.

Al iniciar captura, calcular offset canónico entre objeto y mano. Conservarlo durante el movimiento. Si se cambia escala/orientación de maqueta, pausar y cancelar o terminar capturas antes de recalibrar.

El jitter de la mano se amplifica por 1/s. No escoger una maqueta minúscula por estética. Ajustar tamaño, filtrar señales y aplicar snap/histeresis con feedback. Medir latencia: un filtro demasiado fuerte rompe la sensación de conexión.

`../prototypes/reference_model.py` y sus tests prueban una versión matemática simplificada con giro yaw. No son la implementación 6DoF de Unity; allí hacen falta pruebas con quaternions y jerarquías reales.

## Render y física

Reutilizar assets con dos representaciones independientes enlazadas por ID. Escalas de render diferentes, estado compartido. Capas separadas: input de maqueta, mundo físico virtual, UI y visuales sin colisión.

Solo las colisiones canónicas producen eventos de gameplay. Los colliders de miniatura sirven para seleccionar, no para aplicar daño. La mano espectral no colisiona con el usuario ni cambia la posición de la cámara.

Evitar cámaras recursivas: no es necesario renderizar una cámara a textura para la maqueta. Instanciar geometría simplificada excluyendo maqueta, jugador y auxiliares. Controlar memoria y coste de duplicación visual.

Reflexión de proyectil: `r = d - 2 * dot(d,n) * n`, con dirección y normal normalizadas. Límite de rebotes y vida. Comprobar segmentos entre ticks para evitar atravesar objetivos al ir rápido; no resolver el mismo impacto dos veces.

## Pipeline del entorno

1. Obtener permiso y datos por el flujo oficial validado.
2. Convertir el espacio recibido a marco canónico estable.
3. Seleccionar un sector frontal y superficies útiles. No inferir objetos semánticos ausentes.
4. Crear una geometría reducida para maqueta, colisiones y pruebas de visibilidad.
5. Elegir una plantilla de encuentro compatible.
6. Validar rutas, refugio, destinos, trayectoria reflectante y legibilidad.
7. Confirmar colocación al usuario.

No construir navegación universal por muebles. La primera ruta puede ser una polilínea libre de obstáculos sobre un plano permitido. Rechazar intersecciones y configuraciones sin solución. Si una habitación no sirve, mostrar recuperación; un fixture sintético es modo debug claramente etiquetado, no un supuesto scan real.

La geometría escaneada no es un sistema de seguridad ni garantiza detectar personas, mascotas u objetos movidos. El gameplay nunca debe requerir desplazarse por rutas físicas sugeridas.

## Portales y pared

Primero un portal emisivo correctamente anclado. Efecto de apertura más elaborado después. No prometer extraer texturas del passthrough ni reconstruir la pared. Probar composición y oclusión del SDK antes de elegir shaders.

Los contenidos virtuales del portal deben respetar la malla/superficie prevista. El efecto de rotura no elimina obstáculos físicos. No animar objetos hacia la cara ni invitar a atravesar el portal caminando.

## Pausa, relocalización y persistencia

Pausar dominio y temporizadores ante tracking crítico o foco perdido. Conservar selección segura o cancelarla al último punto válido. No integrar todo el tiempo ausente al retomar.

Si cambia el marco de habitación, rebasar las vistas coherentemente o volver a Placement. Nunca corregir una escala dejando la otra antigua. Persistencia de anclajes entre sesiones es mejora opcional; necesita estrategia para habitaciones cambiadas o anclajes no localizados.

Guardar progreso pequeño con esquema, validación y escritura recuperable. No almacenar logs de manos ni imágenes. Manejar falta de espacio y JSON corrupto sin bloquear gameplay.

## Estructura Unity prevista

`unity/RoomBreakers/Assets/RoomBreakers/{Scripts,Scenes,Prefabs,Art,Audio,Settings,Tests}`.

Scripts separados en `Domain`, `Room`, `Input`, `Presentation`, `Persistence`, `Bootstrap`. Assemblies y tests según el entorno real. El editor genera GUID y archivos .meta; se conservan. No se fabrica un proyecto falso en la fase documental.

## Rendimiento

Pool de criaturas, proyectiles y efectos; materiales compartidos; límites de partículas y transparencias; sin consultas costosas de sala cada frame; eventos en lugar de búsquedas globales. Medir coste de dos representaciones y de la mano ampliada. No activar fidelidad visual hasta medir.

Objetivo propuesto de 72 fps: presupuesto por frame aproximado 13,89 ms, CPU y GPU medidos por separado. Revalidar frecuencia disponible y estándar real. Una medición desde el editor no sustituye un perfil de APK en visor.
