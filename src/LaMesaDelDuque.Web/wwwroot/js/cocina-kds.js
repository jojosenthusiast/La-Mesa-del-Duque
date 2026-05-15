/* ============================================================================
   La Mesa del Duque — Kitchen Display System (KDS)
   Multi-cocinero 3 columnas + SignalR + audio + timers.
   ============================================================================ */

(function () {
    const ESTACIONES = ['Parrilla', 'Fria', 'Caliente', 'Bar', 'Expo', 'Todas'];
    let estacionActual = 'Todas';
    let audioContext = null;
    let audioDesbloqueado = false;

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

    function claseColor(minutos) {
        if (minutos > 15) return 'lmd-kds-card--alert';
        if (minutos >= 5) return 'lmd-kds-card--warn';
        return 'lmd-kds-card--fresh';
    }

    function columnaParaOrden(orden) {
        // Si la orden tiene un cocinero asignado manualmente, usarlo
        if (orden.cocineroId) {
            return orden.cocineroId;
        }
        // Routing automático por estación
        const estacion = orden.estacion || '';
        return STATION_TO_COLUMN[estacion] || 1;
    }

    function renderInstruccionesEspeciales(orden) {
        let html = '';
        if (orden.alergenos) {
            html += `<div class="lmd-kds-card-alergenos">⚠ ALÉRGENOS: ${orden.alergenos}</div>`;
        }
        if (orden.ingredientesQuitados) {
            html += `<div class="lmd-kds-card-quitados">❌ Sin ${orden.ingredientesQuitados}</div>`;
        }
        if (orden.ingredientesExtra) {
            html += `<div class="lmd-kds-card-extras">➕ ${orden.ingredientesExtra}</div>`;
        }
        if (orden.notas) {
            html += `<div class="lmd-kds-card-notas">📝 ${orden.notas}</div>`;
        }
        return html;
    }

    function renderOrden(orden) {
        const colId = columnaParaOrden(orden);
        const container = document.getElementById(`kds-cards-${colId}`);
        if (!container) return;

        const minutos = orden.minutosTranscurridos || 0;
        const colorClass = claseColor(minutos);
        const mesaTexto = orden.mesaNumero ? `Mesa ${orden.mesaNumero}` : (orden.tipoServicio === 'ParaLlevar' ? 'Para llevar' : 'Sin mesa');

        const card = document.createElement('article');
        card.className = `lmd-kds-card ${colorClass}`;
        card.dataset.ordenId = orden.id;
        card.innerHTML = `
            <header class="lmd-kds-card__header">
                <span class="lmd-kds-card__mesa">${mesaTexto}</span>
                <span class="lmd-kds-card__timer" data-hora-recibido="${orden.horaRecibido}">${formatearTiempo(minutos)}</span>
            </header>
            <div class="lmd-kds-card__body">
                <div class="lmd-kds-card__producto">${orden.productoNombre}</div>
                <div class="lmd-kds-card__cantidad">x${orden.cantidad}</div>
                ${renderInstruccionesEspeciales(orden)}
            </div>
            <footer class="lmd-kds-card__footer">
                <button class="lmd-kds-btn-listo" data-orden-id="${orden.id}">LISTO</button>
            </footer>
        `;

        card.querySelector('.lmd-kds-btn-listo').addEventListener('click', () => marcarListo(orden.id));
        container.appendChild(card);
    }

    function removerOrden(ordenId) {
        const card = document.querySelector(`.lmd-kds-card[data-orden-id="${ordenId}"]`);
        if (card) card.remove();
        actualizarContadores();
    }

    function actualizarContadores() {
        const contador = document.getElementById('lmd-kds-contador');
        let total = 0;
        COOKS.forEach(cook => {
            const container = document.getElementById(`kds-cards-${cook.id}`);
            const countEl = document.getElementById(`kds-count-${cook.id}`);
            const count = container ? container.children.length : 0;
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
                card.classList.remove('lmd-kds-card--fresh', 'lmd-kds-card--warn', 'lmd-kds-card--alert');
                card.classList.add(claseColor(minutos));
            }
        });
    }

    // ── API ─────────────────────────────────────────────────
    async function marcarListo(ordenId) {
        const form = new FormData();
        form.append('__RequestVerificationToken', csrfToken());
        form.append('ordenId', ordenId);

        try {
            const res = await fetch('?handler=MarcarListoJson', {
                method: 'POST',
                body: form,
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            if (!res.ok) throw new Error(await res.text());
            removerOrden(ordenId);
        } catch (e) {
            alert('Error al marcar como listo: ' + e.message);
        }
    }

    // ── SignalR ─────────────────────────────────────────────
    let connection = null;

    async function iniciarSignalR() {
        if (!window.signalR) {
            console.warn('SignalR no está cargado.');
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

        await connection.start();
        await conectarAEstacion(estacionActual);
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
