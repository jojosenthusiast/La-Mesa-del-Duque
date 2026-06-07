const assert = require('assert');
const fs = require('fs');
const vm = require('vm');

const kdsJavaScriptPath = process.argv[2];
if (!kdsJavaScriptPath) throw new Error('Usage: node kds-runtime-smoke.js <path-to-cocina-kds.js>');

const toDatasetKey = name => name.replace(/^data-/, '').replace(/-([a-z])/g, (_, c) => c.toUpperCase());

class Element {
  constructor(tagName, document) {
    this.tagName = tagName.toUpperCase();
    this.ownerDocument = document;
    this.children = [];
    this.parentElement = null;
    this.dataset = {};
    this.style = {};
    this.eventHandlers = {};
    this._id = '';
    this._innerHTML = '';
    this._textContent = '';
    this._classes = new Set();
    this.value = '';
    this.classList = {
      add: (...names) => names.forEach(name => this._classes.add(name)),
      remove: (...names) => names.forEach(name => this._classes.delete(name)),
      toggle: (name, force) => {
        const add = force === undefined ? !this._classes.has(name) : !!force;
        add ? this._classes.add(name) : this._classes.delete(name);
        return add;
      },
      contains: name => this._classes.has(name)
    };
  }

  set id(value) {
    this._id = String(value || '');
    if (this._id) this.ownerDocument.elementsById.set(this._id, this);
  }
  get id() { return this._id; }

  set className(value) { this._classes = new Set(String(value || '').split(/\s+/).filter(Boolean)); }
  get className() { return [...this._classes].join(' '); }

  set textContent(value) { this._textContent = String(value ?? ''); }
  get textContent() { return this._textContent; }

  set innerHTML(value) {
    this._innerHTML = String(value ?? '');
    this.children = [];
    this.addVirtualChild('lmd-kds-btn-listo', 'button');
    this.addVirtualChild('lmd-kds-btn-86', 'button');
    this.addVirtualChild('lmd-kds-mesa-group-header', 'div');
    if (this._innerHTML.includes('lmd-kds-card__timer')) {
      const timer = this.ownerDocument.createElement('span');
      timer.className = 'lmd-kds-card__timer';
      const match = this._innerHTML.match(/data-hora-recibido="([^"]*)"/);
      if (match) timer.dataset.horaRecibido = match[1];
      this.appendChild(timer);
    }
  }
  get innerHTML() { return this._innerHTML; }

  addVirtualChild(className, tagName) {
    if (!this._innerHTML.includes(className)) return;
    const child = this.ownerDocument.createElement(tagName);
    child.className = className;
    this.appendChild(child);
  }

  appendChild(child) {
    child.parentElement = this;
    this.children.push(child);
    return child;
  }

  insertBefore(child, before) {
    child.parentElement = this;
    const index = this.children.indexOf(before);
    index === -1 ? this.children.unshift(child) : this.children.splice(index, 0, child);
    return child;
  }

  remove() {
    if (!this.parentElement) return;
    const siblings = this.parentElement.children;
    const index = siblings.indexOf(this);
    if (index >= 0) siblings.splice(index, 1);
    this.parentElement = null;
  }

  addEventListener(event, handler) { this.eventHandlers[event] = handler; }

  closest(selector) {
    for (let current = this.parentElement; current; current = current.parentElement) {
      if (current.matches(selector)) return current;
    }
    return null;
  }

  querySelector(selector) { return this.querySelectorAll(selector)[0] || null; }

  querySelectorAll(selector) {
    const results = [];
    const visit = element => {
      if (element.matches(selector)) results.push(element);
      element.children.forEach(visit);
    };
    this.children.forEach(visit);
    return results;
  }

  matches(selector) {
    const trimmed = selector.trim();
    if (trimmed === 'input[name="__RequestVerificationToken"]') {
      return this.tagName === 'INPUT' && this.name === '__RequestVerificationToken';
    }

    const rejectedClasses = [...trimmed.matchAll(/:not\(\.([\w-]+)\)/g)].map(match => match[1]);
    if (rejectedClasses.some(className => this.classList.contains(className))) return false;

    if (/:not\(\[hidden\]\)/.test(trimmed) && this.hidden) return false;

    const normalized = trimmed
      .replace(/:not\(\.[\w-]+\)/g, '')
      .replace(/:not\(\[hidden\]\)/g, '');

    const match = normalized.match(/^\.([\w-]+)(?:\[([^\]=]+)(?:="([^"]*)")?\])?$/);
    if (!match) return false;

    const [, requiredClass, attrName, attrValue] = match;
    if (!this.classList.contains(requiredClass)) return false;
    if (!attrName) return true;

    const key = toDatasetKey(attrName);
    return key in this.dataset && (attrValue === undefined || this.dataset[key] === attrValue);
  }
}

class Document {
  constructor() {
    this.elementsById = new Map();
    this.listeners = new Map();
    this.body = this.createElement('body');
  }
  createElement(tagName) { return new Element(tagName, this); }
  getElementById(id) { return this.elementsById.get(id) || null; }
  querySelector(selector) {
    if (selector.startsWith('#')) return this.getElementById(selector.slice(1));
    return this.body.matches(selector) ? this.body : this.body.querySelector(selector);
  }
  querySelectorAll(selector) {
    return (this.body.matches(selector) ? [this.body] : []).concat(this.body.querySelectorAll(selector));
  }
  addEventListener(event, handler) { this.listeners.set(event, handler); }
  dispatchDOMContentLoaded() { this.listeners.get('DOMContentLoaded')?.(); }
}

const document = new Document();
const contador = document.createElement('span');
contador.id = 'lmd-kds-contador';
contador.textContent = '0 ordenes';
document.body.appendChild(contador);

for (const id of [1, 2, 3]) {
  const count = document.createElement('span');
  count.id = `kds-count-${id}`;
  document.body.appendChild(count);
  const container = document.createElement('div');
  container.id = `kds-cards-${id}`;
  container.className = 'lmd-kds-orders';
  document.body.appendChild(container);
}

const order = {
  id: '161952f6-bc50-4adb-8a52-89b3689f6f65',
  pedidoId: '7441abcf-2531-4f7c-a99f-ed5e6e4145ae',
  productoNombre: '<img src=x onerror=alert(1)>Solomillo',
  cantidad: 1,
  notas: '<script>alert(2)</script>',
  alergenos: '<b>gluten</b>',
  ingredientesQuitados: '<i>cebolla</i>',
  ingredientesExtra: '<u>salsa</u>',
  cocineroId: null,
  estacion: 'Parrilla',
  estado: 'Pendiente',
  horaRecibido: '2026-05-29T15:10:17.4663638',
  mesaNumero: 1,
  tipoServicio: 'ComerAqui',
  curso: null,
  productoId: 'cd8b9766-9397-486b-ae01-b8f2902b6883',
  tiempoPreparacionMin: 25,
  minutosTranscurridos: 0
};

const context = {
  console,
  document,
  window: {
    __lmdKdsCooks: [
      { id: 1, name: 'Cocinero 1', color: '#e74c3c' },
      { id: 2, name: 'Cocinero 2', color: '#3498db' },
      { id: 3, name: 'Cocinero 3', color: '#2ecc71' }
    ],
    __lmdKdsStationMap: { Parrilla: 1, Fria: 2, Caliente: 3, Bar: 2, Expo: 1 }
  },
  fetch: async url => ({ ok: true, json: async () => String(url).includes('OrdenesJson') ? [order] : { ordenesCocina: [order] } }),
  setInterval: () => 1,
  clearInterval: () => {},
  setTimeout: () => 1,
  clearTimeout: () => {},
  Date,
  Promise
};
context.window.document = document;
context.window.fetch = context.fetch;

vm.createContext(context);
vm.runInContext(fs.readFileSync(kdsJavaScriptPath, 'utf8'), context, { filename: kdsJavaScriptPath });
document.dispatchDOMContentLoaded();

(async () => {
  await Promise.resolve();
  await Promise.resolve();
  await new Promise(resolve => setImmediate(resolve));

  const cards = document.querySelectorAll('.lmd-kds-card');
  assert.strictEqual(cards.length, 1, `expected one KDS card, got ${cards.length}`);
  assert.strictEqual(contador.textContent, '1 ordenes', `expected KDS counter to reflect rendered JSON order, got "${contador.textContent}"`);

  const html = cards[0].innerHTML;
  assert(!html.includes('<img'), 'productoNombre from backend JSON must be escaped before innerHTML rendering');
  assert(!html.includes('<script>'), 'notas from backend JSON must be escaped before innerHTML rendering');
  assert(html.includes('&lt;img'), 'escaped productoNombre should remain visible as text for the cook');
  assert.strictEqual(cards[0].dataset.horaRecibido, order.horaRecibido, 'rendered card should expose horaRecibido for escalation checks');
})().catch(error => {
  console.error(error.stack || error.message || error);
  process.exitCode = 1;
});
