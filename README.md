# ROOMBREAKERS — Tu habitación, en tus manos

> Manipula una maqueta de la habitación, protege a Pip y observa las consecuencias a escala real.

**Decisión vigente: desarrollo y demostración en computador, sin comprar ni depender de un visor físico.** Unity + Meta XR Simulator es la ruta elegida. Esto NO significa que el simulador ya esté instalado o ejecutando el proyecto. [Contexto simulator-first](docs/SIMULATOR_FIRST.md).

**Estado:** núcleo C# y herramientas probados; encuentro Motes → reflector → Shell, Pip y progreso local implementados en código. Unity/Meta/MRUK, las pruebas del motor, la integración XR Simulator y el APK siguen sin ejecución acreditada. No hay una entrega final validada ni garantía de premio. [Estado y evidencia](docs/ESTADO.md).

## Qué debe leer cualquier agente

[AGENTS.md](AGENTS.md) → [SIMULATOR_FIRST](docs/SIMULATOR_FIRST.md) → [ESTADO](docs/ESTADO.md) → [CONTEXTO](docs/CONTEXTO.md). Claude y Codex comparten la misma política. Las antiguas exigencias de conseguir/probar un visor quedan sustituidas; los registros históricos no son instrucciones vigentes de compra.

## Próximo paso: ejecutar software real

Con un Unity 6 instalado/activado, proyecto cerrado y `UNITY_EDITOR` apuntando a su ejecutable:

```bash
python3 tools/setup_unity_project.py --create
```

El preparador conecta el paquete local y encarga al editor la escena `Assets/RoomBreakersGenerated/DesktopEncounter.unity`; conserva dependencias y escenas existentes. Con Test Framework configurado, añadir `--verify` ejecuta 12 casos EditMode y cuatro PlayMode. **Ese proceso del motor todavía no está acreditado aquí.** [Arranque y recuperación](docs/ARRANQUE_UNITY.md).

Después corresponde conectar el runtime standalone de Meta, manos simuladas y datos de habitación. La escena desktop con ratón no es esa integración. El simulador no ejecuta una imagen Android: el APK para jueces se compila por separado. [Entorno](docs/ENTORNO.md) · [Plan](docs/PLAN.md) · [Backlog](docs/BACKLOG.md).

## Juego que se conserva

LOAD ROOM → maqueta, portal y ruta → tutorial de captura → tres Motes devueltos → orientar reflector → devolver el pulso de Shell → capturarlo vulnerable → devolverlo → resultado. Tres fallos consumen la integridad del refugio. Hay pausa, reinicio confirmado y recolocación, sin duplicar autoridad de juego.

Pip da ayuda contextual; una, tres y cinco victorias añaden piezas cosméticas al faro. El reflector actual gira en un soporte fijo; no tiene colocación libre. La mano ampliada y el arte son provisionales. No está calibrada la duración final de seis a ocho minutos.

Las rutas de editor existentes se mantienen:

- `Tools → RoomBreakers → Open First Encounter (Desktop)`: sala sintética y ratón; Q/E para orientar el reflector. Herramienta de desarrollo, no prueba de Meta XR.
- `Tools → RoomBreakers → Add Device Room Encounter`: adaptador escrito para rig Meta existente; hay que auditarlo al conectarlo a XR Simulator y conservarlo para la ruta Android de evaluación. No instala ni configura el runtime.

También siguen disponibles Open Scale Lab y Add Meta Hands Harness. No se quitaron protecciones de release: el mouse y las salas de laboratorio no pasan a ser sustitutos silenciosos del entorno del jugador.

## Documentación

| Tema | Documento |
| --- | --- |
| Restricción sin visor y fuentes actuales | [SIMULATOR_FIRST](docs/SIMULATOR_FIRST.md) |
| Contexto y antecedentes | [Contexto](docs/CONTEXTO.md) |
| Instrucciones de ejecución | [AGENTS](AGENTS.md) · [Claude](CLAUDE.md) · [Inicio](prompts/INICIO.md) · [Continuar](prompts/CONTINUAR.md) |
| Producto y reglas | [Producto](docs/PRODUCTO.md) · [Juego](docs/JUEGO.md) |
| Una simulación y dos escalas | [Arquitectura](docs/ARQUITECTURA.md) |
| Software e integración | [Entorno](docs/ENTORNO.md) · [Arranque Unity](docs/ARRANQUE_UNITY.md) |
| Habitación, Motes y combate | [Primer encuentro](docs/PRIMER_ENCUENTRO.md) · [Shell](docs/SHELL_REFLECTOR.md) |
| Pip y guardado | [Experiencia y progreso](docs/EXPERIENCIA_PROGRESO.md) |
| Manos, menú y seguridad | [Manos](docs/MANOS.md) · [Controles](docs/CONTROLES.md) · [UX](docs/UX_SEGURIDAD.md) |
| Trabajo y evidencia | [Plan](docs/PLAN.md) · [Backlog](docs/BACKLOG.md) · [Pruebas](docs/PRUEBAS.md) · [Autochecks](docs/AUTOCHECKS.md) · [Estado](docs/ESTADO.md) |
| Concurso y envío | [Concurso](docs/CONCURSO.md) · [Entrega](docs/ENTREGA.md) |
| Fuentes previas y decisiones | [Fuentes](docs/FUENTES.md) · [Decisiones](docs/DECISIONES.md) |

## Pruebas disponibles

```bash
python3 tools/check_repo.py
python3 -m unittest discover -s tests -v
```

Con SDK .NET y runtime .NET 8:

```bash
for project in validation/RoomBreakers.*.Tests/*.csproj; do
  dotnet run --project "$project" --configuration Release || exit 1
done
```

Con editor/proyecto reales y Test Framework:

```bash
python3 tools/run_unity_checks.py --platform EditMode
python3 tools/run_unity_checks.py --platform PlayMode
```

.NET/Python no compilan Unity/Meta. EditMode/PlayMode no prueban sensores. XR Simulator no acredita ejecución Android ni rendimiento Quest. La compilación APK no acredita instalación. [Protocolo de evidencia](docs/PRUEBAS.md).

## Entrega y privacidad

Nuestra ruta nativa conserva APK y acceso Competition para jueces, además de vídeo de gameplay desde XR Simulator; no se cambia a una entrega exclusiva de escritorio. [Procedimiento](docs/ENTREGA.md). Publicación y candidatura requieren autorización.

El juego es offline. El progreso guarda resultados, no manos, fotos, planos, UUID de anclajes, cuentas o números de serie. Práctica y release permanecen separados; la recuperación de dos checkpoints no garantiza conservar el último resultado ante cualquier fallo. Sin backend, pagos, telemetría, Terraform o eye tracking obligatorio.

Repositorio público: no subir secretos, invitaciones, claves, datos domésticos o assets sin autorización. No se cambia licencia ni visibilidad. Esta actualización de contexto no instala herramientas ni modifica el código de gameplay.
