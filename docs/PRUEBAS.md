# Pruebas y evidencia — simulator-first

[Decisión vigente](SIMULATOR_FIRST.md): todo en computador; hardware del equipo fuera del plan. La falta de visor no bloquea un hito de simulación, pero no se convierte en PASS de dispositivo. Las reglas de entrega siguen vigentes.

## Niveles independientes

| Nivel | Qué acredita |
| --- | --- |
| Repositorio/Python | Archivos, enlaces, modelo de referencia y herramientas |
| Núcleo .NET | Reglas C# con entradas sintéticas, incluidos archivos temporales de guardado |
| Unity EditMode | Los tests ejecutados contra APIs reales del motor |
| Unity PlayMode | Ciclo de componentes, vistas e integración usando sus fuentes declaradas |
| Meta XR Simulator | Ruta OpenXR/SDK y juego observado con manos/entornos simulados |
| Compilación Android | APK producido, configuración y dependencias revisadas |
| Acceso de evaluación | Canal/invitación comprobados hasta el punto realmente accesible |
| Hardware | NO VALIDADO y fuera del plan; no confundirlo con ninguno anterior |

La escena desktop de ratón no acredita XR Simulator. Meta documenta el simulador como runtime de API sin imagen Android; no prueba por ello el APK. Fuentes R1/R4 en SIMULATOR_FIRST.

## Comandos ya existentes

```bash
python3 tools/check_repo.py
python3 -m unittest discover -s tests -v
```

Con .NET, ejecutar suites de validation según README. Con editor/proyecto/Test Framework reales:

```bash
python3 tools/run_unity_checks.py --platform EditMode
python3 tools/run_unity_checks.py --platform PlayMode
```

El verificador exige 12 EditMode o cuatro PlayMode según el modo. Sus tests Python usan procesos/XML sintéticos; no son una ejecución Unity. Ver [AUTOCHECKS](AUTOCHECKS.md). El runtime XR requiere comprobación adicional, no incluida automáticamente por esos comandos.

## Invariantes

Mantener pruebas de round-trip 6DoF, offset de captura, exclusión de propietarios, rechazo de datos inválidos, resolución/daño únicos, pausa sin catch-up, cancelación segura, recalibración, armadura durante Held, un pulso/un rebote, geometría barrida y guardado recuperable. El número de tests no es un porcentaje de calidad o probabilidad de ganar.

## Matriz sintética mínima a ejecutar

Vacía, estrecha/pequeña, escritorio, sofá/obstáculo, amplia admitida, origen rotado, suelo cóncavo, paredes/superficies faltantes, sector frontal bloqueado, corredor reflectante imposible, cambio de sala durante captura y carga fallida. Reservar fixtures no usados para ajustar el planificador.

Por caso: fixture/versiones, perfil, origen de datos, pasos, esperado, observado y evidencia. Un resultado válido puede ser un rechazo recuperable. No producir configuraciones imposibles para forzar un PASS. No publicar scans domésticos.

## Matriz de input y UI

Recorrer arranque → carga/colocación → Motes → reflector → Shell → resultado → reinicio/pausa/salida mediante manos simuladas. Cubrir solo izquierda, solo derecha y ambas; pinza cerrada al iniciar; entradas duplicadas/atrasadas/NaN; pérdida de mano propietaria; recuperación abriendo mano; foco; recolocación; cambio de etapa y controles superpuestos.

La entrada debe pasar por el adaptador de SDK/runtime al acreditar XR. Inyectar HandSample al núcleo es útil, pero se etiqueta como prueba de dominio. No usar un controlador físico como requisito. No manipular estado interno para fabricar un vídeo de gameplay.

## Observación y claridad

Revisar ambos ojos y FoV del perfil, tamaño/altura de maqueta, UI, objetivos y mano ampliada. Se busca juego sentado con movimientos cortos. Usuarios de escritorio nuevos, cuando sea viable, pueden aportar evidencia de comprensión; registrar número, ayudas y consentimiento. No llamar a esto ergonomía o confort físico VR comprobados.

En capturas deliberadas registrar aciertos, fallos y denominador por input/fixture. Los resultados son de la simulación, no tasa de precisión del sensor. No extrapolar iluminación, oclusión de dedos, fatiga o latencia real.

## Rendimiento y plataforma

Registrar CPU/GPU, API gráfica, resolución, memoria, tiempos de frame y sesiones repetidas del host. Separar ejecución grabada y sin grabación. No presentar fps del Mac como fps Quest ni Hz reportados como fps logrados.

Conservar un presupuesto conservador para Android y revisar build/stripping/shaders, sin inventar benchmarks. Environment Depth de XR Simulator está documentado para Windows; no exigirlo en Mac ni informar que se probó allí. Perfil físico, batería y temperatura permanecen no medidos.

## Evidencia para demo y APK

Vídeo desde XR Simulator/emulador equivalente identificado; misma versión de código del candidato, fixtures y diferencias de plataforma declarados. APK real separado: commit/hash/versión y revisión de entrada/escena/permisos. No afirmar que el APK arrancó por haber grabado el editor.

La ruta release no requiere mouse, datos de laboratorio o conexión al ordenador del autor. Invitación web revisada no equivale a instalación del juez. No es necesario conseguir hardware para cerrar los hitos internos de alcance simulado.

## Severidades y aceptación

P0: caída, bloqueo de sesión, daño/recompensa inválidos graves, necesidad de mandos en la ruta prevista, secretos o build imposible. Se corrigen antes de enviar.
P1: selección/escala inestable, sala imposible sin recuperación, degradación persistente observada. Priorizar antes de efectos.
P2: acabado no bloqueante.

La ausencia de pruebas de hardware es una limitación de alcance explícita, no un PASS y no un ticket de compra. Los fallos conocidos sí bloquean una afirmación de funcionamiento.

## Registro

```text
Fecha / ticket / commit:
Nivel: documentos | .NET | EditMode | PlayMode | XR Simulator | Android build | acceso
Host / editor / SDK / runtime / API gráfica:
Perfil y fixture; origen: synthetic | runtime-simulated | device:
Input utilizado y adaptador recorrido:
Pasos / esperado / observado:
Mediciones y denominadores:
PASS | FAIL | BLOCKED | NOT RUN:
Evidencia no sensible:
Hardware: NO VALIDADO; fuera del plan del equipo
Limitaciones / siguiente acción de software:
```

No rellenar métricas con estimaciones. Resultados actuales en [ESTADO](ESTADO.md); fuentes técnicas de esta política en [SIMULATOR_FIRST](SIMULATOR_FIRST.md).
