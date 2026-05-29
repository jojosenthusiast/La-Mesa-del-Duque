/* ============================================================================
   La Mesa del Duque — Tableside Ordering (tablet-optimized)
   Pantalla táctil grande, sin sidebar, envío incremental a cocina.
   ============================================================================ */

(function () {
    function mostrarToast(mensaje, tipo = 'error') {
        if (window.lmdToast) {
            window.lmdToast(mensaje, tipo);
        } else {
            alert(tipo + ': ' + mensaje);
        }
    }
    const api = {
        async crear(tipoServicio, mesaId, lineas) {
            const form = new FormData();
            form.append('__RequestVerificationToken', csrfToken());
            form.append('Vm.CrearPedido.TipoServicio', tipoServicio);
            if (mesaId) form.append('Vm.CrearPedido.MesaId', mesaId);
            lineas.forEach((l, i) => {
                form.append(`Vm.CrearPedido.Lineas[${i}].ProductoId`, l.productoId);
                form.append(`Vm.CrearPedido.Lineas[${i}].Cantidad`, l.cantidad);
                form.append(`Vm.CrearPedido.Lineas[${i}].PrecioUnitario`, (l.precioUnitario || 0).toString());
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

        async enviarACocina(pedidoId) {
            const form = new FormData();
            form.append('__RequestVerificationToken', csrfToken());
            form.append('pedidoId', pedidoId);

            const res = await fetch('?handler=EnviarACocinaJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!res.ok) throw new Error((await res.text()) || 'Error al enviar');
            return res.json();
        }
    };

    function csrfToken() {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    // ── Estado ──────────────────────────────────────────────
    const state = {
        pantalla: 'mesa', // 'mesa' | 'productos'
        mesaId: null,
        pedidoActual: null,
        lineas: []
    };

    function formatMoney(n) {
        return new Intl.NumberFormat('es-SV', { style: 'currency', currency: 'USD' }).format(n);
    }

    function persistState() {
        localStorage.setItem('lmdd_pos_state', JSON.stringify({
            pedidoActual: state.pedidoActual,
            lineas: state.lineas,
            cuentas: [],
            tipoServicio: 'ComerAqui',
            mesaId: state.mesaId
        }));
    }

    // ── SignalR ─────────────────────────────────────────────
    let connection = null;

    function initSignalR() {
        if (typeof signalR === 'undefined') return;
        connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/pedidos')
            .withAutomaticReconnect()
            .build();

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
        } catch (e) {
            setTimeout(startConnection, 5000);
        }
    }

    async function joinPedidoGroup(pedidoId) {
        if (!connection) return;
        try { await connection.invoke('UnirseAPedido', pedidoId); } catch (e) { /* ignore */ }
    }

    // ── Render ──────────────────────────────────────────────
    function renderPantallaMesa() {
        const grid = document.getElementById('tableside-grid');
        const tabs = document.getElementById('tableside-cat-tabs');
        if (!grid || !tabs) return;

        tabs.innerHTML = '';
        tabs.style.display = 'none';

        const mesasHtml = window.__lmdMesasDisponibles
            .map(m => `
                <button class="lmd-mesa-card ${state.mesaId === m.id ? 'lmd-mesa-card--selected' : ''}"
                        onclick="tableside.seleccionarMesa('${m.id}')" style="min-height:120px;">
                    <span class="lmd-mesa-card__numero">${m.numero}</span>
                    <span class="lmd-mesa-card__capacidad">${m.capacidad}p</span>
                </button>`)
            .join('');

        grid.innerHTML = `
            <div style="grid-column:1/-1;text-align:center;padding:2rem 0;">
                <h2 style="font-family:'Cinzel',serif;margin-bottom:1.5rem;color:var(--lmd-azul-duque);">Seleccionar mesa</h2>
                <div class="lmd-pos-mesas-grid" style="max-width:600px;margin:0 auto;">
                    ${mesasHtml}
                </div>
                ${state.mesaId ? `
                <div style="margin-top:2rem;">
                    <button class="lmd-pos-btn-primario" onclick="tableside.irAProductos()" style="font-size:1.25rem;padding:1.25rem 3rem;">
                        Continuar → Productos
                    </button>
                </div>` : ''}
            </div>`;

        actualizarBottomBar();
    }

    function renderPantallaProductos() {
        const grid = document.getElementById('tableside-grid');
        const tabs = document.getElementById('tableside-cat-tabs');
        if (!grid || !tabs) return;

        tabs.style.display = 'flex';

        const categorias = {};
        window.__lmdProductosDisponibles.forEach(p => {
            if (!categorias[p.categoriaNombre]) categorias[p.categoriaNombre] = [];
            categorias[p.categoriaNombre].push(p);
        });

        const cats = Object.keys(categorias);
        const catTabs = cats.map((cat, i) =>
            `<button class="lmd-tableside-cat-tab ${i === 0 ? 'lmd-tableside-cat-tab--activo' : ''}" onclick="tableside.filtrarCategoria('${cat.replace(/'/g, "\\'")}')">${cat}</button>`
        ).join('');
        tabs.innerHTML = catTabs;

        const allCards = Object.entries(categorias).map(([cat, prods]) =>
            prods.map(p => `
                <button class="lmd-tableside-producto" data-categoria="${cat.replace(/'/g, "\\'")}"
                        onclick="tableside.agregarProducto('${p.id}')">
                    <div class="lmd-tableside-producto__nombre">${p.nombre}</div>
                    <div class="lmd-tableside-producto__precio">${formatMoney(p.precio)}</div>
                    <div class="lmd-tableside-producto__tiempo">${p.tiempoPreparacionMin} min</div>
                </button>`).join('')
        ).join('');

        grid.innerHTML = allCards;
        actualizarBottomBar();
    }

    function actualizarBottomBar() {
        const info = document.getElementById('tableside-pedido-info');
        const btn = document.getElementById('tableside-btn-enviar');
        const mesaInfo = document.getElementById('tableside-mesa-info');
        if (!info || !btn) return;

        const total = state.lineas.reduce((s, l) => s + l.subtotal, 0);
        const totalItems = state.lineas.reduce((s, l) => s + l.cantidad, 0);
        info.innerHTML = `${totalItems} item${totalItems !== 1 ? 's' : ''} — <strong>${formatMoney(total)}</strong>`;

        const mesa = window.__lmdMesasDisponibles.find(m => m.id === state.mesaId);
        if (mesaInfo) {
            mesaInfo.textContent = state.pantalla === 'mesa'
                ? 'Seleccione mesa'
                : (mesa ? `Mesa ${mesa.numero}` : 'Para llevar');
        }

        btn.disabled = state.lineas.length === 0;
        btn.textContent = state.pedidoActual && state.pedidoActual.enviado
            ? 'Enviar más a Cocina'
            : 'Enviar a Cocina';
    }

    // ── API pública ─────────────────────────────────────────
    window.tableside = {
        seleccionarMesa(id) {
            state.mesaId = state.mesaId === id ? null : id;
            renderPantallaMesa();
        },

        irAProductos() {
            if (!state.mesaId) return;
            state.pantalla = 'productos';
            renderPantallaProductos();
        },

        irAMesas() {
            state.pantalla = 'mesa';
            renderPantallaMesa();
        },

        filtrarCategoria(cat) {
            document.querySelectorAll('.lmd-tableside-cat-tab').forEach(t => t.classList.remove('lmd-tableside-cat-tab--activo'));
            const tab = Array.from(document.querySelectorAll('.lmd-tableside-cat-tab')).find(t => t.textContent.trim() === cat);
            if (tab) tab.classList.add('lmd-tableside-cat-tab--activo');

            document.querySelectorAll('.lmd-tableside-producto').forEach(c => {
                c.style.display = cat && c.dataset.categoria !== cat ? 'none' : '';
            });
        },

        async agregarProducto(productoId) {
            const prod = window.__lmdProductosDisponibles.find(p => p.id === productoId);
            if (!prod) return;

            if (!state.pedidoActual) {
                try {
                    const result = await api.crear('ComerAqui', state.mesaId, [{
                        productoId,
                        cantidad: 1,
                        precioUnitario: prod.precio
                    }]);
                    state.pedidoActual = { id: result.pedidoId, estado: result.estado, enviado: false };
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
                    mostrarToast('Error al crear pedido: ' + e.message, 'error');
                    return;
                }
            } else {
                try {
                    await api.agregar(state.pedidoActual.id, productoId, 1);
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
                    mostrarToast('Error: ' + e.message, 'error');
                    return;
                }
            }

            actualizarBottomBar();

            // Feedback visual táctil
            const btn = document.activeElement;
            if (btn && btn.classList.contains('lmd-tableside-producto')) {
                btn.style.transform = 'scale(0.95)';
                setTimeout(() => btn.style.transform = '', 120);
            }
        },

        async enviarACocina() {
            if (!state.pedidoActual || state.lineas.length === 0) return;
            try {
                await api.enviarACocina(state.pedidoActual.id);
                state.pedidoActual.enviado = true;
                state.pedidoActual.estado = 'EnPreparacion';
                actualizarBottomBar();
                persistState();

                // Opcional: feedback visual
                const btn = document.getElementById('tableside-btn-enviar');
                if (btn) {
                    const original = btn.innerHTML;
                    btn.innerHTML = '<svg class="lmd-icon" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><use href="/lib/lucide-static/icons/check.svg#icon"/></svg> Enviado';
                    setTimeout(() => btn.innerHTML = original, 2000);
                }
            } catch (e) {
                mostrarToast('Error: ' + e.message, 'error');
            }
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
