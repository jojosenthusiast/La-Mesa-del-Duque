# Registro de cambios

Todas las modificaciones notables de este proyecto se documentarán en este archivo.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/),
y este proyecto adhiere al [Versionado Semántico](https://semver.org/lang/es/).
## [0.12.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.11.0...v0.12.0) (2026-05-09)


### Nuevas funcionalidades

* **ui:** convert home page into operational hub with auth guards ([4002306](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/40023069f799b8f32a70fb958fdc7d1ef3432e79))
* **ui:** establecer shell operativo, landing y auth guards — Slice 7a ([2e0a53d](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/2e0a53d86b7a4a0fc0a21ada7b51040c6f30d147))
* **ui:** establish Sprint 1 operational shell ([5d6b8a4](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/5d6b8a49f00095705e12270512d05312e242f193))


### Pruebas

* **ui:** add shell smoke and index page tests ([2f3fcae](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/2f3fcaefdd31d356e1d43001756ac4bb31dffa1f))

## [0.11.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.10.1...v0.11.0) (2026-05-09)

### Summary

- Agrega notificaciones en tiempo real con SignalR para el flujo de pedidos, permitiendo que los clientes reciban eventos cuando se crean, cambian de estado o cancelan pedidos.

### Details

- Crea interfaz `INotificadorPedidos` en la capa de aplicación con métodos `NotificarPedidoCreadoAsync`, `NotificarEstadoCambiadoAsync` y `NotificarPedidoCanceladoAsync`.
- Implementa `PedidosHub` como hub de SignalR en `/hubs/pedidos` para comunicación server→client.
- Implementa `SignalRNotificadorPedidos` con patrón fire-and-forget (try/catch que no propaga errores de SignalR al caller).
- Integra notificaciones en `PedidosServicio`: emite al crear pedido, al transicionar Pendiente→EnPreparacion y EnPreparacion→Pagado, y al cancelar.
- Agrega test doubles: `NotificadorPedidosSpy` para verificar emisión de notificaciones y `NotificadorPedidosNulo` para mantener compatibilidad con tests existentes.
- Expande suite de pruebas de 192 a 195 tests con 3 nuevos tests de contrato de notificaciones.
- SignalR está incluido en el shared framework de ASP.NET Core 8 — no requiere paquete NuGet adicional.

### Nuevas funcionalidades

* **notifications:** agregar notificaciones en tiempo real con SignalR — Slice 6 ([ba4b920](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/ba4b9205871da278211bb9855915954df34931d7))
* **pedidos:** emitir y validar notificaciones de flujo ([1d1430c](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/1d1430cac93497f648083f60c491921379683d60))
* **web:** add SignalR hub and pedido notifier ([8a2bdad](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/8a2bdad1f21d6cd7801f1f3feacb65aba626ff8f))

## [0.10.1](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.10.0...v0.10.1) (2026-05-09)


### Documentación

* **release:** ampliar notas de las versiones 0.9.0 y 0.10.0 ([59082a0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/59082a011cff4e5d32e9f7e58612161ff8e2cbcc))

## [0.10.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.9.0...v0.10.0) (2026-05-09)

### Summary

- Agrega gestión de usuarios, autenticación por cookies con BCrypt y autorización por roles, cubriendo el backend de HU-021 y HU-025 del Sprint 1.

### Details

- Agrega entidad `Usuario` con métodos de dominio: activar/desactivar, cambio de rol, actualización de último acceso y cambio de contraseña.
- Implementa `IUsuariosServicio` y `UsuariosServicio` con CRUD completo, validación de credenciales con BCrypt y reglas de negocio.
- Agrega `IRolRepositorio` y `RolRepositorio` para búsqueda de roles por ID y nombre.
- Configura autenticación por cookies en `Program.cs` con timeout deslizante de 8 horas y claims de rol.
- Agrega páginas Razor `Login.cshtml` y `Logout.cshtml` con validación de credenciales y cierre de sesión.
- Configura middleware de autorización por rol para proteger endpoints.
- Implementa soft delete de usuarios (`activo=false`) en lugar de eliminación física.
- Agrega 137 tests unitarios para `UsuariosServicio`, elevando la suite completa a 192 tests.

### Nuevas funcionalidades

* **auth:** agregar gestión de usuarios, login con bcrypt y autorización por roles — Slice 5 ([c1cd1d6](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/c1cd1d63dbc4c3400191ff80cc51f9c07f4ad974))
* **auth:** agregar gestión de usuarios, login con bcrypt y autorización por roles — Slice 5 ([0e50f12](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/0e50f12754a601d1754b271b63e6a06cbd9beb49))

## [0.9.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.8.0...v0.9.0) (2026-05-09)

### Summary

- Alinea las reglas de estado de mesa con HU-016 del Sprint 1, agregando guardas de transición y liberación automática al completar pedidos.

### Details

- Agrega guarda que impide volver una mesa a `Disponible` si tiene pedidos activos (`Pendiente` o `EnPreparacion`).
- Libera automáticamente la mesa a `Disponible` al pagar o cancelar el último pedido activo asociado.
- `PagarPedidoAsync` y `CancelarPedidoAsync` invocan `LiberarMesaSiCorrespondeAsync` después de la transición de estado del pedido.
- `CambiarEstadoMesaAsync` rechaza la transición a `Disponible` si existen pedidos activos en la mesa.

### Nuevas funcionalidades

* **mesas:** alinear reglas de mesa con criterios de aceptación del sprint 1 — Slice 4 ([3fd4c69](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/3fd4c69f8a66caf86223399757d9be50edd198f0))

## [0.8.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.7.0...v0.8.0) (2026-05-09)

### Summary

- Completa el backend de **Productos + Recetas** para Sprint 1, endureciendo las reglas del catálogo y agregando la base persistente para modelar instrucciones e ingredientes por producto.

### Details

- Endurece la validación de productos para exigir precio **mayor que cero** y evita productos gratuitos por accidente.
- Amplía el contrato de catálogo para devolver y actualizar `ImagenUrl` y `TiempoPreparacionMin` junto con el resto de campos relevantes del producto.
- Agrega `RecetaProducto` y `RecetaIngrediente` como agregado independiente para representar preparación e ingredientes requeridos sin inflar el agregado `Producto`.
- Incorpora repositorios, servicio de aplicación, configuraciones EF Core y migración `AgregarRecetasProductosSprint1` para persistir recetas en la base.
- Deja preparada la estructura para conectar recetas con el descuento automático de inventario en slices posteriores del flujo POS.


## [0.8.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.7.0...v0.8.0) (2026-05-09)


### Nuevas funcionalidades

* **productos-recetas:** completar backend de catálogo y recetas del sprint 1 — Slice 3 ([14ba704](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/14ba7048098675f46b90e90440a19ad22b73bab4))

## [0.7.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.6.0...v0.7.0) (2026-05-08)

### Summary

- Completa el flujo POS del pedido para Sprint 1 en backend, incorporando `tipo_servicio`, asignación opcional de mesa, eliminación segura de pedidos pendientes y trazabilidad de auditoría.

### Details

- Agrega `TipoServicio` al contrato de `Pedido` para distinguir `ParaLlevar` y `ComerAqui`.
- Permite crear pedidos sin mesa y restringe la asignación de mesa a los casos válidos del flujo POS.
- Implementa `EliminarPedidoPendienteAsync` para eliminar pedidos no pagados, liberar la mesa asociada y registrar una auditoría `DELETE`.
- Ajusta persistencia EF Core para soportar `MesaId` nullable y la nueva forma del agregado `Pedido`.
- Amplía las pruebas automatizadas para cubrir creación, modificación, pago, eliminación y reglas de mesa del Slice 2.


### Nuevas funcionalidades

* **pos:** completar flujo de pedidos del sprint 1 — Slice 2 ([9937f10](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/9937f106dee3a6690ce16af9f2b5b4e4593ed580))

## [0.6.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.5.1...v0.6.0) (2026-05-08)


### Nuevas funcionalidades

* **db:** agregar esquema canónico completo de 24 tablas — Slice 0 ([#16](https://github.com/jojosenthusiast/La-Mesa-del-Duque/issues/16)) ([83c7d7a](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/83c7d7a1004d66e1c9d12a4abc0a035cbd2347be))
* **pedidos:** alinear ciclo de vida del pedido con criterios de aceptación — Slice 1 ([d298a62](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/d298a620de1a8bcec35cb91ad8010e55a49fbfb8))
* **pedidos:** alinear ciclo de vida del pedido con criterios de aceptación — Slice 1 ([292f7ee](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/292f7ee5a306a9dd6e4e1ea3651f46f117f9a11e))

## [0.5.1](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.5.0...v0.5.1) (2026-05-07)


### Correcciones

* **calidad:** agregar higiene de releases y politica de documentacion curada ([#13](https://github.com/jojosenthusiast/La-Mesa-del-Duque/issues/13)) ([36a8cb4](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/36a8cb4523ee71bf0731eba8045943180e2c63a1))

## [0.5.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.4.0...v0.5.0) (2026-05-06)


### Nuevas funcionalidades

* **dominio:** implementar base del modelo de pedidos ([7680ad9](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/7680ad98eaf362f4d06e7aac907b3c85afdf39e9))
* **dominio:** implementar base del modelo de pedidos ([2510e69](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/2510e6979e0585b961bb65ac565942abecd0464c))
* **marca:** integrar fundación visual ([db3cf42](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/db3cf42c54f5a5d534b4b5796a53d3b8c04c00a7))
* **marca:** integrar fundación visual ([b4b0986](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/b4b09861801f0c273ab349b1c15bd1612dcdd43d))
* **pedidos:** agregar persistencia y servicios de sprint 1 ([6dd8b76](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/6dd8b76cec695416117c5f5603056942229a391f))
* **pedidos:** agregar persistencia y servicios de sprint 1 ([fc040ed](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/fc040ed19eaa3ed5396fac6c56ced6a03cbd3e8c))


### Documentación

* **pr:** limpiar plantilla de pull request ([4e1ca8d](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/4e1ca8d84f135c52a73a59a56b03e20ddaaabc63))
* **release:** curar notas de la version 0.4.0 ([51f4496](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/51f44962d0f53ae5cd7441b0522c3b0ded625716))
* **releases:** ampliar notas de la versión 0.3.1 ([1fe2aae](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/1fe2aae6b8bcd38b549b581ebbff25640e60d2e1))
* **releases:** documentación propia para las versiones ([a0a4e38](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/a0a4e38afb58a53e17893c48a230a0c45546463b))
* **releases:** establecer documentación profesional de versiones ([3dad06e](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/3dad06e394c297187a0aa64221b19853d917f5d0))
* simplificar redacción de documentación base ([d7ac072](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/d7ac07227dea47579214a7de0cff722213157b71))


### Tareas de mantenimiento

* base inicial del proyecto ([9bada27](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/9bada27c442d0ff57f04e7f3474a27e4de8af44a))
* crear base inicial del proyecto ([34ea98d](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/34ea98d5e3215c2d24b48e378c4411ca00df08c0))
* inicializar repositorio con README base ([f9aa116](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/f9aa116932e5ba8dcaa49ffcf4892cbc44ec7b1d))
* **main:** release 0.1.1 ([4b69a07](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/4b69a071ce6a5da974dbb1da4b537ba5692a0288))
* **main:** release 0.1.1 ([da41548](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/da41548632d32061d03a84568c56aaffe672ac04))
* **main:** release 0.2.0 ([e5bc158](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/e5bc1587ccfc9f1a7a0065b602e6cd53dd80e740))
* **main:** release 0.2.0 ([12ba864](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/12ba8641a775fa4627a66cd1a4bf9d3144dff8ca))
* **main:** release 0.3.0 ([8511fc6](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/8511fc6883b6b52493da5bae96308fc8e85082dc))
* **main:** release 0.3.0 ([f9956fb](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/f9956fb7a95514f5ba8d01052bd9c03699f75711))
* **main:** release 0.3.1 ([2039d37](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/2039d37307aa0e79c425010c27fbec17156a4022))
* **main:** release 0.3.1 ([3296b48](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/3296b48c0c1fd54112311392c2c92438c8b769aa))
* **main:** release 0.4.0 ([ad92083](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/ad92083e15092edafdbb622278d2d20678b6296d))
* **main:** release 0.4.0 ([eea284e](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/eea284e6f3e5765b611634fe8f60cd4ec536fac5))


### Integración continua

* **calidad:** agregar gobernanza progresiva de calidad ([917de32](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/917de32a1f8bb0af21566ebc5950009a059243d0))
* **calidad:** agregar gobernanza progresiva de calidad ([820eb1c](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/820eb1c7050b79b3f04398b4ce3c3c01aa2f8727))

## [0.4.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.3.1...v0.4.0) (2026-05-06)

### Summary

- Incorpora la base de persistencia y servicios de aplicación para el flujo de pedidos, dejando preparados los casos de uso principales de catálogo, mesas y pedidos.

### Details

- Agrega persistencia EF Core/Npgsql para `CategoriaProducto`, `Producto`, `Mesa`, `Pedido` y `DetallePedido`.
- Agrega repositorios específicos, unidad de trabajo y servicios de aplicación para catálogo, mesas y pedidos.
- Incorpora cancelación lógica, actualización de datos, desactivación con guardas y modificación de detalles de pedido.
- Amplía la cobertura automatizada con pruebas unitarias e integración sobre dominio, repositorios, unidad de trabajo y servicios.

## [0.3.1](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.3.0...v0.3.1) (2026-05-02)

### Summary

- Establece un estándar profesional para documentar releases relevantes del producto, separando el changelog mecánico de Release Please de la documentación curada del proyecto.

### Details

- Agrega documentación curada para `v0.2.0` y `v0.3.0` en `docs/releases/`, con resumen ejecutivo, impacto funcional, impacto técnico, verificación, riesgos y próximos pasos.
- Actualiza la definición de hecho para exigir documentación curada cuando un cambio constituya una versión relevante del producto.
- Actualiza la plantilla de Pull Request para recordar la documentación de release antes de fusionar incrementos significativos.
- Define que Release Please se mantiene como automatización de versionado y changelog, mientras `docs/releases/` será la fuente de documentación profesional.
- Documentación detallada: [`docs/releases/v0.3.1-documentacion-profesional-releases.md`](docs/releases/v0.3.1-documentacion-profesional-releases.md).

## [0.3.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.2.0...v0.3.0) (2026-05-02)


### Nuevas funcionalidades

* **dominio:** implementar base del modelo de pedidos ([7680ad9](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/7680ad98eaf362f4d06e7aac907b3c85afdf39e9))
* **dominio:** implementar base del modelo de pedidos ([2510e69](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/2510e6979e0585b961bb65ac565942abecd0464c))

## [0.2.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.1.1...v0.2.0) (2026-05-01)


### Nuevas funcionalidades

* **marca:** integrar fundación visual ([db3cf42](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/db3cf42c54f5a5d534b4b5796a53d3b8c04c00a7))
* **marca:** integrar fundación visual ([b4b0986](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/b4b09861801f0c273ab349b1c15bd1612dcdd43d))

## [0.1.1](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.1.0...v0.1.1) (2026-05-01)


### Documentación

* simplificar redacción de documentación base ([d7ac072](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/d7ac07227dea47579214a7de0cff722213157b71))


### Tareas de mantenimiento

* base inicial del proyecto ([9bada27](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/9bada27c442d0ff57f04e7f3474a27e4de8af44a))
* crear base inicial del proyecto ([34ea98d](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/34ea98d5e3215c2d24b48e378c4411ca00df08c0))
* inicializar repositorio con README base ([f9aa116](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/f9aa116932e5ba8dcaa49ffcf4892cbc44ec7b1d))

## [0.1.0] — 2026-04-30

### Agregado
- Configuración inicial del repositorio con arquitectura en capas (Dominio, Infraestructura, Web).
- Pruebas unitarias con xUnit y recolección de cobertura con Coverlet.
- Pipeline de integración continua (`ci.yml`) con compilación, pruebas, verificación de paquetes vulnerables y gobernanza de documentación.
- Pipeline de seguridad (`security.yml`) con análisis CodeQL, detección de secretos con Gitleaks y verificación de paquetes vulnerables.
- Pipeline de generación de releases (`release.yml`) con Release Please.
- Plantilla de Pull Request con checklist de calidad, seguridad, pruebas, trazabilidad e impacto.
- Documentación de calidad: plan de calidad, definición de hecho, matriz de trazabilidad, registro de riesgos, checklist de revisión de código y checklist de seguridad.
- Documentación de arquitectura y ADR (Architecture Decision Records).
- Documentación de requisitos: historias de usuario y criterios de aceptación.
- Documentación de seguridad: alcance SGSI y declaración de aplicabilidad ISO 27001.
- Documentación de pruebas: estrategia de pruebas, suite de regresión y matriz de impacto de cambios.
- Documentación de auditoría: plan de auditoría y checklist de evidencia.
- Métricas e indicadores de calidad.
- Archivo `.editorconfig` para C#, Razor, JSON, YAML y Markdown.
- Archivo `.gitignore` con exclusiones para secretos, artefactos de compilación y resultados de pruebas.

[0.1.0]: https://github.com/jojosenthusiast/La-Mesa-del-Duque/releases/tag/v0.1.0
