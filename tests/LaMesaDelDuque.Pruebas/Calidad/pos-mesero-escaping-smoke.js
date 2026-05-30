const fs = require('fs');
const vm = require('vm');

const [, , posPath, meseroPath] = process.argv;

if (!posPath || !meseroPath) {
  throw new Error('Usage: node pos-mesero-escaping-smoke.js <pos.js> <mesero.js>');
}

function createFakeDocument() {
  const elements = new Map();
  const assignments = [];
  const listeners = {};

  function makeElement(id) {
    return {
      id,
      children: [],
      dataset: {},
      style: {},
      classList: {
        add() {},
        remove() {},
        toggle() {},
        contains() { return false; }
      },
      addEventListener() {},
      appendChild(child) {
        this.children.push(child);
        return child;
      },
      remove() {},
      querySelector() { return null; },
      querySelectorAll() { return []; },
      set innerHTML(value) {
        this._innerHTML = String(value ?? '');
        assignments.push(this._innerHTML);
      },
      get innerHTML() {
        return this._innerHTML || '';
      },
      set textContent(value) {
        this._textContent = String(value ?? '');
      },
      get textContent() {
        return this._textContent || '';
      }
    };
  }

  const document = {
    _listeners: listeners,
    _assignments: assignments,
    body: makeElement('body'),
    createElement(tag) {
      return makeElement(tag);
    },
    getElementById(id) {
      if (!elements.has(id)) elements.set(id, makeElement(id));
      return elements.get(id);
    },
    querySelector(selector) {
      if (selector === 'input[name="__RequestVerificationToken"]') {
        return { value: 'csrf-token' };
      }
      return null;
    },
    querySelectorAll() {
      return [];
    },
    addEventListener(eventName, callback) {
      listeners[eventName] = callback;
    },
    allAssignedHtml() {
      return assignments.join('\n');
    }
  };

  return document;
}

class FakeFormData {
  constructor() {
    this.values = [];
  }

  append(key, value) {
    this.values.push([key, value]);
  }
}

function response(jsonPayload, ok = true) {
  return {
    ok,
    async json() {
      return jsonPayload;
    }
  };
}

function runScript(scriptPath, windowValues, fetchImpl) {
  const document = createFakeDocument();
  const window = Object.assign({
    document,
    addEventListener() {},
    lmdToast() {},
    lmdConfirm: async () => true
  }, windowValues || {});

  const context = {
    window,
    document,
    console,
    Intl,
    Date,
    Math,
    String,
    Number,
    Array,
    Object,
    Promise,
    JSON,
    setInterval() { return 0; },
    clearInterval() {},
    requestAnimationFrame(callback) { callback(); },
    FormData: FakeFormData,
    fetch: fetchImpl || (async () => response({})),
    lmdToast: window.lmdToast,
    signalR: undefined
  };
  context.global = context;
  context.window.window = context.window;

  vm.createContext(context);
  vm.runInContext(fs.readFileSync(scriptPath, 'utf8'), context, { filename: scriptPath });

  return { context, document };
}

function assertNoRawHtml(label, html, forbiddenFragments, requiredEncodedFragments) {
  const lower = html.toLowerCase();
  const failures = [];

  for (const fragment of forbiddenFragments) {
    if (lower.includes(fragment.toLowerCase())) {
      failures.push(`raw fragment still present: ${fragment}`);
    }
  }

  for (const fragment of requiredEncodedFragments) {
    if (!lower.includes(fragment.toLowerCase())) {
      failures.push(`encoded fragment missing: ${fragment}`);
    }
  }

  if (failures.length > 0) {
    const sample = html.slice(0, 1200);
    throw new Error(`${label} did not escape controlled HTML:\n- ${failures.join('\n- ')}\n\nRendered sample:\n${sample}`);
  }
}

async function verifyPosEscaping() {
  const dangerousProduct = 'Pupusa <img src=x onerror="alert(1)"> & queso';
  const dangerousCategory = 'Especiales <script>alert(2)</script>';
  const dangerousPromo = 'Promo <svg onload=alert(3)></svg>';

  const { context, document } = runScript(posPath, {
    __lmdMesasDisponibles: [],
    __lmdProductosDisponibles: [{
      id: 'prod-1',
      nombre: dangerousProduct,
      categoriaNombre: dangerousCategory,
      precio: 4.25,
      promoNombre: dangerousPromo,
      promoTipo: 'monto',
      promoDescuento: 1,
      tiempoPreparacionMin: 8
    }]
  });

  context.window.pos.seleccionarParaLlevar();
  context.window.pos.agregarAlCarrito('prod-1', dangerousProduct, 4.25);

  const html = document.allAssignedHtml();
  assertNoRawHtml(
    'POS',
    html,
    ['<img', '<script', '<svg onload'],
    ['&lt;img', '&lt;script', '&lt;svg onload']
  );
}

async function verifyMeseroEscaping() {
  const dangerousProduct = 'Taco <img src=x onerror="alert(4)"> & salsa';
  const dangerousCategory = 'Carta <script>alert(5)</script>';

  const mesas = [
    { id: 'mesa-1', numero: 1, capacidad: 2, estado: 'Disponible', zona: 'Terraza <script>alert(6)</script>' },
    { id: 'mesa-2', numero: 2, capacidad: 4, estado: 'Ocupada', pedidoActualId: 'pedido-1', pedidoEstado: 'Pendiente' }
  ];

  const { context, document } = runScript(
    meseroPath,
    {
      __lmdProductosMesero: [{
        id: 'prod-1',
        nombre: dangerousProduct,
        categoriaNombre: dangerousCategory,
        precio: 3.5
      }]
    },
    async (url) => {
      const value = String(url);
      if (value.includes('MesasJson')) {
        return response({ mesas });
      }
      if (value.includes('DetallesPedidoJson')) {
        return response({
          detalles: [{ id: 'detalle-1', productoNombre: dangerousProduct, cantidad: 1, subtotal: 3.5 }],
          total: 3.5,
          estado: 'Pendiente'
        });
      }
      return response({});
    }
  );

  await context.document._listeners.DOMContentLoaded();
  context.window.mesero.abrirNuevoPedido('mesa-1');
  context.window.mesero.addToCart('prod-1');
  await context.window.mesero.abrirDetalle('mesa-2');

  const html = document.allAssignedHtml();
  assertNoRawHtml(
    'Mesero',
    html,
    ['<img', '<script'],
    ['&lt;img', '&lt;script']
  );
}

(async () => {
  await verifyPosEscaping();
  await verifyMeseroEscaping();
  console.log('POS/Mesero escaping smoke passed');
})().catch((error) => {
  console.error(error && error.stack ? error.stack : error);
  process.exit(1);
});
