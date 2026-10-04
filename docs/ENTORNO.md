# Entorno de desarrollo y primera prueba

Estado: procedimiento para ejecutar, no instalación ya realizada. Fuentes oficiales en `FUENTES.md` [S03–S08]. No se fijan versiones de Unity o Meta hasta comprobar una combinación compatible en una máquina real.

## 1. Auditar la máquina y el repositorio

Confirmar sistema operativo, arquitectura, espacio disponible, Unity Hub/editor instalado y acceso al visor. Leer estado de Git; no sobrescribir cambios existentes.

```bash
git status --short
python3 --version
python3 tools/check_repo.py
python3 -m unittest discover -s tests -v
```

Estos tests son del repositorio y modelo de referencia, no de Unity.

## 2. Elegir y registrar versiones

Consultar requisitos actuales de Unity y SDKs oficiales. Elegir una versión del editor soportada por la integración de Meta, con Android Build Support y sus SDK/NDK/JDK compatibles. Crear un proyecto real en `unity/RoomBreakers`.

Registrar en ESTADO.md: versión completa del editor, render pipeline, XR provider, Meta Core/Interaction/MRUK, sistema operativo y motivo de selección. Conservar `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, lockfile generado y archivos .meta.

No combinar sin verificación un rig antiguo con un nuevo provider, ni instalar dos proveedores que compitan por el mismo runtime. No usar números de versión de una respuesta anterior como evidencia de compatibilidad. Los listados del concurso contienen nomenclaturas de SDK que hay que reconciliar con la documentación de instalación vigente.

## 3. Crear proyecto mínimo

Usar plantilla 3D compatible con la ruta oficial escogida. Configurar control de versiones con assets de texto y metadatos visibles según opciones reales del editor. Integrar solo SDKs necesarios: passthrough, manos/interacción y utilidades de entorno. Añadir spatial audio cuando exista una escena que probar.

La primera escena debe contener rig válido, passthrough, manos y un objeto seleccionable. Sin modelos finales, boss, navegación de sala ni shaders complejos.

No publicar una escena sintética como un scan real. El modo escritorio sirve para lógica y depuración y debe estar etiquetado.

## 4. Preparar el Quest

Seguir el flujo oficial de cuenta/organización de desarrollo, modo desarrollador y configuración del dispositivo. Lorenzo debe completar acuerdos, inicio de sesión y autorizaciones que le correspondan. No pedir contraseña o códigos al agente.

Conectar USB de datos y autorizar depuración en el visor. Verificar el dispositivo mediante Meta Quest Developer Hub o herramientas Android de la instalación.

```bash
adb devices
```

Si aparece `unauthorized`, resolver en el visor. No copiar el número de serie a un repositorio público. En equipos con varios dispositivos, seleccionar el objetivo explícitamente antes de instalar.

## 5. Compilar e instalar

Seleccionar Android y arquitectura de 64 bits compatible con requisitos vigentes. Verificar provider, manifest y permisos mediante la guía oficial. No añadir permisos de cámara/micrófono por suposición: mostrar passthrough no implica que el juego necesite acceder a fotogramas crudos.

Usar Build and Run o, con un APK realmente generado y un solo dispositivo seleccionado:

```bash
adb install -r /ruta/local/al/build.apk
```

No guardar claves de firma, passwords ni APKs en el Git del código. El bundle identifier y la firma definitivos se acuerdan con el propietario y se conservan; no reemplazar una app existente de otra finalidad.

## 6. macOS y prueba real

La ruta base en Mac es editar y generar un APK Android que se ejecuta en el Quest. No prometer el flujo Meta Quest Link de PC como si estuviera disponible igual en macOS: consultar la guía actual del host soportado.

Un simulador puede ayudar con input y salas sintéticas. No valida registro espacial, jitter, legibilidad, cansancio, oclusión ni rendimiento del dispositivo final. Para esos puntos se necesita el visor.

## 7. Primer hito técnico

Ver el entorno mediante passthrough → ver manos → seleccionar y mover un objeto → pausar y recuperar tras perder una mano → reiniciar sin controles. Registrar qué ocurrió, dispositivo/OS/SDK y limitaciones.

Luego añadir un fixture de sala y la transformación entre escalas. Solo después conectar datos reales de habitación y validación de colocación.

## Problemas frecuentes y orden de diagnóstico

- No instala: comprobar módulo Android, dispositivo autorizado, arquitectura, firma y versión del paquete. Conservar error exacto.
- Pantalla negra: verificar escena de arranque, rig, render pipeline y logs; no instalar más paquetes al azar.
- No hay manos: verificar ajustes de dispositivo, permisos requeridos por la integración y rig/input real, después iluminación.
- No hay datos de habitación: verificar configuración de espacio, consentimiento y soporte de la API elegida. No inventar una mesa detectada.
- Editor funciona pero APK no: reproducir mínimo en Android y revisar platform defines, stripping, shaders y dependencias.
- Contenido se mueve: revisar marcos, tracking origin y localización; no arreglar una vista con offsets que desalinean la otra.

## Hecho vs. pendiente

Un setup pasa cuando se puede reconstruir desde el repositorio en la misma combinación de versiones y ejecutar la escena mínima en el dispositivo declarado. Adjuntar evidencia no sensible. Una captura del editor o un `npm install` no cumple ese hito.
