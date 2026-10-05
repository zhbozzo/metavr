# Comprobación automática, no ausencia de comprobación

Actualización: incremento 008, 5 de octubre de 2026. La evidencia ejecutada está en [ESTADO](ESTADO.md). Tener archivos de prueba no significa que esas pruebas hayan corrido.

## Núcleo y herramientas

Los PRs configurados ejecutan comprobaciones Python y las suites C# del dominio:

```bash
python3 tools/check_repo.py
python3 -m unittest discover -s tests -v
for project in validation/RoomBreakers.*.Tests/*.csproj; do
  dotnet run --project "$project" --configuration Release || exit 1
done
```

El C# compila `Runtime/Core` como .NET Standard 2.1. Manos/salas son datos sintéticos; las suites de persistencia escriben archivos temporales reales, no almacenamiento Android. Las iteraciones aleatorias dentro de un caso no son usuarios ni pruebas físicas. No se compilan UnityEngine o el SDK Meta en estos comandos.

## Preparar proyecto, paquete y escena mediante Unity

```bash
python3 tools/setup_unity_project.py --create
```

Requiere un editor Unity 6 instalado/activado, `UNITY_EDITOR` o `--unity` apuntando al ejecutable, y proyecto cerrado. Deja que Unity genere un proyecto faltante en un destino vacío, conecta el paquete local y guarda una escena nueva mediante el editor. No elige otro editor, elimina carpetas no vacías, sobrescribe una escena existente o instala SDKs Meta. El comprobante de preparación es específico de la ejecución e incluye un nonce; un exit code cero no basta.

Con Test Framework instalado:

```bash
python3 tools/setup_unity_project.py --create --verify
```

El modificador `--verify` añade las dos suites reales del motor. Si falta Test Framework se solicita una versión explícita o instalación en Package Manager, sin usar latest ni cambiar otro pin. [Procedimiento, límites y fuentes](ARRANQUE_UNITY.md).

## Ejecutar Unity por separado

```bash
python3 tools/run_unity_checks.py --platform EditMode
python3 tools/run_unity_checks.py --platform PlayMode
```

El script lee ProjectVersion y verifica la versión del ejecutable con `-version`, incluso cuando el usuario proporciona una ruta explícita. Rechaza un proyecto abierto, una versión distinta o desconocida y resultados antiguos. No cambia la versión del proyecto.

**EditMode** selecciona `RoomBreakers.Core.UnityTests` y exige los cuatro casos originales de transformaciones **y los ocho UnityInputFrameTests**. El mínimo anterior de cuatro casos ya no basta. Pruebas omitidas, fallidas, inconclusas o identificadores duplicados no pasan.

**PlayMode** selecciona `RoomBreakers.Integration.PlayTests` y exige los cuatro casos de integración: arranque con pinza, ambas representaciones, recorrido Motes/Shell/victoria/reinicio e interrupción/invalidez de habitación. Usa el FirstEncounterRig real con fuentes de prueba sintéticas, escenas temporales y progreso aislado en memoria. No afirma que el ratón o una mano sintética equivalgan a una prueba de sensores.

No se usa `-quit` durante tests, porque puede detenerlos antes de completar. PlayMode conserva un dispositivo gráfico; el script está pensado para una estación de desarrollo con entorno gráfico válido. No se rebaja a .NET cuando Unity no puede arrancar.

Salida del verificador: PASS/código 0, FAIL/1, BLOCKED/2. El preparador sin `--verify` usa PREPARED/0, que significa solamente preparación, no pruebas del motor aprobadas. Sus logs y comprobantes se guardan bajo `.validation-local/`, sin publicarlos automáticamente. Pueden contener rutas del equipo; inspeccionarlos antes de compartir.

## Pruebas del propio verificador

Los tests Python de las herramientas utilizan XML y respuestas de procesos sintéticos, además de operaciones de archivos temporales reales. No ejecutan Unity. El caso de regresión de informe incompleto comprueba que cuatro tests correctos ya no ocultan la ausencia de los otros ocho. Otro caso verifica que un XML previo no autorice un lanzamiento nuevo que no produjo resultados.

La prueba de salida de proceso fallida escribe un XML nuevo durante el proceso simulado; así sigue probando esa condición, no solo la protección contra XML antiguo. Los escenarios de preparación cubren rutas con espacios, manifest cambiado, paquetes duplicados, copias de respaldo, no-op sin reescritura, editor incorrecto, destino no vacío y comprobantes que no corresponden a la ejecución.

## Límites

No se han ejecutado todavía las 12 pruebas EditMode ni las cuatro nuevas PlayMode en un editor real en este entorno. No hay APK, soporte Quest certificado, prueba de comodidad, gráficos evaluados o perfil de rendimiento. El código del watchdog de manos sí existe en el dominio, pero no reemplaza el ensayo del proveedor Meta real.

## Referencias de interfaz

- [Unity 6: argumentos de editor](https://docs.unity3d.com/6000.0/Documentation/Manual/EditorCommandLineArguments.html).
- [Test Framework: comandos](https://docs.unity3d.com/Packages/com.unity.test-framework@1.4/manual/reference-command-line.html).
- [Dependencias de carpeta local](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-localpath.html).

Revisadas el 5 de octubre de 2026. Describen interfaces, no una instalación probada de Unity/Meta. La documentación antigua de este archivo se conserva en el historial Git.
