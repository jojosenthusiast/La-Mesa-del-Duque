# Plan de Auditoría — La Mesa del Duque

## 1. Propósito

Este documento define el plan de auditoría interna del sistema **La Mesa del Duque**, alineado con los principios de auditoría de sistemas de gestión de calidad (ISO 19011) y seguridad de la información (ISO 27001). El plan establece los objetivos, criterios, alcance, cronograma y responsables de las actividades de auditoría.

## 2. Objetivos de la auditoría

1. Verificar que el sistema cumple con los requisitos funcionales definidos en las historias de usuario.
2. Verificar que los procesos de calidad (QA/QC) se ejecutan según el plan de calidad.
3. Verificar que los controles de seguridad documentados en la declaración de aplicabilidad ISO 27001 están implementados y son efectivos.
4. Identificar no conformidades, observaciones y oportunidades de mejora.
5. Proporcionar evidencia objetiva para la mejora continua del proceso de desarrollo.

## 3. Alcance

La auditoría cubre:

- **Código fuente**: Sprint 1 completo (HU-001, 002, 003, 011, 014, 016, 021, 025).
- **Documentación**: Todos los artefactos en `docs/calidad/`, `docs/seguridad/`, `docs/pruebas/`, `docs/requisitos/`.
- **Procesos**: Definición de Hecho, revisión de código, ejecución de pruebas, gestión de riesgos, seguridad.
- **Evidencia**: Pruebas unitarias, resultados de regresión, checklists completados, trazabilidad.

### Exclusiones

- Infraestructura de Supabase (responsabilidad del proveedor).
- Código de sprints futuros.

## 4. Criterios de auditoría

| Fuente                                      | Criterio                                          |
|---------------------------------------------|---------------------------------------------------|
| `docs/calidad/plan-calidad.md`              | Procesos de calidad definidos y ejecutados.       |
| `docs/calidad/definicion-de-hecho.md`       | Criterios DoD cumplidos para cada HU.             |
| `docs/calidad/checklist-revision-codigo.md` | Revisiones completadas con evidencia.             |
| `docs/calidad/checklist-seguridad.md`       | Controles de seguridad verificados.               |
| `docs/seguridad/declaracion-aplicabilidad-iso27001.md` | Controles implementados según SoA.     |
| `docs/pruebas/estrategia-pruebas.md`        | Pruebas ejecutadas, cobertura ≥ 80%.              |
| `docs/requisitos/criterios-aceptacion.md`   | Criterios de aceptación verificados.              |

## 5. Equipo auditor

| Rol              | Responsabilidad                                      |
|------------------|------------------------------------------------------|
| **Auditor líder**| Planificar, coordinar, redactar informe final.       |
| **Auditor**      | Ejecutar checklist, recopilar evidencia, registrar hallazgos. |
| **Auditado**     | Proveer acceso a código, pruebas y documentación.    |

*Nota: En el contexto académico, los roles de auditor pueden ser rotativos entre miembros del equipo.*

## 6. Cronograma de auditoría

| Fase                  | Actividad                                   | Fecha estimada   | Duración |
|-----------------------|---------------------------------------------|------------------|----------|
| **Planificación**     | Definir alcance, criterios y checklist.     | Semana 1         | 1 día    |
| **Preparación**       | Revisar documentación previa.               | Semana 1         | 1 día    |
| **Ejecución**         | Revisión de código y evidencia.             | Semana 2         | 2 días   |
| **Ejecución**         | Verificación de pruebas y cobertura.        | Semana 2         | 1 día    |
| **Ejecución**         | Verificación de controles de seguridad.     | Semana 2         | 1 día    |
| **Informe**           | Redactar hallazgos y no conformidades.      | Semana 3         | 1 día    |
| **Cierre**            | Reunión de cierre, presentación de resultados.| Semana 3       | 0.5 día  |

## 7. Checklist de auditoría

Ver `docs/auditoria/checklist-evidencia-auditoria.md` para la lista detallada de verificación.

## 8. Clasificación de hallazgos

| Tipo                 | Definición                                                       |
|----------------------|-------------------------------------------------------------------|
| **No conformidad mayor** | Incumplimiento de un requisito crítico que bloquea la liberación. |
| **No conformidad menor** | Incumplimiento de un requisito no crítico. Ajustable.           |
| **Observación**      | Situación que no incumple un requisito pero podría mejorar.       |
| **Oportunidad de mejora** | Sugerencia para optimizar un proceso o artefacto.            |

## 9. Informe de auditoría

El informe final incluirá:

- Resumen ejecutivo.
- Lista de hallazgos con clasificación y evidencia.
- Estado de no conformidades previas (si las hay).
- Conclusiones y recomendaciones.
- Anexos: checklists completados, capturas de evidencia.

## 10. Seguimiento

Las no conformidades requieren un plan de acción correctiva en un plazo máximo de 5 días hábiles tras la notificación. El auditor líder verifica el cierre de cada hallazgo antes de la auditoría del siguiente sprint.

---

**Versión**: 1.0 | **Fecha**: Abril 2026 | **Responsable**: Auditor líder
