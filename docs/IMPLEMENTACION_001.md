# Incremento 001 · núcleo C# y Scale Lab

## Decisión

El entorno de esta sesión no tiene Unity, dotnet ni un visor conectado, y no resuelve github.com por DNS desde el shell. La conexión GitHub sí permite leer/escribir el repositorio y ejecutar sus workflows. No se aceptó una licencia ni se instaló software en el equipo de Lorenzo.

Se implementa un incremento desacoplado: núcleo C# comprobable en GitHub Actions y un paquete local para que un editor Unity real genere la primera escena. Esto no da por completado RB-001 y no altera la ruta Unity/Meta. No se cambia a WebXR para aparentar una entrega más rápida.

## Archivos

Código en `unity/Packages/com.zh.room-breakers/`. Runner de comportamiento en `validation/RoomBreakers.Core.Tests/`. Workflow `dual-scale-core.yml`. Instrucciones del paquete en su README. La antigua referencia Python permanece intacta.

## Contrato implementado

Una entidad con ID estable; una pose canónica; vistas a dos escalas; quaternions 6DoF; escala uniforme limitada explícitamente a 0,0001–100; rechazo de datos inválidos; captura exclusiva por mano; offset completo respecto de la mano; cancelar/devuelve una sola vez; pausa por usuario, tracking, foco o colocación sin sobrescribir otros motivos; recalibración solo en pausa; reloj sin recuperar tiempo ausente. No se implementa locomoción ni una segunda física.

El laboratorio de escritorio instancia geometría sintética y el mismo estado. La entrada de ratón no demuestra hands-first. La presentación y los tests EditMode están escritos, pero requieren import y ejecución en Unity. No se ha generado APK.

## Validación prevista y límites

El workflow compila los archivos C# de producción como netstandard2.1 y ejecuta tests en .NET 8. Los tests verifican quaternions, 300 casos de ida/vuelta con semilla fija, propiedad/offset de captura, fallos de entrada, devolución única, cancelación, pausa y recalibración. El resultado observado se registra en ESTADO.md al terminar.

Los tests Unity comparan con Transform.TransformPoint y composición Quaternion del motor. No hay sustitutos de UnityEngine ni stubs que conviertan una compilación .NET en una falsa prueba Unity.

## Pendiente

RB-001: editor/SDKs/visor. RB-002: validación real en Unity y metadatos generados. RB-003: observación del laboratorio en Play Mode. RB-004: adaptador de manos real y pruebas de dispositivo. No se marcan como completos.

Siguiente acción local: importar el paquete en un proyecto Unity real y ejecutar Scale Lab. Si aparece un error de compilación o import, conservar el texto completo del error y la versión del editor, sin secretos. No empezar un boss ni backend para evitar ese bloqueo.
