# Comprobación automática, no ausencia de comprobación

## Ejecutar sin revisar manualmente cada paso

Los pushes y PRs configurados ejecutan las pruebas del núcleo C# y las comprobaciones Python. Las suites prueban el código, no un visor. No es una certificación de ausencia de errores.

```bash
python3 tools/check_repo.py
python3 -m unittest discover -s tests -v
dotnet run --project validation/RoomBreakers.Core.Tests/RoomBreakers.Core.Tests.csproj --configuration Release
dotnet run --project validation/RoomBreakers.Hardening.Tests/RoomBreakers.Hardening.Tests.csproj --configuration Release
```

La suite nueva contiene 15 tests. Dos de ellos ejecutan 5.000 transformaciones 6DoF y 20.000 comandos entrelazados con semillas fijas. Son iteraciones dentro de tests, no usuarios, habitaciones ni pruebas físicas. Incluye casos que fallaban en el código anterior: soltar usando una posición válida antigua tras rechazar un movimiento y aceptar configuraciones/poses que no pueden representarse en ambas vistas.

## Cuando exista un proyecto Unity real

```bash
python3 tools/run_unity_checks.py
```

Lee `unity/RoomBreakers/ProjectSettings/ProjectVersion.txt` y busca ese editor exacto en ubicaciones habituales. No elige automáticamente otra versión ni instala software. Para otra ruta, usar `--project` y/o `--unity`, o `UNITY_EDITOR`. Al indicar un ejecutable explícito, comprobar que coincide con la versión del proyecto.

Requisitos: licencia/editor configurados, proyecto real, paquete local importado y tests del paquete visibles mediante `testables` según el README del paquete. Cerrar el proyecto en Unity antes de ejecutarlo en batch. Un agente local puede ejecutar este comando sin pedir confirmación repetida, pero no aceptar acuerdos ni credenciales por el propietario.

El script ejecuta únicamente `RoomBreakers.Core.UnityTests` en EditMode. Revisa el resultado XML de NUnit y exige los casos de correspondencia con Transform y mano ampliada. No considera éxito que el proceso salga con código cero si faltan resultados, se omiten tests, no se ejecuta nuestra suite o hay fallos. Los informes se generan en carpetas nuevas bajo `.validation-local/`, excluidas de Git, sin subir rutas de máquina ni logs automáticamente.

Salida: PASS y código 0, FAIL y código 1, BLOCKED y código 2. BLOCKED no significa aprobado. Importar, renderizar, usar manos reales, construir Android y perfilar en Quest son verificaciones diferentes. El script no ejecuta PlayMode, no crea un APK ni mide comodidad.

Los 16 tests Python de este script usan informes sintéticos y mocks del lanzamiento del proceso; validan el propio verificador y nunca se presentan como una ejecución de Unity.

## Límite que sigue abierto

La lógica ahora rechaza la devolución después de recibir explícitamente una muestra inválida. Si el sensor deja de enviar callbacks, el futuro adaptador debe detectar el silencio y pausar/cancelar. Este cambio no pretende resolver un sensor que todavía no se ha integrado.

## Fuentes de interfaz verificadas el 4 de octubre de 2026

- [Unity: ejecución de tests desde terminal](https://docs.unity.com/en-us/engine/6000.3/manual/scripting/test-framework-introduction/running-tests/run-tests-from-command-line).
- [Unity: referencia de argumentos](https://docs.unity.com/en-us/engine/6000.7/manual/scripting/test-framework-introduction/reference-command-line): `-batchmode`, `-runTests`, `-assemblyNames`, `-testPlatform`, `-testResults`; no usar `-quit` durante la ejecución de tests.
- [Microsoft: dotnet run](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-run).

Estas referencias explican interfaces; no establecen que se haya instalado esa versión del editor ni que el paquete Meta sea compatible con ella. La compatibilidad real permanece por verificar.
