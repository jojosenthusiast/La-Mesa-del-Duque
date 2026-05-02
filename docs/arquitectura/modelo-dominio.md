# Modelo de Dominio — La Mesa del Duque

## 1. Propósito

Este documento describe el modelo de dominio inicial del sistema, incluyendo las entidades, enumeraciones, reglas de negocio y relaciones entre componentes del núcleo de negocio del restaurante.

## 2. Entidades del dominio

### 2.1 CategoriaProducto

Agrupa productos del menú en categorías (ej. Bebidas, Entradas, Postres).

| Propiedad | Tipo     | Descripción                              |
|-----------|----------|------------------------------------------|
| Id        | Guid     | Identificador único                      |
| Nombre    | string   | Nombre de la categoría                   |
| Activo    | bool     | Indica si la categoría está habilitada   |

**Reglas de negocio:**
- El nombre es obligatorio y no puede ser vacío ni solo espacios en blanco.
- La categoría se crea activa por defecto.
- Puede activarse y desactivarse sin restricciones de dependencia en esta capa.

### 2.2 Producto

Representa un ítem del menú que puede ser pedido por los clientes.

| Propiedad | Tipo             | Descripción                              |
|-----------|------------------|------------------------------------------|
| Id        | Guid             | Identificador único                      |
| Nombre    | string           | Nombre del producto                      |
| Precio    | decimal          | Precio de venta (≥ 0)                    |
| Categoria | CategoriaProducto| Categoría a la que pertenece             |
| Activo    | bool             | Indica si el producto está disponible    |

**Reglas de negocio:**
- El nombre es obligatorio y no puede ser vacío ni solo espacios en blanco.
- El precio debe ser mayor o igual a cero.
- Debe pertenecer a una categoría (no puede ser nula).
- Se crea activo por defecto.
- Puede activarse y desactivarse.

### 2.3 Mesa

Representa una mesa física del restaurante.

| Propiedad  | Tipo       | Descripción                              |
|------------|------------|------------------------------------------|
| Id         | Guid       | Identificador único                      |
| Numero     | int        | Número identificador de la mesa (> 0)     |
| Capacidad  | int        | Cantidad máxima de comensales (> 0)       |
| Estado     | EstadoMesa | Estado actual de la mesa                 |

**Reglas de negocio:**
- El número debe ser mayor que cero.
- La capacidad debe ser mayor que cero.
- Se crea en estado `Disponible` por defecto.
- El estado puede cambiarse libremente en esta capa (validaciones de transición se manejan en servicios de aplicación).

### 2.4 DetallePedido

Representa una línea dentro de un pedido, asociando un producto con cantidad y precio.

| Propiedad      | Tipo     | Descripción                              |
|----------------|----------|------------------------------------------|
| Producto       | Producto | Producto solicitado                      |
| Cantidad       | int      | Unidades solicitadas (> 0)                  |
| PrecioUnitario  | decimal  | Precio al momento de registrar el detalle (≥ 0) |
| Subtotal       | decimal  | Calculado: Cantidad × PrecioUnitario     |

**Reglas de negocio:**
- Debe tener un producto asociado (no nulo).
- La cantidad debe ser mayor que cero.
- El precio unitario debe ser mayor o igual a cero.
- El subtotal se calcula como `Cantidad * PrecioUnitario` y es de solo lectura.

### 2.5 Pedido

Representa una orden de compra asociada a una mesa.

| Propiedad | Tipo                     | Descripción                           |
|-----------|--------------------------|---------------------------------------|
| Id        | Guid                     | Identificador único                   |
| Mesa      | Mesa                     | Mesa a la que pertenece el pedido     |
| Estado    | EstadoPedido             | Estado actual del pedido              |
| Detalles  | IReadOnlyList<DetallePedido> | Líneas del pedido (solo lectura)  |
| Total     | decimal                  | Suma de subtotales de los detalles    |

**Reglas de negocio:**
- Debe estar asociado a una mesa (no nula).
- Se crea en estado `Abierto` por defecto.
- Los detalles se agregan mediante `AgregarDetalle()`.
- No se pueden agregar detalles si el pedido está `Cerrado`.
- No se puede cerrar un pedido sin detalles.
- Al cerrar, el estado cambia a `Cerrado`. Un pedido ya cerrado no puede cerrarse de nuevo.
- El total es la suma de los subtotales de todos los detalles y es de solo lectura.

## 3. Enumeraciones

### 3.1 EstadoMesa

| Valor            | Descripción                                  |
|------------------|----------------------------------------------|
| Disponible       | Mesa libre y lista para ser ocupada          |
| Ocupada          | Mesa actualmente en uso                      |
| Reservada        | Mesa reservada para un horario específico     |
| EnMantenimiento  | Mesa fuera de servicio por mantenimiento     |

### 3.2 EstadoPedido

| Valor    | Descripción                                  |
|----------|----------------------------------------------|
| Abierto  | Pedido activo, acepta modificaciones         |
| Cerrado  | Pedido finalizado, no acepta modificaciones  |

## 4. Excepciones de dominio

### ReglaDominioException

Excepción base para todas las violaciones de reglas de negocio. Hereda de `System.Exception`. Se lanza cuando una operación intenta violar un invariante del dominio (ej. precio negativo, nombre vacío, cerrar pedido sin detalles).

## 5. Diagrama de relaciones

```
┌──────────────────┐     ┌──────────────────────────────────────────┐
│ CategoriaProducto│     │              Pedido                       │
│                  │     │  ┌────────────────────────────────┐      │
│ - Nombre         │     │  │          Mesa                  │      │
│ - Activo         │◄────┤  │  - Numero                      │      │
└──────────────────┘     │  │  - Capacidad                   │      │
        │                │  │  - Estado                      │      │
        │ 1..*           │  └────────────────────────────────┘      │
        ▼                │                                          │
┌──────────────────┐     │  ┌────────────────────────────────┐      │
│    Producto      │     │  │        DetallePedido            │      │
│                  │◄────┤  │  - Cantidad                     │      │
│ - Nombre         │     │  │  - PrecioUnitario               │      │
│ - Precio         │     │  │  - Subtotal (calculado)         │      │
│ - Activo         │     │  └────────────────────────────────┘      │
└──────────────────┘     └──────────────────────────────────────────┘
```

- Un `Producto` pertenece a una `CategoriaProducto`.
- Un `Pedido` está asociado a una `Mesa`.
- Un `Pedido` contiene múltiples `DetallePedido`.
- Cada `DetallePedido` referencia un `Producto`.

## 6. Principios aplicados

- **Encapsulamiento**: Todas las propiedades tienen setters privados. La mutación se realiza mediante métodos con validaciones.
- **Inmutabilidad de colecciones**: `Pedido.Detalles` expone `IReadOnlyList<T>` para evitar mutación externa.
- **Invariantes**: Cada entidad protege sus reglas de negocio en el constructor y métodos de mutación.
- **Lenguaje ubicuo**: Nombres en español que reflejan el dominio del restaurante.
- **Sin dependencias externas**: El proyecto de dominio no referencia paquetes externos ni otras capas.
