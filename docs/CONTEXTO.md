# Contexto del proyecto

Base: 4 de octubre de 2026. Decisión actualizada: 5 de octubre de 2026, hora de Chile. Propietario: `zhbozzo`; repositorio `metavr`; producto provisional **ROOMBREAKERS**.

## Qué quiere el propietario

Dejar aquí el producto, contexto, instrucciones para agentes, arquitectura, plan, pruebas y preparación de candidatura. Construir un juego original y pulido que permita mostrar el valor espacial de Meta, no solo una lista de APIs o un repositorio grande. No hay garantía de premio ni acceso especial a jueces.

**Restricción confirmada: no quiere comprar lentes ni depender de un dispositivo físico; quiere desarrollar y demostrar todo en entorno de computador.** Se adopta simulator-first como plan completo, no como fase antes de comprar o pedir un Quest. La [política vigente](SIMULATOR_FIRST.md) sustituye las obligaciones internas de hardware anteriores.

## Antecedentes relevantes

Se informó la creación de ZH Development y una app de dashboard llamada ZH Spatial AI, un correo de bienvenida a Start y avance de inscripción. Son antecedentes de conversación, no una auditoría actual de elegibilidad. El formulario final debe reflejar los hechos y el motor utilizado.

Inicialmente se eligió IWSDK/WebXR al registrarse. Después se eligió Unity/C# para la integración de manos y geometría. Se conserva Unity; simulator-first no vuelve a cambiar de motor. El trabajo se plantea individual con asistencia de agentes; no inventar integrantes.

El host previsto es macOS Apple Silicon; todavía se debe inspeccionar su instalación real. No se ha acreditado un Unity/XR Simulator ejecutando este proyecto ni acceso remoto al Mac desde el chat. No se planea acceso a Quest del equipo. No guardar correos, App ID privado, invitaciones ni datos personales en Git.

## Evolución de la idea

Productividad espacial → boxeo MR → invasores por paredes → maqueta de la habitación con intervención a escala real. La decisión vigente mantiene esta última idea.

**Frase:** Tu habitación está siendo invadida. La tienes delante en miniatura y tus manos pueden cambiar lo que ocurre a tu alrededor.

El jugador protege a Pip, devuelve Motes y orienta un reflector para abrir la armadura de Shell antes de devolverlo. Una sola simulación alimenta ambas representaciones. La mano ampliada es feedback visual, no una segunda física. La habitación modifica rutas/colocación; no se mueve mobiliario físico.

Se conservan precedentes: First Encounters para invasores en MR y A Fisherman's Tale para relaciones de escala. No presentar cualquiera de esas ideas aisladas como invención inédita. La hipótesis diferencial es su combinación con habitación, control hands-first y defensa breve; su calidad se debe observar, no deducir.

## Lo que ya hay y lo que falta

PRs #1–#8 incorporaron núcleo, entrada de manos y controles, geometría, encuentro Mote, Shell/reflector, guía y progreso, bootstrap Unity y pruebas del motor escritas. El núcleo/herramientas tienen evidencia de CI; importar/compilar Unity y SDKs, ejecutar XR Simulator y generar el APK siguen sin evidencia. Ver [ESTADO](ESTADO.md).

La ruta preparada con ratón y sala sintética no es automáticamente Meta XR Simulator. MRUK puede usar fuentes de entorno distintas, pero hay que conectarlas y comprobarlas en nuestro código. No declarar esos adaptadores terminados por actualizar documentos.

## Plan de trabajo adoptado

Editor real y tests → runtime OpenXR/Meta XR Simulator → manos y salas simuladas → matriz de regresión → compilación Android → vídeo de simulación y acceso de evaluación autorizado. Hardware queda fuera del plan, no aprobado.

El requisito de vídeo permite XR Simulator/emulador; la ruta Unity mantiene un APK y acceso para jueces. La demostración debe identificar que es simulada. No eliminar hands-first ni la ruta real de habitación del APK; los jueces no tienen que compilar el repositorio.

## Correcciones que no deben perderse

- No usar todas las tecnologías por obligación; eye tracking no es requisito del juego.
- MRUK no resuelve automáticamente navegación, física o cualquier mueble.
- No prometer horas, fecha de acabado o probabilidad de ganar sin evidencia.
- Simulator-first no significa listo, instalado ni validado en hardware. XR Simulator tampoco es Android.
- Las salas de prueba, las manos simuladas y las métricas del host se declaran con ese origen.
- El nombre no tiene investigación de marca completada. Una bienvenida a Start no acredita todas las condiciones del concurso.

## «Todo» y «terra form»

Todo significa una base de producto y ejecución más el código efectivamente construido; no un juego final por mera documentación. Terraform administra infraestructura, no escenas Unity. El MVP es offline; no hay nube, pagos o terraform apply aprobados. Ver `../infra/README.md`.
