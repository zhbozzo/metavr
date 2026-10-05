# Controles dentro del visor — incremento 004

Estado: lógica C# compilada y probada con entradas sintéticas; presentación y conexión Unity escritas, pendientes de ejecutar en el motor y el visor. Esto no es todavía una app Quest terminada.

## Recorrido implementado en código

El `HandsScaleRig` crea automáticamente `HandsControlPanel` al iniciar. No hay que añadir otro componente en el asistente ni cablear botones del Inspector para las acciones normales del laboratorio. Se conserva el recorrido de instalación de [MANOS](MANOS.md).

La fila de fichas bajo la maqueta muestra **PAUSE/RESUME, RESTART y MOVE**. Son fichas seleccionables con pinza, no botones de contacto con el índice. La instrucción visible es `Pinch a token, then open to choose`. Al acercarse se resaltan; al mantener la pinza cambian de estado visual; abrir sobre la misma ficha activa la acción. Alejar la mano cancela. Las zonas de selección no se mueven al resaltarse.

- PAUSE/RESUME modifica solamente la pausa del usuario. No borra pérdida de foco ni un fallo de colocación.
- RESTART abre una confirmación. CONFIRM reinicia el objeto de prueba, no los ajustes del proyecto ni el progreso de una futura partida. CANCEL conserva su estado. Cada elección requiere una nueva secuencia de mano abierta → pinza → liberación.
- MOVE también pide confirmación. Con una pose válida de cabeza coloca únicamente la maqueta delante del usuario; conserva escala, marco de habitación y estado canónico del objeto. No mueve la cámara. La posición se calcula al confirmar, no sigue la cabeza continuamente.
- Mirar casi verticalmente, tener una jerarquía inválida o un marco de habitación inesperado rechaza el movimiento. Se conserva el estado anterior y se permite reintentar/cancelar. Un fallo estructural del rig todavía requiere corregir su configuración; no se oculta con un supuesto escaneo.

Con una mano: soltar primero el objeto y utilizar las fichas. Con dos manos, la libre puede pausar una captura; adquirir la ficha cancela el objeto sin premiar una devolución. La mano que sostiene el objeto no puede activar a la vez un control.

## Autoridad y estados

`NearControlDriver` valida secuencia, antigüedad y pose de cada mano, mantiene un solo propietario de control y usa la pose nueva de liberación. Un dato almacenado no renueva el watchdog. Pérdida de datos, objetivo desplazado/deshabilitado o señal inválida cancela, nunca confirma.

`HarnessControls` coordina ese controlador y `HandCaptureDriver` sobre **la misma** `ScaleSession`. La interfaz tiene prioridad cuando coincide con un objetivo de juego. Se añadió `PauseReason.Menu`, independiente de User, TrackingLost, FocusLost y Placement. Mientras se sostiene una ficha o hay una confirmación, la simulación está pausada. Salir de ese modo exige rearme de captura y evita que el gesto atraviese el menú y agarre el objeto.

La pérdida de foco elimina órdenes pendientes sin borrar el resultado del objeto. Restablecer foco no ejecuta una confirmación antigua. La API externa del coordinador no permite borrar directamente Menu ni TrackingLost.

## Archivos

- `Runtime/Core/NearControlDriver.cs`: objetivos y máquina de interacción de fichas.
- `Runtime/Core/HarnessControls.cs`: coordinación, confirmaciones y cálculo de colocación.
- `Runtime/UnityInput/HandsControlPanel.cs`: fichas, etiquetas y feedback visual.
- `Runtime/UnityInput/HandsScaleRig.cs`: conexión a fuentes de manos, vistas, controles y aplicación de colocación.
- `validation/RoomBreakers.Controls.Tests`: pruebas del núcleo real sin dependencias Unity.

## Pruebas ejecutadas

La suite nueva ejecuta 38 casos, incluyendo pausa/reanudación por señales de mano, inicio con una sola mano, confirmación/cancelación, datos inválidos, pérdida de foco, prioridades entre menú y objeto y conservación del marco grande al mover la maqueta. Un caso recorre 5.000 frames con semilla fija; no son pruebas de personas o hardware. Evidencia y comandos en [ESTADO](ESTADO.md).

```bash
dotnet run --project validation/RoomBreakers.Controls.Tests/RoomBreakers.Controls.Tests.csproj --configuration Release
```

Las 49 pruebas previas del núcleo siguen presentes. En la prueba de motivo de pausa desconocido, el valor inválido pasa de 16 a 32 porque 16 ahora corresponde a Menu; no se eliminó la comprobación ni se rebajó una condición de seguridad.

## Comprobación pendiente en Unity/Quest

Tras importar y configurar el rig real, iniciar el laboratorio con una sola mano visible. Reanudar, devolver el objeto, cancelar un reinicio y después confirmarlo; repetir con la otra mano. Mantener una ficha y ocultar la mano: no debe dispararse la orden. Confirmar MOVE: la maqueta cambia de ubicación, pero el objeto grande, el anillo grande y la cámara permanecen donde estaban. Una pausa previa del usuario debe conservarse.

Comprobar tamaño y legibilidad de etiquetas, comodidad, comportamiento al perder el foco y ausencia de conflicto con gestos del sistema. Los valores de tamaño/distancia son propuestas, no mediciones de ergonomía. Todavía no hay seguimiento de sala real, adaptación a obstáculos, malla de mano articulada ni sonido final de los controles. El shader/fuente y la generación visual requieren prueba real de import, Play Mode y APK; los tests .NET no los compilan.

## Referencias de diseño consultadas

Consulta del 4 de octubre de 2026; documentación de trabajo, no aceptación del motor:

- Meta, [Hands Best Practices](https://developers.meta.com/vr/design/hands-best-practices/): reducir vocabulario de interacción y hacer claro el feedback.
- Meta, [Hands UI Best Practices](https://developers.meta.com/vr/design/hands-ui-best-practices/): ergonomía, controles estables y gestos de sistema. Esta primera versión no añade un gesto palm-up propio.
- Meta, [Hands Interaction Types](https://developers.meta.com/vr/design/hands-interaction-types/): distinguir agarrar/pinza de tocar con el índice. Estas fichas usan el primero; no se anuncian como implementación de poke.
- Unity, [Transform.SetPositionAndRotation](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Transform.SetPositionAndRotation.html) y [TextMesh](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/TextMesh.html): presentación espacial, pendiente de compilación en el editor real.
