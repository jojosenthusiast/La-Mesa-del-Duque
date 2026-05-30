# Plan de PRs pequeños y releases v3.x

Este plan permite cerrar la defensa final sin mezclar todas las correcciones en un único PR inmanejable. La regla es simple: cada PR debe ser pequeño, verificable y defendible.

## Estrategia de versiones

| Línea | Sentido del proyecto |
|---|---|
| `0.x` | Base / Sprint 1 |
| `1.x` | Sprint 2 y estabilización inicial |
| `2.x` | Sprint 3 e integración creciente |
| `3.x` | Remediación defensa final: rúbrica, evidencia, P0 y documentación |

## Principios de entrega

- Un PR debe responder a una pregunta clara: “¿qué problema de defensa resuelve?”
- No mezclar stock transaccional con documentación PDF si no dependen entre sí.
- Cada PR debe tener pruebas o evidencia manual.
- Cada PR debe revisar texto visible en español.
- Cada PR debe incluir un resumen para Jira/release notes.
- Si un PR supera ~400 líneas significativas, dividirlo salvo que el equipo apruebe excepción.

## Cadena recomendada de PRs

### PR 1 — `v3.0.0-alpha.1` Delivery explícito y matriz operativa

**Branch sugerida:** `feat/v3-delivery-explicito`
**Título sugerido:** `feat(delivery): add explicit delivery service flow`

**Incluye:**

- Modalidad Delivery/Domicilio explícita.
- Validación de datos mínimos de cliente.
- POS diferenciado: Comer aquí / Para llevar / Delivery.
- Etiquetas en Cocina/KDS y Despacho.
- Ticket con modalidad correcta.
- Actualización parcial de docs si el flujo queda listo.

**No incluye:**

- Reescritura completa de rutas de despacho.
- Sistema avanzado de repartidores si no es necesario para defensa.

**Verificación mínima:**

- `dotnet test`
- `dotnet build`
- Flujo manual: POS → Cocina → Despacho con pedido Delivery.

---

### PR 2 — `v3.0.0-alpha.2` Stock, integridad y concurrencia

**Branch sugerida:** `fix/v3-stock-integrity`
**Título sugerido:** `fix(stock): enforce atomic inventory blocking`

**Incluye:**

- Validación + descuento atómico de inventario.
- Bloqueo de venta por stock insuficiente.
- Protección contra stock negativo.
- Error seguro y claro en POS.
- Prueba de concurrencia o integración equivalente.
- Revisión de alerta de producto agotado.

**No incluye:**

- Kardex completo si pone en riesgo el P0.
- Reportería avanzada de inventario.

**Verificación mínima:**

- `dotnet test`
- `dotnet build`
- Prueba manual: último ingrediente vendido por un cajero y bloqueado para otro.

---

### PR 3 — `v3.0.0-alpha.3` Caja, turnos y cierre de día

**Branch sugerida:** `feat/v3-cierre-turnos`
**Título sugerido:** `feat(cash): align day closing with shifts and payments`

**Incluye:**

- Servicio centralizado de totales de cierre.
- Cierre con ventas efectivo/tarjeta desde datos reales.
- Merma valorizada.
- Responsables/turnos mínimos si el modelo actual lo permite.
- UI más clara y defendible.

**No incluye:**

- Nómina completa.
- Sistema complejo de horas extra.
- Contabilidad avanzada.

**Verificación mínima:**

- `dotnet test`
- `dotnet build`
- Día demo con al menos 2 pagos, 1 merma y cierre consistente.

---

### PR 4 — `v3.0.0-alpha.4` SQL final y seeds reproducibles

**Branch sugerida:** `docs/v3-final-sql`
**Título sugerido:** `docs(database): add final schema and seed script`

**Incluye:**

- Script SQL final de tablas/relaciones/seed.
- README de ejecución/validación.
- Alineación con seeds reales de `Program.cs`.
- Sin secretos ni connection strings.

**No incluye:**

- Cambios funcionales no relacionados.

**Verificación mínima:**

- Validación de sintaxis o ejecución en entorno seguro.
- `git diff --check`.
- Si toca código: `dotnet test` y `dotnet build`.

---

### PR 5 — `v3.0.0-rc.1` Paquete documental de entrega final

**Branch sugerida:** `docs/v3-final-delivery-package`
**Título sugerido:** `docs(final): prepare defense delivery package`

**Incluye:**

- PDF/DOCX fuente o contenido final.
- Matriz de configuración de negocio.
- Manual rápido por rol.
- Guion de video de máximo 5 minutos.
- Checklist de entrega.
- Historias Jira nuevas y trazabilidad.

**No incluye:**

- Cambios funcionales de P0.

**Verificación mínima:**

- Revisión humana del PDF/DOCX.
- Todos los enlaces reales pegados o marcados como pendientes.
- Sin promesas falsas.

---

### PR 6 — `v3.0.0-rc.2` Evidencia QA multirol con capturas

**Branch sugerida:** `test/v3-browser-audit-evidence`
**Título sugerido:** `test(qa): add multi-role browser audit evidence`

**Incluye:**

- Informe de auditoría con navegador.
- Capturas por rol.
- Flujo Cajero → Cocina → Despacho → Admin/Gerente.
- Tabla P0/P1/P2.
- Correcciones pequeñas de documentación si aparecen.

**No incluye:**

- Fixes grandes; esos deben ir en PR separado.

**Verificación mínima:**

- Capturas revisables.
- Checklist completo.
- P0 restantes explícitos o cero P0.

---

### PR 7 — `v3.0.0` Release final

**Branch sugerida:** `release/v3.0.0`
**Título sugerido:** `chore(release): prepare v3.0.0 defense release`

**Incluye:**

- Changelog/release notes.
- Manifest/version bump si aplica.
- Confirmación de links finales.
- Checklist final cerrado.

**Verificación mínima:**

- `dotnet test`
- `dotnet build`
- `git diff --check`
- Revisión final de documentación.

## Orden recomendado

1. Delivery explícito.
2. Stock transaccional.
3. Cierre/caja.
4. SQL final.
5. Documentación final.
6. Browser QA con capturas.
7. Release final.

La razón es técnica: primero se corrige lo que afecta la verdad del producto; después se documenta. Documentar antes de implementar puede dejar promesas falsas, y eso es un riesgo directo en defensa.

## Checklist por PR

- [ ] Scope del PR cabe en una frase.
- [ ] No hay textos visibles nuevos en inglés.
- [ ] `dotnet test` ejecutado o limitación documentada.
- [ ] `dotnet build` ejecutado o limitación documentada.
- [ ] `git diff --check` sin errores.
- [ ] Jira actualizado o nota lista para pegar.
- [ ] Release notes breves preparadas.
- [ ] Capturas adjuntas si el PR cambia UI.

## Formato de release notes v3.0.0

```markdown
# La Mesa del Duque v3.0.0 — Defensa final

## Enfoque
Release orientado a defensa final, adaptabilidad del negocio, integridad de stock, flujos multirol y evidencia documental.

## Cambios principales
- Adaptabilidad explícita a comida rápida, mesa, para llevar y delivery.
- Bloqueo de venta por stock insuficiente con integridad transaccional.
- Cierre de día/caja más coherente con ventas, pagos, merma y responsables.
- Paquete documental final para entrega.
- Evidencia QA multirol con capturas.

## Verificación
- dotnet test: <resultado>
- dotnet build: <resultado>
- Browser QA: <enlace o ruta>
- PDF final: <enlace o ruta>
```
