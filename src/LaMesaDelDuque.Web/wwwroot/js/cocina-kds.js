/* ============================================================================
   La Mesa del Duque — Kitchen Display System (KDS)
   Multi-cocinero 3 columnas + SignalR + audio + timers + diff updates.
   ============================================================================ */

(function () {
    const ESTACIONES = ['Parrilla', 'Fria', 'Caliente', 'Bar', 'Expo', 'Todas'];
    let estacionActual = 'Todas';
    let prioridadActual = 'Todas';
    let servicioActual = 'Todos';
    let audioContext = null;
    let audioDesbloqueado = false;
    let ultimoListo = null;
    let ultimoListoColumna = null;
    let timerEscalacion = null;
    const ALERTA_ESCALACION_MIN = 45;

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

    const SERVICIO_LABELS = {
        'ComerAqui': 'Mesa',
        'ParaLlevar': 'Para llevar',
        'Delivery': 'Delivery'
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

    function escapeHtml(value) {
        return String(value ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function calcularColor(orden) {
        return calcularEstadoTiempo(orden).colorClass;
    }

    function minutosTranscurridos(orden) {
        const ts = new Date(orden.horaRecibido).getTime();
        if (Number.isNaN(ts)) return 0;
        return Math.max(0, (Date.now() - ts) / 60000);
    }

    function calcularEstadoTiempo(orden) {
        const elapsed = minutosTranscurridos(orden);
        const estimado = Math.max(1, parseInt(orden.tiempoPreparacionMin, 10) || 15);
        const ratio = elapsed / estimado;
        const restante = estimado - elapsed;

        if (ratio > 1.2) {
            return {
                prioridad: 'Atrasado',
                columnaId: 1,
                colorClass: 'lmd-kds-card--alert',
                label: 'ATRASADO +' + Math.ceil(Math.abs(restante)) + ' min',
                progreso: 100,
                elapsed,
                estimado
            };
        }

        if (ratio > 0.8) {
            return {
                prioridad: 'PorVencer',
                columnaId: 2,
                colorClass: 'lmd-kds-card--warn',
                label: 'Vence en ' + Math.max(0, Math.ceil(restante)) + ' min',
                progreso: Math.min(100, Math.round(ratio * 100)),
                elapsed,
                estimado
            };
        }

        return {
            prioridad: 'EnTiempo',
            columnaId: 3,
            colorClass: 'lmd-kds-card--fresh',
            label: 'A tiempo',
            progreso: Math.min(100, Math.round(ratio * 100)),
            elapsed,
            estimado
        };
    }

    function columnaParaOrden(orden) {
        return calcularEstadoTiempo(orden).columnaId;
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
            fireBtn.textContent = 'Disparar entradas';
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
                if (limpio) partes.push(`<li class="lmd-kds-mod lmd-kds-mod--quitar">SIN ${escapeHtml(limpio.toUpperCase())}</li>`);
            });
        }

        if (orden.ingredientesExtra) {
            orden.ingredientesExtra.split(',').forEach(ing => {
                const limpio = ing.trim();
                if (limpio) partes.push(`<li class="lmd-kds-mod lmd-kds-mod--extra">+ EXTRA ${escapeHtml(limpio.toUpperCase())}</li>`);
            });
        }

        return partes.length > 0 ? `<ul class="lmd-kds-mods-list">${partes.join('')}</ul>` : '';
    }

    function renderOrden(orden) {
        const estadoTiempo = calcularEstadoTiempo(orden);
        const colId = columnaParaOrden(orden);
        const container = document.getElementById(`kds-cards-${colId}`);
        if (!container) return;

        const curso = orden.curso || '';
        asegurarCursoHeader(colId, curso);

        const colorClass = estadoTiempo.colorClass;
        const ordenId = escapeHtml(orden.id);
        const productoId = escapeHtml(orden.productoId);
        const productoNombre = escapeHtml(orden.productoNombre);
        const cantidad = escapeHtml(orden.cantidad);
        const horaRecibido = escapeHtml(orden.horaRecibido);
        const notas = escapeHtml(orden.notas);
        const alergenos = escapeHtml(String(orden.alergenos ?? '').toUpperCase());
        const estacion = escapeHtml(orden.estacion || 'Expo');
        const cursoLabel = escapeHtml(CURSO_LABELS[orden.curso] || orden.curso || 'SIN CURSO');
        const servicioLabel = escapeHtml(SERVICIO_LABELS[orden.tipoServicio] || orden.tipoServicio || 'Sin servicio');
        const estadoLabel = escapeHtml(estadoTiempo.label);
        const estimado = escapeHtml(estadoTiempo.estimado);
        const progreso = Math.max(0, Math.min(100, estadoTiempo.progreso));
        const pedidoCorto = escapeHtml(String(orden.pedidoId || '').slice(0, 8));
        const mesaTexto = orden.mesaNumero
            ? `Mesa ${escapeHtml(orden.mesaNumero)}`
            : (orden.tipoServicio === 'Delivery'
                ? '<svg class="lmd-kds-icon" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2"><use href="/lib/lucide-static/icons/truck.svg#icon"/></svg> Delivery'
                : (orden.tipoServicio === 'ParaLlevar' ? '<svg class="lmd-kds-icon" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2"><use href="/lib/lucide-static/icons/package.svg#icon"/></svg> Para llevar' : 'Sin mesa'));

        const tieneModificaciones = orden.ingredientesQuitados || orden.ingredientesExtra;
        const tieneNotas = !!orden.notas;
        const tieneAlergenos = !!orden.alergenos;

        const card = document.createElement('article');
        card.className = `lmd-kds-card ${colorClass}${tieneAlergenos ? ' lmd-kds-card--alerta-alergeno' : ''}`;
        card.dataset.id = orden.id;
        card.dataset.ordenId = orden.id;
        card.dataset.curso = curso;
        card.dataset.tiempoPreparacionMin = orden.tiempoPreparacionMin || 15;
        card.dataset.horaRecibido = orden.horaRecibido || '';
        card.dataset.estacion = orden.estacion || '';
        card.dataset.tipoServicio = orden.tipoServicio || '';
        card.dataset.prioridad = estadoTiempo.prioridad;

        card.innerHTML = `
            ${tieneAlergenos ? `<div class="lmd-kds-alergeno-banner"><svg width="18" height="18" fill="none" stroke="currentColor" stroke-width="2"><use href="/lib/lucide-static/icons/alert-triangle.svg#icon"/></svg> ALÉRGENO: ${alergenos}</div>` : ''}
            <header class="lmd-kds-card__header">
                <span class="lmd-kds-card__mesa">${mesaTexto}</span>
                <span class="lmd-kds-card__timer" data-hora-recibido="${horaRecibido}">${formatearTiempo(estadoTiempo.elapsed)}</span>
            </header>
            <div class="lmd-kds-card__meta-row">
                <span>${estacion}</span>
                <span>${cursoLabel}</span>
                <span>${servicioLabel}</span>
            </div>
            <div class="lmd-kds-card__sla-row">
                <span class="lmd-kds-card__sla-text">${estadoLabel}</span>
                <span class="lmd-kds-card__eta"><span class="lmd-kds-card__elapsed">${formatearTiempo(estadoTiempo.elapsed)}</span> / ${estimado} min</span>
            </div>
            <div class="lmd-kds-card__progress" aria-hidden="true"><span style="width:${progreso}%"></span></div>
            <div class="lmd-kds-card__dish-row">
                <span class="lmd-kds-card__producto">${productoNombre}</span>
                <span class="lmd-kds-card__cantidad">${cantidad}</span>
            </div>
            ${tieneModificaciones ? `<div class="lmd-kds-card__modificaciones">${renderModificaciones(orden)}</div>` : ''}
            ${tieneNotas ? `<div class="lmd-kds-card__notas-block"><span class="lmd-kds-notas-label">NOTA</span> ${notas}</div>` : ''}
            <footer class="lmd-kds-card__footer">
                <span class="lmd-kds-card__pedido">Pedido ${pedidoCorto}</span>
                <button class="lmd-kds-btn-listo" data-orden-id="${ordenId}"><svg width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5"><use href="/lib/lucide-static/icons/check.svg#icon"/></svg> LISTO</button>
                ${orden.productoId ? `<button class="lmd-kds-btn-86" data-producto-id="${productoId}" title="86 — Agotado">86</button>` : ''}
            </footer>
        `;

        card.querySelector('.lmd-kds-btn-listo').addEventListener('click', () => marcarListo(orden.id));
        const btn86 = card.querySelector('.lmd-kds-btn-86');
        if (btn86) {
            btn86.addEventListener('click', async () => {
                btn86.disabled = true;
                btn86.textContent = '…';
                try {
                    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
                    const resp = await fetch('?handler=Marcar86Json', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
                        body: JSON.stringify({ productoId: orden.productoId })
                    });
                    const data = await resp.json();
                    if (!data.ok) {
                        btn86.disabled = false;
                        btn86.textContent = '86';
                        alert(data.error ?? 'Error al marcar agotado.');
                    }
                } catch {
                    btn86.disabled = false;
                    btn86.textContent = '86';
                }
            });
        }
        insertarOrdenada(container, card);
    }

    function insertarOrdenada(container, card) {
        const cards = [...container.querySelectorAll('.lmd-kds-card:not(.lmd-kds-card--completing)')];
        const ts = new Date(card.dataset.horaRecibido || '').getTime() || 0;
        const siguiente = cards.find(c => (new Date(c.dataset.horaRecibido || '').getTime() || 0) > ts);
        if (siguiente) container.insertBefore(card, siguiente);
        else container.appendChild(card);
    }

    function agregarCard(orden) {
        renderOrden(orden);
        aplicarFiltrosLocales();
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
            const container = card.closest('.lmd-kds-orders');
            ultimoListo = { ordenId, html: card.outerHTML, columnaId: container?.id };
            card.classList.add('lmd-kds-card--completing');
            setTimeout(() => { card.remove(); actualizarContadores(); verificarMesasVacias(); }, 300);
        }
        actualizarContadores();
        actualizarEscalacion();
    }

    function recuperarUltimoListo() {
        if (!ultimoListo) {
            mostrarToastKds('Nada para recuperar', 'warn');
            return;
        }
        const container = ultimoListo.columnaId
            ? document.getElementById(ultimoListo.columnaId)
            : document.getElementById(`kds-cards-${COOKS[0].id}`);
        if (!container) return;

        const temp = document.createElement('div');
        temp.innerHTML = ultimoListo.html;
        const card = temp.querySelector('.lmd-kds-card');
        if (!card) return;
        card.classList.remove('lmd-kds-card--completing');
        card.classList.add('lmd-kds-card--recuperada');

        // Re-attach LISTO button handler
        const btn = card.querySelector('.lmd-kds-btn-listo');
        if (btn) {
            btn.addEventListener('click', () => marcarListo(ultimoListo.ordenId));
        }
        container.appendChild(card);
        mostrarToastKds('Orden recuperada', 'success');
        actualizarContadores();
        ultimoListo = null;
    }

    function actualizarContadores() {
        const contador = document.getElementById('lmd-kds-contador');
        let total = 0;
        COOKS.forEach(cook => {
            const container = document.getElementById(`kds-cards-${cook.id}`);
            const countEl = document.getElementById(`kds-count-${cook.id}`);
            const count = container ? container.querySelectorAll('.lmd-kds-card:not(.lmd-kds-card--completing):not([hidden])').length : 0;
            total += count;
            if (countEl) countEl.textContent = count + ' ordenes';
        });
        if (contador) contador.textContent = total + ' ordenes';
        actualizarEscalacion();
    }

    // ── Auto-escalación >45min ──────────────────────────
    function actualizarEscalacion() {
        clearTimeout(timerEscalacion);
        const ahora = Date.now();
        let maxMinutos = 0;
        document.querySelectorAll('.lmd-kds-card[data-hora-recibido]').forEach(card => {
            const ts = new Date(card.dataset.horaRecibido).getTime();
            const min = Math.floor((ahora - ts) / 60000);
            if (min > maxMinutos) maxMinutos = min;
            card.classList.toggle('lmd-kds-card--escalado', min >= ALERTA_ESCALACION_MIN);
            if (min >= ALERTA_ESCALACION_MIN) {
                const timer = card.querySelector('.lmd-kds-card__timer');
                if (timer && !timer.textContent.includes('ESCALAR')) {
                    timer.textContent = 'ESCALAR ' + formatearTiempo(min);
                }
            }
        });
        if (maxMinutos >= ALERTA_ESCALACION_MIN) {
            timerEscalacion = setTimeout(() => {
                mostrarToastKds('ATENCIÓN: órdenes >' + ALERTA_ESCALACION_MIN + 'min sin completar', 'error');
                reproducirAlerta();
            }, 5000);
        }
    }

    // ── Group separator: Mesa X → Y items ───────────────
    function verificarMesasVacias() {
        // Clean empty mesa separators
        document.querySelectorAll('.lmd-kds-mesa-group').forEach(g => {
            const col = g.closest('.lmd-kds-orders');
            const cards = col ? col.querySelectorAll('.lmd-kds-card:not(.lmd-kds-card--completing)') : [];
            if (cards.length === 0) g.remove();
        });
    }

    function agregarSeparadorMesa(container, orden) {
        if (!orden.mesaNumero) return;
        const groupId = 'mesa-group-' + orden.mesaNumero;
        const mesaNumero = escapeHtml(orden.mesaNumero);
        let group = document.getElementById(groupId);
        if (!group) {
            group = document.createElement('div');
            group.id = groupId;
            group.className = 'lmd-kds-mesa-group';
            group.innerHTML = '<div class="lmd-kds-mesa-group-header">Mesa ' + mesaNumero + '</div>';
            container.appendChild(group);
        }
        return group;
    }

    function actualizarTimers() {
        document.querySelectorAll('.lmd-kds-card__timer').forEach(el => {
            const card = el.closest('.lmd-kds-card');
            if (card) {
                const orden = {
                    horaRecibido: el.dataset.horaRecibido,
                    tiempoPreparacionMin: parseInt(card.dataset.tiempoPreparacionMin, 10) || 15
                };
                const estado = calcularEstadoTiempo(orden);
                el.textContent = formatearTiempo(estado.elapsed);

                card.classList.remove('lmd-kds-card--fresh', 'lmd-kds-card--warn', 'lmd-kds-card--alert');
                card.classList.add(estado.colorClass);
                card.dataset.prioridad = estado.prioridad;

                const elapsedEl = card.querySelector('.lmd-kds-card__elapsed');
                if (elapsedEl) elapsedEl.textContent = formatearTiempo(estado.elapsed);

                const statusEl = card.querySelector('.lmd-kds-card__sla-text');
                if (statusEl) statusEl.textContent = estado.label;

                const progressEl = card.querySelector('.lmd-kds-card__progress span');
                if (progressEl) progressEl.style.width = Math.max(0, Math.min(100, estado.progreso)) + '%';

                const destino = document.getElementById(`kds-cards-${estado.columnaId}`);
                if (destino && card.parentElement !== destino) {
                    card.remove();
                    insertarOrdenada(destino, card);
                }
            }
        });
        aplicarFiltrosLocales();
        actualizarContadores();
    }

    function debeMostrarCard(card) {
        if (prioridadActual !== 'Todas' && card.dataset.prioridad !== prioridadActual) return false;
        if (servicioActual !== 'Todos' && card.dataset.tipoServicio !== servicioActual) return false;
        return true;
    }

    function aplicarFiltrosLocales() {
        document.querySelectorAll('.lmd-kds-card').forEach(card => {
            card.hidden = !debeMostrarCard(card);
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
            aplicarFiltrosLocales();
            actualizarContadores();
            reproducirAlerta();
        });

        connection.on('ItemListo', ordenId => {
            removerOrden(ordenId);
        });

        connection.on('ItemRecuperado', orden => {
            if (estacionActual !== 'Todas' && orden.estacion !== estacionActual) return;
            renderOrden(orden);
            aplicarFiltrosLocales();
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
                actualizarCardExistente(orden);
            }
        }

        aplicarFiltrosLocales();
        actualizarContadores();
    }

    function actualizarCardExistente(orden) {
        const card = document.querySelector(`.lmd-kds-card[data-id="${orden.id}"]`);
        if (!card) return;
        const estado = calcularEstadoTiempo(orden);
        card.classList.remove('lmd-kds-card--fresh', 'lmd-kds-card--warn', 'lmd-kds-card--alert');
        card.classList.add(estado.colorClass);
        card.dataset.prioridad = estado.prioridad;
        card.dataset.tipoServicio = orden.tipoServicio || card.dataset.tipoServicio || '';
        card.dataset.tiempoPreparacionMin = orden.tiempoPreparacionMin || card.dataset.tiempoPreparacionMin || 15;

        const destino = document.getElementById(`kds-cards-${estado.columnaId}`);
        if (destino && card.parentElement !== destino) {
            card.remove();
            insertarOrdenada(destino, card);
        }
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
        document.querySelectorAll('.lmd-kds-station-btn').forEach(tab => {
            tab.classList.toggle('lmd-kds-station-btn--active', tab.dataset.estacion === estacion);
        });

        // Limpiar columnas
        COOKS.forEach(cook => {
            const container = document.getElementById(`kds-cards-${cook.id}`);
            if (container) container.innerHTML = '';
        });
        cargarOrdenesIniciales();
        conectarAEstacion(estacion);
    }

    function cambiarFiltroRapido(btn) {
        if (btn.dataset.prioridad) {
            prioridadActual = btn.dataset.prioridad;
            document.querySelectorAll('.lmd-kds-filter-btn[data-prioridad]').forEach(tab => {
                tab.classList.toggle('lmd-kds-filter-btn--active', tab === btn);
            });
        }

        if (btn.dataset.servicio) {
            servicioActual = servicioActual === btn.dataset.servicio ? 'Todos' : btn.dataset.servicio;
            document.querySelectorAll('.lmd-kds-filter-btn[data-servicio]').forEach(tab => {
                tab.classList.toggle('lmd-kds-filter-btn--active', tab.dataset.servicio === servicioActual);
            });
        }

        aplicarFiltrosLocales();
        actualizarContadores();
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
            (ordenes || [])
                .sort((a, b) => new Date(a.horaRecibido).getTime() - new Date(b.horaRecibido).getTime())
                .forEach(renderOrden);
            aplicarFiltrosLocales();
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
        document.querySelectorAll('.lmd-kds-station-btn').forEach(tab => {
            tab.addEventListener('click', () => cambiarEstacion(tab.dataset.estacion));
        });

        document.querySelectorAll('.lmd-kds-filter-btn').forEach(btn => {
            btn.addEventListener('click', () => cambiarFiltroRapido(btn));
        });

        // Tabs de columna para mobile
        renderColumnTabs();

        // Cargar ordenes iniciales y conectar SignalR
        cargarOrdenesIniciales();
        iniciarSignalR();

        // Timer updater cada 10s
        setInterval(actualizarTimers, 1000);

        // Clock
        const clock = document.getElementById('lmd-kds-reloj');
        if (clock) {
            setInterval(() => {
                clock.textContent = new Date().toLocaleTimeString('es-SV', { hour: '2-digit', minute: '2-digit' });
            }, 1000);
        }
    });

    // Exponer undo globalmente
    window.__lmdKdsUndo = recuperarUltimoListo;
})();
