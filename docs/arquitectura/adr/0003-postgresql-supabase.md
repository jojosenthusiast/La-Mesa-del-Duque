# ADR 0003: PostgreSQL y Supabase

## Estado

**Aceptado**

## Contexto

El sistema **La Mesa del Duque** necesita una base de datos relacional para almacenar pedidos, productos, recetas, mesas, usuarios y roles. Los requisitos incluyen:

- Persistencia confiable y transaccional (ACID).
- Capacidad de consulta relacional (JOINs entre pedidos y productos, usuarios y roles, etc.).
- Integración fluida con Entity Framework Core 8.
- Entorno de base de datos gestionado para evitar administración de servidores.
- Disponibilidad para desarrollo colaborativo (múltiples miembros del equipo conectándose).

Se evaluaron las siguientes alternativas:

1. **PostgreSQL en Supabase**: Base de datos como servicio (DBaaS) con PostgreSQL gestionado.
2. **SQL Server LocalDB**: Instancia local de SQL Server para desarrollo.
3. **SQLite**: Base de datos ligera basada en archivo.
4. **PostgreSQL local**: Instancia de PostgreSQL instalada en las máquinas de desarrollo.

## Decisión

Se adopta **PostgreSQL** como motor de base de datos y **Supabase** como plataforma de alojamiento gestionada.

PostgreSQL se integra de forma nativa con EF Core a través del proveedor **Npgsql**, ofreciendo todas las capacidades relacionales necesarias (claves foráneas, restricciones, índices, transacciones). Supabase proporciona una instancia de PostgreSQL en la nube gratuita (plan starter), eliminando la necesidad de que cada desarrollador configure su propio servidor de base de datos.

## Consecuencias

### Positivas

- **Cero administración de servidores**: Supabase gestiona parches, respaldos y disponibilidad.
- **Entorno compartido**: Todo el equipo se conecta a la misma base de datos en la nube, garantizando consistencia en los datos de prueba.
- **Migraciones con EF Core**: Las migraciones se aplican con `dotnet ef database update`, igual que en cualquier otro proveedor PostgreSQL.
- **PostgreSQL es open source**: Sin costos de licencia, con amplia comunidad y documentación.
- **Cumplimiento ACID**: Garantiza integridad de datos en transacciones de pedidos.
- **Escalabilidad futura**: Supabase permite escalar a planes pagos si el proyecto lo requiriera.

### Negativas

- **Dependencia de internet**: Sin conexión a internet, la base de datos no es accesible. Esto puede afectar sesiones de desarrollo offline.
- **Latencia**: Las consultas van a través de internet en lugar de ser locales, lo que puede añadir cierta latencia (mitigada por connection pooling).
- **Límites del plan gratuito**: El plan gratuito de Supabase tiene límites de almacenamiento y conexiones concurrentes.
- **Lock-in leve**: Aunque PostgreSQL es portable, algunas características de Supabase (como la API REST automática) no se usan en este proyecto para mantener portabilidad.

### Mitigaciones

- Para desarrollo offline, se documenta cómo apuntar a una instancia local de PostgreSQL usando Docker:
  ```bash
  docker run -d --name postgres-dev -e POSTGRES_PASSWORD=dev -p 5432:5432 postgres:15
  ```
- Se configura connection pooling en EF Core para minimizar el impacto de latencia.
- La cadena de conexión se externaliza en `appsettings.Development.json`, permitiendo cambiar entre Supabase y PostgreSQL local sin modificar código.
- Se mantiene un backup local periódico de los datos de desarrollo.

## Alternativas consideradas

| Alternativa              | Razón del descarte                                            |
|--------------------------|---------------------------------------------------------------|
| SQL Server LocalDB       | Solo Windows; el equipo puede usar macOS/Linux. No es DBaaS.  |
| SQLite                   | No soporta múltiples conexiones concurrentes de escritura; el equipo colabora en la misma BD. EF Core lo soporta pero no es adecuado para un sistema con múltiples usuarios concurrentes reales. |
| PostgreSQL local         | Requiere que cada desarrollador instale y configure PostgreSQL. Agrega fricción y posibles diferencias de configuración entre entornos. |

## Configuración de conexión

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=aws-0-region.pooler.supabase.com;Database=postgres;Username=postgres;Password=****"
  }
}
```

## Relación con otros ADR

- **ADR-0001**: La infraestructura de persistencia reside en la capa de Infraestructura. El dominio no conoce PostgreSQL ni Supabase.
- **ADR-0002**: Las Razor Pages obtienen datos a través de servicios de dominio, que delegan en repositorios implementados en Infraestructura con EF Core.

---

**Fecha**: Abril 2026 | **Decisores**: Arquitecto de software, equipo de desarrollo
