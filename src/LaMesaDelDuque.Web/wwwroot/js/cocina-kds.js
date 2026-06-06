(function () {
    'use strict';

    var ESTACIONES = ['Parrilla', 'Fria', 'Caliente', 'Bar', 'Expo'];
    var stationToColumn = window.__lmdKdsStationMap || { Parrilla: 1, Fria: 2, Caliente: 3, Bar: 2, Expo: 1 };
    var ordenes = new Map();
    var estacionActual = 'Todas';
    var connection = null;
    var ultimoRemovido = null;

    function g(obj, camel, pascal, fallback) {
        if (!obj) return fallback;
        if (obj[camel] !== undefined && obj[camel] !== null) return obj[camel];
        if (obj[pascal] !== undefined && obj[pascal] !== null) return obj[pascal];
        return fallback;
    }

    function normalizarOrden(raw) {
        if (!raw) return null;
        var hora = g(raw, 'horaRecibido', 'HoraRecibido', null);
        return {
            id: String(g(raw, 'id', 'Id', '')),
            pedidoId: String(g(raw, 'pedidoId', 'PedidoId', '')),
            productoNombre: g(raw, 'productoNombre', 'ProductoNombre', 'Producto'),
            cantidad: Number(g(raw, 'cantidad', 'Cantidad', 1)) || 1,
            notas: g(raw, 'notas', 'Notas', '') || '',
            alergenos: g(raw, 'alergenos', 'Alergenos', '') || '',
            ingredientesQuitados: g(raw, 'ingredientesQuitados', 'IngredientesQuitados', '') || '',
            ingredientesExtra: g(raw, 'ingredientesExtra', 'IngredientesExtra', '') || '',
            cocineroId: Number(g(raw, 'cocineroId', 'CocineroId', 0)) || 0,
            estacion: g(raw, 'estacion', 'Estacion', 'Expo') || 'Expo',
            estado: g(raw, 'estado', 'Estado', 'Pendiente') || 'Pendiente',
            horaRecibido: hora,
            minutosTranscurridos: Number(g(raw, 'minutosTranscurridos', 'MinutosTranscurridos', 0)) || minutosDesde(hora),
            mesaNumero: g(raw, 'mesaNumero', 'MesaNumero', null),
            tipoServicio: g(raw, 'tipoServicio', 'TipoServicio', '') || '',
            curso: g(raw, 'curso', 'Curso', '') || '',
            productoId: String(g(raw, 'productoId', 'ProductoId', '')),
            tiempoPreparacionMin: Number(g(raw, 'tiempoPreparacionMin', 'TiempoPreparacionMin', 15)) || 15
        };
    }

    function minutosDesde(hora) {
        if (!hora) return 0;
        var d = new Date(hora);
        if (isNaN(d.getTime())) return 0;
        return Math.max(0, Math.floor((Date.now() - d.getTime()) / 60000));
    }

    function esc(v) {
        return String(v == null ? '' : v)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function toast(msg) {
        var t = document.createElement('div');
        t.className = 'lmd-kds-toast';
        t.textContent = msg;
        document.body.appendChild(t);
        setTimeout(function () { t.remove(); }, 2400);
    }

    function token() {
        var el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    function columnaParaOrden(o) {
        if (o.cocineroId) return o.cocineroId;
        return stationToColumn[o.estacion] || stationToColumn[String(o.estacion || '').normalize('NFD').replace(/[\u0300-\u036f]/g, '')] || 1;
    }

    function claseTiempo(min) {
        if (min >= 30) return 'lmd-kds-card--alert';
        if (min >= 15) return 'lmd-kds-card--warn';
        return 'lmd-kds-card--fresh';
    }

    function etiquetaMesa(o) {
        if (o.mesaNumero) return 'Mesa ' + esc(o.mesaNumero);
        if ((o.tipoServicio || '').toLowerCase().includes('llevar')) return 'Para llevar';
        return 'Sin mesa';
    }

    function renderCard(o) {
        var min = minutosDesde(o.horaRecibido) || o.minutosTranscurridos || 0;
        var mods = '';
        if (o.ingredientesQuitados) mods += '<li class="lmd-kds-mod lmd-kds-mod--quitar">Sin: ' + esc(o.ingredientesQuitados) + '</li>';
        if (o.ingredientesExtra) mods += '<li class="lmd-kds-mod lmd-kds-mod--extra">Extra: ' + esc(o.ingredientesExtra) + '</li>';
        var modsHtml = mods ? '<div class="lmd-kds-card__modificaciones"><ul class="lmd-kds-mods-list">' + mods + '</ul></div>' : '';
        var alergiasHtml = o.alergenos ? '<div class="lmd-kds-alergeno-banner">ALERGIA: ' + esc(o.alergenos) + '</div>' : '';
        var notasHtml = o.notas ? '<div class="lmd-kds-card__notas-block"><span class="lmd-kds-notas-label">NOTA</span>' + esc(o.notas) + '</div>' : '';
        var curso = o.curso ? '<span class="lmd-kds-card__curso">' + esc(o.curso) + '</span>' : '';
        return '' +
            '<article class="lmd-kds-card ' + claseTiempo(min) + '" id="kds-card-' + esc(o.id) + '" data-orden-id="' + esc(o.id) + '">' +
                alergiasHtml +
                '<div class="lmd-kds-card__header">' +
                    '<span class="lmd-kds-card__mesa">' + etiquetaMesa(o) + ' · ' + esc(o.estacion) + '</span>' +
                    '<span class="lmd-kds-card__timer">' + min + 'm</span>' +
                '</div>' +
                '<div class="lmd-kds-card__dish-row">' +
                    '<div class="lmd-kds-card__producto">' + esc(o.productoNombre) + curso + '</div>' +
                    '<div class="lmd-kds-card__cantidad">×' + esc(o.cantidad) + '</div>' +
                '</div>' +
                modsHtml + notasHtml +
                '<div class="lmd-kds-card__footer">' +
                    '<button class="lmd-kds-btn-listo" type="button" onclick="window.__lmdKdsMarcarListo(\'' + esc(o.id) + '\')">Listo</button>' +
                    (o.productoId ? '<button class="lmd-kds-btn-86" type="button" onclick="window.__lmdKdsMarcar86(\'' + esc(o.productoId) + '\')">86</button>' : '') +
                '</div>' +
            '</article>';
    }

    function limpiarColumnas() {
        [1, 2, 3].forEach(function (id) {
            var cont = document.getElementById('kds-cards-' + id);
            if (cont) cont.innerHTML = '';
        });
    }

    function renderTodas() {
        limpiarColumnas();
        var filtradas = Array.from(ordenes.values()).filter(function (o) {
            return estacionActual === 'Todas' || String(o.estacion).toLowerCase() === estacionActual.toLowerCase();
        });
        filtradas.sort(function (a, b) { return new Date(a.horaRecibido || 0) - new Date(b.horaRecibido || 0); });
        var counts = { 1: 0, 2: 0, 3: 0 };
        filtradas.forEach(function (o) {
            var col = columnaParaOrden(o);
            if (![1, 2, 3].includes(col)) col = 1;
            counts[col] += 1;
            var cont = document.getElementById('kds-cards-' + col);
            if (cont) cont.insertAdjacentHTML('beforeend', renderCard(o));
        });
        [1, 2, 3].forEach(function (id) {
            var el = document.getElementById('kds-count-' + id);
            if (el) el.textContent = counts[id] + ' ordenes';
        });
        var total = document.getElementById('lmd-kds-contador');
        if (total) total.textContent = filtradas.length + ' ordenes';
    }

    async function cargarOrdenes() {
        try {
            var res = await fetch('?handler=OrdenesJson&estacion=' + encodeURIComponent(estacionActual), {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            if (!res.ok) return;
            var data = await res.json();
            var lista = Array.isArray(data) ? data : (data.ordenesCocina || data.OrdenesCocina || []);
            ordenes.clear();
            lista.map(normalizarOrden).filter(Boolean).forEach(function (o) { ordenes.set(o.id, o); });
            renderTodas();
            setOffline(false);
        } catch (e) {
            setOffline(true);
        }
    }

    function setOffline(on) {
        var badge = document.getElementById('lmd-kds-offline-badge');
        if (badge) badge.style.display = on ? 'inline-flex' : 'none';
    }

    async function conectarAGrupos() {
        if (!connection) return;
        var grupos = estacionActual === 'Todas' ? ESTACIONES : [estacionActual];
        await Promise.all(grupos.map(function (estacion) {
            return connection.invoke('UnirseAGrupo', 'cocina-' + estacion).catch(function () {});
        }));
    }

    function initSignalR() {
        if (!window.signalR) return;
        connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/pedidos')
            .withAutomaticReconnect()
            .build();

        connection.on('NuevaOrden', function (orden) {
            var o = normalizarOrden(orden);
            if (!o) return;
            ordenes.set(o.id, o);
            renderTodas();
        });
        connection.on('ItemListo', function (ordenId) {
            ordenes.delete(String(ordenId));
            renderTodas();
        });
        connection.on('ItemRecuperado', function (orden) {
            var o = normalizarOrden(orden);
            if (!o) return;
            ordenes.set(o.id, o);
            renderTodas();
        });

        connection.start()
            .then(function () { return conectarAGrupos(); })
            .then(cargarOrdenes)
            .catch(function () { setOffline(true); });
    }

    async function marcarListo(ordenId) {
        var card = document.getElementById('kds-card-' + ordenId);
        if (card) card.classList.add('lmd-kds-card--completing');
        var actual = ordenes.get(String(ordenId));
        try {
            var form = new FormData();
            form.append('__RequestVerificationToken', token());
            form.append('ordenId', ordenId);
            var res = await fetch('?handler=MarcarListoJson', {
                method: 'POST',
                body: form,
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            var data = await res.json().catch(function () { return {}; });
            if (!res.ok || data.ok === false) throw new Error(data.error || 'Error al marcar listo');
            ultimoRemovido = actual || null;
            ordenes.delete(String(ordenId));
            renderTodas();
        } catch (e) {
            if (card) card.classList.remove('lmd-kds-card--completing');
            toast(e.message || 'Error de conexión');
        }
    }

    async function marcar86(productoId) {
        try {
            var res = await fetch('?handler=Marcar86Json', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token(),
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({ productoId: productoId })
            });
            var data = await res.json().catch(function () { return {}; });
            if (!res.ok || data.ok === false) throw new Error(data.error || 'No se pudo marcar 86');
            toast((data.nombre || 'Producto') + ' marcado 86');
        } catch (e) {
            toast(e.message || 'Error de conexión');
        }
    }

    function cambiarEstacion(btn) {
        document.querySelectorAll('.lmd-kds-station-btn').forEach(function (b) { b.classList.remove('lmd-kds-station-btn--active'); });
        btn.classList.add('lmd-kds-station-btn--active');
        estacionActual = btn.dataset.estacion || 'Todas';
        conectarAGrupos().finally(cargarOrdenes);
    }

    function actualizarReloj() {
        var r = document.getElementById('lmd-kds-reloj');
        if (r) r.textContent = new Date().toLocaleTimeString('es-SV', { hour: '2-digit', minute: '2-digit' });
        renderTodas();
    }

    window.__lmdKdsMarcarListo = marcarListo;
    window.__lmdKdsMarcar86 = marcar86;
    window.__lmdKdsUndo = function () {
        if (!ultimoRemovido) { toast('Nada para deshacer'); return; }
        ordenes.set(ultimoRemovido.id, ultimoRemovido);
        ultimoRemovido = null;
        renderTodas();
    };

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('.lmd-kds-station-btn').forEach(function (btn) {
            btn.addEventListener('click', function () { cambiarEstacion(btn); });
        });
        actualizarReloj();
        setInterval(actualizarReloj, 30000);
        cargarOrdenes();
        initSignalR();
        setInterval(cargarOrdenes, 15000);
    });
})();
