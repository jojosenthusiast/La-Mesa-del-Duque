# Declaración de Aplicabilidad ISO/IEC 27001:2022 — La Mesa del Duque

## 1. Propósito

Este documento constituye la Declaración de Aplicabilidad (Statement of Applicability — SoA) del Sistema de Gestión de Seguridad de la Información (SGSI) para **La Mesa del Duque**, conforme al anexo A de la norma ISO/IEC 27001:2022. Para cada control del Anexo A, se indica si es aplicable, se justifica su inclusión o exclusión, y se describe su implementación en el sistema.

## 2. Estructura del documento

| Columna         | Descripción                                                    |
|-----------------|----------------------------------------------------------------|
| ID              | Identificador del control según Anexo A ISO 27001:2022.        |
| Control         | Nombre del control.                                            |
| Aplicable       | Sí / No.                                                       |
| Justificación   | Razón de aplicabilidad o exclusión.                            |
| Implementación  | Cómo se implementa en La Mesa del Duque.                       |
| Evidencia       | Dónde encontrar evidencia del cumplimiento.                    |

## 3. Controles aplicables

### A.5 — Controles organizacionales

| ID       | Control                                    | Aplicable | Justificación / Implementación / Evidencia |
|----------|--------------------------------------------|-----------|--------------------------------------------|
| A.5.1    | Políticas de seguridad de la información   | Sí        | El plan de calidad y este documento constituyen la política. Evidencia: `docs/calidad/plan-calidad.md`, `docs/seguridad/alcance-sgsi-y-riesgos.md`. |
| A.5.2    | Funciones y responsabilidades              | Sí        | Roles definidos en el plan de calidad (Desarrollador, QA, Responsable de seguridad). Evidencia: `docs/calidad/plan-calidad.md` §5. |
| A.5.9    | Inventario de información y activos        | Sí        | Activos listados en `docs/seguridad/alcance-sgsi-y-riesgos.md` §3. |
| A.5.10   | Uso aceptable de la información            | Sí        | Acceso basado en roles (RBAC). Cada usuario ve solo lo autorizado. Evidencia: CA-025-04, CA-025-05. |
| A.5.14   | Transferencia de información               | Sí        | Toda comunicación usa HTTPS (HSTS) y wss:// para SignalR. Evidencia: `Program.cs`, checklist seguridad §3.6. |
| A.5.15   | Control de acceso                          | Sí        | RBAC con `[Authorize(Roles = "...")]`. Evidencia: CA-025, checklist seguridad §3.2. |
| A.5.17   | Información de autenticación               | Sí        | Contraseñas con BCrypt, mínimo 8 caracteres, bloqueo a los 5 intentos. Evidencia: `src/LaMesaDelDuque.Web/Program.cs` (BCrypt.Net-Next), CA-025-02/03. |
| A.5.18   | Derechos de acceso                         | Sí        | Principio de menor privilegio: roles con acceso mínimo. Evidencia: checklist seguridad §3.2, ítem 10. |
| A.5.24   | Planificación de la continuidad            | No        | No aplica en este alcance académico. El sistema no tiene requisitos de alta disponibilidad 24/7. |
| A.5.33   | Protección de registros                    | Sí        | Logs de seguridad (inicios de sesión, cambios de rol). Evidencia: checklist seguridad §3.8. |
| A.5.36   | Cumplimiento con políticas y estándares     | Sí        | Alineación con ISO 27001, checklist de seguridad, auditoría interna. Evidencia: este documento, `docs/auditoria/plan-auditoria.md`. |

### A.8 — Controles tecnológicos

| ID       | Control                                    | Aplicable | Justificación / Implementación / Evidencia |
|----------|--------------------------------------------|-----------|--------------------------------------------|
| A.8.5    | Autenticación segura                       | Sí        | Cookies HttpOnly, Secure, SameSite=Strict. Evidencia: CA-025-01/07. |
| A.8.7    | Protección contra código malicioso         | Sí        | Validación de entradas en servidor (Data Annotations). Evidencia: checklist seguridad §3.4. |
| A.8.8    | Gestión de vulnerabilidades técnicas       | Sí        | Revisión de dependencias NuGet con `dotnet list package --vulnerable`. Evidencia: checklist seguridad §3.7. |
| A.8.9    | Gestión de la configuración                | Sí        | Secretos en `appsettings.Development.json` (en .gitignore). Evidencia: `.gitignore`, checklist seguridad §3.7. |
| A.8.10   | Eliminación de información                 | No        | No se gestiona ciclo de vida de datos en este alcance. |
| A.8.12   | Enmascaramiento de datos                   | No        | No se procesan datos de tarjetas de crédito ni datos sensibles que requieran enmascaramiento. |
| A.8.15   | Registro de actividad                      | Sí        | Logs de eventos de seguridad (login fallido, cambio de rol). Evidencia: checklist seguridad §3.8. |
| A.8.16   | Monitoreo de actividades                   | Sí        | Mecanismo de detección de múltiples fallos de autenticación. Evidencia: CA-025-03. |
| A.8.20   | Seguridad de redes                         | Sí        | HTTPS obligatorio, HSTS, CORS restrictivo. Evidencia: `Program.cs`, checklist seguridad §3.6. |
| A.8.22   | Filtrado                                   | No        | No se implementa filtrado de tráfico a nivel de red en este alcance. |
| A.8.23   | Criptografía                               | Sí        | BCrypt para contraseñas. Evidencia: `LaMesaDelDuque.Web.csproj` (BCrypt.Net-Next). |
| A.8.24   | Ciclo de vida de desarrollo seguro         | Sí        | Checklist de seguridad por PR, pruebas de integración de seguridad, RBAC y CSRF en pruebas. Evidencia: `docs/calidad/checklist-seguridad.md`, `docs/pruebas/estrategia-pruebas.md` §2.4. |
| A.8.25   | Seguridad en el desarrollo                 | Sí        | Validación de entradas, parámetros en consultas (EF Core/LINQ). Evidencia: checklist revisión código §3.5. |
| A.8.31   | Separación de desarrollo y producción       | Sí        | Entornos separados: `appsettings.Development.json` vs producción. Evidencia: configuración del proyecto. |

## 4. Resumen

| Categoría        | Controles totales (Anexo A) | Aplicables | No aplicables |
|------------------|-----------------------------|------------|---------------|
| A.5 — Organizacionales | 37 (se listan los relevantes) | 10       | 1             |
| A.8 — Tecnológicos     | 34 (se listan los relevantes) | 11       | 4             |
| **Total evaluados**    | **25**                       | **21**    | **5**         |

Nota: Solo se evalúan los controles pertinentes al alcance del proyecto. Los no listados no aplican por el tamaño y naturaleza del sistema.

## 5. Aprobación

| Rol                          | Nombre | Fecha      | Firma |
|------------------------------|--------|------------|-------|
| Responsable de seguridad      |        |            |       |
| Arquitecto de software        |        |            |       |
| Líder de proyecto             |        |            |       |

---

**Versión**: 1.0 | **Fecha**: Abril 2026 | **Documento controlado**
