# ROOMBREAKERS — Tu habitación, en tus manos

> Un juego de realidad mixta en el que manipulas una maqueta de tu habitación y ves las consecuencias a escala real, sin levantarte de la silla.

**Estado: preproducción. Este repositorio contiene la especificación y la base de trabajo; todavía no contiene un juego ejecutable ni un APK probado.** El nombre es provisional. No existe garantía de ganar el concurso.

## Empieza aquí

| Necesitas | Documento |
| --- | --- |
| Contexto y decisiones previas | [Contexto](docs/CONTEXTO.md) |
| Qué construir y qué no construir | [Producto](docs/PRODUCTO.md) y [diseño del juego](docs/JUEGO.md) |
| Instrucciones para agentes de código | [AGENTS.md](AGENTS.md) y [prompt de inicio](prompts/INICIO.md) |
| Arquitectura y transformación entre escalas | [Arquitectura](docs/ARQUITECTURA.md) |
| Configurar Unity y probar en Quest | [Entorno](docs/ENTORNO.md) |
| Orden de implementación | [Plan](docs/PLAN.md) y [backlog](docs/BACKLOG.md) |
| Requisitos y estrategia de concurso | [Concurso](docs/CONCURSO.md) |
| Evidencia, pruebas y limitaciones | [Pruebas](docs/PRUEBAS.md) y [estado real](docs/ESTADO.md) |
| Preparar build, vídeo y envío | [Entrega](docs/ENTREGA.md) |
| Fuentes oficiales y discrepancias | [Fuentes](docs/FUENTES.md) |

## La decisión central

Una sola simulación del juego en coordenadas de la habitación; dos representaciones sincronizadas: habitación y maqueta. Agarrar una miniatura modifica esa única simulación. La mano gigante es una representación visual, no una segunda simulación física.

La primera prueba no es un juego completo: **seleccionar una miniatura, moverla y reconocer inmediatamente la consecuencia en el espacio real**, con una interacción cómoda y fiable. Si eso no funciona en un visor, no se añade contenido para ocultar el problema.

## Alcance elegido

Unity + C# + herramientas XR/MR de Meta, con versiones compatibles por verificar y fijar durante el primer hito. Quest 3/3S son objetivos de prueba, no compatibilidad ya demostrada. Partida objetivo de 6–8 minutos; un sector frontal del entorno; dos enemigos; una herramienta reflectante; un personaje guía. Todo el recorrido de la app debe funcionar con manos.

No se incluye backend, login, pagos, multiplayer, IA generativa en ejecución, Terraform ni seguimiento ocular obligatorio. [Motivo de infraestructura](infra/README.md).

## Comprobaciones disponibles ahora

Requieren Python 3.10 o superior y solo su biblioteca estándar:

```bash
python3 tools/check_repo.py
python3 -m unittest discover -s tests -v
```

Estas comprobaciones validan la base documental y un modelo matemático de referencia; **no compilan Unity, no prueban un Quest y no certifican cumplimiento del concurso**. El proyecto Unity se creará siguiendo [Entorno](docs/ENTORNO.md), no fabricando archivos de un SDK aún no comprobado.

## Disciplina de ejecución

Trabajar por hitos pequeños, registrar pruebas realmente ejecutadas y actualizar [ESTADO](docs/ESTADO.md) al terminar cada sesión. Las prioridades son seguridad, control fiable, comprensión de la doble escala, juego completo y presentación, en ese orden. Una función propuesta nunca se describe como implementada.

El repositorio es público: no subir correos privados, credenciales, IDs de dispositivos, enlaces de invitación, planos domésticos reales ni material de terceros sin autorización. No se ha elegido una licencia de redistribución para el código propio; esa decisión corresponde al titular.
