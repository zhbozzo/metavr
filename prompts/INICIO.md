# Prompt de arranque — integración simulator-first

Trabaja dentro del repositorio metavr para continuar ROOMBREAKERS. Lee AGENTS.md, docs/SIMULATOR_FIRST.md, docs/ESTADO.md y docs/CONTEXTO.md; después ARRANQUE_UNITY, ENTORNO, PLAN y BACKLOG. Consulta PRODUCTO/JUEGO/ARQUITECTURA para la tarea, sin reinventarlos.

La decisión explícita del propietario es no comprar, arrendar, pedir prestado ni depender de un visor físico. Todo el desarrollo y la demostración se hace en computador. No incluyas conseguir un Quest, autorizar USB o probar en hardware como paso obligatorio. Conserva el riesgo de hardware no validado en los informes.

Ya existen núcleo C#, captura/controles, geometría, Motes, Shell/reflector, Pip, progreso local y herramientas de preparación. No vuelvas a construirlos desde cero. Unity/C# sigue siendo la ruta; no WebXR, backend, Terraform, login, multiplayer o IA generativa.

Tu tarea inicial es SIM-001, integración real de editor, y luego SIM-002 si queda desbloqueada. Inspecciona Git, OS/arquitectura, RAM/espacio, editor/licencia y herramientas efectivamente disponibles. No sobrescribas cambios ni inventes un proyecto Unity.

Ejecuta las comprobaciones existentes que puedas. Con un Unity 6 activado usa tools/setup_unity_project.py y las instrucciones reales de docs/ARRANQUE_UNITY.md. Conserva ProjectVersion, manifest, lockfiles y metadata generados; no escribas YAML de escenas o GUIDs de SDK supuestos. Ejecuta EditMode y PlayMode con tools/run_unity_checks.py; registra los resultados del motor, no solo los mocks Python.

Luego verifica la combinación compatible de Meta XR Simulator standalone, Unity/OpenXR, Core/Interaction/MRUK y API gráfica del host. macOS Apple Silicon es la ruta preferida. No asumir que mínimos documentados prueban compatibilidad. No instalar una versión latest indiscriminadamente ni usar instrucciones archivadas del simulador.

Escena XR mínima: runtime activo, perfil Quest 3, manos simuladas controladas desde computador y una interacción seleccionable. Acredita que el input entra por el adaptador XR, no por DesktopEncounterHand. Después recorre el encuentro existente con datos sintéticos de habitación del runtime o fixtures Prefab/JSON explícitos, implementando adaptadores que falten. No hacer indispensable Environment Depth en Mac ni requerir data forwarding desde hardware.

No confundas las tres capas: desktop con ratón, XR Simulator y APK Android. El simulador es un runtime de API, no una instalación Android. Más adelante hay que generar un APK nativo para los jueces conservando la ruta real de manos y carga consentida de habitación. No desactivar salvaguardas ni reemplazar sensores por fixtures silenciosos en release.

Si falta editor, licencia o acceso local, informa la acción concreta de software que requiere el propietario y sigue con lo verificable. No describas GitHub como acceso al Mac. No declares éxito de Unity/Quest por tests .NET, ni prueba de comodidad por una captura de pantalla.

Antes de terminar, actualiza docs/ESTADO.md: tarea, archivos, comandos, versiones, PASS/FAIL/BLOCKED/NOT RUN, evidencias y siguiente paso. Publicar el vídeo/APK o enviar candidatura requiere autorización. Responde en español sin delegar revisión rutinaria de código.
