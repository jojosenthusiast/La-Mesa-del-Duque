# Backlog Jira — defensa final v3.x

Este documento convierte las historias adicionales y brechas críticas en tickets listos para Jira. La intención es que el equipo pueda crear épicas, historias y tareas sin volver a interpretar el sistema desde cero.

## Convención recomendada para Jira

| Campo | Valor recomendado |
|---|---|
| Proyecto | SDR / La Mesa del Duque |
| Versión objetivo | `v3.0.0` / `Sprint defensa final` |
| Labels globales | `defensa-final`, `v3`, `restaurante`, `calidad-software` |
| Prioridades | `P0` bloquea defensa; `P1` mejora demostración; `P2` evidencia/documentación |
| Definition of Done | Implementado, probado, visible en español, documentado y con evidencia de captura o test |

## Épica 1 — Adaptabilidad a modelos de negocio gastronómico

**Objetivo:** demostrar que el sistema puede operar y explicarse para los 4 modelos exigidos: comida rápida, restaurante con mesas, para llevar y delivery/despacho.

### SDR-126 · Operar flujo de comida rápida / mostrador

**Tipo:** Historia
**Prioridad:** P1
**Labels:** `adaptabilidad`, `comida-rapida`, `pos`, `defensa-final`
**Release:** `3.0.0-alpha.1`

**Descripción:**
Como Cajero, quiero procesar pedidos de mostrador sin mesa y con entrega inmediata, para que el sistema pueda adaptarse a un modelo de comida rápida.

**Criterios de aceptación:**

- Puedo crear un pedido sin mesa asociada.
- El pedido queda identificado visualmente como mostrador/comida rápida o equivalente operativo.
- El flujo Cajero → Cocina → Despacho funciona sin depender de mapa de mesas.
- El ticket muestra el tipo de servicio correcto.
- La defensa puede explicar este flujo en la matriz de configuración del negocio.

### SDR-127 · Diferenciar Para llevar de Delivery

**Tipo:** Historia
**Prioridad:** P0
**Labels:** `delivery`, `despacho`, `tipo-servicio`, `defensa-final`
**Release:** `3.0.0-alpha.1`

**Descripción:**
Como Cajero, quiero distinguir explícitamente entre pedidos para llevar y pedidos delivery, para que el sistema no mezcle retiro por cliente con entrega a domicilio.

**Criterios de aceptación:**

- El POS muestra opciones separadas para `Comer aquí`, `Para llevar` y `Delivery`.
- Un pedido `Para llevar` no exige dirección ni repartidor.
- Un pedido `Delivery` exige datos mínimos de contacto y dirección.
- Cocina y despacho muestran la modalidad correcta.
- El ticket digital imprime la modalidad correcta.

### SDR-128 · Registrar datos mínimos de Delivery

**Tipo:** Historia
**Prioridad:** P0
**Labels:** `delivery`, `cliente`, `direccion`, `rubrica`
**Release:** `3.0.0-alpha.1`

**Descripción:**
Como Cajero, quiero registrar nombre, teléfono y dirección del cliente en pedidos delivery, para que despacho pueda entregar al destinatario correcto.

**Criterios de aceptación:**

- En modalidad Delivery, nombre del cliente, teléfono y dirección son obligatorios.
- El sistema valida campos vacíos con mensajes seguros y claros.
- Los datos aparecen en despacho.
- Los datos aparecen en el ticket o comprobante interno de entrega.
- Los datos no se solicitan para mesa ni para llevar.

### SDR-129 · Gestionar pedidos Delivery en despacho

**Tipo:** Historia
**Prioridad:** P0
**Labels:** `delivery`, `despacho`, `estado-pedido`
**Release:** `3.0.0-alpha.1`

**Descripción:**
Como Personal de Despacho, quiero ver claramente cuáles pedidos son delivery y marcarlos como entregados, para controlar la salida y cierre de entregas a domicilio.

**Criterios de aceptación:**

- Despacho diferencia `Para llevar` de `Delivery` con etiqueta visual clara.
- Los pedidos delivery muestran datos de entrega.
- El flujo mínimo permite marcar `Listo` → `Despachado/Entregado`.
- La acción queda registrada en el historial de estados.
- Si existe mesa asociada por error, el sistema impide esa combinación.

### SDR-130 · Matriz de configuración de negocio

**Tipo:** Tarea documental
**Prioridad:** P2
**Labels:** `documentacion`, `matriz-negocio`, `entrega-final`
**Release:** `3.0.0-rc.1`

**Descripción:**
Documentar cómo el software se adapta a los 4 modelos solicitados por la rúbrica.

**Criterios de aceptación:**

- La matriz incluye: comida rápida, restaurante mesa, para llevar y delivery/despacho.
- Cada modelo indica configuración, roles involucrados, flujo operativo y evidencia del sistema.
- La matriz diferencia capacidad implementada, capacidad demostrable y mejoras futuras si aplica.
- El contenido está listo para PDF final.

## Épica 2 — Stock, mercadería e integridad de datos

**Objetivo:** cumplir el criterio de rúbrica: si el stock de un ingrediente o platillo llega a cero, el sistema alerta y bloquea venta en caja inmediatamente.

### SDR-131 · Bloquear venta por stock insuficiente en caja

**Tipo:** Historia
**Prioridad:** P0
**Labels:** `stock`, `pos`, `bloqueo-venta`, `integridad`
**Release:** `3.0.0-alpha.2`

**Descripción:**
Como Cajero, quiero que el POS bloquee inmediatamente un producto sin stock suficiente, para evitar vender platos que cocina no puede preparar.

**Criterios de aceptación:**

- Antes de confirmar pago, el sistema valida receta e inventario disponible.
- Si falta un ingrediente, el pago se rechaza.
- El mensaje indica el producto afectado y el ingrediente faltante.
- El producto agotado queda visualmente no vendible en POS.
- No se genera pedido pagado ni comanda de cocina cuando falla la validación.

### SDR-132 · Impedir stock negativo por regla de dominio y base de datos

**Tipo:** Historia técnica
**Prioridad:** P0
**Labels:** `stock`, `base-datos`, `invariantes`, `calidad`
**Release:** `3.0.0-alpha.2`

**Descripción:**
Como sistema, quiero impedir stock negativo tanto en código como en base de datos, para preservar la integridad aunque existan errores o concurrencia.

**Criterios de aceptación:**

- Ningún método de dominio permite dejar `StockActual < 0`.
- La base de datos tiene constraint o protección equivalente para impedir stock negativo.
- Los errores se muestran al usuario como mensajes seguros en español.
- Existe prueba automatizada que intenta descontar más stock del disponible y falla correctamente.

### SDR-133 · Controlar concurrencia de stock entre múltiples cajeros/meseros

**Tipo:** Historia técnica
**Prioridad:** P0
**Labels:** `concurrencia`, `stock`, `transaccion`, `pos`
**Release:** `3.0.0-alpha.2`

**Descripción:**
Como sistema, quiero que dos ventas simultáneas no puedan consumir el mismo stock restante, para evitar sobreventa durante un turno real.

**Criterios de aceptación:**

- La validación y el descuento de inventario ocurren dentro de una operación atómica.
- Dos pedidos concurrentes que compiten por el último ingrediente no dejan stock negativo.
- Uno de los pedidos se confirma y el otro recibe error controlado, o ambos se gestionan según stock real disponible.
- Existe prueba de concurrencia o integración que cubre el escenario.

### SDR-136 · Alertar producto agotado o ingrediente crítico en POS

**Tipo:** Historia
**Prioridad:** P0
**Labels:** `alertas`, `pos`, `stock`, `ux`
**Release:** `3.0.0-alpha.2`

**Descripción:**
Como Cajero, quiero ver alertas de productos agotados o ingredientes críticos desde caja, para informar al cliente antes de cobrar.

**Criterios de aceptación:**

- El POS muestra estado agotado/no disponible por producto.
- La alerta se actualiza después de ventas que consumen stock.
- Los productos sin stock no pueden agregarse o no pueden confirmarse.
- La explicación es clara y no técnica.

### SDR-137 · Registrar movimientos de inventario

**Tipo:** Historia técnica
**Prioridad:** P1
**Labels:** `inventario`, `auditoria`, `stock`, `reportes`
**Release:** `3.0.0-alpha.2`

**Descripción:**
Como Encargado, quiero que cada entrada, salida, merma y ajuste de inventario quede registrado, para auditar el uso real de mercadería.

**Criterios de aceptación:**

- Cada movimiento registra ingrediente, cantidad, tipo de movimiento, usuario, fecha y referencia.
- Las ventas generan salidas por receta.
- La merma genera salida con razón.
- Las compras recibidas generan entrada.
- El historial es consultable por Encargado/Gerente/Administrador.

## Épica 3 — Caja, turnos y cierre de día

**Objetivo:** hacer que caja y cierre sean defendibles: responsables, turnos, efectivo/tarjeta y merma deben alinearse.

### SDR-139 · Abrir día operativo

**Tipo:** Historia
**Prioridad:** P1
**Labels:** `caja`, `cierre-dia`, `turnos`
**Release:** `3.0.0-alpha.3`

**Descripción:**
Como Encargado, quiero abrir un día operativo antes de vender, para que ventas, turnos y cierres queden agrupados correctamente.

**Criterios de aceptación:**

- Solo existe un día operativo abierto por restaurante.
- El sistema permite registrar fecha, responsable y observaciones.
- Los pedidos del día se asocian al día operativo abierto.
- El sistema muestra error si se intenta vender sin día abierto, o abre uno de forma controlada según decisión de diseño.

### SDR-140 · Abrir turno de caja

**Tipo:** Historia
**Prioridad:** P1
**Labels:** `caja`, `turnos`, `responsabilidad`
**Release:** `3.0.0-alpha.3`

**Descripción:**
Como Cajero o Encargado, quiero abrir un turno con monto inicial y responsable, para controlar entradas y salidas de dinero durante la jornada.

**Criterios de aceptación:**

- El turno registra responsable, fecha/hora inicio y monto inicial.
- Los pagos quedan asociados al turno activo.
- No se puede tener más de un turno abierto por usuario/caja si el modelo lo restringe.
- El dashboard/cierre puede consultar ventas por turno.

### SDR-141 · Cerrar turno de caja

**Tipo:** Historia
**Prioridad:** P1
**Labels:** `caja`, `turnos`, `conciliacion`
**Release:** `3.0.0-alpha.3`

**Descripción:**
Como Encargado, quiero cerrar un turno comparando efectivo esperado, efectivo contado y pagos con tarjeta, para identificar diferencias de caja.

**Criterios de aceptación:**

- El cierre de turno muestra efectivo esperado, tarjeta, total ventas, devoluciones y diferencia.
- El usuario ingresa efectivo contado.
- Si hay diferencia, la razón es obligatoria.
- El cierre queda auditado.

### SDR-142 · Cierre de día con turnos, caja y merma

**Tipo:** Historia
**Prioridad:** P1
**Labels:** `cierre-dia`, `merma`, `reportes`, `admin`
**Release:** `3.0.0-alpha.3`

**Descripción:**
Como Encargado, quiero que el cierre del día consolide turnos, ventas, pagos, pedidos y merma, para obtener un resumen operativo real.

**Criterios de aceptación:**

- El cierre del día no es solo visual: consolida datos reales.
- Muestra ventas efectivo/tarjeta, pedidos procesados, devoluciones, merma valorizada y diferencias de caja.
- Incluye responsables de apertura/cierre y turnos.
- No permite cerrar el día si hay pedidos activos sin resolver, salvo confirmación con razón.

## Épica 4 — Usabilidad, roles y manejo de errores

**Objetivo:** que cada rol vea solo lo que necesita, sin fricción operativa ni mensajes técnicos inseguros.

### SDR-144 · Shell administrativo reversible y coherente por rol

**Tipo:** Historia
**Prioridad:** Done / verificar
**Labels:** `ux`, `admin`, `sidebar`, `roles`
**Release:** `3.0.0-alpha.3`

**Descripción:**
Como Administrador/Gerente/Encargado, quiero una navegación lateral que pueda ocultarse y recuperarse sin perder acceso a acciones críticas.

**Criterios de aceptación:**

- Al ocultar sidebar, existe forma visible de expandirlo.
- El contenido aprovecha el espacio liberado.
- El perfil/cerrar sesión sigue accesible.
- La navegación primaria corresponde al rol.
- Verificar con captura por rol.

### SDR-146 · Mensajes de error seguros y útiles

**Tipo:** Historia técnica
**Prioridad:** Done / verificar
**Labels:** `errores`, `seguridad`, `ux`, `spanish`
**Release:** `3.0.0-alpha.3`

**Descripción:**
Como usuario, quiero recibir mensajes de error claros y en español sin detalles internos, para poder corregir el problema sin exponer información técnica.

**Criterios de aceptación:**

- Los errores esperados se muestran en español.
- No se exponen stack traces ni SQL al usuario.
- Los errores inesperados se registran internamente.
- La UI mantiene una ruta de recuperación.

### SDR-158 · Auditoría visual multirol con navegador

**Tipo:** Tarea QA
**Prioridad:** P0
**Labels:** `qa`, `browser`, `screenshots`, `roles`
**Release:** `3.0.0-rc.2`

**Descripción:**
Ejecutar una auditoría con navegador real, capturas y checklist por rol para detectar errores de UI, UX, permisos y flujo.

**Criterios de aceptación:**

- Existe evidencia de login y pantalla principal para cada rol.
- Se prueban flujos Cajero → Cocina → Despacho → Admin/Gerente.
- Se capturan errores y pantallas problemáticas.
- El informe separa P0/P1/P2.

## Épica 5 — Evidencia documental de entrega final

**Objetivo:** preparar todo lo que el evaluador pidió: PDF, SQL, links, manuales, video corto, matriz y trazabilidad Jira.

### SDR-151 · Generar PDF final de entrega

**Tipo:** Tarea documental
**Prioridad:** P0
**Labels:** `pdf`, `entrega-final`, `documentacion`
**Release:** `3.0.0-rc.1`

**Descripción:**
Crear documento final en DOCX/PDF con portada, matriz de configuración, enlaces y anexos.

**Criterios de aceptación:**

- Incluye portada.
- Incluye matriz de configuración de negocio.
- Incluye enlace a GitHub.
- Incluye script SQL o enlace directo.
- Incluye recursos adicionales: manuales o video.
- Está redactado en español formal.

### SDR-152 · Publicar script SQL final reproducible

**Tipo:** Tarea técnica/documental
**Prioridad:** P0
**Labels:** `sql`, `base-datos`, `seed`, `entrega-final`
**Release:** `3.0.0-rc.1`

**Descripción:**
Generar o actualizar un script SQL final con tablas, relaciones y seeders de productos alineados al modelo actual.

**Criterios de aceptación:**

- El script crea o documenta todas las tablas reales necesarias.
- Incluye seeders de roles, usuarios demo, productos, categorías, ingredientes y recetas.
- No contiene datos obsoletos que contradigan `Program.cs`.
- Se valida contra Supabase/PostgreSQL o se documenta claramente cómo ejecutarlo.

### SDR-153 · Manual rápido de usuario por rol

**Tipo:** Tarea documental
**Prioridad:** P1
**Labels:** `manual`, `roles`, `defensa-final`
**Release:** `3.0.0-rc.1`

**Descripción:**
Crear manual breve para Cajero, Cocina, Despacho y Administrador/Gerente, para que el equipo pueda probar sin depender de una sola persona.

**Criterios de aceptación:**

- Cada rol tiene credenciales demo, ruta inicial y acciones principales.
- Incluye errores comunes y cómo recuperarse.
- Está en español.
- Es corto y usable durante pruebas de equipo.

### SDR-156 · Actualizar Jira con trabajo real ejecutado

**Tipo:** Tarea de gestión
**Prioridad:** P0
**Labels:** `jira`, `agilidad`, `evidencia`, `trazabilidad`
**Release:** `3.0.0-rc.1`

**Descripción:**
Actualizar Jira con historias adicionales, estados reales y evidencia de desarrollo para demostrar gestión de calidad y agilidad.

**Criterios de aceptación:**

- Jira contiene historias nuevas SDR-126 en adelante.
- Los estados reflejan trabajo real: Done, In Progress, To Do.
- Cada ticket importante tiene criterios de aceptación.
- Se adjuntan PRs, capturas o notas de verificación cuando existan.

## Épica 6 — Release y estabilización v3.x

**Objetivo:** cerrar los cambios como incrementos revisables, con versiones claras y evidencia de calidad.

### SDR-159 · Preparar release v3.0.0 de defensa final

**Tipo:** Tarea release
**Prioridad:** P0
**Labels:** `release`, `v3`, `github`, `calidad`
**Release:** `3.0.0`

**Descripción:**
Crear una cadena de PRs pequeños y releases hasta v3.0.0, agrupando remediaciones críticas, documentación y evidencia.

**Criterios de aceptación:**

- Cada PR tiene alcance pequeño y verificable.
- Cada PR incluye pruebas o evidencia manual.
- El changelog/release notes explican qué cambió.
- La versión final `v3.0.0` queda lista para defensa.

### SDR-160 · Checklist final de entrega

**Tipo:** Checklist
**Prioridad:** P0
**Labels:** `checklist`, `defensa`, `entrega-final`
**Release:** `3.0.0`

**Descripción:**
Verificar antes de entregar que todos los requisitos explícitos del evaluador están cubiertos.

**Criterios de aceptación:**

- PDF final exportado.
- Repositorio limpio enlazado.
- Script SQL final enlazado o anexado.
- Manual o video corto listo.
- Jira actualizado.
- Demo probada con roles.
- Plan B preparado si falla internet, base de datos o SignalR.
