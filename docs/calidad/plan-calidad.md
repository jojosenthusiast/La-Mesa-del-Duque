# Plan de Calidad — La Mesa del Duque

## 1. Propósito

Este documento establece el plan de calidad del sistema **La Mesa del Duque**, un sistema integral de gestión para restaurante. Define los objetivos de calidad, los procesos de aseguramiento (QA) y control de calidad (QC), los estándares aplicables, los roles responsables y las métricas que guiarán el desarrollo del proyecto durante el ciclo académico de Gestión de la Calidad del Software.

## 2. Alcance

El plan de calidad abarca:

- Todo el código fuente del repositorio (`src/`, `tests/`).
- Los artefactos de documentación de calidad (`docs/calidad/`, `docs/pruebas/`, `docs/seguridad/`, `docs/auditoria/`).
- Los procesos de integración continua y despliegue (GitHub Actions).
- Las pruebas unitarias, de integración y de regresión.
- La seguridad de la información alineada con ISO/IEC 27001.
- Los entregables del Sprint 1: HU-001, HU-002, HU-003, HU-011, HU-014, HU-016, HU-021, HU-025.

## 3. Estándares de referencia

| Estándar / Marco         | Aplicación en el proyecto                                    |
|--------------------------|--------------------------------------------------------------|
| ISO/IEC 25010            | Modelo de calidad del producto software                      |
| ISO/IEC 25002            | Medición de la calidad — métricas e indicadores              |
| ISO/IEC 27001            | Sistema de Gestión de Seguridad de la Información (SGSI)     |
| Principios SOLID         | Diseño de la arquitectura en capas                           |
| Git Flow (adaptado)      | Control de versiones y ramas                                 |
| Convenciones de commits  | `feat:`, `fix:`, `docs:`, `refactor:`, `test:`              |

## 4. Características de calidad (ISO/IEC 25010)

| Característica      | Subcaracterísticas aplicables                              | Prioridad |
|---------------------|------------------------------------------------------------|-----------|
| Funcionalidad       | Completitud funcional, corrección, pertinencia             | Alta      |
| Fiabilidad          | Madurez, tolerancia a fallos, disponibilidad               | Alta      |
| Seguridad           | Confidencialidad, integridad, autenticidad, responsabilidad| Alta      |
| Eficiencia          | Comportamiento temporal, utilización de recursos           | Media     |
| Mantenibilidad      | Modularidad, reusabilidad, analizabilidad, modificabilidad | Alta      |
| Portabilidad        | Adaptabilidad, reemplazabilidad                            | Media     |
| Usabilidad          | Operabilidad, protección frente a errores de usuario       | Media     |

## 5. Roles y responsabilidades

| Rol                    | Responsabilidad                                                                 |
|------------------------|---------------------------------------------------------------------------------|
| **Desarrollador**      | Escribir código siguiendo estándares, pruebas unitarias, revisiones de código.  |
| **Revisor de código**  | Ejecutar checklist de revisión, verificar cumplimiento de estándares.           |
| **QA / Tester**        | Ejecutar pruebas de integración, regresión y reportar defectos.                 |
| **Responsable de seguridad** | Verificar checklist de seguridad, declaración de aplicabilidad ISO 27001. |
| **Auditor**            | Realizar auditorías internas, recopilar y verificar evidencia.                  |

## 6. Procesos de calidad

### 6.1 Aseguramiento de Calidad (QA)

Actividades preventivas que garantizan que los procesos se ejecutan correctamente:

- Revisión de código con checklist (`docs/calidad/checklist-revision-codigo.md`).
- Verificación de seguridad con checklist (`docs/calidad/checklist-seguridad.md`).
- Definición de Hecho aplicada en cada historia de usuario.
- Pipeline de CI con GitHub Actions: compilación, pruebas, análisis estático.
- Trazabilidad de requisitos mantenida en la matriz de trazabilidad.

### 6.2 Control de Calidad (QC)

Actividades de detección de defectos en el producto:

- Pruebas unitarias con xUnit (cobertura mínima objetivo: 80%).
- Pruebas de integración sobre endpoints Razor Pages.
- Suite de regresión automatizada (`docs/calidad/suite-regresion.md`).
- Verificación de criterios de aceptación por historia de usuario.
- Análisis de impacto de cambios (`docs/calidad/matriz-impacto-cambios.md`).

### 6.3 Gestión de riesgos

- Registro de riesgos mantenido en `docs/calidad/registro-riesgos.md`.
- Revisión de riesgos al inicio de cada sprint y ante cambios significativos.
- Estrategias de mitigación documentadas para cada riesgo identificado.

## 7. Métricas e indicadores

Las métricas detalladas se documentan en `docs/metricas/indicadores-calidad.md`. A continuación se resumen las categorías principales:

| Categoría       | Indicador clave                    | Objetivo       |
|-----------------|------------------------------------|----------------|
| Defectos        | Densidad de defectos por HU        | < 2 por HU     |
| Cobertura       | Cobertura de código con pruebas    | ≥ 80%          |
| Seguridad       | Vulnerabilidades detectadas        | 0 críticas     |
| Trazabilidad    | Requisitos con trazabilidad completa | 100%        |
| Revisión        | Tasa de aprobación en revisión     | ≥ 90%          |

## 8. Entregables de calidad

| Entregable                                      | Ubicación                                  |
|-------------------------------------------------|--------------------------------------------|
| Plan de calidad                                 | `docs/calidad/plan-calidad.md`             |
| Definición de Hecho                             | `docs/calidad/definicion-de-hecho.md`      |
| Matriz de trazabilidad                          | `docs/calidad/matriz-trazabilidad.md`      |
| Registro de riesgos                             | `docs/calidad/registro-riesgos.md`         |
| Checklist de revisión de código                 | `docs/calidad/checklist-revision-codigo.md`|
| Checklist de seguridad                          | `docs/calidad/checklist-seguridad.md`      |
| Matriz de impacto de cambios                    | `docs/calidad/matriz-impacto-cambios.md`   |
| Suite de regresión                              | `docs/calidad/suite-regresion.md`          |
| Estrategia de pruebas                           | `docs/pruebas/estrategia-pruebas.md`       |
| Indicadores de calidad                          | `docs/metricas/indicadores-calidad.md`     |
| Alcance SGSI y riesgos                          | `docs/seguridad/alcance-sgsi-y-riesgos.md` |
| Declaración de aplicabilidad ISO 27001          | `docs/seguridad/declaracion-aplicabilidad-iso27001.md` |
| Plan de auditoría                               | `docs/auditoria/plan-auditoria.md`         |
| Checklist de evidencia de auditoría             | `docs/auditoria/checklist-evidencia-auditoria.md` |

## 9. Ciclo de vida de calidad

```
Planificación → Desarrollo → Revisión de código → Pruebas → Verificación de seguridad
                                                      ↓
                                              ¿Cumple DoD? → Sí → Aceptado
                                                      ↓ No
                                              Registrar defecto → Corregir
```

## 10. Revisión y actualización

Este plan de calidad se revisa y actualiza:

- Al inicio de cada sprint.
- Cuando se incorpora un nuevo estándar o requisito.
- Cuando un riesgo materializado requiere ajustar procesos.

---

**Versión**: 1.0 | **Fecha**: Abril 2026 | **Responsable**: Equipo de calidad
