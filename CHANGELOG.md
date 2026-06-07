Registro de cambios

Todas las modificaciones notables de este proyecto se documentarán en este archivo.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/),
y este proyecto adhiere al [Versionado Semántico](https://semver.org/lang/es/).
## [1.1.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v1.0.0...v1.1.0) (2026-06-07)


### Nuevas funcionalidades

* **alergenos:** add Allergen entity, product-specific allergen system with seed data ([fd834bd](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/fd834bd6219e0da17eb82878a6499a46075dc9cd))
* **aplicacion:** agregar CocinaServicio y DTOs ([665af8d](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/665af8dff1fe2727c45927ddfb54f68b221a9def))
* **aplicacion:** agregar logica de pago real con cuentas ([8a02fff](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/8a02fff971ad8fd06f0ca0e78edcdc953f648fef))
* avance, correciones de bugs y mejora de workflows ([08a1cca](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/08a1ccaf22f53d561e650535c28ba8fd5dd965c9))
* avance, correciones de bugs y mejora de workflows ([58d50b6](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/58d50b6738899e058b46f27081e77c2ed18bc994))
* **brechas:** §1.3 rol Gerente, §1.5 ticket post-pago, §1.6 ReferenciaPos tarjeta ([b1e721c](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/b1e721c440b577a9f1921823e4a924671f0b949c))
* **brechas:** §1.4 promociones en POS, §1.9 auditoría transversal EF interceptor ([7fb90a6](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/7fb90a6e8f001acae24ee8512093e22e5dccc7f2))
* **brechas:** §2.1 turno de caja, reporte Z ([b45ee62](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/b45ee62c66d02b748f17cda2eee82fbf0500d544))
* **brechas:** §2.2 descuentos/cortesías, §2.3 devoluciones de cobro ([fa65eed](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/fa65eedf993e5e933c2542cca113a2c8c4ab60cd))
* **brechas:** §6.4 errores JSON, §1.2 despacho, §1.7 dashboard gerencial, §1.8 config, §2.7 auditoria UI, §3.3 86 KDS, §5.x UX roles ([0388bfd](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/0388bfd47fd72e4640052f3d5b88479c898e3939))
* **brechas:** delivery, repartidor, registro, reportes admin y ajustes UX/POS ([23a2ef2](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/23a2ef2fac0dcdb750c20c4afa3c816a7d4e704d))
* **cajero:** Fase 2 — POS con vista completa de mesas y cobro directo ([96c0b4c](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/96c0b4c8200b17acbd0d4cd68b9c82f89adbe3f0))
* **cierre:** add day closing page with open/close form and history ([4b4cc8e](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/4b4cc8ed7a6fd890685bd5bac828dc72be43e76a))
* **dashboard:** add IMetricaRepositorio, Pedido.FechaCreacion, and EF data layer ([1b9895d](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/1b9895dc6b15e17052cf0a72bc9517479480fe13))
* **dashboard:** add MetricaServicio, SignalR invalidation, and real-time plumbing ([ff0e515](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/ff0e51598467c894f8af6ddd87bc3ec707c1c73c))
* **dashboard:** add real-time operational dashboard with KPIs and Chart.js ([48a711a](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/48a711acc984acbaaf4d955670a1b69c4c6225e8))
* **dominio:** agregar Cuenta, Pago, MetodoPago y estado EnCobro ([7c64670](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/7c64670e23efb564a75ea67542654c47fc36b297))
* **dominio:** agregar OrdenCocina y EstadoLineaCocina ([bea1e7f](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/bea1e7f0503f5c24ece80739110de85887473d50))
* **dominio:** estado AnuladoPago + flujo de anulación de pago ([89609b1](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/89609b1808ba7c0ae33fcd38ba2b0ea61b82c52f))
* **dominio:** IngredienteReemplazoId Guid + lógica extra/intercambiar en stock ([1b94c6c](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/1b94c6c1f4c3b39a67421135cf169fbb1b48abd9))
* **dominio:** periodo de gracia de mesa y fix tracking DetallePedido ([d7b93ba](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/d7b93ba363d9213d11a79daf8f8a755dd3c85593))
* Fase A+B — migración consolidada, CierreServicio real, seed data, CRUD inventario, despacho, validación POS ([1665aa0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/1665aa07607e85075fe4f2ee297db146dd47a620))
* **fotos:** agregar fotos reales de platos desde Unsplash (libres de uso) ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **infra:** agregar repositorio y migracion de OrdenCocina ([fd852d5](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/fd852d5e96a94a1af908b5211babc3dcc773d9d1))
* **infra:** agregar repositorios y migracion de Cuenta y Pago ([6e51990](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/6e51990bcfc21a0a236c3ddb473cd349fd064928))
* **inv:** add Inventory UI page with tabs for ingredients, suppliers, waste ([de18bbe](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/de18bbe17f666ab0380a25c4513f1edb0de12503))
* **inv:** add stock alerts service, ObtenerTodos on ingredient repo ([20de575](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/20de57510e6d0d0c08307b3dcfb466e01c0ff282))
* **kds:** agregar layout multi-cocinero de 3 columnas con notas y alergenos ([986cc70](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/986cc70fcf73c9f4045486e36f5401db36ecbe68))
* **kds:** full KDS UX overhaul — proper layout, station tabs, legend, new CSS ([5c9a6c5](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/5c9a6c551afee7c3c90174f0b5133b5cc95cb0b5))
* **kds:** workflow overhaul — grouping, undo, escalation, transitions ([47c7145](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/47c7145b14025350a81c1f7875eb73531ba0e79c))
* **loyalty:** add Cliente entity, loyalty points system, rewards catalog ([8757222](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/8757222d467b06899ce9ba5f921030e7dbbfcbfd))
* **margins:** add IMargenServicio for product cost vs selling price analysis ([afeab43](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/afeab43f0c5d60b34f4f57548b883bdf4816a138))
* merge dashboard-reportes (B3b) — MetricaServicio, KPIs real-time, Chart.js dashboard ([aa006dc](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/aa006dce3add5067a5ac0803b194a1812f5606a1))
* merge mapa-visual (B3) — ZonaSalon, posiciones mesa, mapa interactivo + SignalR ([3abd72b](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/3abd72bd2f4b9f5ffa4ac3fbefe8e6ddcfde032f))
* **merma:** add TipoMerma enum, MermaServicio, CierreDia repos, batch tracking ([161351f](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/161351f516e6192839a1a4636446b052b61666d2))
* **merma:** functional waste tab in Inventory page with daily log ([94fc4db](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/94fc4db30f4af5f6ae96006474c5d476ed9de3bb))
* **mesero:** vista tableside del mesero y estado en-gracia en mapa POS ([a891232](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/a8912320f3fce43056777808aeefaab8debb24d3))
* **offline:** implementar modo offline con PWA, cola IndexedDB y polling fallback para KDS ([d63b6d3](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/d63b6d36fc65e714e873d457f8c8407bb2bf84bd))
* **operaciones:** consolidar delivery y flujo operativo ([fc83cee](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/fc83ceea63524c6ff2745967917127426012fc99))
* **operaciones:** consolidar delivery y flujo operativo ([6318371](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/63183717ca71eeb9f9b47f42d6ffd8e67ef7c93f))
* **pos-ux:** Phase 1 — toast, modal, localStorage persistence, Enviar a Cocina ([31c71c9](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/31c71c9c80714f23075ab1dc72c16d91e24cc7f1))
* **pos-ux:** toast, modal, localStorage, Enviar a Cocina button (Phase 1 WIP) ([dd5cef0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/dd5cef0c3ddb0043e6d1e6ade5f3823375bd54dd))
* **pos:** add ingredient modifier modal to 4-screen UX ([561be83](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/561be8379f956e17cf3dbf1bc566723873a52cb1))
* **pos:** agregar atajos de teclado, indicador de pasos y feedback visual ([ecf3689](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/ecf368983020fb12e554338a063ef5061efed813))
* **pos:** agregar division por items entre cuentas ([bd4c99c](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/bd4c99c810eace9e6c0d1d2f719c92481ed7a1c4))
* **pos:** agregar modificador maestro-detalle de ingredientes con alergias, quitados y extras ([2f619f3](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/2f619f391cd3e0df170788cd6f085c67a9b15658))
* **pos:** agregar propinas y division de cuenta ([79c232a](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/79c232a0f9b6798eeca98274dcaeed97738ea3e0))
* **pos:** agregar vista tableside para tablet ([13d80c8](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/13d80c84f38aa251349b737dea21ff1cef32cecb))
* **pos:** bring 4-screen UX overhaul to current branch ([772d9fa](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/772d9fa49cadd1895c0aea5a6a250ab8f31e6ac6))
* **pos:** conectar SignalR para sincronizacion entre terminales ([3ab3e7f](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/3ab3e7f5d85ff7e10958fd67a55e19a6cd4a07cb))
* **pos:** modal ingredientes con extra/intercambiar ([c263801](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/c26380163ff3623d90bf3979a74355c50883e216))
* **pos:** mostrar estado visual de mesas con colores ([eb8dbfb](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/eb8dbfb0a7d65de54c05d9c7146050df5e44d727))
* **pos:** reconstruir flujo POS con 3 pantallas SPA, tarjetas táctiles, AJAX sin recargas ([e4a9534](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/e4a95348d396f2a4b035d01d556d72e484d89fe7))
* **pos:** rediseñar flujo de pedidos — radio tipo servicio, mesas solo disponibles, pantalla pago con efectivo/cambio, cambiar tipo mid-orden ([dcf0386](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/dcf03865f72e1aa143ded2cadffb39c0ddeef763))
* **pos:** reemplazar split JS con cuentas reales y SignalR concurrente ([fb29a51](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/fb29a51a518a366647967f65499f2c824c7f375f))
* **pos:** rewrite POS UX as state machine with floating overlays ([a7d5e2e](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/a7d5e2efed9d7ea7e4e1c3e9f17ee13cb353ebf2))
* **pos:** split por persona y mixto con tabla de asignación de ítems ([4b5f24e](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/4b5f24efd47218ed921010e9e5408ae9d077a279))
* **pos:** tab abierto, alérgenos como modificadores y cocina parcial ([0741780](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/0741780a3c5adec05799a257218dcc88176c4370))
* **pos:** UX overhaul fullscreen 4 pantallas — selección, productos, pago, documentos ([d4e6f20](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/d4e6f20dc9096926ccc08701926bc7adb0448ce4))
* require closing note for cash discrepancies (Task 7) ([72b2295](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/72b2295da4d4f1c0a021a87edfb1187e368fc115))
* **roles:** Fase 1 — rol Cajero, sidebar gestión y layout por rol ([7d513e0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/7d513e02dd97a0f3dc01f1fe2db737fd0da9e1ee))
* **salon:** add visual floor map with drag-drop, urgency colors, and real-time sync ([78d344d](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/78d344dd1d3969a8387d456511c8a6861fc44889))
* **salon:** add ZonaSalon aggregate and Mesa position fields ([a837d39](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/a837d392ae19e55ab99f410d2d86826a9001eff2))
* **salon:** add zone services, mesa position orchestration, and SignalR notification ([46e803f](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/46e803f52dc953d667fd549e9ae68abd04f7990e))
* **shift:** add ShiftHandoffServicio for mesa transfer between waiters ([7bc8cb5](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/7bc8cb55f38cdd9ab148be0f2bef95bd1e32683b))
* **signalr:** extender PedidosHub para notificaciones de cocina ([7dc0a3a](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/7dc0a3a44f38c8eca3f30b0dc046aaffa0b5866e))
* **slice10:** add mesero table handoff baseline ([a2ae278](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/a2ae278ddf1fd0a583e1b08981c2da68748fb488))
* **sprint3:** Sprint 3 completo — Cajero, Mesero, EnCobro, gracia de mesa, Npgsql ([3e727fa](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/3e727fa05f780a32e917d89813445eaa587a31f1))
* **sprint3:** stock al enviar cocina, tab activo, split server, nota libre, documento ([2165a4d](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/2165a4d9eda8559f71f0096479d70cad7dea18aa))
* **ticket:** add HTML ticket generator with thermal-printer-ready design ([219cb03](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/219cb03ed631ebd7491cc0168d756d74fb1ce338))
* **timers:** add OcupadaDesde to Mesa, TableTimerServicio for &gt;90min alerts ([65494cd](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/65494cd797b17f9943a0d21efaac81fb0ea35fc5))
* **upsell:** add UpsellServicio for postre/bebida suggestions ([4b392bf](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/4b392bf4b7442633f8c98f83842b1d522d9f39cd))
* **web:** implementar pantalla KDS para cocineros ([baf53b3](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/baf53b3697cc4f7e9efc0fcac3e1b5b2a2127af7))


### Correcciones

* add formal migration for audit4 persistence changes ([570adf0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/570adf0d2670adc9d4e089cc78fee4c9b820b8f3))
* agregar ICuentaRepositorio a IUnidadDeTrabajo y DI ([e909598](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/e90959814d83417b245db073c6ac541aa1227609))
* align home navigation, remove inventory subtitle, add all modules by role ([8bb4017](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/8bb401717da412cd1c8c425da55d800fa0b35e1e))
* **audit2:** merma+cierre domain violations and data integrity ([83917ee](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/83917ee81df4ad7b352b762b4a3b6456249d669e))
* **audit3:** CreatedAt on Pedido, cash reconciliation model, MermaServicio+CierreServicio tests ([f02c3e6](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/f02c3e6c88e371b6ffb171a18429ed4cc0992a63))
* **audit4:** resolve all final audit blockers ([594a198](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/594a1982483c3773883856803f8054950fe10cc9))
* **audit:** apply 7 critical fixes from full system audit ([90ecd48](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/90ecd48ca83fc2bae7574aa924bd0389e652c477))
* **auditoria:** cerrar bugs P0/P1 pendientes y reconciliar evidencia de Sprint 1/2 ([d2f3152](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/d2f3152600b10ee66fd8322bd36a5d5bfe09b976))
* **audit:** restaurar 15 registros DI perdidos en merge, agregar Despacho/Dashboard/Mapa a nav ([1123726](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/1123726ca322fab11d6fcb29b5d87538c62855bc))
* **audit:** try/catch Dashboard, agregar Tableside al sidebar, informe de auditoría ([b1405c2](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/b1405c2a72ff2b5ef037b0fdf36c0ba1715e5f59))
* avoid leaking internal exception messages in JSON handlers ([0d0bc5b](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/0d0bc5b0d9800008a9932a395aa9429b9329fa35))
* Cierre simplificado, pedidos van directo a cocina al pagar ([2c73929](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/2c73929fa625d6af2ea73420bf355cda8e2e50d8))
* CierreDia nullable Usuario, CRUD forms in Inventario, NIT, master-detail ([4164a0a](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/4164a0ae84eff3c948f3c542f318d4c159b54c69))
* **cierre:** evitar reabrir dia operativo cerrado ([9cf1d42](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/9cf1d42e9a68462fb170acd47d6d1dce77fa6050))
* **cierre:** evitar reabrir dia operativo cerrado ([118ca7d](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/118ca7d59cb447646ba3ed50de159745ce1ee561))
* close audit remediation and stock integrity ([dc8131b](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/dc8131b7d13b694a168411db518ef6882a517011))
* critical runtime errors — POS crash protection, stack traces, roles, stock ([6cdaf3a](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/6cdaf3a2f766b184b3a35dd2ec8bd3f6d3bc0c92))
* **infraestructura:** excluir AnuladoPago de métricas y consultas de mesa ([1e62344](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/1e6234403ddfa44f7aa2edb891f5e20e3dfa9e3a))
* **infra:** migrar EF Core de SQLite a Npgsql ([38f877c](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/38f877ce156f4a09cee01b798982e3b91f1517a2))
* **integracion:** arreglar 5 tests, conectar Tableside↔POS, agregar Cocina al navbar, verificar flujos ([fd3370e](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/fd3370e8f7c6d5c7472770c90667d0844c0df68b))
* **kds:** agregar 86 agotado con sync SignalR a POS ([8a34702](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/8a34702f0780faa635492fd674e2e76e379de285))
* **kds:** course firing, timer por producto y atajos de teclado ([b9b8828](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/b9b8828e2f661d750cac880a4ca9c2f01e5d992d))
* **kds:** marcarListo idempotente — ReglaDominioException 'Ya está listo' ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **kds:** maximize font sizes, sections separated by borders — impossible to miss ([7b000dd](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/7b000dd482eb25924ba2ffd27bf959f371b9dd81))
* **kds:** polling diferencial, alergias visibles con banner rojo ([3ea73ba](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/3ea73baad5c774934853730862fb8df0881cf5fd))
* **kds:** rediseñar tarjeta KDS — banner rojo de alérgenos, nombre de plato ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **kds:** reemplazar alert() con toast no bloqueante en error de listo ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **kds:** UX overhaul — larger fonts, legible notes, Lucide icons ([c76b852](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/c76b852c8f72921ecbb1f721198612daaab290c1))
* make local development database reproducible and remove secrets ([e32002d](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/e32002d6b985dec8d068eb04d346213c7ba3b300))
* **merge:** restaurar foto menu y modificadores de ingredientes ([5aed773](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/5aed773356b867bf8156be65c6f8669bacaca5d7))
* **merge:** restaurar foto menu y modificadores post split-items ([dc78dbe](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/dc78dbef8ccf0219968a5464092f43162d4f6b84))
* **merge:** restaurar modificadores de ingredientes en pos.js ([dd31b08](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/dd31b08a46ef9bb3e6300d2464b3d9d618d2a512))
* **mesas:** ocultar 'Nueva mesa' y 'Desactivar' del Mesero (BUG-003) ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **mesero:** mejorar navegacion y solicitud de cuenta ([f325ff2](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/f325ff23c8f8ed5cb8510b6e7a6ed6514a62e628))
* **mesero:** mejorar navegacion y solicitud de cuenta ([b06efdb](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/b06efdb02a605601bdcefcda85363fec31ce465c))
* **nav:** ocultar PEDIDOS del Cocinero en _Layout.cshtml (BUG-002) ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* normalize Spanish text encoding and prevent mojibake regressions ([ae30af2](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/ae30af27974217bcbbef3180de6a367a71029ef0))
* **offline:** permitir pagos en modo offline con cola IndexedDB ([49a86a6](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/49a86a669a75ba92af55cc1955ee7bfe26ad1040))
* **pago:** crear Pago entity al cobrar, agregar auditoria de usuario ([98b8b8e](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/98b8b8e120c26d33df363b5b16810ca3dcb5a52a))
* **pedidos:** pasar notas y modificacionesJson en OnPostAgregarLineaJsonAsync ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **pedidos:** rescue ingredient validation and kds guards ([ae050c8](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/ae050c84990ba218349ad6dbfffe20e65c24b991))
* **pedidos:** rescue ingredient validation and kds guards ([3d0b0dd](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/3d0b0dda8615e3d977ec854f27a6b30cd2575b65))
* **pos:** add GET handler for ingredient modifier modal ([3843980](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/384398061bae97ba33ecaa573be6bd473c41268d))
* **pos:** add ingredient visibility endpoint and recipe lookup in POS ([03eeb16](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/03eeb16398e247df7b2549e500b39533487017df))
* **pos:** agregar height explícita a lmd-main-surface para que el POS renderice ([fbd640a](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/fbd640a83203033007c6b467d73906e1d6a95a31))
* **pos:** arreglar flujo pago, SignalR, refresco mesas, fix Tableside PrecioUnitario y toast ([b78727c](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/b78727ca40fe82975ae5ecd1c447f5b0a0436ab7))
* **pos:** cancelación en servidor + persistencia de modificadores de ingredientes ([7d54184](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/7d54184ab2ae6c817a386d5969e74764f89a7459))
* **pos:** carrito bloqueado post-pago + modificaciones en form + estilos cambiar servicio ([c171a65](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/c171a65ac4fa584bd6e91910a6b58e7250893edb))
* **pos:** corregir 6 bugs de estabilidad en el workflow de pedidos ([a42eb44](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/a42eb44faeabfc642582ce60fa6682c7befc1a93))
* **pos:** corregir serializacion JSON a camelCase para mesas y productos en POS y Tableside ([cc74a2a](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/cc74a2a4104f5656bb47b392897c9dd96b2262c9))
* **pos:** corregir serializacion JSON camelCase para datos de mesas y productos ([45f3978](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/45f3978bffa52279ec6d7889221841c060c0bddc))
* **pos:** error de sintaxis JS, botón pagar y total de tab ([69a01b0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/69a01b060b63b7a04d5fd5cba40bb061724dcb8d))
* **pos:** flujo de pago completo, SignalR real-time, refresco de mesas, validación server ([be5599a](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/be5599aa697aba5ca765e11da64f1a0a395b9a64))
* **pos:** make Para llevar section fully clickable, ensure state transitions work ([b7fdaf9](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/b7fdaf973a51314779efaebe5aea9af149839369))
* **pos:** mostrar capacidad de mesa de forma legible en selector de mesas ([5e21895](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/5e21895c243241d839f1c69426c6f2cf081fa7b9))
* **pos:** reemplazar alert() y confirm() con toast y modal en rama split-items ([eb30a9c](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/eb30a9cfbfa1a03e9e78e5ba041df0e54116f12d))
* **pos:** refrescar mapa en EnCobro, polling periódico y MarcarEnCobro al retomar tab ([d0520a8](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/d0520a8babf70c17cc8a054716c48b231c2dc9dc))
* **pos:** replace all remaining emojis with Lucide SVG icons ([c98b1c2](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/c98b1c2fc52c1579a4154e33879589a56bf7737f))
* **pos:** send orders to kitchen on Listo/Pagar — POST CrearJson + PagarJson ([8cd4752](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/8cd4752f8a4a3fedc1520601cfe7d5c0ef468100))
* release tables only on dispatch (Task 5) ([43b8ce5](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/43b8ce5332203e670d8cc46fafda80742210838f))
* require user traceability for payments (Task 6) ([96a3d76](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/96a3d76c7379c58b976a23fe9a662d710b06dc3e))
* **security:** remove legacy aspnetcore abstractions ([d449fe3](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/d449fe372b93392f4b06aa057122e1ea8b879534))
* **seed:** asignar EstacionCocina por categoría — Entradas→Caliente, ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **seguridad:** restringir POS a Mesero/Encargado/Admin, no Cocinero ([c849fdb](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/c849fdb3e295fda86782713f7b83d619db9a0062))
* **signalr:** corregir nombres de metodos UnirseAGrupo/SalirDeGrupo ([227613e](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/227613eba44cd9959f94650e97cfd12ccced376b))
* **slice0:** restore foundation verification ([17f1e10](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17f1e102f0c4e43d97e198734237e79e5a00b5a7))
* **slice10:** expose mesero handoff navigation ([24d71e4](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/24d71e4e3a09d2c6de6d7fc06374f50ecdc527e0))
* **slice11:** align mesero navigation access ([3de781b](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/3de781b9b0ff4aa0532c042a976f3c310a4b4bd5))
* **slice12:** enforce caja day reconciliation ([0ac30f8](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/0ac30f89135de632a1fcbffb3d2d4dcf9d3a1133))
* **slice13:** connect floor map to table catalog ([aae248a](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/aae248af2dc45f38a8eb2fe128d98c036c8a806c))
* **slice14:** harden product save feedback ([15d0689](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/15d0689c7ec384913a2299ea18b8df12c7770fab))
* **slice15:** correct despacho elapsed time ([c6d6209](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/c6d6209d88fec3146d8ac0e2c3a20f8e734608a0))
* **slice16:** polish admin shell navigation ([88784f3](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/88784f3641ef9be8673d93f9388ee8b7a8001b55))
* **slice1:** harden lifecycle and cashier shift flow ([e3f0cef](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/e3f0ceffe5e310db9581bb131639a400a25180b9))
* **slice2:** harden kds runtime rendering ([304bb7d](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/304bb7dc5f3099c6dc535a7318e8439a63e0e232))
* **slice3:** add security headers baseline ([1c526e1](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/1c526e158f9140e9bd99c5d63c0c2c20f3b9a9ff))
* **slice4:** integrate reportes workflow ([5e499b4](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/5e499b470c0bd94e5269a2ee5e28290eb3b98f37))
* **slice5:** harden mesero payment traceability ([12966bb](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/12966bbe453a5b0c70956282167e11429c8ad6cf))
* **slice6:** escape pos and mesero render data ([eee8580](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/eee8580cae1fe118e6503cbf30ac96cd0796dd21))
* **slice7:** add dedicated despacho role ([4df3c32](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/4df3c32f80dcd9af67675fdb27a999af61dbbccc))
* **slice8:** localize critical vendor assets ([73916b3](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/73916b371d51b06d4f2984f0e6c5205fba881b6b))
* **slice9:** harden ui exception messages ([26c233b](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/26c233bc52d3afcf7d2efe03c957fa3f60acdd6b))
* **sprint2:** corregir bugs de dogfood — POS state, KDS, RBAC y persistencia ([5c30c0c](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/5c30c0c95739298d540e70dc0fce804c662353fc))
* **sprint2:** KDS glanceability, RBAC nav, modificaciones, fotos de platos ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **stock:** enforce recipe inventory guards ([564a5ec](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/564a5ec65411b4900ac9a5fb0e4c4fc71a6ea2e6))
* **tableside:** mismo fix de notas/modificacionesJson en Tableside handler ([17a0a60](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/17a0a604c76818e3d749fb7f03d063c9ef907c4c))
* **test:** actualizar FakePedidosServicio con nueva firma de AgregarDetalleAsync ([5c37903](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/5c3790351ebc757857f096d29a51ebf0d567c008))
* **test:** corregir tests que fallan en CI por diferencias Windows/Linux ([095d93e](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/095d93eeaaee24c12ebb9f1c26d6d3025450d1e7))
* **tests:** add missing OrdenCocinaRepositorio and CuentaRepositorio to UnidadDeTrabajo constructor calls ([8b0fa69](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/8b0fa696659a1cc09d19460fdcc1557fa7e07323))
* **ux:** bugs y mejoras de flujo de trabajo post-auditoría ([ebbe7b4](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/ebbe7b413955361566dd63712473bf73ca6e8259))
* **web:** exponer ReglaDominioException y corregir ordenamiento de tab ([66fe5f8](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/66fe5f85526d47373f552d8b2c2da783d1ecb8ad))


### Documentación

* actualizar estado de HU Sprint 1 a Implementado ([bfa6282](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/bfa62827b4387e9700a64043e30d44d19f2abb70))
* add closing plan — temporary fixes, missing features, action items ([af8eb1a](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/af8eb1a0d50c9cedea52641fd366608c65e1c599))
* add Sprint 2 versioned release notes (v1.1.0–v1.8.0) ([fa30507](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/fa305073ae6ed4dfe4fdea90da57c6a1437ab8a4))
* **audit:** close slice remediation batch ([d9e36c4](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/d9e36c4878b3dda695f489578722744881b5f1a4))
* **final:** add defense release package ([a250668](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/a250668bbd7ea512eeb570efca619c38862d6bb9))
* **investigacion:** documentar analisis competitivo de 30+ sistemas POS ([1d9e62c](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/1d9e62c501d0beddc0d51846d5da39921b9f3bdc))
* **investigacion:** documento canonico exhaustivo de 30+ sistemas POS — 5500 palabras, 25 sistemas, 35 tablas ([93b17c2](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/93b17c2a20399158d0888e0543b0cb76cd0cb7d1))
* plan de continuidad Sprint 3 ([a54ad4e](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/a54ad4ec7d8dea7a6c229a84b930104c217317c8))
* **release:** agregar notas curadas de release v2.0.0 Sprint 2 ([b7f4970](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/b7f4970135eca030c7c3920a1f033ec645d6f5ef))
* **slice9:** add safe ui exception plan ([36a39aa](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/36a39aa24012e6943b76f85e51d847decb93c1fc))


### Pruebas

* implementar AnularPagoAsync en FakePedidosServicio ([23b82d8](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/23b82d8b08a9a2793d7bbf694f9f10edb84479a4))
* **kds:** agregar pruebas de integracion y PageModel ([3e5ee18](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/3e5ee1890e1bb11abd8c30d782c36a7c08d047e3))


### Tareas de mantenimiento

* agregar qa-output al gitignore ([d175103](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/d175103182be0a5f6683181bb336511ce8896c9a))
* ignorar archivos de SQLite dev en gitignore ([1ff7cfc](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/1ff7cfcc03acef77b9441950fce28fee75b81a1b))
* merge latest main into rescue branch ([fcd09af](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/fcd09af497df0b159d8a7496a79ab240259123bb))
* merge main into rescue branch ([1fa0b01](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/1fa0b0154793b0f9ba7465e39b1c93dcbe3a8e0d))


### Compilación y despliegue

* 0 errors, 0 warnings. Tests: 283/283. ([4164a0a](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/4164a0ae84eff3c948f3c542f318d4c159b54c69))
* 0 errors, 0 warnings. Tests: 295/295. ([2c73929](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/2c73929fa625d6af2ea73420bf355cda8e2e50d8))
* 0 errors. ([8cd4752](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/8cd4752f8a4a3fedc1520601cfe7d5c0ef468100))
* 0 errors. Tests: 271/271. ([6cdaf3a](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/6cdaf3a2f766b184b3a35dd2ec8bd3f6d3bc0c92))
* 0 errors. Tests: 271/271. ([4b4cc8e](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/4b4cc8ed7a6fd890685bd5bac828dc72be43e76a))
* 0 errors. Tests: 271/271. ([94fc4db](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/94fc4db30f4af5f6ae96006474c5d476ed9de3bb))
* 0 errors. Tests: 271/271. ([161351f](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/161351f516e6192839a1a4636446b052b61666d2))
* 0 errors. Tests: 271/271. ([47c7145](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/47c7145b14025350a81c1f7875eb73531ba0e79c))
* 0 errors. Tests: 271/271. ([7b000dd](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/7b000dd482eb25924ba2ffd27bf959f371b9dd81))
* 0 errors. Tests: 271/271. ([5c9a6c5](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/5c9a6c551afee7c3c90174f0b5133b5cc95cb0b5))
* 0 errors. Tests: 271/271. ([18762bc](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/18762bc96e149b1c31f64ab6e3ffca0b72f95d07))
* 0 errors. Tests: 271/271. ([afeab43](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/afeab43f0c5d60b34f4f57548b883bdf4816a138))
* 0 errors. Tests: 271/271. ([de18bbe](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/de18bbe17f666ab0380a25c4513f1edb0de12503))
* 0 errors. Tests: 271/271. ([8757222](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/8757222d467b06899ce9ba5f921030e7dbbfcbfd))
* 0 errors. Tests: 271/271. ([7bc8cb5](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/7bc8cb55f38cdd9ab148be0f2bef95bd1e32683b))
* 0 errors. Tests: 271/271. ([4b392bf](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/4b392bf4b7442633f8c98f83842b1d522d9f39cd))
* 0 errors. Tests: 271/271. ([65494cd](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/65494cd797b17f9943a0d21efaac81fb0ea35fc5))
* 0 errors. Tests: 271/271. ([c98b1c2](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/c98b1c2fc52c1579a4154e33879589a56bf7737f))
* 0 errors. Tests: 271/271. ([fd834bd](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/fd834bd6219e0da17eb82878a6499a46075dc9cd))
* 0 errors. Tests: 271/271. ([c76b852](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/c76b852c8f72921ecbb1f721198612daaab290c1))
* 0 errors. Tests: 271/271. ([3843980](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/384398061bae97ba33ecaa573be6bd473c41268d))
* 0 errors. Tests: 271/271. ([219cb03](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/219cb03ed631ebd7491cc0168d756d74fb1ce338))
* 0 errors. Tests: 271/271. ([20de575](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/20de57510e6d0d0c08307b3dcfb466e01c0ff282))
* 0 errors. Tests: 283/283. ([561be83](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/561be8379f956e17cf3dbf1bc566723873a52cb1))
* 0 errors. Tests: 283/283. ([772d9fa](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/772d9fa49cadd1895c0aea5a6a250ab8f31e6ac6))
* 0 errors. Tests: 295/295. ([8bb4017](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/8bb401717da412cd1c8c425da55d800fa0b35e1e))


### Estilo de código

* reemplazar emojis y caracteres unicode por iconos Lucide ([28c2aa3](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/28c2aa3942d6459abad81147fff48c4923c5aaa9))


### Revertidos

* remove loyalty/CRM system — requires user data not available ([18762bc](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/18762bc96e149b1c31f64ab6e3ffca0b72f95d07))

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
