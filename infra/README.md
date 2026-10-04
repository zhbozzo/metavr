# Infraestructura: decisión explícita de no provisionar nube

El MVP es una app Unity/Android que se ejecuta en el Quest. La partida y el progreso son locales. No requiere servidor, base de datos, autenticación, pagos o llamadas a modelos. La entrega nativa se hará mediante el canal de Meta correspondiente, no alojando el juego como web.

Terraform es una herramienta para definir y gestionar infraestructura mediante código, según [HashiCorp](https://developer.hashicorp.com/terraform/intro). No crea escenas Unity, rigs de manos, APKs ni una cuenta aprobada del concurso.

Por eso esta base **no incluye archivos .tf ficticios ni ejecuta terraform apply**. «Preparar toda la base» se resuelve con arquitectura, configuración reproducible, scripts, pruebas e instrucciones. No se crean costes de nube.

## Cuándo reconsiderarlo

Solo ante una necesidad aprobada que no pueda resolverse localmente: por ejemplo, servicio online posterior al concurso. Antes hacen falta proveedor, región, presupuesto máximo, responsable de facturación, datos tratados, seguridad, operación y política de eliminación.

En ese caso: decisión escrita, infraestructura mínima, estado protegido, credenciales fuera de Git y plan revisado antes de aplicar. Nunca guardar tfstate, secretos o tfvars reales en un repositorio público. No instalar una nube para justificar una tecnología.

## Infraestructura de trabajo actual

GitHub contiene código/especificación; Unity y tooling Android generan builds; el visor ejecuta la app; Meta Developer Dashboard distribuye el APK de evaluación cuando exista. La automatización de comprobaciones del repo no equivale a build ni publicación de Unity.
