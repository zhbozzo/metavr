# ROOMBREAKERS — instrucciones para agentes

## Leer antes de trabajar

Contrato compartido de ejecución, subordinado a las instrucciones del propietario y a las reglas aplicables. Orden: este archivo → `prompts/CODEX_LOCAL_ENTREGA.md` para el encargo local integral → `docs/SIMULATOR_FIRST.md` → `docs/ESTADO.md` → `docs/CONTEXTO.md` → documento del área → `docs/BACKLOG.md`. README es el índice. No deducir funcionalidades del tamaño del repositorio.

## Encargo vigente: ejecutar en el computador y completar la entrega

El propietario delegó a Codex el trabajo completo, incluida la entrega, en este computador. [Encargo integral](prompts/CODEX_LOCAL_ENTREGA.md): auditoría local, Unity real, integración XR, pruebas, acabado, APK, vídeo, canal Competition y Devpost. No terminar con un plan si hay trabajo ejecutable; no reiniciar el diseño ni exigir revisión de código rutinaria.

Confirma el host y el workspace antes de actuar. Codex local no equivale a una tarea cloud; GitHub conectado no da acceso al Mac. Mantén los permisos normales del cliente y del sistema. El encargo incluye realizar las subidas y el envío del proyecto correcto una vez verificadas las condiciones; no incluye inventar acuerdos, datos administrativos o capacidades, ni publicar comercialmente en Store. Alcance exacto y pasos humanos en la sección 2 del encargo.

## Decisión del propietario: sin visor físico

**Desarrollo y demostración en computador, simulator-first, sin comprar, arrendar, pedir prestado ni exigir acceso a un Quest.** Es una restricción vigente, no una espera hasta que el usuario compre hardware. No poner conseguir/probar un visor en el camino crítico. [Política completa](docs/SIMULATOR_FIRST.md).

Esta política sustituye las exigencias internas antiguas de hardware en prompts, planes y documentación histórica; no elimina el requisito de una app Meta VR para el concurso. Hardware queda fuera del plan y no validado, nunca aprobado por inferencia.

Unity desktop, Unity con XR Simulator y APK Android son tres evidencias distintas. XR Simulator no es un emulador de Android. La escena de mouse existente no es todavía la integración Meta XR. La siguiente tarea es ejecutar y conectar software real, no reescribir el juego o añadir otro boss.

## Misión y diseño que se mantienen

Juego MR pequeño y completo: manipular la maqueta de la habitación produce consecuencias a escala grande. Pip, Motes, Shell y reflector forman el recorrido. Objetivo: calidad demostrable para competir, sin garantía de premio.

- Unity/C# y adaptadores Meta; no regresar a WebXR por el registro inicial.
- Una simulación canónica, dos representaciones, sin físicas ni daño duplicados.
- Hands-first de principio a fin, uso sentado, movimientos cortos; en desarrollo se usan manos simuladas.
- Offline: sin backend, login, pagos, multiplayer, IA generativa en runtime ni Terraform.
- Sin eye tracking obligatorio. Orientación de cabeza no es seguimiento ocular.
- Solo contenido virtual se mueve. No pedir golpes, lanzamientos fuertes ni contacto con muebles físicos.
- Geometría simplificada y consentida en la ruta del jugador; fixtures explícitos en desarrollo. No fallback sintético silencioso ni scans privados en Git.
- No depender de Environment Depth del simulador en Mac; capacidades y API gráfica se comprueban por plataforma/perfil.

## Forma de trabajar

1. Inspeccionar rama, estado y cambios existentes. No borrar trabajo ajeno ni force-push.
2. Elegir el primer ticket de integración desbloqueado. Consultar SIM-001 a SIM-006 y conservar los IDs RB previos.
3. Verificar documentación oficial y SDKs instalados. No inventar clases, permisos, versiones, GUIDs, escenas YAML o compatibilidad del simulator standalone.
4. Implementar un incremento pequeño y probarlo. Conservar lockfiles/meta reales; no cambiar motor por comodidad.
5. Separar Python/.NET, EditMode, PlayMode, XR Simulator, compilación APK, acceso de canal y hardware. No usar stubs de Unity para fingir una compilación del motor.
6. Actualizar ESTADO con comandos, commit, versiones y resultados observados. Una métrica sin medición queda pendiente.
7. Comunicar en español qué cambió y qué evidencia hay. Código/comentarios de APIs y materiales públicos de candidatura en inglés.

No pedir revisión de código rutinaria al usuario. Sí puede hacer falta que active su editor/licencia o dé acceso a una herramienta local: pedir solo la acción necesaria y continuar tareas independientes. No exigir USB, Link, data forwarding o compras de visor para desbloquear la tarea. Antes de terminar por límites de sesión, dejar un checkpoint; no prometer trabajo indefinido fuera de una ejecución activa.

## Aceptación y seguridad

Un hito de simulación puede completarse con evidencia del runtime real y sus fixtures, sin headset. No se llama por eso 'probado en Quest'. El build Android sigue siendo obligatorio para nuestra ruta nativa; conservar la entrada real de manos/entorno para los jueces, sin mouse obligatorio, autoplay o datos ficticios por defecto.

La prioridad es recuperación, control fiable, correspondencia de escalas, partida completa y acabado. Datos viejos, tracking perdido, foco y relocalización no deben producir daño, recompensas o lanzamientos. Recalibrar solo en un estado seguro. Un error de permisos, sala o almacenamiento debe ser visible y recuperable.

Rendimiento medido en Mac/simulador no certifica fps, sensores, calor o comodidad del Quest. La aspiración de rendimiento en dispositivo permanece como objetivo no medido. No bloquear el avance por la ausencia de hardware, ni esconder el riesgo de esa ausencia.

## Límites de autonomía

Se permiten cambios de código/documentos, pruebas y preparación/ejecución local dentro del proyecto, así como la entrega específica delegada en `prompts/CODEX_LOCAL_ENTREGA.md`. La autorización de publicación/envío solicitada por documentos anteriores queda cubierta para el APK del canal Competition, el vídeo de demostración y la candidatura correcta, siempre que se cumplan sus comprobaciones y declaraciones requeridas. No confundir borrador, subida o procesamiento con confirmación de envío.

No comprar assets, créditos o servicios, aceptar acuerdos por inferencia, cambiar visibilidad/licencia, habilitar infraestructura paga, ejecutar terraform apply, actualizar el sistema operativo o sustituir claves existentes sin aprobación específica. Mantener confirmaciones obligatorias del cliente, cuentas y plataforma. Instalar solo dependencias necesarias de fuentes oficiales y con permisos concedidos; no desactivar protecciones para hacerlo.

Meta XR Operator y automatización de navegador/editor son opciones solo cuando sus herramientas reales estén disponibles y autorizadas; no prometer control visual desde un conector de archivos ni extraer cookies para acceder a cuentas.

No solicitar contraseñas, 2FA o secretos en chat/repo. No subir correos, datos personales, invitaciones secretas, claves de firma, planos domésticos, grabaciones privadas o assets sin derechos. Las reglas oficiales prevalecen; verificar declaraciones administrativas antes del envío. Ante una cuenta o identidad ambigua, resolverla antes de una acción externa.

## Entrada de trabajo

**`prompts/CODEX_LOCAL_ENTREGA.md`** es el encargo completo para Codex en el computador. `prompts/INICIO.md` y `prompts/CONTINUAR.md` siguen como referencias parciales; no limitan la delegación integral ni restauran obligaciones antiguas de hardware. No reiniciar la preproducción: hay núcleo y herramientas implementados, pero consultar ESTADO para saber qué partes Unity/Meta/XR Simulator/APK se han ejecutado realmente.
