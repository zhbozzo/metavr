# Shell y reflector — incremento 006

Estado: lógica C# compilada y comprobada en .NET. Presentación y conexión Unity escritas; NO se han ejecutado dentro del editor ni de un Quest. Este incremento no convierte el repositorio en un APK terminado.

## Recorrido implementado

La ruta normal del demo ahora activa `includeShell` en `FirstEncounterRig`:

Cargar habitación → aprender a devolver Mote → completar tres devoluciones → orientar el reflector → devolver el pulso de Shell → capturarlo durante su vulnerabilidad → devolverlo a la grieta → victoria.

`FirstEncounter` conserva `includeShell=false` por defecto en su constructor de dominio para el laboratorio/regresión básico de tres Motes; el rig de la escena lo activa explícitamente. Los tests nuevos recorren el modo ampliado. No se presenta el antiguo recorrido como prueba de Shell.

## Decisión de alcance: soporte fijo, orientación libre

El reflector gira sobre un soporte validado; no se puede trasladar libremente por la habitación en esta versión. Al acercar la mano y hacer pinza se toma su orientación. Girar la muñeca cambia la normal de su superficie; abrir los dedos deja la nueva orientación. Cancelar o perder seguimiento restaura la orientación del inicio de esa captura.

`ScaleSession` distingue `ReturnToTarget` de `OrientInPlace`. Una orientación colocada aumenta `OrientationCommitCount`, NUNCA el contador de criaturas devueltas. La traslación de la mano no arrastra el soporte. Se rechaza una interacción que se aleje más de 0,15 m del punto de agarre en el espacio de la maqueta. Ese número es un valor propuesto de interacción, no una garantía de ergonomía o seguridad.

Esto recorta el posicionamiento libre previsto en JUEGO para terminar una mecánica comprobable, sin fingir que esa parte está implementada. No cambia la decisión de una simulación autoritativa y dos representaciones.

## Shell y el pulso

El primer tramo espera una colocación intencional del reflector sin disparar. Después, Shell anuncia su ataque con un intervalo configurable. El pulso recorre un corredor geométrico hasta el centro del reflector fijo. Su normal determina la reflexión; una superficie de canto falla en vez de generar números inválidos.

Hay un pulso activo como máximo y un solo rebote. La salida usa colisión de segmento contra el volumen de Shell, no solo la posición al final del frame. La geometría se comprueba hasta el primer impacto: una obstrucción anterior impide el acierto; una posterior no lo borra. Salir de la geometría permitida o superar la distancia máxima consume el pulso. Un fallo drena una luz del refugio una sola vez; no se simula un segundo ataque oculto.

Un pulso devuelto abre la armadura. El objeto manipulable pasa entonces del reflector a Shell. Se exige recuperar una mano abierta antes de capturar el nuevo objeto: sostener la pinza anterior no lo toma automáticamente. La vulnerabilidad no se agota mientras Shell está sujeto. Al soltar fuera de su destino se cancela la captura; vuelve a contar la ventana restante.

La vida restante del tramo de Motes se conserva. La victoria requiere devolver Shell; romper su armadura no basta. El reinicio confirmado desde esta fase vuelve al tutorial de Motes y mantiene las pausas de usuario/foco/colocación que correspondan.

## Habitación y anticipación visual

`ReflectionLane.TryCreate` examina hasta 24 candidatos en la ruta ya calculada. Requiere un corredor libre, espacio alrededor del soporte y un trayecto válido de captura hacia el portal. Una habitación sin corredor válido se rechaza ANTES de comenzar el encuentro ampliado con un mensaje para cambiar la dirección y cargar de nuevo. No se genera una sala falsa de reemplazo.

La previsualización usa la misma fórmula y prueba de impacto del pulso. Se corta antes de paredes/obstáculos y muestra texto ALIGNED cuando la dirección alcanza Shell. No es puntería automática: la rotación sigue dependiendo del jugador. Durante vuelo, el pulso conserva la dirección resultante del instante del rebote; una rotación posterior prepara otra dirección, no curva mágicamente el disparo ya reflejado.

## Presentación escrita

`ShellDuelView` dibuja armadura segmentada, disco con soporte y barra de agarre, marca de normal, trayectorias, anillo de aviso y pulso en ambas escalas. La armadura se distingue por geometría, no solo por color. El refugio tiene tres luces visibles, además del contador. Se añadieron tonos originales generados en código para lanzamiento, rebote y ruptura de armadura.

No hay colliders activos ni callbacks de daño en las vistas. La mano ampliada anterior sigue siendo esquemática, no una mano articulada. Materiales y clips se crean al iniciar y se liberan al disponer la vista; no hay assets comprados o descargas de terceros.

## Probar cuando exista Unity configurado

Las entradas de editor siguen siendo `Tools → RoomBreakers → Open First Encounter (Desktop)` y `Add Device Room Encounter`.

En escritorio: devolver los Motes con ratón; mantener pulsado sobre el reflector y usar Q/E para girarlo en pasos de 15 grados; soltar para dejar la orientación. Escape cancela. Ese adaptador está limitado a Editor/Development y no suplanta una mano Meta.

En el visor: pinzar el reflector y girar la muñeca. La captura puede completarse con una sola mano. El modo dispositivo sigue requiriendo el rig real, los SDKs y passthrough previamente configurados. No se han observado renderizado, latencia, fuentes, orientación de muñeca ni confort.

## Evidencia inicial

[CI 37311959004](https://github.com/zhbozzo/metavr/actions/runs/37311959004), job `111769272682`, head `99cb90e1796524aa24a5385bac0a7d53c9c4bd48`: logs leídos con las seis suites aprobadas. 183 casos previos + 35 de Shell = 218 casos C#, cero fallidos. Incluyen recorrido de una mano hasta victoria, vulnerabilidad mientras se sostiene, reinicio, invalidación de habitación, datos inválidos y geometría de reflexión.

Un caso recorre cien secuencias con rotaciones aleatorias reproducibles; son entradas sintéticas, no usuarios ni dispositivos. Las pruebas compilan el núcleo real netstandard2.1; no compilan Unity/Meta ni esta presentación. Los checks finales del PR deben revisarse antes de integrar.

Referencias de las APIs gráficas consultadas: [Quaternion, Unity 6000.0](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/quaternion) y [LineRenderer.SetPositions](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/linerenderer/setpositions). La documentación no sustituye ejecutar el editor.

## Pendiente

Compilación e integración real Unity/Meta, pruebas de visor, ajuste de umbrales/escala/ayudas y coste de render. No se ha completado una sesión calibrada de seis a ocho minutos, varios enemigos simultáneos, guardado, colocación libre del reflector ni arte final. RB-011/012 avanzan en código y pruebas de dominio; su aceptación de dispositivo sigue pendiente.
