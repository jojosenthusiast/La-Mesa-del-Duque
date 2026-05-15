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
                if (l.modificacionesJson) form.append(`Vm.CrearPedido.Lineas[${i}].ModificacionesJson`, l.modificacionesJson);
            });

            const res = await fetch('?handler=CrearJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!res.ok) throw new Error((await res.text()) || 'Error al crear pedido');
            return res.json();
        },

        async agregar(pedidoId, productoId, cantidad, notas, modificacionesJson) {
            const form = new FormData();
            form.append('__RequestVerificationToken', csrfToken());
            form.append('pedidoId', pedidoId);
            form.append('productoId', productoId);
            form.append('cantidad', cantidad);
            if (notas) form.append('notas', notas);
            if (modificacionesJson) form.append('modificacionesJson', modificacionesJson);

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

    // ── Offline Queue (IndexedDB) ───────────────────────────
    const DB_NAME = 'LMDD-Offline';
    const DB_VERSION = 2;
    const STORE_PEDIDOS = 'pedidosPendientes';
    const STORE_PAGOS = 'pagosPendientes';

    function openDB() {
        return new Promise((resolve, reject) => {
            const req = indexedDB.open(DB_NAME, DB_VERSION);
            req.onupgradeneeded = (e) => {
                const db = req.result;
                if (!db.objectStoreNames.contains(STORE_PEDIDOS)) {
                    db.createObjectStore(STORE_PEDIDOS, { keyPath: 'id' });
                }
                if (!db.objectStoreNames.contains(STORE_PAGOS)) {
                    db.createObjectStore(STORE_PAGOS, { keyPath: 'id' });
                }
                // Migration from v1 to v2: ensure pagos store exists
                if (e.oldVersion < 2 && !db.objectStoreNames.contains(STORE_PAGOS)) {
                    db.createObjectStore(STORE_PAGOS, { keyPath: 'id' });
                }
            };
            req.onsuccess = () => resolve(req.result);
            req.onerror = () => reject(req.error);
        });
    }

    async function queuePayment(datos) {
        const db = await openDB();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_PAGOS, 'readwrite');
            const store = tx.objectStore(STORE_PAGOS);
            const item = {
                id: crypto.randomUUID(),
                timestamp: Date.now(),
                ...datos
            };
            const req = store.add(item);
            req.onsuccess = () => resolve(item.id);
            req.onerror = () => reject(req.error);
        });
    }

    async function obtenerPagosPendientes() {
        const db = await openDB();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_PAGOS, 'readonly');
            const store = tx.objectStore(STORE_PAGOS);
            const req = store.getAll();
            req.onsuccess = () => resolve(req.result || []);
            req.onerror = () => reject(req.error);
        });
    }

    async function eliminarPagoPendiente(id) {
        const db = await openDB();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_PAGOS, 'readwrite');
            const store = tx.objectStore(STORE_PAGOS);
            const req = store.delete(id);
            req.onsuccess = () => resolve();
            req.onerror = () => reject(req.error);
        });
    }

    // Offline payment queue — FIX 5
    async function sincronizarPagosPendientes() {
        const pendientes = await obtenerPagosPendientes();
        if (pendientes.length === 0) return;

        let sincronizados = 0;
        for (const pago of pendientes.sort((a, b) => a.timestamp - b.timestamp)) {
            try {
                const form = new FormData();
                form.append('__RequestVerificationToken', csrfToken());
                form.append('pedidoId', pago.pedidoId);
                form.append('efectivoRecibido', pago.amount);

                const res = await fetch('?handler=PagarEfectivoJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
                if (!res.ok) throw new Error(await res.text());
                await eliminarPagoPendiente(pago.id);
                sincronizados++;
            } catch (e) {
                console.error('Sync failed for pago:', pago.id, e);
                break;
            }
        }

        if (sincronizados > 0) {
            mostrarToastOffline('Sincronizados ' + sincronizados + ' pago' + (sincronizados === 1 ? '' : 's') + ' pendientes.');
        }
    }

    async function guardarPedidoPendiente(datos) {
        const db = await openDB();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_PEDIDOS, 'readwrite');
            const store = tx.objectStore(STORE_PEDIDOS);
            const item = {
                id: crypto.randomUUID(),
                timestamp: Date.now(),
                tipoServicio: datos.tipoServicio,
                mesaId: datos.mesaId,
                lineas: datos.lineas,
                syncAttempts: 0
            };
            const req = store.add(item);
            req.onsuccess = () => resolve(item.id);
            req.onerror = () => reject(req.error);
        });
    }

    async function obtenerPedidosPendientes() {
        const db = await openDB();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_PEDIDOS, 'readonly');
            const store = tx.objectStore(STORE_PEDIDOS);
            const req = store.getAll();
            req.onsuccess = () => resolve(req.result || []);
            req.onerror = () => reject(req.error);
        });
    }

    async function eliminarPedidoPendiente(id) {
        const db = await openDB();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_PEDIDOS, 'readwrite');
            const store = tx.objectStore(STORE_PEDIDOS);
            const req = store.delete(id);
            req.onsuccess = () => resolve();
            req.onerror = () => reject(req.error);
        });
    }

    async function contarPedidosPendientes() {
        const pendientes = await obtenerPedidosPendientes();
        return pendientes.length;
    }

    // ── Offline UI helpers ──────────────────────────────────
    function actualizarOfflineUI() {
        const banner = document.getElementById('lmd-offline-banner');
        const badge = document.getElementById('lmd-offline-pendientes');
        const texto = document.getElementById('lmd-offline-texto');
        if (!banner) return;

        if (navigator.onLine) {
            banner.classList.remove('visible');
            banner.style.display = 'none';
        } else {
            banner.style.display = 'flex';
            banner.classList.add('visible');
        }

        if (badge) {
            contarPedidosPendientes().then(count => {
                if (count > 0) {
                    badge.style.display = 'inline-flex';
                    badge.textContent = count;
                    if (texto) texto.textContent = 'Modo offline — ' + count + ' pedido' + (count === 1 ? ' pendiente' : 's pendientes');
                } else {
                    badge.style.display = 'none';
                    if (texto) texto.textContent = 'Modo offline';
                }
            });
        }
    }

    function mostrarToastOffline(mensaje) {
        // Simple toast fallback; reuse site toast if available
        const zone = document.getElementById('lmd-toast-zone') || document.querySelector('.lmd-toast-zone');
        if (zone) {
            const toast = document.createElement('div');
            toast.className = 'alert alert-warning alert-dismissible fade show py-2 mb-2';
            toast.setAttribute('data-lmd-toast', 'true');
            toast.innerHTML = mensaje + '<button type="button" class="btn-close" aria-label="Cerrar"></button>';
            zone.prepend(toast);
            setTimeout(() => {
                toast.classList.remove('show');
                setTimeout(() => toast.remove(), 150);
            }, 4000);
        }
    }

    // ── Sync loop ───────────────────────────────────────────
    async function sincronizarPendientes() {
        const pendientes = await obtenerPedidosPendientes();
        if (pendientes.length === 0) return;

        let sincronizados = 0;
        for (const pedido of pendientes.sort((a, b) => a.timestamp - b.timestamp)) {
            try {
                await api.crear(pedido.tipoServicio, pedido.mesaId, pedido.lineas);
                await eliminarPedidoPendiente(pedido.id);
                sincronizados++;
            } catch (e) {
                console.error('Sync failed for pedido:', pedido.id, e);
                break; // Stop on first failure, retry later
            }
        }

        if (sincronizados > 0) {
            mostrarToastOffline('Sincronizados ' + sincronizados + ' pedido' + (sincronizados === 1 ? '' : 's') + ' pendientes.');
            actualizarOfflineUI();
        }
    }

    window.addEventListener('online', () => {
        actualizarOfflineUI();
        mostrarToastOffline('Conexión restaurada. Sincronizando...');
        sincronizarPendientes();
        sincronizarPagosPendientes();
    });

    window.addEventListener('offline', () => {
        actualizarOfflineUI();
        mostrarToastOffline('Sin conexión. Los pedidos se guardan localmente.');
    });

    // ── SignalR (POS) ───────────────────────────────────────
    let posConnection = null;
    async function iniciarSignalRPOS() {
        if (!window.signalR) return;
        posConnection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/pedidos')
            .withAutomaticReconnect()
            .build();

        posConnection.on('ProductoAgotado', (productoId) => {
            const card = document.querySelector(`.lmd-pos-producto-card[data-id="${productoId}"]`);
            if (card) {
                card.classList.add('lmd-pos-producto-card--agotado');
                card.disabled = true;
            }
        });

        posConnection.on('ProductoReactivado', (productoId) => {
            const card = document.querySelector(`.lmd-pos-producto-card[data-id="${productoId}"]`);
            if (card) {
                card.classList.remove('lmd-pos-producto-card--agotado');
                card.disabled = false;
            }
        });

        try {
            await posConnection.start();
        } catch (e) {
            console.warn('SignalR POS start failed', e);
        }
    }

    // ── Estado POS ──────────────────────────────────────────
    const state = {
        pantalla: 'mesa', // 'mesa' | 'productos' | 'pago'
        tipoServicio: 'ComerAqui',
        mesaId: null,
        pedidoActual: null,
        lineas: [], // { id, productoId, productoNombre, cantidad, precioUnitario, subtotal, notas, modificacionesJson }
    };

    // ── Estado modificadores ────────────────────────────────
    const modificadores = {
        productoId: null,
        productoNombre: '',
        ingredientes: [], // { id, nombre, cantidadRequerida, unidadMedida, quitado, motivo, ingredienteReemplazo }
        alergias: [],     // ['mani', 'lacteos']
        extras: [],       // ['queso extra', 'tocino']
        notaCustom: '',
        curso: 'PlatoFuerte'
    };

    function formatMoney(n) {
        return new Intl.NumberFormat('es-SV', { style: 'currency', currency: 'USD' }).format(n);
    }

    // ── Modal de notas / alérgenos (rápido) ─────────────────
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

    // ── Modal de modificadores de ingredientes ──────────────
    function renderModificadorModal() {
        const overlay = document.getElementById('lmd-modificador-overlay');
        if (!overlay) return;

        const cursoHtml = `
            <div class="lmd-modificador-curso">
                <span>Curso:</span>
                <button class="${modificadores.curso === 'Entrada' ? 'activo' : ''}" onclick="pos.setCurso('Entrada')">🥗 Entrada</button>
                <button class="${modificadores.curso === 'PlatoFuerte' ? 'activo' : ''}" onclick="pos.setCurso('PlatoFuerte')">🍖 Plato fuerte</button>
                <button class="${modificadores.curso === 'Postre' ? 'activo' : ''}" onclick="pos.setCurso('Postre')">🍰 Postre</button>
            </div>
        `;

        const ingredientesHtml = modificadores.ingredientes.map(ing => `
            <div class="lmd-modificador-item" id="mod-ing-${ing.id}">
                <span class="lmd-modificador-item-nombre">
                    ${ing.cantidadRequerida}${ing.unidadMedida} ${ing.nombre}
                </span>
                <div class="lmd-modificador-item-controles">
                    <button class="lmd-modificador-item-quitar ${ing.quitado ? 'activo' : ''}"
                            onclick="pos.toggleQuitar('${ing.id}')">
                        ${ing.quitado ? '✓ Quitado' : 'Quitar'}
                    </button>
                    ${ing.quitado ? `
                        <select class="lmd-modificador-item-motivo" onchange="pos.setMotivo('${ing.id}', this.value)">
                            <option value="">Motivo...</option>
                            <option value="alergia" ${ing.motivo === 'alergia' ? 'selected' : ''}>🚫 Alergia</option>
                            <option value="preferencia" ${ing.motivo === 'preferencia' ? 'selected' : ''}>👤 Preferencia</option>
                            <option value="intercambio" ${ing.motivo === 'intercambio' ? 'selected' : ''}>🔄 Intercambio</option>
                        </select>
                    ` : ''}
                </div>
            </div>
        `).join('');

        const extrasHtml = modificadores.extras.map((extra, idx) => `
            <span class="lmd-modificador-extra-tag">${extra}
                <button onclick="pos.quitarExtra(${idx})">✕</button>
            </span>
        `).join('');

        overlay.innerHTML = `
            <div class="lmd-modificador-modal">
                <div class="lmd-modificador-header">
                    <h3>${modificadores.productoNombre}</h3>
                    <button class="lmd-modificador-cerrar" onclick="pos.cerrarModificadores()">✕</button>
                </div>

                ${cursoHtml}

                <div class="lmd-modificador-alergias">
                    <h4>⚠ Alergias rápidas</h4>
                    <button class="lmd-modificador-alergia-btn ${modificadores.alergias.includes('mani') ? 'activo' : ''}"
                            onclick="pos.toggleAlergia('mani')">🥜 Maní</button>
                    <button class="lmd-modificador-alergia-btn ${modificadores.alergias.includes('lacteos') ? 'activo' : ''}"
                            onclick="pos.toggleAlergia('lacteos')">🥛 Lácteos</button>
                    <button class="lmd-modificador-alergia-btn ${modificadores.alergias.includes('gluten') ? 'activo' : ''}"
                            onclick="pos.toggleAlergia('gluten')">🌾 Gluten</button>
                    <button class="lmd-modificador-alergia-btn ${modificadores.alergias.includes('mariscos') ? 'activo' : ''}"
                            onclick="pos.toggleAlergia('mariscos')">🦐 Mariscos</button>
                </div>

                <div class="lmd-modificador-ingredientes">
                    <h4>Ingredientes</h4>
                    ${ingredientesHtml || '<p class="lmd-modificador-vacio">Este producto no tiene ingredientes configurados.</p>'}
                </div>

                <div class="lmd-modificador-extras">
                    <h4>➕ Extras</h4>
                    <div class="lmd-modificador-extra-btns">
                        <button class="lmd-modificador-extra-btn" onclick="pos.agregarExtra('Queso extra')">Queso extra</button>
                        <button class="lmd-modificador-extra-btn" onclick="pos.agregarExtra('Tocino')">Tocino</button>
                        <button class="lmd-modificador-extra-btn" onclick="pos.agregarExtra('Aguacate')">Aguacate</button>
                    </div>
                    <div class="lmd-modificador-extra-lista">${extrasHtml}</div>
                    <button class="lmd-modificador-nota-btn" onclick="pos.agregarNotaPersonalizada()">📝 Nota personalizada...</button>
                    ${modificadores.notaCustom ? `<p class="lmd-modificador-nota-texto">${modificadores.notaCustom}</p>` : ''}
                </div>

                <button class="lmd-modificador-confirmar" onclick="pos.confirmarModificadores()">
                    Confirmar y agregar al pedido
                </button>
            </div>
        `;
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
                <div class="lmd-pos-producto-card-wrapper" data-categoria="${cat.replace(/'/g, "\\'")}">
                    <button class="lmd-pos-producto-card" data-id="${p.id}"
                            onclick="pos.agregarProducto('${p.id}')">
                        <div class="lmd-pos-producto-card__nombre">${p.nombre}</div>
                        <div class="lmd-pos-producto-card__precio">${formatMoney(p.precio)}</div>
                        <div class="lmd-pos-producto-card__tiempo">${p.tiempoPreparacionMin}min</div>
                    </button>
                    <button class="lmd-pos-producto-card__editar" onclick="event.stopPropagation(); pos.abrirModificadores('${p.id}')"
                            title="Modificar ingredientes">
                        ✏️ Modificar
                    </button>
                </div>`).join('')
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

            document.querySelectorAll('.lmd-pos-producto-card-wrapper').forEach(c => {
                c.style.display = cat && c.dataset.categoria !== cat ? 'none' : '';
            });
        },

        async agregarProducto(productoId, modificaciones, notas) {
            const prod = window.__lmdProductosDisponibles.find(p => p.id === productoId);
            if (!prod) return;

            let notasFinal = notas;
            let modificacionesJson = null;

            if (modificaciones && modificaciones.length > 0) {
                modificacionesJson = JSON.stringify(modificaciones);
            }

            // Si no vienen modificaciones pre-armadas, pedir nota rápida
            if (!modificaciones && notas === undefined) {
                notasFinal = await pedirNota(prod);
                if (notasFinal === null) return; // usuario canceló
            }

            const nuevaLinea = {
                productoId,
                cantidad: 1,
                precioUnitario: prod.precio,
                notas: notasFinal,
                modificacionesJson
            };

            const agregarLineaLocal = () => {
                const existente = state.lineas.find(l => l.productoId === productoId && l.notas === notasFinal && l.modificacionesJson === modificacionesJson);
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
                        notas: notasFinal,
                        modificacionesJson
                    });
                }
            };

            // Offline: no hay pedido activo → crear local y queue
            if (!state.pedidoActual) {
                if (!navigator.onLine) {
                    state.pedidoActual = { id: 'local-' + crypto.randomUUID(), estado: 'Pendiente', isLocal: true };
                    agregarLineaLocal();
                    await guardarPedidoPendiente({ tipoServicio: state.tipoServicio, mesaId: state.mesaId, lineas: [...state.lineas] });
                    actualizarOfflineUI();
                    mostrarToastOffline('Pedido guardado localmente. Se sincronizará al restaurar conexión.');
                    renderPantallaProductos();
                    return;
                }
                try {
                    const result = await api.crear(state.tipoServicio, state.mesaId, [nuevaLinea]);
                    state.pedidoActual = { id: result.pedidoId, estado: result.estado };
                    state.lineas = result.lineas || [{
                        id: result.lineaId || crypto.randomUUID(),
                        productoId,
                        productoNombre: prod.nombre,
                        cantidad: 1,
                        precioUnitario: prod.precio,
                        subtotal: prod.precio,
                        notas: notasFinal,
                        modificacionesJson
                    }];
                } catch (e) {
                    if (e.message && (e.message.includes('fetch') || e.message.includes('NetworkError') || !navigator.onLine)) {
                        state.pedidoActual = { id: 'local-' + crypto.randomUUID(), estado: 'Pendiente', isLocal: true };
                        agregarLineaLocal();
                        await guardarPedidoPendiente({ tipoServicio: state.tipoServicio, mesaId: state.mesaId, lineas: [...state.lineas] });
                        actualizarOfflineUI();
                        mostrarToastOffline('Pedido guardado localmente. Se sincronizará al restaurar conexión.');
                    } else {
                        alert('Error al crear pedido: ' + e.message);
                        return;
                    }
                }
            } else {
                // Pedido activo existente
                if (!navigator.onLine || state.pedidoActual.isLocal) {
                    agregarLineaLocal();
                    if (state.pedidoActual.isLocal) {
                        // Update the queued pedido with the new lineas
                        const pendientes = await obtenerPedidosPendientes();
                        const pendiente = pendientes.find(p => p.id === state.pedidoActual.id.replace('local-', ''));
                        if (pendiente) {
                            // Replace lineas in the queue item
                            // (Simplification: we delete old and create new to avoid partial updates)
                            await eliminarPedidoPendiente(pendiente.id);
                        }
                        await guardarPedidoPendiente({ tipoServicio: state.tipoServicio, mesaId: state.mesaId, lineas: [...state.lineas] });
                        actualizarOfflineUI();
                    }
                    renderPantallaProductos();
                    return;
                }
                try {
                    await api.agregar(state.pedidoActual.id, productoId, 1, notasFinal, modificacionesJson);
                    agregarLineaLocal();
                } catch (e) {
                    if (e.message && (e.message.includes('fetch') || e.message.includes('NetworkError') || !navigator.onLine)) {
                        agregarLineaLocal();
                        mostrarToastOffline('Producto agregado localmente. Se sincronizará al restaurar conexión.');
                    } else {
                        alert('Error: ' + e.message);
                        return;
                    }
                }
            }
            renderPantallaProductos();
        },

        async abrirModificadores(productoId) {
            const prod = window.__lmdProductosDisponibles.find(p => p.id === productoId);
            if (!prod) return;

            // Fetch ingredients from API
            const res = await fetch(`?handler=IngredientesProductoJson&productoId=${productoId}`);
            const data = await res.json();

            // Reset state
            modificadores.productoId = productoId;
            modificadores.productoNombre = prod.nombre;
            modificadores.ingredientes = (data.ingredientes || []).map(ing => ({
                ...ing,
                quitado: false,
                motivo: '',
                ingredienteReemplazo: null
            }));
            modificadores.alergias = [];
            modificadores.extras = [];
            modificadores.notaCustom = '';

            // Create overlay
            const previo = document.getElementById('lmd-modificador-overlay');
            if (previo) previo.remove();

            const overlay = document.createElement('div');
            overlay.id = 'lmd-modificador-overlay';
            overlay.className = 'lmd-modificador-overlay';
            document.body.appendChild(overlay);

            renderModificadorModal();
        },

        cerrarModificadores() {
            const overlay = document.getElementById('lmd-modificador-overlay');
            if (overlay) overlay.remove();
            modificadores.productoId = null;
            modificadores.ingredientes = [];
            modificadores.alergias = [];
            modificadores.extras = [];
            modificadores.notaCustom = '';
            modificadores.curso = 'PlatoFuerte';
        },

        setCurso(curso) {
            modificadores.curso = curso;
            renderModificadorModal();
        },

        toggleQuitar(ingredienteId) {
            const ing = modificadores.ingredientes.find(i => i.id === ingredienteId);
            if (ing) {
                ing.quitado = !ing.quitado;
                if (!ing.quitado) {
                    ing.motivo = '';
                    ing.ingredienteReemplazo = null;
                }
                renderModificadorModal();
            }
        },

        setMotivo(ingredienteId, motivo) {
            const ing = modificadores.ingredientes.find(i => i.id === ingredienteId);
            if (ing) {
                ing.motivo = motivo;
                if (motivo === 'intercambio') {
                    const reemplazo = prompt('¿Con qué ingrediente lo intercambia?');
                    ing.ingredienteReemplazo = reemplazo || '';
                } else {
                    ing.ingredienteReemplazo = null;
                }
            }
        },

        toggleAlergia(alergia) {
            const idx = modificadores.alergias.indexOf(alergia);
            if (idx >= 0) {
                modificadores.alergias.splice(idx, 1);
            } else {
                modificadores.alergias.push(alergia);
            }
            renderModificadorModal();
        },

        agregarExtra(extra) {
            if (!modificadores.extras.includes(extra)) {
                modificadores.extras.push(extra);
            }
            renderModificadorModal();
        },

        quitarExtra(idx) {
            modificadores.extras.splice(idx, 1);
            renderModificadorModal();
        },

        agregarNotaPersonalizada() {
            const nota = prompt('Escriba su nota personalizada:');
            if (nota !== null) {
                modificadores.notaCustom = nota.trim();
            }
            renderModificadorModal();
        },

        async confirmarModificadores() {
            const productoId = modificadores.productoId;
            if (!productoId) return;

            // Build modifications array
            const modificaciones = [];

            // Add quitados / intercambios
            for (const ing of modificadores.ingredientes.filter(i => i.quitado)) {
                modificaciones.push({
                    ingredienteId: ing.id,
                    ingredienteNombre: ing.nombre,
                    accion: ing.motivo === 'intercambio' ? 'intercambiar' : 'quitar',
                    motivo: ing.motivo || 'preferencia',
                    ingredienteReemplazo: ing.ingredienteReemplazo || null
                });
            }

            // Add extras
            for (const extra of modificadores.extras) {
                modificaciones.push({
                    ingredienteId: '00000000-0000-0000-0000-000000000000',
                    ingredienteNombre: extra,
                    accion: 'extra',
                    motivo: 'preferencia',
                    ingredienteReemplazo: null
                });
            }

            // Add curso
            if (modificadores.curso) {
                modificaciones.push({
                    ingredienteId: '00000000-0000-0000-0000-000000000001',
                    ingredienteNombre: modificadores.curso,
                    accion: 'curso',
                    motivo: 'curso',
                    ingredienteReemplazo: null
                });
            }

            // Build alergenos string from selections
            const alergenos = modificadores.alergias.join(', ');

            // Build notes
            const quitadosText = modificaciones
                .filter(m => m.accion === 'quitar')
                .map(m => `Sin ${m.ingredienteNombre}`)
                .join(', ');
            const intercambiosText = modificaciones
                .filter(m => m.accion === 'intercambiar')
                .map(m => `${m.ingredienteNombre} → ${m.ingredienteReemplazo}`)
                .join(', ');
            const extrasText = modificadores.extras.join(', ');
            const notas = [
                alergenos ? `Alergias: ${alergenos}` : '',
                quitadosText,
                intercambiosText,
                extrasText ? `Extra: ${extrasText}` : '',
                modificadores.notaCustom
            ].filter(Boolean).join(' | ');

            this.cerrarModificadores();

            // Call original agregarProducto with modifications
            await this.agregarProducto(productoId, modificaciones, notas);
        },

        async cambiarCantidad(lineaId, nuevaCantidad) {
            if (nuevaCantidad < 1 || !state.pedidoActual) return;
            const linea = state.lineas.find(l => l.id === lineaId);
            if (!linea) return;

            if (!navigator.onLine || state.pedidoActual.isLocal) {
                linea.cantidad = nuevaCantidad;
                linea.subtotal = nuevaCantidad * linea.precioUnitario;
                renderPantallaProductos();
                return;
            }

            try {
                await api.actualizarCantidad(state.pedidoActual.id, linea.id, nuevaCantidad);
                linea.cantidad = nuevaCantidad;
                linea.subtotal = nuevaCantidad * linea.precioUnitario;
                renderPantallaProductos();
            } catch (e) { alert('Error: ' + e.message); }
        },

        async eliminarLinea(lineaId) {
            if (!state.pedidoActual || !confirm('¿Quitar este producto?')) return;
            if (!navigator.onLine || state.pedidoActual.isLocal) {
                state.lineas = state.lineas.filter(l => l.id !== lineaId);
                if (state.lineas.length === 0) {
                    state.pedidoActual = null;
                }
                renderPantallaProductos();
                return;
            }
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
            const total = state.lineas.reduce((s, l) => s + l.subtotal, 0);
            if (!navigator.onLine || state.pedidoActual.isLocal) {
                await queuePayment({ pedidoId: state.pedidoActual.id, method: 'Efectivo', amount: efectivo, tip: efectivo - total, timestamp: Date.now() });
                mostrarToastOffline('Pago guardado. Se procesará al restaurar conexión.');
                state.pedidoActual = null;
                state.lineas = [];
                state.mesaId = null;
                state.pantalla = 'mesa';
                renderPantallaMesa();
                return;
            }
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
            const total = state.lineas.reduce((s, l) => s + l.subtotal, 0);
            if (!navigator.onLine || state.pedidoActual.isLocal) {
                await queuePayment({ pedidoId: state.pedidoActual.id, method: 'Tarjeta', amount: total, tip: 0, timestamp: Date.now() });
                mostrarToastOffline('Pago guardado. Se procesará al restaurar conexión.');
                state.pedidoActual = null;
                state.lineas = [];
                state.mesaId = null;
                state.pantalla = 'mesa';
                renderPantallaMesa();
                return;
            }
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
            if (state.pedidoActual.isLocal) {
                // Cancelar pedido local: eliminar de IndexedDB
                const pendientes = await obtenerPedidosPendientes();
                const pendiente = pendientes.find(p => state.pedidoActual.id.includes(p.id));
                if (pendiente) await eliminarPedidoPendiente(pendiente.id);
                state.pedidoActual = null;
                state.lineas = [];
                state.mesaId = null;
                state.pantalla = 'mesa';
                renderPantallaMesa();
                actualizarOfflineUI();
                return;
            }
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
        actualizarOfflineUI();
        iniciarSignalRPOS();
    });
})();
