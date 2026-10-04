# Build de evaluación, vídeo y candidatura

Estado: procedimiento pendiente. No hay APK ni candidatura enviada desde este repositorio. Consultar CONCURSO.md y FUENTES.md antes de ejecutar; las reglas y el formulario pueden cambiar.

## 1. Congelar un candidato real

Completar los P0 del backlog y registrar limitaciones. Identificar commit, versión de app, versiones Unity/SDK, dispositivo y OS probados. Generar un APK con la configuración Android y firma correspondientes al proyecto. No cambiar una firma de una app existente sin coordinación.

El APK, sus claves y logs sensibles no van al Git del código. Conservar una copia privada del artefacto y su hash SHA-256. Un hash identifica el archivo; no acredita que funcione.

En macOS, sobre un archivo realmente generado:

```bash
shasum -a 256 /ruta/local/RoomBreakers.apk
```

No etiquetar el candidato como release si contiene un modo mouse o sala sintética activado por defecto. Los modos de desarrollo deben quedar separados de la experiencia entregada.

## 2. Ensayar acceso e instalación

Seguir el procedimiento vigente de Meta Developer Dashboard para crear el canal de evaluación llamado Competition y subir el APK. Configurar la invitación para que los jueces accedan según las instrucciones actuales del concurso.

El enlace que se presenta es el acceso de prueba adecuado, no una URL administrativa del dashboard que solo abra para el autor. No commitear enlaces privados de invitación a este repositorio público.

Con una cuenta autorizada distinta, comprobar invitación, instalación, arranque, permisos, colocación, partida, pausa, resultado y reinicio. Registrar qué dispositivo y build se probaron. Si no se realizó este ensayo, permanece pendiente.

La app no necesita publicarse comercialmente en Store para que esta guía esté completa. Publicar o enviar requiere autorización del propietario y cumplimiento del proceso aplicable.

## 3. Instrucciones para jueces

Completar `../submission/JUDGE_INSTRUCTIONS_EN.md` únicamente con comportamientos comprobados. Incluir plataforma, dispositivos probados, setup de habitación, cómo iniciar con manos, objetivo, interacción, pausa y limitaciones conocidas. No confundir dispositivos objetivo con dispositivos probados.

No exigir que el juez tenga credenciales del autor, configure secretos, compile código o descubra un menú oculto para jugar. Mantener acceso gratuito suficiente hasta el anuncio efectivo según reglas.

## 4. Vídeo real menor de tres minutos

Propuesta de montaje de aproximadamente 140 segundos; no una duración oficial distinta al límite del concurso:

- 0–15 s: captura de miniatura y consecuencia de mano grande. Mostrar el valor antes de explicar tecnología.
- 15–35 s: cómo se reconoce la habitación y cómo la geometría altera una posición o trayectoria.
- 35–75 s: captura, reflector y enemigo acorazado en gameplay real.
- 75–100 s: tutorial, juego sentado, pausa y recuperación, sin controles.
- 100–125 s: cierre del encuentro y reacción de Pip.
- 125–140 s: resumen breve y limitaciones/plataforma verificadas cuando corresponda.

El vídeo debe representar el build entregado. No usar escenas generadas con IA, aceleraciones engañosas o montajes que hagan parecer funcional lo que no existe. Distinguir grabación en hardware de simulador si se utiliza este último. Si se mezcla una vista externa con captura del visor, mantener correspondencia real de la acción.

Capturar una habitación sin personas ajenas identificables, fotos familiares, pantallas privadas o marcas que incumplan las reglas. Revisar audio, subtítulos y texto en inglés. No incluir logos que sugieran patrocinio oficial.

Subir a YouTube o Vimeo con visibilidad pública conforme al requisito consultado y comprobar reproducción sin sesión del autor. No publicar automáticamente desde un agente sin autorización.

## 5. Texto y formulario

`../submission/DRAFT_EN.md` contiene material de trabajo, no una descripción de funciones terminadas. Antes de usarlo, contrastar cada afirmación con evidencia y eliminar placeholders.

Volver a leer el formulario vivo. Confirmar tipo de remitente, integrantes, país, correo Start, nombre, Gaming, división correcta, Unity si corresponde, capacidades realmente usadas y descripción de interacción con manos. No guardar correos ni declaraciones personales en Git.

La fecha de lanzamiento es una decisión del propietario, no una fecha que el agente invente. Preparar inspiración, construcción, mejoras futuras y explicación de lo que se puede jugar ahora. Separar siempre futuro de presente.

## 6. Revisión final del propietario

Lorenzo revisa reglas, derechos de material, declaraciones de elegibilidad, contenido de candidatura y acceso de jueces. Ninguna casilla legal se marca por inferencia. No enviar por el mero hecho de que el repositorio tenga un archivo de instrucciones.

Objetivo interno de envío: 16 de noviembre de 2026. Cierre oficial consultado: 18 de noviembre, 20:00 UTC. Verificar vigencia antes del envío y dejar margen para carga y procesamiento.

## 7. Preservación tras cierre

Conservar commit, versión, hash, copia de materiales y confirmación efectiva de submission en lugar adecuado. La evaluación corresponde al estado de cierre; no sustituir silenciosamente el build o alterar materiales después de la fecha límite. Mantener disponibilidad hasta el anuncio real y atender comunicaciones por los canales oficiales.

No hacer público un secreto o dato personal para demostrar que se envió. ESTADO.md solo debe registrar el hecho, fecha, versión y evidencia no sensible cuando la acción ocurra.

## Gate de entrega

No enviar si hay un P0 abierto, el enlace no fue probado, falta vídeo real, quedan afirmaciones no verificadas, el recorrido requiere mandos o no se resolvieron derechos de material. Un repositorio completo no sustituye ninguna de estas pruebas.
