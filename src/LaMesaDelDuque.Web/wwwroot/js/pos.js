/* ═══════════════════════════════════════════════════════
   POS — La Mesa del Duque  v3.0
   Sprint 3 Slice 8 — UX Overhaul
   4-screen SPA state machine. Zero emojis. Lucide SVG icons.
   ═══════════════════════════════════════════════════════ */

(function () {
    // ── Lucide SVG helper ──────────────────────────────
    function icon(name, cls) {
        return '<svg class="' + (cls || '') + '" width="24" height="24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><use href="https://cdn.jsdelivr.net/npm/lucide-static@latest/icons/' + name + '.svg#icon"/></svg>';
    }

    // ── State ──────────────────────────────────────────
    const state = {
        pantalla: 'seleccion', // seleccion | productos | pago | documentos
        tipoServicio: null,    // 'ComerAqui' | 'ParaLlevar'
        mesaId: null,
        mesaNumero: null,
        lineas: [],
        pedidoActual: null,
        pagoMetodo: null,
        pagoMonto: null,
        pagoReferencia: null,
        puedeVolver: true
    };

    const SHELL = document.getElementById('lmd-pos-contenido');
    let connection = null;

    // ── Screen switching ───────────────────────────────
    function mostrarPantalla(nombre) {
        state.pantalla = nombre;
        document.querySelectorAll('.lmd-pos-screen').forEach(s => s.classList.remove('lmd-pos-screen--activa'));
        var el = document.getElementById('lmd-pos-screen-' + nombre);
        if (el) el.classList.add('lmd-pos-screen--activa');
    }

    // ═══════════════════════════════════════════════════
    // SCREEN 1 — Selección (Comer aquí | Para llevar)
    // ═══════════════════════════════════════════════════
    function renderSeleccion() {
        var mesas = window.__lmdMesasDisponibles || [];
        // Group mesas by zone, sort by capacity within each zone
        var zonas = {};
        mesas.forEach(function (m) {
            var z = m.zona || 'General';
            if (!zonas[z]) zonas[z] = [];
            zonas[z].push(m);
        });
        var zonaKeys = Object.keys(zonas).sort();
        var mesasHtml = '';
        zonaKeys.forEach(function (z) {
            mesasHtml += '<div class="lmd-pos-mesa-zona-separator">' + z + '</div>';
            zonas[z].sort(function (a, b) { return a.capacidad - b.capacidad; }).forEach(function (m) {
                var estadoClass = m.estado === 'Disponible' ? 'lmd-pos-mesa-card--disponible' : 'lmd-pos-mesa-card--ocupada';
                mesasHtml += '<div class="lmd-pos-mesa-card ' + estadoClass + '" data-mesa-id="' + m.id + '" data-mesa-numero="' + m.numero + '" data-capacidad="' + m.capacidad + '" onclick="pos.seleccionarMesa(\'' + m.id + '\',' + m.numero + ')">' +
                    '<span class="lmd-pos-mesa-card__numero">' + m.numero + '</span>' +
                    '<span class="lmd-pos-mesa-card__capacidad">' + m.capacidad + ' pax</span>' +
                    '<span class="lmd-pos-mesa-card__zona">' + z + '</span>' +
                '</div>';
            });
        });

        var html = '<div class="lmd-pos-seleccion">' +
            '<div class="lmd-pos-seleccion__mitad lmd-pos-seleccion__comer-aqui">' +
                '<div class="lmd-pos-seleccion__header">' + icon('utensils-crossed', '') + ' Comer aquí</div>' +
                '<div class="lmd-pos-mesas-grid">' + (mesasHtml || '<div class="text-muted p-3">No hay mesas disponibles</div>') + '</div>' +
            '</div>' +
            '<div class="lmd-pos-seleccion__mitad lmd-pos-seleccion__para-llevar" onclick="pos.seleccionarParaLlevar()">' +
                '<div class="lmd-pos-seleccion__header">' + icon('package', '') + ' Para llevar</div>' +
                '<div class="lmd-pos-para-llevar-card">' +
                    icon('shopping-bag', '') +
                    '<div class="lmd-pos-para-llevar-card__titulo">Para llevar</div>' +
                    '<div class="lmd-pos-para-llevar-card__sub">Tocar para iniciar orden sin mesa</div>' +
                '</div>' +
            '</div>' +
        '</div>';

        SHELL.innerHTML = '<div class="lmd-pos-shell">' +
            '<div class="lmd-pos-screen lmd-pos-screen--activa" id="lmd-pos-screen-seleccion">' + html + '</div>' +
            '<div class="lmd-pos-screen" id="lmd-pos-screen-productos"></div>' +
            '<div class="lmd-pos-screen" id="lmd-pos-screen-pago"></div>' +
            '<div class="lmd-pos-screen" id="lmd-pos-screen-documentos"></div>' +
        '</div>';
    }

    function seleccionarMesa(mesaId, numero) {
        var mesas = window.__lmdMesasDisponibles || [];
        var m = mesas.find(function (x) { return x.id === mesaId; });
        if (m && m.estado !== 'Disponible') { lmdToast('Mesa no disponible', 'error'); return; }
        state.tipoServicio = 'ComerAqui';
        state.mesaId = mesaId;
        state.mesaNumero = numero;
        state.lineas = [];
        state.pedidoActual = null;
        state.puedeVolver = true;
        renderProductos();
        mostrarPantalla('productos');
    }

    function seleccionarParaLlevar() {
        state.tipoServicio = 'ParaLlevar';
        state.mesaId = null;
        state.mesaNumero = null;
        state.lineas = [];
        state.pedidoActual = null;
        state.puedeVolver = true;
        renderProductos();
        mostrarPantalla('productos');
    }

    // ═══════════════════════════════════════════════════
    // SCREEN 2 — Productos (25/50/25)
    // ═══════════════════════════════════════════════════
    function renderProductos() {
        var prods = window.__lmdProductosDisponibles || [];
        // Build category list
        var cats = [];
        var seen = {};
        prods.forEach(function (p) {
            var cn = p.categoriaNombre || 'Sin categoría';
            if (!seen[cn]) { seen[cn] = true; cats.push(cn); }
        });
        cats.sort();
        cats.unshift('Todos');

        var catHtml = cats.map(function (c, i) {
            return '<button class="lmd-pos-cat-btn' + (i === 0 ? ' lmd-pos-cat-btn--activa' : '') + '" data-cat="' + c + '" onclick="pos.filtrarCategoria(\'' + c.replace(/'/g, "\\'") + '\')">' +
                icon(c === 'Todos' ? 'layers' : 'tag', '') + c +
            '</button>';
        }).join('');

        var productosHtml = renderProductGrid(prods);

        var total = state.lineas.reduce(function (s, l) { return s + (l.precioUnitario || 0) * (l.cantidad || 0); }, 0);
        var cartItemsHtml = state.lineas.length === 0
            ? '<div class="text-muted text-center py-4">' + icon('shopping-cart', '') + '<br>Carrito vacío</div>'
            : state.lineas.map(function (l, i) {
                return '<div class="lmd-pos-cart-item">' +
                    '<span class="lmd-pos-cart-item__qty">' + l.cantidad + '</span>' +
                    '<span class="lmd-pos-cart-item__nombre">' + (l.productoNombre || l.nombre || '') + '</span>' +
                    '<span class="lmd-pos-cart-item__precio">' + formatMoney((l.precioUnitario || 0) * (l.cantidad || 0)) + '</span>' +
                    '<button class="lmd-pos-cart-item__remove" onclick="pos.eliminarDelCarrito(' + i + ')">' + icon('x', '') + '</button>' +
                '</div>';
              }).join('');

        var html = '<div class="lmd-pos-productos">' +
            '<div class="lmd-pos-categorias" id="lmd-pos-categorias">' + catHtml + '</div>' +
            '<div class="lmd-pos-productos-grid" id="lmd-pos-productos-grid">' + productosHtml + '</div>' +
            '<div class="lmd-pos-cart">' +
                '<div class="lmd-pos-cart__header">' + icon('shopping-bag', '') + ' Pedido</div>' +
                '<div class="lmd-pos-cart__items" id="lmd-pos-cart-items">' + cartItemsHtml + '</div>' +
                '<div class="lmd-pos-cart__total">' + formatMoney(total) + '</div>' +
                '<div class="lmd-pos-cart__acciones">' +
                    '<button class="lmd-pos-cart-btn lmd-pos-cart-btn--listo" onclick="pos.confirmarListo()">' + icon('check-circle', '') + 'Listo</button>' +
                    '<button class="lmd-pos-cart-btn lmd-pos-cart-btn--cancelar" onclick="pos.cancelarOrden()">' + icon('x-circle', '') + 'Cancelar</button>' +
                    '<button class="lmd-pos-cart-btn lmd-pos-cart-btn--pagar" onclick="pos.irAPago()"' + (state.lineas.length === 0 ? ' disabled' : '') + '>' + icon('credit-card', '') + 'Pagar</button>' +
                '</div>' +
            '</div>' +
        '</div>';

        document.getElementById('lmd-pos-screen-productos').innerHTML = html;
    }

    function renderProductGrid(prods, catFiltro) {
        var filtered = catFiltro && catFiltro !== 'Todos' ? prods.filter(function (p) { return (p.categoriaNombre || 'Sin categoría') === catFiltro; }) : prods;
        if (filtered.length === 0) return '<div class="text-muted text-center py-4">Sin productos</div>';
        return filtered.map(function (p) {
            var agotado = p.agotado === true;
            return '<div class="lmd-pos-producto-card' + (agotado ? ' lmd-pos-producto-card--agotado' : '') + '" onclick="' + (agotado ? '' : 'pos.agregarAlCarrito(\'' + p.id + '\',\'' + (p.nombre || '').replace(/'/g, "\\'") + '\',' + (p.precio || 0) + ')') + '">' +
                icon(p.categoriaNombre === 'Bebidas' ? 'wine' : (p.categoriaNombre === 'Postres' ? 'dessert' : 'utensils'), '') +
                '<span class="lmd-pos-producto-card__nombre">' + (p.nombre || '') + '</span>' +
                '<span class="lmd-pos-producto-card__precio">' + formatMoney(p.precio || 0) + '</span>' +
                (p.tiempoPreparacionMin ? '<span class="lmd-pos-producto-card__tiempo">' + p.tiempoPreparacionMin + ' min</span>' : '') +
                '<button class="lmd-pos-producto-card__editar" onclick="event.stopPropagation();pos.abrirModificadores(\'' + p.id + '\')" title="Modificar ingredientes"><svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><use href="https://cdn.jsdelivr.net/npm/lucide-static@latest/icons/edit-3.svg#icon"/></svg></button>' +
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
        var existente = state.lineas.find(function (l) { return l.productoId === prodId; });
        if (existente) { existente.cantidad += 1; }
        else { state.lineas.push({ productoId: prodId, productoNombre: nombre, cantidad: 1, precioUnitario: precio }); }
        renderProductos();
    }

    function eliminarDelCarrito(idx) {
        state.lineas.splice(idx, 1);
        renderProductos();
    }

    function confirmarListo() {
        if (state.lineas.length === 0) { lmdToast('Agregue productos primero.', 'error'); return; }
        state.puedeVolver = false;
        renderDocumentos();
        mostrarPantalla('documentos');
    }

    async function cancelarOrden() {
        if (state.lineas.length === 0) return;
        var ok = await window.lmdConfirm('Cancelar esta orden?');
        if (!ok) return;
        state.lineas = [];
        state.pedidoActual = null;
        renderProductos();
    }

    function irAPago() {
        if (state.lineas.length === 0) { lmdToast('Agregue productos primero.', 'error'); return; }
        state.puedeVolver = true;
        renderPago();
        mostrarPantalla('pago');
    }

    // ═══════════════════════════════════════════════════
    // SCREEN 3 — Pago (2x3 fullscreen grid)
    // ═══════════════════════════════════════════════════
    var METODOS_PAGO = [
        { codigo: 'efectivo', label: 'Efectivo', icon: 'banknote', cls: 'lmd-pos-pago-btn--efectivo', sub: 'Cambio automático' },
        { codigo: 'tarjeta', label: 'Tarjeta', icon: 'credit-card', cls: 'lmd-pos-pago-btn--tarjeta', sub: 'Débito / Crédito' },
        { codigo: 'qr', label: 'QR / Transf.', icon: 'qr-code', cls: 'lmd-pos-pago-btn--qr', sub: 'Wompi, BAC, Niu' },
        { codigo: 'credito', label: 'Crédito Emp.', icon: 'building-2', cls: '', sub: 'Cuenta corriente' },
        { codigo: 'vale', label: 'Vale', icon: 'ticket', cls: '', sub: 'Voucher alimentación' },
        { codigo: 'cortesia', label: 'Cortesía', icon: 'gift', cls: '', sub: 'Invitación ($0)' }
    ];

    function renderPago() {
        var total = state.lineas.reduce(function (s, l) { return s + (l.precioUnitario || 0) * (l.cantidad || 0); }, 0);
        var btnsHtml = METODOS_PAGO.map(function (m) {
            return '<button class="lmd-pos-pago-btn ' + (m.cls || '') + '" onclick="pos.procesarPago(\'' + m.codigo + '\',' + total.toFixed(2) + ')">' +
                icon(m.icon, '') + '<span class="lmd-pos-pago-btn__label">' + m.label + '</span>' +
                '<span class="lmd-pos-pago-btn__sub">' + (m.sub || '') + '</span></button>';
        }).join('');

        var html = '<div class="lmd-pos-pago">' +
            btnsHtml +
        '</div>' +
        '<div class="text-center mt-2"><button class="btn btn-sm btn-outline-secondary" onclick="pos.volverAProductos()">' + icon('arrow-left', '') + ' Volver</button></div>';

        document.getElementById('lmd-pos-screen-pago').innerHTML = html;
    }

    function procesarPago(metodo, total) {
        state.pagoMetodo = metodo;
        state.pagoMonto = total;

        if (metodo === 'efectivo') {
            mostrarKeypad(total);
        } else if (metodo === 'tarjeta') {
            lmdToast('Conecte terminal bancario para $' + total.toFixed(2), 'info');
            finalizarPago();
        } else if (metodo === 'cortesia') {
            finalizarPago();
        } else {
            // QR / Crédito / Vale — simulado
            state.pagoReferencia = metodo.toUpperCase() + '-' + Date.now().toString(36);
            lmdToast('Pago ' + metodo + ' iniciado — ref: ' + state.pagoReferencia, 'success');
            finalizarPago();
        }
    }

    function mostrarKeypad(total) {
        var digitos = '';
        var html = '<div class="lmd-pos-keypad" id="lmd-pos-keypad">' +
            '<div class="lmd-pos-keypad__panel">' +
                '<div class="text-center mb-2"><strong>Total: ' + formatMoney(total) + '</strong></div>' +
                '<div class="lmd-pos-keypad__display" id="lmd-pos-keypad-display">$0.00</div>' +
                '<div class="lmd-pos-keypad__grid">' +
                    [1,2,3,4,5,6,7,8,9,'.',0,'⌫'].map(function(k) {
                        return '<button class="lmd-pos-keypad__btn" onclick="pos.keypadInput(\'' + k + '\')">' + k + '</button>';
                    }).join('') +
                '</div>' +
                '<div class="d-flex gap-2 mt-2">' +
                    '<button class="lmd-pos-keypad__btn lmd-pos-keypad__btn--back flex-fill" onclick="document.getElementById(\'lmd-pos-keypad\').remove()">Volver</button>' +
                    '<button class="lmd-pos-keypad__btn lmd-pos-keypad__btn--confirm flex-fill" onclick="pos.keypadConfirmar(' + total.toFixed(2) + ')">Confirmar</button>' +
                '</div>' +
            '</div></div>';
        document.body.insertAdjacentHTML('beforeend', html);
    }

    var keypadValue = '0';
    function keypadInput(k) {
        if (k === '⌫') { keypadValue = keypadValue.length > 1 ? keypadValue.slice(0, -1) : '0'; }
        else if (k === '.') { if (keypadValue.indexOf('.') === -1) keypadValue += '.'; }
        else { keypadValue = keypadValue === '0' ? '' + k : keypadValue + k; }
        document.getElementById('lmd-pos-keypad-display').textContent = '$' + parseFloat(keypadValue || '0').toFixed(2);
    }

    function keypadConfirmar(total) {
        var recibido = parseFloat(keypadValue || '0');
        if (recibido < total) { lmdToast('Monto insuficiente', 'error'); return; }
        document.getElementById('lmd-pos-keypad').remove();
        lmdToast('Cambio: ' + formatMoney(recibido - total), 'success');
        finalizarPago();
    }

    function finalizarPago() {
        state.puedeVolver = false;
        lmdToast('Pago procesado', 'success');
        renderDocumentos();
        mostrarPantalla('documentos');
    }

    function volverAProductos() {
        mostrarPantalla('productos');
    }

    // ═══════════════════════════════════════════════════
    // SCREEN 4 — Documentos fiscales (2x3 fullscreen)
    // ═══════════════════════════════════════════════════
    var DOCUMENTOS = [
        { codigo: 'ticket', label: 'Ticket', icon: 'receipt', sub: 'Imprimir / Descargar PDF' },
        { codigo: 'fcf', label: 'Factura C.F.', icon: 'file-text', sub: 'Consumidor Final' },
        { codigo: 'ccf', label: 'CCF', icon: 'file-check', sub: 'Crédito Fiscal' },
        { codigo: 'nota-credito', label: 'Nota Crédito', icon: 'file-minus', sub: 'Anulación / Ajuste' },
        { codigo: 'email', label: 'Enviar correo', icon: 'mail', sub: 'Factura electrónica' },
        { codigo: 'mas', label: 'Más opciones', icon: 'ellipsis', sub: '' }
    ];

    function renderDocumentos() {
        var total = state.lineas.reduce(function (s, l) { return s + (l.precioUnitario || 0) * (l.cantidad || 0); }, 0);
        var btnsHtml = DOCUMENTOS.map(function (d) {
            return '<button class="lmd-pos-doc-btn" onclick="pos.emitirDocumento(\'' + d.codigo + '\')">' +
                icon(d.icon, '') + '<span class="lmd-pos-doc-btn__label">' + d.label + '</span>' +
                (d.sub ? '<small>' + d.sub + '</small>' : '') + '</button>';
        }).join('');

        var html = '<div class="text-center mb-2"><strong>Total pagado: ' + formatMoney(total) + '</strong> · ' + (state.pagoMetodo || '') + '</div>' +
            '<div class="lmd-pos-documentos">' + btnsHtml + '</div>' +
            '<div class="text-center mt-2"><button class="btn btn-sm btn-outline-secondary" onclick="pos.nuevaOrden()">' + icon('plus-circle', '') + ' Nueva orden</button></div>';

        document.getElementById('lmd-pos-screen-documentos').innerHTML = html;
    }

    function emitirDocumento(codigo) {
        if (codigo === 'ticket') {
            lmdToast('Ticket generado. Descargando PDF...', 'success');
        } else if (codigo === 'fcf') {
            lmdToast('Factura Consumidor Final emitida', 'success');
        } else if (codigo === 'ccf') {
            lmdToast('Comprobante de Crédito Fiscal emitido', 'success');
        } else if (codigo === 'nota-credito') {
            lmdToast('Nota de crédito registrada', 'success');
        } else if (codigo === 'email') {
            lmdToast('Factura enviada por correo', 'success');
        } else {
            lmdToast('Opción no disponible en simulación', 'info');
        }
    }

    function nuevaOrden() {
        state.tipoServicio = null;
        state.mesaId = null;
        state.mesaNumero = null;
        state.lineas = [];
        state.pedidoActual = null;
        state.pagoMetodo = null;
        state.pagoMonto = null;
        state.pagoReferencia = null;
        state.puedeVolver = true;
        renderSeleccion();
        mostrarPantalla('seleccion');
    }

    // ── Modifier system (ingredients + allergens) ─────────
    var modificadores = { productoId: null, productoNombre: '', ingredientes: [], alergias: [], alergenosProducto: [], extras: [], notaCustom: '', curso: 'PlatoFuerte' };

    async function abrirModificadores(productoId) {
        var prod = (window.__lmdProductosDisponibles || []).find(function(p) { return p.id === productoId; });
        if (!prod) return;
        modificadores.productoId = productoId;
        modificadores.productoNombre = prod.nombre;
        modificadores.ingredientes = [];
        modificadores.alergias = [];
        modificadores.extras = [];
        modificadores.notaCustom = '';

        try {
            var res = await fetch('?handler=IngredientesProductoJson&productoId=' + productoId);
            var data = await res.json();
            modificadores.ingredientes = (data.ingredientes || []).map(function(ing) { return { id: ing.id, nombre: ing.nombre, cantidad: ing.cantidad, quitado: false, motivo: '' }; });
        } catch(e) {}

        try {
            var ar = await fetch('?handler=AlergenosProductoJson&productoId=' + productoId);
            var ad = await ar.json();
            modificadores.alergenosProducto = ad || [];
        } catch(e) { modificadores.alergenosProducto = []; }

        renderModificadorModal();
    }

    function renderModificadorModal() {
        var old = document.getElementById('lmd-modificador-overlay');
        if (old) old.remove();

        var over = document.createElement('div');
        over.id = 'lmd-modificador-overlay';
        over.style.cssText = 'position:fixed;inset:0;background:rgba(0,0,0,0.6);z-index:4000;display:flex;align-items:flex-end;justify-content:center';
        over.onclick = function(e) { if (e.target === over) cerrarModificadores(); };

        var alergenosHtml = (modificadores.alergenosProducto && modificadores.alergenosProducto.length > 0)
            ? modificadores.alergenosProducto.map(function(a) {
                return '<button class="lmd-mod-alergia-btn' + (modificadores.alergias.indexOf(a.nombre.toLowerCase()) >= 0 ? ' activo' : '') + '" onclick="pos.toggleAlergia(\'' + a.nombre.toLowerCase() + '\')">' + a.nombre + '</button>';
            }).join('')
            : '<span style="font-size:0.75rem;opacity:0.5">Sin alérgenos registrados</span>';

        var ingsHtml = modificadores.ingredientes.map(function(ing) {
            return '<div style="display:flex;justify-content:space-between;align-items:center;padding:0.3rem 0;border-bottom:1px solid rgba(255,255,255,0.05)"><span>' + ing.nombre + ' (' + ing.cantidad + ')</span><button style="padding:0.3rem 0.6rem;border-radius:4px;border:none;cursor:pointer;font-size:0.75rem;' + (ing.quitado ? 'background:#c0392b;color:#fff' : 'background:#333;color:#aaa') + '" onclick="pos.toggleIngrediente(\'' + ing.id + '\')">' + (ing.quitado ? '<svg width="12" height="12" fill="none" stroke="currentColor" stroke-width="2.5"><use href=\'https://cdn.jsdelivr.net/npm/lucide-static@latest/icons/check.svg#icon\'/></svg> Quitado' : 'Quitar') + '</button></div>';
        }).join('');

        over.innerHTML = '<div style="background:#1a1a2e;width:100%;max-width:500px;max-height:80vh;overflow-y:auto;border-radius:12px 12px 0 0;padding:1rem;color:#fff;font-family:Montserrat,sans-serif">' +
            '<div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:0.5rem"><h3 style="margin:0;font-size:1.1rem">' + modificadores.productoNombre + '</h3><button onclick="pos.cerrarModificadores()" style="background:none;border:none;color:#aaa;font-size:1.3rem;cursor:pointer"><svg width="20" height="20" fill="none" stroke="currentColor" stroke-width="2"><use href=\'https://cdn.jsdelivr.net/npm/lucide-static@latest/icons/x.svg#icon\'/></svg></button></div>' +
            '<div style="margin-bottom:0.75rem"><strong style="font-size:0.8rem;opacity:0.6">ALÉRGENOS</strong><div style="display:flex;flex-wrap:wrap;gap:0.3rem;margin-top:0.3rem">' + alergenosHtml + '</div></div>' +
            '<div style="margin-bottom:0.75rem"><strong style="font-size:0.8rem;opacity:0.6">INGREDIENTES</strong><div style="margin-top:0.3rem">' + (ingsHtml || '<span style="font-size:0.75rem;opacity:0.5">Sin ingredientes</span>') + '</div></div>' +
            '<button onclick="pos.confirmarModificadores()" style="width:100%;padding:0.8rem;background:#27ae60;color:#fff;border:none;border-radius:8px;font-size:1rem;font-weight:700;cursor:pointer;margin-top:0.5rem"><svg width="16" height="16" fill="none" stroke="currentColor" stroke-width="2.5"><use href=\'https://cdn.jsdelivr.net/npm/lucide-static@latest/icons/check.svg#icon\'/></svg> Confirmar</button>' +
        '</div>';

        document.body.appendChild(over);
    }

    function toggleIngrediente(id) {
        var ing = modificadores.ingredientes.find(function(i) { return i.id === id; });
        if (ing) { ing.quitado = !ing.quitado; renderModificadorModal(); }
    }
    function toggleAlergia(alergia) {
        var idx = modificadores.alergias.indexOf(alergia);
        if (idx >= 0) modificadores.alergias.splice(idx, 1);
        else modificadores.alergias.push(alergia);
        renderModificadorModal();
    }
    function cerrarModificadores() {
        var over = document.getElementById('lmd-modificador-overlay');
        if (over) over.remove();
    }
    function confirmarModificadores() {
        cerrarModificadores();
        lmdToast('Modificaciones guardadas', 'success');
        renderProductos();
    }

    // ── Helpers ─────────────────────────────────────────
    function formatMoney(n) {
        return new Intl.NumberFormat('es-SV', { style: 'currency', currency: 'USD' }).format(n);
    }

    // ── Init ────────────────────────────────────────────
    window.pos = {
        seleccionarMesa: seleccionarMesa,
        seleccionarParaLlevar: seleccionarParaLlevar,
        filtrarCategoria: filtrarCategoria,
        agregarAlCarrito: agregarAlCarrito,
        eliminarDelCarrito: eliminarDelCarrito,
        confirmarListo: confirmarListo,
        cancelarOrden: cancelarOrden,
        irAPago: irAPago,
        procesarPago: procesarPago,
        keypadInput: keypadInput,
        keypadConfirmar: keypadConfirmar,
        emitirDocumento: emitirDocumento,
        nuevaOrden: nuevaOrden,
        volverAProductos: volverAProductos,
        abrirModificadores: abrirModificadores,
        toggleIngrediente: toggleIngrediente,
        toggleAlergia: toggleAlergia,
        cerrarModificadores: cerrarModificadores,
        confirmarModificadores: confirmarModificadores
    };

    document.addEventListener('DOMContentLoaded', function () {
        renderSeleccion();
    });
})();
