# Decisiones técnicas

### 1. Uso de `PasswordHasher<T>` para el manejo de contraseñas

Decidí utilizar `PasswordHasher<T>` de ASP.NET Core para el almacenamiento y validación de contraseñas. La alternativa considerada fue BCrypt. Elegí `PasswordHasher<T>` porque es una solución integrada en el ecosistema .NET, ampliamente utilizada y suficiente para los requisitos de la prueba.

Además, al tener mayor familiaridad con esta opción, pude implementar la autenticación con más seguridad y rapidez, reduciendo el riesgo de cometer errores por utilizar una herramienta que no recordaba en detalle.

### 2. Uso de Docker Compose para la ejecución del proyecto

Decidí utilizar Docker Compose para levantar toda la solución. La alternativa era ejecutar frontend y backend por separado mediante comandos independientes.

Elegí Docker porque simplifica la puesta en marcha del proyecto, reduce los pasos necesarios para el evaluador y permite que el entorno sea más consistente entre diferentes equipos. También facilita el manejo de variables de entorno, puertos y dependencias desde un único punto de configuración.

### 3. Arquitectura monolítica por capas

Decidí organizar la solución utilizando una arquitectura monolítica por capas (`Api`, `Aplicacion`, `Dominio` e `Infraestructura`). La alternativa descartada fue concentrar toda la lógica en una estructura más simple basada únicamente en carpetas y archivos sin una separación clara de responsabilidades.

Esta organización facilita la ubicación del código, mejora la mantenibilidad y permite separar la lógica de negocio, el acceso a datos y la exposición de la API. Para el tamaño y alcance de esta prueba, un monolito por capas ofrece un equilibrio adecuado entre simplicidad y orden sin añadir la complejidad de una arquitectura distribuida.


### Uso de IA

Utilicé herramientas de IA durante todo el desarrollo del proyecto para acelerar la implementación, generar estructuras iniciales, proponer soluciones técnicas, crear parte del código, documentación y pruebas.

Mi participación se centró en analizar el enunciado, definir el enfoque de implementación, validar que las reglas de negocio se cumplieran correctamente, revisar el código generado, ajustar requerimientos mediante iteraciones, ejecutar pruebas, verificar el comportamiento de la aplicación y tomar las decisiones técnicas finales sobre la solución entregada.

La mayor parte del código fue generada con asistencia de IA, pero cada funcionalidad fue revisada y validada antes de incorporarse al proyecto. También se realizaron múltiples ciclos de corrección y refinamiento para asegurar el cumplimiento de los requisitos de la prueba técnica.



## 4. Dificultad y resolución

La parte más difícil fue asegurar que la solución cumpliera exactamente con todos los detalles del enunciado. Había muchos requisitos pequeños que podían pasar desapercibidos, como permisos según el rol del usuario, cambios válidos de estado, validaciones específicas y respuestas esperadas por la API.

Lo resolví revisando varias veces la especificación, probando distintos escenarios y apoyándome en herramientas de IA para contrastar ideas y verificar que cada funcionalidad se comportara como se esperaba.


## Código de error para respuestas 500

La especificación define códigos de error para los escenarios de negocio y validación (401, 403, 404, 409 y 422), pero no establece un código específico para errores internos del servidor (500).

Para mantener consistencia en la estructura de respuestas de error, se decidió utilizar:

"codigo": "ERROR_INTERNO"

Este valor fue definido como una decisión de implementación y no forma parte explícita del contrato proporcionado.


# Aspectos que requirieron validación adicional

## RN-04 — Cálculo de SLA

Fue una de las reglas que más tuve que revisar durante el desarrollo para asegurarme de que la estaba aplicando correctamente.

En varias ocasiones volví a consultar la especificación para confirmar cómo debía calcularse la fecha límite a partir de las horas base de la categoría y el factor asociado a la prioridad seleccionada.

También fue necesario verificar en qué situaciones correspondía recalcular el SLA y cuándo una solicitud debía considerarse vencida.

La implementación final sigue la fórmula y reglas definidas en la especificación.

## RN-02 — Máquina de estados

La idea general de los estados y sus cambios la entendí desde el inicio. Sin embargo, en algunos momentos tuve que volver a revisar la especificación para confirmar qué transiciones estaban permitidas y cuáles no.

Esto me ayudó a verificar que estaba aplicando correctamente las reglas definidas y que no estaba permitiendo cambios de estado que no correspondían.

## Tablas adicionales creadas automáticamente

El sistema utiliza Entity Framework Core para manejar los cambios en la base de datos. Por esta razón aparecen dos tablas adicionales llamadas `__EFMigrationsHistory` y `__EFMigrationsLock`.

Estas tablas no pertenecen al funcionamiento del sistema ni guardan información de usuarios, solicitudes o categorías.

- `__EFMigrationsHistory`: guarda un registro de los cambios realizados en la base de datos.
- `__EFMigrationsLock`: ayuda a evitar problemas si se intenta realizar un cambio al mismo tiempo desde dos procesos diferentes.

Las únicas tablas creadas para la información del sistema son `Tenants`, `Usuarios`, `Categorias` y `Solicitudes`.