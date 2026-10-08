# Encargo integral para Codex local — ROOMBREAKERS

Actualización de contexto: 7 de octubre de 2026. Repositorio: `zhbozzo/metavr`. Este es un encargo de ejecución en el computador del propietario, no una afirmación de instalación, compilación o envío ya realizados.

## 1. El mandato del propietario

Lorenzo quiere que Codex se encargue del proyecto completo en este computador: preparar el entorno, ejecutar Unity, corregir e integrar el juego, conectar el simulador XR, probar, mejorar la presentación, compilar el APK, producir el vídeo y los materiales, configurar el acceso de evaluación y realizar la entrega del concurso. No quiere revisar código rutinario, copiar archivos a mano ni dar una nueva orden de continuar después de cada pequeño cambio.

Trabaja hasta el siguiente resultado verificable y encadena tareas desbloqueadas dentro de la sesión activa. No termines con otra propuesta de trabajo cuando puedas ejecutarla. Si hay una dependencia humana real, identifica la acción mínima y continúa en las tareas independientes. No prometas ejecución en segundo plano después de finalizar la sesión. Antes de un límite de contexto, registra un punto de continuación completo.

**Restricción firme: nada de comprar, arrendar, pedir prestado ni depender de un visor físico.** El desarrollo y la demostración son íntegramente en computador. No vuelvas a convertir conseguir un Quest, conectar USB, Link o data forwarding en el próximo paso. El producto entregado sigue siendo una aplicación Meta VR: la restricción del equipo no autoriza sustituirla por un juego de ratón.

**No empieces de cero.** Conserva la idea, el código y las pruebas existentes; verifica lo que realmente encuentres y corrige lo que falle.

## 2. Autorización operativa y límites

Este encargo amplía la tarea anterior de preparar archivos: incluye la ejecución local y la entrega de ROOMBREAKERS al concurso indicado. Una vez cumplidas las comprobaciones y resueltas las declaraciones obligatorias, no necesitas otra autorización genérica para cada subida técnica. Puedes completar el envío con las herramientas y permisos realmente disponibles.

Dentro del alcance: cambios de código/documentación, pruebas, commits y PRs, instalación/configuración de dependencias gratuitas necesarias mediante fuentes oficiales y los permisos del sistema, compilación local, creación de materiales originales, preparación de firma local segura, subida del APK al canal Competition del proyecto correcto, publicación del vídeo de demo en la cuenta autorizada y envío del proyecto a esta competencia de Devpost. No publicar comercialmente en Meta Horizon Store ni en otros concursos por inferencia.

Lo siguiente sigue requiriendo intervención o aprobación específica cuando corresponda:

- Inicio de sesión, 2FA, CAPTCHA, aceptación de licencias/acuerdos y declaraciones legales o administrativas que no consten explícitamente confirmadas. Presentar el texto relevante y la acción necesaria; no marcar casillas por deducción.
- Elegir entre cuentas, organizaciones o proyectos ambiguos; confirmar firmante/remitente cuando no sea resoluble desde la sesión y datos autorizados. No inventar compañeros, correos, edad, elegibilidad, fecha de lanzamiento ni representación societaria.
- Compras, créditos adicionales, suscripciones, nube de pago, cambio de licencia/visibilidad del repositorio, actualización mayor del sistema operativo, sustitución de una firma existente, eliminación de datos o publicación ajena a esta entrega.
- Permisos del cliente Codex, instalación, acceso al navegador, grabación de pantalla o automatización de aplicaciones. No desactivar controles de seguridad, sandbox, Gatekeeper o aprobaciones para evitarlos.

Usa sesiones y almacenes de credenciales autorizados. Nunca pidas contraseñas, tokens, códigos de recuperación o claves en el chat/Git. No extraigas cookies ni abras perfiles personales completos para automatizar una subida. Si una herramienta no existe en tu sesión, no la des por disponible: descubre una integración soportada o identifica el paso humano exacto. Que ChatGPT tuviera GitHub conectado no transfiere automáticamente esas conexiones a Codex.

## 3. Confirmar que estás en ESTE computador

Antes de instalar o ejecutar, confirma la máquina y el workspace. La referencia de trabajo es macOS Apple Silicon; no asumas modelo, RAM, disco, versión del sistema ni rutas del usuario a partir del historial. No uses los límites del contenedor de ChatGPT como si fueran los de este Mac.

Si estás en una tarea cloud aislada y no tienes acceso local, dilo de inmediato y pide abrir este encargo en Codex local. No presentes un checkout remoto como una instalación en el computador del propietario. No contrates una máquina remota como sustituto.

Inspección inicial acotada al proyecto y herramientas:

```bash
pwd
git status --short
git remote -v
uname -s
uname -m
sw_vers                 # solo si el host es macOS
sysctl -n hw.memsize    # solo si el host es macOS
df -h .
python3 --version
command -v codex
command -v git
command -v dotnet
```

No vuelques variables de entorno, credenciales, números de serie o inventarios personales en logs públicos. Las rutas de máquina y diagnósticos detallados se guardan localmente.

Si ya existe `metavr`, úsalo y comprueba que `origin` corresponde al repositorio correcto. Lee las instrucciones locales, incluidas las de directorios superiores. Haz fetch y revisa divergencia antes de integrar remoto; conserva cambios sin commit y trabajo de otras ramas. Nada de `reset --hard`, limpieza destructiva o force-push. Si no existe, clona en un directorio de trabajo autorizado y vacío:

```bash
git clone https://github.com/zhbozzo/metavr.git
cd metavr
```

No recorras todo el disco buscando información personal. Limita búsquedas a la carpeta seleccionada, ubicaciones estándar de Unity y archivos pertinentes. Si falta acceso, pide solo ese permiso.

## 4. Lectura y estado recibido

Lee primero [AGENTS](../AGENTS.md), [SIMULATOR_FIRST](../docs/SIMULATOR_FIRST.md), [ESTADO](../docs/ESTADO.md) y [CONTEXTO](../docs/CONTEXTO.md). Después usa [BACKLOG](../docs/BACKLOG.md), [ARRANQUE_UNITY](../docs/ARRANQUE_UNITY.md), [ARQUITECTURA](../docs/ARQUITECTURA.md), [JUEGO](../docs/JUEGO.md), [UX_SEGURIDAD](../docs/UX_SEGURIDAD.md) y la especificación del módulo que toques. Para la fase final lee [ENTREGA](../docs/ENTREGA.md) y [CONCURSO](../docs/CONCURSO.md).

Base remota inspeccionada para este traspaso: `a142054d9b6c48dadefe96c33d4cf4eedda07df7`, posterior al PR #9. Es una referencia histórica, NO una orden de volver a ese commit o ignorar cambios posteriores. Determina el HEAD real al empezar.

Estado recibido, sujeto a tu auditoría:

| Capa | Qué existe | Evidencia recibida |
| --- | --- | --- |
| Dominio C# | Doble escala, captura, controles, geometría, encuentro, reflector/Shell, guía y progreso | 265 casos aprobados previamente en CI .NET; no son pruebas Unity |
| Herramientas Python | Referencia, comprobación documental, preparador y verificador Unity | 68 casos aprobados previamente; volver a ejecutar |
| Paquete Unity | Runtime, vistas, menús de editor y adaptadores Meta/MRUK escritos | Aún sin compilación/ejecución acreditada del motor |
| Pruebas Unity | 12 EditMode y 4 PlayMode escritas | NOT RUN en el traspaso |
| XR Simulator | Ruta y tareas SIM-001–006 documentadas | Integración pendiente, no runtime funcionando |
| Android | Objetivo APK con manos y entorno del dispositivo | Ningún APK verificado recibido |
| Entrega | Borradores ingleses e instrucciones | Sin vídeo/build/candidatura enviados por este traspaso |

Los números son historia verificable en [PR #8](https://github.com/zhbozzo/metavr/pull/8) y [PR #9](https://github.com/zhbozzo/metavr/pull/9), no una obligación de mantener un número artificial. Añade, corrige y ejecuta pruebas útiles; nunca elimines una regresión para mostrar verde.

Mapa para orientarte, no contrato de APIs nuevas:

- `unity/Packages/com.zh.room-breakers/Runtime/Core`: autoridad de simulación. Revisar `ScaleSession`, `FirstEncounter`, `ShellDuel`, `RoomGeometry`, `EncounterCoach` y `LocalProgress`.
- `Runtime/UnityInput` dentro del paquete: integración y presentación, entre ellas `FirstEncounterRig`, `FirstEncounterView`, `PipGuidanceView`, `ShellDuelView`, fuentes de manos/habitación y controles. Auditar la ubicación y APIs efectivas de los adaptadores Meta en el árbol, no inventarlas.
- `Editor`: menús y generación de escena mediante el editor. `Tests/Editor` y `Tests/PlayMode`: suites que necesitan Unity real.
- `validation`: ejecutables .NET que compilan el núcleo real. `tools`: bootstrap, comprobaciones y scripts. `docs`: diseño y evidencia. `submission`: borradores, no candidatura terminada.

## 5. El producto que debes terminar

ROOMBREAKERS: «Tu habitación, en tus manos». Juego MR individual y sentado. Una maqueta simplificada y la representación grande de la habitación comparten una sola simulación. Agarrar una criatura pequeña produce una intervención ampliada, visible en la habitación. Se protege el refugio del pequeño personaje Pip.

Recorrido existente: cargar habitación → generar configuración válida y maqueta → primera captura sin presión → devolver tres Motes → orientar reflector → devolver un pulso de Shell → capturarlo vulnerable → devolverlo al portal → victoria o derrota → reinicio. Tres daños agotan la integridad del refugio. Pip enseña según el estado; las victorias añaden piezas cosméticas a su faro. El guardado local conserva resultados, no una partida en curso.

La hipótesis competitiva es una interacción espacial inmediatamente comprensible, no una lista de SDKs. Prioriza un primer minuto excelente, un recorrido completo, señales claras y recuperación fiable. El objetivo orientativo de sesión es seis a ocho minutos, no una duración ya medida. No alargues artificialmente con esperas ni agregues niveles para alcanzar ese número.

Conserva estas decisiones:

1. Una autoridad canónica, dos vistas; sin daño, movimiento autónomo o físicas duplicadas. La mano ampliada y efectos son presentación.
2. Manos de extremo a extremo, incluyendo inicio, menús, ajustes, pausa y reinicio. Las manos simuladas deben atravesar la ruta XR/SDK, no un atajo de mouse que modifique el estado.
3. Movimientos cortos y pausables, sin golpes o lanzamientos fuertes; nada exige tocar una pared física. No mover automáticamente la cámara para dar espectáculo.
4. Reflector orientable sobre soporte fijo, un pulso/un rebote. No expandir a física universal sin necesidad demostrada.
5. Geometría conservadora con recuperación cuando falta una solución. Nunca sustituir un fallo de habitación real por un fixture sin avisar.
6. Sin backend, cuentas del juego, pagos, multiplayer, IA generativa en ejecución, Terraform ni seguimiento ocular obligatorio. No añadir features solo para perseguir premios.

Consulta precedentes y alcance ya discutidos en los documentos. No prometas originalidad absoluta, premio, compatibilidad física, confort o fps no medidos.

## 6. Fase A — abrir y ejecutar Unity de verdad

Audita lo instalado. Reutiliza una combinación compatible en vez de descargar varias versiones. Comprueba fuentes oficiales para Unity, módulos Android, Test Framework, Meta Core/Interaction/MRUK, OpenXR y XR Simulator. Fija versiones comprobadas y conserva lockfiles y metadata generados por herramientas reales. No uses números del chat o ejemplos de tests como recomendación de versión.

Instala solamente lo necesario, sin cambiar herramientas globales ajenas o saturar disco/RAM. Presenta tamaño real del instalador cuando esté disponible y revisa margen antes de descargas grandes. No borres archivos personales para liberar espacio. Si aparecen licencia, login o permisos del sistema, deja ese paso al usuario y conserva el avance.

Desde la raíz del repo:

```bash
python3 tools/check_repo.py
python3 -m unittest discover -s tests -v
# Cuando dotnet y su runtime requerido estén disponibles:
for project in validation/RoomBreakers.*.Tests/*.csproj; do
  dotnet run --project "$project" --configuration Release || exit 1
done
```

Identifica el ejecutable Unity realmente instalado; con el proyecto cerrado:

```bash
export UNITY_EDITOR="/Applications/Unity/Hub/Editor/VERSION_REAL/Unity.app/Contents/MacOS/Unity"
python3 tools/setup_unity_project.py --create
python3 tools/run_unity_checks.py --platform EditMode
python3 tools/run_unity_checks.py --platform PlayMode
```

Reemplaza VERSION_REAL tras inspeccionar el equipo. `--create --verify` combina preparación y pruebas si Test Framework ya está disponible. Lee `--help` antes de añadir opciones. El destino previsto es `unity/RoomBreakers`; no sobrescribas otra carpeta no vacía. Si el bootstrap falla, corrígelo a partir del error real y conserva sus protecciones. No fabriques YAML/GUIDs ni un comprobante de éxito.

Abre `Assets/RoomBreakersGenerated/DesktopEncounter.unity` y ejecuta Play. Los menús de laboratorio existentes permiten aislar fallos. Resuelve errores de compilación, assembly definitions, referencias, input, shaders, fuente, audio y ciclo de vida antes de agregar gameplay. Inspecciona la pantalla, no solo Console. Captura evidencia exclusivamente de la app y sin datos privados.

**Aceptación A:** proyecto real importado; suites Unity ejecutadas sin ocultar fallos; escena observada; instrucciones de arranque reproducibles. No confundir pasar tests del script preparador con ejecutar Unity.

## 7. Fase B — integrar Meta XR Simulator sin hardware

Sigue la documentación vigente enlazada al final. El objetivo local es Unity con el runtime de Meta y su entrada de manos simuladas. Descubre las herramientas reales de tu entorno: CLI oficial, Meta XR Operator/MCP, scripting de editor y automatización visual autorizada. No presupongas sus nombres de comandos o disponibilidad; comprueba ayuda, esquemas y versiones. Prefiere acciones reproducibles y vinculadas a este proyecto.

No configures todos los SDKs a la vez. Haz funcionar un rig y una cámara, una fuente de manos y una habitación de prueba; después conecta el encuentro existente. Conserva componentes reales del SDK en la ruta del jugador. No reemplaces un error de importación por clases falsas con los mismos nombres.

En la documentación Meta consultada, XR Simulator se distribuye como aplicación independiente y modela APIs OpenXR; no contiene Android. La ruta macOS tiene requisitos específicos de arquitectura, plugin y API gráfica. Environment Depth del simulador se documenta para Windows: no lo hagas requisito de este Mac. Verifica las versiones instaladas y perfiles disponibles en lugar de asumir que la palabra 'Meta' garantiza compatibilidad.

Conecta manos del runtime y fuentes de entorno compatibles a la misma autoridad del juego. Etiqueta origen de datos: fixture desktop, runtime simulado o dispositivo. MRUK/Prefab/JSON son rutas a auditar e integrar; su mención en documentación no significa que `MetaRoomSource` ya las implemente. Conserva permisos/recuperación y la separación práctica/release.

**Aceptación B:** el juego, no solo un ejemplo del SDK, se ejecuta en XR Simulator y permite completar inicio → Motes → reflector → Shell → resultado → reinicio con manos simuladas. Documenta perfil, runtime, gráficos, entrada y fixture. Sin mouse de gameplay obligatorio en Android. Si una limitación de macOS bloquea una capacidad, prueba una alternativa de software soportada sin cambiar el motor o reducir requisitos a escondidas; registra el bloqueo específico si persiste.

## 8. Fase C — calidad del juego y regresiones

Después del primer recorrido observado, prioriza los problemas visibles. No consumas la sesión añadiendo documentos o miles de tests de una función que ya está bien mientras el juego no arranca.

Trabaja en comprensión de escalas, colocación y legibilidad, selección, feedback de la captura y retorno, continuidad entre etapas, señales redundantes de peligro, reacciones de Pip, transiciones, sonido y coherencia de materiales. Optimiza la representación provisional antes de encargar arte enorme. No compartir fuentes comerciales, música o modelos sin derechos; documenta procedencia y permisos. Una licencia de uso no implica derecho a subir el asset completo al Git público.

Completa preferencias necesarias con alcance pequeño: tamaño/altura/distancia de maqueta, texto legible, efectos moderables y recorrido secuencial de una mano. No impongas gestos complejos. Mantén el ajuste de la maqueta en pausa y verifica que nunca desplace la habitación canónica.

Ejecuta la matriz de SIMULATOR_FIRST: salas vacías, estrechas, obstruidas, rotadas, cóncavas y sin corredor; datos incompletos, cambio de habitación durante captura y recuperación. Añade entradas antiguas/repetidas/ausentes, pérdida de foco, pinza cerrada al arrancar, izquierda/derecha, dos manos, reinicio y fallo de guardado. Un rechazo útil puede ser el resultado esperado de un fixture imposible.

Comprueba repetición de sesiones, fugas de objetos/materiales/audio, allocations y coste de dos vistas. Mide rendimiento del host como rendimiento del host: no convertirlo en cifras de Quest. Si grabas replay, distingue entrada reproducida sobre el juego de un guion que teletransporte entidades o active victoria. El demo público no debe depender de atajos de test ni autoplay oculto.

**Aceptación C:** recorrido completo, recuperación y guardado probados en software; material visual inspeccionado; ningún bloqueo conocido del alcance; evidencia versionada y límites claros. Hardware permanece no validado y fuera del plan, no una tarea de compra pendiente.

## 9. Fase D — candidato Android real

Genera pronto un primer APK para descubrir errores de plataforma antes de gastar todo el tiempo en acabado. Luego congela un candidato reproducible. Revisa escena XR de arranque, arquitectura, backend de scripting, stripping, shaders incluidos, manifest/permisos y dependencias contra la documentación y requisitos vigentes. No asumas un target SDK, permiso o flag por memoria.

Mantén fuera del build de evaluación las herramientas del editor, proveedores de ratón, fixtures activados por defecto y operadores de depuración. Las mismas reglas deben funcionar con manos y entorno del destinatario. No conviertas una sala fallida en éxito ficticio. No cambies visibilidad o crea una app de Meta duplicada sin comprobar la existente; ZH Spatial AI / ZH Development son nombres de referencia del registro inicial, no IDs verificados.

Usa bundle identifier y firma persistentes del proyecto. No reemplaces claves existentes. Si no hay firma, prepara una nueva localmente con herramientas seguras, sin exponer contraseñas en comandos, logs, Git o chat; confirma las decisiones no resolubles del titular y el resguardo de la clave. No inventes App ID.

Conserva el APK fuera del Git del código y registra nombre, tamaño medido, versión, commit fuente, Unity/SDKs, configuración y SHA-256. Audita el artefacto y la información que devuelva el dashboard; no digas 'se instaló' por haberlo compilado o subido.

**Aceptación D:** archivo Android real y comprobaciones de build/configuración completas. La ejecución física sigue NO VALIDADA. Un vídeo de XR Simulator no demuestra que el APK arrancó.

## 10. Fase E — materiales y entrega efectiva

Reconsulta reglas, requisitos, campos y plazos antes de esta fase. Usa el conector correspondiente cuando esté disponible; si falla su autenticación, pide reconectar o utiliza un navegador con sesión explícitamente autorizado. No simules que un texto escrito en Git equivale a rellenar el formulario.

Referencia pública consultada para este encargo: Unity requiere APK en un canal llamado Competition e invitación de acceso; además vídeo público de menos de tres minutos en YouTube/Vimeo, que puede mostrar XR Simulator, y formulario completo. El cierre visible consultado es 18 de noviembre de 2026, 12:00 PST / 20:00 UTC. Verifica de nuevo el instante y la zona antes de actuar; no dependas del calendario histórico del repositorio. [D1, D2]

Preparación original propuesta:

- Vídeo objetivo de 120–150 segundos, sin presentarlo como requisito oficial. Abrir con captura de miniatura y mano ampliada; mostrar cómo la geometría cambia una decisión; enseñar reflector/Shell; finalizar con Pip, resultado y reinicio. Audio/textos claros en inglés. Sin vídeo generado por IA para fingir gameplay.
- Capturar el juego real en el runtime y editar únicamente su presentación. Mantener correspondencia con el código del APK; documentar diferencias host/Android. Revisar el archivo exportado, duración, imagen, audio y reproducción pública.
- Preparar título, tagline, descripción, capacidades e interacción con manos según campos reales. Track previsto: Gaming. División New solo después de comprobar la ventana y procedencia. No inventar declaraciones personales ni fechas de lanzamiento; agrupa lo no resoluble en una petición al usuario.
- Completar los borradores `submission/DRAFT_EN.md` y `submission/JUDGE_INSTRUCTIONS_EN.md` con lo observado. Añadir capturas originales del runtime y procedencia de assets. Nada de prometer que Meta patrocina el juego.

Una vez ejecutada la simulación, disclosure previsto: 'Developed and demonstrated using Meta XR Simulator. Physical-headset validation has not been performed by the team.' No usar ni siquiera esa primera frase si solo corrió el laboratorio desktop.

Publica exclusivamente los artefactos de esta entrega en las cuentas autorizadas. Para Meta: comprobar organización/app correcta, subir APK al canal Competition y verificar procesamiento/estado y acceso utilizable. La invitación no se commitea ni se publica en el vídeo. Para vídeo: comprobar cuenta/canal y ausencia de datos privados. Para Devpost: inspeccionar proyectos existentes, actualizar el correcto en vez de duplicarlo, consultar campos, resolver acuerdos y enviar.

Si la autenticación o un acuerdo necesita al usuario, deja terminados el APK, vídeo, textos y formulario preparado hasta el punto posible, informa el bloqueo exacto y reanuda después. No redefinas 'entregado' como 'hay un borrador'. Si la plataforma ya no permite envío, conserva los artefactos y reporta esa restricción, sin falsificar fecha o cambiar a otro concurso.

**Aceptación E:** confirmación real de envío, no solo Draft; proyecto correcto, enlace público y acceso de jueces, versión exacta del APK y vídeo, fecha y evidencia saneada. Tras el cierre, conserva la versión evaluada y accesibilidad según reglas. No alteres el candidato fuera del periodo permitido.

## 11. Estado, autonomía y cierre de sesión

Mantén una sola fuente de estado en `docs/ESTADO.md`. Usa PASS / FAIL / BLOCKED / NOT RUN por nivel. No sobrescribas historia de pruebas ni marques hardware aprobado. Separa build, procesamiento del canal, accesibilidad y submission: son resultados distintos.

Actualiza al terminar cada bloque: commit/archivos; comandos y versiones; resultado observado; problemas pendientes ordenados; la siguiente acción ejecutable. Los logs extensos, rutas locales, vídeo sin sanear, llaves y enlaces privados permanecen fuera del Git público. Documenta decisiones en DECISIONES solo cuando cambie un contrato importante.

Para retomar una sesión deja: rama y cambios pendientes, ruta del proyecto/escena en nota local privada, editor/runtime utilizados, último error exacto saneado, comando de continuación, artefactos reales y pasos humanos estrictamente necesarios. No ejecutes Unity concurrentemente sobre el mismo proyecto desde varios agentes. Un segundo agente puede revisar código o pruebas, sin hacerse pasar por una segunda validación del motor.

Informe final al propietario: cómo abrir/jugar en este computador; dónde están APK, vídeo y materiales; pruebas efectivas; enlace y estado real del envío; limitaciones restantes. No te limites a contar líneas de código o PRs.

## 12. Primer resultado que esperamos de ti

Empieza ahora con la auditoría local y el bootstrap existente. Tu primer hito es **abrir el proyecto en Unity, resolver sus errores y observar el encuentro**, no otra lluvia de ideas. Después sigue SIM-002–006 y las fases anteriores hasta entregar, dentro de la autorización y los accesos disponibles. La ausencia de hardware es una restricción de diseño ya resuelta; la falta de software es algo que debes diagnosticar y, cuando esté permitido, instalar/configurar.

## Fuentes verificables y alcance de este documento

- [O1 — Codex local y clientes disponibles](https://help.openai.com/en/articles/11369540-using-codex-with-your-chatgpt-plan). Los permisos y accesos son propios de cada cliente; no se conceden al escribir este archivo.
- [O2 — Instrucciones AGENTS.md](https://developers.openai.com/codex/guides/agents-md/). Este encargo se enlaza desde AGENTS, no se asume memoria automática de la conversación.
- [M1 — Meta XR Simulator](https://developers.meta.com/vr/documentation/unity/xrsim-intro/).
- [M2 — Integración Unity y requisitos del host](https://developers.meta.com/vr/documentation/unity/unity-simulate-xrsim/).
- [D1 — Concurso y What to Submit](https://start-developer-competition-26.devpost.com/).
- [D2 — Reglas oficiales](https://start-developer-competition-26.devpost.com/rules).

En la sesión que redactó este encargo, el conector Devpost devolvió 401 y solicitó reautenticación; se consultó la información pública por web. Eso no demuestra que Codex comparta el fallo o una sesión válida: debe comprobar su propio acceso. No se accedió al computador del usuario, instaló software o realizó la candidatura al redactar el handoff.
