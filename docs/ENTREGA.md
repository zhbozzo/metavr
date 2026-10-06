# Candidato Android, vídeo simulado y candidatura

Procedimiento pendiente. No hay APK ni candidatura enviada desde este repositorio. [SIMULATOR_FIRST](SIMULATOR_FIRST.md) gobierna el plan: sin compras ni acceso obligatorio a visor; hardware no validado. Esto no exime del build Meta VR ni de hands-first.

## 1. Congelar un candidato comprobable

Completar integración y pruebas del alcance de simulación, registrar fallos y limitaciones. No enviar con bloqueos conocidos o afirmaciones falsas. Fijar commit, editor, SDKs, runtime, API gráfica, perfiles y fixtures realmente probados.

Producir APK Android con configuración/firma autorizadas. Revisar escena inicial, dependencias, permisos, arquitectura y stripping. Mantener entrada real de manos y carga consentida del entorno para el jugador. No convertir mouse, autoplay, operador MCP o una sala sintética de desarrollo en dependencias de release.

El simulador no contiene Android; un vídeo del runtime no acredita arranque del APK. Identificar el archivo real:

```bash
shasum -a 256 /ruta/local/RoomBreakers.apk
```

Guardar artefacto y llaves en lugares privados apropiados, no en el Git del código. El hash no prueba que funcione. No se planifica adb install, USB o compra/préstamo de Quest.

## 2. Canal de evaluación

Los requisitos Devpost reconsultados piden APK en canal nuevo Competition e Invite URL para nuestra ruta Unity. Consultar originales enlazados en SIMULATOR_FIRST y CONCURSO antes de publicar. Subir/configurar solo con autorización.

Comprobar lo accesible de invitación, permisos de acceso y estado del build en dashboard. No enviar una URL administrativa que solo vea el autor. No commitear invitaciones secretas. No llamar a esta revisión instalación probada por otra cuenta/dispositivo: esa comprobación física queda no realizada, fuera del plan.

Los jueces deben poder obtener el build sin compilar fuentes, credenciales del autor o trucos escondidos. Mantener acceso según reglas vigentes; no cambiar el candidato tras el cierre.

## 3. Transparencia de plataforma

Completar [instrucciones de jueces](../submission/JUDGE_INSTRUCTIONS_EN.md) con versiones y comportamientos observados, no hipótesis. Separar perfiles simulados de dispositivos físicos probados. Disclosure previsto:

> Developed and demonstrated using Meta XR Simulator. Physical-headset validation has not been performed by the team.

Solo usar la primera frase cuando realmente se haya ejecutado la integración XR. Hasta entonces sigue siendo un texto de preparación. No certificar sensores, confort, rendimiento Quest o almacenamiento Android mediante resultados del host.

## 4. Vídeo real menor de tres minutos

La sección What to Submit admite XR Simulator o emulador equivalente. Plan de grabación: XR Simulator real, no imágenes generadas con IA ni una escena de ratón presentada como runtime XR sin justificar equivalencia.

Montaje propuesto de unos 140 segundos:

- Inicio: miniatura capturada y consecuencia ampliada.
- Relación con la sala sintética y una diferencia de geometría que cambie el juego.
- Motes, reflector, Shell y cierre con gameplay ejecutado.
- Pip, pausa/recuperación y reinicio con manos simuladas.
- Entorno/perfil/versiones y limitación de hardware explícitos.

El vídeo corresponde a la misma versión fuente del APK y declara diferencias relevantes entre ejecución host/Android. No se afirma que es una grabación del APK si es del editor. No usar aceleración engañosa, edición para esconder pasos manuales o manipulación del estado para fingir funcionalidades.

Revisar audio/texto/subtítulos ingleses, privacidad y derechos. Las salas son sintéticas: no hacen falta fotos o scans del domicilio. Publicar en YouTube/Vimeo según el requisito vigente y solo con autorización; comprobar reproducción pública.

## 5. Formulario y derechos

[DRAFT_EN](../submission/DRAFT_EN.md) sigue siendo borrador. Contrastar cada función con evidencia, separar presente/futuro y quitar placeholders antes del envío. Volver a consultar el formulario real.

Confirmar remitente, integrantes, país, correo Start, Gaming, división y Unity reales; no guardar correos o declaraciones personales en Git. La fecha de lanzamiento la decide el propietario. No inferir casillas legales ni condiciones administrativas de un correo de bienvenida.

La decisión de no usar hardware no autoriza omitir requisitos del concurso. El propietario revisa el riesgo residual, los derechos de material y el acceso antes de autorizar publicación/envío.

## 6. Preservación

Conservar SHA, versión/hash del APK, versión de vídeo, configuración del simulador, fixtures y confirmación efectiva de submission en un lugar adecuado. Mantener disponibilidad durante evaluación y respetar la congelación tras el cierre. Reconsultar fechas antes de enviar; no depender de una subida de último minuto.

## Gate interno de entrega

Se exige build producido, pruebas del alcance simulado, materiales honestos, acceso configurado y autorización. Hardware sigue NO VALIDADO y no es una compra pendiente. No confundir la decisión del equipo de aceptar ese riesgo con garantía de elegibilidad, aceptación del build o buen funcionamiento físico.
