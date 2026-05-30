/* ═══════════════════════════════════════════════════════
   Mapa Visual del Salón — La Mesa del Duque
   Pointer Events drag-drop + SignalR + tap-to-act +
   real-time occupancy timers.
   ═══════════════════════════════════════════════════════ */

(function () {
    const csrfToken = () => {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    };

    const ZONA_TAB_SELECTOR = '.lmd-mapa-zona-tab';
    const ZONA_CANVAS_SELECTOR = '.lmd-mapa-zona-canvas';
    const MESA_SELECTOR = '.lmd-mapa-mesa';
    const PANEL = document.getElementById('lmd-mapa-panel');
    const PANEL_TITULO = document.getElementById('lmd-mapa-panel-titulo');
    const PANEL_ESTADO = document.getElementById('lmd-mapa-panel-estado');
    const PANEL_TIMER = document.getElementById('lmd-mapa-panel-timer');
    const PANEL_CERRAR = document.getElementById('lmd-mapa-panel-cerrar');
    const PANEL_ACCIONES = document.getElementById('lmd-mapa-panel-acciones');
    const BADGE_CONEXION = document.getElementById('lmd-mapa-conexion');
    const BADGE_OFFLINE = document.getElementById('lmd-mapa-offline');

    let signalRConnection = null;
    let pollingTimer = null;
    let mesaActivaId = null;

    // ── Zona tabs ─────────────────────────────────────────
    function activarZona(zonaId) {
        document.querySelectorAll(ZONA_TAB_SELECTOR).forEach(t => {
            t.classList.toggle('lmd-mapa-zona-tab--activa', t.dataset.zonaId === zonaId);
        });
        document.querySelectorAll(ZONA_CANVAS_SELECTOR).forEach(c => {
            c.classList.toggle('lmd-mapa-zona-canvas--activa', c.dataset.zonaId === zonaId);
        });
    }

    document.querySelectorAll(ZONA_TAB_SELECTOR).forEach(tab => {
        tab.addEventListener('click', () => activarZona(tab.dataset.zonaId));
    });

    // ── Timer updates ─────────────────────────────────────
    function actualizarTimers() {
        document.querySelectorAll(MESA_SELECTOR).forEach(mesa => {
            const ocupadaDesde = mesa.dataset.mesaOcupadaDesde;
            const timerEl = mesa.querySelector('.lmd-mapa-mesa-timer');
            if (!ocupadaDesde || !timerEl) return;

            const inicio = new Date(ocupadaDesde);
            const ahora = new Date();
            const diffMin = Math.floor((ahora - inicio) / 60000);
            const h = Math.floor(diffMin / 60);
            const m = diffMin % 60;
            timerEl.textContent = h > 0 ? `${h}h ${m}m` : `${m}m`;

            // Update urgency class
            mesa.classList.remove('lmd-mapa--ocupada-verde', 'lmd-mapa--ocupada-amarillo', 'lmd-mapa--ocupada-rojo');
            if (diffMin > 30) mesa.classList.add('lmd-mapa--ocupada-rojo');
            else if (diffMin > 15) mesa.classList.add('lmd-mapa--ocupada-amarillo');
            else mesa.classList.add('lmd-mapa--ocupada-verde');
        });

        // Update panel timer if open
        if (mesaActivaId && PANEL.style.display !== 'none') {
            const mesa = document.querySelector(`${MESA_SELECTOR}[data-mesa-id="${mesaActivaId}"]`);
            if (mesa) {
                const ocupadaDesde = mesa.dataset.mesaOcupadaDesde;
                if (ocupadaDesde) {
                    const diffMin = Math.floor((new Date() - new Date(ocupadaDesde)) / 60000);
                    const h = Math.floor(diffMin / 60);
                    const m = diffMin % 60;
                    PANEL_TIMER.textContent = h > 0 ? `Tiempo: ${h}h ${m}m` : `Tiempo: ${m}m`;
                } else {
                    PANEL_TIMER.textContent = '';
                }
            }
        }
    }

    setInterval(actualizarTimers, 10000);
    actualizarTimers();

    // ── Tap-to-act panel ──────────────────────────────────
    function abrirPanel(mesaEl) {
        const numero = mesaEl.dataset.mesaNumero;
        const estado = mesaEl.dataset.mesaEstado;
        const capacidad = mesaEl.dataset.mesaCapacidad;
        const puedeEditar = mesaEl.dataset.puedeEditar === 'true';
        const activa = mesaEl.dataset.mesaActiva !== 'false';
        const ocupadaDesde = mesaEl.dataset.mesaOcupadaDesde;

        mesaActivaId = mesaEl.dataset.mesaId;
        PANEL_TITULO.textContent = `Mesa ${numero} · ${capacidad} pax`;
        PANEL_ESTADO.textContent = estado;
        PANEL_ESTADO.className = 'lmd-mapa-panel-estado';

        // Set estado badge color
        const estadoClass = {
            'Disponible': 'lmd-mapa--disponible',
            'Ocupada': 'lmd-mapa--ocupada-verde',
            'Reservada': 'lmd-mapa--reservada',
            'EnMantenimiento': 'lmd-mapa--mantenimiento',
            'Inactiva': 'lmd-mapa--inactiva'
        }[estado] || 'lmd-mapa--neutral';
        PANEL_ESTADO.classList.add(estadoClass);

        if (ocupadaDesde && estado === 'Ocupada') {
            const diffMin = Math.floor((new Date() - new Date(ocupadaDesde)) / 60000);
            const h = Math.floor(diffMin / 60);
            const m = diffMin % 60;
            PANEL_TIMER.textContent = h > 0 ? `Tiempo: ${h}h ${m}m` : `Tiempo: ${m}m`;
        } else {
            PANEL_TIMER.textContent = '';
        }

        // Disable buttons for current estado
        PANEL_ACCIONES.querySelectorAll('.lmd-mapa-btn').forEach(btn => {
            btn.disabled = btn.dataset.estado === estado || !puedeEditar || !activa;
        });

        PANEL.style.display = 'block';
    }

    function cerrarPanel() {
        PANEL.style.display = 'none';
        mesaActivaId = null;
    }

    PANEL_CERRAR.addEventListener('click', cerrarPanel);

    PANEL_ACCIONES.querySelectorAll('.lmd-mapa-btn').forEach(btn => {
        btn.addEventListener('click', async () => {
            if (!mesaActivaId) return;
            const nuevoEstado = btn.dataset.estado;
            btn.disabled = true;
            try {
                const res = await fetch('?handler=CambiarEstado', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': csrfToken()
                    },
                    body: JSON.stringify({ mesaId: mesaActivaId, nuevoEstado })
                });
                const data = await res.json();
                if (data.exito) {
                    cerrarPanel();
                } else {
                    alert(data.error || 'Error al cambiar estado');
                }
            } catch (e) {
                console.error(e);
                alert('Error de conexión');
            } finally {
                btn.disabled = false;
            }
        });
    });

    // ── Pointer Events drag-drop ────────────────────────────
    let dragMesa = null;
    let dragStartX = 0;
    let dragStartY = 0;
    let dragInitialLeft = 0;
    let dragInitialTop = 0;
    let dragCanvas = null;
    let hasDragged = false;
    const DRAG_THRESHOLD = 5; // px

    document.querySelectorAll(MESA_SELECTOR).forEach(mesa => {
        mesa.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                abrirPanel(mesa);
            }
        });

        mesa.addEventListener('pointerdown', (e) => {
            const puedeEditar = mesa.dataset.puedeEditar === 'true';
            if (!puedeEditar) {
                abrirPanel(mesa);
                return;
            }

            dragMesa = mesa;
            dragStartX = e.clientX;
            dragStartY = e.clientY;
            dragInitialLeft = parseFloat(mesa.style.left) || 0;
            dragInitialTop = parseFloat(mesa.style.top) || 0;
            dragCanvas = mesa.closest(ZONA_CANVAS_SELECTOR);
            hasDragged = false;
            mesa.setPointerCapture(e.pointerId);
            mesa.style.zIndex = 100;
        });

        mesa.addEventListener('pointermove', (e) => {
            if (!dragMesa || dragMesa !== mesa) return;
            const dx = e.clientX - dragStartX;
            const dy = e.clientY - dragStartY;

            if (Math.abs(dx) > DRAG_THRESHOLD || Math.abs(dy) > DRAG_THRESHOLD) {
                hasDragged = true;
            }

            if (!hasDragged) return;

            if (!dragCanvas) return;
            const rect = dragCanvas.getBoundingClientRect();
            const newLeft = ((e.clientX - rect.left) / rect.width) * 100;
            const newTop = ((e.clientY - rect.top) / rect.height) * 100;

            // Clamp 0-100 with margin
            const clampedLeft = Math.max(2, Math.min(98, newLeft));
            const clampedTop = Math.max(2, Math.min(98, newTop));

            mesa.style.left = clampedLeft + '%';
            mesa.style.top = clampedTop + '%';
        });

        mesa.addEventListener('pointerup', async (e) => {
            if (!dragMesa || dragMesa !== mesa) return;
            mesa.releasePointerCapture(e.pointerId);
            mesa.style.zIndex = 10;

            if (!hasDragged) {
                // It was a tap
                abrirPanel(mesa);
                dragMesa = null;
                return;
            }

            // It was a drag — persist position
            const zonaId = dragCanvas ? dragCanvas.dataset.zonaId : '';
            const posX = parseFloat(mesa.style.left);
            const posY = parseFloat(mesa.style.top);
            const forma = mesa.classList.contains('lmd-mapa-mesa--redonda') ? 'Redonda'
                : mesa.classList.contains('lmd-mapa-mesa--cuadrada') ? 'Cuadrada'
                : 'Bar';
            const rotacion = 0; // Keep current or parse from transform

            dragMesa = null;

            try {
                const res = await fetch('?handler=ActualizarPosicion', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': csrfToken()
                    },
                    body: JSON.stringify({
                        mesaId: mesa.dataset.mesaId,
                        posicionX: Math.round(posX),
                        posicionY: Math.round(posY),
                        zonaId: zonaId,
                        forma: forma,
                        rotacion: rotacion
                    })
                });
                const data = await res.json();
                if (!data.exito) {
                    alert(data.error || 'Error al guardar posición');
                }
            } catch (err) {
                console.error(err);
                alert('Error de conexión al guardar posición');
            }
        });

        mesa.addEventListener('pointercancel', () => {
            if (dragMesa === mesa) {
                mesa.style.left = dragInitialLeft + '%';
                mesa.style.top = dragInitialTop + '%';
                mesa.style.zIndex = 10;
                dragMesa = null;
            }
        });
    });

    // Close panel on canvas click (outside mesa)
    document.querySelectorAll(ZONA_CANVAS_SELECTOR).forEach(canvas => {
        canvas.addEventListener('click', (e) => {
            if (e.target === canvas) cerrarPanel();
        });
    });

    // ── SignalR ───────────────────────────────────────────
    async function iniciarSignalR() {
        if (!window.signalR) {
            console.warn('SignalR no está cargado.');
            mostrarOffline(true);
            iniciarPolling();
            return;
        }

        signalRConnection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/pedidos')
            .withAutomaticReconnect()
            .build();

        signalRConnection.on('MesaMovida', (payload) => {
            const mesa = document.querySelector(`${MESA_SELECTOR}[data-mesa-id="${payload.mesaId}"]`);
            if (mesa) {
                mesa.style.left = payload.posX + '%';
                mesa.style.top = payload.posY + '%';
            }
        });

        signalRConnection.on('MesaActualizada', (payload) => {
            const mesa = document.querySelector(`${MESA_SELECTOR}[data-mesa-id="${payload.mesaId}"]`);
            if (!mesa) return;
            mesa.dataset.mesaEstado = payload.estado;
            mesa.classList.remove('lmd-mapa--disponible', 'lmd-mapa--ocupada-verde', 'lmd-mapa--ocupada-amarillo', 'lmd-mapa--ocupada-rojo', 'lmd-mapa--reservada', 'lmd-mapa--mantenimiento', 'lmd-mapa--inactiva');

            const estadoClass = {
                'Disponible': 'lmd-mapa--disponible',
                'Ocupada': 'lmd-mapa--ocupada-verde',
                'Reservada': 'lmd-mapa--reservada',
                'EnMantenimiento': 'lmd-mapa--mantenimiento',
                'Inactiva': 'lmd-mapa--inactiva'
            }[payload.estado] || 'lmd-mapa--neutral';
            mesa.classList.add(estadoClass);

            if (payload.estado === 'Ocupada') {
                mesa.dataset.mesaOcupadaDesde = new Date().toISOString();
            } else {
                mesa.dataset.mesaOcupadaDesde = '';
            }
            actualizarTimers();
        });

        signalRConnection.onclose(() => {
            mostrarOffline(true);
            iniciarPolling();
        });

        signalRConnection.onreconnecting(() => {
            mostrarOffline(true);
        });

        signalRConnection.onreconnected(() => {
            mostrarOffline(false);
            detenerPolling();
        });

        try {
            await signalRConnection.start();
            await signalRConnection.invoke('UnirseASalon');
            mostrarOffline(false);
        } catch (e) {
            console.warn('SignalR start failed', e);
            mostrarOffline(true);
            iniciarPolling();
        }
    }

    function mostrarOffline(esOffline) {
        if (BADGE_CONEXION) BADGE_CONEXION.style.display = esOffline ? 'none' : 'inline-block';
        if (BADGE_OFFLINE) BADGE_OFFLINE.style.display = esOffline ? 'inline-block' : 'none';
    }

    // ── Polling fallback ──────────────────────────────────
    function iniciarPolling() {
        if (pollingTimer) return;
        pollingTimer = setInterval(async () => {
            try {
                const res = await fetch('?handler=ObtenerDatos');
                if (!res.ok) return;
                const data = await res.json();
                sincronizarMesas(data.mesas);
                mostrarOffline(false);
            } catch {
                mostrarOffline(true);
            }
        }, 15000);
    }

    function detenerPolling() {
        if (pollingTimer) {
            clearInterval(pollingTimer);
            pollingTimer = null;
        }
    }

    function sincronizarMesas(mesas) {
        if (!mesas) return;
        mesas.forEach(m => {
            const mesa = document.querySelector(`${MESA_SELECTOR}[data-mesa-id="${m.id}"]`);
            if (!mesa) return;
            const estado = m.estadoVisual || (m.activa === false ? 'Inactiva' : m.estado);
            mesa.dataset.mesaEstado = estado;
            mesa.dataset.mesaActiva = m.activa === false ? 'false' : 'true';
            mesa.style.left = (m.posicionX ?? 0) + '%';
            mesa.style.top = (m.posicionY ?? 0) + '%';

            mesa.classList.remove('lmd-mapa--disponible', 'lmd-mapa--ocupada-verde', 'lmd-mapa--ocupada-amarillo', 'lmd-mapa--ocupada-rojo', 'lmd-mapa--reservada', 'lmd-mapa--mantenimiento', 'lmd-mapa--inactiva');
            const estadoClass = {
                'Disponible': 'lmd-mapa--disponible',
                'Ocupada': 'lmd-mapa--ocupada-verde',
                'Reservada': 'lmd-mapa--reservada',
                'EnMantenimiento': 'lmd-mapa--mantenimiento',
                'Inactiva': 'lmd-mapa--inactiva'
            }[estado] || 'lmd-mapa--neutral';
            mesa.classList.add(estadoClass);

            if (m.ocupadaDesde) mesa.dataset.mesaOcupadaDesde = m.ocupadaDesde;
            else mesa.dataset.mesaOcupadaDesde = '';
        });
        actualizarTimers();
    }

    // ── Keyboard shortcuts ─────────────────────────────────
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') cerrarPanel();
    });

    // ── Init ──────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', () => {
        iniciarSignalR();
    });
})();
