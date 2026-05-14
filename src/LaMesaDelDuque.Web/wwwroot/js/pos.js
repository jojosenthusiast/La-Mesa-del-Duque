/* ============================================================================
   La Mesa del Duque — POS Pedidos (SPA via fetch)
   Tarjetas táctiles, 3 pantallas, sin recargas de página.
   Cuentas reales + SignalR concurrente + Split por items.
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

        async cambiarEstado(pedidoId, handler) {
            const form = new FormData();
            form.append('__RequestVerificationToken', csrfToken());
            form.append('pedidoId', pedidoId);

            await fetch(`?handler=${handler}`, { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        },

        async marcarEnCobro(pedidoId) {
            const form = new FormData();
            form.append('__RequestVerificationToken', csrfToken());
            form.append('pedidoId', pedidoId);

            const res = await fetch('?handler=MarcarEnCobroJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!res.ok) throw new Error((await res.text()) || 'Error al marcar en cobro');
            return res.json();
        },

        async crearCuentas(pedidoId, cantidad) {
            const form = new FormData();
            form.append('__RequestVerificationToken', csrfToken());
            form.append('pedidoId', pedidoId);
            form.append('cantidad', cantidad);

            const res = await fetch('?handler=CrearCuentasJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!res.ok) throw new Error((await res.text()) || 'Error al crear cuentas');
            return res.json();
        },

        async crearCuentasConItems(pedidoId, asignaciones) {
            const res = await fetch('?handler=CrearCuentasConItemsJson', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': csrfToken(),
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({ pedidoId, asignaciones })
            });
            if (!res.ok) throw new Error((await res.text()) || 'Error al crear cuentas por items');
            return res.json();
        },

        async obtenerCuentas(pedidoId) {
            const form = new FormData();
            form.append('__RequestVerificationToken', csrfToken());
            form.append('pedidoId', pedidoId);

            const res = await fetch('?handler=ObtenerCuentasJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!res.ok) throw new Error((await res.text()) || 'Error al obtener cuentas');
            return res.json();
        },

        async pagarCuenta(cuentaId, metodoPago, propinaMonto) {
            const form = new FormData();
            form.append('__RequestVerificationToken', csrfToken());
            form.append('cuentaId', cuentaId);
            form.append('metodoPago', metodoPago);
            form.append('propinaMonto', propinaMonto);

            const res = await fetch('?handler=PagarCuentaJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!res.ok) throw new Error((await res.text()) || 'Error al pagar cuenta');
            return res.json();
        }
    };

    function csrfToken() {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    // ── Toast notifications ─────────────────────────────────
    const toast = {
        show(message, type = 'error', duration = 4000) {
            const container = document.getElementById('lmd-toast-zone') || (() => {
                const div = document.createElement('div'); div.id = 'lmd-toast-zone';
                document.body.appendChild(div); return div;
            })();
            const el = document.createElement('div');
            el.className = `lmd-toast lmd-toast--${type}`;
            el.textContent = message;
            container.appendChild(el);
            setTimeout(() => { el.classList.add('lmd-toast--out'); setTimeout(() => el.remove(), 300); }, duration);
        }
    };

    function modalConfirm(message) {
        return new Promise(resolve => {
            const overlay = document.createElement('div'); overlay.className = 'lmd-modal-overlay';
            const modal = document.createElement('div'); modal.className = 'lmd-modal';
            modal.innerHTML = `<p>${message}</p>
                <div class="lmd-modal-actions">
                    <button class="lmd-pos-btn-cancelar" id="modal-no">Cancelar</button>
                    <button class="lmd-pos-btn-primario" id="modal-si">Confirmar</button>
                </div>`;
            overlay.appendChild(modal); document.body.appendChild(overlay);
            document.getElementById('modal-si').onclick = () => { overlay.remove(); resolve(true); };
            document.getElementById('modal-no').onclick = () => { overlay.remove(); resolve(false); };
        });
    }

    // ── SignalR ─────────────────────────────────────────────
    let connection = null;

    function initSignalR() {
        if (typeof signalR === 'undefined') {
            console.warn('SignalR no disponible');
            return;
        }
        connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/pedidos')
            .withAutomaticReconnect()
            .build();

        connection.on('CuentaPagada', (cuentaId, pedidoId) => {
            if (state.pedidoActual && state.pedidoActual.id === pedidoId) {
                const cuenta = state.cuentas.find(c => c.id === cuentaId);
                if (cuenta) {
                    cuenta.estado = 'Pagada';
                    renderPantallaPago();
                }
            }
        });

        connection.on('CuentasCreadas', (pedidoId, cuentas) => {
            if (state.pedidoActual && state.pedidoActual.id === pedidoId) {
                state.cuentas = cuentas.map(c => ({
                    ...c,
                    metodoPagoSeleccionado: null,
                    propinaMonto: 0,
                    propinaPorcentaje: null
                }));
                renderPantallaPago();
            }
        });

        connection.on('EstadoCambiado', (pedidoId, nuevoEstado) => {
            if (state.pedidoActual && state.pedidoActual.id === pedidoId) {
                state.pedidoActual.estado = nuevoEstado;
            }
        });

        startConnection();
    }

    async function startConnection() {
        if (!connection) return;
        try {
            await connection.start();
            console.log('SignalR conectado.');
            if (state.pedidoActual) {
                await connection.invoke('UnirseAPedido', state.pedidoActual.id);
            }
        } catch (e) {
            console.error('SignalR error:', e);
            setTimeout(startConnection, 5000);
        }
    }

    async function leavePedidoGroup() {
        if (!connection || !state.pedidoActual) return;
        try {
            await connection.invoke('SalirDePedido', state.pedidoActual.id);
        } catch (e) { /* ignore */ }
    }

    async function joinPedidoGroup(pedidoId) {
        if (!connection) return;
        try {
            await connection.invoke('UnirseAPedido', pedidoId);
        } catch (e) { /* ignore */ }
    }

    // ── Estado POS ──────────────────────────────────────────
    const state = {
        pantalla: 'mesa',
        tipoServicio: 'ComerAqui',
        mesaId: null,
        pedidoActual: null,
        lineas: [],
        cuentas: [],
        modoSplit: 'igual',
        splitAsignaciones: {},
        splitCuentaActiva: 1
    };

    function formatMoney(n) {
        return new Intl.NumberFormat('es-SV', { style: 'currency', currency: 'USD' }).format(n);
    }

    // ── Render por pantalla ────────────────────────────────
    function renderPantallaMesa() {
        const container = document.getElementById('lmd-pos-contenido');
        if (!container) return;

        const mesasHtml = window.__lmdMesasDisponibles
            .map(m => `
                <button class="lmd-mesa-card ${state.mesaId === m.id ? 'lmd-mesa-card--selected' : ''}"
                        data-mesa-id="${m.id}" onclick="pos.seleccionarMesa('${m.id}')">
                    <span class="lmd-mesa-card__numero">${m.numero}</span>
                    <span class="lmd-mesa-card__capacidad">${m.capacidad}p</span>
                </button>`)
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
                    <button class="lmd-pos-btn-primario lmd-pos-btn-pagar" ${state.lineas.length === 0 ? 'disabled' : ''}
                            onclick="pos.irAPantallaPago()">
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
        const cuentas = state.cuentas || [];
        const pagadas = cuentas.filter(c => c.estado === 'Pagada').length;
        const todasPagadas = cuentas.length > 0 && pagadas === cuentas.length;

        if (todasPagadas) {
            container.innerHTML = `
                <div class="lmd-pos-pantalla" id="pantalla-pago">
                    <div class="lmd-pos-pago-exito">
                        <div class="lmd-pos-pago-exito__icono">✅</div>
                        <h2 class="lmd-pos-pago-exito__titulo">Todas las cuentas pagadas</h2>
                        <div class="lmd-pos-pago-total">${formatMoney(total)}</div>
                        <button class="lmd-pos-btn-primario" onclick="pos.nuevoPedido()">Nuevo pedido</button>
                    </div>
                </div>`;
            return;
        }

        let cuentasHtml = '';

        if (cuentas.length === 0) {
            const modoIgualActivo = state.modoSplit === 'igual';
            const splitControls = modoIgualActivo ? `
                <div class="lmd-cuentas-crear">
                    <p class="lmd-pos-subtitulo">Dividir cuenta en:</p>
                    <div class="lmd-cuentas-split-grid">
                        <button class="lmd-pos-tipo-btn" onclick="pos.crearCuentas(2)">÷ 2</button>
                        <button class="lmd-pos-tipo-btn" onclick="pos.crearCuentas(3)">÷ 3</button>
                        <button class="lmd-pos-tipo-btn" onclick="pos.crearCuentas(4)">÷ 4</button>
                        <button class="lmd-pos-tipo-btn" onclick="pos.crearCuentas(5)">÷ 5</button>
                    </div>
                    <button class="lmd-pos-btn-secundario" onclick="pos.crearCuentas(1)">Cuenta única</button>
                </div>` : renderSplitItemsUI();

            cuentasHtml = `
                <div class="lmd-pos-split-modo">
                    <button class="lmd-pos-split-modo-btn ${modoIgualActivo ? 'lmd-pos-split-modo-btn--activo' : ''}"
                            onclick="pos.cambiarModoSplit('igual')">Partes iguales</button>
                    <button class="lmd-pos-split-modo-btn ${!modoIgualActivo ? 'lmd-pos-split-modo-btn--activo' : ''}"
                            onclick="pos.cambiarModoSplit('items')">Por items</button>
                </div>
                ${splitControls}`;
        } else {
            const cuentaCards = cuentas.map((c) => {
                const esPagada = c.estado === 'Pagada';
                const metodos = ['Efectivo', 'Tarjeta', 'Transferencia', 'QR'];
                const metodoBtns = metodos.map(m => `
                    <button class="lmd-cuenta-metodo-btn ${c.metodoPagoSeleccionado === m ? 'lmd-cuenta-metodo-btn--activo' : ''}"
                            onclick="pos.seleccionarMetodo('${c.id}', '${m}')"
                            ${esPagada ? 'disabled' : ''}>${m}</button>
                `).join('');

                const propinaBtns = [10, 15, 20].map(p => `
                    <button class="lmd-cuenta-propina-btn ${c.propinaPorcentaje === p ? 'lmd-cuenta-propina-btn--activo' : ''}"
                            onclick="pos.seleccionarPropina('${c.id}', ${p})"
                            ${esPagada ? 'disabled' : ''}>${p}%</button>
                `).join('');

                const totalConPropina = c.total + (c.propinaMonto || 0);

                return `
                    <div class="lmd-cuenta-card ${esPagada ? 'lmd-cuenta-card--pagada' : ''}">
                        <div class="lmd-cuenta-card__header">
                            <span class="lmd-cuenta-card__numero">Cuenta ${c.numero}</span>
                            <span class="lmd-cuenta-card__total">${formatMoney(totalConPropina)}</span>
                        </div>
                        ${!esPagada ? `
                        <div class="lmd-cuenta-card__metodos">
                            ${metodoBtns}
                        </div>
                        <div class="lmd-cuenta-card__propina">
                            <span class="lmd-cuenta-card__propina-label">Propina:</span>
                            <div class="lmd-cuenta-propina-btns">${propinaBtns}</div>
                            <input type="number" class="lmd-cuenta-propina-custom" placeholder="Otra $" min="0" step="0.01"
                                   onchange="pos.propinaCustom('${c.id}', this.value)" />
                        </div>
                        <button class="lmd-pos-btn-primario lmd-cuenta-pagar-btn"
                                onclick="pos.pagarCuenta('${c.id}')"
                                ${!c.metodoPagoSeleccionado ? 'disabled' : ''}>
                            Pagar ${formatMoney(totalConPropina)}
                        </button>
                        ` : '<div class="lmd-cuenta-pagada-badge">✓ Pagada</div>'}
                    </div>`;
            }).join('');

            cuentasHtml = `
                <div class="lmd-cuentas-grid">
                    ${cuentaCards}
                </div>
                <div class="lmd-cuenta-progress">
                    <div class="lmd-cuenta-progress__track">
                        <div class="lmd-cuenta-progress__bar" style="width: ${(pagadas / cuentas.length) * 100}%"></div>
                    </div>
                    <span class="lmd-cuenta-progress__text">${pagadas} de ${cuentas.length} cuentas pagadas</span>
                </div>`;
        }

        container.innerHTML = `
            <div class="lmd-pos-pantalla" id="pantalla-pago">
                <button class="lmd-pos-btn-atras" onclick="pos.irAPantalla('productos')">← Volver a productos</button>
                <h2 class="lmd-pos-titulo">Pago</h2>
                <div class="lmd-pos-pago-total">${formatMoney(total)}</div>
                ${cuentasHtml}
                <button class="lmd-pos-btn-cancelar" onclick="pos.cancelarPedido()">Cancelar pedido</button>
            </div>`;
    }

    function renderSplitItemsUI() {
        const asignaciones = state.splitAsignaciones;
        const cuentasNums = Object.keys(asignaciones).map(Number).sort((a, b) => a - b);
        if (cuentasNums.length === 0) {
            state.splitAsignaciones = { 1: [], 2: [] };
            cuentasNums.push(1, 2);
        }

        // Calcular cantidades asignadas por detalle
        const asignadoPorDetalle = {};
        Object.values(asignaciones).forEach(items => {
            items.forEach(it => {
                asignadoPorDetalle[it.detalleId] = (asignadoPorDetalle[it.detalleId] || 0) + it.cantidad;
            });
        });

        const colsHtml = cuentasNums.map(num => {
            const items = asignaciones[num] || [];
            const colTotal = items.reduce((s, it) => s + it.cantidad * it.precioUnitario, 0);
            const esActiva = state.splitCuentaActiva === num;
            const itemsHtml = items.map((it, idx) => `
                <div class="lmd-pos-split-item lmd-pos-split-item-asignado" onclick="pos.quitarItemDeCuenta(${num}, ${idx})">
                    <div class="lmd-pos-split-item-info">
                        <span class="lmd-pos-split-item-nombre">${it.cantidad}× ${it.productoNombre}</span>
                        <span class="lmd-pos-split-item-precio">${formatMoney(it.cantidad * it.precioUnitario)}</span>
                    </div>
                </div>
            `).join('');

            return `
                <div class="lmd-pos-split-col ${esActiva ? 'lmd-pos-split-col--activa' : ''}" onclick="pos.seleccionarCuentaSplit(${num})">
                    <h4>Cuenta ${num} <span class="lmd-pos-split-col-total">${formatMoney(colTotal)}</span></h4>
                    <div class="lmd-pos-split-col-items">${itemsHtml || '<span style="color:var(--lmd-gris-piedra);font-size:0.75rem">Toca items para asignar</span>'}</div>
                </div>`;
        }).join('');

        const poolItemsHtml = state.lineas.map(l => {
            const yaAsignado = asignadoPorDetalle[l.id] || 0;
            const restante = l.cantidad - yaAsignado;
            if (restante <= 0) return '';
            return `
                <div class="lmd-pos-split-item" data-detalle-id="${l.id}" onclick="pos.asignarItemACuenta('${l.id}')">
                    <div class="lmd-pos-split-item-info">
                        <span class="lmd-pos-split-item-nombre">${restante}× ${l.productoNombre}</span>
                        <span class="lmd-pos-split-item-precio">${formatMoney(restante * l.precioUnitario)}</span>
                    </div>
                </div>`;
        }).join('');

        const puedeConfirmar = cuentasNums.length >= 2 && Object.values(asignaciones).some(items => items.length > 0);

        return `
            <div class="lmd-pos-split-items" id="split-items">
                <div class="lmd-pos-split-header">
                    <span>Asignar items a cuentas</span>
                    <button onclick="pos.agregarCuentaSplit()">+ Cuenta</button>
                </div>
                <div class="lmd-pos-split-cols" id="split-cols">
                    ${colsHtml}
                </div>
                <div class="lmd-pos-split-pool">
                    <h4>Items sin asignar</h4>
                    ${poolItemsHtml || '<span style="color:var(--lmd-gris-piedra);font-size:0.8125rem">Todos los items asignados</span>'}
                </div>
                <button class="lmd-pos-btn-primario" onclick="pos.confirmarSplitItems()" ${puedeConfirmar ? '' : 'disabled'}>
                    Confirmar división
                </button>
            </div>`;
    }

    // ── API pública ─────────────────────────────────────────
    window.pos = {
        cambiarTipo(tipo) {
            state.tipoServicio = tipo;
            if (tipo === 'ParaLlevar') state.mesaId = null;
            renderPantallaMesa();
        },

        seleccionarMesa(id) {
            state.mesaId = state.mesaId === id ? null : id;
            renderPantallaMesa();
        },

        irAPantalla(pantalla) {
            state.pantalla = pantalla;
            if (pantalla === 'mesa') renderPantallaMesa();
            else if (pantalla === 'productos') renderPantallaProductos();
            else if (pantalla === 'pago') renderPantallaPago();
        },

        async irAPantallaPago() {
            state.pantalla = 'pago';
            if (state.pedidoActual) {
                try {
                    await api.marcarEnCobro(state.pedidoActual.id);
                    const cuentas = await api.obtenerCuentas(state.pedidoActual.id);
                    state.cuentas = cuentas.map(c => ({
                        ...c,
                        metodoPagoSeleccionado: null,
                        propinaMonto: 0,
                        propinaPorcentaje: null
                    }));
                    await joinPedidoGroup(state.pedidoActual.id);
                } catch (e) {
                    console.warn('Error al preparar pago:', e);
                }
            }
            renderPantallaPago();
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
                    await joinPedidoGroup(state.pedidoActual.id);
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
                } catch (e) {
                    toast.show('Error: ' + e.message, 'error');
                    return;
                }
            }
            renderPantallaProductos();
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
            } catch (e) { toast.show('Error: ' + e.message, 'error'); }
        },

        async eliminarLinea(lineaId) {
            if (!state.pedidoActual || !await modalConfirm('¿Quitar este producto?')) return;
            try {
                await api.eliminar(state.pedidoActual.id, lineaId);
                state.lineas = state.lineas.filter(l => l.id !== lineaId);
                if (state.lineas.length === 0) {
                    state.pedidoActual = null;
                }
                renderPantallaProductos();
            } catch (e) { toast.show('Error: ' + e.message, 'error'); }
        },

        calcularCambio() {
            const input = document.getElementById('efectivo-input');
            const display = document.getElementById('cambio-display');
            const btn = document.getElementById('btn-pagar-efectivo');
            if (!input || !display || !btn) return;

            const total = state.lineas.reduce((s, l) => s + l.subtotal, 0);
            const efectivo = parseFloat(input.value) || 0;

            if (efectivo >= total) {
                const cambio = efectivo - total;
                display.innerHTML = `<span class="lmd-pos-pago-cambio-valor">Cambio: ${formatMoney(cambio)}</span>`;
                display.className = 'lmd-pos-pago-cambio lmd-pos-pago-cambio--ok';
                btn.disabled = false;
            } else {
                display.innerHTML = efectivo > 0 ? `<span>Faltan ${formatMoney(total - efectivo)}</span>` : '';
                display.className = 'lmd-pos-pago-cambio';
                btn.disabled = true;
            }
        },

        async pagarEfectivo() {
            const input = document.getElementById('efectivo-input');
            const efectivo = parseFloat(input?.value || 0);
            if (!state.pedidoActual || efectivo <= 0) return;
            try {
                const result = await api.pagarEfectivo(state.pedidoActual.id, efectivo);
                await leavePedidoGroup();
                state.pedidoActual = null;
                state.lineas = [];
                state.cuentas = [];
                state.mesaId = null;
                toast.show(result.mensaje || 'Pedido pagado.', 'success');
                state.pantalla = 'mesa';
                renderPantallaMesa();
            } catch (e) { toast.show('Error: ' + e.message, 'error'); }
        },

        async pagarConTarjeta() {
            if (!state.pedidoActual) return;
            try {
                await api.cambiarEstado(state.pedidoActual.id, 'Pagar');
                await leavePedidoGroup();
                state.pedidoActual = null;
                state.lineas = [];
                state.cuentas = [];
                state.mesaId = null;
                toast.show('Pedido pagado con tarjeta.', 'success');
                state.pantalla = 'mesa';
                renderPantallaMesa();
            } catch (e) { toast.show('Error: ' + e.message, 'error'); }
        },

        cambiarModoSplit(modo) {
            state.modoSplit = modo;
            if (modo === 'items') {
                state.splitAsignaciones = { 1: [], 2: [] };
                state.splitCuentaActiva = 1;
            }
            renderPantallaPago();
        },

        seleccionarCuentaSplit(num) {
            state.splitCuentaActiva = num;
            renderPantallaPago();
        },

        agregarCuentaSplit() {
            const nums = Object.keys(state.splitAsignaciones).map(Number);
            const nuevo = nums.length > 0 ? Math.max(...nums) + 1 : 1;
            state.splitAsignaciones[nuevo] = [];
            state.splitCuentaActiva = nuevo;
            renderPantallaPago();
        },

        asignarItemACuenta(detalleId) {
            const linea = state.lineas.find(l => l.id === detalleId);
            if (!linea) return;

            const asignadoTotal = Object.values(state.splitAsignaciones)
                .flat()
                .filter(it => it.detalleId === detalleId)
                .reduce((s, it) => s + it.cantidad, 0);

            if (asignadoTotal >= linea.cantidad) return;

            const cuentaNum = state.splitCuentaActiva;
            const existente = state.splitAsignaciones[cuentaNum]?.find(it => it.detalleId === detalleId);
            if (existente) {
                existente.cantidad++;
            } else {
                state.splitAsignaciones[cuentaNum] = state.splitAsignaciones[cuentaNum] || [];
                state.splitAsignaciones[cuentaNum].push({
                    detalleId,
                    cantidad: 1,
                    productoNombre: linea.productoNombre,
                    precioUnitario: linea.precioUnitario
                });
            }
            renderPantallaPago();
        },

        quitarItemDeCuenta(cuentaNum, itemIdx) {
            const items = state.splitAsignaciones[cuentaNum];
            if (!items || itemIdx < 0 || itemIdx >= items.length) return;
            items.splice(itemIdx, 1);
            renderPantallaPago();
        },

        async confirmarSplitItems() {
            if (!state.pedidoActual) return;
            const asignaciones = Object.entries(state.splitAsignaciones)
                .filter(([_, items]) => items.length > 0)
                .map(([num, items]) => ({
                    cuentaNumero: parseInt(num),
                    items: items.map(it => ({ detalleId: it.detalleId, cantidad: it.cantidad }))
                }));

            if (asignaciones.length < 2) {
                toast.show('Asigne items al menos a 2 cuentas.', 'error');
                return;
            }

            try {
                const cuentas = await api.crearCuentasConItems(state.pedidoActual.id, asignaciones);
                state.cuentas = cuentas.map(c => ({
                    ...c,
                    metodoPagoSeleccionado: null,
                    propinaMonto: 0,
                    propinaPorcentaje: null
                }));
                state.splitAsignaciones = {};
                await joinPedidoGroup(state.pedidoActual.id);
                renderPantallaPago();
            } catch (e) { toast.show('Error al crear cuentas: ' + e.message, 'error'); }
        },

        async crearCuentas(cantidad) {
            if (!state.pedidoActual) return;
            try {
                const cuentas = await api.crearCuentas(state.pedidoActual.id, cantidad);
                state.cuentas = cuentas.map(c => ({
                    ...c,
                    metodoPagoSeleccionado: null,
                    propinaMonto: 0,
                    propinaPorcentaje: null
                }));
                await joinPedidoGroup(state.pedidoActual.id);
                renderPantallaPago();
            } catch (e) { toast.show('Error al crear cuentas: ' + e.message, 'error'); }
        },

        seleccionarMetodo(cuentaId, metodo) {
            const cuenta = state.cuentas.find(c => c.id === cuentaId);
            if (!cuenta || cuenta.estado === 'Pagada') return;
            cuenta.metodoPagoSeleccionado = metodo;
            renderPantallaPago();
        },

        seleccionarPropina(cuentaId, porcentaje) {
            const cuenta = state.cuentas.find(c => c.id === cuentaId);
            if (!cuenta || cuenta.estado === 'Pagada') return;
            cuenta.propinaPorcentaje = porcentaje;
            cuenta.propinaMonto = Math.round(cuenta.total * (porcentaje / 100) * 100) / 100;
            renderPantallaPago();
        },

        propinaCustom(cuentaId, valor) {
            const cuenta = state.cuentas.find(c => c.id === cuentaId);
            if (!cuenta || cuenta.estado === 'Pagada') return;
            const monto = parseFloat(valor) || 0;
            cuenta.propinaPorcentaje = null;
            cuenta.propinaMonto = monto;
            renderPantallaPago();
        },

        async pagarCuenta(cuentaId) {
            const cuenta = state.cuentas.find(c => c.id === cuentaId);
            if (!cuenta || !cuenta.metodoPagoSeleccionado || cuenta.estado === 'Pagada') return;
            try {
                const result = await api.pagarCuenta(cuentaId, cuenta.metodoPagoSeleccionado, cuenta.propinaMonto || 0);
                cuenta.estado = 'Pagada';
                cuenta.metodoPago = cuenta.metodoPagoSeleccionado;
                cuenta.fechaPago = result.fechaPago;
                renderPantallaPago();

                const todasPagadas = state.cuentas.every(c => c.estado === 'Pagada');
                if (todasPagadas) {
                    setTimeout(() => {
                        leavePedidoGroup();
                        state.pedidoActual = null;
                        state.lineas = [];
                        state.cuentas = [];
                        state.mesaId = null;
                        state.pantalla = 'mesa';
                        renderPantallaMesa();
                    }, 3000);
                }
            } catch (e) { toast.show('Error al pagar cuenta: ' + e.message, 'error'); }
        },

        async cancelarPedido() {
            if (!state.pedidoActual || !await modalConfirm('¿Cancelar este pedido?')) return;
            try {
                await api.cambiarEstado(state.pedidoActual.id, 'Cancelar');
                await leavePedidoGroup();
                state.pedidoActual = null;
                state.lineas = [];
                state.cuentas = [];
                state.mesaId = null;
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

        nuevoPedido() {
            leavePedidoGroup();
            state.pedidoActual = null;
            state.lineas = [];
            state.cuentas = [];
            state.mesaId = null;
            state.pantalla = 'mesa';
            renderPantallaMesa();
        }
    };

    // ── Inicialización ──────────────────────────────────────
    window.__lmdMesasDisponibles = window.__lmdMesasDisponibles || [];
    window.__lmdProductosDisponibles = window.__lmdProductosDisponibles || [];

    document.addEventListener('DOMContentLoaded', () => {
        renderPantallaMesa();
        initSignalR();
    });

    window.addEventListener('beforeunload', () => {
        if (connection && state.pedidoActual) {
            connection.invoke('SalirDePedido', state.pedidoActual.id).catch(() => {});
        }
    });
})();
