# Interacción de manos — incremento 003

Estado: lógica C# implementada y probada; adaptador Meta y presentación Unity escritos, todavía sin import/compilación Unity ni ejecución en Quest. El recorrido de abajo es un procedimiento preparado, no una prueba observada. El incremento posterior [Controles](CONTROLES.md) añade la UI de pausa, reinicio y recolocación al mismo harness.

## Qué aporta

`HandCaptureDriver` convierte muestras temporizadas de mano en comandos sobre `ScaleSession`. Una pinza cercana captura la miniatura; mover la mano actualiza la única pose canónica; abrir los dedos devuelve al destino válido o cancela. Ambas vistas leen el mismo estado. La representación ampliada actual es una esfera de posición de pinza: todavía no una malla de mano articulada.

La selección usa distancia en metros del espacio de la maqueta y dos umbrales de pinza. Si ambas manos pinzan, gana la más cercana; empate a la izquierda. Una mano ya propietaria no puede ser reemplazada. Se permite operar con una sola mano visible. Una pinza iniciada lejos no captura al barrer sobre el objeto: debe abrirse y volver a pinzar.

## Pérdida de seguimiento y recuperación

Las muestras incluyen identidad, secuencia creciente, pose, fuerza de pinza, validez y tiempo de recepción monotónico. El adaptador no vuelve a fechar una pose almacenada cada frame. Se ignoran duplicados y secuencias antiguas; se rechazan datos inválidos, futuros o atrasados. `Step` debe ejecutarse incluso si no llegaron datos nuevos.

El watchdog cancela/pausa si se pierde la mano propietaria; que la otra mano siga visible no oculta esa pérdida. Para recuperar se requiere una muestra NUEVA de mano abierta. Una pose abierta almacenada o volver con la mano cerrada no reanuda una captura. Pausa de usuario, foco y colocación se conservan por separado. El temporizador de 0,20 s y los umbrales son valores de ingeniería propuestos, pendientes de ajustar en visor, no límites físicos certificados.

El frame de liberación se procesa antes de premiar el retorno: no se decide usando una posición anterior. Los fallos explícitos y la ausencia de callbacks tienen recorridos distintos, ambos cubiertos por tests del controlador.

## Adaptador Meta

`Runtime/Meta/MetaHandSampleSource.cs` lee `Oculus.Interaction.Input.IHand`. Comprueba validez/confianza de mano, pulgar e índice; usa poses mundiales de las puntas y muñeca; calcula punto medio de pinza; obtiene su fuerza. `CurrentDataVersion` distingue actualizaciones del SDK de lecturas repetidas. La primera lectura establece una línea base; se espera un cambio de versión antes de publicar datos válidos.

El tiempo registrado es el de recepción, no una afirmación sobre el instante de captura del sensor. El adaptador no puede detectar un proveedor defectuoso que avance su versión con datos falsamente válidos. No registra posiciones de manos ni geometría doméstica.

La integración es opcional: `RoomBreakers.MetaHands` y su editor se habilitan con `com.meta.xr.sdk.interaction` en `[207.0.0,208.0.0)`. Esa restricción expresa la familia de API revisada, NO una combinación instalada/probada. No forzar el símbolo manualmente para ocultar errores de versión. El paquete base no descarga SDKs ni cambia providers. Comprobar el import con y sin la dependencia es parte del próximo ensayo.

## Preparar la prueba en Unity

1. Abrir un proyecto Unity real con versión compatible y el paquete local de ROOMBREAKERS. Configurar previamente el rig XR, sus manos reales y passthrough mediante el flujo oficial. No copiar una app anterior para presentar este juego como nuevo.
2. Con Interaction SDK v207.x instalado, abrir **Tools → RoomBreakers → Add Meta Hands Harness**.
3. Asignar la cámara XR existente y componentes `IHand` de izquierda/derecha. Una mano basta. Elegir proveedores de manos rastreadas, no manos generadas a partir de controles. El asistente no adivina cuál de múltiples componentes representa datos reales.
4. Añadir el harness a la escena. No modifica la cámara ni reemplaza el rig, no instala paquetes ni guarda la escena por sí solo. Guardar normalmente desde el editor.
5. En Play Mode, espera datos válidos y mira hacia delante. El contenido virtual de prueba se coloca una vez: maqueta próxima y representación grande delante. No es un escaneo ni un detector de obstáculos. El contenido no debe motivar caminar ni tocar mobiliario.
6. Abre una mano, acerca pulgar/índice al cubo pequeño, pinza y mueve al aro. Al abrir los dedos debe retornar una vez. Alejarse del destino cancela. Oculta la mano durante la captura: debe cancelarse, sin premio ni lanzamiento, y pedir apertura de mano al volver.

El código actual añade fichas **PAUSE/RESUME, RESTART y MOVE** dentro de la escena. RESTART y MOVE requieren confirmación. Funcionamiento y límites en [CONTROLES](CONTROLES.md). Los botones del Inspector se conservan para depuración y recuperación estructural del rig; no son la UI normal de interacción. La nueva UI espacial todavía no se ha compilado ni observado en el visor. El modelo de mano ampliada y el room scan siguen pendientes.

`UnitySpatialFrame` rechaza jerarquías con escalas negativas, no uniformes o inválidas y exige escala uno para la habitación. Cambiar marcos durante juego pausa hasta confirmación; no se arregla una sola vista con offsets. `HandsHarnessPlacement` coloca solo elementos virtuales y conserva una pausa previa del usuario/foco. La colocación inicial configura ambas representaciones; la ficha MOVE posterior cambia solamente la maqueta.

## Pruebas disponibles

```bash
dotnet run --project validation/RoomBreakers.HandInput.Tests/RoomBreakers.HandInput.Tests.csproj --configuration Release
```

46 casos del controlador, incluyendo 5.000 frames de entrada con semilla fija dentro de un caso. Se ejecutan sobre código C# real con entradas sintéticas; no compilan el adaptador Meta ni la presentación Unity. En el incremento 003 se sumaron a los 49 + 15 casos anteriores: 110 casos C#. El total actual y la suite de controles posterior figuran en ESTADO.

Se añadieron ocho casos Unity `UnityInputFrameTests`, en la misma assembly EditMode que los cuatro casos anteriores. Comprueban jerarquías reales y el recorrido mano→maqueta sin doble transformación. Estado: NOT RUN. `tools/run_unity_checks.py` ejecuta la assembly cuando el proyecto/editor y los tests del paquete están configurados; no es una prueba de hardware. Comprobar en Test Runner que se ejecutan los ocho casos nuevos además de los cuatro originales.

## Fuentes oficiales revisadas

- [IHand, Interaction SDK v207](https://developers.meta.com/vr/reference/interaction/v207/interface_oculus_interaction_input_i_hand/): firmas, versión de datos, poses mundiales, confianza y pinza.
- [Paquetes y requisitos de Interaction SDK](https://developers.meta.com/vr/documentation/unity/unity-isdk-packages-and-requirements/): assembly `Oculus.Interaction` y separación de providers.
- [Obtener posiciones de articulaciones](https://developers.meta.com/vr/documentation/unity/unity-isdk-get-bone-position/): ejemplo oficial con `Oculus.Interaction.Input` y `GetJointPose`.

Revisión durante la sesión del 4 de octubre de 2026 en Chile. Comprobar la combinación exacta de editor, SDK, provider y OS en la instalación real. No se han aceptado licencias ni descargado assets de terceros.
