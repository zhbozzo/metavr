# Unity — primer código disponible

El código está en [Packages/com.zh.room-breakers](Packages/com.zh.room-breakers/README.md): núcleo C# de doble escala y laboratorio sintético de Unity. El núcleo pasó 49 tests .NET. El import, Play Mode y los cuatro casos EditMode aún necesitan un editor real.

El proyecto principal se creará en `unity/RoomBreakers` con una instalación compatible de Unity, siguiendo [ENTORNO](../docs/ENTORNO.md). No se han fabricado ProjectSettings, GUIDs o lockfiles para aparentar una instalación.

## Abrir el incremento

Crear/abrir el proyecto 3D real en Unity 6. En Package Manager, usar **Install package from disk** y seleccionar el `package.json` de `Packages/com.zh.room-breakers` en este repositorio. Después: **Tools → RoomBreakers → Open Scale Lab → Play**, en Game view. Instrucciones y límites en el README del paquete.

El laboratorio usa ratón y geometría sintética. No integra Meta XR ni demuestra manos, passthrough, room scan, confort o rendimiento en visor. No enviarlo como app Quest funcional.

RB-001 sigue pendiente de aceptación en el editor/visor. Parte del núcleo de RB-002 ya está implementada; no hay que reescribirla. Validar primero los tests contra Transform/Quaternion reales y la presentación, luego conectar el adaptador de manos y entorno.

Conservar los .meta que genere el editor para el paquete local y las configuraciones/lockfiles del proyecto. No commitear Library, Temp, builds, logs privados, claves de firma ni SDKs/assets ajenos sin permiso.

La referencia Python de `../tests` permanece separada. Para compilar y probar el C# real, usar el comando .NET del README principal. Ambos workflows del PR #1 terminaron correctamente en su commit `d1c1d990df4a9d0c8b242f76c119f9a8414421c3`: [Foundation checks](https://github.com/zhbozzo/metavr/actions/runs/37248587351) y [Dual-scale CSharp core](https://github.com/zhbozzo/metavr/actions/runs/37248587359). Esta actualización solo cambia esta guía.
