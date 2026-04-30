# Registro de Riesgos — La Mesa del Duque

## 1. Propósito

Identificar, evaluar y gestionar los riesgos que pueden afectar la calidad, el cronograma o el cumplimiento de los requisitos del sistema **La Mesa del Duque**. Cada riesgo se clasifica por probabilidad de ocurrencia e impacto, y se define una estrategia de mitigación y un plan de contingencia.

## 2. Escala de evaluación

### Probabilidad

| Nivel | Valor | Descripción               |
|-------|-------|---------------------------|
| Baja  | 1     | < 20% de ocurrencia       |
| Media | 2     | 20% – 50% de ocurrencia   |
| Alta  | 3     | > 50% de ocurrencia       |

### Impacto

| Nivel  | Valor | Descripción                                    |
|--------|-------|------------------------------------------------|
| Bajo   | 1     | Afecta marginalmente; sin bloqueo.             |
| Medio  | 2     | Afecta una HU o retrasa el sprint ≤ 2 días.    |
| Alto   | 3     | Bloquea una HU crítica o retrasa > 2 días.     |

### Severidad = Probabilidad × Impacto

| Rango | Clasificación |
|-------|---------------|
| 1 – 2 | Baja          |
| 3 – 4 | Media         |
| 6 – 9 | Alta          |

## 3. Riesgos identificados

### R-01: Indisponibilidad de Supabase

| Campo              | Detalle                                                     |
|--------------------|-------------------------------------------------------------|
| **Descripción**    | El servicio de base de datos PostgreSQL en Supabase no está disponible por mantenimiento, caída del proveedor o problemas de conectividad. |
| **Probabilidad**   | Baja (1) — Supabase tiene SLA alto.                         |
| **Impacto**        | Alto (3) — Bloquea toda operación que dependa de la BD.     |
| **Severidad**      | Media (3)                                                   |
| **Mitigación**     | - Mantener respaldo local de la BD en SQLite durante desarrollo. - Documentar procedimiento de migración a PostgreSQL local. |
| **Contingencia**   | Migrar temporalmente a instancia PostgreSQL local.          |
| **Estado**         | Activo                                                      |
| **Responsable**    | Desarrollador de infraestructura                            |

### R-02: Complejidad no prevista en SignalR

| Campo              | Detalle                                                     |
|--------------------|-------------------------------------------------------------|
| **Descripción**    | La implementación de tiempo real con SignalR para pedidos resulta más compleja de lo estimado, afectando el cronograma del sprint. |
| **Probabilidad**   | Media (2) — SignalR tiene curva de aprendizaje.             |
| **Impacto**        | Medio (2) — Puede retrasar HU-001.                          |
| **Severidad**      | Media (4)                                                   |
| **Mitigación**     | - Prototipo temprano de conexión SignalR. - Limitar el alcance inicial a actualizaciones básicas. |
| **Contingencia**   | Degradar a polling con recarga de página cada 30 segundos como solución temporal. |
| **Estado**         | Activo                                                      |
| **Responsable**    | Desarrollador web                                           |

### R-03: Fuga de datos sensibles por mala configuración

| Campo              | Detalle                                                     |
|--------------------|-------------------------------------------------------------|
| **Descripción**    | Configuración incorrecta de CORS, exposición de cadenas de conexión en el repositorio, o falta de HTTPS en algún entorno expone datos sensibles. |
| **Probabilidad**   | Media (2) — Errores comunes en proyectos académicos.        |
| **Impacto**        | Alto (3) — Compromete seguridad de datos.                   |
| **Severidad**      | Alta (6)                                                    |
| **Mitigación**     | - Archivos `.env` / `appsettings` con secretos excluidos del repositorio (.gitignore). - Checklist de seguridad obligatorio por PR. - HSTS habilitado. - Revisión de secretos con herramientas automatizadas. |
| **Contingencia**   | Rotación inmediata de credenciales, invalidación de tokens. |
| **Estado**         | Activo                                                      |
| **Responsable**    | Responsable de seguridad                                    |

### R-04: Regresión por cambios en entidades compartidas

| Campo              | Detalle                                                     |
|--------------------|-------------------------------------------------------------|
| **Descripción**    | Modificar una entidad del dominio (Pedido, Producto, Mesa) rompe funcionalidad en otras HU que dependen de ella. |
| **Probabilidad**   | Media (2) — Acoplamiento natural en el dominio de restaurante. |
| **Impacto**        | Medio (2) — Requiere re-trabajo en HU afectadas.            |
| **Severidad**      | Media (4)                                                   |
| **Mitigación**     | - Suite de regresión automatizada ejecutada en cada PR. - Matriz de impacto de cambios actualizada. - Pruebas unitarias que verifican contratos de entidades. |
| **Contingencia**   | Revertir el cambio y planificar refactorización controlada. |
| **Estado**         | Activo                                                      |
| **Responsable**    | QA / Desarrollador                                          |

### R-05: Defectos de seguridad en autenticación (RBAC/CSRF)

| Campo              | Detalle                                                     |
|--------------------|-------------------------------------------------------------|
| **Descripción**    | La implementación de RBAC o CSRF tiene fallos que permiten escalación de privilegios o falsificación de solicitudes. |
| **Probabilidad**   | Media (2) — Implementación manual de seguridad es propensa a errores. |
| **Impacto**        | Alto (3) — Acceso no autorizado a funciones del sistema.    |
| **Severidad**      | Alta (6)                                                    |
| **Mitigación**     | - Usar mecanismos integrados de ASP.NET Core (AuthorizeAttribute, AntiForgeryToken). - Pruebas de penetración manuales. - Checklist de seguridad. - Revisión por responsable de seguridad. |
| **Contingencia**   | Deshabilitar funcionalidad afectada hasta su corrección, reforzar con middleware adicional. |
| **Estado**         | Activo                                                      |
| **Responsable**    | Responsable de seguridad                                    |

### R-06: Baja cobertura de pruebas

| Campo              | Detalle                                                     |
|--------------------|-------------------------------------------------------------|
| **Descripción**    | La cobertura de pruebas no alcanza el objetivo del 80% debido a falta de tiempo o complejidad de algunos componentes (UI, SignalR). |
| **Probabilidad**   | Alta (3) — Las pruebas de UI con Razor Pages son más complejas. |
| **Impacto**        | Medio (2) — Reduce confianza en regresiones.                |
| **Severidad**      | Alta (6)                                                    |
| **Mitigación**     | - Priorizar pruebas unitarias de dominio (alta relación valor/esfuerzo). - Usar pruebas de integración con `WebApplicationFactory`. - Diferir pruebas de UI complejas a sprint 2. |
| **Contingencia**   | Documentar explícitamente las áreas sin cobertura y su justificación en la estrategia de pruebas. |
| **Estado**         | Activo                                                      |
| **Responsable**    | QA / Desarrollador                                          |

### R-07: Inconsistencia en despliegue entre entornos

| Campo              | Detalle                                                     |
|--------------------|-------------------------------------------------------------|
| **Descripción**    | El sistema funciona en desarrollo local pero falla en Supabase o entorno de producción por diferencias de configuración. |
| **Probabilidad**   | Media (2) — Diferencias sutiles entre PostgreSQL local y Supabase. |
| **Impacto**        | Medio (2) — Retrasa entrega.                                |
| **Severidad**      | Media (4)                                                   |
| **Mitigación**     | - Usar Supabase como entorno de desarrollo desde el inicio. - Pipeline de CI que ejecuta pruebas contra base de datos real. - Migraciones probadas en ambos entornos. |
| **Contingencia**   | Ajustar configuración de entorno y re-ejecutar migraciones. |
| **Estado**         | Activo                                                      |
| **Responsable**    | Desarrollador de infraestructura                            |

## 4. Resumen de riesgos

| ID    | Riesgo                                         | Severidad | Estado  |
|-------|------------------------------------------------|-----------|---------|
| R-01  | Indisponibilidad de Supabase                  | Media     | Activo  |
| R-02  | Complejidad no prevista en SignalR            | Media     | Activo  |
| R-03  | Fuga de datos sensibles                       | Alta      | Activo  |
| R-04  | Regresión por cambios en entidades compartidas| Media     | Activo  |
| R-05  | Defectos de seguridad en autenticación        | Alta      | Activo  |
| R-06  | Baja cobertura de pruebas                     | Alta      | Activo  |
| R-07  | Inconsistencia en despliegue                  | Media     | Activo  |

## 5. Revisión de riesgos

| Fecha      | Revisión                               | Cambios |
|------------|----------------------------------------|---------|
| Abr 2026   | Registro inicial                      | 7 riesgos identificados |

---

**Versión**: 1.0 | **Fecha**: Abril 2026 | **Revisar al inicio de cada sprint**
