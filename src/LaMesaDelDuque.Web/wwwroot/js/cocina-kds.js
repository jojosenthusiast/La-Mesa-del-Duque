/* ============================================================================
   La Mesa del Duque — Kitchen Display System (KDS)
   SignalR cliente + render de tarjetas + audio + timers.
   ============================================================================ */

(function () {
    const ESTACIONES = ['Parrilla', 'Fria', 'Caliente', 'Bar', 'Expo', 'Todas'];
    let estacionActual = 'Todas';
    let audioContext = null;
    let audioDesbloqueado = false;

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

    function renderOrden(orden) {
        const grid = document.getElementById('lmd-kds-grid');
        if (!grid) return;

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
                ${orden.notas ? `<div class="lmd-kds-card__notas">${orden.notas}</div>` : ''}
            </div>
            <footer class="lmd-kds-card__footer">
                <button class="lmd-kds-btn-listo" data-orden-id="${orden.id}">LISTO</button>
            </footer>
        `;

        card.querySelector('.lmd-kds-btn-listo').addEventListener('click', () => marcarListo(orden.id));
        grid.appendChild(card);
    }

    function removerOrden(ordenId) {
        const card = document.querySelector(`.lmd-kds-card[data-orden-id="${ordenId}"]`);
        if (card) card.remove();
        actualizarContador();
    }

    function actualizarContador() {
        const contador = document.getElementById('lmd-kds-contador');
        const grid = document.getElementById('lmd-kds-grid');
        if (contador && grid) {
            contador.textContent = `${grid.children.length} ordenes`;
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
            actualizarContador();
            reproducirAlerta();
        });

        connection.on('ItemListo', ordenId => {
            removerOrden(ordenId);
        });

        connection.on('ItemRecuperado', orden => {
            if (estacionActual !== 'Todas' && orden.estacion !== estacionActual) return;
            renderOrden(orden);
            actualizarContador();
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

        // Refrescar grid
        const grid = document.getElementById('lmd-kds-grid');
        if (grid) grid.innerHTML = '';
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
            const grid = document.getElementById('lmd-kds-grid');
            if (grid) grid.innerHTML = '';
            (ordenes || []).forEach(renderOrden);
            actualizarContador();
        } catch (e) {
            console.error('Error cargando ordenes:', e);
        }
    }

    // ── Inicialización ──────────────────────────────────────
    document.addEventListener('DOMContentLoaded', () => {
        // Desbloquear audio con primer interacción
        document.body.addEventListener('click', inicializarAudio, { once: true });

        // Tabs
        document.querySelectorAll('.lmd-kds-tab').forEach(tab => {
            tab.addEventListener('click', () => cambiarEstacion(tab.dataset.estacion));
        });

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
