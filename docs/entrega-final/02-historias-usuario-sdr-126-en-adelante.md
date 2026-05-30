# Historias de usuario adicionales — SDR-126 en adelante

> Continuación del backlog SDR existente. Estas historias reflejan funcionalidades, hardening, documentación y evidencia agregadas o necesarias tras la expansión del sistema hacia una versión candidata **3.x**.

## MÓDULO: MODELOS DE NEGOCIO / ADAPTABILIDAD

### SDR-126 · Configurar modo Comida rápida / Mostrador
Como Cajero, quiero registrar pedidos de comida rápida desde el POS sin depender de una mesa, para atender clientes de mostrador de forma ágil.

Criterios de aceptación:
- Puedo iniciar un pedido como “Comida rápida / Mostrador”.
- El pedido no requiere mesa.
- El pedido se envía a cocina con etiqueta visible de mostrador.
- Al finalizar preparación, aparece en despacho/entrega.
- El flujo no bloquea pedidos de mesa ni para llevar.

### SDR-127 · Diferenciar Para llevar de Delivery
Como Cajero, quiero distinguir claramente un pedido “Para llevar” de un pedido “Delivery”, para evitar errores de entrega y documentación.

Criterios de aceptación:
- “Para llevar” representa retiro por el cliente, sin dirección.
- “Delivery” representa entrega a domicilio con datos de contacto.
- KDS y Despacho muestran etiquetas diferentes.
- La matriz de modelos de negocio puede demostrar ambos flujos sin ambigüedad.

### SDR-128 · Registrar pedido Delivery con datos de cliente
Como Cajero, quiero registrar nombre, teléfono, dirección y referencia de un pedido Delivery, para que despacho pueda entregar correctamente.

Criterios de aceptación:
- Al seleccionar Delivery, el sistema exige nombre del cliente, teléfono y dirección.
- La referencia de dirección es opcional.
- No se permite confirmar Delivery sin datos mínimos.
- Delivery no permite asignar mesa.
- El ticket digital muestra datos de entrega.
- Despacho muestra dirección y contacto.

### SDR-129 · Visualizar pedidos Delivery en Cocina y Despacho
Como Personal de Cocina y Despacho, quiero ver claramente cuáles pedidos son Delivery, para priorizar y preparar la entrega correcta.

Criterios de aceptación:
- KDS muestra etiqueta “Delivery”.
- Despacho muestra dirección, contacto y tipo de servicio.
- Los pedidos Delivery se ordenan por antigüedad o prioridad operativa.
- El pedido mantiene trazabilidad de estado hasta ser despachado.

### SDR-130 · Matriz de configuración del negocio
Como Evaluador, quiero ver una matriz que explique cómo el sistema se adapta a comida rápida, restaurante mesa, para llevar y delivery/despacho, para validar el atributo de calidad de adaptabilidad.

Criterios de aceptación:
- La matriz contiene los cuatro modelos solicitados.
- Cada modelo incluye roles, flujo, configuración y evidencia técnica.
- La matriz distingue implementado, parcial y pendiente.
- La matriz se incluye en el PDF final.

## MÓDULO: STOCK / INVENTARIO / INTEGRIDAD

### SDR-131 · Bloquear venta por stock insuficiente
Como Cajero, quiero que el sistema bloquee la venta cuando un ingrediente no tiene stock suficiente, para no vender productos que no pueden prepararse.

Criterios de aceptación:
- El pedido no se confirma si algún ingrediente requerido no alcanza.
- El mensaje indica el ingrediente faltante y producto afectado.
- No se descuenta stock parcial si falla un ingrediente.
- No se crea pedido inconsistente.
- El POS muestra error claro y en español.

### SDR-132 · Impedir stock negativo en dominio y base de datos
Como Encargado, quiero que el sistema impida stock negativo en cualquier operación, para preservar integridad de inventario.

Criterios de aceptación:
- `DescontarStock` rechaza cantidades que exceden el stock disponible.
- La base de datos o la operación transaccional impide valores negativos.
- Las pruebas verifican que el stock nunca queda por debajo de cero.
- El error es de negocio, no un error técnico genérico.

### SDR-133 · Proteger stock contra ventas concurrentes
Como Administrador, quiero que dos ventas simultáneas no puedan consumir el mismo último ingrediente, para mantener consistencia bajo concurrencia real.

Criterios de aceptación:
- Dos pedidos concurrentes compiten por el mismo stock limitado.
- Solo uno se confirma si el stock alcanza para uno.
- El segundo falla con error de stock insuficiente.
- La base de datos queda consistente.
- Existe prueba automatizada de concurrencia.

### SDR-134 · Reconciliar stock al modificar cantidad
Como Cajero, quiero que al cambiar la cantidad de un producto antes del pago se ajuste correctamente la reserva/descuento de stock, para evitar sobreventa o sobrereserva.

Criterios de aceptación:
- Aumentar cantidad valida stock adicional.
- Reducir cantidad devuelve o libera stock correspondiente.
- Si no alcanza stock para aumentar, el cambio se rechaza.
- El total se recalcula correctamente.
- La operación queda consistente aunque falle.

### SDR-135 · Reconciliar stock al eliminar detalle de pedido
Como Cajero, quiero que al quitar un producto del pedido se libere o revierta el stock reservado, para mantener inventario correcto.

Criterios de aceptación:
- Quitar producto antes de confirmación libera stock reservado.
- No se libera stock de pedidos ya consumidos si la política indica que no se revierte.
- El inventario queda trazable.
- Existe prueba automatizada.

### SDR-136 · Alertar producto agotado en POS
Como Cajero, quiero ver productos agotados o bloqueados por stock cero, para no intentar venderlos al cliente.

Criterios de aceptación:
- Si un ingrediente llega a cero, los productos afectados quedan bloqueados o marcados como agotados.
- El POS actualiza la disponibilidad sin recargar cuando sea posible.
- El cajero ve una alerta clara.
- El producto agotado no se puede agregar a una venta nueva.

### SDR-137 · Registrar movimientos de inventario
Como Encargado, quiero un historial de movimientos de inventario, para auditar ventas, ajustes, mermas y devoluciones de stock.

Criterios de aceptación:
- Cada descuento por venta crea movimiento.
- Cada ajuste manual crea movimiento.
- Cada merma crea movimiento.
- Cada devolución/reverso permitido crea movimiento.
- El historial muestra usuario, fecha, ingrediente, cantidad, tipo y referencia.

### SDR-138 · Kardex de ingrediente
Como Gerente, quiero consultar el kardex de un ingrediente, para entender entradas, salidas, mermas y consumo real.

Criterios de aceptación:
- Puedo seleccionar un ingrediente.
- Veo movimientos ordenados por fecha.
- Veo saldo inicial, entradas, salidas y saldo final.
- Puedo filtrar por rango de fechas.
- Solo Encargado, Gerente y Administrador acceden.

## MÓDULO: CAJA / CIERRE / TURNOS

### SDR-139 · Abrir día operativo antes de vender
Como Encargado, quiero abrir el día operativo antes de registrar ventas, para que todos los pagos queden asociados a un cierre diario.

Criterios de aceptación:
- No se permite pagar si no hay día operativo abierto.
- El mensaje indica que debe abrirse el día.
- El día abierto registra responsable y hora.
- Solo roles autorizados pueden abrir día.

### SDR-140 · Abrir turno de caja antes de cobrar
Como Cajero, quiero abrir mi turno de caja antes de cobrar, para que los pagos queden asociados al responsable correcto.

Criterios de aceptación:
- No se permite cobrar sin turno de caja activo.
- El turno registra cajero, hora de apertura y monto inicial.
- Los pagos del turno se consolidan al cerrar.
- El cierre muestra diferencias esperadas vs reales.

### SDR-141 · Cerrar turno de caja con conciliación
Como Cajero, quiero cerrar mi turno de caja conciliando efectivo, tarjeta y diferencias, para entregar control correcto al encargado.

Criterios de aceptación:
- El sistema muestra ventas del turno por método de pago.
- El cajero ingresa efectivo contado.
- El sistema calcula diferencia.
- Se registra responsable, hora y observaciones.
- No se puede cerrar el día si hay turnos abiertos.

### SDR-142 · Cierre de día con turnos y responsables
Como Encargado, quiero cerrar el día consolidando turnos, ventas, mermas y responsables, para tener un resumen confiable de operación.

Criterios de aceptación:
- El cierre incluye todos los turnos cerrados.
- El cierre no permite turnos abiertos.
- El cierre muestra ventas reales desde pagos, no desde pedidos no cobrados.
- Incluye mermas, descuentos, devoluciones y observaciones.
- El cierre queda auditable.

### SDR-143 · Registrar cobertura de turno
Como Encargado, quiero registrar cuando un empleado cubre el turno de otro, para mantener responsabilidad y pago justo.

Criterios de aceptación:
- Se puede registrar empleado original, empleado que cubre, horario y motivo.
- La cobertura queda asociada al turno.
- El cierre muestra responsables reales.
- La información queda disponible para pago o revisión administrativa.

## MÓDULO: UI / UX / ROLES

### SDR-144 · Shell administrativo por rol ✅ Done
Como Administrador o Gerente, quiero una navegación acorde a mi rol, para ver solo las funciones relevantes de gestión y control.

Criterios de aceptación:
- Administrador no ve como navegación primaria KDS, Despacho, Caja, Pedidos ni Transferir mesas.
- Gerente entra a dashboard gerencial.
- Encargado conserva operación de turno.
- El sidebar se puede colapsar y expandir.
- Logout permanece accesible.

### SDR-145 · Mapa de salón conectado al catálogo de mesas ✅ Done
Como Encargado, quiero que el mapa de salón muestre todas las mesas del catálogo, para administrar la distribución real del restaurante.

Criterios de aceptación:
- Toda mesa activa aparece en el mapa.
- Mesas sin posición reciben ubicación sugerida.
- El mapa muestra capacidad, estado y advertencias.
- El mapa se actualiza sin confundir mesas inactivas.

### SDR-146 · Mensajes de error seguros y útiles ✅ Done
Como Usuario, quiero recibir mensajes claros cuando una operación falla, para saber cómo corregir sin ver errores técnicos.

Criterios de aceptación:
- No se muestran stack traces al usuario.
- Los errores de negocio son específicos.
- Los errores inesperados muestran mensaje seguro.
- Se registra detalle técnico en logs internos.

### SDR-147 · Experiencia offline básica para assets críticos ✅ Done
Como Usuario operativo, quiero que el sistema mantenga recursos visuales/JS críticos disponibles, para evitar fallos básicos en operación inestable.

Criterios de aceptación:
- Los assets críticos están locales o cacheados.
- El sistema evita depender de CDN para operación básica.
- Hay banner o estado offline cuando aplique.
- No se rompe POS/KDS por pérdida temporal de conexión a assets.

## MÓDULO: REPORTES / GERENCIA / AUDITORÍA

### SDR-148 · Dashboard gerencial con datos reales
Como Gerente, quiero ver ventas, ticket promedio, productos vendidos y tendencia, para tomar decisiones con información confiable.

Criterios de aceptación:
- Ventas se calculan desde pagos reales.
- Ticket promedio usa pedidos cobrados.
- Top productos usa detalles pagados.
- Gráficos muestran rangos claros.
- Solo Gerente y Administrador acceden.

### SDR-149 · Auditoría de acciones críticas
Como Administrador, quiero consultar acciones críticas del sistema, para saber quién hizo qué y cuándo.

Criterios de aceptación:
- Se auditan cambios de precio.
- Se auditan devoluciones.
- Se auditan usuarios/roles.
- Se auditan cierres y turnos.
- El log es consultable y no editable desde UI.

### SDR-150 · Reportes integrados con caja y cierre
Como Gerente, quiero que los reportes usen pagos/caja/cierre como fuente confiable, para evitar inconsistencias contables.

Criterios de aceptación:
- Los reportes no cuentan pedidos no pagados como ventas.
- El cierre y reportes comparten fuente de datos.
- Las devoluciones/descuentos se reflejan.
- Hay tests de consistencia.

## MÓDULO: DOCUMENTACIÓN / ENTREGA FINAL

### SDR-151 · Generar PDF final de entrega
Como Equipo, quiero un documento PDF final con portada, matriz, enlaces y evidencia, para cumplir los requisitos de entrega académica.

Criterios de aceptación:
- Incluye portada.
- Incluye matriz de configuración del negocio.
- Incluye enlace al repositorio.
- Incluye enlace o anexo del SQL.
- Incluye manuales rápidos o video.
- Incluye evidencia de calidad/Jira.

### SDR-152 · Crear script SQL final de esquema y datos semilla
Como Evaluador, quiero un script SQL único de creación y seed, para revisar la base de datos del sistema sin depender del entorno local.

Criterios de aceptación:
- El script corre sobre PostgreSQL/Supabase vacío.
- Crea tablas y relaciones actuales.
- Inserta usuarios demo actuales.
- Inserta productos, ingredientes, recetas, alérgenos y mesas.
- Está documentado en `scripts/README.md`.

### SDR-153 · Manual rápido por rol
Como Usuario evaluador, quiero un manual rápido por rol, para probar el sistema sin explicación adicional.

Criterios de aceptación:
- Manual incluye Cajero, Cocina, Despacho y Administrador.
- Incluye credenciales demo.
- Incluye ruta inicial.
- Incluye flujo principal.
- Incluye resultado esperado.

### SDR-154 · Guion de video demostrativo de 5 minutos
Como Equipo, quiero un guion corto de video, para mostrar el producto de forma clara dentro del límite exigido.

Criterios de aceptación:
- El guion dura máximo 5 minutos.
- Cubre flujo multirol.
- Muestra stock/integridad si está implementado.
- Muestra dashboard/reportes.
- Incluye cierre narrativo de calidad.

### SDR-155 · Runbook de defensa de 30 minutos
Como Equipo, quiero un runbook de defensa, para ejecutar la simulación sin improvisar.

Criterios de aceptación:
- El runbook divide la defensa por minutos.
- Incluye usuarios, rutas y acciones.
- Incluye qué decir en cada sección.
- Incluye plan de contingencia.
- Incluye evidencias a mostrar.

### SDR-156 · Evidencia Jira y trazabilidad
Como Evaluador, quiero ver Jira organizado con historias, estados, releases y evidencia, para comprobar gestión ágil y calidad.

Criterios de aceptación:
- Jira contiene épicas por módulo.
- Historias SDR están reflejadas.
- Cada historia tiene criterios de aceptación.
- Se vinculan PRs, releases o evidencias.
- Hay screenshots/export de tablero para el PDF.

### SDR-157 · Matriz de trazabilidad actualizada
Como Equipo, quiero una matriz que conecte historias, criterios, pruebas, PRs y releases, para demostrar control de calidad.

Criterios de aceptación:
- Cada SDR tiene estado.
- Cada SDR tiene criterios o motivo de pendiente.
- Cada SDR implementado referencia pruebas/evidencia.
- La matriz distingue Sprint 1, Sprint 2, Sprint 3 y v3.x.

## MÓDULO: QA / RELEASES / OPERACIÓN

### SDR-158 · Auditoría browser multirol con screenshots
Como Equipo, quiero una auditoría visual con navegador real y capturas, para comprobar que la simulación funciona antes de defender.

Criterios de aceptación:
- Se prueba login de Cajero, Cocina, Despacho y Administrador.
- Se ejecuta flujo pedido → KDS → despacho → dashboard.
- Se guardan screenshots por checkpoint.
- Se reportan rutas, errores y resultados.
- La evidencia se incorpora al paquete final.

### SDR-159 · Release 3.0.0 de defensa final
Como Equipo, quiero publicar una versión 3.0.0 que agrupe las mejoras finales, para separar el trabajo de defensa de los sprints anteriores.

Criterios de aceptación:
- Se preserva historial 0.x, 1.x y 2.x.
- Se reconcilia Release Please antes de cortar 3.0.0.
- CI está verde.
- Release notes describen mejoras finales.
- El PDF referencia la versión final.

### SDR-160 · Checklist de entrega final
Como Equipo, quiero un checklist final, para asegurar que ningún entregable obligatorio falte al subir la entrega.

Criterios de aceptación:
- Checklist incluye PDF, SQL, repo, manual, video y Jira.
- Cada ítem tiene responsable.
- Cada ítem tiene estado.
- Se revisa antes de la defensa.
