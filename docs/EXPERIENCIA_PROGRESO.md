# Pip, feedback y progreso local — incremento 007

El núcleo de esta mejora existe en C# y se prueba con entradas sintéticas y archivos temporales reales. La integración visual/almacenamiento Android se ha escrito, pero no se ha ejecutado en Unity o Quest. Los resultados definitivos de cada commit están en sus checks y en ESTADO.

## Qué cambia al jugar

Pip distingue entre recuperar seguimiento, capturar, llevar al portal, orientar el reflector, soltar una orientación alineada y aprovechar la vulnerabilidad de Shell. No es un chatbot ni un agente generativo: las pistas se derivan de las condiciones reales del encuentro, sin conexión de red.

La instrucción inicial es breve. Tras seis segundos activos sin progreso del jugador, se intensifica la ayuda; tras catorce, se explica la acción paso a paso. Son valores iniciales, pendientes de ajuste en visor. El avance autónomo de una criatura no se confunde con progreso del jugador. Pausar no consume ese tiempo. Un reinicio intencional limpia las pistas del intento anterior.

`PipGuidanceView` representa arcos de guía y énfasis del objetivo en ambas escalas, solo durante una introducción de fase o ayuda reforzada. Apunta a la criatura, al reflector o a la zona receptora que corresponda. Son auxiliares visuales sin colisiones ni capacidad de resolver acciones. Se añadieron pulsos breves al devolver criaturas y expresiones simples de Pip. Los efectos observan tiempo activo; el resultado final usa adornos estáticos, no una animación indefinida. No se oculta una amenaza activa para reproducir una cinemática.

El texto se distribuye en líneas y se mantiene en el marco de UI confirmado en vez de trasladarse con cada inclinación de cabeza. En el rig, se construyen primero los objetivos del menú, se ejecuta un paso autoritativo y después se actualizan feedback/render/audio; no se ejecutan dos actualizaciones visuales completas por frame.

## Resultado y motivo para volver

Solo una victoria o derrota terminada genera `RunResult`. Un intento abandonado, la pérdida de seguimiento y una invalidación de habitación no se registran como derrotas. En el recorrido ampliado, devolver los tres Motes no cuenta como ganar: hace falta resolver Shell.

Se guardan cinco totales: partidas terminadas, victorias, victorias perfectas, mejor integridad y criaturas devueltas. No hay récord de velocidad ni incentivos a hacer movimientos rápidos. El resultado distingue protección perfecta, victoria y derrota, y explica que RESTART inicia otro intento sin borrar los anteriores.

El faro cosmético de Pip tiene tres piezas, desbloqueadas con una, tres y cinco victorias. Cada pieza aparece en la maqueta y en el refugio grande. No cambia daño, dificultad o probabilidades. No hay compras, rachas, premios monetarios ni servidores.

`RunId` es un identificador aleatorio local de intento, no una identidad personal/dispositivo. No cambia entre Mote y Shell; cambia al reiniciar. El observador registra el resultado una sola vez. El archivo conserva los últimos 16 identificadores para reconocer reintentos de guardado/reapertura recientes; no pretende ser un registro infinito contra fraude.

## Guardado local y fallos

`ProgressJournal` alterna dos checkpoints pequeños. Escribe el inactivo, vacía el stream y comprueba que vuelve a leerse correctamente. La copia aceptada anterior queda intacta durante esa escritura. Una suma SHA-256 detecta corrupción accidental: no cifra ni autentica los datos y no es un sistema antitrampas.

Al cargar, elige el checkpoint válido con más resultados. Si el nuevo está truncado y el otro funciona, recupera el anterior; se puede perder el último checkpoint. Si ambos están dañados, existe una versión desconocida o los datos son ambiguos, conserva los archivos y continúa con progreso de sesión sin sobrescribirlos. No afirma tolerancia absoluta a cortes eléctricos, disco defectuoso o múltiples procesos escribiendo simultáneamente. Detecta instancias atrasadas, pero su contrato es un escritor por perfil.

Un fallo de escritura conserva el progreso pendiente en memoria y se informa en el resultado. El rig vuelve a intentar guardar al terminar otro encuentro o ante límites de ciclo de vida como pausa, pérdida de foco y cierre. No reintenta en cada frame. Si la aplicación se termina antes de que un guardado pendiente tenga éxito, ese progreso puede perderse.

En una instalación Unity, la ruta base es `Application.persistentDataPath/room-breakers/`. Los subdirectorios son:

- `practice-v1`: editor, builds Development, salas sintéticas o el modo básico sin Shell.
- `device-v1`: recorrido ampliado de una sala del dispositivo en un build no Development.

No se comparten logros entre práctica y release. Un build Development que usa un Quest real sigue siendo práctica. Si la ruta no está disponible, se usa un registro explícitamente en memoria y el juego sigue funcionando. Los journals se conservan durante LOAD ROOM para no perder resultados pendientes dentro de la misma ejecución.

**Nunca se guardan planos, fotografías, manos, UUID de anclajes, cuentas, correos, números de serie ni identificadores de visor.** No hay upload, analítica o sincronización. No se cambian las licencias o la privacidad del repositorio.

## Verificación que sí se puede automatizar

```bash
for project in validation/RoomBreakers.*.Tests/*.csproj; do
  dotnet run --project "$project" --configuration Release || exit 1
done
```

La suite Experience recorre el juego con entradas de una mano, observa victoria/reinicio, comprueba prioridades de pistas y escribe/recupera archivos en directorios temporales. La suite ProgressRecovery prueba memoria sin disco, formatos futuros, ambigüedad de checkpoints, fallo de escritura y reconocimiento de un guardado cuyo acuse quedó interrumpido. No reemplazan una prueba de Android ni del motor.

## Aceptación pendiente en Unity/Quest

Abrir una de las dos escenas de encuentro existentes. Comprobar que se siguen pudiendo capturar criaturas, orientar el reflector y usar PAUSE/RESUME, RESTART/CONFIRM/CANCEL y MOVE. Esperar sin actuar en la enseñanza, revisar las ayudas, pausar y verificar que no se adelantan. Ganar y volver a abrir la app: revisar resumen, checkpoint y pieza del faro. Verificar práctica y release por separado; no manipular datos reales para simular resultados de dispositivo.

Medir legibilidad, comodidad, colocación de texto, coste CPU de los arcos y coste del guardado en el visor. Ajustar tamaños y tiempos con esa evidencia. Todavía no se ha medido ninguna de estas métricas.

## Contratos consultados

- Unity, `Application.persistentDataPath`: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Application-persistentDataPath.html
- .NET, `FileStream.Flush(Boolean)`: https://learn.microsoft.com/dotnet/api/system.io.filestream.flush

El uso de esas APIs no demuestra que la integración Android ya haya sido ejecutada. No se descargaron SDKs ni se aceptaron licencias en este incremento.
