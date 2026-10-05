# Decisiones y riesgos

Registro inicial: 4 de octubre de 2026. Una decisión aceptada de diseño no implica que esté implementada o validada.

## ADR-001 · Unity como ruta principal

Motivo: integrar interacción de manos, utilidades del entorno y presentación nativa en el mismo proyecto. Se mantiene el motor hasta que exista un bloqueo concreto, no porque otro agente prefiera tecnología web.

Consecuencia: entorno Android/Unity real y visor son necesarios. La selección inicial WebXR del registro no se conserva en el formulario final si no describe el build.

## ADR-002 · Una autoridad, dos representaciones

Motivo: prevenir estados divergentes, daño duplicado y problemas de física a escalas diferentes. Pose y eventos en coordenadas canónicas; vistas como consumidores. La mano grande es visual.

Consecuencia: transformación y sincronización son el primer bloque testeado. Nunca reparar desalineación con offsets no documentados en una sola vista.

## ADR-003 · Sector frontal y plantillas validadas

Motivo: comodidad sentado, FoV y viabilidad. El proyecto no intenta navegar por cualquier geometría doméstica. Datos insuficientes producen reconfiguración explícita.

Consecuencia: menos contenido espacial, pero comportamiento verificable. Un modo sintético de desarrollo no sustituye el escaneo real del producto.

## ADR-004 · MVP offline

Motivo: no hay necesidad de servidor para una partida individual y progreso local. Evita cuentas, latencia, gastos y exposición de geometría doméstica.

Consecuencia: sin Terraform, APIs de modelos, rankings globales, multiplayer ni telemetría automática. Un cambio exige necesidad concreta, presupuesto, privacidad y aprobación del propietario.

## ADR-005 · Manos con movimientos controlados

Motivo: reducir dependencia de tracking de movimientos rápidos y mantener comodidad. Soltar en destino validado reemplaza un lanzamiento físico. Eye tracking no es dependencia.

Consecuencia: más claridad de selección y destinos; modo una mano secuencial; pruebas de falsa activación y tracking perdido.

## ADR-006 · Evidencia antes que promesas

Motivo: la idea tiene riesgos de atención, originalidad e implementación. El objetivo es competir, no fingir una entrega acabada.

Consecuencia: estado y métricas explícitos; requisitos frente a evidencia; no falsear vídeos o cifras; no garantizar premios ni plazos de desarrollo sin datos.

## ADR-007 · Núcleo C# y paquete local antes del editor disponible

Fecha: 4 de octubre de 2026. No hay editor Unity ni visor en el entorno remoto; sí hay escritura de código y ejecución de CI en GitHub. Se implementa el núcleo en C# compatible con .NET Standard 2.1 y se valida el mismo código mediante .NET. Se añade un paquete local de Unity con una escena generada por script de editor, pendiente de ejecución real.

Consecuencia: avanzar sin cambiar motor ni fabricar ProjectSettings, GUIDs, API Meta o resultados de hardware. RB-001 sigue bloqueado; los demás tickets conservan pendientes sus condiciones de Unity/visor. El paquete no sustituye el proyecto final. Validar import, metadatos, pipeline y tests EditMode antes de ampliar la escena. Ver `IMPLEMENTACION_001.md` y las fuentes del README del paquete.

## Registro de riesgos

| Riesgo | Señal temprana | Respuesta |
| --- | --- | --- |
| Atención dividida entre escalas | Mirar abajo permanentemente o giros incómodos | Ajustar maqueta/sector, destacar correspondencia, probar pausa táctica |
| Jitter amplificado | Objeto grande tiembla con mano aparentemente quieta | Aumentar escala, filtros medidos, snap e histéresis; no más detalle visual |
| Selección de objetos diminutos | Errores al escoger criaturas cercanas | Aumentar collider, separar amenazas y fijar objetivo |
| Registro espacial débil | Portal se desliza o vistas discrepan | Revisar marcos, pausar al perder localización, reubicar |
| Sala sin solución | Ruta bloqueada o reflector inútil | Validar plantilla antes de partida y ofrecer otro sector |
| Sensación de tablero genérico | Usuarios no identifican efecto de la habitación | Exigir una decisión dependiente de geometría antes de más niveles |
| Coste de doble render | Frame time crece con dos vistas | Geometría simplificada, pooling, materiales compartidos y límites |
| Sin acceso a visor | Solo evidencia de editor | Resolver acceso antes de comprometer cronograma; declarar bloqueo |
| Alcance excesivo | Nuevas funciones antes de sesión completa | Recortar P1 y seguir PLAN.md |
| Candidatura inaccesible | Enlace exige cuenta del autor o build no instala | Ensayo con cuenta autorizada distinta antes de envío |
| Material privado o sin licencia | Assets/capturas sin registro | Retirar de publicación y resolver derechos antes de continuar |

## Plantilla de nueva decisión

Fecha, ID, contexto, alternativas, decisión, consecuencias, fuente cuando aplique, prueba necesaria y condición para revisarla. Nunca cambiar un principio de seguridad o privacidad sin dejar rastro y obtener la aprobación correspondiente.
