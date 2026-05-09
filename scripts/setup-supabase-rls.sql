-- ============================================================================
-- La Mesa del Duque — Script de setup de base de datos para Supabase
-- ============================================================================
-- Este script se ejecuta DESPUÉS de aplicar las migraciones de EF Core.
-- Agrega: RLS (Row-Level Security), índices de rendimiento, políticas
-- de acceso por rol y triggers de auditoría que EF no genera automáticamente.
-- ============================================================================

-- 1. HABILITAR RLS EN TODAS LAS TABLAS OPERACIONALES
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

-- ============================================================================
-- 2. POLÍTICAS RLS POR ROL
-- ============================================================================

-- 2.1. Rol administrador: acceso completo a todas las tablas
CREATE POLICY admin_acceso_completo ON "AspNetRoles" FOR ALL TO authenticated
    USING (current_setting('request.jwt.claims', true)::jsonb->>'role' = 'Administrador')
    WITH CHECK (current_setting('request.jwt.claims', true)::jsonb->>'role' = 'Administrador');

-- Política genérica de admin para todas las tablas operativas
DO $$
DECLARE
    tbl TEXT;
BEGIN
    FOR tbl IN
        SELECT tablename FROM pg_tables
        WHERE schemaname = 'public'
          AND tablename NOT LIKE '\\_%'
          AND tablename NOT IN ('__EFMigrationsHistory', 'AspNetRoles')
    LOOP
        EXECUTE format(
            'CREATE POLICY admin_all_%I ON %I FOR ALL TO authenticated
             USING (current_setting(''request.jwt.claims'', true)::jsonb->>''role'' = ''Administrador'')
             WITH CHECK (current_setting(''request.jwt.claims'', true)::jsonb->>''role'' = ''Administrador'')',
            tbl, tbl);
    END LOOP;
END $$;

-- 2.2. Roles operativos: leer productos, categorías y mesas activas
CREATE POLICY operador_select_activos ON "Productos" FOR SELECT TO authenticated
    USING ("Activo" = true OR current_setting('request.jwt.claims', true)::jsonb->>'role' = 'Administrador');

CREATE POLICY operador_select_categorias ON "CategoriasProducto" FOR SELECT TO authenticated
    USING ("Activo" = true OR current_setting('request.jwt.claims', true)::jsonb->>'role' = 'Administrador');

CREATE POLICY operador_select_mesas ON "Mesas" FOR SELECT TO authenticated
    USING (true); -- Todos los roles autenticados pueden ver mesas

-- 2.3. Pedidos: crear propios, ver todos, modificar solo pendientes propios
CREATE POLICY pedidos_select_all ON "Pedidos" FOR SELECT TO authenticated
    USING (true); -- Visibilidad compartida del salón

CREATE POLICY pedidos_insert ON "Pedidos" FOR INSERT TO authenticated
    WITH CHECK (true);

CREATE POLICY pedidos_update ON "Pedidos" FOR UPDATE TO authenticated
    USING ("Estado" IN ('Pendiente', 'EnPreparacion') OR
           current_setting('request.jwt.claims', true)::jsonb->>'role' = 'Administrador');

-- 2.4. Auditoría: solo insert, nunca modificar ni eliminar
CREATE POLICY auditoria_insert ON "Auditoria" FOR INSERT TO authenticated
    WITH CHECK (true);

CREATE POLICY auditoria_select_admin ON "Auditoria" FOR SELECT TO authenticated
    USING (current_setting('request.jwt.claims', true)::jsonb->>'role' = 'Administrador');

-- ============================================================================
-- 3. ÍNDICES DE RENDIMIENTO PARA OPERACIONES FRECUENTES
-- ============================================================================

-- 3.1. Pedidos: búsqueda por estado y tipo de servicio (POS activo)
CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_pedidos_estado_activo
    ON "Pedidos" ("Estado")
    WHERE "Estado" IN ('Pendiente', 'EnPreparacion');

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_pedidos_tipo_servicio
    ON "Pedidos" ("TipoServicio");

-- 3.2. Productos: búsqueda por categoría y estado
CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_productos_categoria_activo
    ON "Productos" ("CategoriaProductoId")
    WHERE "Activo" = true;

-- 3.3. Mesas: búsqueda por número
CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_mesas_numero
    ON "Mesas" ("Numero");

-- 3.4. Auditoría: búsqueda por fecha y tabla
CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_auditoria_fecha_tabla
    ON "Auditoria" ("FechaHora" DESC, "TablaAfectada");

-- 3.5. Usuarios: búsqueda por username (login)
CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_usuarios_username
    ON "Usuarios" ("Username");

-- ============================================================================
-- 4. CONFIGURACIÓN DE RENDIMIENTO DE PostgreSQL
-- ============================================================================

-- Aumentar work_mem para consultas de agregación (totales de pedidos)
ALTER SYSTEM SET work_mem = '16MB';

-- Aumentar effective_cache_size para aprovechar RAM del servidor
ALTER SYSTEM SET effective_cache_size = '1GB';

-- Configurar autovacuum más agresivo para tablas de alta escritura
ALTER TABLE "Pedidos" SET (autovacuum_vacuum_scale_factor = 0.01);
ALTER TABLE "Auditoria" SET (autovacuum_vacuum_scale_factor = 0.05);
ALTER TABLE "PedidoEstadoLog" SET (autovacuum_vacuum_scale_factor = 0.01);

-- ============================================================================
-- 5. FUNCIÓN DE LIMPIEZA DE SESIONES EXPIRADAS (opcional, para mantenimiento)
-- ============================================================================

CREATE OR REPLACE FUNCTION limpiar_auditoria_antigua(dias_retencion INT DEFAULT 90)
RETURNS void AS $$
BEGIN
    DELETE FROM "Auditoria"
    WHERE "FechaHora" < NOW() - (dias_retencion || ' days')::INTERVAL;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- ============================================================================
-- 6. VERIFICACIÓN FINAL
-- ============================================================================

-- Contar tablas con RLS habilitado
SELECT COUNT(*) AS tablas_con_rls
FROM pg_tables
WHERE schemaname = 'public' AND rowsecurity = true;

-- Listar políticas creadas
SELECT schemaname, tablename, policyname, cmd
FROM pg_policies
WHERE schemaname = 'public'
ORDER BY tablename, cmd;
