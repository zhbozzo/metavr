# ROOMBREAKERS — Tu habitación, en tus manos

> Un juego de realidad mixta en el que manipulas una maqueta de tu habitación y ves las consecuencias a escala real, sin levantarte de la silla.

**Estado: primer núcleo C# implementado y probado en .NET; laboratorio de Unity escrito, pendiente de import y ejecución en el editor. No existe todavía un APK ni una app Quest validada.** El nombre es provisional. No existe garantía de ganar el concurso.

## Código: empieza aquí

El [paquete Scale Lab](unity/Packages/com.zh.room-breakers/README.md) contiene el código original del núcleo, un laboratorio sintético con ratón y tests para el editor. No hace falta cambiar de motor: se instala como paquete local en un proyecto Unity real. La combinación de SDKs Meta sigue pendiente de validar.

En Unity 6: Package Manager → Install package from disk → seleccionar `unity/Packages/com.zh.room-breakers/package.json` → Tools → RoomBreakers → Open Scale Lab → Play. Usar Game view. **Este recorrido está preparado en código; su smoke test real en Unity está pendiente.** El [incremento 001](docs/IMPLEMENTACION_001.md) explica el alcance inicial. El [estado real](docs/ESTADO.md) registra los incrementos y pruebas posteriores.

## Documentación

| Necesitas | Documento |
| --- | --- |
| Contexto y decisiones previas | [Contexto](docs/CONTEXTO.md) |
| Qué construir y qué no construir | [Producto](docs/PRODUCTO.md) y [diseño del juego](docs/JUEGO.md) |
| Instrucciones para agentes de código | [AGENTS.md](AGENTS.md) y [prompt de inicio](prompts/INICIO.md) |
| Arquitectura y transformación entre escalas | [Arquitectura](docs/ARQUITECTURA.md) |
| Configurar Unity y probar en Quest | [Entorno](docs/ENTORNO.md) |
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
```

Las dos suites compilan los mismos archivos de `Runtime/Core` como .NET Standard 2.1. Se observaron 49 + 15 tests C# y 36 Python aprobados; evidencia y alcance en ESTADO. **No equivalen a compilar Unity, probar un Quest o certificar cumplimiento del concurso.**

Cuando exista un proyecto real con Unity y los tests del paquete configurados:

```bash
python3 tools/run_unity_checks.py
```

Ejecuta los tests EditMode del paquete mediante el editor real y revisa sus resultados. No marca PASS si falta el editor/proyecto, si no hay resultados o si se omitieron tests. No ejecuta PlayMode, Android ni el visor. Ver [Autochecks](docs/AUTOCHECKS.md).

## Disciplina de ejecución

Trabajar por hitos pequeños, registrar pruebas realmente ejecutadas y actualizar [ESTADO](docs/ESTADO.md). Prioridades: seguridad, control fiable, comprensión de la doble escala, juego completo y presentación. Una función propuesta nunca se describe como implementada.

Repositorio público: no subir correos privados, credenciales, IDs de dispositivos, enlaces de invitación, planos domésticos reales ni material de terceros sin autorización. No se ha elegido una licencia de redistribución para el código propio; esa decisión corresponde al titular.
