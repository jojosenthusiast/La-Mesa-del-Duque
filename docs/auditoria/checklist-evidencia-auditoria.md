# Checklist de Evidencia de Auditoría — La Mesa del Duque

## 1. Propósito

Este checklist guía la recopilación de evidencia objetiva durante la auditoría interna del sistema **La Mesa del Duque**. Cada ítem debe verificarse con evidencia concreta (archivo, captura, resultado de prueba, registro) y registrarse con su estado.

## 2. Instrucciones

1. Para cada ítem, localizar la evidencia en el repositorio o en la herramienta correspondiente.
2. Registrar la ruta exacta o enlace a la evidencia.
3. Marcar el estado: **✓ Conforme**, **✗ No conforme**, **N/A**.
4. Si es **No conforme**, registrar un hallazgo con clasificación (Mayor / Menor / Observación).

## 3. Checklist

### 3.1 Documentación del plan de calidad

| #  | Evidencia requerida                                        | Ruta / Enlace                    | Estado |
|----|------------------------------------------------------------|----------------------------------|--------|
| 1  | Plan de calidad documentado y actualizado.                 | `docs/calidad/plan-calidad.md`   | ☐      |
| 2  | Definición de Hecho documentada.                           | `docs/calidad/definicion-de-hecho.md` | ☐ |
| 3  | Matriz de trazabilidad actualizada con todas las HU.       | `docs/calidad/matriz-trazabilidad.md` | ☐ |
| 4  | Registro de riesgos con mitigaciones.                      | `docs/calidad/registro-riesgos.md` | ☐    |
| 5  | Checklist de revisión de código completado por cada PR.    | PRs en GitHub                    | ☐      |
| 6  | Checklist de seguridad completado por cada HU.             | `docs/calidad/checklist-seguridad.md` (o adjunto al PR) | ☐ |
| 7  | Matriz de impacto de cambios actualizada.                  | `docs/calidad/matriz-impacto-cambios.md` | ☐ |
| 8  | Suite de regresión definida.                               | `docs/calidad/suite-regresion.md`| ☐      |

### 3.2 Historias de usuario y criterios de aceptación

| #  | Evidencia requerida                                        | Ruta / Enlace                    | Estado |
|----|------------------------------------------------------------|----------------------------------|--------|
| 9  | Historias de usuario del Sprint 1 documentadas.            | `docs/requisitos/historias-usuario.md` | ☐ |
| 10 | Criterios de aceptación definidos para cada HU.            | `docs/requisitos/criterios-aceptacion.md` | ☐ |
| 11 | Cada HU tiene código implementado.                         | `src/`                           | ☐      |
| 12 | Cada HU tiene pruebas unitarias asociadas.                 | `tests/LaMesaDelDuque.Pruebas/`  | ☐      |

### 3.3 Pruebas

| #  | Evidencia requerida                                        | Ruta / Enlace                    | Estado |
|----|------------------------------------------------------------|----------------------------------|--------|
| 13 | Estrategia de pruebas documentada.                         | `docs/pruebas/estrategia-pruebas.md` | ☐  |
| 14 | `dotnet test` se ejecuta sin errores.                      | Salida del comando o CI log      | ☐      |
| 15 | Cobertura de código ≥ 80% (Coverlet).                      | Reporte de cobertura             | ☐      |
| 16 | Pruebas de integración documentadas y pasando.             | `tests/LaMesaDelDuque.Pruebas/`  | ☐      |
| 17 | Suite de regresión ejecutada exitosamente antes de merge.  | `tests/regresion/resultados/`    | ☐      |

### 3.4 Seguridad (ISO 27001)

| #  | Evidencia requerida                                        | Ruta / Enlace                    | Estado |
|----|------------------------------------------------------------|----------------------------------|--------|
| 18 | Alcance del SGSI documentado.                              | `docs/seguridad/alcance-sgsi-y-riesgos.md` | ☐ |
| 19 | Declaración de aplicabilidad ISO 27001 completada.         | `docs/seguridad/declaracion-aplicabilidad-iso27001.md` | ☐ |
| 20 | Contraseñas encriptadas con BCrypt (verificar en código).  | `src/LaMesaDelDuque.Web/`        | ☐      |
| 21 | Autorización RBAC implementada en páginas restringidas.    | `src/LaMesaDelDuque.Web/Pages/`  | ☐      |
| 22 | Tokens CSRF en formularios POST.                           | `src/LaMesaDelDuque.Web/Pages/`  | ☐      |
| 23 | Validación de entradas en el servidor.                     | PageModels (`.cshtml.cs`)        | ☐      |
| 24 | HTTPS forzoso (HSTS) en producción.                        | `Program.cs`                     | ☐      |
| 25 | Cookies de sesión con HttpOnly, Secure, SameSite=Strict.   | `Program.cs`                     | ☐      |
| 26 | Secretos fuera del repositorio (.gitignore verificado).    | `.gitignore`                     | ☐      |
| 27 | Sin SQL crudo concatenado (verificar repositorios).        | `src/LaMesaDelDuque.Infraestructura/` | ☐ |
| 28 | Revisión de dependencias vulnerables.                      | `dotnet list package --vulnerable` | ☐    |

### 3.5 Arquitectura

| #  | Evidencia requerida                                        | Ruta / Enlace                    | Estado |
|----|------------------------------------------------------------|----------------------------------|--------|
| 29 | Documento de arquitectura del sistema.                     | `docs/arquitectura/arquitectura-sistema.md` | ☐ |
| 30 | ADR 0001 — Arquitectura en capas.                          | `docs/arquitectura/adr/0001-arquitectura-en-capas.md` | ☐ |
| 31 | ADR 0002 — ASP.NET Razor Pages.                            | `docs/arquitectura/adr/0002-aspnet-razor-pages.md` | ☐ |
| 32 | ADR 0003 — PostgreSQL / Supabase.                          | `docs/arquitectura/adr/0003-postgresql-supabase.md` | ☐ |

### 3.6 Métricas

| #  | Evidencia requerida                                        | Ruta / Enlace                    | Estado |
|----|------------------------------------------------------------|----------------------------------|--------|
| 33 | Indicadores de calidad definidos.                          | `docs/metricas/indicadores-calidad.md` | ☐ |

### 3.7 Procesos de control de cambios

| #  | Evidencia requerida                                        | Ruta / Enlace                    | Estado |
|----|------------------------------------------------------------|----------------------------------|--------|
| 34 | Pull Requests con revisión de código.                      | GitHub                           | ☐      |
| 35 | Commits siguen convención (`feat:`, `fix:`, etc.).         | Historial de Git                 | ☐      |
| 36 | Ramas `feature/*` fusionadas vía PR a `develop`.           | GitHub                           | ☐      |

## 4. Resumen de hallazgos

| # | Hallazgo | Tipo (Mayor/Menor/Obs) | Evidencia | Acción correctiva | Responsable | Fecha cierre |
|---|----------|------------------------|-----------|-------------------|-------------|--------------|
| 1 |          |                        |           |                   |             |              |
| 2 |          |                        |           |                   |             |              |

## 5. Conclusión del auditor

_(Completar al finalizar la auditoría.)_

---

**Versión**: 1.0 | **Fecha**: Abril 2026 | **Auditor**: _______________
