# Estrategia de Pruebas — La Mesa del Duque

## 1. Propósito

Este documento define la estrategia de pruebas del sistema **La Mesa del Duque**. Establece los niveles de prueba, las herramientas, los criterios de cobertura, el flujo de ejecución y la responsabilidad de cada tipo de prueba, alineado con el plan de calidad y el modelo ISO/IEC 25010.

## 2. Niveles de prueba

Se adopta la pirámide de pruebas clásica, adaptada a la arquitectura en capas:

```
        ┌──────────┐
        │   E2E    │ ← Pocas. Flujos críticos manuales documentados.
        │ (Manual) │
       ┌┴──────────┴┐
       │ Integración │ ← WebApplicationFactory, BD real en memoria o local.
       │  (xUnit)    │
      ┌┴─────────────┴┐
      │   Unitarias    │ ← Lógica de dominio pura, servicios, validaciones.
      │   (xUnit)      │
     └─────────────────┘
```

### 2.1 Pruebas unitarias

| Aspecto             | Detalle                                                 |
|---------------------|---------------------------------------------------------|
| **Propósito**       | Verificar la lógica de negocio de forma aislada.        |
| **Alcance**         | Entidades de dominio, value objects, servicios de dominio, validaciones. |
| **Framework**       | xUnit 2.5                                               |
| **Cobertura**       | Coverlet 6.0. Reporte en GitHub Actions.                |
| **Objetivo**        | ≥ 80% de cobertura de código en la capa de Dominio.      |
| **Nomenclatura**    | `Debe_[ResultadoEsperado]_Cuando_[Condicion]`            |
| **Aisladas**        | Sin dependencias externas. Se usan mocks (Moq o NSubstitute) para repositorios. |

**Ejemplo de convención:**

```csharp
[Fact]
public void Debe_CrearPedido_Cuando_DatosSonValidos()
{
    // Arrange, Act, Assert
}
```

### 2.2 Pruebas de integración

| Aspecto             | Detalle                                                 |
|---------------------|---------------------------------------------------------|
| **Propósito**       | Verificar la interacción entre capas: Web → Infraestructura → PostgreSQL. |
| **Alcance**         | Repositorios EF Core, Razor Pages (endpoints), flujo autenticación. |
| **Framework**       | xUnit 2.5 + `Microsoft.AspNetCore.Mvc.Testing` (WebApplicationFactory). |
| **Base de datos**   | PostgreSQL local vía Docker para CI; Supabase para desarrollo. |
| **Objetivo**        | Todos los repositorios y endpoints de API/PageModel cubiertos. |

### 2.3 Pruebas de regresión

| Aspecto             | Detalle                                                 |
|---------------------|---------------------------------------------------------|
| **Propósito**       | Garantizar que cambios nuevos no rompen funcionalidad existente. |
| **Alcance**         | Catálogo definido en `docs/calidad/suite-regresion.md`. |
| **Ejecución**       | Automática en cada PR (Nivel 1+2) y pre-release (Nivel 1+2+3). |
| **Ubicación**       | `tests/regresion/`.                                     |

### 2.4 Pruebas de seguridad

| Aspecto             | Detalle                                                 |
|---------------------|---------------------------------------------------------|
| **Propósito**       | Verificar controles de autenticación, autorización, CSRF y protección de datos. |
| **Tipo**            | Manual (documentadas) + automatizadas (xUnit para RBAC y CSRF). |
| **Checklist**       | `docs/calidad/checklist-seguridad.md`.                  |

### 2.5 Pruebas de aceptación (UAT)

| Aspecto             | Detalle                                                 |
|---------------------|---------------------------------------------------------|
| **Propósito**       | Validar que el sistema cumple los criterios de aceptación definidos en cada HU. |
| **Responsable**     | QA / Product Owner (rol académico).                     |
| **Evidencia**       | Documentada en `docs/auditoria/checklist-evidencia-auditoria.md`. |

## 3. Herramientas

| Herramienta              | Uso                                                  |
|--------------------------|------------------------------------------------------|
| xUnit 2.5                | Framework de pruebas unitarias e integración.        |
| Coverlet 6.0             | Cobertura de código.                                 |
| WebApplicationFactory    | Pruebas de integración de Razor Pages con servidor en memoria. |
| Docker                   | Entorno de PostgreSQL local para CI.                 |
| GitHub Actions           | Ejecución automatizada de pruebas en cada PR.        |

## 4. Entorno de pruebas

| Entorno         | Configuración                                   |
|-----------------|-------------------------------------------------|
| **CI**          | Ubuntu + Docker (PostgreSQL 15) + .NET 8 SDK    |
| **Desarrollo**  | PostgreSQL Supabase + .NET 8 SDK local          |
| **Aislamiento** | Cada ejecución de prueba crea/limpia su propio contexto de BD. |

## 5. Criterios de cobertura

| Capa               | Cobertura objetivo | Prioridad |
|--------------------|--------------------|-----------|
| Dominio             | ≥ 90%              | Crítica   |
| Infraestructura     | ≥ 70%              | Alta      |
| Web (PageModels)    | ≥ 60%              | Media     |
| Global              | ≥ 80%              | Alta      |

Las áreas sin cobertura deben justificarse en el informe de pruebas del sprint.

## 6. Flujo de ejecución

```
Developer escribe código
        │
        ▼
Ejecuta pruebas unitarias localmente (dotnet test)
        │
        ▼
Push a feature/*
        │
        ▼
GitHub Actions: compilación + pruebas unitarias + smoke tests
        │
        ▼
Pull Request → pruebas de integración + regresión Nivel 1+2
        │
        ▼
Revisión de código + checklist seguridad
        │
        ▼
Merge a develop → regresión Nivel 3
        │
        ▼
Pre-release → suite completa + UAT manual
```

## 7. Reporte de pruebas

Cada sprint genera un informe de pruebas que incluye:

- Total de pruebas ejecutadas / pasaron / fallaron.
- Cobertura de código por capa.
- Defectos encontrados, clasificados por severidad.
- Casos de regresión ejecutados y resultados.
- Trazabilidad: cada prueba vinculada a su HU y criterio de aceptación.

## 8. Gestión de defectos

| Severidad | Definición                                        | Acción                           |
|-----------|---------------------------------------------------|----------------------------------|
| Crítica   | Bloquea funcionalidad principal, sin workaround.   | Corrección inmediata. Bloquea release. |
| Alta      | Funcionalidad afectada pero con workaround.        | Corrección en el sprint actual.  |
| Media     | No bloquea funcionalidad; afecta usabilidad.       | Priorizar en backlog.            |
| Baja      | Cosmético o mejora menor.                          | Registrar; corregir si hay tiempo.|

---

**Versión**: 1.0 | **Fecha**: Abril 2026 | **Revisar al inicio de cada sprint**
