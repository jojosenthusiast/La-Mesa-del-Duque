/* ═══════════════════════════════════════════════════════
   POS — La Mesa del Duque  v4.0
   Workflow: Selección → Productos (overlays de pago / docs)
   ═══════════════════════════════════════════════════════ */

(function () {
    // ── Lucide SVG helper ──────────────────────────────
    // Íconos lucide embebidos (mismo origen, sin CORS). lucide-static v1.x — ISC.
    var LMD_ICONOS = {
        'alert-circle': '<circle cx="12" cy="12" r="10" /><line x1="12" x2="12" y1="8" y2="12" /><line x1="12" x2="12.01" y1="16" y2="16" />',
        'alert-triangle': '<path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3" /><path d="M12 9v4" /><path d="M12 17h.01" />',
        'arrow-left': '<path d="m12 19-7-7 7-7" /><path d="M19 12H5" />',
        'arrow-right': '<path d="M5 12h14" /><path d="m12 5 7 7-7 7" />',
        'banknote': '<rect width="20" height="12" x="2" y="6" rx="2" /><circle cx="12" cy="12" r="2" /><path d="M6 12h.01M18 12h.01" />',
        'building-2': '<path d="M10 12h4" /><path d="M10 8h4" /><path d="M14 21v-3a2 2 0 0 0-4 0v3" /><path d="M6 10H4a2 2 0 0 0-2 2v7a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2h-2" /><path d="M6 21V5a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v16" />',
        'cake-slice': '<path d="M16 13H3" /><path d="M16 17H3" /><path d="m7.2 7.9-3.388 2.5A2 2 0 0 0 3 12.01V20a1 1 0 0 0 1 1h16a1 1 0 0 0 1-1v-8.654c0-2-2.44-6.026-6.44-8.026a1 1 0 0 0-1.082.057L10.4 5.6" /><circle cx="9" cy="7" r="2" />',
        'check': '<path d="M20 6 9 17l-5-5" />',
        'check-circle': '<path d="M21.801 10A10 10 0 1 1 17 3.335" /><path d="m9 11 3 3L22 4" />',
        'clock': '<circle cx="12" cy="12" r="10" /><path d="M12 6v6l4 2" />',
        'credit-card': '<rect width="20" height="14" x="2" y="5" rx="2" /><line x1="2" x2="22" y1="10" y2="10" />',
        'delete': '<path d="M10 5a2 2 0 0 0-1.344.519l-6.328 5.74a1 1 0 0 0 0 1.481l6.328 5.741A2 2 0 0 0 10 19h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2z" /><path d="m12 9 6 6" /><path d="m18 9-6 6" />',
        'edit-3': '<path d="M13 21h8" /><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z" />',
        'equal': '<line x1="5" x2="19" y1="9" y2="9" /><line x1="5" x2="19" y1="15" y2="15" />',
        'file-check': '<path d="M6 22a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h8a2.4 2.4 0 0 1 1.704.706l3.588 3.588A2.4 2.4 0 0 1 20 8v12a2 2 0 0 1-2 2z" /><path d="M14 2v5a1 1 0 0 0 1 1h5" /><path d="m9 15 2 2 4-4" />',
        'file-minus': '<path d="M6 22a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h8a2.4 2.4 0 0 1 1.704.706l3.588 3.588A2.4 2.4 0 0 1 20 8v12a2 2 0 0 1-2 2z" /><path d="M14 2v5a1 1 0 0 0 1 1h5" /><path d="M9 15h6" />',
        'file-text': '<path d="M6 22a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h8a2.4 2.4 0 0 1 1.704.706l3.588 3.588A2.4 2.4 0 0 1 20 8v12a2 2 0 0 1-2 2z" /><path d="M14 2v5a1 1 0 0 0 1 1h5" /><path d="M10 9H8" /><path d="M16 13H8" /><path d="M16 17H8" />',
        'gift': '<path d="M12 7v14" /><path d="M20 11v8a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2v-8" /><path d="M7.5 7a1 1 0 0 1 0-5A4.8 8 0 0 1 12 7a4.8 8 0 0 1 4.5-5 1 1 0 0 1 0 5" /><rect x="3" y="7" width="18" height="4" rx="1" />',
        'git-branch': '<path d="M15 6a9 9 0 0 0-9 9V3" /><circle cx="18" cy="6" r="3" /><circle cx="6" cy="18" r="3" />',
        'git-merge': '<circle cx="18" cy="18" r="3" /><circle cx="6" cy="6" r="3" /><path d="M6 21V9a9 9 0 0 0 9 9" />',
        'hash': '<line x1="4" x2="20" y1="9" y2="9" /><line x1="4" x2="20" y1="15" y2="15" /><line x1="10" x2="8" y1="3" y2="21" /><line x1="16" x2="14" y1="3" y2="21" />',
        'heart': '<path d="M2 9.5a5.5 5.5 0 0 1 9.591-3.676.56.56 0 0 0 .818 0A5.49 5.49 0 0 1 22 9.5c0 2.29-1.5 4-3 5.5l-5.492 5.313a2 2 0 0 1-3 .019L5 15c-1.5-1.5-3-3.2-3-5.5" />',
        'layers': '<path d="M12.83 2.18a2 2 0 0 0-1.66 0L2.6 6.08a1 1 0 0 0 0 1.83l8.58 3.91a2 2 0 0 0 1.66 0l8.58-3.9a1 1 0 0 0 0-1.83z" /><path d="M2 12a1 1 0 0 0 .58.91l8.6 3.91a2 2 0 0 0 1.65 0l8.58-3.9A1 1 0 0 0 22 12" /><path d="M2 17a1 1 0 0 0 .58.91l8.6 3.91a2 2 0 0 0 1.65 0l8.58-3.9A1 1 0 0 0 22 17" />',
        'list': '<path d="M3 5h.01" /><path d="M3 12h.01" /><path d="M3 19h.01" /><path d="M8 5h13" /><path d="M8 12h13" /><path d="M8 19h13" />',
        'mail': '<path d="m22 7-8.991 5.727a2 2 0 0 1-2.009 0L2 7" /><rect x="2" y="4" width="20" height="16" rx="2" />',
        'message-square': '<path d="M22 17a2 2 0 0 1-2 2H6.828a2 2 0 0 0-1.414.586l-2.202 2.202A.71.71 0 0 1 2 21.286V5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2z" />',
        'minus': '<path d="M5 12h14" />',
        'minus-circle': '<circle cx="12" cy="12" r="10" /><path d="M8 12h8" />',
        'package': '<path d="M11 21.73a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73z" /><path d="M12 22V12" /><polyline points="3.29 7 12 12 20.71 7" /><path d="m7.5 4.27 9 5.15" />',
        'plus': '<path d="M5 12h14" /><path d="M12 5v14" />',
        'plus-circle': '<circle cx="12" cy="12" r="10" /><path d="M8 12h8" /><path d="M12 8v8" />',
        'printer': '<path d="M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2" /><path d="M6 9V3a1 1 0 0 1 1-1h10a1 1 0 0 1 1 1v6" /><rect x="6" y="14" width="12" height="8" rx="1" />',
        'qr-code': '<rect width="5" height="5" x="3" y="3" rx="1" /><rect width="5" height="5" x="16" y="3" rx="1" /><rect width="5" height="5" x="3" y="16" rx="1" /><path d="M21 16h-3a2 2 0 0 0-2 2v3" /><path d="M21 21v.01" /><path d="M12 7v3a2 2 0 0 1-2 2H7" /><path d="M3 12h.01" /><path d="M12 3h.01" /><path d="M12 16v.01" /><path d="M16 12h1" /><path d="M21 12v.01" /><path d="M12 21v-1" />',
        'receipt': '<path d="M12 17V7" /><path d="M16 8h-6a2 2 0 0 0 0 4h4a2 2 0 0 1 0 4H8" /><path d="M4 3a1 1 0 0 1 1-1 1.3 1.3 0 0 1 .7.2l.933.6a1.3 1.3 0 0 0 1.4 0l.934-.6a1.3 1.3 0 0 1 1.4 0l.933.6a1.3 1.3 0 0 0 1.4 0l.933-.6a1.3 1.3 0 0 1 1.4 0l.934.6a1.3 1.3 0 0 0 1.4 0l.933-.6A1.3 1.3 0 0 1 19 2a1 1 0 0 1 1 1v18a1 1 0 0 1-1 1 1.3 1.3 0 0 1-.7-.2l-.933-.6a1.3 1.3 0 0 0-1.4 0l-.934.6a1.3 1.3 0 0 1-1.4 0l-.933-.6a1.3 1.3 0 0 0-1.4 0l-.933.6a1.3 1.3 0 0 1-1.4 0l-.934-.6a1.3 1.3 0 0 0-1.4 0l-.933.6a1.3 1.3 0 0 1-.7.2 1 1 0 0 1-1-1z" />',
        'refresh-cw': '<path d="M3 12a9 9 0 0 1 9-9 9.75 9.75 0 0 1 6.74 2.74L21 8" /><path d="M21 3v5h-5" /><path d="M21 12a9 9 0 0 1-9 9 9.75 9.75 0 0 1-6.74-2.74L3 16" /><path d="M8 16H3v5" />',
        'rotate-ccw': '<path d="M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8" /><path d="M3 3v5h5" />',
        'send': '<path d="M14.536 21.686a.5.5 0 0 0 .937-.024l6.5-19a.496.496 0 0 0-.635-.635l-19 6.5a.5.5 0 0 0-.024.937l7.93 3.18a2 2 0 0 1 1.112 1.11z" /><path d="m21.854 2.147-10.94 10.939" />',
        'share-2': '<circle cx="18" cy="5" r="3" /><circle cx="6" cy="12" r="3" /><circle cx="18" cy="19" r="3" /><line x1="8.59" x2="15.42" y1="13.51" y2="17.49" /><line x1="15.41" x2="8.59" y1="6.51" y2="10.49" />',
        'shopping-bag': '<path d="M16 10a4 4 0 0 1-8 0" /><path d="M3.103 6.034h17.794" /><path d="M3.4 5.467a2 2 0 0 0-.4 1.2V20a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6.667a2 2 0 0 0-.4-1.2l-2-2.667A2 2 0 0 0 17 2H7a2 2 0 0 0-1.6.8z" />',
        'shopping-cart': '<circle cx="8" cy="21" r="1" /><circle cx="19" cy="21" r="1" /><path d="M2.05 2.05h2l2.66 12.42a2 2 0 0 0 2 1.58h9.78a2 2 0 0 0 1.95-1.57l1.65-7.43H5.12" />',
        'shuffle': '<path d="m18 14 4 4-4 4" /><path d="m18 2 4 4-4 4" /><path d="M2 18h1.973a4 4 0 0 0 3.3-1.7l5.454-8.6a4 4 0 0 1 3.3-1.7H22" /><path d="M2 6h1.972a4 4 0 0 1 3.6 2.2" /><path d="M22 18h-6.041a4 4 0 0 1-3.3-1.8l-.359-.45" />',
        'tag': '<path d="M12.586 2.586A2 2 0 0 0 11.172 2H4a2 2 0 0 0-2 2v7.172a2 2 0 0 0 .586 1.414l8.704 8.704a2.426 2.426 0 0 0 3.42 0l6.58-6.58a2.426 2.426 0 0 0 0-3.42z" /><circle cx="7.5" cy="7.5" r=".5" fill="currentColor" />',
        'ticket': '<path d="M2 9a3 3 0 0 1 0 6v2a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-2a3 3 0 0 1 0-6V7a2 2 0 0 0-2-2H4a2 2 0 0 0-2 2Z" /><path d="M13 5v2" /><path d="M13 17v2" /><path d="M13 11v2" />',
        'timer': '<line x1="10" x2="14" y1="2" y2="2" /><line x1="12" x2="15" y1="14" y2="11" /><circle cx="12" cy="14" r="8" />',
        'user': '<path d="M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2" /><circle cx="12" cy="7" r="4" />',
        'user-check': '<path d="m16 11 2 2 4-4" /><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" /><circle cx="9" cy="7" r="4" />',
        'users': '<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" /><path d="M16 3.128a4 4 0 0 1 0 7.744" /><path d="M22 21v-2a4 4 0 0 0-3-3.87" /><circle cx="9" cy="7" r="4" />',
        'utensils': '<path d="M3 2v7c0 1.1.9 2 2 2h4a2 2 0 0 0 2-2V2" /><path d="M7 2v20" /><path d="M21 15V2a5 5 0 0 0-5 5v6c0 1.1.9 2 2 2h3Zm0 0v7" />',
        'utensils-crossed': '<path d="m16 2-2.3 2.3a3 3 0 0 0 0 4.2l1.8 1.8a3 3 0 0 0 4.2 0L22 8" /><path d="M15 15 3.3 3.3a4.2 4.2 0 0 0 0 6l7.3 7.3c.7.7 2 .7 2.8 0L15 15Zm0 0 7 7" /><path d="m2.1 21.8 6.4-6.3" /><path d="m19 5-7 7" />',
        'wine': '<path d="M8 22h8" /><path d="M7 10h10" /><path d="M12 15v7" /><path d="M12 15a5 5 0 0 0 5-5c0-2-.5-4-2-8H9c-1.5 4-2 6-2 8a5 5 0 0 0 5 5Z" />',
        'x': '<path d="M18 6 6 18" /><path d="m6 6 12 12" />',
        'x-circle': '<circle cx="12" cy="12" r="10" /><path d="m15 9-6 6" /><path d="m9 9 6 6" />'
    };

    function icon(name, cls) {
        var inner = LMD_ICONOS[name] || '';
        return '<svg class="lmd-icon ' + (cls || '') + '" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">' + inner + '</svg>';
    }

    // ── Helpers ─────────────────────────────────────────
    function fmt(n) {
        return new Intl.NumberFormat('es-SV', { style: 'currency', currency: 'USD' }).format(n || 0);
    }
    function totalLineas(lineas) {
        return lineas.reduce(function (s, l) { return s + (l.precioUnitario || 0) * (l.cantidad || 0); }, 0);
    }
    function detallesServidor() {
        return state && state.pedidoActual && Array.isArray(state.pedidoActual.detalles)
            ? state.pedidoActual.detalles
            : [];
    }
    function totalPedidoActual() {
        var totalServidor = state && state.pedidoActual && !isNaN(Number(state.pedidoActual.total))
            ? Number(state.pedidoActual.total)
            : 0;
        return totalServidor + totalLineas(state ? state.lineas : []);
    }
    function htmlEscape(value) {
        return String(value || '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }
    function jsString(value) {
        return String(value || '').replace(/\\/g, '\\\\').replace(/'/g, "\\'");
    }

    function productoPorId(id) {
        return (window.__lmdProductosDisponibles || []).find(function (p) { return p.id === id; });
    }
    function productoRequiereConfirmacion(p) { return !!(p && (p.tieneReceta || p.TieneReceta)); }
    function crearConfirmacionOriginal() {
        return JSON.stringify([{
            ingredienteId: '00000000-0000-0000-0000-000000000000',
            ingredienteNombre: 'Original confirmado',
            accion: 'confirmado',
            motivo: 'original',
            ingredienteReemplazoId: null,
            ingredienteReemplazoNombre: null
        }]);
    }
    function lineaConfirmada(linea) {
        if (!linea) return false;
        var producto = productoPorId(linea.productoId);
        if (!productoRequiereConfirmacion(producto)) return true;
        return !!(linea.ingredientesConfirmados || linea.modificacionesJson);
    }
    function validarLineasConfirmadas() {
        var pendientes = state.lineas.filter(function (l) { return !lineaConfirmada(l); });
        if (pendientes.length === 0) return true;
        lmdToast('Confirma los ingredientes de: ' + pendientes.map(function (l) { return l.productoNombre; }).join(', '), 'error');
        abrirModificadores(pendientes[0].productoId);
        return false;
    }

    // ── State ──────────────────────────────────────────
    const state = {
        pantalla: 'seleccion',     // seleccion | productos
        tipoServicio: null,
        mesaId: null,
        mesaNumero: null,
        entrega: { direccion: '', telefono: '', repartidorId: '' },
        lineas: [],
        pedidoActual: null,        // { id }
        pagado: false,
        pagoMetodo: null,
        pagoMonto: null,
        pagoReferencia: null,
        // split payment
        split: {
            activo: false,
            personas: [],          // [{ id, nombre, items: [], metodoPago, monto, pagado }]
            personaActual: 0
        }
    };

    const SHELL = document.getElementById('lmd-pos-contenido');
    let connection = null;
    let keypadValue = '0';
    let _creandoPedido = false;
    let _pagoEnProceso = false;

    // ── Screen machine ──────────────────────────────────
    function mostrarPantalla(nombre) {
        state.pantalla = nombre;
        document.querySelectorAll('.lmd-pos-screen').forEach(function (s) {
            s.classList.remove('lmd-pos-screen--activa');
        });
        var el = document.getElementById('lmd-pos-screen-' + nombre);
        if (el) el.classList.add('lmd-pos-screen--activa');

        // El botón superior de volver en Caja solo debe aparecer dentro de una orden.
        var back = document.getElementById('lmd-pos-back-btn');
        if (back) back.style.display = nombre === 'productos' ? 'inline-flex' : 'none';
    }

    // ── Overlay system ──────────────────────────────────
    function abrirOverlay(id, html, opts) {
        cerrarOverlay(id);
        opts = opts || {};
        var ov = document.createElement('div');
        ov.id = 'lmd-ov-' + id;
        ov.className = 'lmd-pos-overlay' + (opts.bottom ? ' lmd-pos-overlay--bottom' : '');
        if (opts.closeOnBackdrop !== false) {
            ov.addEventListener('click', function (e) {
                if (e.target === ov) cerrarOverlay(id);
            });
        }
        var panel = document.createElement('div');
        panel.className = 'lmd-pos-ov-panel' + (opts.wide ? ' lmd-pos-ov-panel--wide' : '') + (opts.bottom ? ' lmd-pos-ov-panel--bottom' : '');
        panel.innerHTML = html;
        ov.appendChild(panel);
        document.body.appendChild(ov);
        // animate in
        requestAnimationFrame(function () { ov.classList.add('lmd-pos-overlay--visible'); });
    }

    function cerrarOverlay(id) {
        var el = document.getElementById('lmd-ov-' + id);
        if (el) el.remove();
    }

    function cerrarTodasOverlaysPago() {
        ['pago', 'efectivo', 'tarjeta', 'qr', 'otro', 'errorpago', 'documentos', 'split', 'splitdetalle'].forEach(cerrarOverlay);
    }

    // Quita cualquier capa de pantalla completa que pueda quedar huérfana
    // bloqueando los clics (ticket post-pago o diálogo de confirmación).
    function limpiarCapasHuerfanas() {
        document.querySelectorAll('.lmd-ticket-modal-backdrop, .lmd-modal-overlay').forEach(function (el) {
            el.remove();
        });
    }

    // ═══════════════════════════════════════════════════
    // SCREEN 1 — Selección
    // ═══════════════════════════════════════════════════
    function renderSeleccion() {
        var mesas = window.__lmdMesasDisponibles || [];
        var pedidosSinMesa = window.__lmdPedidosSinMesaActivos || [];
        // Group by capacity, sort ascending within each group
        var grupos = {};
        mesas.forEach(function (m) {
            var cap = m.capacidad || 0;
            if (!grupos[cap]) grupos[cap] = [];
            grupos[cap].push(m);
        });
        var caps = Object.keys(grupos).map(Number).sort(function (a, b) { return a - b; });
        var mesasHtml = '';
        caps.forEach(function (cap) {
            mesasHtml += '<div class="lmd-pos-mesa-zona-separator">' + cap + ' personas</div>';
            grupos[cap].forEach(function (m) {
                var disponible = m.estado === 'Disponible';
                var enGracia = disponible && m.graciaHasta && new Date(m.graciaHasta) > Date.now();
                var hayTab = !disponible && m.pedidoActualId;
                var enCobro = hayTab && (m.pedidoEstado === 'EnCobro' || m.pedidoEstado === 'Listo');
                var cls = enGracia ? 'lmd-pos-mesa-card--en-gracia'
                    : disponible ? 'lmd-pos-mesa-card--disponible'
                    : enCobro ? 'lmd-pos-mesa-card--en-cobro'
                    : 'lmd-pos-mesa-card--ocupada';
                var onclick = enGracia ? '' : ' onclick="pos.seleccionarMesa(\'' + m.id + '\',' + m.numero + ')"';
                var badgeHtml = '';
                if (enGracia) {
                    var secsLeft = Math.max(0, Math.floor((new Date(m.graciaHasta).getTime() - Date.now()) / 1000));
                    var mins = Math.floor(secsLeft / 60), secs = secsLeft % 60;
                    badgeHtml = '<span class="lmd-pos-mesa-card__gracia-badge" data-gracia-hasta="' + m.graciaHasta + '">' +
                        icon('timer') + ' <span class="lmd-gracia-tiempo">' + mins + ':' + (secs < 10 ? '0' : '') + secs + '</span></span>';
                } else if (enCobro) {
                    badgeHtml = '<span class="lmd-pos-mesa-card__cobrar-badge">' + icon('receipt') + ' Cobrar</span>';
                } else if (hayTab) {
                    var fechaAttr = m.pedidoFechaCreacion ? ' data-pedido-fecha="' + m.pedidoFechaCreacion + '"' : '';
                    var minTab = m.pedidoFechaCreacion ? Math.floor((Date.now() - new Date(m.pedidoFechaCreacion).getTime()) / 60000) : 0;
                    var tiempoLabel = minTab > 0 ? minTab + ' min' : 'Tab';
                    badgeHtml = '<span class="lmd-pos-mesa-card__tab-badge"' + fechaAttr + '>' + icon('clock') + ' <span class="lmd-tab-tiempo">' + tiempoLabel + '</span></span>';
                }
                mesasHtml += '<div class="lmd-pos-mesa-card ' + cls + '"' + onclick + '>' +
                    '<span class="lmd-pos-mesa-card__numero">' + m.numero + '</span>' +
                    '<span class="lmd-pos-mesa-card__capacidad">' + m.capacidad + ' pax</span>' +
                    badgeHtml +
                    (m.zona ? '<span class="lmd-pos-mesa-card__zona">' + m.zona + '</span>' : '') +
                '</div>';
            });
        });

        var pedidosSinMesaHtml = pedidosSinMesa.length > 0
            ? '<div class="lmd-pos-offpremise-list">' +
                '<div class="lmd-pos-offpremise-list__title">Pedidos abiertos</div>' +
                pedidosSinMesa.map(function (p) {
                    var tipo = p.tipoServicio === 'Domicilio' ? 'Delivery' : 'Retiro';
                    var estado = p.estado || 'Activo';
                    var cobrar = estado === 'Listo' || estado === 'EnCobro';
                    var sub = p.tipoServicio === 'Domicilio'
                        ? (p.direccionEntrega || 'Sin dirección')
                        : 'Retiro en caja';
                    var fechaAttr = p.fechaCreacion ? ' data-pedido-fecha="' + p.fechaCreacion + '"' : '';
                    return '<button type="button" class="lmd-pos-offpremise-card' + (cobrar ? ' lmd-pos-offpremise-card--cobrar' : '') + '" onclick="pos.retomarPedidoSinMesa(\'' + jsString(p.id) + '\')">' +
                        '<span class="lmd-pos-offpremise-card__main">' + icon(p.tipoServicio === 'Domicilio' ? 'truck' : 'package') + ' ' + tipo + '</span>' +
                        '<span class="lmd-pos-offpremise-card__sub">' + htmlEscape(sub) + '</span>' +
                        '<span class="lmd-pos-offpremise-card__meta">' + fmt(p.total || 0) + ' · <span' + fechaAttr + '><span class="lmd-tab-tiempo">' + htmlEscape(estado) + '</span></span></span>' +
                        '<span class="lmd-pos-offpremise-card__badge">' + (cobrar ? 'Cobrar' : htmlEscape(estado)) + '</span>' +
                    '</button>';
                }).join('') +
              '</div>'
            : '';

        var html = '<div class="lmd-pos-seleccion">' +
            '<div class="lmd-pos-seleccion__mitad lmd-pos-seleccion__comer-aqui">' +
                '<div class="lmd-pos-seleccion__header">' + icon('utensils-crossed') + ' Comer aquí</div>' +
                '<div class="lmd-pos-mesas-grid">' + (mesasHtml || '<div class="lmd-pos-empty">Sin mesas disponibles</div>') + '</div>' +
            '</div>' +
            '<div class="lmd-pos-seleccion__mitad lmd-pos-seleccion__para-llevar">' +
                '<div class="lmd-pos-seleccion__header">' + icon('package') + ' Para llevar / delivery</div>' +
                pedidosSinMesaHtml +
                '<div class="lmd-pos-para-llevar-card">' +
                    '<div class="lmd-pos-para-llevar-card__icon">' + icon('shopping-bag') + '</div>' +
                    '<div class="lmd-pos-para-llevar-card__titulo">Pedido sin mesa</div>' +
                    '<div class="lmd-pos-para-llevar-card__sub">Usalo para retiro en caja o configurá envío a domicilio.</div>' +
                    '<div class="d-grid gap-2 mt-3">' +
                        '<button type="button" class="btn lmd-pos-service-btn lmd-pos-service-btn--takeout" onclick="pos.seleccionarParaLlevar()">Retiro en caja</button>' +
                        '<button type="button" class="btn lmd-pos-service-btn lmd-pos-service-btn--delivery" onclick="pos.seleccionarDelivery()">Configurar delivery</button>' +
                    '</div>' +
                '</div>' +
            '</div>' +
        '</div>';

        SHELL.innerHTML = '<div class="lmd-pos-shell">' +
            '<div class="lmd-pos-screen lmd-pos-screen--activa" id="lmd-pos-screen-seleccion">' + html + '</div>' +
            '<div class="lmd-pos-screen" id="lmd-pos-screen-productos"></div>' +
        '</div>';
    }

    function seleccionarMesa(mesaId, numero) {
        var mesas = window.__lmdMesasDisponibles || [];
        var m = mesas.find(function (x) { return x.id === mesaId; });
        if (m && m.estado !== 'Disponible') {
            if (m.pedidoActualId) {
                // Cualquier tab activo (pendiente, en preparación, listo o en cobro)
                // va DIRECTO al pago en un solo toque. Si se necesita agregar más
                // items, se cierra el overlay de pago (X) y queda la pantalla de productos.
                cobrarMesaDirecto(mesaId, numero, m.pedidoActualId, m.pedidoTotal || 0);
            } else { lmdToast('Mesa ocupada — selecciona otra', 'error'); }
            return;
        }
        state.tipoServicio = 'ComerAqui';
        state.mesaId = mesaId;
        state.mesaNumero = numero;
        state.entrega = { direccion: '', telefono: '', repartidorId: '' };
        _resetPedido();
        renderProductos();
        mostrarPantalla('productos');
    }

    async function retomarTab(mesaId, mesaNumero, pedidoId, tabTotal) {
        var ok = await window.lmdConfirm('Mesa ' + mesaNumero + ' tiene un tab activo (' + fmt(tabTotal) + '). ¿Retomar?');
        if (!ok) return;
        state.tipoServicio = 'ComerAqui';
        state.mesaId = mesaId;
        state.mesaNumero = mesaNumero;
        state.entrega = { direccion: '', telefono: '', repartidorId: '' };
        state.lineas = [];
        state.pedidoActual = { id: pedidoId, total: tabTotal, detalles: [] };
        state.pagado = false;
        state.pagoMetodo = null;
        state.pagoMonto = null;
        state.pagoReferencia = null;
        state.split = { activo: false, personas: [], personaActual: 0 };
        var csrf = document.querySelector('input[name="__RequestVerificationToken"]');
        var form = new FormData();
        form.append('__RequestVerificationToken', csrf ? csrf.value : '');
        form.append('pedidoId', pedidoId);
        try {
            await fetch('?handler=MarcarEnCobroJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        } catch (e) { /* continúa aunque falle el server */ }
        await cargarDetallesPedidoActual();
        renderProductos();
        mostrarPantalla('productos');
        lmdToast('Tab retomado — Mesa ' + mesaNumero, 'success');
    }

    function cobrarParaLlevar(pedidoId, tabTotal) {
        state.tipoServicio = 'ParaLlevar';
        state.mesaId = null;
        state.mesaNumero = null;
        state.lineas = [];
        state.pedidoActual = { id: pedidoId, total: tabTotal, detalles: [] };
        state.pagado = false;
        state.pagoMetodo = null;
        state.pagoMonto = null;
        state.pagoReferencia = null;
        state.propinaMonto = 0;
        state.split = { activo: false, personas: [], personaActual: 0 };
        keypadValue = '0';
        renderProductos();
        mostrarPantalla('productos');
        abrirOverlayPago();
    }

    function cobrarMesaDirecto(mesaId, mesaNumero, pedidoId, tabTotal) {
        state.tipoServicio = 'ComerAqui';
        state.mesaId = mesaId;
        state.mesaNumero = mesaNumero;
        state.entrega = { direccion: '', telefono: '', repartidorId: '' };
        state.lineas = [];
        state.pedidoActual = { id: pedidoId, total: tabTotal, detalles: [] };
        state.pagado = false;
        state.pagoMetodo = null;
        state.pagoMonto = null;
        state.pagoReferencia = null;
        state.propinaMonto = 0;
        state.split = { activo: false, personas: [], personaActual: 0 };
        keypadValue = '0';
        cargarDetallesPedidoActual().finally(function () {
            renderProductos();
            mostrarPantalla('productos');
            abrirOverlayPago();
        });
    }

    async function retomarPedidoSinMesa(pedidoId) {
        var pedidos = window.__lmdPedidosSinMesaActivos || [];
        var p = pedidos.find(function (x) { return String(x.id) === String(pedidoId); }) || { id: pedidoId, tipoServicio: 'ParaLlevar', total: 0 };
        state.tipoServicio = p.tipoServicio === 'Domicilio' ? 'Domicilio' : 'ParaLlevar';
        state.mesaId = null;
        state.mesaNumero = null;
        state.entrega = {
            direccion: p.direccionEntrega || '',
            telefono: p.telefonoCliente || '',
            repartidorId: p.repartidorId || ''
        };
        state.lineas = [];
        state.pedidoActual = { id: p.id || pedidoId, total: Number(p.total || 0), detalles: p.detalles || [], estado: p.estado || '' };
        state.pagado = false;
        state.pagoMetodo = null;
        state.pagoMonto = null;
        state.pagoReferencia = null;
        state.propinaMonto = 0;
        state.split = { activo: false, personas: [], personaActual: 0 };
        keypadValue = '0';
        await cargarDetallesPedidoActual();
        renderProductos();
        mostrarPantalla('productos');
        lmdToast((p.estado === 'Listo' || p.estado === 'EnCobro') ? 'Pedido listo para cobrar.' : 'Pedido retomado. Podés agregar más productos o cobrar.', 'success');
    }

    async function cargarDetallesPedidoActual() {
        if (!state.pedidoActual || !state.pedidoActual.id) return;
        try {
            var r = await fetch('?handler=DetallesPedidoJson&pedidoId=' + encodeURIComponent(state.pedidoActual.id), { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!r.ok) return;
            var d = await r.json();
            state.pedidoActual.detalles = d.detalles || [];
            if (d.total != null) state.pedidoActual.total = Number(d.total || 0);
        } catch (e) {}
    }

    function seleccionarParaLlevar() {
        state.tipoServicio = 'ParaLlevar';
        state.mesaId = null;
        state.mesaNumero = null;
        state.entrega = { direccion: '', telefono: '', repartidorId: '' };
        _resetPedido();
        renderProductos();
        mostrarPantalla('productos');
    }

    function seleccionarDelivery() {
        state.tipoServicio = 'Domicilio';
        state.mesaId = null;
        state.mesaNumero = null;
        state.entrega = { direccion: '', telefono: '', repartidorId: '' };
        _resetPedido();
        renderProductos();
        mostrarPantalla('productos');
        abrirConfigEnvio(true);
    }

    function servicioLabel() {
        if (state.tipoServicio === 'ComerAqui') return 'Mesa ' + state.mesaNumero;
        if (state.tipoServicio === 'Domicilio') return 'Delivery';
        return 'Para llevar';
    }

    function entregaResumenHtml() {
        if (state.tipoServicio !== 'Domicilio') return '';
        var dir = (state.entrega && state.entrega.direccion) ? state.entrega.direccion : 'Sin dirección';
        return '<span class="lmd-pos-delivery-badge">' + icon('truck') + ' ' + dir + '</span>';
    }

    function _resetPedido() {
        state.lineas = [];
        state.pedidoActual = null;
        state.pagado = false;
        state.pagoMetodo = null;
        state.pagoMonto = null;
        state.pagoReferencia = null;
        state.propinaMonto = 0;
        state.split = { activo: false, personas: [], personaActual: 0 };
        keypadValue = '0';
    }

    // ═══════════════════════════════════════════════════
    // SCREEN 2 — Productos (25 / 50 / 25)
    // ═══════════════════════════════════════════════════
    function renderProductos() {
        var prods = window.__lmdProductosDisponibles || [];
        var cats = ['Todos'];
        var seen = {};
        prods.forEach(function (p) {
            var cn = p.categoriaNombre || 'Sin categoría';
            if (!seen[cn]) { seen[cn] = true; cats.push(cn); }
        });
        cats.sort(function (a, b) { return a === 'Todos' ? -1 : b === 'Todos' ? 1 : a.localeCompare(b); });

        var catHtml = cats.map(function (c) {
            return '<button class="lmd-pos-cat-btn' + (c === 'Todos' ? ' lmd-pos-cat-btn--activa' : '') + '" data-cat="' + c + '" onclick="pos.filtrarCategoria(\'' + c.replace(/'/g, "\\'") + '\')">' +
                icon(c === 'Todos' ? 'layers' : c === 'Bebidas' ? 'wine' : c === 'Postres' ? 'cake-slice' : 'utensils') + '<span>' + c + '</span>' +
            '</button>';
        }).join('');

        var productosHtml = renderProductGrid(prods);
        var detalles = detallesServidor();
        var total = totalPedidoActual();
        var hayItems = state.lineas.length > 0 || detalles.length > 0;

        var detallesHtml = detalles.map(function (d) {
            var cantidad = d.cantidad || 1;
            var precio = d.subtotal != null ? d.subtotal : (d.precioUnitario || 0) * cantidad;
            return '<div class="lmd-pos-cart-item lmd-pos-cart-item--registrado">' +
                '<div class="lmd-pos-cart-item__info">' +
                    '<span class="lmd-pos-cart-item__nombre">' + htmlEscape(d.productoNombre || d.nombre || 'Producto') + '<small class="lmd-pos-cart-item__estado">En pedido</small></span>' +
                    '<span class="lmd-pos-cart-item__precio">' + fmt(precio) + '</span>' +
                '</div>' +
                '<div class="lmd-pos-cart-item__controles">' +
                    '<span class="lmd-pos-cart-item__qty lmd-pos-cart-item__qty--locked">x' + cantidad + '</span>' +
                '</div>' +
            '</div>';
        }).join('');

        var lineasHtml = state.lineas.map(function (l, i) {
            return '<div class="lmd-pos-cart-item">' +
                '<div class="lmd-pos-cart-item__info">' +
                    '<span class="lmd-pos-cart-item__nombre">' + htmlEscape(l.productoNombre || l.nombre || '') + (l.tieneModificaciones ? '<span class="lmd-pos-mod-dot" title="Tiene modificaciones"></span>' : '') + '<small class="lmd-pos-cart-item__estado">Nuevo</small></span>' +
                    '<span class="lmd-pos-cart-item__precio">' + fmt((l.precioUnitario || 0) * (l.cantidad || 0)) + '</span>' +
                '</div>' +
                '<div class="lmd-pos-cart-item__controles">' +
                    (state.pagado
                        ? '<span class="lmd-pos-cart-item__qty lmd-pos-cart-item__qty--locked">x' + l.cantidad + '</span>'
                        : '<button class="lmd-pos-cart-item__qty-btn" onclick="pos.decrementarItem(' + i + ')">' + icon('minus') + '</button>' +
                          '<span class="lmd-pos-cart-item__qty">' + l.cantidad + '</span>' +
                          '<button class="lmd-pos-cart-item__qty-btn" onclick="pos.incrementarItem(' + i + ')">' + icon('plus') + '</button>' +
                          '<button class="lmd-pos-cart-item__remove" onclick="pos.eliminarDelCarrito(' + i + ')">' + icon('x') + '</button>'
                    ) +
                '</div>' +
            '</div>';
        }).join('');

        var cartItemsHtml = !hayItems
            ? '<div class="lmd-pos-cart__empty">' + icon('shopping-cart') + '<span>Carrito vacío</span></div>'
            : detallesHtml + lineasHtml;

        var pagarLabel = state.pagado ? icon('check-circle') + ' Pagado' : icon('credit-card') + ' Pagar';
        var listoLabel = state.pagado
            ? icon('receipt') + ' Finalizar'
            : state.pedidoActual
                ? icon('plus-circle') + ' Enviar más'
                : icon('send') + ' Enviar a cocina';

        var html = '<div class="lmd-pos-productos">' +
            '<div class="lmd-pos-categorias" id="lmd-pos-categorias">' + catHtml + '</div>' +
            '<div class="lmd-pos-productos-grid" id="lmd-pos-productos-grid">' + productosHtml + '</div>' +
            '<div class="lmd-pos-cart">' +
                '<div class="lmd-pos-cart__header">' +
                    icon('shopping-bag') +
                    '<span>' + servicioLabel() + '</span>' +
                    (state.pedidoActual && !state.pagado ? '<span class="lmd-pos-tab-activo-badge">' + icon('clock') + ' Tab activo</span>' : '') +
                    (!state.pagado ? '<button class="lmd-pos-cart-change-servicio" onclick="pos.cambiarServicio()" title="Cambiar tipo de servicio">' + icon('refresh-cw') + '</button>' : '') +
                    (!state.pagado && state.tipoServicio !== 'ComerAqui' ? '<button class="lmd-pos-cart-change-servicio" onclick="pos.abrirConfigEnvio()" title="Configurar envío">' + icon('truck') + '</button>' : '') +
                    entregaResumenHtml() +
                    (state.pagado ? '<span class="lmd-pos-pagado-badge">' + icon('check-circle') + ' Pagado</span>' : '') +
                '</div>' +
                '<div class="lmd-pos-cart__items" id="lmd-pos-cart-items">' + cartItemsHtml + '</div>' +
                '<div class="lmd-pos-cart__total">' + fmt(total) + '</div>' +
                '<div class="lmd-pos-cart__acciones">' +
                    '<button class="lmd-pos-cart-btn lmd-pos-cart-btn--listo" onclick="pos.confirmarListo()"' + (!state.pagado && !hayItems ? ' disabled' : '') + '>' + listoLabel + '</button>' +
                    '<button class="lmd-pos-cart-btn lmd-pos-cart-btn--cancelar" onclick="pos.cancelarOrden()">' + icon('x-circle') + ' Cancelar</button>' +
                    '<button class="lmd-pos-cart-btn lmd-pos-cart-btn--pagar' + (state.pagado ? ' lmd-pos-cart-btn--pagado' : '') + '" onclick="pos.irAPago()"' + ((!hayItems && !state.pedidoActual) || state.pagado ? ' disabled' : '') + '>' + pagarLabel + '</button>' +
                    (state.pagado ? '<button class="lmd-pos-cart-btn lmd-pos-cart-btn--anular" onclick="pos.confirmarAnulacion()">' + icon('rotate-ccw') + ' Anular pago</button>' : '') +
                '</div>' +
            '</div>' +
        '</div>';

        document.getElementById('lmd-pos-screen-productos').innerHTML = html;
    }

    function renderProductGrid(prods, catFiltro) {
        var filtered = catFiltro && catFiltro !== 'Todos'
            ? prods.filter(function (p) { return (p.categoriaNombre || 'Sin categoría') === catFiltro; })
            : prods;
        if (filtered.length === 0) return '<div class="lmd-pos-empty">Sin productos en esta categoría</div>';
        return filtered.map(function (p) {
            var agotado = p.agotado === true;
            var ico = p.categoriaNombre === 'Bebidas' ? 'wine' : p.categoriaNombre === 'Postres' ? 'cake-slice' : 'utensils';
            var tienePromo = !!p.promoNombre;
            var precioConDescuento = tienePromo
                ? (p.promoTipo === 'porcentaje'
                    ? p.precio - Math.round(p.precio * p.promoDescuento / 100 * 100) / 100
                    : p.precio - p.promoDescuento)
                : p.precio;
            var precioHtml = tienePromo
                ? '<span class="lmd-pos-producto-card__precio lmd-pos-producto-card__precio--promo">' +
                      '<s class="lmd-pos-producto-card__precio-original">' + fmt(p.precio || 0) + '</s> ' +
                      fmt(Math.max(0, precioConDescuento)) +
                  '</span>'
                : '<span class="lmd-pos-producto-card__precio">' + fmt(p.precio || 0) + '</span>';
            // Badge cantidad en carrito
            var lineaEnCarrito = state.lineas.find(function (l) { return l.productoId === p.id; });
            var cartBadge = lineaEnCarrito ? '<span class="lmd-pos-producto-card__cart-badge">×' + lineaEnCarrito.cantidad + '</span>' : '';

            var requiereConfirmacion = productoRequiereConfirmacion(p);
            var nombreSeguro = (p.nombre || '').replace(/\\/g, '\\\\').replace(/'/g, "\\'");
            var accionClick = agotado || state.pagado ? ''
                : requiereConfirmacion
                    ? 'pos.abrirModificadores(\'' + p.id + '\')'
                    : 'pos.agregarAlCarrito(\'' + p.id + '\',\'' + nombreSeguro + '\',' + (p.precio || 0) + ')';

            return '<div class="lmd-pos-producto-card' + (agotado ? ' lmd-pos-producto-card--agotado' : '') + (tienePromo ? ' lmd-pos-producto-card--promo' : '') + (lineaEnCarrito ? ' lmd-pos-producto-card--en-carrito' : '') + (requiereConfirmacion ? ' lmd-pos-producto-card--requiere-confirmacion' : '') + '">' +
                cartBadge + '<div class="lmd-pos-producto-card__body" onclick="' + accionClick + '">' +
                    '<div class="lmd-pos-producto-card__ico">' + icon(ico) + '</div>' +
                    '<span class="lmd-pos-producto-card__nombre">' + (p.nombre || '') + '</span>' +
                    precioHtml +
                    (requiereConfirmacion ? '<span class="lmd-pos-producto-card__confirm-badge">Confirmar ingredientes</span>' : '') +
                    (tienePromo ? '<span class="lmd-pos-producto-card__promo-badge">' + icon('tag') + ' ' + (p.promoNombre || 'PROMO') + '</span>' : '') +
                    (p.tiempoPreparacionMin ? '<span class="lmd-pos-producto-card__tiempo">' + p.tiempoPreparacionMin + ' min</span>' : '') +
                    (agotado ? '<span class="lmd-pos-producto-card__agotado-badge">Agotado</span>' : '') +
                '</div>' +
                (requiereConfirmacion ? '<button class="lmd-pos-producto-card__editar" onclick="pos.abrirModificadores(\'' + p.id + '\')" title="Editar ingredientes">' + icon('edit-3') + '</button>' : '') +
            '</div>';
        }).join('');
    }

    function filtrarCategoria(cat) {
        document.querySelectorAll('.lmd-pos-cat-btn').forEach(function (b) {
            b.classList.toggle('lmd-pos-cat-btn--activa', b.dataset.cat === cat);
        });
        var prods = window.__lmdProductosDisponibles || [];
        document.getElementById('lmd-pos-productos-grid').innerHTML = renderProductGrid(prods, cat);
    }

    function agregarAlCarrito(prodId, nombre, precio) {
        if (state.pagado) return;
        var existente = state.lineas.find(function (l) { return l.productoId === prodId; });
        if (existente) { existente.cantidad += 1; }
        else { state.lineas.push({ productoId: prodId, productoNombre: nombre, cantidad: 1, precioUnitario: precio }); }
        renderProductos();
        mostrarPantalla('productos');
    }

    function incrementarItem(idx) {
        if (state.pagado) return;
        if (state.lineas[idx]) { state.lineas[idx].cantidad += 1; renderProductos(); }
    }

    function decrementarItem(idx) {
        if (state.pagado) return;
        if (!state.lineas[idx]) return;
        if (state.lineas[idx].cantidad > 1) { state.lineas[idx].cantidad -= 1; renderProductos(); }
        else { eliminarDelCarrito(idx); }
    }

    function eliminarDelCarrito(idx) {
        if (state.pagado) return;
        state.lineas.splice(idx, 1);
        renderProductos();
    }

    async function cancelarOrden() {
        if (state.lineas.length === 0 && !state.pedidoActual) return;
        var ok = await window.lmdConfirm('¿Cancelar esta orden? El stock regresará a inventario.');
        if (!ok) return;

        if (state.pedidoActual && state.pedidoActual.id) {
            var csrf = document.querySelector('input[name="__RequestVerificationToken"]');
            var form = new FormData();
            form.append('__RequestVerificationToken', csrf ? csrf.value : '');
            form.append('pedidoId', state.pedidoActual.id);
            try {
                await fetch('?handler=CancelarJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            } catch (e) { /* continúa aunque falle el server */ }
        }

        nuevaOrden();
    }

    async function confirmarAnulacion() {
        if (!state.pagado || !state.pedidoActual) return;
        var ok = await window.lmdConfirm('¿Anular el pago de esta orden? El stock regresará a inventario.');
        if (!ok) return;

        var csrf = document.querySelector('input[name="__RequestVerificationToken"]');
        var form = new FormData();
        form.append('__RequestVerificationToken', csrf ? csrf.value : '');
        form.append('pedidoId', state.pedidoActual.id);
        try {
            var res = await fetch('?handler=AnularPagoJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!res.ok) { var d = await res.json().catch(function() { return {}; }); lmdToast(d.error || 'Error al anular', 'error'); return; }
            lmdToast('Pago anulado', 'success');
            nuevaOrden();
        } catch (e) { lmdToast('Error de conexión', 'error'); }
    }

    // ═══════════════════════════════════════════════════
    // LISTO — enviar a cocina (+ docs si está pagado)
    // ═══════════════════════════════════════════════════
    async function confirmarListo() {
        if (state.pagado) { abrirOverlayDocumentos(); return; }
        if (state.lineas.length === 0) { lmdToast('Agrega productos primero', 'error'); return; }
        if (!validarLineasConfirmadas()) return;

        if (state.lineas.length === 0) {
            lmdToast(state.pedidoActual ? 'Agrega productos nuevos para enviar más a cocina.' : 'Agrega productos primero', state.pedidoActual ? 'info' : 'error');
            return;
        }

        if (_creandoPedido) return;
        _creandoPedido = true;

        var csrf = document.querySelector('input[name="__RequestVerificationToken"]');

        // Tab abierto: agregar items nuevos al pedido existente
        if (state.pedidoActual) {
            var formMas = new FormData();
            formMas.append('__RequestVerificationToken', csrf ? csrf.value : '');
            formMas.append('pedidoId', state.pedidoActual.id);
            state.lineas.forEach(function (l, i) {
                formMas.append('Vm.CrearPedido.Lineas[' + i + '].ProductoId', l.productoId);
                formMas.append('Vm.CrearPedido.Lineas[' + i + '].Cantidad', l.cantidad || 1);
                if (l.notas) formMas.append('Vm.CrearPedido.Lineas[' + i + '].Notas', l.notas);
                if (l.modificacionesJson) formMas.append('Vm.CrearPedido.Lineas[' + i + '].ModificacionesJson', l.modificacionesJson);
            });
            try {
                var resMas = await fetch('?handler=EnviarMasJson', { method: 'POST', body: formMas, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
                if (resMas.ok) {
                    var dataMas = await resMas.json().catch(function() { return {}; });
                    lmdToast('Items enviados a cocina', 'success');
                    state.pedidoActual.total = dataMas.total || ((state.pedidoActual.total || 0) + totalLineas(state.lineas));
                    if (dataMas.detalles) state.pedidoActual.detalles = dataMas.detalles;
                    state.lineas = [];
                    await refrescarPedidosSinMesa();
                    renderProductos();
                } else {
                    var errMas = await resMas.json().catch(function () { return null; });
                    lmdToast(errMas && errMas.error ? errMas.error : 'Error al enviar items', 'error');
                }
            } catch (e) { lmdToast('Error al enviar items', 'error'); }
            _creandoPedido = false;
            return;
        }

        // Sin tab: crear pedido nuevo
        if (!validarEntregaAntesDeCrear()) { _creandoPedido = false; return; }
        var form = new FormData();
        form.append('__RequestVerificationToken', csrf ? csrf.value : '');
        form.append('Vm.CrearPedido.TipoServicio', state.tipoServicio || 'ComerAqui');
        if (state.mesaId) form.append('Vm.CrearPedido.MesaId', state.mesaId);
        aplicarDatosEntrega(form);
        state.lineas.forEach(function (l, i) {
            form.append('Vm.CrearPedido.Lineas[' + i + '].ProductoId', l.productoId);
            form.append('Vm.CrearPedido.Lineas[' + i + '].Cantidad', l.cantidad || 1);
            if (l.notas) form.append('Vm.CrearPedido.Lineas[' + i + '].Notas', l.notas);
            if (l.modificacionesJson) form.append('Vm.CrearPedido.Lineas[' + i + '].ModificacionesJson', l.modificacionesJson);
        });
        try {
            var res = await fetch('?handler=CrearJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            var data = await res.json();
            if (data.pedidoId) {
                state.pedidoActual = { id: data.pedidoId, total: data.total || totalLineas(state.lineas), detalles: data.detalles || [] };
                state.lineas = [];
                _creandoPedido = false;
                await Promise.all([refrescarMesas(), refrescarPedidosSinMesa()]);
                renderProductos();
                lmdToast('Pedido enviado a cocina. Podés cobrarlo desde esta misma pantalla.', 'success');
                return;
            }
        } catch (e) { lmdToast('Error al enviar pedido', 'error'); }
        _creandoPedido = false;

    }


    function aplicarDatosEntrega(form) {
        if (state.tipoServicio !== 'Domicilio') return;
        form.append('Vm.CrearPedido.DireccionEntrega', (state.entrega && state.entrega.direccion) || '');
        form.append('Vm.CrearPedido.TelefonoCliente', (state.entrega && state.entrega.telefono) || '');
        if (state.entrega && state.entrega.repartidorId) {
            form.append('Vm.CrearPedido.RepartidorId', state.entrega.repartidorId);
        }
    }

    function validarEntregaAntesDeCrear() {
        if (state.tipoServicio !== 'Domicilio') return true;
        if (state.entrega && state.entrega.direccion && state.entrega.direccion.trim()) return true;
        lmdToast('Configurá la dirección de delivery antes de enviar o cobrar.', 'error');
        abrirConfigEnvio(true);
        return false;
    }

    function abrirConfigEnvio(obligatorio) {
        var repartidores = window.__lmdRepartidoresDisponibles || [];
        var opts = '<option value="">Asignar después</option>' + repartidores.map(function (r) {
            var selected = state.entrega && state.entrega.repartidorId === r.id ? ' selected' : '';
            return '<option value="' + r.id + '"' + selected + '>' + (r.nombre || 'Repartidor') + '</option>';
        }).join('');
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<span class="lmd-pos-ov-title">' + icon('truck') + ' Configurar envío</span>' +
                (!obligatorio ? '<button class="lmd-pos-ov-close" onclick="pos.cerrarConfigEnvio()">' + icon('x') + '</button>' : '') +
            '</div>' +
            '<div class="lmd-pos-cambiar-servicio-body">' +
                '<label class="form-label">Dirección de entrega</label>' +
                '<input id="lmd-delivery-dir" class="form-control mb-2" maxlength="250" placeholder="Calle, número, colonia, referencia" value="' + ((state.entrega && state.entrega.direccion) || '').replace(/"/g, '&quot;') + '" />' +
                '<label class="form-label">Teléfono del cliente</label>' +
                '<input id="lmd-delivery-tel" class="form-control mb-2" maxlength="30" placeholder="0000-0000" value="' + ((state.entrega && state.entrega.telefono) || '').replace(/"/g, '&quot;') + '" />' +
                '<label class="form-label">Repartidor</label>' +
                '<select id="lmd-delivery-rep" class="form-select mb-3">' + opts + '</select>' +
                '<div class="d-flex gap-2 flex-wrap">' +
                    '<button type="button" class="btn btn-dark" onclick="pos.guardarConfigEnvio()">Guardar envío</button>' +
                    '<button type="button" class="btn btn-outline-secondary" onclick="pos.quitarEnvio()">Retiro en caja</button>' +
                '</div>' +
            '</div>';
        state.tipoServicio = 'Domicilio';
        abrirOverlay('delivery', html, { closeOnBackdrop: !obligatorio, wide: true });
    }

    function guardarConfigEnvio() {
        var dir = document.getElementById('lmd-delivery-dir')?.value || '';
        var tel = document.getElementById('lmd-delivery-tel')?.value || '';
        var rep = document.getElementById('lmd-delivery-rep')?.value || '';
        if (!dir.trim()) { lmdToast('La dirección de entrega es obligatoria.', 'error'); return; }
        state.tipoServicio = 'Domicilio';
        state.mesaId = null;
        state.mesaNumero = null;
        state.entrega = { direccion: dir.trim(), telefono: tel.trim(), repartidorId: rep };
        cerrarOverlay('delivery');
        renderProductos();
        lmdToast('Datos de delivery guardados', 'success');
    }

    function cerrarConfigEnvio() { cerrarOverlay('delivery'); }

    function quitarEnvio() {
        state.tipoServicio = 'ParaLlevar';
        state.entrega = { direccion: '', telefono: '', repartidorId: '' };
        cerrarOverlay('delivery');
        renderProductos();
        lmdToast('Pedido cambiado a retiro en caja', 'info');
    }

    // ═══════════════════════════════════════════════════
    // PAGO — overlay de 6 métodos
    // ═══════════════════════════════════════════════════
    async function irAPago() {
        if (state.pagado) return;
        if (_creandoPedido) return;
        if (state.lineas.length > 0 && !validarLineasConfirmadas()) return;

        // Tab abierto con items pendientes: enviarlos a cocina primero, luego cobrar
        if (state.pedidoActual && state.lineas.length > 0) {
            await confirmarListo();
            if (state.lineas.length > 0) return; // envio fallo
        }

        if (!state.pedidoActual && state.lineas.length === 0) {
            lmdToast('Agrega productos primero', 'error');
            return;
        }

        if (!state.pedidoActual) {
            if (!validarEntregaAntesDeCrear()) return;
            _creandoPedido = true;
            var csrf = document.querySelector('input[name="__RequestVerificationToken"]');
            var form = new FormData();
            form.append('__RequestVerificationToken', csrf ? csrf.value : '');
            form.append('Vm.CrearPedido.TipoServicio', state.tipoServicio || 'ComerAqui');
            if (state.mesaId) form.append('Vm.CrearPedido.MesaId', state.mesaId);
            aplicarDatosEntrega(form);
            state.lineas.forEach(function (l, i) {
                form.append('Vm.CrearPedido.Lineas[' + i + '].ProductoId', l.productoId);
                form.append('Vm.CrearPedido.Lineas[' + i + '].Cantidad', l.cantidad || 1);
                if (l.notas) form.append('Vm.CrearPedido.Lineas[' + i + '].Notas', l.notas);
                if (l.modificacionesJson) form.append('Vm.CrearPedido.Lineas[' + i + '].ModificacionesJson', l.modificacionesJson);
            });
            try {
                var res = await fetch('?handler=CrearJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
                var data = await res.json();
                if (data.pedidoId) {
                    state.pedidoActual = { id: data.pedidoId, total: data.total || totalLineas(state.lineas), detalles: data.detalles || [] };
                    state.lineas = [];
                    await refrescarPedidosSinMesa();
                    renderProductos();
                }
                else { lmdToast('No se pudo crear el pedido', 'error'); _creandoPedido = false; return; }
            } catch (e) { lmdToast('Error al crear pedido', 'error'); _creandoPedido = false; return; }
            _creandoPedido = false;
        }

        abrirOverlayPago();
    }

    var METODOS_PAGO = [
        { codigo: 'efectivo',  label: 'Efectivo',    icon: 'banknote',   sub: 'Cambio automático',  cls: 'lmd-pos-pm--efectivo' },
        { codigo: 'tarjeta',   label: 'Tarjeta',     icon: 'credit-card', sub: 'Débito / Crédito',  cls: 'lmd-pos-pm--tarjeta'  },
        { codigo: 'qr',        label: 'QR / Transf.', icon: 'qr-code',   sub: 'Wompi · BAC · Niu',  cls: 'lmd-pos-pm--qr'      },
        { codigo: 'credito',   label: 'Crédito Emp.', icon: 'building-2', sub: 'Cuenta corriente',  cls: ''                     },
        { codigo: 'vale',      label: 'Vale',         icon: 'ticket',    sub: 'Voucher alimentación', cls: ''                    },
        { codigo: 'cortesia',  label: 'Cortesía',     icon: 'gift',      sub: 'Invitación ($0)',     cls: ''                     }
    ];

    function abrirOverlayPago() {
        limpiarCapasHuerfanas();
        var total = totalPedidoActual();
        var btnsHtml = METODOS_PAGO.map(function (m) {
            return '<button class="lmd-pos-pm-btn ' + (m.cls || '') + '" onclick="pos.procesarPago(\'' + m.codigo + '\',' + total.toFixed(2) + ')">' +
                '<span class="lmd-pos-pm-btn__icon">' + icon(m.icon) + '</span>' +
                '<span class="lmd-pos-pm-btn__label">' + m.label + '</span>' +
                '<span class="lmd-pos-pm-btn__sub">' + m.sub + '</span>' +
            '</button>';
        }).join('');

        var html =
            '<div class="lmd-pos-ov-header">' +
                '<span class="lmd-pos-ov-title">' + icon('credit-card') + ' Seleccionar pago</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
                '<button class="lmd-pos-ov-close" onclick="pos.cerrarPago()">' + icon('x') + '</button>' +
            '</div>' +
            '<div class="lmd-pos-pm-grid">' + btnsHtml + '</div>' +
            '<div class="lmd-pos-ov-footer">' +
                '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--split" onclick="pos.abrirSplit()">' + icon('users') + ' Dividir cuenta</button>' +
            '</div>';

        abrirOverlay('pago', html, { wide: true, closeOnBackdrop: false });
    }

    function cerrarPago() {
        cerrarTodasOverlaysPago();
    }

    function procesarPago(metodo, total) {
        if (metodo === 'efectivo') {
            abrirOverlayEfectivo(total);
        } else if (metodo === 'tarjeta') {
            abrirOverlayTarjeta(total);
        } else if (metodo === 'qr') {
            abrirOverlayQR(total);
        } else if (metodo === 'cortesia') {
            finalizarPago(metodo, 0, null);
        } else {
            // Crédito, Vale
            abrirOverlayOtro(metodo, total);
        }
    }

    // ── Efectivo: keypad + shortcuts + cambio ──────────
    function abrirOverlayEfectivo(total) {
        keypadValue = '0';
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos.volverAMetodos()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('banknote') + ' Efectivo</span>' +
                '<div class="lmd-pos-ov-total">Total: ' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-efectivo-body">' +
                '<div class="lmd-pos-bill-shortcuts">' +
                    '<button class="lmd-pos-bill-btn lmd-pos-bill-btn--exacto" onclick="pos.seleccionarBillete(' + total.toFixed(2) + ',' + total.toFixed(2) + ')">Exacto</button>' +
                    [5, 10, 20, 50, 100].map(function (b) {
                        return '<button class="lmd-pos-bill-btn" onclick="pos.seleccionarBillete(' + b + ',' + total.toFixed(2) + ')">' + icon('banknote') + ' $' + b + '</button>';
                    }).join('') +
                '</div>' +
                '<div class="lmd-pos-keypad" id="lmd-pos-keypad">' +
                    '<div class="lmd-pos-keypad__display" id="lmd-pos-keypad-display">$0.00</div>' +
                    '<div class="lmd-pos-cambio" id="lmd-pos-cambio"></div>' +
                    '<div class="lmd-pos-keypad__grid">' +
                        [1,2,3,4,5,6,7,8,9,'.',0,'⌫'].map(function (k) {
                            return '<button class="lmd-pos-keypad__btn" onclick="pos.keypadInput(\'' + k + '\',' + total.toFixed(2) + ')">' + (k === '⌫' ? icon('delete') : k) + '</button>';
                        }).join('') +
                    '</div>' +
                    '<div class="lmd-pos-keypad__actions">' +
                        '<button class="lmd-pos-keypad__btn lmd-pos-keypad__btn--back" onclick="pos.volverAMetodos()">Volver</button>' +
                        '<button class="lmd-pos-keypad__btn lmd-pos-keypad__btn--confirm" id="lmd-keypad-confirmar" onclick="pos.keypadConfirmar(' + total.toFixed(2) + ')" disabled>Confirmar</button>' +
                    '</div>' +
                '</div>' +
            '</div>';

        cerrarOverlay('pago');
        abrirOverlay('efectivo', html, { closeOnBackdrop: false });
    }

    function seleccionarBillete(valor, total) {
        keypadValue = parseFloat(valor).toFixed(2).replace(/\.00$/, '');
        _actualizarKeypad(total);
    }

    function keypadInput(k, total) {
        if (k === '⌫') {
            keypadValue = keypadValue.length > 1 ? keypadValue.slice(0, -1) : '0';
        } else if (k === '.') {
            if (keypadValue.indexOf('.') === -1) keypadValue += '.';
        } else {
            // Max 2 decimal places
            var parts = keypadValue.split('.');
            if (parts[1] && parts[1].length >= 2) return;
            keypadValue = keypadValue === '0' ? '' + k : keypadValue + k;
        }
        _actualizarKeypad(total);
    }

    function _actualizarKeypad(total) {
        var recibido = parseFloat(keypadValue || '0');
        var display = document.getElementById('lmd-pos-keypad-display');
        var cambioEl = document.getElementById('lmd-pos-cambio');
        var confirmar = document.getElementById('lmd-keypad-confirmar');
        if (display) display.textContent = '$' + (recibido || 0).toFixed(2);
        if (cambioEl) {
            var cambio = recibido - total;
            if (recibido <= 0) {
                cambioEl.textContent = '';
                cambioEl.className = 'lmd-pos-cambio';
            } else if (cambio >= 0) {
                cambioEl.textContent = 'Cambio: ' + fmt(cambio);
                cambioEl.className = 'lmd-pos-cambio lmd-pos-cambio--ok';
            } else {
                cambioEl.textContent = 'Faltan: ' + fmt(-cambio);
                cambioEl.className = 'lmd-pos-cambio lmd-pos-cambio--falta';
            }
        }
        if (confirmar) confirmar.disabled = recibido < total;
    }

    function keypadConfirmar(total) {
        var recibido = parseFloat(keypadValue || '0');
        if (recibido < total) { lmdToast('Monto insuficiente', 'error'); return; }
        cerrarOverlay('efectivo');
        abrirOverlayPropina(total, recibido);
    }

    function abrirOverlayPropina(total, recibido) {
        var cambio = recibido - total;
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos.volverAMetodos()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('heart') + ' Propina</span>' +
                '<div class="lmd-pos-ov-total">Cambio: ' + fmt(cambio) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-propina-body">' +
                '<p class="lmd-pos-propina-hint">¿El cliente desea agregar propina?</p>' +
                '<div class="lmd-pos-bill-shortcuts">' +
                    [1, 2, 5, 10].map(function (v) {
                        return '<button class="lmd-pos-bill-btn" onclick="pos.confirmarPropina(' + total.toFixed(2) + ',' + recibido.toFixed(2) + ',' + v + ')">+' + fmt(v) + '</button>';
                    }).join('') +
                    '<button class="lmd-pos-bill-btn lmd-pos-bill-btn--exacto" onclick="pos.confirmarPropina(' + total.toFixed(2) + ',' + recibido.toFixed(2) + ',0)">Sin propina</button>' +
                '</div>' +
            '</div>';
        abrirOverlay('propina', html, { closeOnBackdrop: false });
    }

    function confirmarPropina(total, recibido, propina) {
        state.propinaMonto = propina || 0;
        cerrarOverlay('propina');
        finalizarPago('efectivo', recibido, null);
    }

    function volverAMetodos() {
        cerrarOverlay('efectivo');
        cerrarOverlay('tarjeta');
        cerrarOverlay('qr');
        cerrarOverlay('otro');
        cerrarOverlay('errorpago');
        abrirOverlayPago();
    }

    // ── Tarjeta ────────────────────────────────────────
    function abrirOverlayTarjeta(total) {
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos.volverAMetodos()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('credit-card') + ' Tarjeta</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-tarjeta-body">' +
                '<div class="lmd-pos-tarjeta-ilustracion">' + icon('credit-card') + '</div>' +
                '<p class="lmd-pos-tarjeta-instruccion">Acerca o inserta la tarjeta en el terminal</p>' +
                '<p class="lmd-pos-tarjeta-monto">' + fmt(total) + '</p>' +
                '<div class="lmd-pos-tarjeta-tipos">' +
                    '<span class="lmd-pos-tarjeta-chip">Visa</span>' +
                    '<span class="lmd-pos-tarjeta-chip">Mastercard</span>' +
                    '<span class="lmd-pos-tarjeta-chip">Amex</span>' +
                    '<span class="lmd-pos-tarjeta-chip">Débito</span>' +
                '</div>' +
                '<div class="lmd-pos-tarjeta-ref-group">' +
                    '<label class="lmd-pos-tarjeta-ref-label" for="pos-tarjeta-ref">' + icon('hash') + ' N.° de autorización (voucher)</label>' +
                    '<input id="pos-tarjeta-ref" class="lmd-pos-tarjeta-ref-input" type="text" placeholder="Ej. 123456 / AUTH-ABC" autocomplete="off" />' +
                    '<p id="pos-tarjeta-ref-error" class="lmd-pos-tarjeta-ref-error" style="display:none">El número de autorización es obligatorio.</p>' +
                '</div>' +
                '<div class="lmd-pos-tarjeta-actions">' +
                    '<button class="lmd-pos-ov-btn" onclick="pos.volverAMetodos()">' + icon('arrow-left') + ' Volver</button>' +
                    '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--danger" onclick="pos.simularRechazo(\'tarjeta\')">' + icon('x-circle') + ' Rechazada</button>' +
                    '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--primary" onclick="pos.confirmarTarjeta(' + total.toFixed(2) + ')">' + icon('check-circle') + ' Confirmar pago</button>' +
                '</div>' +
            '</div>';

        cerrarOverlay('pago');
        abrirOverlay('tarjeta', html, { closeOnBackdrop: false });
        setTimeout(function () { var el = document.getElementById('pos-tarjeta-ref'); if (el) el.focus(); }, 100);
    }

    function confirmarTarjeta(total) {
        var input = document.getElementById('pos-tarjeta-ref');
        var ref = input ? input.value.trim() : '';
        var errEl = document.getElementById('pos-tarjeta-ref-error');
        if (!ref) {
            if (errEl) errEl.style.display = '';
            if (input) input.focus();
            return;
        }
        cerrarOverlay('tarjeta');
        finalizarPago('tarjeta', total, ref);
    }

    function simularTarjeta(total) {
        cerrarOverlay('tarjeta');
        finalizarPago('tarjeta', total, 'TARJ-' + Date.now().toString(36).toUpperCase());
    }

    // ── QR / Transferencia ─────────────────────────────
    function abrirOverlayQR(total) {
        var ref = 'QR-' + Date.now().toString(36).toUpperCase();
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos.volverAMetodos()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('qr-code') + ' QR / Transferencia</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-qr-body">' +
                '<div class="lmd-pos-qr-box">' +
                    '<div class="lmd-pos-qr-simulado">' +
                        '<div class="lmd-pos-qr-patron"></div>' +
                    '</div>' +
                    '<p class="lmd-pos-qr-ref">Ref: ' + ref + '</p>' +
                    '<p class="lmd-pos-qr-monto">' + fmt(total) + '</p>' +
                '</div>' +
                '<p class="lmd-pos-qr-instruccion">Escanea con Wompi, BAC Credomatic o Niu</p>' +
                '<div class="lmd-pos-qr-actions">' +
                    '<button class="lmd-pos-ov-btn" onclick="pos.volverAMetodos()">' + icon('arrow-left') + ' Volver</button>' +
                    '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--danger" onclick="pos.simularRechazo(\'qr\')">' + icon('x-circle') + ' Simular rechazo</button>' +
                    '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--primary" onclick="pos.simularQR(\'' + ref + '\',' + total.toFixed(2) + ')">' + icon('check-circle') + ' Confirmar recepción</button>' +
                '</div>' +
            '</div>';

        cerrarOverlay('pago');
        abrirOverlay('qr', html, { closeOnBackdrop: false });
    }

    function simularQR(ref, total) {
        cerrarOverlay('qr');
        lmdToast('Transferencia confirmada — ref: ' + ref, 'success');
        finalizarPago('qr', total, ref);
    }

    // ── Otros métodos ──────────────────────────────────
    function abrirOverlayOtro(metodo, total) {
        var label = metodo === 'credito' ? 'Crédito Empresarial' : 'Vale';
        var ref = metodo.toUpperCase() + '-' + Date.now().toString(36).toUpperCase();
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos.volverAMetodos()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + label + '</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-otro-body">' +
                '<p>Ref: ' + ref + '</p>' +
                '<p class="lmd-pos-qr-monto">' + fmt(total) + '</p>' +
                '<div class="lmd-pos-qr-actions">' +
                    '<button class="lmd-pos-ov-btn" onclick="pos.volverAMetodos()">' + icon('arrow-left') + ' Volver</button>' +
                    '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--primary" onclick="pos.confirmarOtro(\'' + metodo + '\',' + total.toFixed(2) + ',\'' + ref + '\')">' + icon('check-circle') + ' Confirmar</button>' +
                '</div>' +
            '</div>';

        cerrarOverlay('pago');
        abrirOverlay('otro', html, { closeOnBackdrop: false });
    }

    function confirmarOtro(metodo, total, ref) {
        cerrarOverlay('otro');
        finalizarPago(metodo, total, ref);
    }

    // ═══════════════════════════════════════════════════
    // SPLIT PAYMENT
    // ═══════════════════════════════════════════════════
    async function abrirSplit() {
        cerrarOverlay('pago');
        // Fetch current detalles with server IDs (needed for per-item split)
        if (state.pedidoActual && state.pedidoActual.id) {
            try {
                var r = await fetch('?handler=DetallesPedidoJson&pedidoId=' + state.pedidoActual.id, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
                if (r.ok) {
                    var rd = await r.json();
                    state.pedidoActual.detalles = rd.detalles || [];
                    if (rd.total) state.pedidoActual.total = rd.total;
                }
            } catch (e) {}
        }
        var total = totalPedidoActual();
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos.volverAPago()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('users') + ' Dividir cuenta</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-split-body">' +
                '<p class="lmd-pos-split-desc">¿Cómo dividir ' + fmt(total) + '?</p>' +
                '<div class="lmd-pos-split-opciones">' +
                    '<button class="lmd-pos-split-opcion" onclick="pos.splitIgualitario()">' +
                        icon('equal') +
                        '<span>Partes iguales</span>' +
                        '<small>Dividir el total entre N personas</small>' +
                    '</button>' +
                    '<button class="lmd-pos-split-opcion" onclick="pos.splitPorPersona()">' +
                        icon('user-check') +
                        '<span>Por persona</span>' +
                        '<small>Cada quien paga lo suyo</small>' +
                    '</button>' +
                    '<button class="lmd-pos-split-opcion" onclick="pos.splitMixto()">' +
                        icon('shuffle') +
                        '<span>Mixto</span>' +
                        '<small>Grupos personalizados</small>' +
                    '</button>' +
                '</div>' +
            '</div>';

        abrirOverlay('split', html, { closeOnBackdrop: false });
    }

    function volverAPago() {
        cerrarOverlay('split');
        cerrarOverlay('splitdetalle');
        abrirOverlayPago();
    }

    function splitIgualitario() {
        _splitN = 2;
        cerrarOverlay('split');
        var total = totalPedidoActual();
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos.volverAPago()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('equal') + ' Partes iguales</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-split-igualitario">' +
                '<p>¿Entre cuántas personas?</p>' +
                '<div class="lmd-pos-split-counter">' +
                    '<button class="lmd-pos-split-counter__btn" onclick="pos.ajustarSplitN(-1)">' + icon('minus') + '</button>' +
                    '<span class="lmd-pos-split-counter__val" id="split-n">2</span>' +
                    '<button class="lmd-pos-split-counter__btn" onclick="pos.ajustarSplitN(1)">' + icon('plus') + '</button>' +
                '</div>' +
                '<div class="lmd-pos-split-preview" id="split-preview">' + _splitPreview(total, 2) + '</div>' +
                '<div class="lmd-pos-split-actions">' +
                    '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--primary" onclick="pos.iniciarSplitIgualitario(' + total.toFixed(2) + ')">' + icon('check-circle') + ' Iniciar cobro</button>' +
                '</div>' +
            '</div>';

        abrirOverlay('splitdetalle', html, { closeOnBackdrop: false });
    }

    var _splitN = 2;
    var _splitTipo = 'persona'; // 'persona' | 'mixto'
    var _splitItems = [];      // [{ nombre, cantidad, precio, persona }] persona: -1=sin asignar/compartido
    function ajustarSplitN(delta) {
        _splitN = Math.max(2, Math.min(10, _splitN + delta));
        var el = document.getElementById('split-n');
        if (el) el.textContent = _splitN;
        var total = totalPedidoActual();
        var preview = document.getElementById('split-preview');
        if (preview) preview.innerHTML = _splitPreview(total, _splitN);
    }

    function _splitPreview(total, n) {
        var partes = Array.from({ length: n }, function (_, i) {
            var monto = (i < n - 1) ? Math.floor(total / n * 100) / 100 : total - Math.floor(total / n * 100) / 100 * (n - 1);
            return '<div class="lmd-pos-split-row"><span>Persona ' + (i + 1) + '</span><span>' + fmt(monto) + '</span></div>';
        }).join('');
        return partes;
    }

    function iniciarSplitIgualitario(total) {
        cerrarOverlay('splitdetalle');
        var n = _splitN;
        state.split.activo = true;
        state.split.personas = Array.from({ length: n }, function (_, i) {
            var monto = (i < n - 1) ? Math.floor(total / n * 100) / 100 : total - Math.floor(total / n * 100) / 100 * (n - 1);
            return { id: i, nombre: 'Persona ' + (i + 1), monto: monto, metodoPago: null, pagado: false };
        });
        state.split.personaActual = 0;
        cobrarSiguientePersona();
    }

    function splitPorPersona() {
        _splitN = 2; _splitTipo = 'persona';
        cerrarOverlay('split');
        _renderSplitNPicker();
    }

    function splitMixto() {
        _splitN = 2; _splitTipo = 'mixto';
        cerrarOverlay('split');
        _renderSplitNPicker();
    }

    function _renderSplitNPicker() {
        var total = totalPedidoActual();
        var esMixto = _splitTipo === 'mixto';
        var titulo = esMixto ? 'División mixta' : 'Por persona';
        var icono = esMixto ? 'git-merge' : 'users';
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos.volverAPago()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon(icono) + ' ' + titulo + '</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-split-igualitario">' +
                '<p>' + (esMixto ? 'Asigná ítems por persona o como compartidos' : 'Asigná cada ítem a una persona') + '</p>' +
                '<div class="lmd-pos-split-counter">' +
                    '<button class="lmd-pos-split-counter__btn" onclick="pos.ajustarSplitN(-1)">' + icon('minus') + '</button>' +
                    '<span class="lmd-pos-split-counter__val" id="split-n">' + _splitN + '</span>' +
                    '<button class="lmd-pos-split-counter__btn" onclick="pos.ajustarSplitN(1)">' + icon('plus') + '</button>' +
                '</div>' +
                '<div class="lmd-pos-split-actions">' +
                    '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--primary" onclick="pos.iniciarAsignacionSplit()">' + icon('arrow-right') + ' Asignar ítems</button>' +
                '</div>' +
            '</div>';
        abrirOverlay('splitdetalle', html, { closeOnBackdrop: false });
    }

    function iniciarAsignacionSplit() {
        var esMixto = _splitTipo === 'mixto';
        // Prefer server detalles (have real IDs) over client lineas
        var serverDetalles = state.pedidoActual && state.pedidoActual.detalles && state.pedidoActual.detalles.length > 0;
        _splitItems = serverDetalles
            ? state.pedidoActual.detalles.map(function (d) {
                return { nombre: d.productoNombre, cantidad: d.cantidad, precio: (d.precioUnitario || 0) * d.cantidad, persona: -1, detalleId: d.id };
              })
            : state.lineas.map(function (l) {
                return { nombre: l.productoNombre || l.nombre || '?', cantidad: l.cantidad || 1, precio: (l.precioUnitario || 0) * (l.cantidad || 1), persona: -1, detalleId: null };
              });
        _renderSplitAsignacion();
    }

    function _renderSplitAsignacion() {
        var n = _splitN;
        var esMixto = _splitTipo === 'mixto';
        var headers = '';
        for (var hi = 0; hi < n; hi++) headers += '<th>P' + (hi + 1) + '</th>';
        if (esMixto) headers += '<th>' + icon('share-2') + '</th>';

        var rows = _splitItems.map(function (item, idx) {
            var celdas = '';
            for (var ci = 0; ci < n; ci++) {
                var activo = item.persona === ci;
                celdas += '<td><button class="lmd-pos-split-asig-btn' + (activo ? ' activo' : '') + '" onclick="pos.asignarItemSplit(' + idx + ',' + ci + ')">' + (activo ? icon('check') : '') + '</button></td>';
            }
            if (esMixto) {
                var comp = item.persona === -1;
                celdas += '<td><button class="lmd-pos-split-asig-btn lmd-pos-split-asig-btn--comp' + (comp ? ' activo' : '') + '" onclick="pos.asignarItemSplit(' + idx + ',-1)">' + (comp ? icon('check') : '') + '</button></td>';
            }
            var pendiente = !esMixto && item.persona < 0;
            return '<tr' + (pendiente ? ' class="lmd-pos-split-asig-row--pendiente"' : '') + '>' +
                '<td class="lmd-pos-split-asig-nombre">' + (item.cantidad > 1 ? item.cantidad + 'x ' : '') + item.nombre + '<span class="lmd-pos-split-asig-precio">' + fmt(item.precio) + '</span></td>' +
                celdas + '</tr>';
        }).join('');

        var totales = Array.from({ length: n }, function () { return 0; });
        var totalCompartido = 0;
        _splitItems.forEach(function (item) {
            if (item.persona >= 0) totales[item.persona] += item.precio;
            else totalCompartido += item.precio;
        });
        var porcadaUno = esMixto ? totalCompartido / n : 0;

        var totalRow = '<td></td>';
        for (var ti = 0; ti < n; ti++) {
            totalRow += '<td class="lmd-pos-split-asig-total">' + fmt(Math.round((totales[ti] + porcadaUno) * 100) / 100) + '</td>';
        }
        if (esMixto) totalRow += '<td class="lmd-pos-split-asig-total">' + fmt(totalCompartido) + '</td>';

        var sinAsignar = esMixto ? 0 : _splitItems.filter(function (i) { return i.persona < 0; }).length;
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos._renderSplitNPicker()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('git-branch') + ' Asignar ítems</span>' +
            '</div>' +
            '<div class="lmd-pos-split-asig-body">' +
                (sinAsignar > 0 ? '<div class="lmd-pos-split-asig-alerta">' + icon('alert-circle') + ' ' + sinAsignar + ' ítem(s) sin asignar</div>' : '') +
                '<div class="lmd-pos-split-asig-scroll">' +
                    '<table class="lmd-pos-split-asig-tabla">' +
                        '<thead><tr><th>Ítem</th>' + headers + '</tr></thead>' +
                        '<tbody>' + rows + '</tbody>' +
                        '<tfoot><tr>' + totalRow + '</tr></tfoot>' +
                    '</table>' +
                '</div>' +
                '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--primary"' + (sinAsignar > 0 ? ' disabled' : '') + ' onclick="pos.confirmarAsignacionSplit()">' + icon('check-circle') + ' Confirmar y cobrar</button>' +
            '</div>';
        cerrarOverlay('splitdetalle');
        abrirOverlay('splitdetalle', html, { closeOnBackdrop: false, wide: true });
    }

    function asignarItemSplit(idx, persona) {
        if (_splitItems[idx]) { _splitItems[idx].persona = persona; _renderSplitAsignacion(); }
    }

    async function confirmarAsignacionSplit() {
        var n = _splitN;
        var esMixto = _splitTipo === 'mixto';
        var totales = Array.from({ length: n }, function () { return 0; });
        var totalCompartido = 0;
        _splitItems.forEach(function (item) {
            if (item.persona >= 0) totales[item.persona] += item.precio;
            else totalCompartido += item.precio;
        });
        var porcadaUno = esMixto ? totalCompartido / n : 0;
        if (!esMixto) {
            var personasSinItems = totales.filter(function (t) { return t === 0; }).length;
            if (personasSinItems > 0) { lmdToast('Cada persona debe tener al menos un item asignado', 'error'); return; }
        }
        state.split.activo = true;
        state.split.personas = totales.map(function (t, i) {
            return { id: i, nombre: 'Persona ' + (i + 1), monto: Math.round((t + porcadaUno) * 100) / 100, metodoPago: null, pagado: false, cuentaId: null };
        });
        state.split.personaActual = 0;

        // Conectar con servidor si todos los ítems tienen detalleId (modo por persona)
        var todosConId = !esMixto && _splitItems.every(function (si) { return si.detalleId && si.persona >= 0; });
        if (todosConId && state.pedidoActual && state.pedidoActual.id) {
            var asignaciones = Array.from({ length: n }, function (_, i) {
                var items = _splitItems
                    .filter(function (si) { return si.persona === i; })
                    .map(function (si) { return { detalleId: si.detalleId, cantidad: si.cantidad }; });
                return { cuentaNumero: i + 1, items: items };
            }).filter(function (a) { return a.items.length > 0; });

            if (asignaciones.length >= 2) {
                var csrf = document.querySelector('input[name="__RequestVerificationToken"]');
                try {
                    var res = await fetch('?handler=CrearCuentasConItemsJson', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'X-Requested-With': 'XMLHttpRequest',
                            'RequestVerificationToken': csrf ? csrf.value : ''
                        },
                        body: JSON.stringify({ pedidoId: state.pedidoActual.id, asignaciones: asignaciones })
                    });
                    if (res.ok) {
                        var cuentas = await res.json();
                        (cuentas || []).forEach(function (c) {
                            var p = state.split.personas[c.numero - 1];
                            if (p) { p.cuentaId = c.id; p.monto = c.total || p.monto; }
                        });
                    }
                } catch (e) {}
            }
        }

        cerrarOverlay('splitdetalle');
        cobrarSiguientePersona();
    }

    function cobrarSiguientePersona() {
        var personas = state.split.personas;
        var idx = personas.findIndex(function (p) { return !p.pagado; });
        if (idx < 0) {
            state.split.activo = false;
            var total = totalPedidoActual();
            finalizarPago('split', total, 'SPLIT-' + state.split.personas.length);
            return;
        }
        state.split.personaActual = idx;
        var persona = personas[idx];
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<span class="lmd-pos-ov-title">' + icon('user') + ' ' + persona.nombre + '</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(persona.monto) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-pm-grid">' +
                METODOS_PAGO.map(function (m) {
                    return '<button class="lmd-pos-pm-btn ' + (m.cls || '') + '" onclick="pos.pagarPersonaSplit(' + idx + ',\'' + m.codigo + '\',' + persona.monto.toFixed(2) + ')">' +
                        '<span class="lmd-pos-pm-btn__icon">' + icon(m.icon) + '</span>' +
                        '<span class="lmd-pos-pm-btn__label">' + m.label + '</span>' +
                        '<span class="lmd-pos-pm-btn__sub">' + m.sub + '</span>' +
                    '</button>';
                }).join('') +
            '</div>' +
            '<div class="lmd-pos-split-progress">' +
                personas.map(function (p, i) {
                    return '<div class="lmd-pos-split-progress__item' + (p.pagado ? ' pagado' : '') + (i === idx ? ' activo' : '') + '">' +
                        '<span>' + p.nombre + '</span><span>' + (p.pagado ? icon('check') : fmt(p.monto)) + '</span>' +
                    '</div>';
                }).join('') +
            '</div>';

        cerrarOverlay('splitdetalle');
        abrirOverlay('splitdetalle', html, { closeOnBackdrop: false, wide: true });
    }

    async function pagarPersonaSplit(idx, metodo, monto) {
        if (metodo === 'efectivo') { abrirOverlayEfectivoSplit(idx, monto); return; }
        var persona = state.split.personas[idx];
        if (persona.cuentaId) {
            var csrf = document.querySelector('input[name="__RequestVerificationToken"]');
            var form = new FormData();
            form.append('__RequestVerificationToken', csrf ? csrf.value : '');
            form.append('cuentaId', persona.cuentaId);
            form.append('metodoPago', metodo);
            form.append('propinaMonto', '0');
            try { await fetch('?handler=PagarCuentaJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } }); } catch (e) {}
        }
        persona.metodoPago = metodo;
        persona.pagado = true;
        lmdToast(persona.nombre + ' — pagado con ' + metodo, 'success');
        cobrarSiguientePersona();
    }

    function abrirOverlayEfectivoSplit(idx, total) {
        keypadValue = '0';
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos.cobrarSiguientePersona()">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('banknote') + ' Efectivo — ' + state.split.personas[idx].nombre + '</span>' +
                '<div class="lmd-pos-ov-total">' + fmt(total) + '</div>' +
            '</div>' +
            '<div class="lmd-pos-efectivo-body">' +
                '<div class="lmd-pos-bill-shortcuts">' +
                    '<button class="lmd-pos-bill-btn lmd-pos-bill-btn--exacto" onclick="pos.seleccionarBilleteSplit(' + total.toFixed(2) + ',' + total.toFixed(2) + ',' + idx + ')">Exacto</button>' +
                    [5, 10, 20, 50, 100].map(function (b) {
                        return '<button class="lmd-pos-bill-btn" onclick="pos.seleccionarBilleteSplit(' + b + ',' + total.toFixed(2) + ',' + idx + ')">' + icon('banknote') + ' $' + b + '</button>';
                    }).join('') +
                '</div>' +
                '<div class="lmd-pos-keypad">' +
                    '<div class="lmd-pos-keypad__display" id="lmd-pos-keypad-display">$0.00</div>' +
                    '<div class="lmd-pos-cambio" id="lmd-pos-cambio"></div>' +
                    '<div class="lmd-pos-keypad__grid">' +
                        [1,2,3,4,5,6,7,8,9,'.',0,'⌫'].map(function (k) {
                            return '<button class="lmd-pos-keypad__btn" onclick="pos.keypadInput(\'' + k + '\',' + total.toFixed(2) + ')">' + (k === '⌫' ? icon('delete') : k) + '</button>';
                        }).join('') +
                    '</div>' +
                    '<div class="lmd-pos-keypad__actions">' +
                        '<button class="lmd-pos-keypad__btn lmd-pos-keypad__btn--back" onclick="pos.cobrarSiguientePersona()">Volver</button>' +
                        '<button class="lmd-pos-keypad__btn lmd-pos-keypad__btn--confirm" id="lmd-keypad-confirmar" onclick="pos.confirmarEfectivoSplit(' + idx + ',' + total.toFixed(2) + ')" disabled>Confirmar</button>' +
                    '</div>' +
                '</div>' +
            '</div>';
        cerrarOverlay('splitdetalle');
        abrirOverlay('splitdetalle', html, { closeOnBackdrop: false });
    }

    function seleccionarBilleteSplit(valor, total, idx) {
        keypadValue = parseFloat(valor).toFixed(2).replace(/\.00$/, '');
        _actualizarKeypad(total);
    }

    async function confirmarEfectivoSplit(idx, total) {
        var recibido = parseFloat(keypadValue || '0');
        if (recibido < total) { lmdToast('Monto insuficiente', 'error'); return; }
        var cambio = recibido - total;
        if (cambio > 0) lmdToast('Cambio: ' + fmt(cambio), 'info');
        var persona = state.split.personas[idx];
        if (persona.cuentaId) {
            var csrf = document.querySelector('input[name="__RequestVerificationToken"]');
            var form = new FormData();
            form.append('__RequestVerificationToken', csrf ? csrf.value : '');
            form.append('cuentaId', persona.cuentaId);
            form.append('metodoPago', 'Efectivo');
            form.append('propinaMonto', '0');
            try { await fetch('?handler=PagarCuentaJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } }); } catch (e) {}
        }
        persona.metodoPago = 'efectivo';
        persona.monto = recibido;
        persona.pagado = true;
        cobrarSiguientePersona();
    }

    // ═══════════════════════════════════════════════════
    // FINALIZAR PAGO
    // ═══════════════════════════════════════════════════
    async function finalizarPago(metodo, monto, referencia) {
        if (_pagoEnProceso) return;
        _pagoEnProceso = true;
        state.pagoMetodo = metodo;
        state.pagoMonto = monto;
        state.pagoReferencia = referencia;

        try {
            if (state.pedidoActual && state.pedidoActual.id) {
                var csrf = document.querySelector('input[name="__RequestVerificationToken"]');
                var form = new FormData();
                form.append('__RequestVerificationToken', csrf ? csrf.value : '');
                form.append('pedidoId', state.pedidoActual.id);
                form.append('metodoPago', metodo || 'efectivo');
                if (monto != null) form.append('monto', monto.toString());
                if (referencia) form.append('referencia', referencia);
                if (state.propinaMonto) form.append('propinaMonto', state.propinaMonto.toString());
                try {
                    var res = await fetch('?handler=PagarJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
                    var data = await res.json().catch(function () { return null; });
                    if (!res.ok) {
                        var errMsg = data && data.error ? data.error : 'Error al registrar pago';
                        lmdToast(errMsg, 'error');
                        // Si el pedido ya fue pagado no reabrir el overlay de pago (evita bucle)
                        var yaFinalizado = errMsg && (errMsg.toLowerCase().includes('pagado') || errMsg.toLowerCase().includes('cancelado') || errMsg.toLowerCase().includes('despachado'));
                        if (yaFinalizado) {
                            cerrarTodasOverlaysPago();
                            await Promise.all([refrescarMesas(), refrescarPedidosSinMesa()]);
                            state.pagado = true;
                            renderProductos();
                        } else {
                            abrirOverlayPago();
                        }
                        return;
                    }
                    // Éxito: SIEMPRE cerrar overlays y marcar como pagado
                    await Promise.all([refrescarMesas(), refrescarPedidosSinMesa()]);
                    state.pagado = true;
                    cerrarTodasOverlaysPago();
                    renderProductos();
                    mostrarPantalla('productos');
                    if (data && data.mensaje) lmdToast(data.mensaje, 'success');
                    if (data && data.ticketHtml) {
                        mostrarTicketModal(data.ticketHtml);
                    } else {
                        lmdToast('Pago registrado. Presiona Finalizar para el comprobante.', 'success');
                    }
                    return;
                } catch (e) { lmdToast('Error de conexión', 'error'); cerrarTodasOverlaysPago(); return; }
            }

            await Promise.all([refrescarMesas(), refrescarPedidosSinMesa()]);
            state.pagado = true;
            cerrarTodasOverlaysPago();
            renderProductos();
            mostrarPantalla('productos');
            lmdToast('Pago registrado. Presiona Finalizar para el comprobante.', 'success');
        } finally {
            _pagoEnProceso = false;
        }
    }

    function mostrarTicketModal(html) {
        // Nunca dejar tickets previos apilados que bloqueen la pantalla.
        limpiarCapasHuerfanas();
        var overlay = document.createElement('div');
        overlay.className = 'lmd-ticket-modal-backdrop';
        overlay.innerHTML =
            '<div class="lmd-ticket-modal">' +
                '<div class="lmd-ticket-modal-header">' +
                    '<span>' + icon('receipt') + ' Ticket de compra</span>' +
                    '<div class="lmd-ticket-modal-actions">' +
                        '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--primary" onclick="this.closest(\'.lmd-ticket-modal\').querySelector(\'iframe\').contentWindow.print()">' + icon('printer') + ' Imprimir</button>' +
                        '<button class="lmd-pos-ov-btn" onclick="this.closest(\'.lmd-ticket-modal-backdrop\').remove(); pos.nuevaOrden();">' + icon('check') + ' Listo</button>' +
                    '</div>' +
                '</div>' +
                '<iframe class="lmd-ticket-modal-frame" srcdoc="' + html.replace(/"/g, '&quot;') + '" sandbox="allow-same-origin allow-scripts"></iframe>' +
            '</div>';
        document.body.appendChild(overlay);

        function cerrar() {
            overlay.remove();
            document.removeEventListener('keydown', onKey);
            nuevaOrden();
        }
        function onKey(e) { if (e.key === 'Escape') cerrar(); }
        overlay.addEventListener('click', function (e) { if (e.target === overlay) cerrar(); });
        document.addEventListener('keydown', onKey);
    }

    // ═══════════════════════════════════════════════════
    // DOCUMENTOS — overlay tras Finalizar
    // ═══════════════════════════════════════════════════
    var DOCUMENTOS = [
        { codigo: 'ticket',      label: 'Ticket',       icon: 'receipt',     sub: 'Imprimir / PDF' },
        { codigo: 'fcf',         label: 'Factura C.F.', icon: 'file-text',   sub: 'Consumidor Final' },
        { codigo: 'ccf',         label: 'CCF',          icon: 'file-check',  sub: 'Crédito Fiscal' },
        { codigo: 'nota-credito',label: 'Nota Crédito', icon: 'file-minus',  sub: 'Anulación/Ajuste' },
        { codigo: 'email',       label: 'Email',        icon: 'mail',        sub: 'Factura electrónica' },
        { codigo: 'sin-doc',     label: 'Sin doc.',     icon: 'x-circle',    sub: 'Finalizar sin comprobante' }
    ];

    function abrirOverlayDocumentos() {
        var total = totalPedidoActual();
        var propina = state.propinaMonto || 0;
        var btnsHtml = DOCUMENTOS.map(function (d) {
            return '<button class="lmd-pos-pm-btn" onclick="pos.emitirDocumento(\'' + d.codigo + '\')">' +
                '<span class="lmd-pos-pm-btn__icon">' + icon(d.icon) + '</span>' +
                '<span class="lmd-pos-pm-btn__label">' + d.label + '</span>' +
                '<span class="lmd-pos-pm-btn__sub">' + d.sub + '</span>' +
            '</button>';
        }).join('');

        var resumenPago = propina > 0
            ? fmt(total) + ' + ' + fmt(propina) + ' propina = ' + fmt(total + propina)
            : fmt(total);
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<span class="lmd-pos-ov-title">' + icon('receipt') + ' Comprobante</span>' +
                '<div class="lmd-pos-ov-total">' + resumenPago + ' · ' + (state.pagoMetodo || '').toUpperCase() + '</div>' +
            '</div>' +
            '<div class="lmd-pos-pm-grid">' + btnsHtml + '</div>';

        abrirOverlay('documentos', html, { wide: true, closeOnBackdrop: false });
    }

    async function emitirDocumento(codigo) {
        cerrarOverlay('documentos');
        if (codigo === 'sin-doc') { nuevaOrden(); return; }

        if (state.pedidoActual && state.pedidoActual.id && (codigo === 'ticket' || codigo === 'fcf' || codigo === 'ccf')) {
            var csrf = document.querySelector('input[name="__RequestVerificationToken"]');
            var form = new FormData();
            form.append('__RequestVerificationToken', csrf ? csrf.value : '');
            form.append('pedidoId', state.pedidoActual.id);
            try {
                var res = await fetch('?handler=TicketHtmlJson', { method: 'POST', body: form, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
                if (res.ok) {
                    var d = await res.json();
                    if (d && d.html) {
                        var win = window.open('', '_blank', 'width=420,height=600,scrollbars=yes');
                        if (win) {
                            win.document.write(d.html);
                            win.document.close();
                            win.focus();
                            setTimeout(function () { win.print(); }, 400);
                        }
                    }
                }
            } catch (e) {}
        }

        var labels = { ticket: 'Ticket generado', fcf: 'Factura C.F. emitida', ccf: 'CCF emitido', 'nota-credito': 'Nota de crédito registrada', email: 'Factura enviada por correo' };
        lmdToast(labels[codigo] || 'Documento generado', 'success');
        nuevaOrden();
    }

    // ═══════════════════════════════════════════════════
    // NUEVA ORDEN
    // ═══════════════════════════════════════════════════
    async function nuevaOrden() {
        cerrarOverlay('documentos');
        cerrarTodasOverlaysPago();
        state.tipoServicio = null;
        state.mesaId = null;
        state.mesaNumero = null;
        state.entrega = { direccion: '', telefono: '', repartidorId: '' };
        _resetPedido();
        await Promise.all([refrescarMesas(), refrescarPedidosSinMesa()]);
        renderSeleccion();
        mostrarPantalla('seleccion');
    }

    // ═══════════════════════════════════════════════════
    // MODIFIER MODAL
    // ═══════════════════════════════════════════════════
    var _mod = { productoId: null, productoNombre: '', ingredientes: [], alergias: [], alergenosProducto: [], notaCustom: '' };

    async function abrirModificadores(productoId) {
        var prod = productoPorId(productoId);
        if (!prod) return;
        _mod.productoId = productoId;
        _mod.productoNombre = prod.nombre;
        _mod.ingredientes = [];
        _mod.alergias = [];
        _mod.alergenosProducto = [];
        _mod.notaCustom = '';

        try {
            var r1 = await fetch('?handler=IngredientesProductoJson&productoId=' + encodeURIComponent(productoId));
            var d1 = await r1.json();
            _mod.ingredientes = (d1.ingredientes || []).map(function (ing) {
                return { id: ing.id, nombre: ing.nombre, cantidad: ing.cantidad, estado: 'normal' };
            });
        } catch (e) {}

        try {
            var r2 = await fetch('?handler=AlergenosProductoJson&productoId=' + encodeURIComponent(productoId));
            _mod.alergenosProducto = await r2.json() || [];
        } catch (e) { _mod.alergenosProducto = []; }

        var lineaExistente = state.lineas.find(function (l) { return l.productoId === productoId; });
        if (lineaExistente && lineaExistente.modificacionesJson) {
            try {
                var modsExistentes = JSON.parse(lineaExistente.modificacionesJson);
                modsExistentes.forEach(function (m) {
                    if (m.accion === 'confirmado') return;
                    if (m.accion === 'alergia') {
                        var alergia = String(m.ingredienteNombre || '').toLowerCase();
                        if (alergia && _mod.alergias.indexOf(alergia) < 0) _mod.alergias.push(alergia);
                        return;
                    }
                    var ing = _mod.ingredientes.find(function (i) { return i.id === m.ingredienteId; });
                    if (ing && (m.accion === 'quitar' || m.accion === 'extra')) ing.estado = m.accion === 'quitar' ? 'quitado' : 'extra';
                });
            } catch (e) {}
        }
        if (lineaExistente && lineaExistente.notas) _mod.notaCustom = lineaExistente.notas;

        renderModificadorModal();
    }

    function renderModificadorModal() {
        cerrarOverlay('modificador');

        var alergenosHtml = _mod.alergenosProducto.length > 0
            ? _mod.alergenosProducto.map(function (a) {
                var activo = _mod.alergias.indexOf(String(a.nombre || '').toLowerCase()) >= 0;
                return '<button class="lmd-mod-alergia-btn' + (activo ? ' activo' : '') + '" onclick="pos.toggleAlergia(\'' + String(a.nombre || '').toLowerCase().replace(/'/g, "\\'") + '\')">' + a.nombre + '</button>';
              }).join('')
            : '<span class="lmd-mod-empty">Sin alérgenos registrados</span>';

        var ingsHtml = _mod.ingredientes.length > 0
            ? _mod.ingredientes.map(function (ing) {
                var est = ing.estado || 'normal';
                return '<div class="lmd-mod-ing-row lmd-mod-ing-row--' + est + '">' +
                    '<span class="lmd-mod-ing-nombre">' + ing.nombre + ' <small>(' + ing.cantidad + ')</small></span>' +
                    '<div class="lmd-mod-ing-acciones">' +
                        '<button class="lmd-mod-ing-btn lmd-mod-ing-btn--extra' + (est === 'extra' ? ' activo' : '') + '" onclick="pos.toggleEstadoIngrediente(\'' + ing.id + '\', \'extra\')" title="Extra">' + icon('plus-circle') + '</button>' +
                        '<button class="lmd-mod-ing-btn lmd-mod-ing-btn--quitar' + (est === 'quitado' ? ' activo' : '') + '" onclick="pos.toggleEstadoIngrediente(\'' + ing.id + '\', \'quitado\')" title="Quitar">' + icon('minus-circle') + '</button>' +
                    '</div>' +
                '</div>';
              }).join('')
            : '<span class="lmd-mod-empty">Sin ingredientes registrados</span>';

        var html =
            '<div class="lmd-pos-ov-header">' +
                '<button class="lmd-pos-ov-back" onclick="pos.cerrarModificadores()" title="Volver sin agregar">' + icon('arrow-left') + '</button>' +
                '<span class="lmd-pos-ov-title">' + icon('edit-3') + ' ' + _mod.productoNombre + '</span>' +
                '<button class="lmd-pos-ov-close" onclick="pos.cerrarModificadores()">' + icon('x') + '</button>' +
            '</div>' +
            '<div class="lmd-mod-body">' +
                '<div class="lmd-mod-alerta">Debes confirmar ingredientes antes de enviar este producto a cocina.</div>' +
                '<div class="lmd-mod-section">' +
                    '<div class="lmd-mod-section__title">' + icon('alert-triangle') + ' Alergias / restricciones</div>' +
                    '<div class="lmd-mod-alergias">' + alergenosHtml + '</div>' +
                '</div>' +
                '<div class="lmd-mod-section">' +
                    '<div class="lmd-mod-section__title">' + icon('list') + ' Ingredientes</div>' +
                    '<div class="lmd-mod-ings">' + ingsHtml + '</div>' +
                '</div>' +
                '<div class="lmd-mod-section">' +
                    '<div class="lmd-mod-section__title">' + icon('message-square') + ' Nota para cocina</div>' +
                    '<textarea class="lmd-mod-nota" rows="2" placeholder="Ej: sin sal, bien cocido..." oninput="pos._setNotaCustom(this.value)">' + (_mod.notaCustom || '') + '</textarea>' +
                '</div>' +
                '<div class="lmd-mod-actions">' +
                    '<button class="lmd-mod-confirmar lmd-mod-confirmar--ghost" onclick="pos.confirmarModificadores(true)">' + icon('check') + ' Agregar original</button>' +
                    '<button class="lmd-mod-confirmar" onclick="pos.confirmarModificadores(false)">' + icon('check-circle') + ' Confirmar cambios</button>' +
                '</div>' +
            '</div>';

        abrirOverlay('modificador', html, { bottom: true });
    }

    function toggleEstadoIngrediente(id, estado) {
        var ing = _mod.ingredientes.find(function (i) { return i.id === id; });
        if (!ing) return;
        // BUG FIX: si ya está en ese estado, volver a normal; si no, cambiar
        ing.estado = ing.estado === estado ? 'normal' : estado;
        renderModificadorModal();
    }

    function cambiarReemplazo() {
        lmdToast('Los reemplazos se deshabilitaron para evitar ingredientes duplicados.', 'info');
    }

    function toggleAlergia(alergia) {
        var key = String(alergia || '').toLowerCase();
        var idx = _mod.alergias.indexOf(key);
        if (idx >= 0) _mod.alergias.splice(idx, 1);
        else _mod.alergias.push(key);
        renderModificadorModal();
    }

    function _setNotaCustom(val) { _mod.notaCustom = val; }

    function cerrarModificadores() { cerrarOverlay('modificador'); }

    function confirmarModificadores(original) {
        var prod = productoPorId(_mod.productoId);
        var mods = [];
        var notas = _mod.notaCustom && _mod.notaCustom.trim() ? _mod.notaCustom.trim() : null;

        if (original === true) {
            mods = JSON.parse(crearConfirmacionOriginal());
        } else {
            _mod.ingredientes.forEach(function (ing) {
                var est = ing.estado || 'normal';
                if (est === 'quitado') {
                    mods.push({ ingredienteId: ing.id, ingredienteNombre: ing.nombre, accion: 'quitar', motivo: 'preferencia', ingredienteReemplazoId: null, ingredienteReemplazoNombre: null });
                } else if (est === 'extra') {
                    mods.push({ ingredienteId: ing.id, ingredienteNombre: ing.nombre, accion: 'extra', motivo: 'preferencia', ingredienteReemplazoId: null, ingredienteReemplazoNombre: null });
                }
            });
            _mod.alergias.forEach(function (alergia) {
                mods.push({ ingredienteId: '00000000-0000-0000-0000-000000000000', ingredienteNombre: alergia, accion: 'alergia', motivo: 'alergia', ingredienteReemplazoId: null, ingredienteReemplazoNombre: null });
            });

            if (mods.length === 0 && !notas) {
                lmdToast('Elige un cambio o usa Agregar original.', 'error');
                return;
            }
            if (mods.length === 0 && notas) {
                mods = JSON.parse(crearConfirmacionOriginal());
            }
        }

        var linea = state.lineas.find(function (l) { return l.productoId === _mod.productoId; });
        if (!linea && prod) {
            state.lineas.push({ productoId: _mod.productoId, productoNombre: _mod.productoNombre, cantidad: 1, precioUnitario: prod.precio || 0 });
            linea = state.lineas[state.lineas.length - 1];
        }

        if (linea) {
            linea.modificacionesJson = JSON.stringify(mods);
            linea.notas = notas;
            linea.tieneModificaciones = original !== true || !!notas;
            linea.ingredientesConfirmados = true;
            cerrarModificadores();
            renderProductos();
            lmdToast(original === true ? 'Producto original confirmado' : 'Modificaciones aplicadas', 'success');
        }
    }

    // ═══════════════════════════════════════════════════
    // CAMBIAR TIPO DE SERVICIO
    // ═══════════════════════════════════════════════════
    function cambiarServicio() {
        if (state.pagado) return;
        if (state.pedidoActual) {
            lmdToast('El pedido ya fue registrado. Cancela la orden para cambiar el servicio.', 'error');
            return;
        }
        abrirOverlayCambiarServicio();
    }

    function abrirOverlayCambiarServicio() {
        var mesas = window.__lmdMesasDisponibles || [];
        var mesasHtml = mesas.filter(function (m) { return m.estado === 'Disponible'; }).map(function (m) {
            var seleccionada = state.mesaId === m.id;
            return '<button class="lmd-pos-mesa-card lmd-pos-mesa-card--disponible' + (seleccionada ? ' lmd-pos-mesa-card--seleccionada' : '') + '" onclick="pos.cambiarAMesa(\'' + m.id + '\',' + m.numero + ')">' +
                '<span class="lmd-pos-mesa-card__numero">' + m.numero + '</span>' +
                '<span class="lmd-pos-mesa-card__capacidad">' + m.capacidad + ' pax</span>' +
            '</button>';
        }).join('');

        var esComerAqui = state.tipoServicio === 'ComerAqui';
        var esDelivery = state.tipoServicio === 'Domicilio';
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<span class="lmd-pos-ov-title">' + icon('refresh-cw') + ' Cambiar servicio</span>' +
                '<button class="lmd-pos-ov-close" onclick="pos.cerrarCambiarServicio()">' + icon('x') + '</button>' +
            '</div>' +
            '<div class="lmd-pos-cambiar-servicio-body">' +
                '<button class="lmd-pos-cambiar-servicio-opcion' + (!esComerAqui && !esDelivery ? ' lmd-pos-cambiar-servicio-opcion--activa' : '') + '" onclick="pos.cambiarAParaLlevar()">' +
                    icon('package') + '<span>Retiro en caja</span>' +
                '</button>' +
                '<button class="lmd-pos-cambiar-servicio-opcion' + (esDelivery ? ' lmd-pos-cambiar-servicio-opcion--activa' : '') + '" onclick="pos.cambiarADelivery()">' +
                    icon('truck') + '<span>Delivery</span>' +
                '</button>' +
                '<div class="lmd-pos-cambiar-servicio-sep">— o selecciona una mesa —</div>' +
                '<div class="lmd-pos-mesas-grid lmd-pos-mesas-grid--mini">' +
                    (mesasHtml || '<span class="lmd-pos-empty">Sin mesas disponibles</span>') +
                '</div>' +
            '</div>';

        abrirOverlay('cambiarservicio', html, { closeOnBackdrop: true });
    }

    function cambiarAMesa(mesaId, numero) {
        var mesas = window.__lmdMesasDisponibles || [];
        var m = mesas.find(function (x) { return x.id === mesaId; });
        if (m && m.estado !== 'Disponible') { lmdToast('Mesa ocupada — selecciona otra', 'error'); return; }
        state.tipoServicio = 'ComerAqui';
        state.mesaId = mesaId;
        state.mesaNumero = numero;
        state.entrega = { direccion: '', telefono: '', repartidorId: '' };
        cerrarOverlay('cambiarservicio');
        renderProductos();
        lmdToast('Cambiado a Mesa ' + numero, 'success');
    }

    function cambiarAParaLlevar() {
        state.tipoServicio = 'ParaLlevar';
        state.mesaId = null;
        state.mesaNumero = null;
        state.entrega = { direccion: '', telefono: '', repartidorId: '' };
        cerrarOverlay('cambiarservicio');
        renderProductos();
        lmdToast('Cambiado a Para llevar', 'success');
    }

    function cambiarADelivery() {
        state.tipoServicio = 'Domicilio';
        state.mesaId = null;
        state.mesaNumero = null;
        cerrarOverlay('cambiarservicio');
        abrirConfigEnvio(true);
        renderProductos();
    }

    function cerrarCambiarServicio() { cerrarOverlay('cambiarservicio'); }

    // ═══════════════════════════════════════════════════
    // PAGO RECHAZADO — recuperación de error
    // ═══════════════════════════════════════════════════
    function simularRechazo(metodo) {
        cerrarOverlay(metodo === 'qr' ? 'qr' : 'tarjeta');
        var mensajes = {
            tarjeta: 'Tarjeta rechazada por el emisor',
            qr: 'Transferencia no recibida — tiempo agotado'
        };
        abrirOverlayErrorPago(metodo, mensajes[metodo] || 'Pago rechazado');
    }

    function abrirOverlayErrorPago(metodo, mensaje) {
        var html =
            '<div class="lmd-pos-ov-header">' +
                '<span class="lmd-pos-ov-title lmd-pos-ov-title--error">' + icon('alert-circle') + ' Pago rechazado</span>' +
            '</div>' +
            '<div class="lmd-pos-error-pago-body">' +
                '<div class="lmd-pos-error-pago-icon">' + icon('x-circle') + '</div>' +
                '<p class="lmd-pos-error-pago-mensaje">' + mensaje + '</p>' +
                '<div class="lmd-pos-error-pago-actions">' +
                    '<button class="lmd-pos-ov-btn lmd-pos-ov-btn--primary" onclick="pos.reintentarPago(\'' + metodo + '\')">' + icon('refresh-cw') + ' Reintentar</button>' +
                    '<button class="lmd-pos-ov-btn" onclick="pos.volverAMetodos()">' + icon('credit-card') + ' Otro método</button>' +
                '</div>' +
            '</div>';

        abrirOverlay('errorpago', html, { closeOnBackdrop: false });
    }

    function reintentarPago(metodo) {
        cerrarOverlay('errorpago');
        var total = totalPedidoActual();
        if (metodo === 'tarjeta') abrirOverlayTarjeta(total);
        else if (metodo === 'qr') abrirOverlayQR(total);
        else abrirOverlayPago();
    }

    // ═══════════════════════════════════════════════════
    // SignalR + Init
    // ═══════════════════════════════════════════════════
    async function initSignalR() {
        if (typeof signalR === 'undefined') return;
        try {
            connection = new signalR.HubConnectionBuilder()
                .withUrl('/hubs/pedidos')
                .withAutomaticReconnect()
                .build();
            connection.on('EstadoCambiado', function (pedidoId, nuevoEstado) {
                if (nuevoEstado === 'Pagado' || nuevoEstado === 'Despachado' || nuevoEstado === 'EnCobro' || nuevoEstado === 'Listo' || nuevoEstado === 'EnPreparacion') {
                    refrescarSeleccionSiVisible();
                    if (state.pedidoActual && String(state.pedidoActual.id) === String(pedidoId)) {
                        state.pedidoActual.estado = nuevoEstado;
                        if (nuevoEstado === 'Listo') lmdToast('Cocina marcó este pedido como listo para cobrar.', 'success');
                    }
                }
            });
            connection.on('PedidoCreado', function () { refrescarSeleccionSiVisible(); });
            connection.on('ItemRecuperado', function (orden) {
                if (orden && state.pedidoActual && orden.pedidoId === state.pedidoActual.id) {
                    lmdToast('Cocina recuperó un item — orden aún en preparación', 'warn');
                }
            });
            connection.on('ProductoAgotado', function (productoId) {
                marcarProductoAgotadoEnUI(productoId, '');
            });
            connection.on('productoAgotado', function (productoId, nombreProducto) {
                marcarProductoAgotadoEnUI(productoId, nombreProducto);
            });
            connection.on('productoReactivado', function (productoId) {
                var prods = window.__lmdProductosDisponibles || [];
                prods.forEach(function (p) { if (String(p.id) === String(productoId)) { p.agotado = false; } });
                document.querySelectorAll('[data-producto-id="' + productoId + '"]').forEach(function (el) {
                    el.classList.remove('lmd-producto-agotado');
                    var badge = el.querySelector('.lmd-badge-agotado');
                    if (badge) badge.remove();
                    el.style.pointerEvents = '';
                });
                renderProductos && renderProductos();
            });
            await connection.start();
        } catch (e) {}
    }

    function marcarProductoAgotadoEnUI(productoId, nombre) {
        var prods = window.__lmdProductosDisponibles || [];
        var marcado = false;
        prods.forEach(function (p) {
            if (String(p.id) === String(productoId)) { p.agotado = true; marcado = true; }
        });
        document.querySelectorAll('[data-producto-id="' + productoId + '"]').forEach(function (el) {
            el.classList.add('lmd-producto-agotado');
            el.style.pointerEvents = 'none';
            if (!el.querySelector('.lmd-badge-agotado')) {
                var badge = document.createElement('span');
                badge.className = 'lmd-badge-agotado';
                badge.textContent = 'AGOTADO';
                el.appendChild(badge);
            }
        });
        if (marcado) {
            renderProductos();
            lmdToast((nombre ? nombre + ': ' : '') + 'producto marcado como agotado (86)', 'warn');
        }
    }

    async function refrescarMesas() {
        try {
            var res = await fetch('?handler=MesasJson', { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (res.ok) {
                var data = await res.json();
                if (data && data.mesas) window.__lmdMesasDisponibles = data.mesas;
                window.__lmdParaLlevar = (data && data.paraLlevar) ? data.paraLlevar : [];
            }
        } catch (e) {}
    }

    async function refrescarPedidosSinMesa() {
        try {
            var res = await fetch('?handler=PedidosSinMesaJson', { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (res.ok) {
                var data = await res.json();
                window.__lmdPedidosSinMesaActivos = (data && data.pedidos) ? data.pedidos : [];
            }
        } catch (e) {}
    }

    async function refrescarSeleccionSiVisible() {
        await Promise.all([refrescarMesas(), refrescarPedidosSinMesa()]);
        if (state.pantalla === 'seleccion') renderSeleccion();
    }

    async function volverASeleccion() {
        cerrarOverlay('delivery');
        cerrarOverlay('cambiarservicio');
        cerrarOverlay('modificador');
        cerrarOverlay('documentos');
        cerrarTodasOverlaysPago();
        state.tipoServicio = null;
        state.mesaId = null;
        state.mesaNumero = null;
        state.entrega = { direccion: '', telefono: '', repartidorId: '' };
        _resetPedido();
        await Promise.all([refrescarMesas(), refrescarPedidosSinMesa()]);
        renderSeleccion();
        mostrarPantalla('seleccion');
    }

    // ── Public API ──────────────────────────────────────
    window.pos = {
        seleccionarMesa, seleccionarParaLlevar, seleccionarDelivery, retomarPedidoSinMesa, volverASeleccion, cobrarParaLlevar,
        filtrarCategoria, agregarAlCarrito, incrementarItem, decrementarItem, eliminarDelCarrito,
        cancelarOrden, confirmarListo, irAPago,
        cerrarPago, procesarPago,
        seleccionarBillete, keypadInput, keypadConfirmar,
        volverAMetodos, simularTarjeta, confirmarTarjeta, simularQR, confirmarOtro,
        abrirSplit, volverAPago, splitIgualitario, splitPorPersona, splitMixto,
        toggleEstadoIngrediente, cambiarReemplazo,
        iniciarAsignacionSplit, asignarItemSplit, confirmarAsignacionSplit, _renderSplitNPicker, _renderSplitAsignacion,
        confirmarAnulacion,
        ajustarSplitN, iniciarSplitIgualitario, cobrarSiguientePersona,
        pagarPersonaSplit, seleccionarBilleteSplit, confirmarEfectivoSplit,
        emitirDocumento, nuevaOrden, mostrarTicketModal,
        confirmarPropina,
        abrirModificadores, toggleAlergia, _setNotaCustom,
        cerrarModificadores, confirmarModificadores,
        cambiarServicio, cambiarAMesa, cambiarAParaLlevar, cambiarADelivery, cerrarCambiarServicio,
        abrirConfigEnvio, guardarConfigEnvio, cerrarConfigEnvio, quitarEnvio,
        simularRechazo, reintentarPago
    };

    function actualizarTiemposEnMesa() {
        document.querySelectorAll('[data-pedido-fecha]').forEach(function (badge) {
            var span = badge.querySelector('.lmd-tab-tiempo');
            if (!span) return;
            var min = Math.floor((Date.now() - new Date(badge.dataset.pedidoFecha).getTime()) / 60000);
            span.textContent = min > 0 ? min + ' min' : 'Tab';
        });
    }

    var _graciaRefrescada = false;
    function actualizarTimersGracia() {
        var badges = document.querySelectorAll('[data-gracia-hasta]');
        var hayVencidos = false;
        badges.forEach(function (badge) {
            var span = badge.querySelector('.lmd-gracia-tiempo');
            if (!span) return;
            var secsLeft = Math.max(0, Math.floor((new Date(badge.dataset.graciaHasta).getTime() - Date.now()) / 1000));
            if (secsLeft <= 0) { hayVencidos = true; return; }
            var mins = Math.floor(secsLeft / 60), secs = secsLeft % 60;
            span.textContent = mins + ':' + (secs < 10 ? '0' : '') + secs;
        });
        if (hayVencidos && !_graciaRefrescada) {
            _graciaRefrescada = true;
            refrescarMesas()
                .then(function () { renderSeleccion(); })
                .catch(function () {})
                .finally(function () { _graciaRefrescada = false; });
        }
    }

    document.addEventListener('DOMContentLoaded', async function () {
        await Promise.all([refrescarMesas(), refrescarPedidosSinMesa()]);
        renderSeleccion();
        initSignalR();
        setInterval(actualizarTiemposEnMesa, 30000);
        setInterval(actualizarTimersGracia, 1000);
        setInterval(refrescarSeleccionSiVisible, 30000);

        window.addEventListener('offline', function () {
            var banner = document.getElementById('lmd-offline-banner');
            if (banner) { banner.style.display = ''; banner.classList.add('visible'); }
        });
        window.addEventListener('online', function () {
            var banner = document.getElementById('lmd-offline-banner');
            if (banner) {
                banner.classList.remove('visible');
                setTimeout(function () { if (!banner.classList.contains('visible')) banner.style.display = 'none'; }, 500);
            }
        });
    });
})();
