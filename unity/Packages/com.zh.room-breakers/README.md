# ROOMBREAKERS Scale Lab · 0.1.0

Primer incremento de código: núcleo original C# y escena de depuración sintética para Unity. **No es aún un proyecto Unity generado por el editor ni una aplicación Quest.** El import, la escena y los tests Unity necesitan validarse en un editor real. No contiene Meta XR, passthrough, room scanning o hand tracking.

## Qué contiene

`Runtime/Core`: poses 6DoF, marcos con escala uniforme, correspondencia habitación/maqueta, una entidad autoritativa, propiedad de captura, offset de posición/rotación, devolución validada, cancelación, pausa por motivos independientes y recalibración en pausa. El núcleo no depende de UnityEngine; el mismo código fuente compila como .NET Standard 2.1 en `validation/`.

`Runtime/Debug`: escena con habitación y maqueta sintéticas, dos vistas de la misma entidad, destino de devolución, proxy visual de mano ampliada y controles de ratón. Es una prueba de desarrollo, no el juego final. La figura de mano es un proxy geométrico, no articulaciones rastreadas.

`Editor`: comando para crear la escena sin fabricar YAML, GUID, ProjectSettings o manifests de Meta. `Tests/Editor`: cuatro casos para comprobar las transformaciones contra el motor Unity real; no ejecutados por el workflow .NET.

## Abrir el laboratorio en Unity

Trabajar en esta rama o en el commit que contenga este paquete. Crear/abrir un proyecto 3D real con Unity 6, preferentemente bajo `unity/RoomBreakers` siguiendo `docs/ENTORNO.md`. La versión `6000.0` del paquete es el objetivo mínimo de API, **no una combinación de SDKs Meta validada**.

En Package Manager, usar **Install package from disk** y seleccionar el `package.json` de esta carpeta. No copiar todo el paquete a Assets además de instalarlo. Cuando el editor termine de compilar, abrir **Tools → RoomBreakers → Open Scale Lab** y pulsar **Play**. Usar la pestaña **Game**. La escena se crea en memoria después de pedir guardar cambios existentes; no sobrescribe escenas guardadas.

Mover el cubo pequeño amarillo hasta el anillo verde de la maqueta. El cubo grande representa el mismo estado. Q/E orientan la captura; la rueda cambia la altura; Esc cancela. Soltar fuera del destino restaura la posición inicial de captura. Pausar para cambiar el tamaño de la maqueta. El botón de pérdida de tracking **simula un fallo**, no comprueba sensores reales.

El laboratorio usa IMGUI para el ratón y no añade dependencias de Input System. Espera shaders unlit del pipeline Built-in o URP. No añadir esta escena a una candidatura como si fuera MR funcional.

## Pruebas

Desde la raíz del repositorio, con SDK .NET y runtime .NET 8 instalados:

```bash
dotnet run --project validation/RoomBreakers.Core.Tests/RoomBreakers.Core.Tests.csproj --configuration Release
```

El runner compila los archivos reales de `Runtime/Core` y falla con exit code 1 ante un test fallido. No tiene paquetes NuGet externos. `validation/NuGet.Config` impide descargas implícitas.

Para los tests Unity, instalar la versión de Unity Test Framework compatible con el editor. Añadir `com.zh.room-breakers` al array `testables` del manifest del proyecto existente, sin sustituir sus demás campos, y ejecutar EditMode en Test Runner. Registrar versión, logs y resultado. No llamar a los tests .NET una ejecución de Test Runner.

## Límites técnicos de esta entrega

Una sola entidad y un destino; sin enemigos, oleadas, IA, audio final o guardado. El límite de interacción no detecta obstáculos físicos. El prototipo no calcula rutas en habitaciones reales. El control de captura es reutilizable; el adaptador de manos real sigue pendiente.

El editor generará los .meta al importar el paquete local. Conservarlos después en Git junto con el proyecto real y sus versiones. No se han inventado referencias a assets ni GUIDs.

## Fuentes consultadas

- [Instalar paquete local](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-ui-local.html)
- [Formato de asmdef](https://docs.unity3d.com/6000.0/Documentation/Manual/assembly-definition-file-format.html)
- [Perfiles .NET](https://docs.unity3d.com/6000.0/Documentation/Manual/dotnet-profile-support.html)
- [OnGUI](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/MonoBehaviour.OnGUI.html)
- [MenuItem](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/MenuItem.html)
- [Quaternion](https://learn.microsoft.com/dotnet/api/system.numerics.quaternion)

Las fuentes documentan APIs, no prueban que esta integración se haya ejecutado en Unity o Quest.
