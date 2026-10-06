# ROOMBREAKERS — instrucciones para agentes

## Leer antes de trabajar

Contrato compartido de ejecución, subordinado a las instrucciones del propietario y a las reglas aplicables. Orden: `docs/SIMULATOR_FIRST.md` → `docs/ESTADO.md` → `docs/CONTEXTO.md` → documento del área → `docs/BACKLOG.md`. README es el índice. No deducir funcionalidades del tamaño del repositorio.

## Decisión del propietario: sin visor físico

**Desarrollo y demostración en computador, simulator-first, sin comprar, arrendar, pedir prestado ni exigir acceso a un Quest.** Es una restricción vigente, no una espera hasta que el usuario compre hardware. No poner conseguir/probar un visor en el camino crítico. [Política completa](docs/SIMULATOR_FIRST.md).

Esta política sustituye las exigencias internas antiguas de hardware en prompts, planes y documentación histórica; no elimina el requisito de una app Meta VR para el concurso. Hardware queda fuera del plan y no validado, nunca aprobado por inferencia.

Unity desktop, Unity con XR Simulator y APK Android son tres evidencias distintas. XR Simulator no es un emulador de Android. La escena de mouse existente no es todavía la integración XR. La siguiente tarea es ejecutar y conectar software real, no reescribir el juego o añadir otro boss.

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

No pedir revisión de código rutinaria al usuario. Sí puede hacer falta que active su editor/licencia o dé acceso a una herramienta local: pedir solo la acción necesaria. No exigir USB, Link, data forwarding o compras de visor para desbloquear la tarea.

## Aceptación y seguridad

Un hito de simulación puede completarse con evidencia del runtime real y sus fixtures, sin headset. No se llama por eso 'probado en Quest'. El build Android sigue siendo obligatorio para nuestra ruta nativa; conservar la entrada real de manos/entorno para los jueces, sin mouse obligatorio, autoplay o datos ficticios por defecto.

La prioridad es recuperación, control fiable, correspondencia de escalas, partida completa y acabado. Datos viejos, tracking perdido, foco y relocalización no deben producir daño, recompensas o lanzamientos. Recalibrar solo en un estado seguro. Un error de permisos, sala o almacenamiento debe ser visible y recuperable.

Rendimiento medido en Mac/simulador no certifica fps, sensores, calor o comodidad del Quest. La aspiración de rendimiento en dispositivo permanece como objetivo no medido. No bloquear el avance por la ausencia de hardware, ni esconder el riesgo de esa ausencia.

## Límites de autonomía

Se permiten cambios de código/documentos dentro del alcance y pruebas disponibles. No comprar assets, aceptar acuerdos, cambiar visibilidad/licencia, habilitar infraestructura paga, ejecutar terraform apply, publicar builds/vídeos ni enviar Devpost sin autorización explícita.

GitHub conectado no implica acceso al editor local. Meta XR Operator es una opción de desarrollo solo cuando sus herramientas reales estén disponibles y verificadas; no prometer automatización física o visual desde un conector de archivos.

No solicitar contraseñas, 2FA o secretos en chat/repo. No subir correos, datos personales, invitaciones secretas, claves de firma, planos domésticos, grabaciones privadas o assets sin derechos. Las reglas oficiales prevalecen; verificar declaraciones administrativas antes del envío.

## Entrada de trabajo

`prompts/INICIO.md` para preparar integración; `prompts/CONTINUAR.md` para retomar. No reiniciar la preproducción: hay núcleo y herramientas implementados, pero Unity/Meta/XR Simulator/APK siguen sin ejecución acreditada en el estado actual.
