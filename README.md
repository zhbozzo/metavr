# ROOMBREAKERS — Tu habitación, en tus manos

> Manipula una maqueta de tu habitación, protege a Pip y construye su refugio viendo las consecuencias a escala real.

**Estado: encuentro ampliado implementado en código con Motes, Shell, reflector, ayudas contextuales y progreso local. El núcleo se compila/prueba con .NET; presentación Unity y adaptadores Meta/MRUK todavía no se han compilado o ejecutado en Unity/Quest. No hay APK ni entrega final validada.** El nombre es provisional; no hay garantía de premio. [Evidencia actual](docs/ESTADO.md).

## Demo: empieza aquí

LOAD ROOM → maqueta/portal/ruta → tutorial de captura → tres Motes devueltos → orientar reflector → devolver el pulso de Shell → capturarlo vulnerable → devolverlo al portal → resultado. Tres fallos que consuman la integridad del refugio terminan el encuentro. Pausa, reinicio confirmado y recolocación mantienen una sola autoridad de juego.

**Nuevo: [Pip, feedback y progreso local](docs/EXPERIENCIA_PROGRESO.md).** Pip adapta sus instrucciones a lo que realmente está pasando y refuerza la ayuda cuando no avanzas. Arcos y marcas señalan el objetivo correspondiente en ambas escalas. Terminar una partida registra un solo resultado; una, tres y cinco victorias añaden piezas cosméticas al faro del refugio. No hay recompensas por velocidad ni progreso obtenido al perder el seguimiento.

**[Shell y reflector](docs/SHELL_REFLECTOR.md)** explica el combate y sus límites. **[Primer encuentro](docs/PRIMER_ENCUENTRO.md)** documenta la base de geometría/Motes.

Con el [paquete local](unity/Packages/com.zh.room-breakers/README.md) instalado en Unity:

- **`Tools → RoomBreakers → Open First Encounter (Desktop)`** crea una escena de desarrollo con sala sintética y ratón. En el reflector, mantener pulsado y usar Q/E para orientarlo, luego soltar. El editor genera la escena; abrir Game view y Play. Recorrido preparado en código, todavía no observado.
- **`Tools → RoomBreakers → Add Device Room Encounter`** añade el encuentro a un rig XR existente. Requiere MRUK/Interaction SDK de la familia v207 revisada, manos/cámara reales y passthrough configurado. No instala dependencias ni reemplaza la cámara.

El rig activa Shell por defecto. Desactivar `includeShell` conserva el recorrido básico de tres Motes como práctica. El reflector gira sobre un soporte fijo; no se coloca libremente por la habitación.

La ruta real carga datos del dispositivo con consentimiento; no sustituye un fallo por una habitación falsa. Valida también el corredor de reflexión antes de empezar. La fuente sintética queda identificada y deshabilitada en builds no Development. Los laboratorios `Open Scale Lab` y `Add Meta Hands Harness` permanecen para aislar problemas.

## Progreso y privacidad

Los resultados se guardan localmente con dos checkpoints pequeños y verificación de integridad. Si la copia nueva está dañada y la anterior sigue legible, se recupera esta última; no se garantiza recuperar el último resultado ante cualquier fallo del dispositivo. Archivos incompatibles o ambiguos se conservan sin sobrescribirlos. Si no se puede guardar, el juego continúa y avisa que el progreso pendiente está solo en memoria.

Editor, builds Development, salas sintéticas y modo básico usan progreso de práctica separado. Las victorias de prueba no desbloquean las piezas de la ruta release de dispositivo. Se guardan totales de resultados e identificadores locales recientes de intento: **no se guardan manos, fotos, planos, UUID de anclajes, cuentas ni números de serie**. Sin nube ni telemetría. [Contrato y límites de almacenamiento](docs/EXPERIENCIA_PROGRESO.md).

## Documentación

| Necesitas | Documento |
| --- | --- |
| Contexto y decisiones | [Contexto](docs/CONTEXTO.md) |
| Producto y diseño completo | [Producto](docs/PRODUCTO.md) · [Juego](docs/JUEGO.md) |
| Trabajo de agentes | [AGENTS.md](AGENTS.md) · [Inicio](prompts/INICIO.md) |
| Una simulación y dos escalas | [Arquitectura](docs/ARQUITECTURA.md) |
| Entorno de Unity y Quest | [Entorno](docs/ENTORNO.md) |
| Base de habitación y Motes | [Primer encuentro](docs/PRIMER_ENCUENTRO.md) |
| Combate y enseñanza del reflector | [Shell y reflector](docs/SHELL_REFLECTOR.md) |
| Ayudas, resultado y guardado | [Experiencia y progreso](docs/EXPERIENCIA_PROGRESO.md) |
| Captura, watchdog y Meta IHand | [Manos](docs/MANOS.md) |
| Menú, pausa, reinicio y MOVE | [Controles](docs/CONTROLES.md) |
| Ejecución de comprobaciones | [Autochecks](docs/AUTOCHECKS.md) |
| Hitos y tareas | [Plan](docs/PLAN.md) · [Backlog](docs/BACKLOG.md) |
| Estrategia y requisitos | [Concurso](docs/CONCURSO.md) |
| Evidencia real y límites | [Estado](docs/ESTADO.md) · [Pruebas](docs/PRUEBAS.md) |
| Preparación del envío | [Entrega](docs/ENTREGA.md) |
| Referencias oficiales | [Fuentes](docs/FUENTES.md) |

## Arquitectura y alcance

Unity/C# y adaptadores Meta, offline. Un estado canónico alimenta ambas vistas, sin duplicar daño o física. Al pasar de Mote a reflector y Shell se reinicia la propiedad de input; no se arrastra una pinza vieja al objeto siguiente. La mano ampliada y las ayudas son feedback visual, no autoridad. Solo se manipula contenido virtual; no se camina a portales ni se golpean muebles.

El núcleo incluye dos tipos de desafío, pero no se ha calibrado la sesión final de seis a ocho minutos ni probado el juego en dispositivo. Quest 3/3S son objetivos, no compatibilidad demostrada. Pip, criaturas, reflector y mano usan arte procedural provisional. Los pulsos y las rutas son sistemas acotados, no física/navegación universal. Las ayudas de Pip son reglas contextuales, no un chatbot.

Sin backend, cuentas, pagos, multiplayer, IA generativa en ejecución, Terraform o eye tracking obligatorio. [Infraestructura](infra/README.md).

## Comprobaciones

Python 3.10+ para referencia, documentos y herramientas:

```bash
python3 tools/check_repo.py
python3 -m unittest discover -s tests -v
```

Con SDK .NET y runtime .NET 8, ejecutar todas las suites:

```bash
for project in validation/RoomBreakers.*.Tests/*.csproj; do
  dotnet run --project "$project" --configuration Release || exit 1
done
```

Las suites compilan los archivos reales de `Runtime/Core` como .NET Standard 2.1. Usan salas/entradas sintéticas; Experience y ProgressRecovery también hacen operaciones reales en archivos temporales. **No compilan UnityEngine/SDKs Meta ni prueban almacenamiento Android, gráficos o sensores.** Los resultados y ejecuciones concretos están en [ESTADO](docs/ESTADO.md), no se deducen de que exista un archivo de pruebas.

Para los 12 casos EditMode previos, cuando exista el proyecto/editor real:

```bash
python3 tools/run_unity_checks.py
```

El verificador no da PASS si falta el editor o el resultado. Su requisito mínimo sigue centrado en cuatro tests de transformación; revisar también los ocho UnityInputFrameTests. No ejecuta Play Mode, APK ni Quest. [Alcance de autochecks](docs/AUTOCHECKS.md).

## Trabajo y privacidad del repositorio

Actualizar ESTADO con evidencia real al terminar cada incremento. No marcar una API escrita como una integración probada ni una prueba .NET como una sesión en visor. Mantener errores y accesos faltantes explícitos sin delegar al usuario modificaciones rutinarias.

Repositorio público: no subir credenciales, correos privados, números de serie, invitaciones secretas, geometría real de habitaciones o assets sin autorización. No se ha elegido una licencia de redistribución para el código propio; esa decisión corresponde al titular.
