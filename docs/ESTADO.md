# Estado real de ROOMBREAKERS

Última actualización: 4 de octubre de 2026. Este archivo registra evidencia, no ambiciones. No hay juego Unity ejecutable, APK o prueba de visor todavía.

## Preparado en el repositorio

Contexto y decisiones; requisitos de producto; diseño de juego; UX, seguridad y accesibilidad objetivo; arquitectura de dos escalas; procedimiento Unity/Quest; plan por hitos; 20 tickets de implementación con aceptación; pruebas; fuentes oficiales; arte/audio; privacidad/licencias; instrucciones de agentes; prompts de arranque y continuación; preparación de entrega y borradores ingleses.

Se añadió un modelo Python de referencia, sin dependencias externas, para transformación yaw/escala, reflexión y propiedad de captura. No es un motor de juego ni una integración Meta. Se añadieron tests de referencia y comprobación de integridad documental.

## Evidencia de ejecución

| Comprobación | Estado |
| --- | --- |
| Escritura de la base en GitHub | Realizada; ver historial del repositorio |
| Integridad de archivos/enlaces | Pendiente de ejecución final |
| Tests Python del modelo de referencia | Pendiente de ejecución final |
| Proyecto Unity creado por editor real | NOT RUN |
| Compilación Unity/EditMode/PlayMode | NOT RUN |
| APK Android | NOT RUN |
| Ejecución en Quest | NOT RUN |
| Confort y comprensión con personas | NOT RUN |
| Habitaciones reales no usadas para desarrollo | NOT RUN |
| Rendimiento en dispositivo | NOT RUN |
| Canal Competition y ensayo de instalación | NOT RUN |
| Candidatura enviada | NO |

## Versiones y acceso

Unity: por validar. Meta Core/Interaction/MRUK/provider XR: por validar. Android tooling: por validar. Quest y versión de sistema: acceso por confirmar. No se han inventado manifests ni ProjectSettings para aparentar una instalación.

## Backlog

RB-001 a RB-020: pendientes. Las pruebas Python no completan RB-002, que requiere implementación y pruebas Unity 6DoF.

## Próximo paso

Abrir este repositorio en el agente de código y ejecutar `prompts/INICIO.md`. Trabajar RB-001: auditar la máquina, instalar/verificar una combinación compatible y ejecutar una escena mínima con passthrough, manos y un objeto seleccionable en un Quest real. Registrar bloqueos concretos si falta editor o visor.

No empezar boss, arte final, backend o más documentación estratégica antes de desbloquear esa prueba.

## Actualización al terminar una sesión

Añadir fecha, ticket, commit/archivos, comandos ejecutados, resultado observado y limitaciones. Usar PASS, FAIL, BLOCKED o NOT RUN. Adjuntar solo evidencia no sensible. No reemplazar pendientes por estimaciones ni considerar que compilar demuestra comodidad.
