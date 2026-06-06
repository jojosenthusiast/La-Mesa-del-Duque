# Configuración de base de datos — La Mesa del Duque

## Requisitos previos

- Cuenta de [Supabase](https://supabase.com) con un proyecto activo
- Variable de entorno `LMD_CONNECTION_STRING` con la URL de conexión pooler de Supabase
- .NET 8 SDK

## 1. Configurar variable de entorno

```powershell
# Copiar la URL de conexión desde Supabase Dashboard → Settings → Database → Connection string → URI
$env:LMD_CONNECTION_STRING = "postgresql://postgres.[PROJECT_REF]:[PASSWORD]@aws-1-us-west-2.pooler.supabase.com:6543/postgres"
```

## 2. Aplicar migraciones

```bash
dotnet ef database update \
  --project src/LaMesaDelDuque.Infraestructura/LaMesaDelDuque.Infraestructura.csproj \
  --startup-project src/LaMesaDelDuque.Infraestructura/LaMesaDelDuque.Infraestructura.csproj
```

## 3. Aplicar políticas RLS y optimizaciones

```bash
# Conectar a Supabase y ejecutar el script de setup
psql "$env:LMD_CONNECTION_STRING" -f scripts/setup-supabase-rls.sql
```

Si `psql` no está instalado, ejecutar el script desde el SQL Editor de Supabase Dashboard.

## 4. Verificar conectividad

```bash
dotnet test LaMesaDelDuque.slnx --filter "FullyQualifiedName~Persistencia"
```

Todas las pruebas de integración deben pasar contra la base de datos real.

## Notas

- **ConexionHelper**: El proyecto normaliza automáticamente URLs de pooler al formato clave-valor que Npgsql entiende. Soporta contraseñas con caracteres especiales.
- **RLS**: Las políticas Row-Level Security restringen acceso por rol. En MVP actual la autenticación es vía cookies (no JWT de Supabase), por lo que las políticas RLS actúan como defensa en profundidad, no como mecanismo primario de autorización.
- **Pooler vs directa**: La conexión pooler (puerto 6543) es la recomendada para entornos serverless o con alta concurrencia. La conexión directa (puerto 5432) es útil para desarrollo local.
