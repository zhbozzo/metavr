# Contexto del proyecto

Fecha base: 4 de octubre de 2026. Propietario del repositorio: `zhbozzo`. Producto provisional: **ROOMBREAKERS**. Repositorio: `metavr`.

## Qué pidió Lorenzo

Dejar en este repositorio el contexto, diseño completo de la app, instrucciones para agentes, arquitectura, configuración, plan de ejecución y preparación de la candidatura. El objetivo es construir una experiencia suficientemente original, cómoda, estable y atractiva como para competir seriamente en la Meta VR Start Developer Competition 2026. No basta una idea vistosa ni una carpeta de documentación.

La tesis expresada por Lorenzo es que Meta pueda mostrar el resultado como una demostración convincente de su plataforma. La traducimos en una prueba: alguien debe entender el valor de la experiencia viendo una acción real, sin escuchar una lista de APIs. No asumimos acceso especial a jueces, acuerdos promocionales ni probabilidad de premio.

## Antecedentes relevantes, informados en la conversación

- Se creó un equipo de desarrollo llamado ZH Development y una app de dashboard con nombre provisional ZH Spatial AI.
- Lorenzo recibió un correo de bienvenida al programa Start y avanzó por el registro del concurso. Esta documentación no reemplaza verificar su estado actual en ambas plataformas.
- En el registro se eligió inicialmente IWSDK/WebXR. El diseño evolucionó después hacia Unity por la integración de manos y entorno. El formulario final debe reflejar el motor realmente utilizado.
- Se quiere trabajar con asistencia de agentes de código. El equipo inicial de trabajo se plantea como individual; no inventar integrantes ni atribuciones.
- No está confirmado aquí que ya exista acceso a un Quest ni una instalación funcional de Unity.

No almacenar en este repositorio público el correo de la cuenta, datos personales, capturas privadas de inscripción, App ID extraído de imágenes o enlaces de acceso a canales.

## Evolución y decisión vigente

Primero se discutió productividad espacial; luego boxeo MR; después invasores entrando por paredes. La versión vigente combina una maqueta del espacio real con intervenciones a tamaño real.

**Frase del producto:** Tu habitación está siendo invadida. La tienes delante en miniatura y tus manos pueden cambiar lo que ocurre a tu alrededor.

El jugador protege el refugio de Pip, captura criaturas en una réplica simplificada de su habitación y ve una mano espectral ampliada ejecutar la intervención en el espacio real. Una herramienta reflectante permite romper la armadura de ciertos enemigos. Todo ocurre sentado, con movimientos cortos y una simulación única.

## Por qué no basta la versión anterior

Invasores atravesando paredes no constituye por sí solo una diferenciación: First Encounters de Meta es un precedente relevante. Manipular una miniatura tampoco es una invención sin precedentes: A Fisherman's Tale explora relaciones entre escalas. Consultar enlaces en `FUENTES.md`.

La hipótesis diferencial es la combinación de **geometría de la habitación real + maqueta manipulable + consecuencias a escala real + defensa estratégica hands-first**, con un recorrido de pocos minutos. Debe comprobarse que el espacio altera decisiones y que alternar entre escalas es cómodo.

## Correcciones que no deben perderse

1. No convertir el requisito hands-first en una obligación de usar cada tecnología disponible. Eye tracking no es requisito de nuestro juego, y Quest 3/3S no tienen ese hardware.
2. MRUK entrega utilidades sobre datos del entorno; no genera automáticamente navegación, reglas de nivel ni comprensión de cualquier mueble.
3. Una grieta virtual sobre una pared no significa reconstruir la textura real de la pared ni mover objetos físicos.
4. No prometer 80–180 horas ni asegurar que se llega a nivel ganador: las estimaciones previas eran orientativas, sin validar entorno, habilidad XR o disponibilidad. Planificar por evidencia e hitos.
5. No confundir simulador, editor y hardware. Un test Python verde no demuestra un juego VR; un APK compilado no demuestra comodidad.
6. El nombre, diseño y alcance son provisionales. No hay investigación de marca ni garantía de elegibilidad por el simple registro.

## Qué significa «todo» en esta entrega

Una base íntegra para construir y verificar: producto, experiencia, arquitectura, backlog, seguridad, pruebas, fuentes y materiales de entrega. **No significa que el juego haya sido desarrollado o probado en un Quest.** `ESTADO.md` separa lo creado de lo pendiente.

## Qué significa «terra form» aquí

Se interpreta como preparar la base técnica y de trabajo completa. Terraform de HashiCorp gestiona infraestructura; no crea escenas Unity ni aporta algo necesario al MVP offline. `../infra/README.md` documenta cuándo tendría sentido reabrir esa decisión. No se provisionan servicios ni se generan costes.
