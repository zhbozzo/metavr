# Estado real de ROOMBREAKERS

Última actualización: 4 de octubre de 2026. Este archivo registra evidencia, no ambiciones. No hay juego Unity ejecutable, APK o prueba de visor todavía.

## Preparado en el repositorio

Contexto y decisiones; requisitos de producto; diseño de juego; UX, seguridad y accesibilidad objetivo; arquitectura de dos escalas; procedimiento Unity/Quest; plan por hitos; 20 tickets de implementación con aceptación; pruebas; fuentes oficiales; arte/audio; privacidad/licencias; instrucciones de agentes; prompts de arranque y continuación; preparación de entrega y borradores ingleses.

Se añadió un modelo Python de referencia, sin dependencias externas, para transformación yaw/escala, reflexión y propiedad de captura. No es un motor de juego ni una integración Meta. Se añadieron tests de referencia, comprobación de integridad documental y un workflow de GitHub Actions. No se provisionó infraestructura cloud ni se eligió una licencia de redistribución para el código propio.

## Evidencia de ejecución

| Comprobación | Estado |
| --- | --- |
| Escritura de la base en GitHub | Realizada en main; árbol remoto consultado |
| Integridad de archivos/enlaces simples | PASS en GitHub Actions, ejecución 37241476190 |
| Tests Python del modelo de referencia | PASS: 20 tests locales; también pasó el paso correspondiente en Actions |
| Proyecto Unity creado por editor real | NOT RUN |
| Compilación Unity/EditMode/PlayMode | NOT RUN |
| APK Android | NOT RUN |
| Ejecución en Quest | NOT RUN |
| Confort y comprensión con personas | NOT RUN |
| Habitaciones reales no usadas para desarrollo | NOT RUN |
| Rendimiento en dispositivo | NOT RUN |
| Canal Competition y ensayo de instalación | NOT RUN |
| Candidatura enviada | NO |

## Detalle de la validación inicial

[GitHub Actions: Foundation checks, ejecución 1](https://github.com/zhbozzo/metavr/actions/runs/37241476190), commit `a84e9b82b94cf61d8090e28a8f2325aafe5c3a6a`, runner ubuntu-24.04. Estado observado: completed / success. Pasos de comprobación de archivos/enlaces y tests de referencia completados con éxito.

Además se ejecutó `python3 -m unittest discover -s tests -v` localmente con Python 3.13.5: 20 tests, resultado OK. Como la clonación HTTP desde el entorno local estaba bloqueada por DNS, se prepararon copias de los dos archivos de código y se verificaron sus hashes de blob Git contra el árbol remoto antes de ejecutarlas:

- `prototypes/reference_model.py`: `2cbcfbbfac08c36c993bdf4311b0f34748398e35`.
- `tests/test_reference_model.py`: `623c463cf6bf585661db4213362a45bdebe732a8`.

La comprobación documental completa se ejecutó en Actions sobre el checkout del repositorio, no sobre una carpeta local parcial. El checker valida presencia y enlaces Markdown relativos simples; no verifica páginas externas, exactitud de todas las afirmaciones ni compatibilidad de SDKs.

Los tests cubren marcos yaw/escala, reflexión, captura, cancelación y resolución única en un modelo de referencia. No cubren input real, Unity 6DoF, física completa, room scans, rendimiento o seguridad física. La actualización de este registro no modifica esos archivos de código.

## Versiones y acceso

Unity: por validar. Meta Core/Interaction/MRUK/provider XR: por validar. Android tooling: por validar. Quest y versión de sistema: acceso por confirmar. No se han inventado manifests ni ProjectSettings para aparentar una instalación.

## Backlog

RB-001 a RB-020: pendientes. Las pruebas Python no completan RB-002, que requiere implementación y pruebas Unity 6DoF.

## Próximo paso

Abrir este repositorio en el agente de código y ejecutar `prompts/INICIO.md`. Trabajar RB-001: auditar la máquina, instalar/verificar una combinación compatible y ejecutar una escena mínima con passthrough, manos y un objeto seleccionable en un Quest real. Registrar bloqueos concretos si falta editor o visor.

No empezar boss, arte final, backend o más documentación estratégica antes de desbloquear esa prueba.

## Actualización al terminar una sesión

Añadir fecha, ticket, commit/archivos, comandos ejecutados, resultado observado y limitaciones. Usar PASS, FAIL, BLOCKED o NOT RUN. Adjuntar solo evidencia no sensible. No reemplazar pendientes por estimaciones ni considerar que compilar demuestra comodidad.
