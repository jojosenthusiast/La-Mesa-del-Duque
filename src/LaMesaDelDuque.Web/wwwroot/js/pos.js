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
                if (l.notas) form.append(`Vm.CrearPedido.Lineas[${i}].Notas`, l.notas);
            });

            const res = await fetch('?handler=CrearJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!res.ok) throw new Error((await res.text()) || 'Error al crear pedido');
            return res.json();
        },

        async agregar(pedidoId, productoId, cantidad, notas) {
            const form = new FormData();
            form.append('__RequestVerificationToken', csrfToken());
            form.append('pedidoId', pedidoId);
            form.append('productoId', productoId);
            form.append('cantidad', cantidad);
            if (notas) form.append('notas', notas);

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
        }
    };

    function csrfToken() {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    // ── Estado POS ──────────────────────────────────────────
    const state = {
        pantalla: 'mesa', // 'mesa' | 'productos' | 'pago'
        tipoServicio: 'ComerAqui',
        mesaId: null,
        pedidoActual: null,
        lineas: [], // { id, productoId, productoNombre, cantidad, precioUnitario, subtotal, notas }
    };

    function formatMoney(n) {
        return new Intl.NumberFormat('es-SV', { style: 'currency', currency: 'USD' }).format(n);
    }

    // ── Modal de notas / alérgenos ──────────────────────────
    function pedirNota(producto) {
        return new Promise((resolve) => {
            // Eliminar modal previo si existe
            const previo = document.getElementById('lmd-pos-nota-modal');
            if (previo) previo.remove();

            const modal = document.createElement('div');
            modal.id = 'lmd-pos-nota-modal';
            modal.style.cssText = `
                position: fixed; inset: 0; z-index: 9999;
                background: rgba(15,27,45,0.55);
                display: flex; align-items: center; justify-content: center;
                font-family: 'Montserrat', sans-serif;
            `;
            modal.innerHTML = `
                <div style="background:#fff;border-radius:1rem;padding:1.5rem;width:92%;max-width:420px;box-shadow:0 12px 40px rgba(0,0,0,0.25);">
                    <h3 style="margin:0 0 0.75rem;font-family:'Cinzel',serif;font-size:1.25rem;color:#0F1B2D;">
                        ${producto.nombre}
                    </h3>
                    <p style="margin:0 0 1rem;color:#6B6F76;font-size:0.875rem;">Añadir notas para cocina</p>

                    <div style="margin-bottom:1rem;">
                        <label style="display:block;font-weight:700;font-size:0.8125rem;margin-bottom:0.5rem;color:#0F1B2D;">Alérgenos</label>
                        <div style="display:flex;flex-wrap:wrap;gap:0.5rem;">
                            <label style="display:flex;align-items:center;gap:0.35rem;font-size:0.875rem;cursor:pointer;padding:0.35rem 0.6rem;border:1px solid rgba(15,27,45,0.12);border-radius:0.5rem;">
                                <input type="checkbox" value="maní" data-alergeno /> 🥜 Maní
                            </label>
                            <label style="display:flex;align-items:center;gap:0.35rem;font-size:0.875rem;cursor:pointer;padding:0.35rem 0.6rem;border:1px solid rgba(15,27,45,0.12);border-radius:0.5rem;">
                                <input type="checkbox" value="lácteos" data-alergeno /> 🥛 Lácteos
                            </label>
                            <label style="display:flex;align-items:center;gap:0.35rem;font-size:0.875rem;cursor:pointer;padding:0.35rem 0.6rem;border:1px solid rgba(15,27,45,0.12);border-radius:0.5rem;">
                                <input type="checkbox" value="gluten" data-alergeno /> 🌾 Gluten
                            </label>
                            <label style="display:flex;align-items:center;gap:0.35rem;font-size:0.875rem;cursor:pointer;padding:0.35rem 0.6rem;border:1px solid rgba(15,27,45,0.12);border-radius:0.5rem;">
                                <input type="checkbox" value="mariscos" data-alergeno /> 🦐 Mariscos
                            </label>
                        </div>
                    </div>

                    <div style="margin-bottom:1.25rem;">
                        <label style="display:block;font-weight:700;font-size:0.8125rem;margin-bottom:0.5rem;color:#0F1B2D;">Nota especial</label>
                        <textarea id="lmd-pos-nota-texto" rows="2" placeholder="Ej: Sin cebolla, término medio..."
                            style="width:100%;border:2px solid rgba(15,27,45,0.12);border-radius:0.5rem;padding:0.6rem;font-size:0.9rem;resize:vertical;box-sizing:border-box;"></textarea>
                    </div>

                    <div style="display:flex;gap:0.75rem;">
                        <button id="lmd-pos-nota-agregar" style="flex:1;padding:0.75rem;border:none;border-radius:0.5rem;background:#C9A24E;color:#0F1B2D;font-weight:700;font-size:0.95rem;cursor:pointer;">Agregar</button>
                        <button id="lmd-pos-nota-cancelar" style="flex:1;padding:0.75rem;border:1px solid rgba(15,27,45,0.12);border-radius:0.5rem;background:#fff;color:#6B6F76;font-weight:700;font-size:0.95rem;cursor:pointer;">Cancelar</button>
                    </div>
                </div>
            `;

            document.body.appendChild(modal);

            const agregar = modal.querySelector('#lmd-pos-nota-agregar');
            const cancelar = modal.querySelector('#lmd-pos-nota-cancelar');
            const textarea = modal.querySelector('#lmd-pos-nota-texto');

            const cerrar = () => { modal.remove(); };

            agregar.addEventListener('click', () => {
                const alergenos = Array.from(modal.querySelectorAll('[data-alergeno]:checked')).map(cb => cb.value).join(', ');
                const notaTexto = textarea.value.trim();
                const partes = [];
                if (alergenos) partes.push(`ALÉRGENOS: ${alergenos}`);
                if (notaTexto) partes.push(notaTexto);
                const notaFinal = partes.join(' | ');
                cerrar();
                resolve(notaFinal || null);
            });

            cancelar.addEventListener('click', () => {
                cerrar();
                resolve(null);
            });

            // Cerrar al hacer click fuera
            modal.addEventListener('click', (e) => {
                if (e.target === modal) {
                    cerrar();
                    resolve(null);
                }
            });

            textarea.focus();
        });
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
                    ${l.notas ? `<small style="color:#C75A3C;font-weight:600;">${l.notas}</small>` : ''}
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

        container.innerHTML = `
            <div class="lmd-pos-pantalla" id="pantalla-pago">
                <button class="lmd-pos-btn-atras" onclick="pos.irAPantalla('productos')">← Volver a productos</button>
                <h2 class="lmd-pos-titulo">Pago</h2>
                <div class="lmd-pos-pago-total">${formatMoney(total)}</div>

                <div class="lmd-pos-pago-efectivo">
                    <label class="lmd-pos-pago-label">Efectivo recibido</label>
                    <div class="lmd-pos-pago-input-group">
                        <span class="lmd-pos-pago-input-prefijo">$</span>
                        <input type="number" id="efectivo-input" class="lmd-pos-pago-input" step="0.01" min="0"
                               placeholder="${total.toFixed(2)}" oninput="pos.calcularCambio()" autofocus />
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

        filtrarCategoria(cat) {
            document.querySelectorAll('.lmd-pos-cat-tab').forEach(t => t.classList.remove('lmd-pos-cat-tab--activo'));
            const tab = Array.from(document.querySelectorAll('.lmd-pos-cat-tab')).find(t => t.textContent.trim() === cat);
            if (tab) tab.classList.add('lmd-pos-cat-tab--activo');

            document.querySelectorAll('.lmd-pos-producto-card').forEach(c => {
                c.style.display = cat && c.dataset.categoria !== cat ? 'none' : '';
            });
        },

        async agregarProducto(productoId) {
            const prod = window.__lmdProductosDisponibles.find(p => p.id === productoId);
            if (!prod) return;

            // Pedir nota antes de agregar
            const notas = await pedirNota(prod);
            if (notas === null) return; // usuario canceló

            // Si no hay pedido activo, crear
            if (!state.pedidoActual) {
                try {
                    const result = await api.crear(state.tipoServicio, state.mesaId, [{
                        productoId,
                        cantidad: 1,
                        precioUnitario: prod.precio,
                        notas
                    }]);
                    state.pedidoActual = { id: result.pedidoId, estado: result.estado };
                    state.lineas = result.lineas || [{
                        id: result.lineaId || crypto.randomUUID(),
                        productoId,
                        productoNombre: prod.nombre,
                        cantidad: 1,
                        precioUnitario: prod.precio,
                        subtotal: prod.precio,
                        notas
                    }];
                } catch (e) {
                    alert('Error al crear pedido: ' + e.message);
                    return;
                }
            } else {
                try {
                    await api.agregar(state.pedidoActual.id, productoId, 1, notas);
                    const existente = state.lineas.find(l => l.productoId === productoId && l.notas === notas);
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
                            subtotal: prod.precio,
                            notas
                        });
                    }
                } catch (e) {
                    alert('Error: ' + e.message);
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
            } catch (e) { alert('Error: ' + e.message); }
        },

        async eliminarLinea(lineaId) {
            if (!state.pedidoActual || !confirm('¿Quitar este producto?')) return;
            try {
                await api.eliminar(state.pedidoActual.id, lineaId);
                state.lineas = state.lineas.filter(l => l.id !== lineaId);
                if (state.lineas.length === 0) {
                    // Pedido vacío: cancelar implícito
                    state.pedidoActual = null;
                }
                renderPantallaProductos();
            } catch (e) { alert('Error: ' + e.message); }
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
                state.pedidoActual = null;
                state.lineas = [];
                state.mesaId = null;
                alert(`✅ ${result.mensaje || 'Pedido pagado.'}`);
                state.pantalla = 'mesa';
                renderPantallaMesa();
            } catch (e) { alert('Error: ' + e.message); }
        },

        async pagarConTarjeta() {
            if (!state.pedidoActual) return;
            try {
                await api.cambiarEstado(state.pedidoActual.id, 'Pagar');
                state.pedidoActual = null;
                state.lineas = [];
                state.mesaId = null;
                alert('✅ Pedido pagado con tarjeta.');
                state.pantalla = 'mesa';
                renderPantallaMesa();
            } catch (e) { alert('Error: ' + e.message); }
        },

        async cancelarPedido() {
            if (!state.pedidoActual || !confirm('¿Cancelar este pedido?')) return;
            try {
                await api.cambiarEstado(state.pedidoActual.id, 'Cancelar');
                state.pedidoActual = null;
                state.lineas = [];
                state.mesaId = null;
                state.pantalla = 'mesa';
                renderPantallaMesa();
            } catch (e) { alert('Error: ' + e.message); }
        },

        async marcarEnPreparacion() {
            if (!state.pedidoActual) return;
            try {
                await api.cambiarEstado(state.pedidoActual.id, 'MarcarEnPreparacion');
                alert('✅ Pedido marcado en preparación.');
            } catch (e) { alert('Error: ' + e.message); }
        }
    };

    // ── Inicialización ──────────────────────────────────────
    window.__lmdMesasDisponibles = window.__lmdMesasDisponibles || [];
    window.__lmdProductosDisponibles = window.__lmdProductosDisponibles || [];

    document.addEventListener('DOMContentLoaded', () => {
        renderPantallaMesa();
    });
})();
