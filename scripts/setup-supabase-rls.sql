-- ============================================================================
-- La Mesa del Duque — RLS y optimizaciones para Supabase
-- ============================================================================
-- Ejecutar desde el SQL Editor de Supabase DESPUÉS de aplicar migraciones.
-- Este script usa los nombres EXACTOS de tabla generados por EF Core.
-- ============================================================================

-- 1. HABILITAR RLS EN TODAS LAS TABLAS
-- ============================================================================
DO $$
DECLARE
    tbl TEXT;
BEGIN
    FOR tbl IN
        SELECT tablename FROM pg_tables
        WHERE schemaname = 'public'
          AND tablename NOT LIKE '\\_%'
          AND tablename NOT IN ('__EFMigrationsHistory', 'spatial_ref_sys')
    LOOP
        EXECUTE format('ALTER TABLE %I ENABLE ROW LEVEL SECURITY', tbl);
    END LOOP;
END $$;

-- 2. POLÍTICA DE ADMIN — acceso completo a todas las tablas
-- ============================================================================
DO $$
DECLARE
    tbl TEXT;
BEGIN
    FOR tbl IN
        SELECT tablename FROM pg_tables
        WHERE schemaname = 'public'
          AND tablename NOT LIKE '\\_%'
          AND tablename NOT IN ('__EFMigrationsHistory')
    LOOP
        EXECUTE format(
            'CREATE POLICY %I ON %I FOR ALL TO authenticated
             USING ((SELECT current_setting(''request.jwt.claims'', true)::jsonb->>''role'') = ''Administrador'')
             WITH CHECK ((SELECT current_setting(''request.jwt.claims'', true)::jsonb->>''role'') = ''Administrador'')',
            'admin_all_' || tbl, tbl);
    END LOOP;
END $$;

-- 3. POLÍTICA DE OPERADOR — lectura de catálogo y mesas
-- ============================================================================

-- Producto: todos los autenticados pueden ver activos
CREATE POLICY operador_select_producto ON "Producto" FOR SELECT TO authenticated
    USING ("Activo" = true
        OR (SELECT current_setting('request.jwt.claims', true)::jsonb->>'role') = 'Administrador');

-- CategoriaProducto: todos los autenticados pueden ver activas
CREATE POLICY operador_select_categoria ON "CategoriaProducto" FOR SELECT TO authenticated
    USING ("Activo" = true
        OR (SELECT current_setting('request.jwt.claims', true)::jsonb->>'role') = 'Administrador');

-- Mesa: todos los autenticados pueden ver
CREATE POLICY operador_select_mesa ON "Mesa" FOR SELECT TO authenticated
    USING (true);

-- Proveedor: todos los autenticados pueden ver
CREATE POLICY operador_select_proveedor ON "Proveedor" FOR SELECT TO authenticated
    USING (true);

-- Ingrediente: todos los autenticados pueden ver
CREATE POLICY operador_select_ingrediente ON "Ingrediente" FOR SELECT TO authenticated
    USING (true);

-- 4. POLÍTICA DE PEDIDOS — crear y ver todos, modificar solo activos
-- ============================================================================

-- Todos los autenticados pueden ver pedidos (visibilidad compartida del salón)
CREATE POLICY pedido_select ON "Pedido" FOR SELECT TO authenticated
    USING (true);

-- Todos los autenticados pueden crear pedidos
CREATE POLICY pedido_insert ON "Pedido" FOR INSERT TO authenticated
    WITH CHECK (true);

-- Solo modificar pedidos en estados activos (a menos que seas admin)
CREATE POLICY pedido_update ON "Pedido" FOR UPDATE TO authenticated
    USING ("Estado" IN ('Pendiente', 'EnPreparacion')
        OR (SELECT current_setting('request.jwt.claims', true)::jsonb->>'role') = 'Administrador');

-- DetallePedido: hereda visibilidad del pedido (RLS se aplica al insert también)
CREATE POLICY detalle_select ON "DetallePedido" FOR SELECT TO authenticated
    USING (true);

CREATE POLICY detalle_insert ON "DetallePedido" FOR INSERT TO authenticated
    WITH CHECK (true);

-- 5. POLÍTICA DE AUDITORÍA — append-only, solo admin lee
-- ============================================================================

CREATE POLICY auditoria_insert ON "Auditorias" FOR INSERT TO authenticated
    WITH CHECK (true);

CREATE POLICY auditoria_select ON "Auditorias" FOR SELECT TO authenticated
    USING ((SELECT current_setting('request.jwt.claims', true)::jsonb->>'role') = 'Administrador');

-- PedidosEstadosLog: append-only, visible para todos
CREATE POLICY estadolog_insert ON "PedidosEstadosLog" FOR INSERT TO authenticated
    WITH CHECK (true);

CREATE POLICY estadolog_select ON "PedidosEstadosLog" FOR SELECT TO authenticated
    USING (true);

-- 6. POLÍTICA DE USUARIOS — solo admin gestiona
-- ============================================================================

-- Solo admin ve la lista de usuarios y roles
CREATE POLICY usuarios_admin ON "Usuarios" FOR ALL TO authenticated
    USING ((SELECT current_setting('request.jwt.claims', true)::jsonb->>'role') = 'Administrador')
    WITH CHECK ((SELECT current_setting('request.jwt.claims', true)::jsonb->>'role') = 'Administrador');

CREATE POLICY roles_admin ON "Roles" FOR ALL TO authenticated
    USING ((SELECT current_setting('request.jwt.claims', true)::jsonb->>'role') = 'Administrador')
    WITH CHECK ((SELECT current_setting('request.jwt.claims', true)::jsonb->>'role') = 'Administrador');

-- ============================================================================
-- 7. ÍNDICES DE RENDIMIENTO
-- ============================================================================

-- Pedidos activos (consulta más frecuente del POS)
CREATE INDEX IF NOT EXISTS ix_pedido_estado_activo
    ON "Pedido" ("Estado")
    WHERE "Estado" IN ('Pendiente', 'EnPreparacion');

-- Productos por categoría (catálogo)
CREATE INDEX IF NOT EXISTS ix_producto_categoria_activo
    ON "Producto" ("CategoriaId")
    WHERE "Activo" = true;

-- Mesas por número (lookup rápido)
CREATE INDEX IF NOT EXISTS ix_mesa_numero ON "Mesa" ("Numero");

-- Auditoría por fecha (consultas de trazabilidad)
CREATE INDEX IF NOT EXISTS ix_auditoria_fecha
    ON "Auditorias" ("Fecha" DESC);

-- Usuarios por username (login)
CREATE INDEX IF NOT EXISTS ix_usuarios_username ON "Usuarios" ("Username");

-- ============================================================================
-- 8. CONFIGURACIÓN DE AUTOVACUUM PARA TABLAS DE ALTA ESCRITURA
-- ============================================================================

ALTER TABLE "Pedido" SET (autovacuum_vacuum_scale_factor = 0.01);
ALTER TABLE "Auditorias" SET (autovacuum_vacuum_scale_factor = 0.05);
ALTER TABLE "PedidosEstadosLog" SET (autovacuum_vacuum_scale_factor = 0.01);

-- ============================================================================
-- 9. VERIFICACIÓN
-- ============================================================================

SELECT count(*) AS tablas_con_rls
FROM pg_tables
WHERE schemaname = 'public' AND rowsecurity = true;

SELECT tablename, policyname, cmd
FROM pg_policies
WHERE schemaname = 'public'
ORDER BY tablename, cmd;
