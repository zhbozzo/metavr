# Privacidad, seguridad de datos y procedencia

Política interna propuesta. No es una política legal publicada ni una certificación. El repositorio es público; no subir información que no deba hacerse pública.

## Datos del MVP

La partida se ejecuta localmente. La geometría de habitación solo se usa en memoria durante la sesión por defecto. Guardado local limitado a preferencias y progreso de juego. Sin backend, analytics automáticos, micrófono, fotogramas de cámara, cuentas ni datos biométricos persistentes.

El sistema operativo y los SDKs pueden tener sus propios permisos y tratamiento de datos. No afirmar que toda la plataforma opera sin recopilar nada; describir únicamente lo verificado en nuestro código y las dependencias reales.

Datos insuficientes o permiso rechazado: informar y permitir recuperar o salir. No mostrar una sala sintética como si fuera una detección del hogar.

## Nunca en Git público

Contraseñas, tokens, recovery codes, claves de firma, keystores, passwords de builds, IDs de dispositivo, enlaces de invitación que concedan acceso, correos de cuenta, capturas de inbox, room scans reales, fotos familiares o vídeos identificables. Tampoco archivos de estado Terraform ni dumps privados, aunque se excluyan normalmente por .gitignore.

Los fixtures de pruebas son habitaciones sintéticas. Los informes públicos usan identificadores de sesión anónimos. Una captura de gameplay se revisa para eliminar datos privados del entorno; no alterar la demostración del producto para esconder fallos.

## Assets y software de terceros

Mantener un registro por asset: nombre, creador, fuente, fecha de obtención, licencia exacta, permiso de uso en juego, permiso de redistribuir fuentes, atribución y prueba de autorización guardada en lugar adecuado.

Un asset comprado puede permitir distribuirlo dentro del juego y prohibir subir su fuente. No commitear bibliotecas comerciales completas. No copiar assets de First Encounters, A Fisherman's Tale ni otros productos de referencia. Las dependencias oficiales se instalan por sus mecanismos autorizados y conservan avisos requeridos.

No escoger una licencia de código abierto para el código propio en nombre del propietario. Esta preparación no añade un LICENSE ni concede nuevos derechos de redistribución. La titularidad exacta y la licencia final se deciden explícitamente.

## Material generado con IA

Registrar herramienta, propósito y condiciones aplicables. Revisar parecido a marcas/personajes, errores y coherencia visual. No asumir exclusividad ni derechos por el mero hecho de generar un asset. El vídeo de candidatura debe mostrar la app real, no una representación generada de un gameplay inexistente.

## Candidatura y publicación

Revisar reglas completas sobre contenidos, personas identificables, publicidad, marcas, material permitido y derechos concedidos a organizadores. Ser miembro Start no implica patrocinio o aval. No aceptar declaraciones legales, publicar en Store o enviar formularios sin autorización del propietario.

## Incidente

Si se detecta un secreto, detener nuevas publicaciones y avisar al propietario. Revocarlo/rotarlo por el canal autorizado antes de considerar saneado el incidente. Borrar la línea de la última versión no elimina el secreto del historial. No hacer force-push ni reescribir historia sin coordinar y autorizar esa acción.
