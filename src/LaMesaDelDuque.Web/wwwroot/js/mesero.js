/* ═══════════════════════════════════════════════════════
   Mesero — La Mesa del Duque  v1.0
   ═══════════════════════════════════════════════════════ */

(function () {
    'use strict';

    // ── Helpers ────────────────────────────────────────
    function icon(name, cls) {
        return '<svg class="lmd-icon ' + (cls || '') + '" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><use href="/lib/lucide-static/icons/' + name + '.svg#icon"/></svg>';
    }

    function fmt(n) {
        return new Intl.NumberFormat('es-SV', { style: 'currency', currency: 'USD' }).format(n || 0);
    }

    function escapeHtml(value) {
        return String(value ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function escapeJsString(value) {
        return String(value ?? '')
            .replace(/\\/g, '\\\\')
            .replace(/'/g, "\\'")
            .replace(/\r/g, '\\r')
            .replace(/\n/g, '\\n')
            .replace(/</g, '\\x3C')
            .replace(/>/g, '\\x3E')
            .replace(/&/g, '\\x26')
            .replace(/"/g, '&quot;');
    }

<<<<<<< HEAD
=======
    async function confirmar(mensaje) {
        if (typeof window.lmdConfirm === 'function') {
            return await window.lmdConfirm(mensaje);
        }
        return window.confirm(mensaje);
    }

    function notificar(mensaje, tipo) {
        if (typeof window.lmdToast === 'function') {
            window.lmdToast(mensaje, tipo);
        } else {
            console.log(mensaje);
        }
    }
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)

    // ── State ──────────────────────────────────────────
    var state = {
        mesas: [],
        mesaActual:     null,
        pedidoDetalles: [],
        pedidoTotal:    0,
        pedidoEstado:   null,
        carrito:        [],
        catFiltro:      'Todos',
        busquedaProd:   ''
    };

    var SHELL = null;
    var connection = null;

    // ── Overlay system ─────────────────────────────────
    function abrirOverlay(id, html, opts) {
        cerrarOverlay(id);
        opts = opts || {};
        var ov = document.createElement('div');
        ov.id = 'lmd-ov-' + id;
        ov.className = 'lmd-pos-overlay';
        if (opts.closeOnBackdrop !== false) {
            ov.addEventListener('click', function (e) { if (e.target === ov) cerrarOverlay(id); });
        }
        var panel = document.createElement('div');
        panel.className = 'lmd-pos-ov-panel' + (opts.wide ? ' lmd-pos-ov-panel--wide' : '');
        panel.innerHTML = html;
        ov.appendChild(panel);
        document.body.appendChild(ov);
        requestAnimationFrame(function () { ov.classList.add('lmd-pos-overlay--visible'); });
    }

    function cerrarOverlay(id) {
        var el = document.getElementById('lmd-ov-' + id);
        if (el) el.remove();
    }

    // ── AJAX helpers ───────────────────────────────────
    function csrf() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
    }

    async function postJson(handler, params) {
        var form = new FormData();
        form.append('__RequestVerificationToken', csrf());
        Object.keys(params).forEach(function (k) {
            if (params[k] != null) form.append(k, params[k].toString());
        });
        return fetch('?handler=' + handler, {
            method: 'POST', body: form,
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
    }

    // ═══════════════════════════════════════════════════
    // GRID DE MESAS
    // ═══════════════════════════════════════════════════
    async function refrescarMesas() {
        try {
            var res = await fetch('?handler=MesasJson', { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (res.ok) {
                var data = await res.json();
                if (data && data.mesas) state.mesas = data.mesas;
            }
        } catch (e) {}
    }

    function renderGrid() {
<<<<<<< HEAD
        var mesas = state.mesas;
=======
        var soloOcupadas = window.__lmdSoloOcupadasMesero === true || window.__lmdSoloOcupadasMesero === 'true';
        var mesas = state.mesas.slice();
        if (soloOcupadas) {
            mesas = mesas.filter(function (m) {
                return m.estado === 'Ocupada';
            });
        }
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
        var grupos = {};
        mesas.forEach(function (m) {
            var cap = m.capacidad || 0;
            if (!grupos[cap]) grupos[cap] = [];
            grupos[cap].push(m);
        });
        var caps = Object.keys(grupos).map(Number).sort(function (a, b) { return a - b; });

        var mesasHtml = '';
        caps.forEach(function (cap) {
            mesasHtml += '<div class="lmd-pos-mesa-zona-separator">' + cap + ' personas</div>';
            grupos[cap].forEach(function (m) {
                var disponible = m.estado === 'Disponible';
<<<<<<< HEAD
                var enGracia   = disponible && m.graciaHasta && new Date(m.graciaHasta) > Date.now();
                var hayTab     = !disponible && m.pedidoActualId;
                var enCobro    = hayTab && m.pedidoEstado === 'EnCobro';
                var cls = enGracia  ? 'lmd-pos-mesa-card--en-gracia'
                    : disponible    ? 'lmd-pos-mesa-card--disponible'
                    : enCobro       ? 'lmd-pos-mesa-card--en-cobro'
                    :                  'lmd-pos-mesa-card--ocupada';
                var onclick = enGracia   ? ''
                    : hayTab     ? ' onclick="mesero.abrirDetalle(\'' + m.id + '\')"'
                    : disponible ? ' onclick="mesero.abrirNuevoPedido(\'' + m.id + '\')"'
=======
                var ocupada     = m.estado === 'Ocupada';
                var reservada   = m.estado === 'Reservada';
                var mantenimiento = m.estado === 'EnMantenimiento';
                var enGracia   = disponible && m.graciaHasta && new Date(m.graciaHasta) > Date.now();
                var hayTab     = !!m.pedidoActualId;
                var enCobro    = hayTab && m.pedidoEstado === 'EnCobro';
                var listo      = hayTab && m.pedidoEstado === 'Listo';
                var cls = enGracia  ? 'lmd-pos-mesa-card--en-gracia'
                    : disponible    ? 'lmd-pos-mesa-card--disponible'
                    : enCobro       ? 'lmd-pos-mesa-card--en-cobro'
                    : listo         ? 'lmd-pos-mesa-card--lista'
                    : reservada     ? 'lmd-pos-mesa-card--reservada'
                    : mantenimiento ? 'lmd-pos-mesa-card--mantenimiento'
                    :                  'lmd-pos-mesa-card--ocupada';
                var bloqueada = enGracia || reservada || mantenimiento;
                var puedeCrearPedido = disponible || (ocupada && !hayTab);
                var onclick = bloqueada ? ''
                    : hayTab ? ' onclick="mesero.abrirDetalle(\'' + m.id + '\')"'
                    : puedeCrearPedido ? ' onclick="mesero.abrirNuevoPedido(\'' + m.id + '\')"'
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
                    : '';

                var badgeHtml = '';
                if (enGracia) {
                    var secsLeft = Math.max(0, Math.floor((new Date(m.graciaHasta).getTime() - Date.now()) / 1000));
                    var mins = Math.floor(secsLeft / 60), secs = secsLeft % 60;
                    badgeHtml = '<span class="lmd-pos-mesa-card__gracia-badge" data-gracia-hasta="' + m.graciaHasta + '">' +
                        icon('timer') + ' <span class="lmd-gracia-tiempo">' + mins + ':' + (secs < 10 ? '0' : '') + secs + '</span></span>';
                } else if (enCobro) {
                    badgeHtml = '<span class="lmd-pos-mesa-card__cobrar-badge">' + icon('receipt') + ' Cobrar</span>';
<<<<<<< HEAD
=======
                } else if (listo) {
                    badgeHtml = '<span class="lmd-pos-mesa-card__listo-badge">' + icon('message-square') + ' Listo</span>';
                } else if (reservada) {
                    badgeHtml = '<span class="lmd-pos-mesa-card__estado-badge">Reservada</span>';
                } else if (mantenimiento) {
                    badgeHtml = '<span class="lmd-pos-mesa-card__estado-badge">Mantenimiento</span>';
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
                } else if (hayTab) {
                    var minTab = m.pedidoFechaCreacion
                        ? Math.floor((Date.now() - new Date(m.pedidoFechaCreacion).getTime()) / 60000) : 0;
                    badgeHtml = '<span class="lmd-pos-mesa-card__tab-badge" data-pedido-fecha="' + (m.pedidoFechaCreacion || '') + '">' +
                        icon('clock') + ' <span class="lmd-tab-tiempo">' + (minTab > 0 ? minTab + ' min' : 'Tab') + '</span></span>';
                }

                mesasHtml += '<div class="lmd-pos-mesa-card ' + cls + '"' + onclick + '>' +
                    '<span class="lmd-pos-mesa-card__numero">' + m.numero + '</span>' +
                    '<span class="lmd-pos-mesa-card__capacidad">' + m.capacidad + ' pax</span>' +
                    badgeHtml +
                    (m.zona ? '<span class="lmd-pos-mesa-card__zona">' + escapeHtml(m.zona) + '</span>' : '') +
                '</div>';
            });
        });

<<<<<<< HEAD
=======
        var listasCount = mesas.filter(function (m) {
            return m.estado === 'Ocupada' && m.pedidoActualId && m.pedidoEstado === 'Listo';
        }).length;
        var notifBtn = soloOcupadas
            ? '<button class="lmd-mesero-notif-btn' + (listasCount > 0 ? ' lmd-mesero-notif-btn--activo' : '') + '" onclick="mesero.verNotificaciones()" title="Ver pedidos listos">' +
                icon('bell') + '<span>Notificaciones</span><strong>' + listasCount + '</strong></button>'
            : '';

>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
        SHELL.innerHTML =
            '<div class="lmd-mesero-wrap">' +
                '<div class="lmd-mesero-header">' +
                    icon('utensils-crossed') +
<<<<<<< HEAD
                    '<span>Salón</span>' +
                    '<button class="lmd-mesero-refrescar" onclick="mesero.refrescarGrid()" title="Refrescar">' + icon('refresh-cw') + '</button>' +
                '</div>' +
                '<div class="lmd-pos-mesas-grid lmd-mesero-grid">' +
                    (mesasHtml || '<div class="lmd-pos-empty">Sin mesas configuradas</div>') +
=======
                    '<span>' + (soloOcupadas ? 'Mesas ocupadas' : 'Salón') + '</span>' +
                    notifBtn +
                    '<button class="lmd-mesero-refrescar" onclick="mesero.refrescarGrid()" title="Refrescar">' + icon('refresh-cw') + '</button>' +
                '</div>' +
                '<div class="lmd-pos-mesas-grid lmd-mesero-grid">' +
                    (mesasHtml || '<div class="lmd-pos-empty">' + (soloOcupadas ? 'No hay mesas ocupadas' : 'Sin mesas configuradas') + '</div>') +
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
                '</div>' +
            '</div>';
    }

    async function refrescarGrid() {
        await refrescarMesas();
        renderGrid();
    }

<<<<<<< HEAD
=======
    function pedidosListos() {
        return state.mesas.filter(function (m) {
            return m.estado === 'Ocupada' && m.pedidoActualId && m.pedidoEstado === 'Listo';
        }).sort(function (a, b) { return (a.numero || 0) - (b.numero || 0); });
    }

    function verNotificaciones() {
        var listas = pedidosListos();
        var cuerpo = listas.length === 0
            ? '<div class="lmd-pos-empty" style="padding:1.5rem;">' + icon('bell-off') + ' No hay pedidos listos por el momento.</div>'
            : listas.map(function (m) {
                return '<button class="lmd-mesero-notif-item" onclick="mesero.abrirNotificacionMesa(\'' + escapeJsString(m.id) + '\')">' +
                    '<span class="lmd-mesero-notif-item__mesa">Mesa ' + escapeHtml(m.numero) + '</span>' +
                    '<span class="lmd-mesero-notif-item__texto">Pedido listo para retirar</span>' +
                    '<span class="lmd-mesero-notif-item__total">' + fmt(m.pedidoTotal || 0) + '</span>' +
                '</button>';
            }).join('');

        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="mesero.cerrarNotificaciones()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('bell') + ' Notificaciones de pedidos</span>' +
            '</div>' +
            '<div class="lmd-mesero-notif-lista">' + cuerpo + '</div>';

        abrirOverlay('notificaciones', html, { closeOnBackdrop: true });
    }

    function cerrarNotificaciones() { cerrarOverlay('notificaciones'); }

    async function abrirNotificacionMesa(mesaId) {
        cerrarNotificaciones();
        await abrirDetalle(mesaId);
    }

>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
    // ═══════════════════════════════════════════════════
    // OVERLAY DETALLE DE MESA
    // ═══════════════════════════════════════════════════
    async function abrirDetalle(mesaId) {
        var m = state.mesas.find(function (x) { return x.id === mesaId; });
        if (!m || !m.pedidoActualId) return;
        state.mesaActual = m;

        try {
            var res = await fetch('?handler=DetallesPedidoJson&pedidoId=' + m.pedidoActualId,
                { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
<<<<<<< HEAD
            if (!res.ok) { window.lmdToast('Error al cargar detalle', 'error'); return; }
=======
            if (!res.ok) { notificar('Error al cargar detalle', 'error'); return; }
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
            var data = await res.json();
            state.pedidoDetalles = data.detalles || [];
            state.pedidoTotal    = data.total    || 0;
            state.pedidoEstado   = data.estado   || m.pedidoEstado;
<<<<<<< HEAD
        } catch (e) { window.lmdToast('Error de conexión', 'error'); return; }
=======
        } catch (e) { notificar('Error de conexión', 'error'); return; }
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)

        _renderDetalle();
    }

    var _ESTADO_LABEL = {
        'Pendiente':     'Pendiente',
        'EnPreparacion': 'En cocina',
        'Listo':         'Listo',
        'EnCobro':       'En cobro',
        'Despachado':    'Despachado',
        'Pagado':        'Pagado',
        'Cancelado':     'Cancelado'
    };

    function _renderDetalle() {
        var m       = state.mesaActual;
        var enCobro = state.pedidoEstado === 'EnCobro';

        var estadoLabel = _ESTADO_LABEL[state.pedidoEstado] || state.pedidoEstado || '';
        var estadoBadge = estadoLabel
            ? '<span class="lmd-mesero-estado-badge lmd-mesero-estado-badge--' +
              escapeHtml((state.pedidoEstado || '').toLowerCase()) + '">' + escapeHtml(estadoLabel) + '</span>'
            : '';

        var itemsHtml = state.pedidoDetalles.length === 0
            ? '<div class="lmd-pos-empty" style="padding:1.5rem 1rem;">' + icon('package-open') + ' Sin ítems</div>'
            : state.pedidoDetalles.map(function (d) {
                var qtyHtml = enCobro
                    ? '<span class="lmd-mesero-item__cant">× ' + d.cantidad + '</span>'
                    : '<div class="lmd-mesero-item__qty-ctrl">' +
                        '<button onclick="mesero.ajustarCantidad(\'' + escapeJsString(m.pedidoActualId) + '\',\'' + escapeJsString(d.id) + '\',-1)">' + icon('minus') + '</button>' +
                        '<span>' + d.cantidad + '</span>' +
                        '<button onclick="mesero.ajustarCantidad(\'' + escapeJsString(m.pedidoActualId) + '\',\'' + escapeJsString(d.id) + '\',1)">' + icon('plus') + '</button>' +
                      '</div>';
                var voidBtn = !enCobro
                    ? '<button class="lmd-mesero-item__void" title="Anular" ' +
                      'onclick="mesero.voidItem(\'' + escapeJsString(m.pedidoActualId) + '\',\'' + escapeJsString(d.id) + '\',\'' + escapeJsString(d.productoNombre) + '\')">' +
                      icon('trash-2') + '</button>'
                    : '';
                return '<div class="lmd-mesero-item">' +
                    '<div class="lmd-mesero-item__info">' +
                        '<span class="lmd-mesero-item__nombre">' + escapeHtml(d.productoNombre) + '</span>' +
                        qtyHtml +
                    '</div>' +
                    '<div class="lmd-mesero-item__right">' +
                        '<span class="lmd-mesero-item__sub">' + fmt(d.subtotal) + '</span>' +
                        voidBtn +
                    '</div>' +
                '</div>';
            }).join('');

        var accionesHtml = enCobro
            ? '<button class="lmd-mesero-btn lmd-mesero-btn--cobrar" onclick="mesero.abrirPago()">' +
                icon('credit-card') + ' Cobrar</button>'
            : '<button class="lmd-mesero-btn" onclick="mesero.abrirAgregar()">' + icon('plus') + ' Agregar</button>' +
              '<button class="lmd-mesero-btn lmd-mesero-btn--cuenta" onclick="mesero.pedirCuenta()">' + icon('receipt') + ' Pedir cuenta</button>';

<<<<<<< HEAD
=======
        var puedeCerrar = m && m.pedidoActualId && state.pedidoEstado !== 'Pagado' && state.pedidoEstado !== 'Despachado' && state.pedidoEstado !== 'Cancelado';
        var cerrarBtn = puedeCerrar
            ? '<button class="lmd-mesero-cerrar-cuenta" onclick="mesero.cerrarCuentaMesa()">' + icon('receipt') + ' Cerrar</button>'
            : '';

>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="mesero.cerrarDetalle()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('armchair') + ' Mesa ' + escapeHtml(m.numero) + estadoBadge + '</span>' +
<<<<<<< HEAD
                '<div class="lmd-pos-ov-total">' + fmt(state.pedidoTotal) + '</div>' +
=======
                '<div class="lmd-mesero-ov-total-actions">' + cerrarBtn + '<div class="lmd-pos-ov-total">' + fmt(state.pedidoTotal) + '</div></div>' +
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
            '</div>' +
            '<div class="lmd-mesero-detalle-body">' +
                '<div class="lmd-mesero-detalle-lista">' + itemsHtml + '</div>' +
                '<div class="lmd-mesero-detalle-footer">' +
                    '<div class="lmd-mesero-total-row">' +
                        '<span>Total</span><span>' + fmt(state.pedidoTotal) + '</span>' +
                    '</div>' +
                    '<div class="lmd-mesero-acciones">' + accionesHtml + '</div>' +
                '</div>' +
            '</div>';

        abrirOverlay('detalle', html, { closeOnBackdrop: true });
    }

    function cerrarDetalle() { cerrarOverlay('detalle'); }

    async function voidItem(pedidoId, detalleId, nombre) {
<<<<<<< HEAD
        var ok = await window.lmdConfirm('¿Anular "' + nombre + '"?');
=======
        var ok = await confirmar('¿Anular "' + nombre + '"?');
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
        if (!ok) return;
        try {
            var res  = await postJson('EliminarLineaJson', { pedidoId: pedidoId, detalleId: detalleId });
            var data = await res.json().catch(function () { return {}; });
<<<<<<< HEAD
            if (!res.ok) { window.lmdToast(data.error || 'Error al anular', 'error'); return; }
            state.pedidoDetalles = state.pedidoDetalles.filter(function (d) { return d.id !== detalleId; });
            state.pedidoTotal    = data.total || 0;
            _renderDetalle();
            window.lmdToast('Ítem anulado', 'success');
        } catch (e) { window.lmdToast('Error de conexión', 'error'); }
=======
            if (!res.ok) { notificar(data.error || 'Error al anular', 'error'); return; }
            state.pedidoDetalles = state.pedidoDetalles.filter(function (d) { return d.id !== detalleId; });
            state.pedidoTotal    = data.total || 0;
            _renderDetalle();
            notificar('Ítem anulado', 'success');
        } catch (e) { notificar('Error de conexión', 'error'); }
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
    }

    async function ajustarCantidad(pedidoId, detalleId, delta) {
        var det = state.pedidoDetalles.find(function (d) { return d.id === detalleId; });
        if (!det) return;
        var nuevaCantidad = det.cantidad + delta;
        if (nuevaCantidad < 0) return;
        try {
            var res  = await postJson('ActualizarCantidadJson', { pedidoId: pedidoId, detalleId: detalleId, cantidad: nuevaCantidad });
            var data = await res.json().catch(function () { return {}; });
<<<<<<< HEAD
            if (!res.ok) { window.lmdToast(data.error || 'Error al actualizar', 'error'); return; }
            state.pedidoDetalles = data.detalles || [];
            state.pedidoTotal    = data.total    || 0;
            _renderDetalle();
        } catch (e) { window.lmdToast('Error de conexión', 'error'); }
=======
            if (!res.ok) { notificar(data.error || 'Error al actualizar', 'error'); return; }
            state.pedidoDetalles = data.detalles || [];
            state.pedidoTotal    = data.total    || 0;
            _renderDetalle();
        } catch (e) { notificar('Error de conexión', 'error'); }
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
    }

    async function pedirCuenta() {
        var m = state.mesaActual;
        if (!m) return;
<<<<<<< HEAD
        var ok = await window.lmdConfirm('¿Solicitar cuenta para Mesa ' + m.numero + '?');
=======
        var ok = await confirmar('¿Solicitar cuenta para Mesa ' + m.numero + '?');
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
        if (!ok) return;
        try {
            var res  = await postJson('MarcarEnCobroJson', { pedidoId: m.pedidoActualId });
            var data = await res.json().catch(function () { return {}; });
<<<<<<< HEAD
            if (!res.ok) { window.lmdToast(data.error || 'Error', 'error'); return; }
=======
            if (!res.ok) { notificar(data.error || 'Error', 'error'); return; }
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
            state.pedidoEstado = 'EnCobro';
            _renderDetalle();
            await refrescarMesas();
            renderGrid();
<<<<<<< HEAD
            window.lmdToast('Cuenta solicitada — Mesa ' + m.numero, 'success');
        } catch (e) { window.lmdToast('Error de conexión', 'error'); }
=======
            notificar('Cuenta solicitada — Mesa ' + m.numero, 'success');
        } catch (e) { notificar('Error de conexión', 'error'); }
    }

    async function cerrarCuentaMesa() {
        var m = state.mesaActual;
        if (!m || !m.pedidoActualId) return;

        var ok = await confirmar(
            '¿Cerrar la cuenta de Mesa ' + m.numero + ' y enviarla a caja? La mesa pasará a mantenimiento.'
        );
        if (!ok) return;

        try {
            var res = await postJson('CerrarMesaJson', { pedidoId: m.pedidoActualId, mesaId: m.id });
            var data = await res.json().catch(function () { return {}; });
            if (!res.ok) {
                notificar(data.error || 'Error al cerrar la cuenta', 'error');
                return;
            }

            state.pedidoEstado = 'EnCobro';
            cerrarOverlay('detalle');
            await refrescarMesas();
            renderGrid();
            notificar(data.mensaje || 'Cuenta cerrada y enviada a caja', 'success');

            if (data.redirectUrl) {
                setTimeout(function () { window.location.href = data.redirectUrl; }, 650);
            }
        } catch (e) {
            notificar('Error de conexión', 'error');
        }
    }

    async function _cerrarPedidoEnCaja(pedidoId, mesaId) {
        var res = await postJson('CerrarMesaJson', { pedidoId: pedidoId, mesaId: mesaId });
        var data = await res.json().catch(function () { return {}; });
        if (!res.ok) {
            notificar(data.error || 'Error al cerrar la cuenta', 'error');
            return null;
        }
        return data;
    }

    async function cerrarCuentaDesdeAgregar() {
        var m = state.mesaActual;
        if (!m) return;

        if (!m.pedidoActualId && state.carrito.length === 0) {
            notificar('Agrega productos antes de cerrar la cuenta', 'error');
            return;
        }

        var mensaje = m.pedidoActualId
            ? (state.carrito.length > 0
                ? '¿Enviar los productos pendientes a cocina y cerrar la cuenta de Mesa ' + m.numero + '? La mesa pasará a mantenimiento y la cuenta irá a caja.'
                : '¿Cerrar la cuenta de Mesa ' + m.numero + ' y enviarla a caja? La mesa pasará a mantenimiento.')
            : '¿Crear el pedido de Mesa ' + m.numero + ', enviarlo a cocina y mandar la cuenta a caja? La mesa pasará a mantenimiento.';

        var ok = await confirmar(mensaje);
        if (!ok) return;

        try {
            var data;
            if (!m.pedidoActualId) {
                var itemsJson = JSON.stringify(state.carrito.map(function (i) {
                    return { productoId: i.id, cantidad: i.cantidad };
                }));
                var resCrear = await postJson('CrearYCerrarMesaJson', { mesaId: m.id, itemsJson: itemsJson });
                data = await resCrear.json().catch(function () { return {}; });
                if (!resCrear.ok) {
                    notificar(data.error || 'Error al crear y cerrar la cuenta', 'error');
                    return;
                }
            } else {
                if (state.carrito.length > 0) {
                    for (var i = 0; i < state.carrito.length; i++) {
                        var item = state.carrito[i];
                        var resItem = await postJson('AgregarLineaJson', {
                            pedidoId: m.pedidoActualId,
                            productoId: item.id,
                            cantidad: item.cantidad
                        });
                        if (!resItem.ok) {
                            var err = await resItem.json().catch(function () { return {}; });
                            notificar(err.error || 'Error al enviar productos a cocina', 'error');
                            return;
                        }
                    }
                }
                data = await _cerrarPedidoEnCaja(m.pedidoActualId, m.id);
                if (!data) return;
            }

            state.carrito = [];
            cerrarOverlay('agregar');
            cerrarOverlay('detalle');
            await refrescarMesas();
            renderGrid();
            notificar(data.mensaje || 'Cuenta enviada a caja', 'success');
            if (data.redirectUrl) {
                setTimeout(function () { window.location.href = data.redirectUrl; }, 650);
            }
        } catch (e) {
            notificar('Error de conexión', 'error');
        }
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
    }

    function abrirNuevoPedido(mesaId) {
        var m = state.mesas.find(function (x) { return x.id === mesaId; });
<<<<<<< HEAD
        if (!m || m.estado !== 'Disponible') return;
=======
        if (!m || m.pedidoActualId) return;
        if (m.estado !== 'Disponible' && m.estado !== 'Ocupada') return;
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
        state.mesaActual   = m;
        state.carrito      = [];
        state.catFiltro    = 'Todos';
        state.busquedaProd = '';
        _renderAgregar();
    }

    async function _crearPedidoConItems(m) {
        var itemsJson = JSON.stringify(state.carrito.map(function (i) {
            return { productoId: i.id, cantidad: i.cantidad };
        }));
        try {
            var res  = await postJson('CrearConItemsJson', { mesaId: m.id, itemsJson: itemsJson });
            var data = await res.json().catch(function () { return {}; });
<<<<<<< HEAD
            if (!res.ok) { window.lmdToast(data.error || 'Error al crear pedido', 'error'); return; }
=======
            if (!res.ok) { notificar(data.error || 'Error al crear pedido', 'error'); return; }
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
            state.mesaActual = Object.assign({}, m, { pedidoActualId: data.pedidoId });
            var r   = await fetch('?handler=DetallesPedidoJson&pedidoId=' + data.pedidoId,
                { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            var det = await r.json();
            state.pedidoDetalles = det.detalles || [];
            state.pedidoTotal    = det.total    || 0;
            state.pedidoEstado   = det.estado   || 'EnPreparacion';
            state.carrito = [];
            cerrarOverlay('agregar');
            await refrescarMesas();
            renderGrid();
            _renderDetalle();
<<<<<<< HEAD
            window.lmdToast('Mesa ' + m.numero + ' — Pedido creado', 'success');
        } catch (e) { window.lmdToast('Error de conexión', 'error'); }
=======
            notificar('Pedido enviado a cocina', 'success');
        } catch (e) { notificar('Error de conexión', 'error'); }
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
    }

    // ═══════════════════════════════════════════════════
    // OVERLAY AGREGAR ÍTEMS
    // ═══════════════════════════════════════════════════
    function abrirAgregar() {
        state.carrito      = [];
        state.catFiltro    = 'Todos';
        state.busquedaProd = '';
        _renderAgregar();
    }

    function buscarProd(q) { state.busquedaProd = q || ''; _renderAgregar(); }

    function _renderAgregar() {
        var prods = window.__lmdProductosMesero || [];
        var cats  = ['Todos'];
        var seen  = {};
        prods.forEach(function (p) {
            var cn = p.categoriaNombre || 'Sin categoría';
            if (!seen[cn]) { seen[cn] = true; cats.push(cn); }
        });
        cats.sort(function (a, b) { return a === 'Todos' ? -1 : b === 'Todos' ? 1 : a.localeCompare(b); });

        var catHtml = cats.map(function (c) {
            return '<button class="lmd-pos-cat-btn' + (c === state.catFiltro ? ' lmd-pos-cat-btn--activa' : '') + '" ' +
                'onclick="mesero.filtrarCat(\'' + escapeJsString(c) + '\')">' +
<<<<<<< HEAD
                icon(c === 'Todos' ? 'layers' : c === 'Bebidas' ? 'wine' : c === 'Postres' ? 'cake-slice' : 'utensils') +
=======
                icon(c === 'Todos' ? 'list' : 'utensils-crossed') +
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
                '<span>' + escapeHtml(c) + '</span>' +
            '</button>';
        }).join('');

        var busq = state.busquedaProd.trim().toLowerCase();
        var filtrados = prods.filter(function (p) {
            var catOk = state.catFiltro === 'Todos' || (p.categoriaNombre || 'Sin categoría') === state.catFiltro;
            var busOk = !busq || p.nombre.toLowerCase().includes(busq);
            return catOk && busOk;
        });

        var prodHtml = filtrados.map(function (p) {
<<<<<<< HEAD
            return '<div class="lmd-pos-product-card" onclick="mesero.addToCart(\'' + escapeJsString(p.id) + '\')">' +
                '<span class="lmd-pos-product-card__nombre">' + escapeHtml(p.nombre) + '</span>' +
                '<span class="lmd-pos-product-card__precio">' + fmt(p.precio) + '</span>' +
=======
            return '<div class="lmd-pos-producto-card lmd-mesero-producto-card" onclick="mesero.addToCart(\'' + escapeJsString(p.id) + '\')">' +
                '<span class="lmd-pos-producto-card__nombre">' + escapeHtml(p.nombre) + '</span>' +
                '<span class="lmd-pos-producto-card__precio">' + fmt(p.precio) + '</span>' +
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
            '</div>';
        }).join('');

        var carritoTotal = state.carrito.reduce(function (s, i) { return s + i.precio * i.cantidad; }, 0);
        var carritoHtml  = state.carrito.length === 0
            ? '<div class="lmd-pos-cart__empty">' + icon('shopping-cart') + '<span>Sin ítems</span></div>'
            : state.carrito.map(function (item) {
                return '<div class="lmd-mesero-cart-item">' +
                    '<span class="lmd-mesero-cart-item__nombre">' + escapeHtml(item.nombre) + '</span>' +
                    '<div class="lmd-mesero-cart-item__qty">' +
                        '<button onclick="mesero.decCart(\'' + item.id + '\')">' + icon('minus') + '</button>' +
                        '<span>' + item.cantidad + '</span>' +
                        '<button onclick="mesero.incCart(\'' + item.id + '\')">' + icon('plus') + '</button>' +
                    '</div>' +
                    '<span class="lmd-mesero-cart-item__sub">' + fmt(item.precio * item.cantidad) + '</span>' +
                '</div>';
            }).join('');

        var esNuevaMesa = !state.mesaActual || !state.mesaActual.pedidoActualId;
        var titulo = esNuevaMesa
            ? 'Nueva orden — Mesa ' + (state.mesaActual ? state.mesaActual.numero : '')
            : 'Agregar ítems';
        var sendDisabled = state.carrito.length === 0 ? ' disabled' : '';
<<<<<<< HEAD
=======
        var cerrarAgregarBtn = state.mesaActual
            ? '<button class="lmd-mesero-cerrar-cuenta" onclick="mesero.cerrarCuentaDesdeAgregar()">' + icon('receipt') + ' Cerrar</button>'
            : '';

>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="mesero.cerrarAgregar()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('plus-circle') + ' ' + titulo + '</span>' +
<<<<<<< HEAD
                '<div class="lmd-pos-ov-total">' + fmt(carritoTotal) + '</div>' +
=======
                '<div class="lmd-mesero-ov-total-actions">' + cerrarAgregarBtn + '<div class="lmd-pos-ov-total">' + fmt(carritoTotal) + '</div></div>' +
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
            '</div>' +
            '<div class="lmd-mesero-agregar-body">' +
                '<div class="lmd-mesero-agregar-izq">' +
                    '<div class="lmd-mesero-agregar-cats">' + catHtml + '</div>' +
                    '<div class="lmd-mesero-busqueda-wrap">' +
                        '<input type="search" class="lmd-mesero-busqueda" placeholder="Buscar producto..." ' +
                        'value="' + state.busquedaProd.replace(/"/g, '&quot;') + '" ' +
                        'oninput="mesero.buscarProd(this.value)" />' +
                    '</div>' +
                    '<div class="lmd-mesero-agregar-prods">' +
                        (prodHtml || '<div class="lmd-pos-empty">Sin productos</div>') +
                    '</div>' +
                '</div>' +
                '<div class="lmd-mesero-agregar-carrito">' +
                    '<div class="lmd-mesero-agregar-carrito__lista">' + carritoHtml + '</div>' +
                    '<div class="lmd-mesero-agregar-carrito__footer">' +
                        '<button class="lmd-mesero-btn lmd-mesero-btn--cobrar"' + sendDisabled + ' onclick="mesero.enviarItems()">' +
                            icon('send') + ' Enviar (' + state.carrito.length + ')' +
                        '</button>' +
                    '</div>' +
                '</div>' +
            '</div>';

        abrirOverlay('agregar', html, { wide: true, closeOnBackdrop: false });
    }

    function filtrarCat(cat) { state.catFiltro = cat; _renderAgregar(); }
    function cerrarAgregar() { cerrarOverlay('agregar'); }

    function addToCart(productoId) {
        var prods = window.__lmdProductosMesero || [];
        var prod  = prods.find(function (p) { return p.id === productoId; });
        if (!prod) return;
        var ex = state.carrito.find(function (i) { return i.id === productoId; });
        if (ex) ex.cantidad++;
        else state.carrito.push({ id: prod.id, nombre: prod.nombre, precio: prod.precio, cantidad: 1 });
        _renderAgregar();
    }

    function incCart(id) {
        var item = state.carrito.find(function (i) { return i.id === id; });
        if (item) { item.cantidad++; _renderAgregar(); }
    }

    function decCart(id) {
        var item = state.carrito.find(function (i) { return i.id === id; });
        if (!item) return;
        if (item.cantidad <= 1) state.carrito = state.carrito.filter(function (i) { return i.id !== id; });
        else item.cantidad--;
        _renderAgregar();
    }

    async function enviarItems() {
        var m = state.mesaActual;
        if (!m || state.carrito.length === 0) return;
        if (!m.pedidoActualId) { await _crearPedidoConItems(m); return; }
        var enviados = 0;
        var fallidos = [];
        try {
            for (var i = 0; i < state.carrito.length; i++) {
                var item = state.carrito[i];
                var res  = await postJson('AgregarLineaJson', {
                    pedidoId: m.pedidoActualId, productoId: item.id, cantidad: item.cantidad
                });
                if (!res.ok) {
                    var d = await res.json().catch(function () { return {}; });
                    fallidos.push({ item: item, error: d.error || 'Error al agregar' });
                } else {
                    enviados++;
                }
            }
            if (enviados > 0) {
                var r    = await fetch('?handler=DetallesPedidoJson&pedidoId=' + m.pedidoActualId,
                    { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
                var data = await r.json();
                state.pedidoDetalles = data.detalles || [];
                state.pedidoTotal    = data.total    || 0;
<<<<<<< HEAD
=======
                state.pedidoEstado   = data.estado   || state.pedidoEstado;
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
            }
            if (fallidos.length === 0) {
                state.carrito = [];
                cerrarOverlay('agregar');
                _renderDetalle();
<<<<<<< HEAD
                window.lmdToast('Ítems enviados a cocina', 'success');
=======
                notificar('Pedido enviado a cocina', 'success');
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
            } else {
                state.carrito = fallidos.map(function (f) { return f.item; });
                var msg = enviados > 0
                    ? enviados + ' ítem(s) enviado(s). ' + fallidos.length + ' no pudo(n) enviarse.'
                    : fallidos[0].error;
<<<<<<< HEAD
                window.lmdToast(msg, 'error');
                _renderAgregar();
            }
        } catch (e) { window.lmdToast('Error de conexión', 'error'); }
=======
                notificar(msg, 'error');
                _renderAgregar();
            }
        } catch (e) { notificar('Error de conexión', 'error'); }
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
    }

    // ═══════════════════════════════════════════════════
    // OVERLAY DE PAGO
    // ═══════════════════════════════════════════════════
    function abrirPago() {
        var total = state.pedidoTotal;
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="mesero.cerrarPago()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('credit-card') + ' Cobrar</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-pm-grid">' +
                '<button class="lmd-pos-pm-btn" onclick="mesero.abrirEfectivo(' + total.toFixed(2) + ')">' +
                    '<span class="lmd-pos-pm-btn__icon">' + icon('banknote') + '</span>' +
                    '<span class="lmd-pos-pm-btn__label">Efectivo</span>' +
                    '<span class="lmd-pos-pm-btn__sub">Billetes y monedas</span>' +
                '</button>' +
                '<button class="lmd-pos-pm-btn" onclick="mesero.abrirReferenciaPago(\'tarjeta\',' + total.toFixed(2) + ')">' +
                    '<span class="lmd-pos-pm-btn__icon">' + icon('credit-card') + '</span>' +
                    '<span class="lmd-pos-pm-btn__label">Tarjeta</span>' +
                    '<span class="lmd-pos-pm-btn__sub">Débito / Crédito</span>' +
                '</button>' +
                '<button class="lmd-pos-pm-btn" onclick="mesero.abrirReferenciaPago(\'qr\',' + total.toFixed(2) + ')">' +
                    '<span class="lmd-pos-pm-btn__icon">' + icon('qr-code') + '</span>' +
                    '<span class="lmd-pos-pm-btn__label">QR / Transferencia</span>' +
                    '<span class="lmd-pos-pm-btn__sub">Wompi, BAC, Niu</span>' +
                '</button>' +
            '</div>';
        abrirOverlay('pago', html, { closeOnBackdrop: false });
    }

    function cerrarPago() { cerrarOverlay('pago'); cerrarOverlay('efectivo'); cerrarOverlay('pago-ref'); }

    function abrirEfectivo(total) {
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="mesero.volverMetodos()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('banknote') + ' Efectivo</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-efectivo-body">' +
                '<label class="lmd-pos-efectivo-label">Monto recibido</label>' +
                '<input id="lmd-mesero-cash" class="lmd-pos-efectivo-input" type="number" step="0.01" ' +
                    'min="' + total.toFixed(2) + '" value="' + total.toFixed(2) + '" />' +
                '<div class="lmd-pos-qr-actions">' +
                    '<button class="lmd-pos-ov-btn" onclick="mesero.volverMetodos()">' + icon('arrow-left') + ' Volver</button>' +
                    '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--primary" onclick="mesero.confirmarEfectivo(' + total.toFixed(2) + ')">' +
                        icon('check-circle') + ' Confirmar</button>' +
                '</div>' +
            '</div>';
        cerrarOverlay('pago');
        abrirOverlay('efectivo', html, { closeOnBackdrop: false });
    }

    function volverMetodos() { cerrarOverlay('efectivo'); cerrarOverlay('pago-ref'); abrirPago(); }

    function confirmarEfectivo(total) {
        var input = document.getElementById('lmd-mesero-cash');
        var monto = parseFloat(input ? input.value : total);
<<<<<<< HEAD
        if (isNaN(monto) || monto < total) { window.lmdToast('Monto insuficiente', 'error'); return; }
=======
        if (isNaN(monto) || monto < total) { notificar('Monto insuficiente', 'error'); return; }
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
        pagarDirecto('efectivo', monto);
    }

    function abrirReferenciaPago(metodo, total) {
        var esTarjeta = metodo === 'tarjeta';
        var titulo = esTarjeta ? 'Referencia de tarjeta' : 'Referencia QR / transferencia';
        var ayuda = esTarjeta ? 'Ingresa el número de autorización del voucher.' : 'Ingresa la referencia visible de la transferencia.';
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="mesero.volverMetodos()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon(esTarjeta ? 'credit-card' : 'qr-code') + ' ' + titulo + '</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-efectivo-body">' +
                '<label class="lmd-pos-efectivo-label" for="lmd-mesero-payment-ref">Referencia</label>' +
                '<input id="lmd-mesero-payment-ref" class="lmd-pos-efectivo-input" type="text" autocomplete="off" placeholder="Ej. AUTH-123456" />' +
                '<p class="text-muted small mb-0">' + ayuda + '</p>' +
                '<div class="lmd-pos-qr-actions">' +
                    '<button class="lmd-pos-ov-btn" onclick="mesero.volverMetodos()">' + icon('arrow-left') + ' Volver</button>' +
                    '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--primary" onclick="mesero.confirmarReferenciaPago(\'' + metodo + '\',' + total.toFixed(2) + ')">' +
                        icon('check-circle') + ' Confirmar</button>' +
                '</div>' +
            '</div>';
        cerrarOverlay('pago');
        abrirOverlay('pago-ref', html, { closeOnBackdrop: false });
        setTimeout(function () { var input = document.getElementById('lmd-mesero-payment-ref'); if (input) input.focus(); }, 50);
    }

    function confirmarReferenciaPago(metodo, total) {
        var input = document.getElementById('lmd-mesero-payment-ref');
        var referencia = (input && input.value ? input.value : '').trim();
        if (!referencia) {
<<<<<<< HEAD
            window.lmdToast('La referencia del pago es obligatoria', 'error');
=======
            notificar('La referencia del pago es obligatoria', 'error');
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
            if (input) input.focus();
            return;
        }
        pagarDirecto(metodo, total, referencia);
    }

    async function pagarDirecto(metodo, monto, referencia) {
        var m = state.mesaActual;
        if (!m) return;
        try {
            var res  = await postJson('PagarJson', { pedidoId: m.pedidoActualId, metodoPago: metodo, monto: monto, referencia: referencia });
            var data = await res.json().catch(function () { return {}; });
<<<<<<< HEAD
            if (!res.ok) { window.lmdToast(data.error || 'Error al registrar pago', 'error'); return; }
=======
            if (!res.ok) { notificar(data.error || 'Error al registrar pago', 'error'); return; }
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
            cerrarOverlay('efectivo');
            cerrarOverlay('pago');
            cerrarOverlay('pago-ref');
            cerrarOverlay('detalle');
            await refrescarMesas();
            renderGrid();
<<<<<<< HEAD
            window.lmdToast(data.mensaje || 'Pago registrado', 'success');
        } catch (e) { window.lmdToast('Error de conexión', 'error'); }
=======
            notificar(data.mensaje || 'Pago registrado', 'success');
        } catch (e) { notificar('Error de conexión', 'error'); }
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
    }
    // SIGNALR
    // ═══════════════════════════════════════════════════
    async function sincronizarDetalle() {
        if (!state.mesaActual) return;
        var m = state.mesas.find(function (x) { return x.id === state.mesaActual.id; });
        if (!m || !m.pedidoActualId) { cerrarDetalle(); return; }
        state.mesaActual = m;
        try {
            var res = await fetch('?handler=DetallesPedidoJson&pedidoId=' + m.pedidoActualId,
                { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!res.ok) return;
            var data = await res.json();
            state.pedidoDetalles = data.detalles || [];
            state.pedidoTotal    = data.total    || 0;
            state.pedidoEstado   = data.estado   || m.pedidoEstado;
            _renderDetalle();
        } catch (e) {}
    }

    function initSignalR() {
        try {
            connection = new signalR.HubConnectionBuilder()
                .withUrl('/hubs/pedidos')
                .withAutomaticReconnect()
                .build();
            connection.on('PedidoCambiado', async function () { await refrescarMesas(); renderGrid(); await sincronizarDetalle(); });
<<<<<<< HEAD
            connection.on('EstadoCambiado', async function () { await refrescarMesas(); renderGrid(); await sincronizarDetalle(); });
=======
            connection.on('EstadoCambiado', async function (pedidoId, nuevoEstado) {
                if (nuevoEstado === 'Listo') notificar('Pedido listo para retirar — revisa la mesa.', 'success');
                await refrescarMesas(); renderGrid(); await sincronizarDetalle();
            });
            connection.on('RecibirNotificacionPedido', async function (payload) {
                if (payload && payload.tipo === 'EstadoCambiado' && payload.estado === 'Listo') {
                    notificar('Pedido listo para retirar — revisa la mesa.', 'success');
                }
                await refrescarMesas(); renderGrid(); await sincronizarDetalle();
            });
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
            connection.on('MesaActualizada', async function () { await refrescarMesas(); renderGrid(); await sincronizarDetalle(); });
            connection.start().catch(function (e) { console.warn('SignalR mesero:', e); });
        } catch (e) {}
    }

    function actualizarTiempos() {
        document.querySelectorAll('[data-pedido-fecha]').forEach(function (badge) {
            var span = badge.querySelector('.lmd-tab-tiempo');
            if (!span) return;
            var min = Math.floor((Date.now() - new Date(badge.dataset.pedidoFecha).getTime()) / 60000);
            span.textContent = min > 0 ? min + ' min' : 'Tab';
        });
    }

    var _graciaRefrescada = false;
    function actualizarTimersGracia() {
        var badges = document.querySelectorAll('[data-gracia-hasta]');
        var hayVencidos = false;
        badges.forEach(function (badge) {
            var span = badge.querySelector('.lmd-gracia-tiempo');
            if (!span) return;
            var secsLeft = Math.max(0, Math.floor((new Date(badge.dataset.graciaHasta).getTime() - Date.now()) / 1000));
            if (secsLeft <= 0) { hayVencidos = true; return; }
            var mins = Math.floor(secsLeft / 60), secs = secsLeft % 60;
            span.textContent = mins + ':' + (secs < 10 ? '0' : '') + secs;
        });
        if (hayVencidos && !_graciaRefrescada) {
            _graciaRefrescada = true;
            refrescarMesas()
                .then(function () { renderGrid(); })
                .catch(function () {})
                .finally(function () { _graciaRefrescada = false; });
        }
    }

    // ── Public API ─────────────────────────────────────
    window.mesero = {
<<<<<<< HEAD
        refrescarGrid,
        abrirDetalle, cerrarDetalle, voidItem, ajustarCantidad, pedirCuenta,
=======
        refrescarGrid, verNotificaciones, cerrarNotificaciones, abrirNotificacionMesa,
        abrirDetalle, cerrarDetalle, voidItem, ajustarCantidad, pedirCuenta, cerrarCuentaMesa, cerrarCuentaDesdeAgregar,
>>>>>>> 03333f6 (Modificaciones apartado mesero, el boton de cerrar funcion no funciona bien lo demas mas o menos, y el para que funcione debe iniciar un dia , y sibre el de tranferir mesas me dicen que debo hacer con ello)
        abrirNuevoPedido,
        abrirAgregar, cerrarAgregar, filtrarCat, buscarProd, addToCart, incCart, decCart, enviarItems,
        abrirPago, cerrarPago, abrirEfectivo, volverMetodos, confirmarEfectivo, abrirReferenciaPago, confirmarReferenciaPago, pagarDirecto
    };

    document.addEventListener('DOMContentLoaded', async function () {
        SHELL = document.getElementById('lmd-mesero-contenido');
        await refrescarMesas();
        renderGrid();
        initSignalR();
        setInterval(actualizarTiempos, 30000);
        setInterval(actualizarTimersGracia, 1000);
    });
})();
