# Registro de cambios

Todas las modificaciones notables de este proyecto se documentarán en este archivo.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/),
y este proyecto adhiere al [Versionado Semántico](https://semver.org/lang/es/).
## [1.1.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v1.0.0...v1.1.0) (2026-05-16)


### Nuevas funcionalidades

* **aplicacion:** agregar CocinaServicio y DTOs ([665af8d](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/665af8dff1fe2727c45927ddfb54f68b221a9def))
* **aplicacion:** agregar logica de pago real con cuentas ([8a02fff](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/8a02fff971ad8fd06f0ca0e78edcdc953f648fef))
* **dominio:** agregar Cuenta, Pago, MetodoPago y estado EnCobro ([7c64670](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/7c64670e23efb564a75ea67542654c47fc36b297))
* **dominio:** agregar OrdenCocina y EstadoLineaCocina ([bea1e7f](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/bea1e7f0503f5c24ece80739110de85887473d50))
* **fotos:** agregar fotos reales de platos desde Unsplash (libres de uso) ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **infra:** agregar repositorio y migracion de OrdenCocina ([fd852d5](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/fd852d5e96a94a1af908b5211babc3dcc773d9d1))
* **infra:** agregar repositorios y migracion de Cuenta y Pago ([6e51990](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/6e51990bcfc21a0a236c3ddb473cd349fd064928))
* **kds:** agregar layout multi-cocinero de 3 columnas con notas y alergenos ([986cc70](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/986cc70fcf73c9f4045486e36f5401db36ecbe68))
* **offline:** implementar modo offline con PWA, cola IndexedDB y polling fallback para KDS ([d63b6d3](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/d63b6d36fc65e714e873d457f8c8407bb2bf84bd))
* **pos-ux:** Phase 1 — toast, modal, localStorage persistence, Enviar a Cocina ([31c71c9](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/31c71c9c80714f23075ab1dc72c16d91e24cc7f1))
* **pos-ux:** toast, modal, localStorage, Enviar a Cocina button (Phase 1 WIP) ([dd5cef0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/dd5cef0c3ddb0043e6d1e6ade5f3823375bd54dd))
* **pos:** agregar atajos de teclado, indicador de pasos y feedback visual ([ecf3689](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/ecf368983020fb12e554338a063ef5061efed813))
* **pos:** agregar division por items entre cuentas ([bd4c99c](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/bd4c99c810eace9e6c0d1d2f719c92481ed7a1c4))
* **pos:** agregar modificador maestro-detalle de ingredientes con alergias, quitados y extras ([2f619f3](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/2f619f391cd3e0df170788cd6f085c67a9b15658))
* **pos:** agregar propinas y division de cuenta ([79c232a](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/79c232a0f9b6798eeca98274dcaeed97738ea3e0))
* **pos:** agregar vista tableside para tablet ([13d80c8](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/13d80c84f38aa251349b737dea21ff1cef32cecb))
* **pos:** conectar SignalR para sincronizacion entre terminales ([3ab3e7f](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/3ab3e7f5d85ff7e10958fd67a55e19a6cd4a07cb))
* **pos:** mostrar estado visual de mesas con colores ([eb8dbfb](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/eb8dbfb0a7d65de54c05d9c7146050df5e44d727))
* **pos:** reconstruir flujo POS con 3 pantallas SPA, tarjetas táctiles, AJAX sin recargas ([e4a9534](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/e4a95348d396f2a4b035d01d556d72e484d89fe7))
* **pos:** rediseñar flujo de pedidos — radio tipo servicio, mesas solo disponibles, pantalla pago con efectivo/cambio, cambiar tipo mid-orden ([dcf0386](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/dcf03865f72e1aa143ded2cadffb39c0ddeef763))
* **pos:** reemplazar split JS con cuentas reales y SignalR concurrente ([fb29a51](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/fb29a51a518a366647967f65499f2c824c7f375f))
* **signalr:** extender PedidosHub para notificaciones de cocina ([7dc0a3a](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/7dc0a3a44f38c8eca3f30b0dc046aaffa0b5866e))
* **web:** implementar pantalla KDS para cocineros ([baf53b3](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/baf53b3697cc4f7e9efc0fcac3e1b5b2a2127af7))


### Correcciones

* agregar ICuentaRepositorio a IUnidadDeTrabajo y DI ([e909598](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/e90959814d83417b245db073c6ac541aa1227609))
* **integracion:** arreglar 5 tests, conectar Tableside↔POS, agregar Cocina al navbar, verificar flujos ([fd3370e](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/fd3370e8f7c6d5c7472770c90667d0844c0df68b))
* **kds:** agregar 86 agotado con sync SignalR a POS ([8a34702](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/8a34702f0780faa635492fd674e2e76e379de285))
* **kds:** course firing, timer por producto y atajos de teclado ([b9b8828](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/b9b8828e2f661d750cac880a4ca9c2f01e5d992d))
* **kds:** marcarListo idempotente — ReglaDominioException 'Ya está listo' ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **kds:** polling diferencial, alergias visibles con banner rojo ([3ea73ba](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/3ea73baad5c774934853730862fb8df0881cf5fd))
* **kds:** rediseñar tarjeta KDS — banner rojo de alérgenos, nombre de plato ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **kds:** reemplazar alert() con toast no bloqueante en error de listo ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **merge:** restaurar foto menu y modificadores de ingredientes ([5aed773](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/5aed773356b867bf8156be65c6f8669bacaca5d7))
* **merge:** restaurar foto menu y modificadores post split-items ([dc78dbe](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/dc78dbef8ccf0219968a5464092f43162d4f6b84))
* **merge:** restaurar modificadores de ingredientes en pos.js ([dd31b08](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/dd31b08a46ef9bb3e6300d2464b3d9d618d2a512))
* **mesas:** ocultar 'Nueva mesa' y 'Desactivar' del Mesero (BUG-003) ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **nav:** ocultar PEDIDOS del Cocinero en _Layout.cshtml (BUG-002) ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **offline:** permitir pagos en modo offline con cola IndexedDB ([49a86a6](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/49a86a669a75ba92af55cc1955ee7bfe26ad1040))
* **pago:** crear Pago entity al cobrar, agregar auditoria de usuario ([98b8b8e](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/98b8b8e120c26d33df363b5b16810ca3dcb5a52a))
* **pedidos:** pasar notas y modificacionesJson en OnPostAgregarLineaJsonAsync ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **pos:** corregir serializacion JSON a camelCase para mesas y productos en POS y Tableside ([cc74a2a](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/cc74a2a4104f5656bb47b392897c9dd96b2262c9))
* **pos:** corregir serializacion JSON camelCase para datos de mesas y productos ([45f3978](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/45f3978bffa52279ec6d7889221841c060c0bddc))
* **pos:** mostrar capacidad de mesa de forma legible en selector de mesas ([5e21895](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/5e21895c243241d839f1c69426c6f2cf081fa7b9))
* **pos:** reemplazar alert() y confirm() con toast y modal en rama split-items ([eb30a9c](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/eb30a9cfbfa1a03e9e78e5ba041df0e54116f12d))
* **seed:** asignar EstacionCocina por categoría — Entradas→Caliente, ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **seguridad:** restringir POS a Mesero/Encargado/Admin, no Cocinero ([c849fdb](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/c849fdb3e295fda86782713f7b83d619db9a0062))
* **signalr:** corregir nombres de metodos UnirseAGrupo/SalirDeGrupo ([227613e](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/227613eba44cd9959f94650e97cfd12ccced376b))
* **sprint2:** corregir bugs de dogfood — POS state, KDS, RBAC y persistencia ([5c30c0c](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/5c30c0c95739298d540e70dc0fce804c662353fc))
* **sprint2:** KDS glanceability, RBAC nav, modificaciones, fotos de platos ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **tableside:** mismo fix de notas/modificacionesJson en Tableside handler ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **test:** actualizar FakePedidosServicio con nueva firma de AgregarDetalleAsync ([5c37903](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/5c3790351ebc757857f096d29a51ebf0d567c008))
* **test:** corregir tests que fallan en CI por diferencias Windows/Linux ([095d93e](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/095d93eeaaee24c12ebb9f1c26d6d3025450d1e7))
* **tests:** add missing OrdenCocinaRepositorio and CuentaRepositorio to UnidadDeTrabajo constructor calls ([8b0fa69](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/8b0fa696659a1cc09d19460fdcc1557fa7e07323))


### Documentación

* actualizar estado de HU Sprint 1 a Implementado ([bfa6282](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/bfa62827b4387e9700a64043e30d44d19f2abb70))
* add Sprint 2 versioned release notes (v1.1.0–v1.8.0) ([fa30507](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/fa305073ae6ed4dfe4fdea90da57c6a1437ab8a4))
* **investigacion:** documentar analisis competitivo de 30+ sistemas POS ([1d9e62c](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/1d9e62c501d0beddc0d51846d5da39921b9f3bdc))
* **investigacion:** documento canonico exhaustivo de 30+ sistemas POS — 5500 palabras, 25 sistemas, 35 tablas ([93b17c2](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/93b17c2a20399158d0888e0543b0cb76cd0cb7d1))
* **release:** agregar notas curadas de release v2.0.0 Sprint 2 ([b7f4970](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/b7f4970135eca030c7c3920a1f033ec645d6f5ef))


### Pruebas

* **kds:** agregar pruebas de integracion y PageModel ([3e5ee18](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/3e5ee1890e1bb11abd8c30d782c36a7c08d047e3))


### Tareas de mantenimiento

* ignorar archivos de SQLite dev en gitignore ([1ff7cfc](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/1ff7cfcc03acef77b9441950fce28fee75b81a1b))

## [1.0.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.13.0...v1.0.0) (2026-05-09)


### ⚠ BREAKING CHANGES

* primera versión operativa completa del sistema de restaurante. Cubre 8 historias de usuario, frontend operativo de 4 módulos, 207 tests, autenticación BCrypt, autorización por roles, SignalR y documentación curada.

### Nuevas funcionalidades

* **db:** agregar helper de conexión Supabase, script RLS e índices de rendimiento ([29cdbff](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/29cdbff6bae135b6aa9db13b2f15af6fe942a61b))
* **db:** generar script DDL idempotente completo (27 tablas, 821 líneas) ([026b6ed](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/026b6ed3d36c97c0617c1f05b754d8377014ff17))
* verificación integrada y cierre del Sprint 1 — Slice 8 ([f218a39](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/f218a3907d3b48c6d9cffca6331029d50083e960))


### Correcciones

* autorización por rol en navbar y PageModels, SQLite fallback desarrollo, cookies SameAsRequest, seed 4 usuarios ([a57cc8c](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/a57cc8cb84ebff39a8dc4ed90d7a957253e48630))
* **db:** agregar columna Modulo y Descripcion al seed de Permisos ([4fefcce](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/4fefcce9438906cea621a619c188718592dd673e))
* **db:** corregir nombres de columna en RLS (CategoriaId, Fecha) ([4bd0d4c](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/4bd0d4c85878b05303479acf8e07c2774949218d))
* **db:** corregir nombres de política RLS (escapar %I para evitar comillas en nombres) ([b0d3191](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/b0d319115697b857f9024b8820014375d4a29472))
* **db:** corregir nombres de tabla en script RLS para coincidir con EF Core ([a3fc870](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/a3fc870f929c15bb78611d3069ee2d6dd9128e25))
* **db:** corregir UUIDs inválidos (g→0a, h→0b) en seed de Proveedor e Ingrediente ([f9811b5](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/f9811b5ba6a85605c357979dfeaca077b8586995))
* **db:** hacer script RLS idempotente — limpiar políticas existentes antes de crear ([da7dc5d](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/da7dc5da1cce837e33224a40d7b97ef270ed58bd))
* **db:** simplificar ConexionHelper — usar NpgsqlConnectionStringBuilder nativo para URLs pooler ([a7b4b63](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/a7b4b63737d7806b2399e5931f73037040e3fb9f))
* endurecer seguridad y corregir hallazgos críticos pre-1.0.0 ([06ab620](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/06ab62074e20c3a2ec200054cf670c6749ef357e))
* proteger IndexModel contra User null en tests, filtrar módulos por autenticación ([fe7fcfa](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/fe7fcfa74110281bc85192dd41993e5f09d7d18c))
* resolver conflicto de merge en CHANGELOG.md ([70f2e63](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/70f2e63c81896c0334aac42e37592e229719c5a0))
* **ui:** corregir navbar condicional por auth, usar logo canvas transparente, agregar seed data completo ([6f57eb8](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/6f57eb8d635b2e3694474f00533ad914fe613f38))


### Mejoras de rendimiento

* **db:** optimizar políticas RLS con SELECT subquery para evitar re-evaluación por fila ([34d68e5](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/34d68e52336d5eccb646f1766ad0177336151e0e))


### Documentación

* **release:** ampliar notas de las versiones 0.12.0 y 0.13.0 ([b2cf9eb](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/b2cf9ebf1a2b92fc56615771e056dcafb188944a))


### Pruebas

* actualizar tests de infraestructura para SQLite fallback y tests de IndexPage para roles dinámicos ([7ac18f6](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/7ac18f62f5dba0883a274e5f54dce178ae4d9fc4))

## [0.13.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.12.0...v0.13.0) (2026-05-09)

### Summary

- Completa el frontend operativo del Sprint 1 con las consolas de Productos, Usuarios, Mesas y la terminal POS de Pedidos sobre los contratos backend estables.

### Details

- Agrega consola de mantenimiento de Productos con tabla densa, filtros (búsqueda, categoría, estado), CRUD completo con formulario colapsable y botón Editar en cada fila.
- Agrega administración de Usuarios protegida con `[Authorize(Roles = "Administrador")]`, creación y desactivación de usuarios con feedback.
- Agrega superficie de control de Mesas con grid de estado del salón, badges de resumen por estado, transiciones rápidas de estado desde cada tarjeta.
- Agrega terminal POS de Pedidos con split workspace: selección de productos a la izquierda y panel de orden a la derecha, total siempre visible, acciones semánticas (Marcar en Preparación, Pagar, Cancelar).
- Agrega cliente SignalR JS que se conecta a `/hubs/pedidos` y muestra toasts en tiempo real al recibir notificaciones de pedidos.
- Agrega pulido cross-módulo: empty states, toasts, confirmación destructiva, enlace Usuarios en nav para admins, accessibility helpers.
- Expande suite de pruebas de 201 a 207 con tests de PageModel para cada módulo.

### Nuevas funcionalidades

* **ui:** completar frontend operativo Sprint 1 — Slices 7b+7c+7d ([58a7d9d](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/58a7d9d68dd8aab48627f1af04bd9512fe16990a))

## [0.12.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.11.0...v0.12.0) (2026-05-09)

### Summary

- Establece el shell operativo compartido del Sprint 1: navegación por módulos, landing operativo, CSS operacional completo y guardas de autenticación.

### Details

- Reemplaza el layout scaffold por una navbar operativa con tabs de módulos (Productos, Mesas, Pedidos) y estado activo visual.
- Convierte la home page de hero promocional a hub operativo con tarjetas de acceso rápido a los tres flujos del Sprint 1.
- Agrega ~280 líneas de CSS operacional a `marca.css`: shell, page headers, status badges (5 variantes semánticas), acciones (primary/success/danger/neutral), tabla operativa con tabular numerals, toast zone, mesa grid y accessibility helpers.
- Configura `AuthorizeFolder("/Operaciones")` en Program.cs para proteger todas las páginas operativas.
- Agrega página AccesoDenegado para redirección segura.
- Agrega tests de smoke para el shell y la landing page operativa.
- Suite de pruebas: 201 tests, 0 fallas.

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
