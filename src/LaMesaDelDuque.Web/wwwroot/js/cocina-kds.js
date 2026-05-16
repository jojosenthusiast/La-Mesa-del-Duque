/* ============================================================================
   La Mesa del Duque — Kitchen Display System (KDS)
   Multi-cocinero 3 columnas + SignalR + audio + timers + diff updates.
   ============================================================================ */

(function () {
    const ESTACIONES = ['Parrilla', 'Fria', 'Caliente', 'Bar', 'Expo', 'Todas'];
    let estacionActual = 'Todas';
    let audioContext = null;
    let audioDesbloqueado = false;
    let ultimoListo = null;

    const COOKS = window.__lmdKdsCooks || [
        { id: 1, name: 'Cocinero 1', color: '#e74c3c' },
        { id: 2, name: 'Cocinero 2', color: '#3498db' },
        { id: 3, name: 'Cocinero 3', color: '#2ecc71' }
    ];

    const STATION_TO_COLUMN = window.__lmdKdsStationMap || {
        'Parrilla': 1,
        'Fria': 2,
        'Caliente': 3,
        'Bar': 2,
        'Expo': 1
    };

    const CURSO_LABELS = {
        'Entrada': 'ENTRADAS',
        'PlatoFuerte': 'PLATOS FUERTES',
        'Postre': 'POSTRES',
        'Bebida': 'BEBIDAS'
    };

    // ── Audio (Web Audio API) ───────────────────────────────
    function inicializarAudio() {
        if (!audioContext) {
            audioContext = new (window.AudioContext || window.webkitAudioContext)();
        }
        if (audioContext.state === 'suspended') {
            audioContext.resume();
        }
        audioDesbloqueado = true;
    }

    function reproducirAlerta() {
        if (!audioContext || !audioDesbloqueado) {
            mostrarIndicadorVisual();
            return;
        }
        try {
            const osc = audioContext.createOscillator();
            const gain = audioContext.createGain();
            osc.connect(gain);
            gain.connect(audioContext.destination);
            osc.type = 'sine';
            osc.frequency.setValueAtTime(880, audioContext.currentTime);
            gain.gain.setValueAtTime(0.1, audioContext.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.001, audioContext.currentTime + 0.4);
            osc.start(audioContext.currentTime);
            osc.stop(audioContext.currentTime + 0.4);
        } catch (e) {
            mostrarIndicadorVisual();
        }
    }

    function mostrarIndicadorVisual() {
        const indicador = document.getElementById('lmd-kds-indicador-visual');
        if (!indicador) return;
        indicador.classList.add('lmd-kds-indicador--activo');
        setTimeout(() => indicador.classList.remove('lmd-kds-indicador--activo'), 2000);
    }

    // ── CSRF ────────────────────────────────────────────────
    function csrfToken() {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    // ── Render ──────────────────────────────────────────────
    function formatearTiempo(minutos) {
        const m = Math.floor(minutos);
        const s = Math.floor((minutos - m) * 60);
        return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
    }

    function calcularColor(orden) {
        const elapsed = (Date.now() - new Date(orden.horaRecibido).getTime()) / 60000;
        const maxTime = orden.tiempoPreparacionMin || 15;
        if (elapsed > maxTime * 1.2) return 'lmd-kds-card--alert';
        if (elapsed > maxTime * 0.8) return 'lmd-kds-card--warn';
        return 'lmd-kds-card--fresh';
    }

    function columnaParaOrden(orden) {
        if (orden.cocineroId) {
            return orden.cocineroId;
        }
        const estacion = orden.estacion || '';
        return STATION_TO_COLUMN[estacion] || 1;
    }

    function cursoHeaderId(colId, curso) {
        return `kds-curso-${colId}-${curso || 'SinCurso'}`;
    }

    function asegurarCursoHeader(colId, curso) {
        const container = document.getElementById(`kds-cards-${colId}`);
        if (!container) return;
        const headerId = cursoHeaderId(colId, curso);
        if (document.getElementById(headerId)) return;

        const header = document.createElement('div');
        header.id = headerId;
        header.className = 'lmd-kds-curso-header';
        header.textContent = CURSO_LABELS[curso] || curso || 'SIN CURSO';

        if (curso === 'Entrada') {
            const fireBtn = document.createElement('button');
            fireBtn.className = 'lmd-kds-fire-btn';
            fireBtn.textContent = '🔥 Disparar entradas';
            fireBtn.addEventListener('click', () => dispararCurso(colId, 'Entrada'));
            header.appendChild(fireBtn);
        }

        container.appendChild(header);
    }

    function dispararCurso(colId, curso) {
        const container = document.getElementById(`kds-cards-${colId}`);
        if (!container) return;
        const cards = container.querySelectorAll('.lmd-kds-card');
        cards.forEach(card => {
            if (card.dataset.curso === curso) {
                card.classList.add('lmd-kds-card--fired');
            }
        });
    }

    function renderModificaciones(orden) {
        const partes = [];

        if (orden.ingredientesQuitados) {
            orden.ingredientesQuitados.split(',').forEach(ing => {
                const limpio = ing.trim();
                if (limpio) partes.push(`<li class="lmd-kds-mod lmd-kds-mod--quitar">SIN ${limpio.toUpperCase()}</li>`);
            });
        }

        if (orden.ingredientesExtra) {
            orden.ingredientesExtra.split(',').forEach(ing => {
                const limpio = ing.trim();
                if (limpio) partes.push(`<li class="lmd-kds-mod lmd-kds-mod--extra">+ EXTRA ${limpio.toUpperCase()}</li>`);
            });
        }

        return partes.length > 0 ? `<ul class="lmd-kds-mods-list">${partes.join('')}</ul>` : '';
    }

    function renderOrden(orden) {
        const colId = columnaParaOrden(orden);
        const container = document.getElementById(`kds-cards-${colId}`);
        if (!container) return;

        const curso = orden.curso || '';
        asegurarCursoHeader(colId, curso);

        const colorClass = calcularColor(orden);
        const mesaTexto = orden.mesaNumero
            ? `Mesa ${orden.mesaNumero}`
            : (orden.tipoServicio === 'ParaLlevar' ? '🛍 Para llevar' : 'Sin mesa');

        const tieneModificaciones = orden.ingredientesQuitados || orden.ingredientesExtra;
        const tieneNotas = !!orden.notas;
        const tieneAlergenos = !!orden.alergenos;

        const card = document.createElement('article');
        card.className = `lmd-kds-card ${colorClass}${tieneAlergenos ? ' lmd-kds-card--alerta-alergeno' : ''}`;
        card.dataset.id = orden.id;
        card.dataset.ordenId = orden.id;
        card.dataset.curso = curso;
        card.dataset.tiempoPreparacionMin = orden.tiempoPreparacionMin || 15;

        card.innerHTML = `
            ${tieneAlergenos ? `<div class="lmd-kds-alergeno-banner">⚠ ALÉRGENO: ${orden.alergenos.toUpperCase()}</div>` : ''}
            <header class="lmd-kds-card__header">
                <span class="lmd-kds-card__mesa">${mesaTexto}</span>
                <span class="lmd-kds-card__timer" data-hora-recibido="${orden.horaRecibido}">${formatearTiempo(orden.minutosTranscurridos || 0)}</span>
            </header>
            <div class="lmd-kds-card__dish-row">
                <span class="lmd-kds-card__producto">${orden.productoNombre}</span>
                <span class="lmd-kds-card__cantidad">${orden.cantidad}</span>
            </div>
            ${tieneModificaciones ? `<div class="lmd-kds-card__modificaciones">${renderModificaciones(orden)}</div>` : ''}
            ${tieneNotas ? `<div class="lmd-kds-card__notas-block"><span class="lmd-kds-notas-label">NOTA</span> ${orden.notas}</div>` : ''}
            <footer class="lmd-kds-card__footer">
                <button class="lmd-kds-btn-listo" data-orden-id="${orden.id}">✓ LISTO</button>
                ${orden.productoId ? `<button class="lmd-kds-btn-86" data-producto-id="${orden.productoId}" title="86 — Agotado">86</button>` : ''}
            </footer>
        `;

        card.querySelector('.lmd-kds-btn-listo').addEventListener('click', () => marcarListo(orden.id));
        const btn86 = card.querySelector('.lmd-kds-btn-86');
        if (btn86) {
            btn86.addEventListener('click', () => {
                if (connection) connection.invoke('MarcarAgotado', orden.productoId);
            });
        }
        container.appendChild(card);
    }

    function agregarCard(orden) {
        renderOrden(orden);
        actualizarContadores();
    }

    function actualizarColorCard(ordenId, colorClass) {
        const card = document.querySelector(`.lmd-kds-card[data-id="${ordenId}"]`);
        if (!card) return;
        card.classList.remove('lmd-kds-card--fresh', 'lmd-kds-card--warn', 'lmd-kds-card--alert');
        card.classList.add(colorClass);
    }

    function removerOrden(ordenId) {
        const card = document.querySelector(`.lmd-kds-card[data-orden-id="${ordenId}"]`);
        if (card) {
            ultimoListo = { ordenId, html: card.outerHTML };
            card.remove();
        }
        actualizarContadores();
    }

    function recuperarUltimoListo() {
        if (!ultimoListo) return;
        // In a real implementation, this would call the API to recover
        console.log('Recuperar último listo:', ultimoListo.ordenId);
    }

    function actualizarContadores() {
        const contador = document.getElementById('lmd-kds-contador');
        let total = 0;
        COOKS.forEach(cook => {
            const container = document.getElementById(`kds-cards-${cook.id}`);
            const countEl = document.getElementById(`kds-count-${cook.id}`);
            const count = container ? container.querySelectorAll('.lmd-kds-card').length : 0;
            total += count;
            if (countEl) {
                countEl.textContent = `${count} ${count === 1 ? 'orden' : 'ordenes'}`;
            }
        });
        if (contador) {
            contador.textContent = `${total} ${total === 1 ? 'orden' : 'ordenes'}`;
        }
    }

    function actualizarTimers() {
        document.querySelectorAll('.lmd-kds-card__timer').forEach(el => {
            const horaRecibido = new Date(el.dataset.horaRecibido);
            const minutos = (Date.now() - horaRecibido.getTime()) / 60000;
            el.textContent = formatearTiempo(minutos);

            const card = el.closest('.lmd-kds-card');
            if (card) {
                const ordenId = card.dataset.id;
                const orden = { horaRecibido: el.dataset.horaRecibido, tiempoPreparacionMin: 15 };
                // Try to read tiempoPreparacionMin from card data if available
                const maxTime = parseInt(card.dataset.tiempoPreparacionMin) || 15;
                const elapsed = minutos;
                let colorClass = 'lmd-kds-card--fresh';
                if (elapsed > maxTime * 1.2) colorClass = 'lmd-kds-card--alert';
                else if (elapsed > maxTime * 0.8) colorClass = 'lmd-kds-card--warn';

                card.classList.remove('lmd-kds-card--fresh', 'lmd-kds-card--warn', 'lmd-kds-card--alert');
                card.classList.add(colorClass);
            }
        });
    }

    // ── API ─────────────────────────────────────────────────
    async function marcarListo(ordenId) {
        const card = document.querySelector(`.lmd-kds-card[data-orden-id="${ordenId}"]`);
        if (card) card.classList.add('lmd-kds-card--completing');

        const form = new FormData();
        form.append('__RequestVerificationToken', csrfToken());
        form.append('ordenId', ordenId);

        try {
            const res = await fetch('?handler=MarcarListoJson', {
                method: 'POST',
                body: form,
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            if (!res.ok) {
                const msg = await res.text();
                if (card) card.classList.remove('lmd-kds-card--completing');
                mostrarToastKds('Error: ' + msg, 'error');
                return;
            }
            removerOrden(ordenId);
        } catch (e) {
            if (card) card.classList.remove('lmd-kds-card--completing');
            mostrarToastKds('Sin conexión — reintentando...', 'warn');
        }
    }

    function mostrarToastKds(msg, tipo) {
        let toast = document.getElementById('lmd-kds-toast');
        if (!toast) {
            toast = document.createElement('div');
            toast.id = 'lmd-kds-toast';
            document.body.appendChild(toast);
        }
        toast.textContent = msg;
        toast.className = `lmd-kds-toast lmd-kds-toast--${tipo} lmd-kds-toast--visible`;
        clearTimeout(toast._timeout);
        toast._timeout = setTimeout(() => toast.classList.remove('lmd-kds-toast--visible'), 3000);
    }

    // ── SignalR ─────────────────────────────────────────────
    let connection = null;
    let pollingInterval = null;

    async function iniciarSignalR() {
        if (!window.signalR) {
            console.warn('SignalR no está cargado.');
            startPolling();
            return;
        }

        connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/pedidos')
            .withAutomaticReconnect()
            .build();

        connection.on('NuevaOrden', orden => {
            if (estacionActual !== 'Todas' && orden.estacion !== estacionActual) return;
            renderOrden(orden);
            actualizarContadores();
            reproducirAlerta();
        });

        connection.on('ItemListo', ordenId => {
            removerOrden(ordenId);
        });

        connection.on('ItemRecuperado', orden => {
            if (estacionActual !== 'Todas' && orden.estacion !== estacionActual) return;
            renderOrden(orden);
            actualizarContadores();
        });

        connection.onclose(() => {
            console.log('SignalR disconnected — starting polling fallback');
            startPolling();
        });

        connection.onreconnecting(() => {
            console.log('SignalR reconnecting...');
        });

        connection.onreconnected(async () => {
            console.log('SignalR reconnected — stopping polling');
            stopPolling();
            await cargarOrdenesIniciales();
        });

        try {
            await connection.start();
            await conectarAEstacion(estacionActual);
        } catch (e) {
            console.warn('SignalR start failed — falling back to polling', e);
            startPolling();
        }
    }

    // ── Polling fallback ────────────────────────────────────
    function startPolling() {
        if (pollingInterval) return;
        mostrarIndicadorOffline(true);
        pollingInterval = setInterval(async () => {
            try {
                const res = await fetch(`?handler=EstadoActualJson&estacion=${encodeURIComponent(estacionActual)}`, {
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });
                if (!res.ok) return;
                const data = await res.json();
                if (data && data.ordenesCocina) {
                    actualizarCards(data.ordenesCocina);
                }
            } catch (e) {
                // Server unreachable — keep what we have
            }
        }, 5000);
    }

    function stopPolling() {
        if (pollingInterval) {
            clearInterval(pollingInterval);
            pollingInterval = null;
        }
        mostrarIndicadorOffline(false);
    }

    function mostrarIndicadorOffline(visible) {
        const badge = document.getElementById('lmd-kds-offline-badge');
        if (badge) {
            badge.classList.toggle('visible', visible);
        }
    }

    function actualizarCards(nuevasOrdenes) {
        const existingIds = new Set([...document.querySelectorAll('.lmd-kds-card')].map(c => c.dataset.id));
        const newIds = new Set(nuevasOrdenes.map(o => o.id));

        // Remove cards for completed orders
        for (const id of existingIds) {
            if (!newIds.has(id)) {
                document.querySelector(`.lmd-kds-card[data-id="${id}"]`)?.remove();
            }
        }

        // Add new cards
        for (const orden of nuevasOrdenes) {
            if (!existingIds.has(orden.id)) {
                agregarCard(orden);
            } else {
                // Update timer color only (don't re-render)
                actualizarColorCard(orden.id, calcularColor(orden));
            }
        }

        actualizarContadores();
    }

    async function conectarAEstacion(estacion) {
        if (!connection) return;
        for (const est of ESTACIONES) {
            if (est === 'Todas') continue;
            await connection.invoke('SalirDeGrupo', `cocina-${est}`);
        }
        if (estacion !== 'Todas') {
            await connection.invoke('UnirseAGrupo', `cocina-${estacion}`);
        } else {
            for (const est of ESTACIONES) {
                if (est === 'Todas') continue;
                await connection.invoke('UnirseAGrupo', `cocina-${est}`);
            }
        }
    }

    // ── UI Tabs ─────────────────────────────────────────────
    function cambiarEstacion(estacion) {
        estacionActual = estacion;
        document.querySelectorAll('.lmd-kds-tab').forEach(tab => {
            tab.classList.toggle('lmd-kds-tab--activo', tab.dataset.estacion === estacion);
        });

        // Limpiar columnas
        COOKS.forEach(cook => {
            const container = document.getElementById(`kds-cards-${cook.id}`);
            if (container) container.innerHTML = '';
        });
        cargarOrdenesIniciales();
        conectarAEstacion(estacion);
    }

    async function cargarOrdenesIniciales() {
        try {
            const res = await fetch(`?handler=OrdenesJson&estacion=${estacionActual}`, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            if (!res.ok) return;
            const ordenes = await res.json();
            COOKS.forEach(cook => {
                const container = document.getElementById(`kds-cards-${cook.id}`);
                if (container) container.innerHTML = '';
            });
            (ordenes || []).forEach(renderOrden);
            actualizarContadores();
        } catch (e) {
            console.error('Error cargando ordenes:', e);
        }
    }

    // ── Tabs de columna (mobile) ────────────────────────────
    function renderColumnTabs() {
        const main = document.querySelector('.lmd-kds-main');
        if (!main) return;

        // Si ya existe el contenedor de tabs, no recrear
        if (document.querySelector('.lmd-kds-col-tabs')) return;

        const tabsContainer = document.createElement('div');
        tabsContainer.className = 'lmd-kds-col-tabs';
        tabsContainer.innerHTML = COOKS.map((cook, i) =>
            `<button class="lmd-kds-col-tab ${i === 0 ? 'lmd-kds-col-tab--activo' : ''}" data-col-id="${cook.id}">${cook.name}</button>`
        ).join('');

        main.insertBefore(tabsContainer, main.firstChild);

        tabsContainer.querySelectorAll('.lmd-kds-col-tab').forEach(tab => {
            tab.addEventListener('click', () => {
                const colId = tab.dataset.colId;
                tabsContainer.querySelectorAll('.lmd-kds-col-tab').forEach(t => t.classList.remove('lmd-kds-col-tab--activo'));
                tab.classList.add('lmd-kds-col-tab--activo');
                document.querySelectorAll('.lmd-kds-col').forEach(col => col.classList.remove('lmd-kds-col--activa'));
                const col = document.getElementById(`kds-col-${colId}`);
                if (col) col.classList.add('lmd-kds-col--activa');
            });
        });

        // Activar primera columna por defecto en mobile
        const firstCol = document.getElementById(`kds-col-${COOKS[0].id}`);
        if (firstCol) firstCol.classList.add('lmd-kds-col--activa');
    }

    // ── Keyboard shortcuts (bump bar) ───────────────────────
    document.addEventListener('keydown', (e) => {
        // Number keys 1-9: select order
        if (e.key >= '1' && e.key <= '9' && !e.ctrlKey && !e.metaKey) {
            const index = parseInt(e.key) - 1;
            const cards = document.querySelectorAll('.lmd-kds-card');
            if (cards[index]) cards[index].scrollIntoView({ behavior: 'smooth' });
        }
        // Space or Enter: bump selected (mark listo)
        if (e.key === ' ' || e.key === 'Enter') {
            const visible = [...document.querySelectorAll('.lmd-kds-card')].find(c => {
                const rect = c.getBoundingClientRect();
                return rect.top >= 0 && rect.bottom <= window.innerHeight;
            });
            if (visible) visible.querySelector('.lmd-kds-btn-listo')?.click();
        }
        // R: recall last bumped
        if (e.key === 'r' || e.key === 'R') {
            recuperarUltimoListo();
        }
    });

    // ── Inicialización ──────────────────────────────────────
    document.addEventListener('DOMContentLoaded', () => {
        // Desbloquear audio con primer interacción
        document.body.addEventListener('click', inicializarAudio, { once: true });

        // Tabs de estación
        document.querySelectorAll('.lmd-kds-tab').forEach(tab => {
            tab.addEventListener('click', () => cambiarEstacion(tab.dataset.estacion));
        });

        // Tabs de columna para mobile
        renderColumnTabs();

        // Cargar ordenes iniciales y conectar SignalR
        cargarOrdenesIniciales();
        iniciarSignalR();

        // Timer updater cada 10s
        setInterval(actualizarTimers, 10000);

        // Clock
        const clock = document.getElementById('lmd-kds-reloj');
        if (clock) {
            setInterval(() => {
                clock.textContent = new Date().toLocaleTimeString('es-SV', { hour: '2-digit', minute: '2-digit' });
            }, 1000);
        }
    });
})();
