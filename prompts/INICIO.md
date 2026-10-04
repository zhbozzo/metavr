# Prompt de arranque para Claude Code o Codex

Copia el siguiente bloque al agente trabajando dentro del repositorio. No pegar credenciales ni datos privados.

---

Trabaja en este repositorio para construir ROOMBREAKERS, nuestro juego de realidad mixta para la Meta VR Start Developer Competition 2026. No necesito otra lluvia de ideas: necesito ejecutar el diseño existente con incrementos verificables.

Lee primero AGENTS.md, README.md, docs/ESTADO.md y docs/CONTEXTO.md. Después lee PRODUCTO, JUEGO, UX_SEGURIDAD, ARQUITECTURA, ENTORNO, PLAN y BACKLOG en docs/. Usa FUENTES.md para validar afirmaciones externas.

El concepto vigente es manipular una maqueta simplificada de la habitación y ver una mano ampliada intervenir sobre criaturas a escala real. Es un juego individual sentado, hands-first, offline. Unity/C# es la ruta elegida. Una única simulación en coordenadas de habitación alimenta ambas representaciones. No hay Terraform, backend, login, multiplayer, IA generativa en ejecución ni eye tracking obligatorio.

Tu tarea de esta sesión es RB-001 y, solo si queda desbloqueado, la parte inicial de RB-002. Primero inspecciona el estado de Git, sistema operativo, herramientas y versiones disponibles. Respeta cambios existentes.

Ejecuta las comprobaciones Python del README. Confirma que son pruebas de referencia, no Unity. Revisa documentación oficial vigente y selecciona una combinación compatible de editor Unity, Android tooling, XR provider y SDKs Meta. No inventes números de versión, clases, permisos o manifests.

Crea un proyecto Unity real bajo unity/RoomBreakers con la ruta oficial de instalación. Conserva archivos generados, .meta, ProjectVersion y lockfiles. Si falta Unity, licencia, acceso al dispositivo o una autorización, explica exactamente qué paso debe hacer Lorenzo y continúa con lo que sí puedas verificar. No fabriques un proyecto que aparente abrir en Unity.

Primera escena objetivo: rig válido, passthrough, manos y un objeto seleccionable. Añade el mínimo de lógica para probar selección, desplazamiento y pausa. No construyas personajes finales, menús elaborados, gameplay de oleadas, boss ni efectos de pared.

Cuando el entorno funcione, prepara el servicio de transformación entre marco de habitación y maqueta con pruebas Unity. La referencia Python existente usa yaw y no sustituye quaternions/jerarquías 6DoF. Después muestra una sola entidad lógica en dos vistas, nunca dos simulaciones físicas.

No marques el entorno completo sin un APK que arranque en el visor declarado. No digas que se ha probado comodidad o tracking mediante un mouse. Un fallo del SDK debe reportarse con error y versión, no ocultarse mediante una implementación ficticia.

Antes de terminar, actualiza docs/ESTADO.md con el ticket trabajado, archivos, comandos ejecutados, resultados PASS/FAIL/BLOCKED/NOT RUN, versiones, limitaciones y siguiente tarea concreta. Registra decisiones nuevas en docs/DECISIONES.md. No cambies el alcance sin evidencia.

Entrega un resumen en español con: qué funciona realmente, qué falta, cómo probarlo ahora y la única acción que requiere Lorenzo. No publiques el build ni envíes la candidatura sin mi autorización.

---
