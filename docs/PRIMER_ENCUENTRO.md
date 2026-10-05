# Primer encuentro — incremento 005

Ahora hay código para un recorrido completo pequeño: cargar una habitación, construir su maqueta, elegir una grieta y una ruta, enseñar captura, proteger el refugio, ganar/perder y reiniciar. **El dominio C# está compilado y probado; la escena, los scripts Unity y el adaptador MRUK están escritos pero todavía no se han compilado ni observado en Unity o Quest.** No es la entrega final de seis a ocho minutos.

## Recorrido implementado

La ficha LOAD ROOM recibe una pinza y una liberación sobre el mismo objetivo. En modo dispositivo solicita permiso de datos espaciales cuando falta y carga los datos guardados en el Quest. La app no abre un escaneo sin avisar ni descarga una sala ficticia si falla la real. Permiso denegado, ausencia de Space Setup, timeout o geometría incompatible producen un estado visible con opción de reintentar.

El plan coloca un refugio en el sector frontal, una grieta sobre una pared compatible y una ruta libre de los obstáculos representados. La primera criatura espera sin atacar mientras aprendes: pinza en la miniatura, moverla a la zona de devolución marcada delante del portal y abrir los dedos. Después comienzan criaturas móviles, una a la vez. Tres devoluciones en total cierran la grieta; tres llegadas al refugio terminan la partida en derrota. El reinicio tiene confirmación y conserva pausas de foco/usuario. No hay puntuación repetida por eventos duplicados.

La criatura no avanza mientras está sujeta, pausada o durante un menú de confirmación. El movimiento autónomo sigue los segmentos validados, incluso cuando un frame cruza una esquina. El arrastre también comprueba el volumen recorrido contra mobiliario y paredes: no basta con que sus dos extremos estén libres. Una liberación inválida vuelve al punto de captura sin premio. La pausa por datos de manos perdidos sigue perteneciendo al controlador existente.

## La geometría cambia el encuentro

`RoomGeometry.cs` implementa un polígono de suelo validado, paredes verticales, cajas conservadoras de obstáculos y una búsqueda acotada. No navega arbitrariamente por sofás y techos. Usa un plano horizontal de vuelo de las criaturas y un sector frontal. El refugio y la ruta se seleccionan solo entre posiciones válidas. El portal evita también volúmenes etiquetados como puertas, ventanas y otros elementos excluidos que el adaptador suministre.

La malla lógica se limita a 6.400 celdas y la sala a 16 metros por eje; hay límites de vértices, paredes y obstáculos. Se rechazan polígonos degenerados o autointersectados. Una búsqueda por sala se reutiliza para los candidatos de portal; la suavización verifica cada nuevo segmento. Son límites de implementación, no garantía de rendimiento ni comodidad. El cálculo ocurre al preparar la escena y debe perfilarse en el dispositivo.

El test de dos distribuciones comprueba que añadir un obstáculo cambia la grieta elegida o la ruta. Otro caso recorre cien disposiciones sintéticas y exige que toda ruta aceptada tenga sus segmentos libres; las configuraciones sin solución se rechazan con una explicación. Son fixtures, no habitaciones reales probadas.

## Adaptador real y consentimiento

`MetaRoomSource` lee MRUK v207 mediante un componente separado y habilitado solo con `com.meta.xr.mrutilitykit` en `[207.0.0,208.0.0)`. Se exige Device, World Lock habilitado y carga inicial automática desactivada. Después del resultado de carga se comprueba que exista una habitación local, localizada y que contenga al jugador; no basta con usar la primera sala devuelta.

Se normalizan suelo, paredes, volúmenes y planos al mismo marco canónico. La versión inicial admite un suelo horizontal y paredes rectangulares verticales. Una pared irregular o un objeto sin representación adecuada no se inventa. Las cajas de mobiliario rotado son conservadoras y pueden rechazar un espacio realmente libre. El global mesh no se usa como detector completo de todo lo que existe alrededor.

Los eventos de sala cambiada/eliminada y las comprobaciones de localización invalidan el encuentro. No se sigue jugando sobre geometría que ya no corresponde. Cargar otra vez comienza un nuevo encuentro; todavía no se migra una partida entre salas. Se descartan resultados asíncronos de solicitudes antiguas. Un timeout no puede cancelar internamente la búsqueda nativa, por lo que se observa y descarta su resultado tardío.

No se guardan ni envían planos, UUID de sala, imágenes o trazas de manos. Los registros de error del adaptador omiten esos datos. La geometría de MRUK **no es un sistema de seguridad física**: no detecta necesariamente personas, mascotas u objetos movidos. El jugador no debe caminar hacia el portal ni tocar muebles.

## Presentación programada

`FirstEncounterView` genera dos representaciones del mismo estado, contornos de la habitación y sus obstáculos, una grieta, las criaturas Mote y Pip con su refugio. En modo real no dibuja una habitación ficticia encima del passthrough. La maqueta conserva el rumbo del espacio grande; su centro se coloca delante del jugador, mientras el panel mantiene dimensiones físicas independientes de su escala.

Las criaturas tienen modelos provisionales construidos con primitivas, ojos, feedback de selección y orientación compartida. El portal se contrae con las devoluciones. Hay textos de tutorial/estado/resultado y sonidos originales generados en código mediante un AudioSource espacial. La mano grande es una silueta esquemática de pinza, **no una reconstrucción articulada de los dedos**. No hay arte final, destrucción de pared, oclusión avanzada o mezcla de audio ya aprobada.

PAUSE/RESUME, RESTART y MOVE reutilizan la lógica de controles comprobada. MOVE desplaza solo la maqueta; no la cámara ni el contenido grande. Dos aros visibles delimitan la zona de devolución en ambas escalas, sin exigir atravesar una pared real.

## Abrir la versión de escritorio

Con el paquete local instalado en un proyecto Unity real:

1. `Tools → RoomBreakers → Open First Encounter (Desktop)`.
2. Play; usar Game view. La escena está marcada explícitamente como sintética.
3. Clic y liberación sobre LOAD ROOM. Dejar pasar un frame de mano abierta antes de otra pinza. Arrastrar la pequeña criatura hacia sus aros y liberar allí. Esc cancela.
4. Probar tres devoluciones, derrota, pausa, confirmación de reinicio y MOVE.

El editor crea la escena y sus metadatos reales; no se incluyen YAML/GUID inventados. El ratón produce entradas sintéticas para los mismos controladores, no una copia de la lógica del juego. La fuente de habitación y el ratón sintéticos se deshabilitan en builds no Development. **Este procedimiento está preparado, todavía no ejecutado por esta sesión.**

## Abrir la ruta de dispositivo

En un proyecto con rig XR, cámara, manos rastreadas, passthrough, permisos y SDKs compatibles previamente configurados:

1. Configurar MRUK: Device; Load Scene On Startup OFF; World Lock ON.
2. `Tools → RoomBreakers → Add Device Room Encounter`.
3. Asignar MRUK, la cámara XR existente y los componentes IHand rastreados. Una mano basta; no usar manos sintetizadas desde controles.
4. Guardar la escena, compilar y ejecutar la comprobación real en Quest. No se afirma que estos pasos se hayan completado.

El asistente no cambia el proveedor XR, reemplaza la cámara, instala SDKs o acepta términos. Se serializa el shader del prototipo para no depender únicamente de Shader.Find en el build. La configuración concreta de render/passthrough y la compatibilidad de assemblies siguen por verificar en Unity.

## Pruebas ejecutables

```bash
dotnet run --project validation/RoomBreakers.Encounter.Tests/RoomBreakers.Encounter.Tests.csproj --configuration Release
```

35 casos nuevos: geometría, rutas, entrada sintética de manos, tutorial, victoria, derrota, pausa, reinicio y colisiones de arrastre. Las cinco suites suman 183 casos de núcleo C#. El workflow las descubre y ejecuta; no compila los scripts Unity/Meta. Evidencia exacta en [ESTADO](ESTADO.md).

## Alcance pendiente

Import y compilación Unity/SDK, prueba de ambos comandos de editor, Android/Quest, ajuste de selección en maquetas pequeñas, comodidad, oclusión, coste de líneas/materiales, localización y rendimiento real. Sigue faltando el enemigo Shell y reflector, más encuentros, progresión persistente y acabado final. No hay infraestructura, multiplayer, APIs de IA o candidatura enviada.

## Fuentes oficiales de la integración

Contratos revisados durante esta sesión; no equivalen a una instalación validada:

- [MRUK v207](https://developers.meta.com/vr/reference/mruk/v207/class_meta_x_r_m_r_utility_kit_m_r_u_k/): carga Device, permiso espacial, eventos y estado de localización.
- [MRUKRoom v207](https://developers.meta.com/vr/reference/mruk/v207/class_meta_x_r_m_r_utility_kit_m_r_u_k_room/): suelo, anclajes, procedencia local y pertenencia a habitación.
- [MRUKAnchor v207](https://developers.meta.com/vr/reference/mruk/v207/class_meta_x_r_m_r_utility_kit_m_r_u_k_anchor/): límites de planos/volúmenes y etiquetas.
- [Debugging MRUK](https://developers.meta.com/vr/documentation/unity/unity-mr-utility-kit-debug/): nombre de assembly `meta.xr.mrutilitykit`.
- [Configuración inicial MRUK](https://developers.meta.com/vr/documentation/unity/unity-mr-utility-kit-gs/): componentes y requisitos del proyecto.
