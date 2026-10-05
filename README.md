# ROOMBREAKERS — Tu habitación, en tus manos

> Un juego de realidad mixta en el que manipulas una maqueta de tu habitación y ves las consecuencias a escala real, sin levantarte de la silla.

**Estado: núcleo C#, controlador de manos y controles del laboratorio implementados y probados en .NET; presentación Unity y adaptador Meta escritos, pendientes de import/compilación y ejecución en el editor. No existe todavía un APK ni una app Quest validada.** El nombre es provisional. No existe garantía de ganar el concurso.

## Código: empieza aquí

El [paquete Scale Lab](unity/Packages/com.zh.room-breakers/README.md) contiene el núcleo, un laboratorio sintético con ratón y pruebas de editor. Se instala como paquete local en un proyecto Unity real. La combinación instalada de SDKs Meta sigue pendiente de validar.

En Unity 6: Package Manager → Install package from disk → seleccionar `unity/Packages/com.zh.room-breakers/package.json` → Tools → RoomBreakers → Open Scale Lab → Play. Usar Game view. **Este recorrido está preparado en código; su smoke test real en Unity está pendiente.** El [incremento 001](docs/IMPLEMENTACION_001.md) explica el alcance inicial.

**[Controlador y laboratorio de manos](docs/MANOS.md):** selección cercana con pinza, arbitraje de dos manos, vigilancia de datos atrasados, cancelación y recuperación. El adaptador opcional usa `IHand` de Meta v207.x. En una escena con rig XR ya configurado: **Tools → RoomBreakers → Add Meta Hands Harness**. El asistente conserva la cámara existente y añade una prueba de doble escala con distribución sintética; no configura passthrough ni escanea la habitación.

**[Nuevo: controles dentro del visor](docs/CONTROLES.md).** El harness genera fichas PAUSE/RESUME, RESTART y MOVE, con confirmación/cancelación para reiniciar o recolocar. Se eligen con pinza y liberación. La lógica impide que el mismo gesto active un control y capture el objeto. MOVE cambia solo la maqueta, no la cámara ni el marco grande. La presentación está escrita; aún no se ha observado dentro de Unity/Quest.

## Documentación

| Necesitas | Documento |
| --- | --- |
| Contexto y decisiones previas | [Contexto](docs/CONTEXTO.md) |
| Qué construir y qué no construir | [Producto](docs/PRODUCTO.md) y [diseño del juego](docs/JUEGO.md) |
| Instrucciones para agentes de código | [AGENTS.md](AGENTS.md) y [prompt de inicio](prompts/INICIO.md) |
| Arquitectura y transformación entre escalas | [Arquitectura](docs/ARQUITECTURA.md) |
| Configurar Unity y probar en Quest | [Entorno](docs/ENTORNO.md) |
| Controlador de manos y adaptador Meta | [Manos](docs/MANOS.md) |
| Pausa, reinicio y colocación con manos | [Controles](docs/CONTROLES.md) |
| Automatizar comprobaciones | [Autochecks](docs/AUTOCHECKS.md) |
| Orden de implementación | [Plan](docs/PLAN.md) y [backlog](docs/BACKLOG.md) |
| Requisitos y estrategia de concurso | [Concurso](docs/CONCURSO.md) |
| Evidencia, pruebas y limitaciones | [Pruebas](docs/PRUEBAS.md) y [estado real](docs/ESTADO.md) |
| Preparar build, vídeo y envío | [Entrega](docs/ENTREGA.md) |
| Fuentes oficiales y discrepancias | [Fuentes](docs/FUENTES.md) |

## La decisión central

Una sola simulación en coordenadas de la habitación; dos representaciones sincronizadas: habitación y maqueta. Agarrar una miniatura modifica esa única simulación. La mano gigante es feedback visual, no una segunda física.

La primera prueba no es un juego completo: **seleccionar una miniatura, moverla y reconocer inmediatamente la consecuencia en el espacio real**, con una interacción cómoda y fiable. Si eso no funciona en un visor, no se añade contenido para ocultar el problema.

## Alcance elegido

Unity + C# + herramientas XR/MR de Meta. Quest 3/3S son objetivos de prueba, no compatibilidad ya demostrada. Partida objetivo de 6–8 minutos; un sector frontal del entorno; dos enemigos; una herramienta reflectante; un personaje guía. Todo el recorrido final debe funcionar con manos.

No se incluye backend, login, pagos, multiplayer, IA generativa en ejecución, Terraform ni seguimiento ocular obligatorio. [Motivo de infraestructura](infra/README.md).

## Comprobaciones disponibles

Referencia, documentos y verificador, con Python 3.10 o superior:

```bash
python3 tools/check_repo.py
python3 -m unittest discover -s tests -v
```

Código C# real, con SDK .NET y runtime .NET 8 instalados:

```bash
dotnet run --project validation/RoomBreakers.Core.Tests/RoomBreakers.Core.Tests.csproj --configuration Release
dotnet run --project validation/RoomBreakers.Hardening.Tests/RoomBreakers.Hardening.Tests.csproj --configuration Release
dotnet run --project validation/RoomBreakers.HandInput.Tests/RoomBreakers.HandInput.Tests.csproj --configuration Release
dotnet run --project validation/RoomBreakers.Controls.Tests/RoomBreakers.Controls.Tests.csproj --configuration Release
```

Las cuatro suites compilan los archivos reales de `Runtime/Core` como .NET Standard 2.1. Se observaron **49 + 15 + 46 + 38 = 148 casos C# aprobados**. Evidencia, versiones y checks en ESTADO. **No equivalen a compilar Unity/Meta, probar sensores o certificar cumplimiento del concurso.**

Cuando exista un proyecto real con Unity y los tests del paquete configurados:

```bash
python3 tools/run_unity_checks.py
```

Ejecuta tests EditMode mediante el editor real y revisa sus resultados. No marca PASS si falta editor/proyecto o no hay resultados. Su comprobación de presencia mínima sigue centrada en los cuatro casos originales de transformación; verificar también los ocho de UnityInputFrameTests en Test Runner. No ejecuta PlayMode, Android ni el visor. Ver [Autochecks](docs/AUTOCHECKS.md).

## Disciplina de ejecución

Trabajar por hitos pequeños, registrar pruebas realmente ejecutadas y actualizar [ESTADO](docs/ESTADO.md). Prioridades: seguridad, control fiable, comprensión de la doble escala, juego completo y presentación. Una función propuesta nunca se describe como implementada.

Repositorio público: no subir correos privados, credenciales, IDs de dispositivos, enlaces de invitación, planos domésticos reales ni material de terceros sin autorización. No se ha elegido una licencia de redistribución para el código propio; esa decisión corresponde al titular.
