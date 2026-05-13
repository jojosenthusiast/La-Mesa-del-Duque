/* ============================================================================
   La Mesa del Duque — POS Pedidos (SPA via fetch)
   Tarjetas táctiles, 3 pantallas, sin recargas de página.
   ============================================================================ */

(function () {
    const api = {

        async crear(tipoServicio, mesaId, lineas) {
            const form = new FormData();
            form.append('__RequestVerificationToken', csrfToken());
            form.append('Vm.CrearPedido.TipoServicio', tipoServicio);
            if (mesaId) form.append('Vm.CrearPedido.MesaId', mesaId);
            lineas.forEach((l, i) => {
                form.append(`Vm.CrearPedido.Lineas[${i}].ProductoId`, l.productoId);
                form.append(`Vm.CrearPedido.Lineas[${i}].Cantidad`, l.cantidad);
                form.append(`Vm.CrearPedido.Lineas[${i}].PrecioUnitario`, '0');
            });

            const res = await fetch('?handler=CrearJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!res.ok) throw new Error((await res.text()) || 'Error al crear pedido');
            return res.json();
        },

        async agregar(pedidoId, productoId, cantidad) {
            const form = new FormData();
            form.append('__RequestVerificationToken', csrfToken());
            form.append('pedidoId', pedidoId);
            form.append('productoId', productoId);
            form.append('cantidad', cantidad);

            const res = await fetch('?handler=AgregarLineaJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!res.ok) throw new Error((await res.text()) || 'Error al agregar');
            return res.json();
        },

        async eliminar(pedidoId, detalleId) {
            const form = new FormData();
            form.append('__RequestVerificationToken', csrfToken());
            form.append('pedidoId', pedidoId);
            form.append('detalleId', detalleId);

            await fetch('?handler=EliminarLineaJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        },

        async actualizarCantidad(pedidoId, detalleId, cantidad) {
            const form = new FormData();
            form.append('__RequestVerificationToken', csrfToken());
            form.append('pedidoId', pedidoId);
            form.append('detalleId', detalleId);
            form.append('cantidad', cantidad);

            await fetch('?handler=ActualizarCantidadJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        },

        async pagarEfectivo(pedidoId, efectivo) {
            const form = new FormData();
            form.append('__RequestVerificationToken', csrfToken());
            form.append('pedidoId', pedidoId);
            form.append('efectivoRecibido', efectivo);

            const res = await fetch('?handler=PagarEfectivoJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            return res.json();
        },

        async pagarConPropina(pedidoId, efectivo, propina) {
            const form = new FormData();
            form.append('__RequestVerificationToken', csrfToken());
            form.append('pedidoId', pedidoId);
            form.append('efectivoRecibido', efectivo);
            form.append('propina', propina);

            const res = await fetch('?handler=PagarConPropinaJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            return res.json();
        },

        async enviarACocina(pedidoId) {
            const form = new FormData();
            form.append('__RequestVerificationToken', csrfToken());
            form.append('pedidoId', pedidoId);
            const res = await fetch('?handler=EnviarACocinaJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            return res.json();
        },

        async cambiarEstado(pedidoId, handler) {
            const form = new FormData();
            form.append('__RequestVerificationToken', csrfToken());
            form.append('pedidoId', pedidoId);

            await fetch(`?handler=${handler}`, { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        }
    };

    function csrfToken() {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    // ── Toast ───────────────────────────────────────────────
    const toast = {
        show(message, type = 'success', duration = 4000) {
            const zone = document.getElementById('lmd-pos-toast-zone');
            if (!zone) return;
            const el = document.createElement('div');
            el.className = `lmd-toast lmd-toast--${type}`;
            el.textContent = message;
            zone.appendChild(el);
            requestAnimationFrame(() => el.classList.add('lmd-toast--visible'));
            setTimeout(() => {
                el.classList.remove('lmd-toast--visible');
                setTimeout(() => el.remove(), 300);
            }, duration);
        }
    };

    // ── Modal confirm ───────────────────────────────────────
    function modalConfirm(message) {
        return new Promise(resolve => {
            const host = document.getElementById('lmd-pos-modal-host');
            if (!host) { resolve(false); return; }
            host.innerHTML = `
                <div class="lmd-modal-overlay">
                    <div class="lmd-modal">
                        <p class="lmd-modal__message">${message}</p>
                        <div class="lmd-modal__acciones">
                            <button class="lmd-pos-btn-primario" id="lmd-modal-confirmar">Confirmar</button>
                            <button class="lmd-pos-btn-cancelar" id="lmd-modal-cancelar">Cancelar</button>
                        </div>
                    </div>
                </div>`;
            host.querySelector('#lmd-modal-confirmar').onclick = () => { host.innerHTML = ''; resolve(true); };
            host.querySelector('#lmd-modal-cancelar').onclick = () => { host.innerHTML = ''; resolve(false); };
        });
    }

    // ── Estado POS ──────────────────────────────────────────
    const state = {
        pantalla: 'mesa',
        tipoServicio: 'ComerAqui',
        mesaId: null,
        pedidoActual: null,
        lineas: [],
        enviadoACocina: false,
        propinaPct: 0,
        dividirEntre: 0,
        personasPagadas: 0,
    };

    function persistState() {
        localStorage.setItem('lmdd_pos_state', JSON.stringify({ ...state, timestamp: Date.now() }));
    }

    function restoreState() {
        const saved = localStorage.getItem('lmdd_pos_state');
        if (!saved) return false;
        try {
            const data = JSON.parse(saved);
            if (Date.now() - data.timestamp > 4 * 60 * 60 * 1000) {
                localStorage.removeItem('lmdd_pos_state');
                return false;
            }
            Object.assign(state, data);
            return true;
        } catch {
            localStorage.removeItem('lmdd_pos_state');
            return false;
        }
    }

    function clearState() {
        localStorage.removeItem('lmdd_pos_state');
    }

    function formatMoney(n) {
        return new Intl.NumberFormat('es-SV', { style: 'currency', currency: 'USD' }).format(n);
    }

    // ── Render por pantalla ────────────────────────────────
    function renderPantallaMesa() {
        const container = document.getElementById('lmd-pos-contenido');
        if (!container) return;

        const estadoClase = {
            'Disponible': 'lmd-mesa-card--disponible',
            'Ocupada': 'lmd-mesa-card--ocupada',
            'Reservada': 'lmd-mesa-card--reservada',
            'EnMantenimiento': 'lmd-mesa-card--mantenimiento'
        };

        const mesasHtml = window.__lmdMesasDisponibles
            .map(m => {
                const claseEstado = estadoClase[m.estado] || '';
                const esDisponible = m.estado === 'Disponible';
                const esOcupada = m.estado === 'Ocupada';
                const pedidoInfo = esOcupada && m.pedidosActivos && m.pedidosActivos.length > 0
                    ? `<span class="lmd-mesa-card__cuenta">$${m.pedidosActivos.reduce((s, p) => s + p.total, 0).toFixed(2)}</span>`
                    : '';
                const clickHandler = esDisponible
                    ? `onclick="pos.seleccionarMesa('${m.id}')"`
                    : esOcupada
                        ? `onclick="pos.verCuentaMesa('${m.id}')"`
                        : '';
                return `
                <button class="lmd-mesa-card ${claseEstado} ${state.mesaId === m.id ? 'lmd-mesa-card--selected' : ''}"
                        data-mesa-id="${m.id}" ${clickHandler} ${!esDisponible && !esOcupada ? 'disabled' : ''}>
                    <span class="lmd-mesa-card__numero">${m.numero}</span>
                    <span class="lmd-mesa-card__capacidad">${m.capacidad}p</span>
                    ${pedidoInfo}
                </button>`;
            })
            .join('');

        container.innerHTML = `
            <div class="lmd-pos-pantalla" id="pantalla-mesa">
                <h2 class="lmd-pos-titulo">Nuevo pedido</h2>
                <div class="lmd-pos-tipo-servicio">
                    <button class="lmd-pos-tipo-btn ${state.tipoServicio === 'ComerAqui' ? 'lmd-pos-tipo-btn--activo' : ''}"
                            onclick="pos.cambiarTipo('ComerAqui')">🍽 Comer aquí</button>
                    <button class="lmd-pos-tipo-btn ${state.tipoServicio === 'ParaLlevar' ? 'lmd-pos-tipo-btn--activo' : ''}"
                            onclick="pos.cambiarTipo('ParaLlevar')">🛍 Para llevar</button>
                </div>
                ${state.tipoServicio === 'ComerAqui' ? `
                <div class="lmd-pos-mesas-grid">
                    <p class="lmd-pos-subtitulo">Seleccionar mesa</p>
                    ${mesasHtml}
                    ${!state.mesaId ? '' : `
                    <div class="lmd-pos-continuar">
                        <button class="lmd-pos-btn-primario" onclick="pos.irAPantalla('productos')">
                            Continuar → Productos
                        </button>
                    </div>`}
                </div>` : `
                <div class="lmd-pos-continuar">
                    <button class="lmd-pos-btn-primario" onclick="pos.irAPantalla('productos')">
                        Continuar → Productos
                    </button>
                </div>`}
            </div>`;
    }

    function renderPantallaProductos() {
        const container = document.getElementById('lmd-pos-contenido');
        if (!container) return;

        const categorias = {};
        window.__lmdProductosDisponibles.forEach(p => {
            if (!categorias[p.categoriaNombre]) categorias[p.categoriaNombre] = [];
            categorias[p.categoriaNombre].push(p);
        });

        const catTabs = Object.keys(categorias).map((cat, i) =>
            `<button class="lmd-pos-cat-tab ${i === 0 ? 'lmd-pos-cat-tab--activo' : ''}" onclick="pos.filtrarCategoria('${cat.replace(/'/g, "\\'")}')">${cat}</button>`
        ).join('');

        const allProductCards = Object.entries(categorias).map(([cat, prods]) =>
            prods.map(p => `
                <button class="lmd-pos-producto-card" data-categoria="${cat.replace(/'/g, "\\'")}"
                        onclick="pos.agregarProducto('${p.id}')">
                    <div class="lmd-pos-producto-card__nombre">${p.nombre}</div>
                    <div class="lmd-pos-producto-card__precio">${formatMoney(p.precio)}</div>
                    <div class="lmd-pos-producto-card__tiempo">${p.tiempoPreparacionMin}min</div>
                </button>`).join('')
        ).join('');

        const lineasHtml = state.lineas.map(l => `
            <div class="lmd-pos-linea">
                <div class="lmd-pos-linea__info">
                    <strong>${l.productoNombre}</strong>
                    <small>${formatMoney(l.precioUnitario)} c/u</small>
                </div>
                <div class="lmd-pos-linea__acciones">
                    <button class="lmd-pos-linea__qty" onclick="pos.cambiarCantidad('${l.id}', ${l.cantidad - 1})" ${l.cantidad <= 1 ? 'disabled' : ''}>−</button>
                    <span class="lmd-pos-linea__cantidad">${l.cantidad}</span>
                    <button class="lmd-pos-linea__qty" onclick="pos.cambiarCantidad('${l.id}', ${l.cantidad + 1})">+</button>
                    <span class="lmd-pos-linea__subtotal">${formatMoney(l.subtotal)}</span>
                    <button class="lmd-pos-linea__eliminar" onclick="pos.eliminarLinea('${l.id}')" title="Quitar">✕</button>
                </div>
            </div>`).join('');

        const total = state.lineas.reduce((s, l) => s + l.subtotal, 0);
        const cocinaHtml = state.pedidoActual && !state.enviadoACocina
            ? `<button class="lmd-pos-btn-cocina" onclick="pos.enviarACocina()">🍳 Enviar a Cocina</button>`
            : state.enviadoACocina
                ? `<div class="lmd-pos-cocina-estado">✅ Enviado a cocina</div>`
                : '';

        container.innerHTML = `
            <div class="lmd-pos-pantalla lmd-pos-productos-pantalla" id="pantalla-productos">
                <div class="lmd-pos-catalogo">
                    <div class="lmd-pos-topbar">
                        <button class="lmd-pos-btn-atras" onclick="pos.irAPantalla('mesa')">← ${state.tipoServicio === 'ComerAqui' ? 'Cambiar mesa' : 'Tipo servicio'}</button>
                        <h2 class="lmd-pos-titulo">Productos</h2>
                    </div>
                    <div class="lmd-pos-cat-tabs">${catTabs}</div>
                    <div class="lmd-pos-productos-grid">${allProductCards}</div>
                </div>
                <aside class="lmd-pos-resumen">
                    <h3 class="lmd-pos-resumen__titulo">Pedido</h3>
                    <div class="lmd-pos-resumen__lineas">${lineasHtml || '<p class="lmd-empty-state">Agregue productos del catálogo</p>'}</div>
                    <div class="lmd-pos-total-bar">
                        <span>Total</span>
                        <strong>${formatMoney(total)}</strong>
                    </div>
                    ${cocinaHtml}
                    <button class="lmd-pos-btn-primario lmd-pos-btn-pagar" ${state.lineas.length === 0 ? 'disabled' : ''}
                            onclick="pos.irAPantalla('pago')">
                        Ir a pagar
                    </button>
                    <button class="lmd-pos-btn-cancelar" onclick="pos.cancelarPedido()">Cancelar</button>
                </aside>
            </div>`;
    }

    function renderPantallaPago() {
        const container = document.getElementById('lmd-pos-contenido');
        if (!container) return;

        const total = state.lineas.reduce((s, l) => s + l.subtotal, 0);
        const propina = total * (state.propinaPct / 100);
        const totalConPropina = total + propina;

        const splitHtml = state.dividirEntre > 1
            ? `<div class="lmd-pos-split-info">Persona ${state.personasPagadas + 1} de ${state.dividirEntre} — <strong>${formatMoney(totalConPropina / state.dividirEntre)}</strong></div>`
            : '';

        container.innerHTML = `
            <div class="lmd-pos-pantalla" id="pantalla-pago">
                <button class="lmd-pos-btn-atras" onclick="pos.irAPantalla('productos')">← Volver a productos</button>
                <h2 class="lmd-pos-titulo">Pago</h2>
                <div class="lmd-pos-pago-total">${formatMoney(total)}</div>

                <div class="lmd-pos-split-btns" id="split-btns">
                    <span class="lmd-pos-pago-label">Dividir cuenta</span>
                    <button class="lmd-pos-split-btn ${state.dividirEntre === 2 ? 'lmd-pos-split-btn--activo' : ''}" onclick="pos.dividirCuenta(2)">÷2</button>
                    <button class="lmd-pos-split-btn ${state.dividirEntre === 3 ? 'lmd-pos-split-btn--activo' : ''}" onclick="pos.dividirCuenta(3)">÷3</button>
                    <button class="lmd-pos-split-btn ${state.dividirEntre === 4 ? 'lmd-pos-split-btn--activo' : ''}" onclick="pos.dividirCuenta(4)">÷4</button>
                    <button class="lmd-pos-split-btn ${state.dividirEntre === 5 ? 'lmd-pos-split-btn--activo' : ''}" onclick="pos.dividirCuenta(5)">÷5</button>
                    ${state.dividirEntre > 1 ? `<button class="lmd-pos-split-btn lmd-pos-split-btn--reset" onclick="pos.dividirCuenta(0)">×</button>` : ''}
                </div>

                <div class="lmd-pos-tip-btns" id="tip-btns">
                    <span class="lmd-pos-pago-label">Propina</span>
                    <button class="lmd-pos-tip-btn ${state.propinaPct === 10 ? 'lmd-pos-tip-btn--activo' : ''}" onclick="pos.seleccionarPropina(10)">10%</button>
                    <button class="lmd-pos-tip-btn ${state.propinaPct === 15 ? 'lmd-pos-tip-btn--activo' : ''}" onclick="pos.seleccionarPropina(15)">15%</button>
                    <button class="lmd-pos-tip-btn ${state.propinaPct === 20 ? 'lmd-pos-tip-btn--activo' : ''}" onclick="pos.seleccionarPropina(20)">20%</button>
                    <button class="lmd-pos-tip-btn lmd-pos-tip-btn--custom ${state.propinaPct === 0 ? 'lmd-pos-tip-btn--activo' : ''}" id="tip-sin-propina" onclick="pos.seleccionarPropina(0)">Sin propina</button>
                </div>

                ${splitHtml}

                <div class="lmd-pos-pago-efectivo">
                    <label class="lmd-pos-pago-label">Efectivo recibido</label>
                    <div class="lmd-pos-pago-input-group">
                        <span class="lmd-pos-pago-input-prefijo">$</span>
                        <input type="number" id="efectivo-input" class="lmd-pos-pago-input" step="0.01" min="0"
                               placeholder="${totalConPropina.toFixed(2)}" oninput="pos.calcularCambio()" autofocus />
                    </div>
                    <div class="lmd-pos-pago-cambio" id="cambio-display"></div>
                </div>

                <div class="lmd-pos-pago-acciones">
                    <button class="lmd-pos-btn-primario" id="btn-pagar-efectivo" onclick="pos.pagarEfectivo()" disabled>Pagar con efectivo</button>
                    <button class="lmd-pos-btn-secundario" onclick="pos.pagarConTarjeta()">Pagar con tarjeta</button>
                </div>

                <button class="lmd-pos-btn-cancelar" onclick="pos.cancelarPedido()">Cancelar pedido</button>
            </div>`;
    }

    // ── API pública ─────────────────────────────────────────
    window.pos = {
        cambiarTipo(tipo) {
            state.tipoServicio = tipo;
            if (tipo === 'ParaLlevar') state.mesaId = null;
            renderPantallaMesa();
            persistState();
        },

        seleccionarMesa(id) {
            state.mesaId = state.mesaId === id ? null : id;
            renderPantallaMesa();
            persistState();
        },

        verCuentaMesa(id) {
            const mesa = window.__lmdMesasDisponibles.find(m => m.id === id);
            if (!mesa || !mesa.pedidosActivos || mesa.pedidosActivos.length === 0) {
                toast.show('No hay pedidos activos en esta mesa.', 'info');
                return;
            }
            const total = mesa.pedidosActivos.reduce((s, p) => s + p.total, 0);
            toast.show(`Mesa ${mesa.numero}: ${mesa.pedidosActivos.length} pedido(s). Total: ${formatMoney(total)}`, 'info');
        },

        irAPantalla(pantalla) {
            state.pantalla = pantalla;
            if (pantalla === 'mesa') renderPantallaMesa();
            else if (pantalla === 'productos') renderPantallaProductos();
            else if (pantalla === 'pago') renderPantallaPago();
            persistState();
        },

        filtrarCategoria(cat) {
            document.querySelectorAll('.lmd-pos-cat-tab').forEach(t => t.classList.remove('lmd-pos-cat-tab--activo'));
            const tab = Array.from(document.querySelectorAll('.lmd-pos-cat-tab')).find(t => t.textContent.trim() === cat);
            if (tab) tab.classList.add('lmd-pos-cat-tab--activo');

            document.querySelectorAll('.lmd-pos-producto-card').forEach(c => {
                c.style.display = cat && c.dataset.categoria !== cat ? 'none' : '';
            });
        },

        async agregarProducto(productoId) {
            if (!state.pedidoActual) {
                try {
                    const prod = window.__lmdProductosDisponibles.find(p => p.id === productoId);
                    const result = await api.crear(state.tipoServicio, state.mesaId, [{
                        productoId,
                        cantidad: 1,
                        precioUnitario: prod.precio
                    }]);
                    state.pedidoActual = { id: result.pedidoId, estado: result.estado };
                    state.lineas = result.lineas || [{
                        id: result.lineaId || crypto.randomUUID(),
                        productoId,
                        productoNombre: prod.nombre,
                        cantidad: 1,
                        precioUnitario: prod.precio,
                        subtotal: prod.precio
                    }];
                    state.enviadoACocina = false;
                    toast.show('Pedido creado');
                } catch (e) {
                    toast.show('Error al crear pedido: ' + e.message, 'error');
                    return;
                }
            } else {
                try {
                    await api.agregar(state.pedidoActual.id, productoId, 1);
                    const prod = window.__lmdProductosDisponibles.find(p => p.id === productoId);
                    const existente = state.lineas.find(l => l.productoId === productoId);
                    if (existente) {
                        existente.cantidad++;
                        existente.subtotal = existente.cantidad * existente.precioUnitario;
                    } else {
                        state.lineas.push({
                            id: crypto.randomUUID(),
                            productoId,
                            productoNombre: prod.nombre,
                            cantidad: 1,
                            precioUnitario: prod.precio,
                            subtotal: prod.precio
                        });
                    }
                    toast.show('Producto agregado');
                } catch (e) {
                    toast.show('Error: ' + e.message, 'error');
                    return;
                }
            }
            renderPantallaProductos();
            persistState();
        },

        async cambiarCantidad(lineaId, nuevaCantidad) {
            if (nuevaCantidad < 1 || !state.pedidoActual) return;
            const linea = state.lineas.find(l => l.id === lineaId);
            if (!linea) return;

            try {
                await api.actualizarCantidad(state.pedidoActual.id, linea.id, nuevaCantidad);
                linea.cantidad = nuevaCantidad;
                linea.subtotal = nuevaCantidad * linea.precioUnitario;
                renderPantallaProductos();
                persistState();
            } catch (e) { toast.show('Error: ' + e.message, 'error'); }
        },

        async eliminarLinea(lineaId) {
            if (!state.pedidoActual) return;
            const confirmed = await modalConfirm('¿Quitar este producto?');
            if (!confirmed) return;
            try {
                await api.eliminar(state.pedidoActual.id, lineaId);
                state.lineas = state.lineas.filter(l => l.id !== lineaId);
                if (state.lineas.length === 0) {
                    state.pedidoActual = null;
                    state.enviadoACocina = false;
                }
                renderPantallaProductos();
                persistState();
            } catch (e) { toast.show('Error: ' + e.message, 'error'); }
        },

        calcularCambio() {
            const input = document.getElementById('efectivo-input');
            const display = document.getElementById('cambio-display');
            const btn = document.getElementById('btn-pagar-efectivo');
            if (!input || !display || !btn) return;

            const total = state.lineas.reduce((s, l) => s + l.subtotal, 0);
            const propina = total * (state.propinaPct / 100);
            const totalConPropina = total + propina;
            const montoAdeudado = state.dividirEntre > 1
                ? totalConPropina / state.dividirEntre
                : totalConPropina;
            const efectivo = parseFloat(input.value) || 0;

            if (efectivo >= montoAdeudado) {
                const cambio = efectivo - montoAdeudado;
                let html = `<span class="lmd-pos-pago-cambio-valor">Cambio: ${formatMoney(cambio)}</span>`;
                if (propina > 0) html += `<div class="lmd-pos-pago-propina">Propina (${state.propinaPct}%): ${formatMoney(propina)}</div>`;
                if (state.dividirEntre > 1) html += `<div class="lmd-pos-pago-split-info">Persona ${state.personasPagadas + 1} de ${state.dividirEntre}</div>`;
                html += `<div class="lmd-pos-pago-total-final">Total: ${formatMoney(totalConPropina)}</div>`;
                display.innerHTML = html;
                display.className = 'lmd-pos-pago-cambio lmd-pos-pago-cambio--ok';
                btn.disabled = false;
            } else {
                display.innerHTML = efectivo > 0
                    ? `<span>Faltan ${formatMoney(montoAdeudado - efectivo)}</span>${propina > 0 ? `<div class="lmd-pos-pago-propina">Propina: ${formatMoney(propina)}</div>` : ''}`
                    : (propina > 0 ? `<div class="lmd-pos-pago-propina">Propina: ${formatMoney(propina)}</div>` : '');
                display.className = 'lmd-pos-pago-cambio';
                btn.disabled = true;
            }
        },

        async pagarEfectivo() {
            const input = document.getElementById('efectivo-input');
            const efectivo = parseFloat(input?.value || 0);
            if (!state.pedidoActual || efectivo <= 0) return;

            const total = state.lineas.reduce((s, l) => s + l.subtotal, 0);
            const propina = total * (state.propinaPct / 100);
            const totalConPropina = total + propina;

            if (state.dividirEntre > 1) {
                const montoPorPersona = totalConPropina / state.dividirEntre;
                if (efectivo < montoPorPersona) {
                    toast.show(`Faltan ${formatMoney(montoPorPersona - efectivo)} para esta persona`, 'error');
                    return;
                }
                state.personasPagadas++;
                if (state.personasPagadas < state.dividirEntre) {
                    toast.show(`Persona ${state.personasPagadas} pagó ${formatMoney(efectivo)}. Quedan ${state.dividirEntre - state.personasPagadas}.`, 'info');
                    input.value = '';
                    pos.calcularCambio();
                    renderPantallaPago();
                    persistState();
                    return;
                }
            }

            try {
                const result = state.propinaPct > 0
                    ? await api.pagarConPropina(state.pedidoActual.id, efectivo, propina)
                    : await api.pagarEfectivo(state.pedidoActual.id, efectivo);
                state.pedidoActual = null;
                state.lineas = [];
                state.mesaId = null;
                state.enviadoACocina = false;
                state.propinaPct = 0;
                state.dividirEntre = 0;
                state.personasPagadas = 0;
                clearState();
                toast.show(result.mensaje || 'Pedido pagado.', 'success');
                state.pantalla = 'mesa';
                renderPantallaMesa();
            } catch (e) { toast.show('Error: ' + e.message, 'error'); }
        },

        seleccionarPropina(pct) {
            state.propinaPct = pct;
            renderPantallaPago();
            persistState();
        },

        dividirCuenta(n) {
            state.dividirEntre = n;
            state.personasPagadas = 0;
            renderPantallaPago();
            persistState();
        },

        async pagarConTarjeta() {
            if (!state.pedidoActual) return;
            try {
                await api.cambiarEstado(state.pedidoActual.id, 'Pagar');
                state.pedidoActual = null;
                state.lineas = [];
                state.mesaId = null;
                state.enviadoACocina = false;
                clearState();
                toast.show('Pedido pagado con tarjeta.', 'success');
                state.pantalla = 'mesa';
                renderPantallaMesa();
            } catch (e) { toast.show('Error: ' + e.message, 'error'); }
        },

        async cancelarPedido() {
            if (!state.pedidoActual) {
                state.pantalla = 'mesa';
                renderPantallaMesa();
                persistState();
                return;
            }
            const confirmed = await modalConfirm('¿Cancelar este pedido?');
            if (!confirmed) return;
            try {
                await api.cambiarEstado(state.pedidoActual.id, 'Cancelar');
                state.pedidoActual = null;
                state.lineas = [];
                state.mesaId = null;
                state.enviadoACocina = false;
                state.propinaPct = 0;
                state.dividirEntre = 0;
                state.personasPagadas = 0;
                clearState();
                state.pantalla = 'mesa';
                renderPantallaMesa();
            } catch (e) { toast.show('Error: ' + e.message, 'error'); }
        },

        async marcarEnPreparacion() {
            if (!state.pedidoActual) return;
            try {
                await api.cambiarEstado(state.pedidoActual.id, 'MarcarEnPreparacion');
                toast.show('Pedido marcado en preparación.', 'success');
            } catch (e) { toast.show('Error: ' + e.message, 'error'); }
        },

        async enviarACocina() {
            if (!state.pedidoActual || state.enviadoACocina) return;
            try {
                await api.enviarACocina(state.pedidoActual.id);
                state.enviadoACocina = true;
                toast.show('Pedido enviado a cocina.', 'success');
                renderPantallaProductos();
                persistState();
            } catch (e) {
                toast.show('Error al enviar: ' + e.message, 'error');
            }
        }
    };

    // ── Inicialización ──────────────────────────────────────
    window.__lmdMesasDisponibles = window.__lmdMesasDisponibles || [];
    window.__lmdProductosDisponibles = window.__lmdProductosDisponibles || [];

    document.addEventListener('DOMContentLoaded', () => {
        const restored = restoreState();
        if (restored) {
            if (state.pantalla === 'mesa') renderPantallaMesa();
            else if (state.pantalla === 'productos') renderPantallaProductos();
            else if (state.pantalla === 'pago') renderPantallaPago();
        } else {
            renderPantallaMesa();
        }
    });
})();
