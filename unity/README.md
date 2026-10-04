# Proyecto Unity — pendiente de crear y validar

El proyecto real se creará en `unity/RoomBreakers` mediante una instalación compatible de Unity, siguiendo [ENTORNO](../docs/ENTORNO.md). Esta carpeta no contiene una app ejecutable ni un conjunto de ProjectSettings fabricado.

Primera tarea: [RB-001](../docs/BACKLOG.md). Instalar/verificar herramientas, generar el proyecto real, fijar versiones y ejecutar un APK mínimo en un Quest. Después implementar transformación y doble representación.

Estructura prevista dentro del proyecto: `Assets/RoomBreakers/` con Scripts, Scenes, Prefabs, Art, Audio, Settings y Tests. La separación lógica está en [ARQUITECTURA](../docs/ARQUITECTURA.md).

Commitear los archivos .meta y configuraciones/lockfiles generados por el editor. No commitear Library, Temp, builds, logs privados, claves de firma ni SDKs/assets de terceros fuera de sus permisos de redistribución.

Las pruebas Python de `../tests` validan solo el modelo de referencia. La primera implementación Unity necesita tests propios con rotaciones 6DoF y jerarquías reales; copiar una fórmula de Python no acredita su integración con manos o entorno.
