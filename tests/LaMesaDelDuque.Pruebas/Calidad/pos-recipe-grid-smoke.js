const fs = require('fs');
const vm = require('vm');

const [, , posPath] = process.argv;

if (!posPath) {
  throw new Error('Usage: node pos-recipe-grid-smoke.js <pos.js>');
}

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
    },
    get innerHTML() {
      return this._innerHTML || '';
    }
  };
}

function createDocument() {
  const elements = new Map();
  const document = {
    body: makeElement('body'),
    createElement: makeElement,
    getElementById(id) {
      if (!elements.has(id)) elements.set(id, makeElement(id));
      return elements.get(id);
    },
    querySelector(selector) {
      if (selector === 'input[name="__RequestVerificationToken"]') return { value: 'csrf-token' };
      return null;
    },
    querySelectorAll() { return []; },
    addEventListener() {}
  };
  return document;
}

class FakeFormData {
  constructor() {
    this.values = [];
    FakeFormData.instances.push(this);
  }

  append(key, value) {
    this.values.push([key, String(value ?? '')]);
  }
}

FakeFormData.instances = [];

const document = createDocument();
const window = {
  document,
  addEventListener() {},
  lmdToast() {},
  lmdConfirm: async () => true,
  __lmdMesasDisponibles: [],
  __lmdProductosDisponibles: [{
    id: 'prod-receta',
    nombre: 'Pizza receta',
    categoriaNombre: 'Comidas',
    precio: 10,
    tieneReceta: true
  }]
};

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
  fetch: async (url) => {
    const value = String(url);
    if (value.includes('IngredientesProductoJson')) return { async json() { return { ingredientes: [] }; } };
    if (value.includes('AlergenosProductoJson')) return { async json() { return []; } };
    if (value.includes('CrearJson')) return { ok: true, async json() { return { pedidoId: 'pedido-1', total: 20, detalles: [] }; } };
    return { ok: true, async json() { return {}; } };
  },
  lmdToast: window.lmdToast,
  signalR: undefined
};
context.global = context;
context.window.window = context.window;

vm.createContext(context);
vm.runInContext(fs.readFileSync(posPath, 'utf8'), context, { filename: posPath });

(async () => {
  context.window.pos.seleccionarParaLlevar();
  await context.window.pos.abrirModificadores('prod-receta');
  context.window.pos.confirmarModificadores();
  context.window.pos.agregarProductoDesdeGrid('prod-receta', 'Pizza receta', 10);

  const productScreen = document.getElementById('lmd-pos-screen-productos').innerHTML;
  if (!productScreen.includes('lmd-pos-cart-item__qty') || !productScreen.includes('>2</span>')) {
    throw new Error(`Expected confirmed recipe product grid tap to increment quantity to 2. Product screen HTML: ${productScreen}`);
  }

  await context.window.pos.abrirModificadores('prod-receta');
  context.window.pos.toggleAlergia('mani');
  context.window.pos.confirmarModificadores();
  await context.window.pos.confirmarListo();

  const formWithLineModifiers = FakeFormData.instances.find((form) =>
    form.values.some(([key]) => key === 'Vm.CrearPedido.Lineas[0].ModificacionesJson'));

  if (!formWithLineModifiers) {
    throw new Error('Expected POS to submit recipe modifiers for allergy-only confirmation.');
  }

  const [, modificacionesJson] = formWithLineModifiers.values.find(([key]) => key === 'Vm.CrearPedido.Lineas[0].ModificacionesJson');
  const modificaciones = JSON.parse(modificacionesJson);
  const acciones = modificaciones.map((mod) => mod.accion).sort();

  if (!acciones.includes('alergia') || !acciones.includes('confirmado')) {
    throw new Error(`Expected allergy-only recipe payload to include alergia and confirmado actions. Actual payload: ${modificacionesJson}`);
  }

  console.log('POS recipe grid smoke passed');
})().catch((error) => {
  console.error(error && error.stack ? error.stack : error);
  process.exit(1);
});
