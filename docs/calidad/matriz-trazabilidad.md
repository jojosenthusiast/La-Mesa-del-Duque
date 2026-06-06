# Matriz de Trazabilidad — La Mesa del Duque

## 1. Propósito

Este documento establece la trazabilidad bidireccional entre los requisitos del sistema (historias de usuario), los criterios de aceptación, las pruebas, los ADR y los componentes de código implementados. Garantiza que cada requisito está cubierto por pruebas y que cada prueba responde a un requisito concreto.

## 2. Leyenda de trazabilidad

| Símbolo | Significado                       |
|---------|-----------------------------------|
| ✓       | Trazabilidad verificada           |
| ◐       | Trazabilidad parcial / en proceso |
| ✗       | Trazabilidad pendiente            |

### 2.1 Convención de filas planificadas

Las funcionalidades aún no implementadas deben marcarse con el sufijo `(planificado)` en la celda del artefacto. Las rutas de artefactos planificados no requieren backticks y pueden no existir todavía en el repositorio.

### 2.2 Convención de rutas

Todas las rutas a archivos del repositorio deben ir entre backticks (`` ` ``) para que el validador de trazabilidad pueda verificarlas.

## 3. Matriz principal — Sprint 1

### HU-001: Registrar pedido (POS)

| Elemento           | Artefacto                                      | Estado |
|--------------------|------------------------------------------------|--------|
| Historia           | `docs/requisitos/historias-usuario.md#HU-001`  | ✓      |
| Criterios          | `docs/requisitos/criterios-aceptacion.md#CA-001`| ✓      |
| Pruebas unitarias  | `tests/LaMesaDelDuque.Pruebas/Entidades/PedidoTests.cs` | ✓      |
| Pruebas servicio   | `tests/LaMesaDelDuque.Pruebas/Aplicacion/PedidosServicioTests.cs` | ✓      |
| Pruebas UI         | `tests/LaMesaDelDuque.Pruebas/Web/PedidosPageTests.cs` | ✓      |
| Regresión          | `tests/regresion/` — TC-REG-001                | ✓      |
| Código dominio     | `src/LaMesaDelDuque.Dominio/Entidades/Pedido.cs` | ✓      |
| Código servicio    | `src/LaMesaDelDuque.Aplicacion/Servicios/PedidosServicio.cs` | ✓      |
| Código UI          | `src/LaMesaDelDuque.Web/Pages/Operaciones/Pedidos/Index.cshtml` | ✓      |
| Notificaciones     | `src/LaMesaDelDuque.Aplicacion/Notificaciones/INotificadorPedidos.cs` | ✓      |
| ADR relacionado    | `docs/arquitectura/adr/0001-arquitectura-en-capas.md` | ✓      |
| Checklist seguridad| `docs/calidad/checklist-seguridad.md`          | ✓      |

### HU-002: Modificar pedido no pagado

| Elemento           | Artefacto                                      | Estado |
|--------------------|------------------------------------------------|--------|
| Historia           | `docs/requisitos/historias-usuario.md#HU-002`  | ✓      |
| Criterios          | `docs/requisitos/criterios-aceptacion.md#CA-002`| ✓      |
| Pruebas unitarias  | `tests/LaMesaDelDuque.Pruebas/Entidades/PedidoTests.cs` | ✓      |
| Pruebas servicio   | `tests/LaMesaDelDuque.Pruebas/Aplicacion/PedidosServicioTests.cs` | ✓      |
| Pruebas UI         | `tests/LaMesaDelDuque.Pruebas/Web/PedidosPageTests.cs` | ✓      |
| Regresión          | `tests/regresion/` — TC-REG-002                | ✓      |
| Código servicio    | `src/LaMesaDelDuque.Aplicacion/Servicios/PedidosServicio.cs` | ✓      |
| Código UI          | `src/LaMesaDelDuque.Web/Pages/Operaciones/Pedidos/Index.cshtml` | ✓      |
| Checklist seguridad| `docs/calidad/checklist-seguridad.md`          | ✓      |

### HU-003: Eliminar pedido pendiente no pagado

| Elemento           | Artefacto                                      | Estado |
|--------------------|------------------------------------------------|--------|
| Historia           | `docs/requisitos/historias-usuario.md#HU-003`  | ✓      |
| Criterios          | `docs/requisitos/criterios-aceptacion.md#CA-003`| ✓      |
| Pruebas unitarias  | `tests/LaMesaDelDuque.Pruebas/Entidades/PedidoTests.cs` | ✓      |
| Pruebas servicio   | `tests/LaMesaDelDuque.Pruebas/Aplicacion/PedidosServicioTests.cs` | ✓      |
| Pruebas UI         | `tests/LaMesaDelDuque.Pruebas/Web/PedidosPageTests.cs` | ✓      |
| Regresión          | `tests/regresion/` — TC-REG-003                | ✓      |
| Código UI          | `src/LaMesaDelDuque.Web/Pages/Operaciones/Pedidos/Index.cshtml` | ✓      |
| Checklist seguridad| `docs/calidad/checklist-seguridad.md`          | ✓      |

### HU-011: Gestionar productos

| Elemento           | Artefacto                                      | Estado |
|--------------------|------------------------------------------------|--------|
| Historia           | `docs/requisitos/historias-usuario.md#HU-011`  | ✓      |
| Criterios          | `docs/requisitos/criterios-aceptacion.md#CA-011`| ✓      |
| Pruebas unitarias  | `tests/LaMesaDelDuque.Pruebas/Entidades/ProductoTests.cs` | ✓      |
| Pruebas servicio   | `tests/LaMesaDelDuque.Pruebas/Aplicacion/CatalogoProductosServicioTests.cs` | ✓      |
| Pruebas UI         | `tests/LaMesaDelDuque.Pruebas/Web/ProductosPageTests.cs` | ✓      |
| Regresión          | `tests/regresion/` — TC-REG-011                | ✓      |
| Código dominio     | `src/LaMesaDelDuque.Dominio/Entidades/Producto.cs` | ✓      |
| Código servicio    | `src/LaMesaDelDuque.Aplicacion/Servicios/CatalogoProductosServicio.cs` | ✓      |
| Código UI          | `src/LaMesaDelDuque.Web/Pages/Operaciones/Productos/Index.cshtml` | ✓      |
| Checklist seguridad| `docs/calidad/checklist-seguridad.md`          | ✓      |

### HU-014: Recetas de productos

| Elemento           | Artefacto                                      | Estado |
|--------------------|------------------------------------------------|--------|
| Historia           | `docs/requisitos/historias-usuario.md#HU-014`  | ✓      |
| Criterios          | `docs/requisitos/criterios-aceptacion.md#CA-014`| ✓      |
| Pruebas unitarias  | `tests/LaMesaDelDuque.Pruebas/Entidades/RecetaProductoTests.cs` | ✓      |
| Pruebas servicio   | `tests/LaMesaDelDuque.Pruebas/Aplicacion/RecetasProductosServicioTests.cs` | ✓      |
| Regresión          | `tests/regresion/` — TC-REG-014                | ✓      |
| Código dominio     | `src/LaMesaDelDuque.Dominio/Entidades/RecetaProducto.cs` | ✓      |
| Código dominio     | `src/LaMesaDelDuque.Dominio/Entidades/RecetaIngrediente.cs` | ✓      |
| Código servicio    | `src/LaMesaDelDuque.Aplicacion/Servicios/RecetasProductosServicio.cs` | ✓      |
| Checklist seguridad| `docs/calidad/checklist-seguridad.md`          | ✓      |

### HU-016: Gestión de mesas

| Elemento           | Artefacto                                      | Estado |
|--------------------|------------------------------------------------|--------|
| Historia           | `docs/requisitos/historias-usuario.md#HU-016`  | ✓      |
| Criterios          | `docs/requisitos/criterios-aceptacion.md#CA-016`| ✓      |
| Pruebas unitarias  | `tests/LaMesaDelDuque.Pruebas/Entidades/MesaTests.cs` | ✓      |
| Pruebas servicio   | `tests/LaMesaDelDuque.Pruebas/Aplicacion/MesasServicioTests.cs` | ✓      |
| Pruebas UI         | `tests/LaMesaDelDuque.Pruebas/Web/MesasPageTests.cs` | ✓      |
| Regresión          | `tests/regresion/` — TC-REG-016                | ✓      |
| Código dominio     | `src/LaMesaDelDuque.Dominio/Entidades/Mesa.cs` | ✓      |
| Código servicio    | `src/LaMesaDelDuque.Aplicacion/Servicios/MesasServicio.cs` | ✓      |
| Código UI          | `src/LaMesaDelDuque.Web/Pages/Operaciones/Mesas/Index.cshtml` | ✓      |
| Checklist seguridad| `docs/calidad/checklist-seguridad.md`          | ✓      |

### HU-021: Gestión de usuarios y roles

| Elemento           | Artefacto                                      | Estado |
|--------------------|------------------------------------------------|--------|
| Historia           | `docs/requisitos/historias-usuario.md#HU-021`  | ✓      |
| Criterios          | `docs/requisitos/criterios-aceptacion.md#CA-021`| ✓      |
| Pruebas servicio   | `tests/LaMesaDelDuque.Pruebas/Aplicacion/UsuariosServicioTests.cs` | ✓      |
| Pruebas UI         | `tests/LaMesaDelDuque.Pruebas/Web/UsuariosPageTests.cs` | ✓      |
| Regresión          | `tests/regresion/` — TC-REG-021                | ✓      |
| Código dominio     | `src/LaMesaDelDuque.Dominio/Entidades/Usuario.cs` | ✓      |
| Código servicio    | `src/LaMesaDelDuque.Aplicacion/Servicios/UsuariosServicio.cs` | ✓      |
| Código UI          | `src/LaMesaDelDuque.Web/Pages/Admin/Usuarios/Index.cshtml` | ✓      |
| Código repositorio | `src/LaMesaDelDuque.Infraestructura/Repositorios/RolRepositorio.cs` | ✓      |
| ADR relacionado    | `docs/arquitectura/adr/0002-aspnet-razor-pages.md` | ✓      |
| Checklist seguridad| `docs/calidad/checklist-seguridad.md`          | ✓      |

### HU-025: Inicio de sesión, RBAC y protección CSRF

| Elemento           | Artefacto                                      | Estado |
|--------------------|------------------------------------------------|--------|
| Historia           | `docs/requisitos/historias-usuario.md#HU-025`  | ✓      |
| Criterios          | `docs/requisitos/criterios-aceptacion.md#CA-025`| ✓      |
| Pruebas servicio   | `tests/LaMesaDelDuque.Pruebas/Aplicacion/UsuariosServicioTests.cs` | ✓      |
| Regresión          | `tests/regresion/` — TC-REG-025                | ✓      |
| Código auth        | `src/LaMesaDelDuque.Web/Pages/Auth/Login.cshtml` | ✓      |
| Código auth        | `src/LaMesaDelDuque.Web/Pages/Auth/Logout.cshtml` | ✓      |
| Código auth        | `src/LaMesaDelDuque.Web/Pages/Auth/AccesoDenegado.cshtml` | ✓      |
| Código config      | `src/LaMesaDelDuque.Web/Program.cs`            | ✓      |
| Checklist seguridad| `docs/calidad/checklist-seguridad.md`          | ✓      |
| ISO 27001          | `docs/seguridad/declaracion-aplicabilidad-iso27001.md` | ✓      |

## 4. Trazabilidad inversa (pruebas → requisitos)

| Prueba                                    | HU cubierta    |
|-------------------------------------------|----------------|
| `tests/LaMesaDelDuque.Pruebas/Entidades/PedidoTests.cs`  | HU-001, 002, 003 |
| `tests/LaMesaDelDuque.Pruebas/Aplicacion/PedidosServicioTests.cs` | HU-001, 002, 003 |
| `tests/LaMesaDelDuque.Pruebas/Web/PedidosPageTests.cs` | HU-001, 002, 003 |
| `tests/LaMesaDelDuque.Pruebas/Entidades/ProductoTests.cs` | HU-011         |
| `tests/LaMesaDelDuque.Pruebas/Aplicacion/CatalogoProductosServicioTests.cs` | HU-011 |
| `tests/LaMesaDelDuque.Pruebas/Web/ProductosPageTests.cs` | HU-011         |
| `tests/LaMesaDelDuque.Pruebas/Entidades/RecetaProductoTests.cs` | HU-014   |
| `tests/LaMesaDelDuque.Pruebas/Aplicacion/RecetasProductosServicioTests.cs` | HU-014 |
| `tests/LaMesaDelDuque.Pruebas/Entidades/MesaTests.cs`     | HU-016         |
| `tests/LaMesaDelDuque.Pruebas/Aplicacion/MesasServicioTests.cs` | HU-016   |
| `tests/LaMesaDelDuque.Pruebas/Web/MesasPageTests.cs`      | HU-016         |
| `tests/LaMesaDelDuque.Pruebas/Aplicacion/UsuariosServicioTests.cs` | HU-021, 025 |
| `tests/LaMesaDelDuque.Pruebas/Web/UsuariosPageTests.cs`   | HU-021         |
| `tests/LaMesaDelDuque.Pruebas/Entidades/CategoriaProductoTests.cs` | HU-011 (base) |
| `tests/LaMesaDelDuque.Pruebas/Entidades/DetallePedidoTests.cs`     | HU-001 (base) |
| `tests/LaMesaDelDuque.Pruebas/Web/IndexPageTests.cs`      | Shell operativo |
| `tests/LaMesaDelDuque.Pruebas/Web/LayoutShellSmokeTests.cs` | Shell operativo |

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

**Versión**: 2.0 | **Fecha**: Mayo 2026 | **Sprint 1 — Verificación integrada completada**
