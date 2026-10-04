# Fuentes y vigencia

Consulta base: 4 de octubre de 2026. Las reglas y metadatos del concurso se leyeron mediante la integración Devpost; la documentación técnica se consultó en páginas oficiales. Antes de instalar, integrar una API o entregar, verificar versiones y cambios.

Estos enlaces son fuentes, no dependencias descargadas ni permisos de redistribución. No copiar al repositorio contenido privado de Start.

## Concurso

**S01 · Reglas oficiales**
https://start-developer-competition-26.devpost.com/rules

Fuente de criterios/pesos, manos, divisiones, requisitos, acceso, plazos y condiciones. La página web puede mostrar bloqueo a un lector automatizado; en esta preparación se obtuvo su texto mediante `get_hackathon_rules` de Devpost. Consultar el original completo para declaraciones legales.

**S02 · Overview, requisitos y formulario**
https://start-developer-competition-26.devpost.com/

Consultados `get_hackathon_overview`, `get_submission_requirements` y `get_key_dates`. Envío nativo mediante APK/canal Competition; vídeo menor de 3 minutos; los campos vivos del formulario prevalecen para saber qué completar. No guardar datos personales del formulario en Git.

**S02b · Criterios ampliados**
https://start-developer-competition-26.devpost.com/details/judging

Usar junto con reglas, no como permiso para ignorarlas.

## Plataforma

**S03 · MR Utility Kit**
https://developers.meta.com/vr/documentation/unity/unity-mr-utility-kit-overview/

Utilidades de consultas y colocación sobre entorno, geometría y depuración. No equivale a un generador de niveles ni a detección perfecta del mundo.

**S04 · Límites del seguimiento de manos**
https://developers.meta.com/vr/design/hands-limitations-mitigations/

Iluminación, movimiento, oclusión y diseño de mitigaciones. Sustenta el uso de movimientos cortos, feedback y recuperación. Las mitigaciones concretas del juego requieren pruebas.

**S05 · Capacidades inmersivas**
https://developers.meta.com/horizon/essentials/horizon-os-immersive-features/

La documentación consultada distingue dispositivos y declara que Quest 3/3S no tienen hardware de seguimiento ocular. Diseñar por capacidades reales y no equiparar orientación de cabeza con datos de ojos.

**S06 · VRC.Quest.Performance.1**
https://developers.meta.com/vr/resources/vrc-quest-performance-1/

Distingue frecuencia de refresco permitida de rendering rate. La consulta menciona mínimo 60 fps y frecuencias interactivas que incluyen 72 Hz, con disponibilidad y excepciones específicas. Nuestro objetivo propio de 72 fps no se presenta como mínimo universal oficial. Revisar criterios y excepciones antes de release.

**S07 · Preparar visor para desarrollo**
https://developers.meta.com/vr/documentation/unity/unity-env-device-setup/

Cuenta, dispositivo, desarrollo y conexión. Las autorizaciones que corresponden al usuario no se automatizan mediante credenciales compartidas.

**S08 · Link para desarrollo**
https://developers.meta.com/vr/documentation/unity/unity-link/

Revisar soporte del host. En Mac, nuestra ruta base es compilar APK y probar en el visor, sin asumir que Link ofrece el mismo flujo que en Windows.

**S09 · Interaction SDK**
https://developers.meta.com/vr/documentation/unity/unity-isdk-interaction-sdk-overview/

Punto de entrada para integrar input. Registrar versiones reales y usar documentación que corresponda a ellas. Los nombres internos propuestos en ARQUITECTURA.md no son nombres de APIs Meta.

**S13 · Simular campo de visión**
https://developers.meta.com/vr/documentation/unity/in-headset-fov-simulation/

Procedimientos oficiales para comprobar FoV reducido. Una simulación de FoV no acredita haber probado otro visor real.

## Infraestructura

**S10 · Qué es Terraform — HashiCorp**
https://developer.hashicorp.com/terraform/intro

Herramienta de infraestructura como código. No es un motor de juego; el MVP no requiere provisionar nube.

## Precedentes para contraste creativo

**S11 · First Encounters**
https://developers.meta.com/vr/discover/success-stories/first-encounters/

Referencia relevante para invasores y entorno MR. Revisar producto y material oficial antes de redactar comparaciones públicas. No copiar assets, personajes o identidad visual.

**S12 · A Fisherman's Tale**
https://www.meta.com/experiences/a-fishermans-tale/2299967930057156/

Referencia para relaciones de escala/miniaturas. No afirmar que inventamos la interacción de escala; la hipótesis propia usa geometría real, manos y defensa. El nombre ROOMBREAKERS aún requiere revisión de disponibilidad/marca.

## Pendientes explícitos

- Versión exacta de Unity, provider XR y SDKs: por validar en instalación real. No resuelto con un número copiado de un ejemplo.
- Acceso físico a Quest 3/3S: por confirmar.
- Compatibilidad con otros dispositivos: no declarada.
- Inicio de evaluación: hay diferencia entre texto de reglas y campos estructurados. Ver CONCURSO.md; no cambia nuestro objetivo de enviar antes del cierre ni mantener acceso hasta anuncio.
- Estado vivo de la inscripción, tipo de remitente y declaraciones de elegibilidad: revisión del propietario antes de envío.
- Derechos de nombre, arte y software: revisión de procedencia antes de publicar.
- Usabilidad de doble escala: hipótesis principal, sin resultados de visor todavía.

## Cómo añadir una fuente

Registrar URL oficial, fecha, afirmación concreta que respalda, versión/plataforma aplicable y limitaciones. Distinguir hecho externo, decisión de diseño e hipótesis. No usar artículos de terceros como autoridad de una API cuando hay documentación primaria.
