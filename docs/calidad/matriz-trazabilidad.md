# Matriz de Trazabilidad — La Mesa del Duque

## 1. Propósito

Este documento establece la trazabilidad bidireccional entre los requisitos del sistema (historias de usuario), los criterios de aceptación, las pruebas, los ADR y los componentes de código implementados. Garantiza que cada requisito está cubierto por pruebas y que cada prueba responde a un requisito concreto.

## 2. Leyenda de trazabilidad

| Símbolo | Significado                       |
|---------|-----------------------------------|
| ✓       | Trazabilidad verificada           |
| ◐       | Trazabilidad parcial / en proceso |
| ✗       | Trazabilidad pendiente            |

## 3. Matriz principal — Sprint 1

### HU-001: Registrar pedido (POS)

| Elemento           | Artefacto                                      | Estado |
|--------------------|------------------------------------------------|--------|
| Historia           | `docs/requisitos/historias-usuario.md#HU-001`  | ✓      |
| Criterios          | `docs/requisitos/criterios-aceptacion.md#CA-001`| ✓      |
| Pruebas unitarias  | `tests/LaMesaDelDuque.Pruebas/PedidoTests.cs`   | ◐      |
| Integración        | Prueba manual de flujo POS                     | ◐      |
| Regresión          | `tests/regresion/` — TC-REG-001                | ◐      |
| Código             | `src/LaMesaDelDuque.Dominio/Entidades/Pedido.cs`| ◐      |
| Código             | `src/LaMesaDelDuque.Web/Pages/POS/`             | ◐      |
| ADR relacionado    | `docs/arquitectura/adr/0001-arquitectura-en-capas.md`| ✓      |
| Checklist seguridad| `docs/calidad/checklist-seguridad.md`          | ◐      |

### HU-002: Modificar pedido no pagado

| Elemento           | Artefacto                                      | Estado |
|--------------------|------------------------------------------------|--------|
| Historia           | `docs/requisitos/historias-usuario.md#HU-002`  | ✓      |
| Criterios          | `docs/requisitos/criterios-aceptacion.md#CA-002`| ✓      |
| Pruebas unitarias  | `tests/LaMesaDelDuque.Pruebas/PedidoTests.cs`   | ◐      |
| Integración        | Prueba manual de modificación de pedido         | ◐      |
| Regresión          | `tests/regresion/` — TC-REG-002                | ◐      |
| Código             | `src/LaMesaDelDuque.Dominio/Servicios/PedidoServicio.cs`| ◐      |
| Código             | `src/LaMesaDelDuque.Web/Pages/POS/Editar.cshtml` | ◐     |
| Checklist seguridad| `docs/calidad/checklist-seguridad.md`          | ◐      |

### HU-003: Eliminar pedido pendiente no pagado

| Elemento           | Artefacto                                      | Estado |
|--------------------|------------------------------------------------|--------|
| Historia           | `docs/requisitos/historias-usuario.md#HU-003`  | ✓      |
| Criterios          | `docs/requisitos/criterios-aceptacion.md#CA-003`| ✓      |
| Pruebas unitarias  | `tests/LaMesaDelDuque.Pruebas/PedidoTests.cs`   | ◐      |
| Integración        | Prueba manual de eliminación de pedido          | ◐      |
| Regresión          | `tests/regresion/` — TC-REG-003                | ◐      |
| Código             | `src/LaMesaDelDuque.Web/Pages/POS/`             | ◐      |
| Checklist seguridad| `docs/calidad/checklist-seguridad.md`          | ◐      |

### HU-011: Gestionar productos

| Elemento           | Artefacto                                      | Estado |
|--------------------|------------------------------------------------|--------|
| Historia           | `docs/requisitos/historias-usuario.md#HU-011`  | ✓      |
| Criterios          | `docs/requisitos/criterios-aceptacion.md#CA-011`| ✓      |
| Pruebas unitarias  | `tests/LaMesaDelDuque.Pruebas/ProductoTests.cs` | ◐      |
| Integración        | Prueba manual de CRUD de productos             | ◐      |
| Regresión          | `tests/regresion/` — TC-REG-011                | ◐      |
| Código             | `src/LaMesaDelDuque.Dominio/Entidades/Producto.cs`| ◐    |
| Código             | `src/LaMesaDelDuque.Web/Pages/Productos/`       | ◐      |
| Checklist seguridad| `docs/calidad/checklist-seguridad.md`          | ◐      |

### HU-014: Recetas de productos

| Elemento           | Artefacto                                      | Estado |
|--------------------|------------------------------------------------|--------|
| Historia           | `docs/requisitos/historias-usuario.md#HU-014`  | ✓      |
| Criterios          | `docs/requisitos/criterios-aceptacion.md#CA-014`| ✓      |
| Pruebas unitarias  | `tests/LaMesaDelDuque.Pruebas/RecetaTests.cs`   | ◐      |
| Integración        | Prueba manual de asociación producto-receta     | ◐      |
| Regresión          | `tests/regresion/` — TC-REG-014                | ◐      |
| Código             | `src/LaMesaDelDuque.Dominio/Entidades/Receta.cs` | ◐     |
| Checklist seguridad| `docs/calidad/checklist-seguridad.md`          | ◐      |

### HU-016: Gestión de mesas

| Elemento           | Artefacto                                      | Estado |
|--------------------|------------------------------------------------|--------|
| Historia           | `docs/requisitos/historias-usuario.md#HU-016`  | ✓      |
| Criterios          | `docs/requisitos/criterios-aceptacion.md#CA-016`| ✓      |
| Pruebas unitarias  | `tests/LaMesaDelDuque.Pruebas/MesaTests.cs`     | ◐      |
| Integración        | Prueba manual de gestión de mesas              | ◐      |
| Regresión          | `tests/regresion/` — TC-REG-016                | ◐      |
| Código             | `src/LaMesaDelDuque.Dominio/Entidades/Mesa.cs`   | ◐      |
| Código             | `src/LaMesaDelDuque.Web/Pages/Mesas/`           | ◐      |
| Checklist seguridad| `docs/calidad/checklist-seguridad.md`          | ◐      |

### HU-021: Gestión de usuarios y roles

| Elemento           | Artefacto                                      | Estado |
|--------------------|------------------------------------------------|--------|
| Historia           | `docs/requisitos/historias-usuario.md#HU-021`  | ✓      |
| Criterios          | `docs/requisitos/criterios-aceptacion.md#CA-021`| ✓      |
| Pruebas unitarias  | `tests/LaMesaDelDuque.Pruebas/UsuarioTests.cs`  | ◐      |
| Integración        | Prueba manual de CRUD de usuarios y roles      | ◐      |
| Regresión          | `tests/regresion/` — TC-REG-021                | ◐      |
| Código             | `src/LaMesaDelDuque.Dominio/Entidades/Usuario.cs`| ◐     |
| Código             | `src/LaMesaDelDuque.Web/Pages/Usuarios/`        | ◐      |
| ADR relacionado    | `docs/arquitectura/adr/0002-aspnet-razor-pages.md`| ✓    |
| Checklist seguridad| `docs/calidad/checklist-seguridad.md`          | ◐      |

### HU-025: Inicio de sesión, RBAC y protección CSRF

| Elemento           | Artefacto                                      | Estado |
|--------------------|------------------------------------------------|--------|
| Historia           | `docs/requisitos/historias-usuario.md#HU-025`  | ✓      |
| Criterios          | `docs/requisitos/criterios-aceptacion.md#CA-025`| ✓      |
| Pruebas unitarias  | `tests/LaMesaDelDuque.Pruebas/AuthTests.cs`     | ◐      |
| Integración        | Prueba manual de inicio de sesión y RBAC       | ◐      |
| Regresión          | `tests/regresion/` — TC-REG-025                | ◐      |
| Código             | `src/LaMesaDelDuque.Web/Pages/Acceso/`          | ◐      |
| Código             | `src/LaMesaDelDuque.Web/Seguridad/`             | ◐      |
| Código             | `src/LaMesaDelDuque.Infraestructura/Auth/`      | ◐      |
| Checklist seguridad| `docs/calidad/checklist-seguridad.md`          | ◐      |
| ISO 27001          | `docs/seguridad/declaracion-aplicabilidad-iso27001.md`| ◐ |

## 4. Trazabilidad inversa (pruebas → requisitos)

| Prueba                                    | HU cubierta    |
|-------------------------------------------|----------------|
| `PedidoTests.cs`                          | HU-001, 002, 003 |
| `ProductoTests.cs`                        | HU-011         |
| `RecetaTests.cs`                          | HU-014         |
| `MesaTests.cs`                            | HU-016         |
| `UsuarioTests.cs`                         | HU-021         |
| `AuthTests.cs`                            | HU-025         |
| `CategoriaProductoTests.cs`               | HU-011 (base)  |
| `DetallePedidoTests.cs`                   | HU-001 (base)  |
| TC-REG-001 a TC-REG-025 (Regresión)       | HU-001 a HU-025 |

### HU-000: Arquitectura base del dominio

| Elemento           | Artefacto                                      | Estado |
|--------------------|------------------------------------------------|--------|
| Modelo de dominio  | `docs/arquitectura/modelo-dominio.md`           | ✓      |
| Pruebas unitarias  | `tests/LaMesaDelDuque.Pruebas/Entidades/`       | ✓      |
| Código             | `src/LaMesaDelDuque.Dominio/Entidades/`         | ✓      |
| Código             | `src/LaMesaDelDuque.Dominio/Enumeraciones/`     | ✓      |
| Código             | `src/LaMesaDelDuque.Dominio/Excepciones/`       | ✓      |
| ADR relacionado    | `docs/arquitectura/adr/0001-arquitectura-en-capas.md`| ✓  |

## 5. Trazabilidad ADR → HU

| ADR | HU impactada |
|-----|-------------|
| 0001 — Arquitectura en capas             | Todas          |
| 0002 — ASP.NET Razor Pages               | Todas          |
| 0003 — PostgreSQL / Supabase             | HU-001, HU-021, HU-025 |

---

**Versión**: 1.0 | **Fecha**: Abril 2026 | **Actualizar en cada sprint**
