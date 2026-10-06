# Entorno de desarrollo sin visor

Política: [SIMULATOR_FIRST](SIMULATOR_FIRST.md). Procedimiento pendiente de ejecución, no instalación ya realizada. No comprar, arrendar, pedir prestado ni exigir un dispositivo físico. No usar USB, Link o data forwarding como prerequisitos.

## 1. Auditar el host y conservar trabajo

Inspeccionar OS, arquitectura, RAM, espacio disponible, Git y editor/licencia reales. Ruta preferida: macOS Apple Silicon. El agente con acceso solo a GitHub no tiene automáticamente acceso al Mac.

```bash
git status --short
python3 --version
python3 tools/check_repo.py
python3 -m unittest discover -s tests -v
```

Con .NET disponible ejecutar las suites del README. Estas comprobaciones no son tests del motor.

## 2. Proyecto y primera ejecución desktop

Seguir [ARRANQUE_UNITY](ARRANQUE_UNITY.md). Con Unity 6 instalado/activado y `UNITY_EDITOR` correcto:

```bash
python3 tools/setup_unity_project.py --create
```

Con Test Framework configurado:

```bash
python3 tools/setup_unity_project.py --create --verify
```

No usar un número de versión supuesto. Conservar ProjectVersion, manifest, lockfiles y .meta generados; no sobreescribir escenas. Abrir DesktopEncounter y comprobar el recorrido existente, sin reescribir las mecánicas. El ratón y la habitación sintética son desarrollo explícito.

## 3. Runtime XR compatible

Verificar documentación y versiones del editor, render pipeline, OpenXR Plugin, Meta Core, Interaction SDK, MRUK, XR Simulator standalone y API gráfica. Registrar la combinación realmente instalada. La guía oficial contempla macOS ARM y OpenXR Plugin 1.13.0+; eso es un mínimo documentado, no nuestra configuración probada. Ver referencias R1/R2 en SIMULATOR_FIRST.

Instalar/configurar software por la ruta oficial en el equipo accesible, sin aceptar licencias por el propietario. No mezclar el antiguo paquete del simulador con standalone ni desinstalar dependencias ajenas automáticamente. Activar el runtime solo durante el desarrollo y registrar cómo desactivarlo para restaurar el anterior.

Usar primero perfil Quest 3 y manos simuladas por teclado/ratón. Comprobar extensiones de manos, escena y passthrough. El perfil solo determina capacidades reportadas: no emula CPU/GPU ni ejecuta Android. Probar después Quest 3S con reinicio de sesión según la guía del runtime.

## 4. Primera escena XR

Rig válido, runtime activo, manos simuladas y un objeto seleccionable. Confirmar que la entrada pasa por el adaptador XR real y no por DesktopEncounterHand. Después conectar el encuentro existente, conservando cámara y marcos de coordenadas.

Sin controller obligatorio ni compras de mandos. No usar funcionalidades que solo se obtienen reenviando datos desde hardware. No depender de Environment Depth en Mac: la documentación del simulador lo limita a Windows. Trabajar con geometría de escena admitida y declarar capacidades ausentes.

## 5. Habitaciones de desarrollo

MRUK contempla Device/runtime, Prefab y JSON. Auditar MetaRoomSource y crear adaptadores explícitos que falten. Un nombre de API Device no convierte datos procedentes de XR Simulator en un scan real.

Conservar el mismo RoomSnapshot, planificador y dominio. Etiquetar fuente, fixture y versión; cargar paredes/suelo/obstáculos sintéticos y recorrer el juego. Denegación, datos faltantes y sala imposible deben producir recuperación visible. No sustituirlos silenciosamente por la habitación ideal.

No modificar las protecciones de SyntheticRoomSource en release por el simple cambio de política. La ruta Android debe seguir usando datos consentidos del entorno del juez y manos del dispositivo; los fixtures son de desarrollo.

## 6. Compilar Android sin instalar en hardware propio

Instalar únicamente el Android Build Support y SDK/NDK/JDK compatibles con el editor escogido cuando corresponda. Verificar configuración vigente para Meta VR, arquitectura, manifest, escenas, stripping y shaders serializados. No inventar permisos: passthrough no implica acceso a cámara cruda.

Producir un APK con firma/configuración del proyecto autorizadas. Registrar commit, versiones y hash del archivo real. No guardar llaves o APKs privados en Git. No ejecutar adb install, no exigir depuración USB y no marcar arranque Android como probado. XR Simulator no contiene una capa Android.

## Diagnóstico antes de añadir funciones

- Sin editor/licencia: bloqueo de software; continuar tareas verificables, no pedir Quest.
- Runtime no arranca: registrar versión, provider, API gráfica y extensiones; no instalar paquetes al azar.
- Sin manos simuladas: comprobar perfil/input/SDK y recorrido de datos, no sugerir comprar mandos.
- Sin habitación: comprobar entorno sintético/runtime o fixture y callback de carga; no fingir detección.
- Desktop funciona, XR falla: aislar rig y adaptadores, no eliminar la integración Meta.
- APK no compila: revisar errores Android, símbolos, stripping, permisos y referencias; no declarar que el simulador prueba ese binario.

## Cierre de hitos

SIM-001 se acredita con editor y tests reales. SIM-002/SIM-003 requieren runtime XR y recorrido observado con manos/salas simuladas. SIM-005 requiere APK producido. Ningún hito exige disponer de un visor; ninguno convierte la ausencia de prueba física en validación. [Pruebas](PRUEBAS.md) · [Backlog](BACKLOG.md).
