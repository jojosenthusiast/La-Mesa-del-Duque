/* ═══════════════════════════════════════════════════════
   POS — La Mesa del Duque  v4.0
   Workflow: Selección → Productos (overlays de pago / docs)
   ═══════════════════════════════════════════════════════ */

(function () {
    // ── Lucide SVG helper ──────────────────────────────
    function icon(name, cls) {
        return '<svg class="lmd-icon ' + (cls || '') + '" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><use href="https://cdn.jsdelivr.net/npm/lucide-static@latest/icons/' + name + '.svg#icon"/></svg>';
    }

    // ── Helpers ─────────────────────────────────────────
    function fmt(n) {
        return new Intl.NumberFormat('es-SV', { style: 'currency', currency: 'USD' }).format(n || 0);
    }
    function totalLineas(lineas) {
        return lineas.reduce(function (s, l) { return s + (l.precioUnitario || 0) * (l.cantidad || 0); }, 0);
    }

    // ── State ──────────────────────────────────────────
    const state = {
        pantalla: 'seleccion',     // seleccion | productos
        tipoServicio: null,
        mesaId: null,
        mesaNumero: null,
        lineas: [],
        pedidoActual: null,        // { id }
        pagado: false,
        pagoMetodo: null,
        pagoMonto: null,
        pagoReferencia: null,
        // split payment
        split: {
            activo: false,
            personas: [],          // [{ id, nombre, items: [], metodoPago, monto, pagado }]
            personaActual: 0
        }
    };

    const SHELL = document.getElementById('lmd-pos-contenido');
    let connection = null;
    let keypadValue = '0';
    let _creandoPedido = false;

    // ── Screen machine ──────────────────────────────────
    function mostrarPantalla(nombre) {
        state.pantalla = nombre;
        document.querySelectorAll('.lmd-pos-screen').forEach(function (s) {
            s.classList.remove('lmd-pos-screen--activa');
        });
        var el = document.getElementById('lmd-pos-screen-' + nombre);
        if (el) el.classList.add('lmd-pos-screen--activa');
    }

    // ── Overlay system ──────────────────────────────────
    function abrirOverlay(id, html, opts) {
        cerrarOverlay(id);
        opts = opts || {};
        var ov = document.createElement('div');
        ov.id = 'lmd-ov-' + id;
        ov.className = 'lmd-pos-overlay' + (opts.bottom ? ' lmd-pos-overlay--bottom' : '');
        if (opts.closeOnBackdrop !== false) {
            ov.addEventListener('click', function (e) {
                if (e.target === ov) cerrarOverlay(id);
            });
        }
        var panel = document.createElement('div');
        panel.className = 'lmd-pos-ov-panel' + (opts.wide ? ' lmd-pos-ov-panel--wide' : '') + (opts.bottom ? ' lmd-pos-ov-panel--bottom' : '');
        panel.innerHTML = html;
        ov.appendChild(panel);
        document.body.appendChild(ov);
        // animate in
        requestAnimationFrame(function () { ov.classList.add('lmd-pos-overlay--visible'); });
    }

    function cerrarOverlay(id) {
        var el = document.getElementById('lmd-ov-' + id);
        if (el) el.remove();
    }

    function cerrarTodasOverlaysPago() {
        ['pago', 'efectivo', 'tarjeta', 'qr', 'otro', 'split', 'splitdetalle'].forEach(cerrarOverlay);
    }

    // ═══════════════════════════════════════════════════
    // SCREEN 1 — Selección
    // ═══════════════════════════════════════════════════
    function renderSeleccion() {
        var mesas = window.__lmdMesasDisponibles || [];
        // Group by capacity, sort ascending within each group
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
                var cls = disponible ? 'lmd-pos-mesa-card--disponible' : 'lmd-pos-mesa-card--ocupada';
                mesasHtml += '<div class="lmd-pos-mesa-card ' + cls + '" onclick="pos.seleccionarMesa(\'' + m.id + '\',' + m.numero + ')">' +
                    '<span class="lmd-pos-mesa-card__numero">' + m.numero + '</span>' +
                    '<span class="lmd-pos-mesa-card__capacidad">' + m.capacidad + ' pax</span>' +
                    (m.zona ? '<span class="lmd-pos-mesa-card__zona">' + m.zona + '</span>' : '') +
                '</div>';
            });
        });

        var html = '<div class="lmd-pos-seleccion">' +
            '<div class="lmd-pos-seleccion__mitad lmd-pos-seleccion__comer-aqui">' +
                '<div class="lmd-pos-seleccion__header">' + icon('utensils-crossed') + ' Comer aquí</div>' +
                '<div class="lmd-pos-mesas-grid">' + (mesasHtml || '<div class="lmd-pos-empty">Sin mesas disponibles</div>') + '</div>' +
            '</div>' +
            '<div class="lmd-pos-seleccion__mitad lmd-pos-seleccion__para-llevar" onclick="pos.seleccionarParaLlevar()">' +
                '<div class="lmd-pos-seleccion__header">' + icon('package') + ' Para llevar</div>' +
                '<div class="lmd-pos-para-llevar-card">' +
                    '<div class="lmd-pos-para-llevar-card__icon">' + icon('shopping-bag') + '</div>' +
                    '<div class="lmd-pos-para-llevar-card__titulo">Para llevar</div>' +
                    '<div class="lmd-pos-para-llevar-card__sub">Toca para iniciar sin mesa</div>' +
                '</div>' +
            '</div>' +
        '</div>';

        SHELL.innerHTML = '<div class="lmd-pos-shell">' +
            '<div class="lmd-pos-screen lmd-pos-screen--activa" id="lmd-pos-screen-seleccion">' + html + '</div>' +
            '<div class="lmd-pos-screen" id="lmd-pos-screen-productos"></div>' +
        '</div>';
    }

    function seleccionarMesa(mesaId, numero) {
        var mesas = window.__lmdMesasDisponibles || [];
        var m = mesas.find(function (x) { return x.id === mesaId; });
        if (m && m.estado !== 'Disponible') { lmdToast('Mesa ocupada — selecciona otra', 'error'); return; }
        state.tipoServicio = 'ComerAqui';
        state.mesaId = mesaId;
        state.mesaNumero = numero;
        _resetPedido();
        renderProductos();
        mostrarPantalla('productos');
    }

    function seleccionarParaLlevar() {
        state.tipoServicio = 'ParaLlevar';
        state.mesaId = null;
        state.mesaNumero = null;
        _resetPedido();
        renderProductos();
        mostrarPantalla('productos');
    }

    function _resetPedido() {
        state.lineas = [];
        state.pedidoActual = null;
        state.pagado = false;
        state.pagoMetodo = null;
        state.pagoMonto = null;
        state.pagoReferencia = null;
        state.split = { activo: false, personas: [], personaActual: 0 };
        keypadValue = '0';
    }

    // ═══════════════════════════════════════════════════
    // SCREEN 2 — Productos (25 / 50 / 25)
    // ═══════════════════════════════════════════════════
    function renderProductos() {
        var prods = window.__lmdProductosDisponibles || [];
        var cats = ['Todos'];
        var seen = {};
        prods.forEach(function (p) {
            var cn = p.categoriaNombre || 'Sin categoría';
            if (!seen[cn]) { seen[cn] = true; cats.push(cn); }
        });
        cats.sort(function (a, b) { return a === 'Todos' ? -1 : b === 'Todos' ? 1 : a.localeCompare(b); });

        var catHtml = cats.map(function (c) {
            return '<button class="lmd-pos-cat-btn' + (c === 'Todos' ? ' lmd-pos-cat-btn--activa' : '') + '" data-cat="' + c + '" onclick="pos.filtrarCategoria(\'' + c.replace(/'/g, "\\'") + '\')">' +
                icon(c === 'Todos' ? 'layers' : c === 'Bebidas' ? 'wine' : c === 'Postres' ? 'cake-slice' : 'utensils') + '<span>' + c + '</span>' +
            '</button>';
        }).join('');

        var productosHtml = renderProductGrid(prods);
        var total = totalLineas(state.lineas);
        var hayItems = state.lineas.length > 0;

        var cartItemsHtml = !hayItems
            ? '<div class="lmd-pos-cart__empty">' + icon('shopping-cart') + '<span>Carrito vacío</span></div>'
            : state.lineas.map(function (l, i) {
                return '<div class="lmd-pos-cart-item">' +
                    '<div class="lmd-pos-cart-item__info">' +
                        '<span class="lmd-pos-cart-item__nombre">' + (l.productoNombre || l.nombre || '') + (l.tieneModificaciones ? '<span class="lmd-pos-mod-dot" title="Tiene modificaciones"></span>' : '') + '</span>' +
                        '<span class="lmd-pos-cart-item__precio">' + fmt((l.precioUnitario || 0) * (l.cantidad || 0)) + '</span>' +
                    '</div>' +
                    '<div class="lmd-pos-cart-item__controles">' +
                        (state.pagado
                            ? '<span class="lmd-pos-cart-item__qty lmd-pos-cart-item__qty--locked">x' + l.cantidad + '</span>'
                            : '<button class="lmd-pos-cart-item__qty-btn" onclick="pos.decrementarItem(' + i + ')">' + icon('minus') + '</button>' +
                              '<span class="lmd-pos-cart-item__qty">' + l.cantidad + '</span>' +
                              '<button class="lmd-pos-cart-item__qty-btn" onclick="pos.incrementarItem(' + i + ')">' + icon('plus') + '</button>' +
                              '<button class="lmd-pos-cart-item__remove" onclick="pos.eliminarDelCarrito(' + i + ')">' + icon('x') + '</button>'
                        ) +
                    '</div>' +
                '</div>';
              }).join('');

        var pagarLabel = state.pagado ? icon('check-circle') + ' Pagado' : icon('credit-card') + ' Pagar';
        var listoLabel = state.pagado
            ? icon('receipt') + ' Finalizar'
            : state.pedidoActual
                ? icon('plus-circle') + ' Enviar más'
                : icon('send') + ' Enviar a cocina';

        var html = '<div class="lmd-pos-productos">' +
            '<div class="lmd-pos-categorias" id="lmd-pos-categorias">' + catHtml + '</div>' +
            '<div class="lmd-pos-productos-grid" id="lmd-pos-productos-grid">' + productosHtml + '</div>' +
            '<div class="lmd-pos-cart">' +
                '<div class="lmd-pos-cart__header">' +
                    icon('shopping-bag') +
                    '<span>' + (state.tipoServicio === 'ComerAqui' ? 'Mesa ' + state.mesaNumero : 'Para llevar') + '</span>' +
                    (state.pedidoActual && !state.pagado ? '<span class="lmd-pos-tab-activo-badge">' + icon('clock') + ' Tab activo</span>' : '') +
                    (!state.pagado ? '<button class="lmd-pos-cart-change-servicio" onclick="pos.cambiarServicio()" title="Cambiar tipo de servicio">' + icon('refresh-cw') + '</button>' : '') +
                    (state.pagado ? '<span class="lmd-pos-pagado-badge">' + icon('check-circle') + ' Pagado</span>' : '') +
                '</div>' +
                '<div class="lmd-pos-cart__items" id="lmd-pos-cart-items">' + cartItemsHtml + '</div>' +
                '<div class="lmd-pos-cart__total">' + fmt(total) + '</div>' +
                '<div class="lmd-pos-cart__acciones">' +
                    '<button class="lmd-pos-cart-btn lmd-pos-cart-btn--listo" onclick="pos.confirmarListo()"' + (!hayItems ? ' disabled' : '') + '>' + listoLabel + '</button>' +
                    '<button class="lmd-pos-cart-btn lmd-pos-cart-btn--cancelar" onclick="pos.cancelarOrden()">' + icon('x-circle') + ' Cancelar</button>' +
                    '<button class="lmd-pos-cart-btn lmd-pos-cart-btn--pagar' + (state.pagado ? ' lmd-pos-cart-btn--pagado' : '') + '" onclick="pos.irAPago()"' + ((!hayItems && !state.pedidoActual) || state.pagado ? ' disabled' : '') + '>' + pagarLabel + '</button>' +
                    (state.pagado ? '<button class="lmd-pos-cart-btn lmd-pos-cart-btn--anular" onclick="pos.confirmarAnulacion()">' + icon('rotate-ccw') + ' Anular pago</button>' : '') +
                '</div>' +
            '</div>' +
        '</div>';

        document.getElementById('lmd-pos-screen-productos').innerHTML = html;
    }

    function renderProductGrid(prods, catFiltro) {
        var filtered = catFiltro && catFiltro !== 'Todos'
            ? prods.filter(function (p) { return (p.categoriaNombre || 'Sin categoría') === catFiltro; })
            : prods;
        if (filtered.length === 0) return '<div class="lmd-pos-empty">Sin productos en esta categoría</div>';
        return filtered.map(function (p) {
            var agotado = p.agotado === true;
            var ico = p.categoriaNombre === 'Bebidas' ? 'wine' : p.categoriaNombre === 'Postres' ? 'cake-slice' : 'utensils';
            return '<div class="lmd-pos-producto-card' + (agotado ? ' lmd-pos-producto-card--agotado' : '') + '">' +
                '<div class="lmd-pos-producto-card__body" onclick="' + (agotado || state.pagado ? '' : 'pos.agregarAlCarrito(\'' + p.id + '\',\'' + (p.nombre || '').replace(/'/g, "\\'") + '\',' + (p.precio || 0) + ')') + '">' +
                    '<div class="lmd-pos-producto-card__ico">' + icon(ico) + '</div>' +
                    '<span class="lmd-pos-producto-card__nombre">' + (p.nombre || '') + '</span>' +
                    '<span class="lmd-pos-producto-card__precio">' + fmt(p.precio || 0) + '</span>' +
                    (p.tiempoPreparacionMin ? '<span class="lmd-pos-producto-card__tiempo">' + p.tiempoPreparacionMin + ' min</span>' : '') +
                    (agotado ? '<span class="lmd-pos-producto-card__agotado-badge">Agotado</span>' : '') +
                '</div>' +
                '<button class="lmd-pos-producto-card__editar" onclick="pos.abrirModificadores(\'' + p.id + '\')" title="Editar ingredientes">' + icon('edit-3') + '</button>' +
            '</div>';
        }).join('');
    }

    function filtrarCategoria(cat) {
        document.querySelectorAll('.lmd-pos-cat-btn').forEach(function (b) {
            b.classList.toggle('lmd-pos-cat-btn--activa', b.dataset.cat === cat);
        });
        var prods = window.__lmdProductosDisponibles || [];
        document.getElementById('lmd-pos-productos-grid').innerHTML = renderProductGrid(prods, cat);
    }

    function agregarAlCarrito(prodId, nombre, precio) {
        if (state.pagado) return;
        var existente = state.lineas.find(function (l) { return l.productoId === prodId; });
        if (existente) { existente.cantidad += 1; }
        else { state.lineas.push({ productoId: prodId, productoNombre: nombre, cantidad: 1, precioUnitario: precio }); }
        renderProductos();
        mostrarPantalla('productos');
    }

    function incrementarItem(idx) {
        if (state.pagado) return;
        if (state.lineas[idx]) { state.lineas[idx].cantidad += 1; renderProductos(); }
    }

    function decrementarItem(idx) {
        if (state.pagado) return;
        if (!state.lineas[idx]) return;
        if (state.lineas[idx].cantidad > 1) { state.lineas[idx].cantidad -= 1; renderProductos(); }
        else { eliminarDelCarrito(idx); }
    }

    function eliminarDelCarrito(idx) {
        if (state.pagado) return;
        state.lineas.splice(idx, 1);
        renderProductos();
    }

    async function cancelarOrden() {
        if (state.lineas.length === 0 && !state.pedidoActual) return;
        var ok = await window.lmdConfirm('¿Cancelar esta orden?');
        if (!ok) return;

        if (state.pedidoActual && state.pedidoActual.id) {
            var csrf = document.querySelector('input[name="__RequestVerificationToken"]');
            var form = new FormData();
            form.append('__RequestVerificationToken', csrf ? csrf.value : '');
            form.append('pedidoId', state.pedidoActual.id);
            try {
                await fetch('?handler=CancelarJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            } catch (e) { /* continúa aunque falle el server */ }
        }

        nuevaOrden();
    }

    async function confirmarAnulacion() {
        if (!state.pagado || !state.pedidoActual) return;
        var ok = await window.lmdConfirm('¿Anular el pago de esta orden? El stock no se revertirá.');
        if (!ok) return;

        var csrf = document.querySelector('input[name="__RequestVerificationToken"]');
        var form = new FormData();
        form.append('__RequestVerificationToken', csrf ? csrf.value : '');
        form.append('pedidoId', state.pedidoActual.id);
        try {
            var res = await fetch('?handler=AnularPagoJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!res.ok) { var d = await res.json().catch(function() { return {}; }); lmdToast(d.error || 'Error al anular', 'error'); return; }
            lmdToast('Pago anulado', 'success');
            nuevaOrden();
        } catch (e) { lmdToast('Error de conexión', 'error'); }
    }

    // ═══════════════════════════════════════════════════
    // LISTO — enviar a cocina (+ docs si está pagado)
    // ═══════════════════════════════════════════════════
    async function confirmarListo() {
        if (state.lineas.length === 0) { lmdToast('Agrega productos primero', 'error'); return; }

        if (state.pagado) { abrirOverlayDocumentos(); return; }

        if (_creandoPedido) return;
        _creandoPedido = true;

        var csrf = document.querySelector('input[name="__RequestVerificationToken"]');

        // Tab abierto: agregar items nuevos al pedido existente
        if (state.pedidoActual) {
            var formMas = new FormData();
            formMas.append('__RequestVerificationToken', csrf ? csrf.value : '');
            formMas.append('pedidoId', state.pedidoActual.id);
            state.lineas.forEach(function (l, i) {
                formMas.append('Vm.CrearPedido.Lineas[' + i + '].ProductoId', l.productoId);
                formMas.append('Vm.CrearPedido.Lineas[' + i + '].Cantidad', l.cantidad || 1);
                if (l.notas) formMas.append('Vm.CrearPedido.Lineas[' + i + '].Notas', l.notas);
                if (l.modificacionesJson) formMas.append('Vm.CrearPedido.Lineas[' + i + '].ModificacionesJson', l.modificacionesJson);
            });
            try {
                var resMas = await fetch('?handler=EnviarMasJson', { method: 'POST', body: formMas, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
                if (resMas.ok) {
                    lmdToast('Items enviados a cocina', 'success');
                    state.pedidoActual.total = (state.pedidoActual.total || 0) + totalLineas(state.lineas);
                    state.lineas = [];
                    renderProductos();
                } else {
                    var errMas = await resMas.json().catch(function () { return null; });
                    lmdToast(errMas && errMas.error ? errMas.error : 'Error al enviar items', 'error');
                }
            } catch (e) { lmdToast('Error al enviar items', 'error'); }
            _creandoPedido = false;
            return;
        }

        // Sin tab: crear pedido nuevo
        var form = new FormData();
        form.append('__RequestVerificationToken', csrf ? csrf.value : '');
        form.append('Vm.CrearPedido.TipoServicio', state.tipoServicio || 'ComerAqui');
        if (state.mesaId) form.append('Vm.CrearPedido.MesaId', state.mesaId);
        state.lineas.forEach(function (l, i) {
            form.append('Vm.CrearPedido.Lineas[' + i + '].ProductoId', l.productoId);
            form.append('Vm.CrearPedido.Lineas[' + i + '].Cantidad', l.cantidad || 1);
            if (l.notas) form.append('Vm.CrearPedido.Lineas[' + i + '].Notas', l.notas);
            if (l.modificacionesJson) form.append('Vm.CrearPedido.Lineas[' + i + '].ModificacionesJson', l.modificacionesJson);
        });
        try {
            var res = await fetch('?handler=CrearJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            var data = await res.json();
            if (data.pedidoId) {
                lmdToast('Pedido enviado a cocina', 'success');
                if (state.tipoServicio === 'ParaLlevar') {
                    _creandoPedido = false;
                    nuevaOrden();
                } else {
                    state.pedidoActual = { id: data.pedidoId, total: totalLineas(state.lineas) };
                    state.lineas = [];
                    _creandoPedido = false;
                    renderProductos();
                }
                return;
            }
        } catch (e) { lmdToast('Error al enviar pedido', 'error'); }
        _creandoPedido = false;

    }

    // ═══════════════════════════════════════════════════
    // PAGO — overlay de 6 métodos
    // ═══════════════════════════════════════════════════
    async function irAPago() {
        if (state.pagado) return;
        if (_creandoPedido) return;

        // Tab abierto con items pendientes: enviarlos a cocina primero, luego cobrar
        if (state.pedidoActual && state.lineas.length > 0) {
            await confirmarListo();
            if (state.lineas.length > 0) return; // envio fallo
        }

        if (!state.pedidoActual && state.lineas.length === 0) {
            lmdToast('Agrega productos primero', 'error');
            return;
        }

        if (!state.pedidoActual) {
            _creandoPedido = true;
            var csrf = document.querySelector('input[name="__RequestVerificationToken"]');
            var form = new FormData();
            form.append('__RequestVerificationToken', csrf ? csrf.value : '');
            form.append('Vm.CrearPedido.TipoServicio', state.tipoServicio || 'ComerAqui');
            if (state.mesaId) form.append('Vm.CrearPedido.MesaId', state.mesaId);
            state.lineas.forEach(function (l, i) {
                form.append('Vm.CrearPedido.Lineas[' + i + '].ProductoId', l.productoId);
                form.append('Vm.CrearPedido.Lineas[' + i + '].Cantidad', l.cantidad || 1);
                if (l.notas) form.append('Vm.CrearPedido.Lineas[' + i + '].Notas', l.notas);
                if (l.modificacionesJson) form.append('Vm.CrearPedido.Lineas[' + i + '].ModificacionesJson', l.modificacionesJson);
            });
            try {
                var res = await fetch('?handler=CrearJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
                var data = await res.json();
                if (data.pedidoId) state.pedidoActual = { id: data.pedidoId };
                else { lmdToast('No se pudo crear el pedido', 'error'); _creandoPedido = false; return; }
            } catch (e) { lmdToast('Error al crear pedido', 'error'); _creandoPedido = false; return; }
            _creandoPedido = false;
        }

        abrirOverlayPago();
    }

    var METODOS_PAGO = [
        { codigo: 'efectivo',  label: 'Efectivo',    icon: 'banknote',   sub: 'Cambio automático',  cls: 'lmd-pos-pm--efectivo' },
        { codigo: 'tarjeta',   label: 'Tarjeta',     icon: 'credit-card', sub: 'Débito / Crédito',  cls: 'lmd-pos-pm--tarjeta'  },
        { codigo: 'qr',        label: 'QR / Transf.', icon: 'qr-code',   sub: 'Wompi · BAC · Niu',  cls: 'lmd-pos-pm--qr'      },
        { codigo: 'credito',   label: 'Crédito Emp.', icon: 'building-2', sub: 'Cuenta corriente',  cls: ''                     },
        { codigo: 'vale',      label: 'Vale',         icon: 'ticket',    sub: 'Voucher alimentación', cls: ''                    },
        { codigo: 'cortesia',  label: 'Cortesía',     icon: 'gift',      sub: 'Invitación ($0)',     cls: ''                     }
    ];

    function abrirOverlayPago() {
        var total = (state.pedidoActual && state.pedidoActual.total)
            ? state.pedidoActual.total + totalLineas(state.lineas)
            : totalLineas(state.lineas);
        var btnsHtml = METODOS_PAGO.map(function (m) {
            return '<button class="lmd-pos-pm-btn ' + (m.cls || '') + '" onclick="pos.procesarPago(\'' + m.codigo + '\',' + total.toFixed(2) + ')">' +
                '<span class="lmd-pos-pm-btn__icon">' + icon(m.icon) + '</span>' +
                '<span class="lmd-pos-pm-btn__label">' + m.label + '</span>' +
                '<span class="lmd-pos-pm-btn__sub">' + m.sub + '</span>' +
            '</button>';
        }).join('');

        var html =
            '<div class="lmd-pos-ov-header">' +
                '<span class="lmd-pos-ov-title">' + icon('credit-card') + ' Seleccionar pago</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
                '<button class="lmd-pos-ov-close" onclick="pos.cerrarPago()">' + icon('x') + '</button>' +
            '</div>' +
            '<div class="lmd-pos-pm-grid">' + btnsHtml + '</div>' +
            '<div class="lmd-pos-ov-footer">' +
                '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--split" onclick="pos.abrirSplit()">' + icon('users') + ' Dividir cuenta</button>' +
            '</div>';

        abrirOverlay('pago', html, { wide: true, closeOnBackdrop: false });
    }

    function cerrarPago() {
        cerrarTodasOverlaysPago();
    }

    function procesarPago(metodo, total) {
        if (metodo === 'efectivo') {
            abrirOverlayEfectivo(total);
        } else if (metodo === 'tarjeta') {
            abrirOverlayTarjeta(total);
        } else if (metodo === 'qr') {
            abrirOverlayQR(total);
        } else if (metodo === 'cortesia') {
            finalizarPago(metodo, 0, null);
        } else {
            // Crédito, Vale
            abrirOverlayOtro(metodo, total);
        }
    }

    // ── Efectivo: keypad + shortcuts + cambio ──────────
    function abrirOverlayEfectivo(total) {
        keypadValue = '0';
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos.volverAMetodos()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('banknote') + ' Efectivo</span>' +
                '<div class="lmd-pos-ov-total">Total: ' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-efectivo-body">' +
                '<div class="lmd-pos-bill-shortcuts">' +
                    '<button class="lmd-pos-bill-btn lmd-pos-bill-btn--exacto" onclick="pos.seleccionarBillete(' + total.toFixed(2) + ',' + total.toFixed(2) + ')">Exacto</button>' +
                    [5, 10, 20, 50, 100].map(function (b) {
                        return '<button class="lmd-pos-bill-btn" onclick="pos.seleccionarBillete(' + b + ',' + total.toFixed(2) + ')">' + icon('banknote') + ' $' + b + '</button>';
                    }).join('') +
                '</div>' +
                '<div class="lmd-pos-keypad" id="lmd-pos-keypad">' +
                    '<div class="lmd-pos-keypad__display" id="lmd-pos-keypad-display">$0.00</div>' +
                    '<div class="lmd-pos-cambio" id="lmd-pos-cambio"></div>' +
                    '<div class="lmd-pos-keypad__grid">' +
                        [1,2,3,4,5,6,7,8,9,'.',0,'⌫'].map(function (k) {
                            return '<button class="lmd-pos-keypad__btn" onclick="pos.keypadInput(\'' + k + '\',' + total.toFixed(2) + ')">' + (k === '⌫' ? icon('delete') : k) + '</button>';
                        }).join('') +
                    '</div>' +
                    '<div class="lmd-pos-keypad__actions">' +
                        '<button class="lmd-pos-keypad__btn lmd-pos-keypad__btn--back" onclick="pos.volverAMetodos()">Volver</button>' +
                        '<button class="lmd-pos-keypad__btn lmd-pos-keypad__btn--confirm" id="lmd-keypad-confirmar" onclick="pos.keypadConfirmar(' + total.toFixed(2) + ')" disabled>Confirmar</button>' +
                    '</div>' +
                '</div>' +
            '</div>';

        cerrarOverlay('pago');
        abrirOverlay('efectivo', html, { closeOnBackdrop: false });
    }

    function seleccionarBillete(valor, total) {
        keypadValue = parseFloat(valor).toFixed(2).replace(/\.00$/, '');
        _actualizarKeypad(total);
    }

    function keypadInput(k, total) {
        if (k === '⌫') {
            keypadValue = keypadValue.length > 1 ? keypadValue.slice(0, -1) : '0';
        } else if (k === '.') {
            if (keypadValue.indexOf('.') === -1) keypadValue += '.';
        } else {
            // Max 2 decimal places
            var parts = keypadValue.split('.');
            if (parts[1] && parts[1].length >= 2) return;
            keypadValue = keypadValue === '0' ? '' + k : keypadValue + k;
        }
        _actualizarKeypad(total);
    }

    function _actualizarKeypad(total) {
        var recibido = parseFloat(keypadValue || '0');
        var display = document.getElementById('lmd-pos-keypad-display');
        var cambioEl = document.getElementById('lmd-pos-cambio');
        var confirmar = document.getElementById('lmd-keypad-confirmar');
        if (display) display.textContent = '$' + (recibido || 0).toFixed(2);
        if (cambioEl) {
            var cambio = recibido - total;
            if (recibido <= 0) {
                cambioEl.textContent = '';
                cambioEl.className = 'lmd-pos-cambio';
            } else if (cambio >= 0) {
                cambioEl.textContent = 'Cambio: ' + fmt(cambio);
                cambioEl.className = 'lmd-pos-cambio lmd-pos-cambio--ok';
            } else {
                cambioEl.textContent = 'Faltan: ' + fmt(-cambio);
                cambioEl.className = 'lmd-pos-cambio lmd-pos-cambio--falta';
            }
        }
        if (confirmar) confirmar.disabled = recibido < total;
    }

    function keypadConfirmar(total) {
        var recibido = parseFloat(keypadValue || '0');
        if (recibido < total) { lmdToast('Monto insuficiente', 'error'); return; }
        cerrarOverlay('efectivo');
        finalizarPago('efectivo', recibido, null);
    }

    function volverAMetodos() {
        cerrarOverlay('efectivo');
        cerrarOverlay('tarjeta');
        cerrarOverlay('qr');
        cerrarOverlay('otro');
        cerrarOverlay('errorpago');
        abrirOverlayPago();
    }

    // ── Tarjeta ────────────────────────────────────────
    function abrirOverlayTarjeta(total) {
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos.volverAMetodos()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('credit-card') + ' Tarjeta</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-tarjeta-body">' +
                '<div class="lmd-pos-tarjeta-ilustracion">' + icon('credit-card') + '</div>' +
                '<p class="lmd-pos-tarjeta-instruccion">Acerca o inserta la tarjeta en el terminal</p>' +
                '<p class="lmd-pos-tarjeta-monto">' + fmt(total) + '</p>' +
                '<div class="lmd-pos-tarjeta-tipos">' +
                    '<span class="lmd-pos-tarjeta-chip">Visa</span>' +
                    '<span class="lmd-pos-tarjeta-chip">Mastercard</span>' +
                    '<span class="lmd-pos-tarjeta-chip">Amex</span>' +
                    '<span class="lmd-pos-tarjeta-chip">Débito</span>' +
                '</div>' +
                '<div class="lmd-pos-tarjeta-actions">' +
                    '<button class="lmd-pos-ov-btn" onclick="pos.volverAMetodos()">' + icon('arrow-left') + ' Volver</button>' +
                    '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--danger" onclick="pos.simularRechazo(\'tarjeta\')">' + icon('x-circle') + ' Simular rechazo</button>' +
                    '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--primary" onclick="pos.simularTarjeta(' + total.toFixed(2) + ')">' + icon('check-circle') + ' Confirmar pago</button>' +
                '</div>' +
            '</div>';

        cerrarOverlay('pago');
        abrirOverlay('tarjeta', html, { closeOnBackdrop: false });
    }

    function simularTarjeta(total) {
        cerrarOverlay('tarjeta');
        lmdToast('Tarjeta procesada correctamente', 'success');
        finalizarPago('tarjeta', total, 'TARJ-' + Date.now().toString(36).toUpperCase());
    }

    // ── QR / Transferencia ─────────────────────────────
    function abrirOverlayQR(total) {
        var ref = 'QR-' + Date.now().toString(36).toUpperCase();
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos.volverAMetodos()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('qr-code') + ' QR / Transferencia</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-qr-body">' +
                '<div class="lmd-pos-qr-box">' +
                    '<div class="lmd-pos-qr-simulado">' +
                        '<div class="lmd-pos-qr-patron"></div>' +
                    '</div>' +
                    '<p class="lmd-pos-qr-ref">Ref: ' + ref + '</p>' +
                    '<p class="lmd-pos-qr-monto">' + fmt(total) + '</p>' +
                '</div>' +
                '<p class="lmd-pos-qr-instruccion">Escanea con Wompi, BAC Credomatic o Niu</p>' +
                '<div class="lmd-pos-qr-actions">' +
                    '<button class="lmd-pos-ov-btn" onclick="pos.volverAMetodos()">' + icon('arrow-left') + ' Volver</button>' +
                    '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--danger" onclick="pos.simularRechazo(\'qr\')">' + icon('x-circle') + ' Simular rechazo</button>' +
                    '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--primary" onclick="pos.simularQR(\'' + ref + '\',' + total.toFixed(2) + ')">' + icon('check-circle') + ' Confirmar recepción</button>' +
                '</div>' +
            '</div>';

        cerrarOverlay('pago');
        abrirOverlay('qr', html, { closeOnBackdrop: false });
    }

    function simularQR(ref, total) {
        cerrarOverlay('qr');
        lmdToast('Transferencia confirmada — ref: ' + ref, 'success');
        finalizarPago('qr', total, ref);
    }

    // ── Otros métodos ──────────────────────────────────
    function abrirOverlayOtro(metodo, total) {
        var label = metodo === 'credito' ? 'Crédito Empresarial' : 'Vale';
        var ref = metodo.toUpperCase() + '-' + Date.now().toString(36).toUpperCase();
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos.volverAMetodos()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + label + '</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-otro-body">' +
                '<p>Ref: ' + ref + '</p>' +
                '<p class="lmd-pos-qr-monto">' + fmt(total) + '</p>' +
                '<div class="lmd-pos-qr-actions">' +
                    '<button class="lmd-pos-ov-btn" onclick="pos.volverAMetodos()">' + icon('arrow-left') + ' Volver</button>' +
                    '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--primary" onclick="pos.confirmarOtro(\'' + metodo + '\',' + total.toFixed(2) + ',\'' + ref + '\')">' + icon('check-circle') + ' Confirmar</button>' +
                '</div>' +
            '</div>';

        cerrarOverlay('pago');
        abrirOverlay('otro', html, { closeOnBackdrop: false });
    }

    function confirmarOtro(metodo, total, ref) {
        cerrarOverlay('otro');
        finalizarPago(metodo, total, ref);
    }

    // ═══════════════════════════════════════════════════
    // SPLIT PAYMENT
    // ═══════════════════════════════════════════════════
    function abrirSplit() {
        cerrarOverlay('pago');
        var total = (state.pedidoActual && state.pedidoActual.total)
            ? state.pedidoActual.total + totalLineas(state.lineas)
            : totalLineas(state.lineas);
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos.volverAPago()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('users') + ' Dividir cuenta</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-split-body">' +
                '<p class="lmd-pos-split-desc">¿Cómo dividir ' + fmt(total) + '?</p>' +
                '<div class="lmd-pos-split-opciones">' +
                    '<button class="lmd-pos-split-opcion" onclick="pos.splitIgualitario()">' +
                        icon('equal') +
                        '<span>Partes iguales</span>' +
                        '<small>Dividir el total entre N personas</small>' +
                    '</button>' +
                    '<button class="lmd-pos-split-opcion" onclick="pos.splitPorPersona()">' +
                        icon('user-check') +
                        '<span>Por persona</span>' +
                        '<small>Cada quien paga lo suyo</small>' +
                    '</button>' +
                    '<button class="lmd-pos-split-opcion" onclick="pos.splitMixto()">' +
                        icon('shuffle') +
                        '<span>Mixto</span>' +
                        '<small>Grupos personalizados</small>' +
                    '</button>' +
                '</div>' +
            '</div>';

        abrirOverlay('split', html, { closeOnBackdrop: false });
    }

    function volverAPago() {
        cerrarOverlay('split');
        cerrarOverlay('splitdetalle');
        abrirOverlayPago();
    }

    function splitIgualitario() {
        _splitN = 2;
        cerrarOverlay('split');
        var total = totalLineas(state.lineas);
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos.volverAPago()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('equal') + ' Partes iguales</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-split-igualitario">' +
                '<p>¿Entre cuántas personas?</p>' +
                '<div class="lmd-pos-split-counter">' +
                    '<button class="lmd-pos-split-counter__btn" onclick="pos.ajustarSplitN(-1)">' + icon('minus') + '</button>' +
                    '<span class="lmd-pos-split-counter__val" id="split-n">2</span>' +
                    '<button class="lmd-pos-split-counter__btn" onclick="pos.ajustarSplitN(1)">' + icon('plus') + '</button>' +
                '</div>' +
                '<div class="lmd-pos-split-preview" id="split-preview">' + _splitPreview(total, 2) + '</div>' +
                '<div class="lmd-pos-split-actions">' +
                    '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--primary" onclick="pos.iniciarSplitIgualitario(' + total.toFixed(2) + ')">' + icon('check-circle') + ' Iniciar cobro</button>' +
                '</div>' +
            '</div>';

        abrirOverlay('splitdetalle', html, { closeOnBackdrop: false });
    }

    var _splitN = 2;
    var _splitTipo = 'persona'; // 'persona' | 'mixto'
    var _splitItems = [];      // [{ nombre, cantidad, precio, persona }] persona: -1=sin asignar/compartido
    function ajustarSplitN(delta) {
        _splitN = Math.max(2, Math.min(10, _splitN + delta));
        var el = document.getElementById('split-n');
        if (el) el.textContent = _splitN;
        var total = totalLineas(state.lineas);
        var preview = document.getElementById('split-preview');
        if (preview) preview.innerHTML = _splitPreview(total, _splitN);
    }

    function _splitPreview(total, n) {
        var partes = Array.from({ length: n }, function (_, i) {
            var monto = (i < n - 1) ? Math.floor(total / n * 100) / 100 : total - Math.floor(total / n * 100) / 100 * (n - 1);
            return '<div class="lmd-pos-split-row"><span>Persona ' + (i + 1) + '</span><span>' + fmt(monto) + '</span></div>';
        }).join('');
        return partes;
    }

    function iniciarSplitIgualitario(total) {
        cerrarOverlay('splitdetalle');
        var n = _splitN;
        state.split.activo = true;
        state.split.personas = Array.from({ length: n }, function (_, i) {
            var monto = (i < n - 1) ? Math.floor(total / n * 100) / 100 : total - Math.floor(total / n * 100) / 100 * (n - 1);
            return { id: i, nombre: 'Persona ' + (i + 1), monto: monto, metodoPago: null, pagado: false };
        });
        state.split.personaActual = 0;
        cobrarSiguientePersona();
    }

    function splitPorPersona() {
        _splitN = 2; _splitTipo = 'persona';
        cerrarOverlay('split');
        _renderSplitNPicker();
    }

    function splitMixto() {
        _splitN = 2; _splitTipo = 'mixto';
        cerrarOverlay('split');
        _renderSplitNPicker();
    }

    function _renderSplitNPicker() {
        var total = totalLineas(state.lineas);
        var esMixto = _splitTipo === 'mixto';
        var titulo = esMixto ? 'División mixta' : 'Por persona';
        var icono = esMixto ? 'git-merge' : 'users';
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos.volverAPago()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon(icono) + ' ' + titulo + '</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-split-igualitario">' +
                '<p>' + (esMixto ? 'Asigná ítems por persona o como compartidos' : 'Asigná cada ítem a una persona') + '</p>' +
                '<div class="lmd-pos-split-counter">' +
                    '<button class="lmd-pos-split-counter__btn" onclick="pos.ajustarSplitN(-1)">' + icon('minus') + '</button>' +
                    '<span class="lmd-pos-split-counter__val" id="split-n">' + _splitN + '</span>' +
                    '<button class="lmd-pos-split-counter__btn" onclick="pos.ajustarSplitN(1)">' + icon('plus') + '</button>' +
                '</div>' +
                '<div class="lmd-pos-split-actions">' +
                    '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--primary" onclick="pos.iniciarAsignacionSplit()">' + icon('arrow-right') + ' Asignar ítems</button>' +
                '</div>' +
            '</div>';
        abrirOverlay('splitdetalle', html, { closeOnBackdrop: false });
    }

    function iniciarAsignacionSplit() {
        var esMixto = _splitTipo === 'mixto';
        _splitItems = state.lineas.map(function (l) {
            return { nombre: l.productoNombre || l.nombre || '?', cantidad: l.cantidad || 1, precio: (l.precioUnitario || 0) * (l.cantidad || 1), persona: esMixto ? -1 : -1 };
        });
        _renderSplitAsignacion();
    }

    function _renderSplitAsignacion() {
        var n = _splitN;
        var esMixto = _splitTipo === 'mixto';
        var headers = '';
        for (var hi = 0; hi < n; hi++) headers += '<th>P' + (hi + 1) + '</th>';
        if (esMixto) headers += '<th>' + icon('share-2') + '</th>';

        var rows = _splitItems.map(function (item, idx) {
            var celdas = '';
            for (var ci = 0; ci < n; ci++) {
                var activo = item.persona === ci;
                celdas += '<td><button class="lmd-pos-split-asig-btn' + (activo ? ' activo' : '') + '" onclick="pos.asignarItemSplit(' + idx + ',' + ci + ')">' + (activo ? icon('check') : '') + '</button></td>';
            }
            if (esMixto) {
                var comp = item.persona === -1;
                celdas += '<td><button class="lmd-pos-split-asig-btn lmd-pos-split-asig-btn--comp' + (comp ? ' activo' : '') + '" onclick="pos.asignarItemSplit(' + idx + ',-1)">' + (comp ? icon('check') : '') + '</button></td>';
            }
            var pendiente = !esMixto && item.persona < 0;
            return '<tr' + (pendiente ? ' class="lmd-pos-split-asig-row--pendiente"' : '') + '>' +
                '<td class="lmd-pos-split-asig-nombre">' + (item.cantidad > 1 ? item.cantidad + 'x ' : '') + item.nombre + '<span class="lmd-pos-split-asig-precio">' + fmt(item.precio) + '</span></td>' +
                celdas + '</tr>';
        }).join('');

        var totales = Array.from({ length: n }, function () { return 0; });
        var totalCompartido = 0;
        _splitItems.forEach(function (item) {
            if (item.persona >= 0) totales[item.persona] += item.precio;
            else totalCompartido += item.precio;
        });
        var porcadaUno = esMixto ? totalCompartido / n : 0;

        var totalRow = '<td></td>';
        for (var ti = 0; ti < n; ti++) {
            totalRow += '<td class="lmd-pos-split-asig-total">' + fmt(Math.round((totales[ti] + porcadaUno) * 100) / 100) + '</td>';
        }
        if (esMixto) totalRow += '<td class="lmd-pos-split-asig-total">' + fmt(totalCompartido) + '</td>';

        var sinAsignar = esMixto ? 0 : _splitItems.filter(function (i) { return i.persona < 0; }).length;
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos._renderSplitNPicker()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('git-branch') + ' Asignar ítems</span>' +
            '</div>' +
            '<div class="lmd-pos-split-asig-body">' +
                (sinAsignar > 0 ? '<div class="lmd-pos-split-asig-alerta">' + icon('alert-circle') + ' ' + sinAsignar + ' ítem(s) sin asignar</div>' : '') +
                '<div class="lmd-pos-split-asig-scroll">' +
                    '<table class="lmd-pos-split-asig-tabla">' +
                        '<thead><tr><th>Ítem</th>' + headers + '</tr></thead>' +
                        '<tbody>' + rows + '</tbody>' +
                        '<tfoot><tr>' + totalRow + '</tr></tfoot>' +
                    '</table>' +
                '</div>' +
                '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--primary"' + (sinAsignar > 0 ? ' disabled' : '') + ' onclick="pos.confirmarAsignacionSplit()">' + icon('check-circle') + ' Confirmar y cobrar</button>' +
            '</div>';
        cerrarOverlay('splitdetalle');
        abrirOverlay('splitdetalle', html, { closeOnBackdrop: false, wide: true });
    }

    function asignarItemSplit(idx, persona) {
        if (_splitItems[idx]) { _splitItems[idx].persona = persona; _renderSplitAsignacion(); }
    }

    function confirmarAsignacionSplit() {
        var n = _splitN;
        var esMixto = _splitTipo === 'mixto';
        var totales = Array.from({ length: n }, function () { return 0; });
        var totalCompartido = 0;
        _splitItems.forEach(function (item) {
            if (item.persona >= 0) totales[item.persona] += item.precio;
            else totalCompartido += item.precio;
        });
        var porcadaUno = esMixto ? totalCompartido / n : 0;
        state.split.activo = true;
        state.split.personas = totales.map(function (t, i) {
            return { id: i, nombre: 'Persona ' + (i + 1), monto: Math.round((t + porcadaUno) * 100) / 100, metodoPago: null, pagado: false };
        });
        state.split.personaActual = 0;
        cerrarOverlay('splitdetalle');
        cobrarSiguientePersona();
    }

    function cobrarSiguientePersona() {
        var personas = state.split.personas;
        var idx = personas.findIndex(function (p) { return !p.pagado; });
        if (idx < 0) {
            state.split.activo = false;
            var total = totalLineas(state.lineas);
            finalizarPago('split', total, 'SPLIT-' + state.split.personas.length);
            return;
        }
        state.split.personaActual = idx;
        var persona = personas[idx];
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<span class="lmd-pos-ov-title">' + icon('user') + ' ' + persona.nombre + '</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(persona.monto) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-pm-grid">' +
                METODOS_PAGO.map(function (m) {
                    return '<button class="lmd-pos-pm-btn ' + (m.cls || '') + '" onclick="pos.pagarPersonaSplit(' + idx + ',\'' + m.codigo + '\',' + persona.monto.toFixed(2) + ')">' +
                        '<span class="lmd-pos-pm-btn__icon">' + icon(m.icon) + '</span>' +
                        '<span class="lmd-pos-pm-btn__label">' + m.label + '</span>' +
                        '<span class="lmd-pos-pm-btn__sub">' + m.sub + '</span>' +
                    '</button>';
                }).join('') +
            '</div>' +
            '<div class="lmd-pos-split-progress">' +
                personas.map(function (p, i) {
                    return '<div class="lmd-pos-split-progress__item' + (p.pagado ? ' pagado' : '') + (i === idx ? ' activo' : '') + '">' +
                        '<span>' + p.nombre + '</span><span>' + (p.pagado ? icon('check') : fmt(p.monto)) + '</span>' +
                    '</div>';
                }).join('') +
            '</div>';

        cerrarOverlay('splitdetalle');
        abrirOverlay('splitdetalle', html, { closeOnBackdrop: false, wide: true });
    }

    function pagarPersonaSplit(idx, metodo, monto) {
        if (metodo === 'efectivo') {
            // Abrir keypad para esta persona
            abrirOverlayEfectivoSplit(idx, monto);
        } else {
            state.split.personas[idx].metodoPago = metodo;
            state.split.personas[idx].pagado = true;
            lmdToast(state.split.personas[idx].nombre + ' — pagado con ' + metodo, 'success');
            cobrarSiguientePersona();
        }
    }

    function abrirOverlayEfectivoSplit(idx, total) {
        keypadValue = '0';
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos.cobrarSiguientePersona()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('banknote') + ' Efectivo — ' + state.split.personas[idx].nombre + '</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-efectivo-body">' +
                '<div class="lmd-pos-bill-shortcuts">' +
                    '<button class="lmd-pos-bill-btn lmd-pos-bill-btn--exacto" onclick="pos.seleccionarBilleteSplit(' + total.toFixed(2) + ',' + total.toFixed(2) + ',' + idx + ')">Exacto</button>' +
                    [5, 10, 20, 50, 100].map(function (b) {
                        return '<button class="lmd-pos-bill-btn" onclick="pos.seleccionarBilleteSplit(' + b + ',' + total.toFixed(2) + ',' + idx + ')">' + icon('banknote') + ' $' + b + '</button>';
                    }).join('') +
                '</div>' +
                '<div class="lmd-pos-keypad">' +
                    '<div class="lmd-pos-keypad__display" id="lmd-pos-keypad-display">$0.00</div>' +
                    '<div class="lmd-pos-cambio" id="lmd-pos-cambio"></div>' +
                    '<div class="lmd-pos-keypad__grid">' +
                        [1,2,3,4,5,6,7,8,9,'.',0,'⌫'].map(function (k) {
                            return '<button class="lmd-pos-keypad__btn" onclick="pos.keypadInput(\'' + k + '\',' + total.toFixed(2) + ')">' + (k === '⌫' ? icon('delete') : k) + '</button>';
                        }).join('') +
                    '</div>' +
                    '<div class="lmd-pos-keypad__actions">' +
                        '<button class="lmd-pos-keypad__btn lmd-pos-keypad__btn--back" onclick="pos.cobrarSiguientePersona()">Volver</button>' +
                        '<button class="lmd-pos-keypad__btn lmd-pos-keypad__btn--confirm" id="lmd-keypad-confirmar" onclick="pos.confirmarEfectivoSplit(' + idx + ',' + total.toFixed(2) + ')" disabled>Confirmar</button>' +
                    '</div>' +
                '</div>' +
            '</div>';
        cerrarOverlay('splitdetalle');
        abrirOverlay('splitdetalle', html, { closeOnBackdrop: false });
    }

    function seleccionarBilleteSplit(valor, total, idx) {
        keypadValue = parseFloat(valor).toFixed(2).replace(/\.00$/, '');
        _actualizarKeypad(total);
    }

    function confirmarEfectivoSplit(idx, total) {
        var recibido = parseFloat(keypadValue || '0');
        if (recibido < total) { lmdToast('Monto insuficiente', 'error'); return; }
        var cambio = recibido - total;
        if (cambio > 0) lmdToast('Cambio: ' + fmt(cambio), 'info');
        state.split.personas[idx].metodoPago = 'efectivo';
        state.split.personas[idx].monto = recibido;
        state.split.personas[idx].pagado = true;
        cobrarSiguientePersona();
    }

    // ═══════════════════════════════════════════════════
    // FINALIZAR PAGO
    // ═══════════════════════════════════════════════════
    async function finalizarPago(metodo, monto, referencia) {
        state.pagoMetodo = metodo;
        state.pagoMonto = monto;
        state.pagoReferencia = referencia;

        if (state.pedidoActual && state.pedidoActual.id) {
            var csrf = document.querySelector('input[name="__RequestVerificationToken"]');
            var form = new FormData();
            form.append('__RequestVerificationToken', csrf ? csrf.value : '');
            form.append('pedidoId', state.pedidoActual.id);
            form.append('metodoPago', metodo || 'efectivo');
            if (monto != null) form.append('monto', monto.toString());
            if (referencia) form.append('referencia', referencia);
            try {
                var res = await fetch('?handler=PagarJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
                var data = await res.json().catch(function () { return null; });
                if (!res.ok) {
                    lmdToast(data && data.error ? data.error : 'Error al registrar pago', 'error');
                    abrirOverlayPago();
                    return;
                }
                if (data && data.mensaje) lmdToast(data.mensaje, 'success');
            } catch (e) { lmdToast('Error de conexión', 'error'); return; }
        }

        state.pagado = true;
        cerrarTodasOverlaysPago();
        renderProductos();
        mostrarPantalla('productos');
        lmdToast('Pago registrado. Presiona Finalizar para el comprobante.', 'success');
    }

    // ═══════════════════════════════════════════════════
    // DOCUMENTOS — overlay tras Finalizar
    // ═══════════════════════════════════════════════════
    var DOCUMENTOS = [
        { codigo: 'ticket',      label: 'Ticket',       icon: 'receipt',     sub: 'Imprimir / PDF' },
        { codigo: 'fcf',         label: 'Factura C.F.', icon: 'file-text',   sub: 'Consumidor Final' },
        { codigo: 'ccf',         label: 'CCF',          icon: 'file-check',  sub: 'Crédito Fiscal' },
        { codigo: 'nota-credito',label: 'Nota Crédito', icon: 'file-minus',  sub: 'Anulación/Ajuste' },
        { codigo: 'email',       label: 'Email',        icon: 'mail',        sub: 'Factura electrónica' },
        { codigo: 'sin-doc',     label: 'Sin doc.',     icon: 'x-circle',    sub: 'Finalizar sin comprobante' }
    ];

    function abrirOverlayDocumentos() {
        var total = (state.pedidoActual && state.pedidoActual.total)
            ? state.pedidoActual.total
            : totalLineas(state.lineas);
        var btnsHtml = DOCUMENTOS.map(function (d) {
            return '<button class="lmd-pos-pm-btn" onclick="pos.emitirDocumento(\'' + d.codigo + '\')">' +
                '<span class="lmd-pos-pm-btn__icon">' + icon(d.icon) + '</span>' +
                '<span class="lmd-pos-pm-btn__label">' + d.label + '</span>' +
                '<span class="lmd-pos-pm-btn__sub">' + d.sub + '</span>' +
            '</button>';
        }).join('');

        var html =
            '<div class="lmd-pos-ov-header">' +
                '<span class="lmd-pos-ov-title">' + icon('receipt') + ' Comprobante</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + ' · ' + (state.pagoMetodo || '').toUpperCase() + '</div>' +
            '</div>' +
            '<div class="lmd-pos-pm-grid">' + btnsHtml + '</div>';

        abrirOverlay('documentos', html, { wide: true, closeOnBackdrop: false });
    }

    function emitirDocumento(codigo) {
        cerrarOverlay('documentos');
        if (codigo === 'sin-doc') {
            nuevaOrden();
            return;
        }
        var labels = { ticket: 'Ticket generado', fcf: 'Factura C.F. emitida', ccf: 'CCF emitido', 'nota-credito': 'Nota de crédito registrada', email: 'Factura enviada por correo' };
        lmdToast(labels[codigo] || 'Documento generado', 'success');
        nuevaOrden();
    }

    // ═══════════════════════════════════════════════════
    // NUEVA ORDEN
    // ═══════════════════════════════════════════════════
    async function nuevaOrden() {
        cerrarOverlay('documentos');
        cerrarTodasOverlaysPago();
        state.tipoServicio = null;
        state.mesaId = null;
        state.mesaNumero = null;
        _resetPedido();
        await refrescarMesas();
        renderSeleccion();
        mostrarPantalla('seleccion');
    }

    // ═══════════════════════════════════════════════════
    // MODIFIER MODAL
    // ═══════════════════════════════════════════════════
    var _mod = { productoId: null, productoNombre: '', ingredientes: [], alergias: [], alergenosProducto: [], notaCustom: '' };

    async function abrirModificadores(productoId) {
        var prod = (window.__lmdProductosDisponibles || []).find(function (p) { return p.id === productoId; });
        if (!prod) return;
        _mod.productoId = productoId;
        _mod.productoNombre = prod.nombre;
        _mod.ingredientes = [];
        _mod.alergias = [];
        _mod.notaCustom = '';

        try {
            var r1 = await fetch('?handler=IngredientesProductoJson&productoId=' + productoId);
            var d1 = await r1.json();
            _mod.ingredientes = (d1.ingredientes || []).map(function (ing) {
                return { id: ing.id, nombre: ing.nombre, cantidad: ing.cantidad, estado: 'normal', reemplazoId: null, reemplazoNombre: '' };
            });
        } catch (e) {}

        try {
            var r2 = await fetch('?handler=AlergenosProductoJson&productoId=' + productoId);
            _mod.alergenosProducto = await r2.json() || [];
        } catch (e) { _mod.alergenosProducto = []; }

        renderModificadorModal();
    }

    function renderModificadorModal() {
        cerrarOverlay('modificador');

        var alergenosHtml = _mod.alergenosProducto.length > 0
            ? _mod.alergenosProducto.map(function (a) {
                var activo = _mod.alergias.indexOf(a.nombre.toLowerCase()) >= 0;
                return '<button class="lmd-mod-alergia-btn' + (activo ? ' activo' : '') + '" onclick="pos.toggleAlergia(\'' + a.nombre.toLowerCase() + '\')">' + a.nombre + '</button>';
              }).join('')
            : '<span class="lmd-mod-empty">Sin alérgenos registrados</span>';

        var ingsHtml = _mod.ingredientes.length > 0
            ? _mod.ingredientes.map(function (ing) {
                var est = ing.estado || 'normal';
                var otros = _mod.ingredientes.filter(function (o) { return o.id !== ing.id; });
                var reemplazoSel = est === 'quitado' && otros.length > 0
                    ? '<select class="lmd-mod-ing-reemplazo" onchange="pos.cambiarReemplazo(\'' + ing.id + '\', this.value)">' +
                          '<option value="">— Sin reemplazo</option>' +
                          otros.map(function (o) {
                              return '<option value="' + o.id + '"' + (ing.reemplazoId === o.id ? ' selected' : '') + '>' + o.nombre + '</option>';
                          }).join('') +
                      '</select>'
                    : '';
                return '<div class="lmd-mod-ing-row lmd-mod-ing-row--' + est + '">' +
                    '<span class="lmd-mod-ing-nombre">' + ing.nombre + ' <small>(' + ing.cantidad + ')</small></span>' +
                    '<div class="lmd-mod-ing-acciones">' +
                        '<button class="lmd-mod-ing-btn lmd-mod-ing-btn--extra' + (est === 'extra' ? ' activo' : '') + '" onclick="pos.toggleEstadoIngrediente(\'' + ing.id + '\', \'extra\')" title="Extra">' + icon('plus-circle') + '</button>' +
                        '<button class="lmd-mod-ing-btn lmd-mod-ing-btn--quitar' + (est === 'quitado' ? ' activo' : '') + '" onclick="pos.toggleEstadoIngrediente(\'' + ing.id + '\', \'quitado\')" title="Quitar">' + icon('minus-circle') + '</button>' +
                    '</div>' +
                    reemplazoSel +
                '</div>';
              }).join('')
            : '<span class="lmd-mod-empty">Sin ingredientes registrados</span>';

        var html =
            '<div class="lmd-pos-ov-header">' +
                '<span class="lmd-pos-ov-title">' + icon('edit-3') + ' ' + _mod.productoNombre + '</span>' +
                '<button class="lmd-pos-ov-close" onclick="pos.cerrarModificadores()">' + icon('x') + '</button>' +
            '</div>' +
            '<div class="lmd-mod-body">' +
                '<div class="lmd-mod-section">' +
                    '<div class="lmd-mod-section__title">' + icon('alert-triangle') + ' Alergias / restricciones</div>' +
                    '<div class="lmd-mod-alergias">' + alergenosHtml + '</div>' +
                '</div>' +
                '<div class="lmd-mod-section">' +
                    '<div class="lmd-mod-section__title">' + icon('list') + ' Ingredientes</div>' +
                    '<div class="lmd-mod-ings">' + ingsHtml + '</div>' +
                '</div>' +
                '<button class="lmd-mod-confirmar" onclick="pos.confirmarModificadores()">' + icon('check-circle') + ' Confirmar cambios</button>' +
            '</div>';

        abrirOverlay('modificador', html, { bottom: true });
    }

    function toggleEstadoIngrediente(id, estado) {
        var ing = _mod.ingredientes.find(function (i) { return i.id === id; });
        if (!ing) return;
        ing.estado = ing.estado === estado ? 'normal' : estado;
        if (ing.estado !== 'quitado') { ing.reemplazoId = null; ing.reemplazoNombre = ''; }
        renderModificadorModal();
    }

    function cambiarReemplazo(ingId, reemplazoId) {
        var ing = _mod.ingredientes.find(function (i) { return i.id === ingId; });
        if (!ing) return;
        if (!reemplazoId) { ing.reemplazoId = null; ing.reemplazoNombre = ''; }
        else {
            var r = _mod.ingredientes.find(function (i) { return i.id === reemplazoId; });
            ing.reemplazoId = reemplazoId;
            ing.reemplazoNombre = r ? r.nombre : '';
        }
    }

    function toggleAlergia(alergia) {
        var idx = _mod.alergias.indexOf(alergia);
        if (idx >= 0) _mod.alergias.splice(idx, 1);
        else _mod.alergias.push(alergia);
        renderModificadorModal();
    }

    function cerrarModificadores() { cerrarOverlay('modificador'); }

    function confirmarModificadores() {
        cerrarModificadores();

        var mods = [];
        _mod.ingredientes.forEach(function (ing) {
            var est = ing.estado || 'normal';
            if (est === 'quitado') {
                if (ing.reemplazoId) {
                    mods.push({ ingredienteId: ing.id, ingredienteNombre: ing.nombre, accion: 'intercambiar', motivo: 'preferencia', ingredienteReemplazoId: ing.reemplazoId, ingredienteReemplazoNombre: ing.reemplazoNombre });
                } else {
                    mods.push({ ingredienteId: ing.id, ingredienteNombre: ing.nombre, accion: 'quitar', motivo: 'preferencia', ingredienteReemplazoId: null, ingredienteReemplazoNombre: null });
                }
            } else if (est === 'extra') {
                mods.push({ ingredienteId: ing.id, ingredienteNombre: ing.nombre, accion: 'extra', motivo: 'preferencia', ingredienteReemplazoId: null, ingredienteReemplazoNombre: null });
            }
        });

        // Alergias como ModificacionIngrediente de primera clase (motivo:'alergia')
        _mod.alergias.forEach(function (alergia) {
            mods.push({ ingredienteId: '00000000-0000-0000-0000-000000000000', ingredienteNombre: alergia, accion: 'alergia', motivo: 'alergia', ingredienteReemplazoId: null, ingredienteReemplazoNombre: null });
        });

        var notasArr = [];
        if (_mod.notaCustom && _mod.notaCustom.trim()) notasArr.push(_mod.notaCustom.trim());
        var notas = notasArr.length > 0 ? notasArr.join(' | ') : null;

        var linea = state.lineas.find(function (l) { return l.productoId === _mod.productoId; });
        if (linea) {
            linea.modificacionesJson = mods.length > 0 ? JSON.stringify(mods) : null;
            linea.notas = notas;
            linea.tieneModificaciones = mods.length > 0 || !!notas;
            renderProductos();
        }

        var total = mods.length + (notas ? 1 : 0);
        lmdToast(total > 0 ? 'Modificaciones aplicadas (' + total + ')' : 'Sin cambios', total > 0 ? 'success' : 'info');
    }

    // ═══════════════════════════════════════════════════
    // CAMBIAR TIPO DE SERVICIO
    // ═══════════════════════════════════════════════════
    function cambiarServicio() {
        if (state.pagado) return;
        if (state.pedidoActual) {
            lmdToast('El pedido ya fue registrado. Cancela la orden para cambiar el servicio.', 'error');
            return;
        }
        abrirOverlayCambiarServicio();
    }

    function abrirOverlayCambiarServicio() {
        var mesas = window.__lmdMesasDisponibles || [];
        var mesasHtml = mesas.filter(function (m) { return m.estado === 'Disponible'; }).map(function (m) {
            var seleccionada = state.mesaId === m.id;
            return '<button class="lmd-pos-mesa-card lmd-pos-mesa-card--disponible' + (seleccionada ? ' lmd-pos-mesa-card--seleccionada' : '') + '" onclick="pos.cambiarAMesa(\'' + m.id + '\',' + m.numero + ')">' +
                '<span class="lmd-pos-mesa-card__numero">' + m.numero + '</span>' +
                '<span class="lmd-pos-mesa-card__capacidad">' + m.capacidad + ' pax</span>' +
            '</button>';
        }).join('');

        var esComerAqui = state.tipoServicio === 'ComerAqui';
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<span class="lmd-pos-ov-title">' + icon('refresh-cw') + ' Cambiar servicio</span>' +
                '<button class="lmd-pos-ov-close" onclick="pos.cerrarCambiarServicio()">' + icon('x') + '</button>' +
            '</div>' +
            '<div class="lmd-pos-cambiar-servicio-body">' +
                '<button class="lmd-pos-cambiar-servicio-opcion' + (!esComerAqui ? ' lmd-pos-cambiar-servicio-opcion--activa' : '') + '" onclick="pos.cambiarAParaLlevar()">' +
                    icon('package') + '<span>Para llevar</span>' +
                '</button>' +
                '<div class="lmd-pos-cambiar-servicio-sep">— o selecciona una mesa —</div>' +
                '<div class="lmd-pos-mesas-grid lmd-pos-mesas-grid--mini">' +
                    (mesasHtml || '<span class="lmd-pos-empty">Sin mesas disponibles</span>') +
                '</div>' +
            '</div>';

        abrirOverlay('cambiarservicio', html, { closeOnBackdrop: true });
    }

    function cambiarAMesa(mesaId, numero) {
        var mesas = window.__lmdMesasDisponibles || [];
        var m = mesas.find(function (x) { return x.id === mesaId; });
        if (m && m.estado !== 'Disponible') { lmdToast('Mesa ocupada — selecciona otra', 'error'); return; }
        state.tipoServicio = 'ComerAqui';
        state.mesaId = mesaId;
        state.mesaNumero = numero;
        cerrarOverlay('cambiarservicio');
        renderProductos();
        lmdToast('Cambiado a Mesa ' + numero, 'success');
    }

    function cambiarAParaLlevar() {
        state.tipoServicio = 'ParaLlevar';
        state.mesaId = null;
        state.mesaNumero = null;
        cerrarOverlay('cambiarservicio');
        renderProductos();
        lmdToast('Cambiado a Para llevar', 'success');
    }

    function cerrarCambiarServicio() { cerrarOverlay('cambiarservicio'); }

    // ═══════════════════════════════════════════════════
    // PAGO RECHAZADO — recuperación de error
    // ═══════════════════════════════════════════════════
    function simularRechazo(metodo) {
        cerrarOverlay(metodo === 'qr' ? 'qr' : 'tarjeta');
        var mensajes = {
            tarjeta: 'Tarjeta rechazada por el emisor',
            qr: 'Transferencia no recibida — tiempo agotado'
        };
        abrirOverlayErrorPago(metodo, mensajes[metodo] || 'Pago rechazado');
    }

    function abrirOverlayErrorPago(metodo, mensaje) {
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<span class="lmd-pos-ov-title lmd-pos-ov-title--error">' + icon('alert-circle') + ' Pago rechazado</span>' +
            '</div>' +
            '<div class="lmd-pos-error-pago-body">' +
                '<div class="lmd-pos-error-pago-icon">' + icon('x-circle') + '</div>' +
                '<p class="lmd-pos-error-pago-mensaje">' + mensaje + '</p>' +
                '<div class="lmd-pos-error-pago-actions">' +
                    '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--primary" onclick="pos.reintentarPago(\'' + metodo + '\')">' + icon('refresh-cw') + ' Reintentar</button>' +
                    '<button class="lmd-pos-ov-btn" onclick="pos.volverAMetodos()">' + icon('credit-card') + ' Otro método</button>' +
                '</div>' +
            '</div>';

        abrirOverlay('errorpago', html, { closeOnBackdrop: false });
    }

    function reintentarPago(metodo) {
        cerrarOverlay('errorpago');
        var total = totalLineas(state.lineas);
        if (metodo === 'tarjeta') abrirOverlayTarjeta(total);
        else if (metodo === 'qr') abrirOverlayQR(total);
        else abrirOverlayPago();
    }

    // ═══════════════════════════════════════════════════
    // SignalR + Init
    // ═══════════════════════════════════════════════════
    async function initSignalR() {
        if (typeof signalR === 'undefined') return;
        try {
            connection = new signalR.HubConnectionBuilder()
                .withUrl('/hubs/pedidos')
                .withAutomaticReconnect()
                .build();
            connection.on('EstadoCambiado', function (pedidoId, nuevoEstado) {
                if (nuevoEstado === 'Pagado' || nuevoEstado === 'Despachado') refrescarMesas();
            });
            connection.on('PedidoCreado', function () { refrescarMesas(); });
            await connection.start();
        } catch (e) {}
    }

    async function refrescarMesas() {
        try {
            var res = await fetch('?handler=MesasJson', { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (res.ok) {
                var data = await res.json();
                if (data && data.mesas) window.__lmdMesasDisponibles = data.mesas;
            }
        } catch (e) {}
    }

    // ── Public API ──────────────────────────────────────
    window.pos = {
        seleccionarMesa, seleccionarParaLlevar,
        filtrarCategoria, agregarAlCarrito, incrementarItem, decrementarItem, eliminarDelCarrito,
        cancelarOrden, confirmarListo, irAPago,
        cerrarPago, procesarPago,
        seleccionarBillete, keypadInput, keypadConfirmar,
        volverAMetodos, simularTarjeta, simularQR, confirmarOtro,
        abrirSplit, volverAPago, splitIgualitario, splitPorPersona, splitMixto,
        toggleEstadoIngrediente, cambiarReemplazo,
        iniciarAsignacionSplit, asignarItemSplit, confirmarAsignacionSplit, _renderSplitNPicker, _renderSplitAsignacion,
        confirmarAnulacion,
        ajustarSplitN, iniciarSplitIgualitario, cobrarSiguientePersona,
        pagarPersonaSplit, seleccionarBilleteSplit, confirmarEfectivoSplit,
        emitirDocumento, nuevaOrden,
        abrirModificadores, toggleAlergia,
        cerrarModificadores, confirmarModificadores,
        cambiarServicio, cambiarAMesa, cambiarAParaLlevar, cerrarCambiarServicio,
        simularRechazo, reintentarPago
    };

    document.addEventListener('DOMContentLoaded', function () {
        renderSeleccion();
        initSignalR();

        window.addEventListener('offline', function () {
            var banner = document.getElementById('lmd-offline-banner');
            if (banner) { banner.style.display = ''; banner.classList.add('visible'); }
        });
        window.addEventListener('online', function () {
            var banner = document.getElementById('lmd-offline-banner');
            if (banner) {
                banner.classList.remove('visible');
                setTimeout(function () { if (!banner.classList.contains('visible')) banner.style.display = 'none'; }, 500);
            }
        });
    });
})();
