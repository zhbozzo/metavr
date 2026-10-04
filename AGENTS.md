# ROOMBREAKERS — instrucciones para agentes

Estas instrucciones gobiernan el trabajo en este repositorio. Son contexto de proyecto, no permiso para saltarse controles de seguridad, términos de plataformas o instrucciones del propietario.

## Misión

Construir un juego MR pequeño y completo para la Meta VR Start Developer Competition 2026. El jugador manipula una maqueta de su habitación y ve consecuencias sincronizadas a escala real. La ambición es competir por calidad demostrable, no afirmar que el proyecto ganará.

**Estado inicial: especificación y herramientas de referencia. No existe todavía una app Unity ejecutable ni un APK verificado.** Consulta siempre `docs/ESTADO.md`; no arrastres afirmaciones antiguas de implementación.

## Orden de lectura

1. `README.md`, `docs/ESTADO.md`, `docs/CONTEXTO.md`.
2. `docs/PRODUCTO.md`, `docs/JUEGO.md`, `docs/UX_SEGURIDAD.md`.
3. `docs/ARQUITECTURA.md`, `docs/ENTORNO.md`.
4. `docs/PLAN.md`, `docs/BACKLOG.md`, `docs/PRUEBAS.md`.
5. Antes de preparar entrega: `docs/CONCURSO.md`, `docs/ENTREGA.md`, `docs/FUENTES.md`.

No reescribir todos los documentos en cada sesión. Leer lo necesario para la tarea y mantener una sola fuente de verdad por asunto.

## Decisiones que no debes reabrir sin evidencia

- Unity/C# con adaptadores para las herramientas XR de Meta; la selección WebXR del registro inicial no define la arquitectura actual.
- Una simulación autoritativa en coordenadas canónicas de habitación; dos representaciones, no dos físicas independientes.
- Manos de extremo a extremo, uso sentado, movimientos cortos y seguridad por encima del espectáculo.
- MVP offline: sin backend, cuentas, pagos, IA generativa en runtime ni multiplayer.
- Sin seguimiento ocular obligatorio. Orientación de cabeza no equivale a eye tracking.
- Geometría del entorno simplificada y consentida; no captura fotográfica, no escaneo permanente ni reconocimiento semántico arbitrario.
- Interacción base: seleccionar/agarrar, desplazar, orientar, soltar. No vocabulario de poses complejas.
- No tirar con fuerza ni golpear superficies reales. Soltar en un destino validado sustituye al lanzamiento físico.
- Solo se transforma contenido virtual. No fingir que se mueve mobiliario físico.

## Forma de trabajar

1. Inspecciona rama, cambios existentes y último estado antes de modificar. Nunca borres trabajo ajeno ni hagas force-push.
2. Elige el primer ticket desbloqueado del backlog. Declara alcance, dependencias y criterio de aceptación.
3. Verifica documentación oficial y compatibilidad de versiones antes de escribir APIs de Meta. No inventes clases, permisos, GUID, paquetes o versiones. Registra la combinación que realmente instalaste.
4. Implementa un incremento pequeño. Prefiere componentes claros a frameworks genéricos. No cambies motor por comodidad del agente.
5. Ejecuta las pruebas disponibles. Diferencia Python, pruebas Unity, build Android, prueba de visor y prueba con usuarios.
6. Actualiza `docs/ESTADO.md`: archivos cambiados, pruebas y resultados reales, bloqueos, siguiente paso. Anota una decisión importante en `docs/DECISIONES.md`.
7. Resume qué está terminado, qué sigue siendo hipótesis y qué acción concreta requiere Lorenzo. Un build no se llama terminado hasta que otro usuario puede instalarlo y completar su recorrido previsto.

No marques un ticket completado porque hay código o porque compila. Debe cumplir su criterio de aceptación. Las métricas sin medición permanecen en blanco, nunca se rellenan con estimaciones.

## Límites de autonomía

Puedes modificar código y documentación del alcance, escribir pruebas y preparar instrucciones. No compres assets, aceptes acuerdos, cambies visibilidad/licencia del repositorio, habilites servicios pagos, publiques en Store/Devpost ni envíes una candidatura sin autorización explícita. No ejecutes `terraform apply`; no hay infraestructura aprobada.

La cuenta de Meta, licencias de Unity, autorizaciones USB, instalación física y pruebas de comodidad pueden requerir al usuario. Pide una acción concreta, no le delegues trabajo que sí puedes realizar. Nunca solicites contraseñas, códigos 2FA, recovery codes o secretos en el chat/repo.

## Ingeniería y evidencia

- Separar dominio, input, adaptador de habitación y presentación. Los IDs de entidades son estables; no identificar lógica por nombres de GameObjects.
- Mantener autoridad única de poses, colisiones y daño. La representación pequeña no origina daño duplicado.
- Recalibración de maqueta solo fuera de interacción activa; la partida pausa ante pérdida de localización o tracking crítico.
- Fallos de permisos, tracking, almacenamiento y habitaciones no compatibles tienen recorridos visibles y recuperables.
- Target de ingeniería propuesto: 72 fps sostenidos en Quest 3/3S; verificar estándar vigente. No confundir con la mención de 60 fps en la rúbrica del concurso.
- No declarar soporte de un dispositivo sin prueba. Simulador y mouse son herramientas internas, no evidencia hands-first.
- Código/comentarios de APIs y materiales públicos de submission en inglés; documentación de trabajo y comunicación con Lorenzo en español.
- No fijar dependencias a `latest` una vez validado el entorno. Commitear los lockfiles generados por herramientas reales.

## Fuente y privacidad

Las reglas oficiales prevalecen sobre nuestros resúmenes. `docs/FUENTES.md` registra fuentes y puntos pendientes. No inventes premios, porcentajes de éxito, datos de pruebas ni citas de jueces. No presentes el nombre provisional como marca registrada.

Repositorio público: nunca subir correos de inscripción, datos familiares/académicos/financieros, credenciales, enlaces secretos de canales, capturas identificables, room scans reales, rutas domésticas o llaves de firma. Usar habitaciones sintéticas en tests. No subir assets comerciales completos ni contenido privado de Start.

## Primera tarea

Lee `prompts/INICIO.md` y ejecuta RB-001. El objetivo inicial es una prueba de doble escala con una entidad; no un menú final, jefe, trailer ni sistema de IA. Si no tienes Unity o visor, ejecuta lo verificable y registra el bloqueo con precisión.
