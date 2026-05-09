-- ============================================================================
-- La Mesa del Duque — Datos semilla para Sprint 1
-- ============================================================================
-- Ejecutar desde el SQL Editor de Supabase después de las migraciones.
-- Usa UUIDs fijos para referencias consistentes entre tablas.
-- Contraseñas: Admin123!, Mesero789!, Cocina456!, Encargado321!
-- (Cambiar en producción inmediatamente después del primer login)
-- ============================================================================

-- 1. ROLES
-- ============================================================================
INSERT INTO "Roles" ("Id", "Nombre", "Descripcion", "Activo")
VALUES
    ('a0000000-0000-0000-0000-000000000001', 'Administrador', 'Acceso total al sistema', true),
    ('a0000000-0000-0000-0000-000000000002', 'Encargado',  'Gestión de catálogo, mesas y reportes', true),
    ('a0000000-0000-0000-0000-000000000003', 'Mesero',      'Captura de pedidos y consulta de salón', true),
    ('a0000000-0000-0000-0000-000000000004', 'Cocinero',    'Visualización de pedidos en preparación', true)
ON CONFLICT ("Id") DO NOTHING;

-- 2. PERMISOS
-- ============================================================================
INSERT INTO "Permisos" ("Id", "Nombre", "Modulo", "Descripcion")
VALUES
    ('b0000000-0000-0000-0000-000000000001', 'productos.leer',      'Productos', 'Ver lista de productos y categorías'),
    ('b0000000-0000-0000-0000-000000000002', 'productos.crear',     'Productos', 'Crear nuevos productos'),
    ('b0000000-0000-0000-0000-000000000003', 'productos.editar',    'Productos', 'Modificar productos existentes'),
    ('b0000000-0000-0000-0000-000000000004', 'productos.desactivar','Productos', 'Desactivar productos'),
    ('b0000000-0000-0000-0000-000000000005', 'pedidos.crear',       'Pedidos',   'Crear nuevos pedidos'),
    ('b0000000-0000-0000-0000-000000000006', 'pedidos.ver_todos',   'Pedidos',   'Ver todos los pedidos activos'),
    ('b0000000-0000-0000-0000-000000000007', 'pedidos.modificar',   'Pedidos',   'Modificar pedidos existentes'),
    ('b0000000-0000-0000-0000-000000000008', 'pedidos.cancelar',    'Pedidos',   'Cancelar pedidos'),
    ('b0000000-0000-0000-0000-000000000009', 'pedidos.pagar',       'Pedidos',   'Pagar y cerrar pedidos'),
    ('b0000000-0000-0000-0000-000000000010', 'mesas.leer',          'Mesas',     'Ver estado de mesas'),
    ('b0000000-0000-0000-0000-000000000011', 'mesas.gestionar',     'Mesas',     'Gestionar mesas (crear/editar/estado)'),
    ('b0000000-0000-0000-0000-000000000012', 'usuarios.gestionar',  'Usuarios',  'Administrar usuarios y roles'),
    ('b0000000-0000-0000-0000-000000000013', 'reportes.ver',        'Reportes',  'Ver reportes y estadísticas')
ON CONFLICT ("Id") DO NOTHING;

-- 3. ROLES-PERMISOS
-- ============================================================================
-- Administrador: todos los permisos
INSERT INTO "RolesPermisos" ("RolId", "PermisoId")
SELECT 'a0000000-0000-0000-0000-000000000001', "Id" FROM "Permisos"
ON CONFLICT DO NOTHING;

-- Encargado: productos + mesas + reportes + ver pedidos
INSERT INTO "RolesPermisos" ("RolId", "PermisoId") VALUES
    ('a0000000-0000-0000-0000-000000000002', 'b0000000-0000-0000-0000-000000000001'),
    ('a0000000-0000-0000-0000-000000000002', 'b0000000-0000-0000-0000-000000000002'),
    ('a0000000-0000-0000-0000-000000000002', 'b0000000-0000-0000-0000-000000000003'),
    ('a0000000-0000-0000-0000-000000000002', 'b0000000-0000-0000-0000-000000000004'),
    ('a0000000-0000-0000-0000-000000000002', 'b0000000-0000-0000-0000-000000000006'),
    ('a0000000-0000-0000-0000-000000000002', 'b0000000-0000-0000-0000-000000000010'),
    ('a0000000-0000-0000-0000-000000000002', 'b0000000-0000-0000-0000-000000000011'),
    ('a0000000-0000-0000-0000-000000000002', 'b0000000-0000-0000-0000-000000000013')
ON CONFLICT DO NOTHING;

-- Mesero: crear/ver pedidos + ver mesas/productos
INSERT INTO "RolesPermisos" ("RolId", "PermisoId") VALUES
    ('a0000000-0000-0000-0000-000000000003', 'b0000000-0000-0000-0000-000000000001'),
    ('a0000000-0000-0000-0000-000000000003', 'b0000000-0000-0000-0000-000000000005'),
    ('a0000000-0000-0000-0000-000000000003', 'b0000000-0000-0000-0000-000000000006'),
    ('a0000000-0000-0000-0000-000000000003', 'b0000000-0000-0000-0000-000000000007'),
    ('a0000000-0000-0000-0000-000000000003', 'b0000000-0000-0000-0000-000000000008'),
    ('a0000000-0000-0000-0000-000000000003', 'b0000000-0000-0000-0000-000000000010')
ON CONFLICT DO NOTHING;

-- Cocinero: ver pedidos en preparación
INSERT INTO "RolesPermisos" ("RolId", "PermisoId") VALUES
    ('a0000000-0000-0000-0000-000000000004', 'b0000000-0000-0000-0000-000000000006')
ON CONFLICT DO NOTHING;

-- 4. USUARIOS (contraseñas hasheadas con BCrypt work factor 12)
-- ============================================================================
INSERT INTO "Usuarios" ("Id", "Username", "Email", "PasswordHash", "NombreCompleto", "RolId", "Activo")
VALUES
    ('c0000000-0000-0000-0000-000000000001', 'admin', 'admin@mesadelduque.com',
     '$2a$12$o/8cIcp6U01ERzpTu24Kee4id7.MC1zvkJ2QU94DpnZSTvHijJ2oa',
     'Administrador del Sistema', 'a0000000-0000-0000-0000-000000000001', true),
    ('c0000000-0000-0000-0000-000000000002', 'carlos', 'carlos@mesadelduque.com',
     '$2a$12$eE3CqpbRHFFwarMvTcZgk.WJp8iKbNFSMz70sfZlzhPNRgPnuVoPu',
     'Carlos Encargado', 'a0000000-0000-0000-0000-000000000002', true),
    ('c0000000-0000-0000-0000-000000000003', 'maria', 'maria@mesadelduque.com',
     '$2a$12$TgZ3OhmwXiIeYF0Br5a3YutLXdTV364i.p/Bj2Ec5VXzH9oKIRS22',
     'María Mesera', 'a0000000-0000-0000-0000-000000000003', true),
    ('c0000000-0000-0000-0000-000000000004', 'pedro', 'pedro@mesadelduque.com',
     '$2a$12$AOKs0Kkzr/90JDOT3zomF.KCOk2wViUndtGZRLgkGprqdMQ8Kgf/W',
     'Pedro Cocinero', 'a0000000-0000-0000-0000-000000000004', true)
ON CONFLICT ("Id") DO NOTHING;

-- 5. CATEGORÍAS DE PRODUCTO
-- ============================================================================
INSERT INTO "CategoriaProducto" ("Id", "Nombre", "Descripcion", "OrdenDisplay", "Activo")
VALUES
    ('d0000000-0000-0000-0000-000000000001', 'Entradas', 'Aperitivos y entrantes', 1, true),
    ('d0000000-0000-0000-0000-000000000002', 'Platos Fuertes', 'Platos principales', 2, true),
    ('d0000000-0000-0000-0000-000000000003', 'Bebidas', 'Bebidas alcohólicas y sin alcohol', 3, true),
    ('d0000000-0000-0000-0000-000000000004', 'Postres', 'Postres y dulces', 4, true),
    ('d0000000-0000-0000-0000-000000000005', 'Acompañantes', 'Guarniciones y extras', 5, true)
ON CONFLICT ("Id") DO NOTHING;

-- 6. PRODUCTOS
-- ============================================================================
INSERT INTO "Producto" ("Id", "Nombre", "Precio", "CategoriaId", "Descripcion", "Activo", "TiempoPreparacionMin")
VALUES
    -- Entradas
    ('e0000000-0000-0000-0000-000000000001', 'Bruschetta Clásica', 8.50, 'd0000000-0000-0000-0000-000000000001', 'Pan tostado con tomate, albahaca y ajo', true, 8),
    ('e0000000-0000-0000-0000-000000000002', 'Croquetas de Jamón', 7.00, 'd0000000-0000-0000-0000-000000000001', 'Croquetas cremosas de jamón serrano (6 uds)', true, 10),
    ('e0000000-0000-0000-0000-000000000003', 'Ensalada de la Casa', 9.00, 'd0000000-0000-0000-0000-000000000001', 'Mix de lechugas, tomate cherry, nueces y queso de cabra', true, 5),
    -- Platos Fuertes
    ('e0000000-0000-0000-0000-000000000004', 'Solomillo al Duque', 24.00, 'd0000000-0000-0000-0000-000000000002', 'Solomillo de res con salsa de vino tinto y puré de papas', true, 25),
    ('e0000000-0000-0000-0000-000000000005', 'Salmón a la Parrilla', 21.00, 'd0000000-0000-0000-0000-000000000002', 'Salmón fresco con vegetales salteados y arroz', true, 20),
    ('e0000000-0000-0000-0000-000000000006', 'Pasta Alfredo', 15.00, 'd0000000-0000-0000-0000-000000000002', 'Fettuccine en salsa cremosa con pollo y parmesano', true, 15),
    ('e0000000-0000-0000-0000-000000000007', 'Hamburguesa Ducal', 16.50, 'd0000000-0000-0000-0000-000000000002', 'Angus beef 200g, queso cheddar, bacon, lechuga y tomate', true, 12),
    -- Bebidas
    ('e0000000-0000-0000-0000-000000000008', 'Agua Mineral', 2.50, 'd0000000-0000-0000-0000-000000000003', 'Agua mineral natural 500ml', true, 1),
    ('e0000000-0000-0000-0000-000000000009', 'Refresco', 3.00, 'd0000000-0000-0000-0000-000000000003', 'Coca-Cola, Sprite o Fanta 355ml', true, 1),
    ('e0000000-0000-0000-0000-000000000010', 'Cerveza Artesanal', 6.00, 'd0000000-0000-0000-0000-000000000003', 'Cerveza artesanal local 330ml', true, 1),
    ('e0000000-0000-0000-0000-000000000011', 'Vino Tinto Copa', 8.00, 'd0000000-0000-0000-0000-000000000003', 'Vino tinto de la casa por copa', true, 1),
    -- Postres
    ('e0000000-0000-0000-0000-000000000012', 'Tiramisú', 9.00, 'd0000000-0000-0000-0000-000000000004', 'Tiramisú clásico con mascarpone y cacao', true, 5),
    ('e0000000-0000-0000-0000-000000000013', 'Flan de Caramelo', 6.50, 'd0000000-0000-0000-0000-000000000004', 'Flan casero con caramelo líquido', true, 3),
    -- Acompañantes
    ('e0000000-0000-0000-0000-000000000014', 'Papas Fritas', 5.00, 'd0000000-0000-0000-0000-000000000005', 'Papas fritas crujientes con sal de ajo', true, 8),
    ('e0000000-0000-0000-0000-000000000015', 'Arroz Blanco', 4.00, 'd0000000-0000-0000-0000-000000000005', 'Arroz blanco al vapor', true, 10)
ON CONFLICT ("Id") DO NOTHING;

-- 7. MESAS (10 mesas para el salón)
-- ============================================================================
INSERT INTO "Mesa" ("Id", "Numero", "Capacidad", "Estado", "Activa")
VALUES
    ('f0000000-0000-0000-0000-000000000001', 1, 4, 'Disponible', true),
    ('f0000000-0000-0000-0000-000000000002', 2, 4, 'Disponible', true),
    ('f0000000-0000-0000-0000-000000000003', 3, 2, 'Disponible', true),
    ('f0000000-0000-0000-0000-000000000004', 4, 2, 'Disponible', true),
    ('f0000000-0000-0000-0000-000000000005', 5, 6, 'Disponible', true),
    ('f0000000-0000-0000-0000-000000000006', 6, 6, 'Disponible', true),
    ('f0000000-0000-0000-0000-000000000007', 7, 4, 'Disponible', true),
    ('f0000000-0000-0000-0000-000000000008', 8, 4, 'Disponible', true),
    ('f0000000-0000-0000-0000-000000000009', 9, 8, 'Disponible', true),
    ('f0000000-0000-0000-0000-000000000010', 10, 8, 'Reservada', true)
ON CONFLICT ("Id") DO NOTHING;

-- 8. PROVEEDOR
-- ============================================================================
INSERT INTO "Proveedor" ("Id", "Nombre", "Nit", "Contacto", "Telefono", "Email", "Activo")
VALUES
    ('g0000000-0000-0000-0000-000000000001', 'Distribuidora La Económica', '0614-010101-001-1', 'Juan Distribuidor', '2222-3333', 'ventas@laeconomica.com', true)
ON CONFLICT ("Id") DO NOTHING;

-- 9. INGREDIENTES (algunos para pruebas de recetas)
-- ============================================================================
INSERT INTO "Ingrediente" ("Id", "Nombre", "UnidadMedida", "StockActual", "StockMinimo", "CostoUnitario", "ProveedorDefaultId", "Activo")
VALUES
    ('h0000000-0000-0000-0000-000000000001', 'Tomate', 'kg', 25.0, 5.0, 2.50, 'g0000000-0000-0000-0000-000000000001', true),
    ('h0000000-0000-0000-0000-000000000002', 'Queso Mozzarella', 'kg', 10.0, 2.0, 8.00, 'g0000000-0000-0000-0000-000000000001', true),
    ('h0000000-0000-0000-0000-000000000003', 'Harina', 'kg', 50.0, 10.0, 1.20, 'g0000000-0000-0000-0000-000000000001', true),
    ('h0000000-0000-0000-0000-000000000004', 'Aceite de Oliva', 'L', 15.0, 3.0, 6.00, 'g0000000-0000-0000-0000-000000000001', true),
    ('h0000000-0000-0000-0000-000000000005', 'Pechuga de Pollo', 'kg', 20.0, 5.0, 7.50, 'g0000000-0000-0000-0000-000000000001', true)
ON CONFLICT ("Id") DO NOTHING;

-- 10. CONFIGURACIÓN DEL RESTAURANTE
-- ============================================================================
INSERT INTO "RestauranteConfigs" ("Id", "Nombre", "Direccion", "Telefono", "HorarioApertura", "HorarioCierre", "CantidadMesas")
VALUES
    (1, 'La Mesa del Duque', 'Calle Principal #123, San Salvador', '2222-1111', '11:00', '22:00', 10)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================================
-- VERIFICACIÓN
-- ============================================================================
SELECT 'Roles' AS tabla, count(*) AS registros FROM "Roles"
UNION ALL SELECT 'Permisos', count(*) FROM "Permisos"
UNION ALL SELECT 'RolesPermisos', count(*) FROM "RolesPermisos"
UNION ALL SELECT 'Usuarios', count(*) FROM "Usuarios"
UNION ALL SELECT 'CategoriaProducto', count(*) FROM "CategoriaProducto"
UNION ALL SELECT 'Producto', count(*) FROM "Producto"
UNION ALL SELECT 'Mesa', count(*) FROM "Mesa"
UNION ALL SELECT 'Proveedor', count(*) FROM "Proveedor"
UNION ALL SELECT 'Ingrediente', count(*) FROM "Ingrediente"
UNION ALL SELECT 'RestauranteConfigs', count(*) FROM "RestauranteConfigs"
ORDER BY tabla;
