# ROOMBREAKERS — Tu habitación, en tus manos

> Manipula una maqueta de tu habitación y protege el refugio de Pip viendo las consecuencias a escala real.

**Estado: encuentro ampliado implementado en código, con Motes, Shell y reflector. Núcleo C# compilado y probado; presentación Unity, entrada Meta y carga MRUK escritas pero aún sin import/compilación ni ejecución en Unity o Quest. No existe APK ni entrega final validada.** El nombre es provisional; no hay garantía de premio.

## Demo: empieza aquí

LOAD ROOM → maqueta/portal/ruta → tutorial de captura → tres Motes devueltos → orientar reflector → devolver el pulso de Shell → capturarlo sin armadura → devolverlo al portal → victoria. Tres fallos que consuman la integridad del refugio terminan el encuentro. Pausa, reinicio confirmado y recolocación mantienen una sola autoridad de juego.

**[Shell y reflector](docs/SHELL_REFLECTOR.md)** describe el incremento vigente, cómo se usa, sus límites y la evidencia. **[Primer encuentro](docs/PRIMER_ENCUENTRO.md)** documenta la base de geometría/Motes del incremento 005.

Con el [paquete local](unity/Packages/com.zh.room-breakers/README.md) instalado en Unity:

- **`Tools → RoomBreakers → Open First Encounter (Desktop)`** crea una escena de desarrollo con sala sintética y ratón. En el reflector, mantener pulsado y usar Q/E para orientarlo, luego soltar. El editor genera la escena; abrir Game view y Play. Recorrido preparado en código, todavía no observado.
- **`Tools → RoomBreakers → Add Device Room Encounter`** añade el encuentro a un rig XR existente. Requiere MRUK/Interaction SDK de la familia v207 revisada, manos/cámara reales y passthrough configurado. No instala dependencias ni reemplaza la cámara.

El rig activa Shell por defecto. Desactivar `includeShell` conserva el recorrido básico de tres Motes como práctica; las pruebas específicas sí recorren el modo ampliado. El reflector actual gira sobre un soporte fijo: aún no se coloca libremente por la habitación.

La ruta real carga solo datos del dispositivo con consentimiento; no sustituye un fallo por una habitación falsa. También valida que exista un corredor de reflexión antes de empezar. La fuente sintética queda identificada y deshabilitada en builds no Development.

Los laboratorios `Open Scale Lab` y `Add Meta Hands Harness` permanecen para aislar problemas de input y transformaciones.

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
| Captura, watchdog y Meta IHand | [Manos](docs/MANOS.md) |
| Menú, pausa, reinicio y MOVE | [Controles](docs/CONTROLES.md) |
| Ejecución de comprobaciones | [Autochecks](docs/AUTOCHECKS.md) |
| Hitos y tareas | [Plan](docs/PLAN.md) · [Backlog](docs/BACKLOG.md) |
| Estrategia y requisitos | [Concurso](docs/CONCURSO.md) |
| Evidencia real y límites | [Estado](docs/ESTADO.md) · [Pruebas](docs/PRUEBAS.md) |
| Preparación del envío | [Entrega](docs/ENTREGA.md) |
| Referencias oficiales | [Fuentes](docs/FUENTES.md) |

## Arquitectura y alcance

Unity/C# y adaptadores Meta, offline. Un estado canónico alimenta ambas vistas, sin duplicar daño o física. Al pasar de Mote a reflector y Shell se reinicia la propiedad de input, no se arrastra una pinza vieja al objeto siguiente. La mano ampliada es feedback visual. Solo se manipula contenido virtual; no se camina a portales ni se golpean muebles.

El núcleo incluye dos tipos de desafío, pero no se ha calibrado la sesión final de seis a ocho minutos ni probado el juego en un dispositivo. Quest 3/3S son objetivos, no compatibilidad demostrada. Pip, criaturas, reflector y mano tienen arte procedural provisional; el pulso es un sistema acotado de un rebote, no física general.

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

**218 casos C# aprobados** en la ejecución registrada en ESTADO: 183 anteriores y 35 nuevos de Shell/reflector. Compilan los archivos reales de `Runtime/Core` como .NET Standard 2.1 y usan salas/entradas sintéticas. No compilan UnityEngine/SDKs Meta ni prueban hardware.

Para los 12 casos EditMode previos, cuando exista el proyecto/editor real:

```bash
python3 tools/run_unity_checks.py
```

El verificador no da PASS si falta el editor o el resultado. Su requisito mínimo sigue centrado en cuatro tests de transformación; revisar también los ocho UnityInputFrameTests. No ejecuta Play Mode, APK ni Quest. [Alcance de autochecks](docs/AUTOCHECKS.md).

## Trabajo y privacidad

Actualizar ESTADO con evidencia real al terminar cada incremento. No marcar una API escrita como una integración probada, ni una prueba .NET como una sesión en visor. Mantener los errores y accesos faltantes explícitos sin delegar al usuario modificaciones rutinarias.

Repositorio público: no subir credenciales, correos privados, números de serie, invitaciones secretas, geometría real de habitaciones o assets sin autorización. No se ha elegido una licencia de redistribución para el código propio; esa decisión corresponde al titular.
