# Wireframes base

Estructura visual de las páginas principales del sistema.

## Estructura general (Layout)

```
┌──────────────────────────────────────────────────────────┐
│  ██ La Mesa del Duque         Inicio · Privacidad        │ ← navbar azul duque
├──────────────────────────────────────────────────────────┤
│                                                          │
│                    [contenido]                            │ ← fondo marfil
│                                                          │
├──────────────────────────────────────────────────────────┤
│  © 2026 La Mesa del Duque                   Privacidad   │ ← footer azul duque
└──────────────────────────────────────────────────────────┘
```

## Página de inicio

```
┌──────────────────────────────────────────────────────────┐
│  ██ La Mesa del Duque         Inicio · Privacidad        │
├──────────────────────────────────────────────────────────┤
│                                                          │
│              ┌──────────────────────┐                    │
│              │    [logo completo]   │                    │
│              └──────────────────────┘                    │
│           LA MESA DEL DUQUE                              │ ← hero azul duque
│     El SaaS que gestiona tu restaurante                  │
│            como la realeza                               │
│                                                          │
├──────────────────────────────────────────────────────────┤
│                      ───◆───                             │ ← separador dorado
├──────────────────────────────────────────────────────────┤
│                                                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │  Pedidos en  │  │ Inventario  │  │  Reportes   │     │
│  │ tiempo real  │  │ inteligente │  │ y métricas  │     │ ← tarjetas
│  │             │  │             │  │             │     │
│  │ Gestión     │  │ Control     │  │ Información │     │
│  │ ágil...     │  │ automático..│  │ clave...    │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
│                                                          │
├──────────────────────────────────────────────────────────┤
│  © 2026 La Mesa del Duque                   Privacidad   │
└──────────────────────────────────────────────────────────┘
```

## Página de error

```
┌──────────────────────────────────────────────────────────┐
│  ██ La Mesa del Duque         Inicio · Privacidad        │
├──────────────────────────────────────────────────────────┤
│                                                          │
│                                                          │
│                      Error                               │ ← terracota
│           Ocurrió un error al procesar                   │
│               la solicitud.                              │ ← gris piedra
│                                                          │
│          ID de solicitud: xxxxx                           │
│                                                          │
│                                                          │
├──────────────────────────────────────────────────────────┤
│  © 2026 La Mesa del Duque                   Privacidad   │
└──────────────────────────────────────────────────────────┘
```

## Wireframes futuros (Sprint 1)

### POS — Punto de venta

```
┌──────────────────────────────────────────────────────────┐
│  ██ La Mesa del Duque    POS · Cocina · Inventario · ... │
├──────────┬───────────────────────────────────────────────┤
│          │                                               │
│ Menú     │  ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐       │
│ lateral  │  │ Prod │ │ Prod │ │ Prod │ │ Prod │       │
│          │  └──────┘ └──────┘ └──────┘ └──────┘       │
│ Categoría│                                               │
│ ────────│  ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐       │
│ Entradas │  │ Prod │ │ Prod │ │ Prod │ │ Prod │       │
│ Platos   │  └──────┘ └──────┘ └──────┘ └──────┘       │
│ Bebidas  │                                               │
│ Postres  ├───────────────────────────────────────────────┤
│          │  Pedido #001    Mesa: [opcional]               │
│          │  ──────────────────────────────               │
│          │  2x Producto A          $xx.xx                │
│          │  1x Producto B          $xx.xx                │
│          │  ─────────────────────────────                │
│          │  Total:                $xxx.xx                │
│          │  [ Confirmar pedido ]                          │
└──────────┴───────────────────────────────────────────────┘
```

### Cocina — Pantalla KDS

```
┌──────────────────────────────────────────────────────────┐
│  ██ La Mesa del Duque                          Cocina    │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  ┌────────────┐ ┌────────────┐ ┌────────────┐          │
│  │ Pedido #003│ │ Pedido #001│ │ Pedido #002│          │
│  │ 12:34      │ │ 12:30      │ │ 12:32      │          │
│  │ ─────────  │ │ ─────────  │ │ ─────────  │          │
│  │ 2x Plato A │ │ 1x Plato C │ │ 3x Plato B │          │
│  │ 1x Plato B │ │ 2x Plato A │ │ 1x Plato D │          │
│  │            │ │            │ │            │          │
│  │ [Listo ✓]  │ │ [Listo ✓]  │ │ [Listo ✓]  │          │
│  └────────────┘ └────────────┘ └────────────┘          │
│                                                          │
│  Prioridad: por complejidad del pedido                   │
└──────────────────────────────────────────────────────────┘
```

## Flujo de navegación

```
Inicio
  ├── POS (punto de venta)
  │     ├── Crear pedido
  │     └── Ver pedidos activos
  ├── Cocina (KDS)
  │     └── Gestionar preparación
  ├── Despacho
  │     └── Marcar entregas
  ├── Inventario
  │     ├── Productos
  │     ├── Ingredientes
  │     └── Merma diaria
  ├── Administración
  │     ├── Usuarios y roles
  │     ├── Configuración
  │     └── Proveedores
  └── Gerencia
        ├── Reportes
        ├── Cierre de día
        └── Indicadores
```
