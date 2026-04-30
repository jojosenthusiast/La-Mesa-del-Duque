# Matriz de Impacto de Cambios — La Mesa del Duque

## 1. Propósito

Este documento permite evaluar el impacto de cualquier cambio propuesto en el sistema **La Mesa del Duque** sobre los componentes existentes. Antes de modificar una entidad, servicio, página o configuración, se debe consultar esta matriz para identificar qué otras partes del sistema pueden verse afectadas y así planificar las pruebas de regresión y mitigaciones necesarias.

## 2. Componentes del sistema

### 2.1 Capa de Dominio (`LaMesaDelDuque.Dominio`)

| ID  | Componente                   | Descripción                                    |
|-----|------------------------------|------------------------------------------------|
| D01 | `Entidades/Pedido.cs`        | Entidad principal de pedido.                   |
| D02 | `Entidades/DetallePedido.cs` | Línea de pedido (producto + cantidad).         |
| D03 | `Entidades/Producto.cs`      | Producto del menú.                             |
| D04 | `Entidades/Receta.cs`        | Receta asociada a un producto.                 |
| D05 | `Entidades/Mesa.cs`          | Mesa del restaurante.                          |
| D06 | `Entidades/Usuario.cs`       | Usuario del sistema.                           |
| D07 | `Entidades/Rol.cs`           | Rol de usuario.                                |
| D08 | `Interfaces/IPedidoRepositorio.cs` | Contrato del repositorio de pedidos.    |
| D09 | `Interfaces/IProductoRepositorio.cs`| Contrato del repositorio de productos.  |
| D10 | `Interfaces/IUsuarioRepositorio.cs` | Contrato del repositorio de usuarios.   |
| D11 | `Servicios/PedidoServicio.cs`| Lógica de negocio de pedidos.                  |
| D12 | `Servicios/UsuarioServicio.cs`| Lógica de negocio de usuarios.                |

### 2.2 Capa de Infraestructura (`LaMesaDelDuque.Infraestructura`)

| ID  | Componente                        | Descripción                               |
|-----|-----------------------------------|-------------------------------------------|
| I01 | `AppDbContext.cs`                 | Contexto de EF Core (DbSets, config).     |
| I02 | `Repositorios/PedidoRepositorio.cs`| Implementación del repositorio de pedidos.|
| I03 | `Repositorios/ProductoRepositorio.cs`| Implementación del repositorio de productos.|
| I04 | `Repositorios/UsuarioRepositorio.cs`| Implementación del repositorio de usuarios.|
| I05 | `Migraciones/`                    | Migraciones de EF Core.                   |

### 2.3 Capa Web (`LaMesaDelDuque.Web`)

| ID  | Componente                        | Descripción                               |
|-----|-----------------------------------|-------------------------------------------|
| W01 | `Pages/POS/`                      | Páginas del punto de venta (registrar, modificar, eliminar pedidos). |
| W02 | `Pages/Productos/`                | Páginas de gestión de productos.          |
| W03 | `Pages/Mesas/`                    | Páginas de gestión de mesas.              |
| W04 | `Pages/Usuarios/`                 | Páginas de gestión de usuarios y roles.   |
| W05 | `Pages/Acceso/`                   | Páginas de inicio de sesión, cierre de sesión. |
| W06 | `Seguridad/`                      | Middleware de autenticación, RBAC, CSRF.  |
| W07 | `Hubs/PedidoHub.cs`               | Hub de SignalR para notificaciones de pedidos en tiempo real. |
| W08 | `wwwroot/js/`                     | Scripts de cliente (validación, SignalR). |
| W09 | `Program.cs`                      | Configuración de la aplicación.           |
| W10 | `appsettings.json`                | Configuración general.                    |

### 2.4 Pruebas (`tests/`)

| ID  | Componente                        | Descripción                               |
|-----|-----------------------------------|-------------------------------------------|
| T01 | `LaMesaDelDuque.Pruebas/`         | Pruebas unitarias y de integración.       |
| T02 | `regresion/`                      | Suite de pruebas de regresión.            |

## 3. Matriz de dependencias

La siguiente matriz muestra qué componentes se ven afectados cuando se modifica un componente fuente. Las filas representan el componente **modificado**; las columnas, los componentes **impactados**.

| Fuente ↓ / Impactado → | D01 | D02 | D03 | D04 | D05 | D06 | D07 | D08 | D09 | D10 | D11 | D12 | I01 | I02 | I03 | I04 | I05 | W01 | W02 | W03 | W04 | W05 | W06 | W07 | W08 | W09 | W10 | T01 | T02 |
|------------------------|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|
| **D01 — Pedido**       |  —  |  ✓  |     |     |  ✓  |  ✓  |     |  ✓  |     |     |  ✓  |     |  ✓  |  ✓  |     |     |  ✓  |  ✓  |     |  ✓  |     |     |     |  ✓  |  ✓  |     |     |  ✓  |  ✓  |
| **D02 — DetallePedido**|  ✓  |  —  |  ✓  |     |     |     |     |     |     |     |  ✓  |     |  ✓  |  ✓  |     |     |  ✓  |  ✓  |     |     |     |     |     |  ✓  |  ✓  |     |     |  ✓  |  ✓  |
| **D03 — Producto**     |     |  ✓  |  —  |  ✓  |     |     |     |     |  ✓  |     |     |     |  ✓  |     |  ✓  |     |  ✓  |  ✓  |  ✓  |     |     |     |     |     |     |     |     |  ✓  |  ✓  |
| **D04 — Receta**       |     |     |  ✓  |  —  |     |     |     |     |     |     |     |     |  ✓  |     |     |     |  ✓  |     |  ✓  |     |     |     |     |     |     |     |     |  ✓  |  ✓  |
| **D05 — Mesa**         |  ✓  |     |     |     |  —  |     |     |     |     |     |     |     |  ✓  |     |     |     |  ✓  |  ✓  |     |  ✓  |     |     |     |  ✓  |     |     |     |  ✓  |  ✓  |
| **D06 — Usuario**      |  ✓  |     |     |     |     |  —  |  ✓  |     |     |  ✓  |     |  ✓  |  ✓  |     |     |  ✓  |  ✓  |     |     |     |  ✓  |  ✓  |  ✓  |     |     |     |     |  ✓  |  ✓  |
| **D07 — Rol**          |     |     |     |     |     |  ✓  |  —  |     |     |     |     |     |  ✓  |     |     |     |  ✓  |     |     |     |  ✓  |  ✓  |  ✓  |     |     |     |     |  ✓  |  ✓  |
| **W06 — Seguridad**    |     |     |     |     |     |  ✓  |  ✓  |     |     |     |     |  ✓  |     |     |     |     |     |  ✓  |  ✓  |  ✓  |  ✓  |  ✓  |  —  |     |     |  ✓  |  ✓  |  ✓  |  ✓  |
| **W09 — Program.cs**   |     |     |     |     |     |     |     |     |     |     |     |     |  ✓  |     |     |     |     |  ✓  |  ✓  |  ✓  |  ✓  |  ✓  |  ✓  |  ✓  |     |  —  |  ✓  |     |     |
| **I01 — AppDbContext** |  ✓  |  ✓  |  ✓  |  ✓  |  ✓  |  ✓  |  ✓  |     |     |     |     |     |  —  |  ✓  |  ✓  |  ✓  |  ✓  |     |     |     |     |     |     |     |     |     |     |  ✓  |  ✓  |

*Nota: ✓ indica que una modificación en el componente fuente puede impactar al componente destino. Las celdas vacías indican que no hay dependencia directa.*

## 4. Protocolo ante un cambio

Cuando se planifica modificar un componente:

1. **Identificar** el componente en la columna *Fuente* de la matriz.
2. **Revisar** todos los componentes marcados con ✓ en esa fila.
3. **Verificar** que las pruebas unitarias asociadas a los componentes impactados se ejecutan.
4. **Ejecutar** la suite de regresión (`tests/regresion/`) completa.
5. **Actualizar** esta matriz si el cambio introduce nuevas dependencias.
6. **Documentar** el impacto en el PR.

## 5. Historial de cambios

| Fecha      | Componente modificado | Impacto identificado    | Acción tomada              |
|------------|-----------------------|-------------------------|----------------------------|
| Abr 2026   | —                     | Registro inicial        | Creación de la matriz      |

---

**Versión**: 1.0 | **Fecha**: Abril 2026 | **Actualizar ante cada cambio significativo**
