# ROOMBREAKERS — Tu habitación, en tus manos

> Manipula una maqueta de tu habitación y protege el refugio de Pip viendo las consecuencias a escala real.

**Estado: primer encuentro implementado en código; núcleo C# compilado y probado. Presentación Unity, entrada Meta y carga MRUK escritas pero aún sin import/compilación ni ejecución en Unity o Quest. No existe todavía APK ni entrega final validada.** El nombre es provisional; no hay garantía de premio.

## Primer encuentro: empieza aquí

Ya hay lógica de habitación y una sesión pequeña: LOAD ROOM → maqueta/portal/ruta → tutorial de captura → criaturas móviles → victoria o derrota → reinicio. Tres devoluciones cierran la grieta; tres daños al refugio terminan la partida. El espacio modifica la ruta y la colocación. El menú conserva pausa, confirmación y recolocación sin duplicar la simulación.

**[Primer encuentro y procedimiento completo](docs/PRIMER_ENCUENTRO.md)** explica qué se implementó, qué queda pendiente, fuentes y limitaciones.

Con el [paquete local](unity/Packages/com.zh.room-breakers/README.md) instalado en Unity:

- **`Tools → RoomBreakers → Open First Encounter (Desktop)`**: crea una escena de desarrollo con una habitación sintética y ratón. El editor genera la escena real. Abrir Game view y Play. Este recorrido está preparado, no observado todavía.
- **`Tools → RoomBreakers → Add Device Room Encounter`**: añade el encuentro a una escena XR ya configurada. Requiere MRUK e Interaction SDK de la familia v207 revisada, referencias de cámara/manos reales y passthrough configurado. No instala dependencias ni reemplaza la cámara.

La ruta real carga solo datos del dispositivo con consentimiento; no sustituye un fallo por una habitación falsa. La fuente sintética queda explícitamente identificada y deshabilitada en builds no Development.

Los laboratorios anteriores siguen disponibles: `Open Scale Lab` y `Add Meta Hands Harness`. Sirven para aislar problemas de input/transformación sin recorrer el juego.

## Documentación

| Necesitas | Documento |
| --- | --- |
| Contexto y decisiones | [Contexto](docs/CONTEXTO.md) |
| Producto y diseño completo | [Producto](docs/PRODUCTO.md) · [Juego](docs/JUEGO.md) |
| Trabajo de agentes | [AGENTS.md](AGENTS.md) · [Inicio](prompts/INICIO.md) |
| Una simulación y dos escalas | [Arquitectura](docs/ARQUITECTURA.md) |
| Entorno de Unity y Quest | [Entorno](docs/ENTORNO.md) |
| Primer encuentro y datos de habitación | [Primer encuentro](docs/PRIMER_ENCUENTRO.md) |
| Captura, watchdog y Meta IHand | [Manos](docs/MANOS.md) |
| Menú, pausa, reinicio y MOVE | [Controles](docs/CONTROLES.md) |
| Ejecución de comprobaciones | [Autochecks](docs/AUTOCHECKS.md) |
| Hitos y tareas | [Plan](docs/PLAN.md) · [Backlog](docs/BACKLOG.md) |
| Estrategia y requisitos | [Concurso](docs/CONCURSO.md) |
| Evidencia real y límites | [Estado](docs/ESTADO.md) · [Pruebas](docs/PRUEBAS.md) |
| Preparación del envío | [Entrega](docs/ENTREGA.md) |
| Referencias oficiales | [Fuentes](docs/FUENTES.md) |

## Arquitectura y alcance

Unity/C# y adaptadores Meta, offline. Una pose y un estado canónicos alimentan habitación y maqueta; no hay dos físicas. La mano ampliada es feedback, no autoridad de colisión. Solo se manipula contenido virtual. No se camina hasta portales ni se golpean muebles reales.

El encuentro actual usa un solo tipo de criatura. La propuesta final conserva dos enemigos, reflector, personaje guía y partidas de seis a ocho minutos; eso **no está completo** todavía. Quest 3/3S siguen siendo objetivos, no compatibilidad demostrada. La versión actual de Pip/Motes/mano está construida con primitivas, no arte final.

Sin backend, cuentas, pagos, multiplayer, IA generativa en ejecución, Terraform o eye tracking obligatorio. [Infraestructura](infra/README.md).

## Comprobaciones

Python 3.10+ para referencia, documentos y herramientas:

```bash
python3 tools/check_repo.py
python3 -m unittest discover -s tests -v
```

Con SDK .NET y runtime .NET 8, ejecutar las cinco suites del código real:

```bash
for project in validation/RoomBreakers.*.Tests/*.csproj; do
  dotnet run --project "$project" --configuration Release || exit 1
done
```

**183 casos C# aprobados** en la ejecución registrada en ESTADO: 49 del núcleo, 15 de robustez, 46 de manos, 38 de controles y 35 de habitación/encuentro. Se compilan los archivos reales de `Runtime/Core` como .NET Standard 2.1; se usan salas y entradas sintéticas. No se compilan UnityEngine/SDKs Meta ni se prueba hardware con este comando.

Para los 12 casos EditMode escritos, cuando exista el proyecto/editor real:

```bash
python3 tools/run_unity_checks.py
```

El verificador no da PASS si falta el editor o el resultado. Su requisito mínimo sigue centrado en cuatro tests de transformación; revisar también los ocho UnityInputFrameTests. No ejecuta Play Mode, APK ni Quest. [Alcance de autochecks](docs/AUTOCHECKS.md).

## Trabajo y privacidad

Actualizar ESTADO con evidencia real al terminar cada incremento. No marcar una API escrita como una integración probada, ni una prueba .NET como una sesión en visor. Mantener los errores y accesos faltantes explícitos sin delegar al usuario las modificaciones rutinarias.

Repositorio público: no subir credenciales, correos privados, números de serie, invitaciones secretas, geometría de habitaciones reales o assets sin autorización. No se ha elegido una licencia de redistribución para el código propio; esa decisión corresponde al titular.
