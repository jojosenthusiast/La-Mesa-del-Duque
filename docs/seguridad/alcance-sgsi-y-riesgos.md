# Alcance del SGSI y Evaluación de Riesgos — La Mesa del Duque

## 1. Propósito

Este documento define el alcance del Sistema de Gestión de Seguridad de la Información (SGSI) para **La Mesa del Duque**, alineado con la norma ISO/IEC 27001:2022. Identifica los activos de información, las amenazas, las vulnerabilidades y los riesgos asociados, estableciendo las bases para la declaración de aplicabilidad y los controles de seguridad.

## 2. Alcance del SGSI

### 2.1 Límites del sistema

El SGSI cubre:

- **Aplicación web** La Mesa del Duque: código fuente, configuración y despliegue.
- **Base de datos** PostgreSQL en Supabase: esquema, datos de pedidos, productos, usuarios, roles y credenciales.
- **Infraestructura de desarrollo y CI/CD**: repositorio Git, pipeline de GitHub Actions.
- **Canales de comunicación**: HTTPS entre cliente y servidor; WebSockets seguros (wss://) para SignalR.
- **Autenticación y autorización**: cookies de sesión, RBAC, tokens CSRF.

### 2.2 Exclusiones

- Dispositivos físicos de los usuarios (PCs, tablets, redes Wi-Fi del restaurante): fuera del alcance del proyecto académico.
- Infraestructura física de Supabase: gestionada por el proveedor (responsabilidad compartida).
- Datos personales de clientes del restaurante (no se almacenan en el sistema por ahora).

## 3. Activos de información

| ID    | Activo                        | Tipo          | Criticidad |
|-------|-------------------------------|---------------|------------|
| A-01  | Código fuente del sistema     | Software      | Alta       |
| A-02  | Base de datos (PostgreSQL)    | Datos         | Crítica    |
| A-03  | Credenciales de usuarios      | Datos         | Crítica    |
| A-04  | Cookies de sesión             | Datos         | Alta       |
| A-05  | Cadenas de conexión y secretos| Configuración | Crítica    |
| A-06  | Repositorio Git (GitHub)      | Plataforma    | Alta       |
| A-07  | Pipeline de CI/CD             | Infraestructura| Media     |
| A-08  | Logs del sistema              | Datos         | Media      |

## 4. Evaluación de riesgos de seguridad

### 4.1 Metodología

Se utiliza un enfoque cualitativo basado en la identificación de amenazas, vulnerabilidades e impactos. Cada riesgo se evalúa en términos de:

- **Probabilidad**: Baja (1), Media (2), Alta (3).
- **Impacto**: Bajo (1), Medio (2), Alto (3).
- **Riesgo = Probabilidad × Impacto**.

### 4.2 Riesgos identificados

| ID      | Amenaza                                      | Vulnerabilidad                                  | Prob. | Impacto | Riesgo |
|---------|----------------------------------------------|-------------------------------------------------|-------|---------|--------|
| RS-01   | Acceso no autorizado por credenciales débiles | Contraseñas cortas o sin política de complejidad | 2     | 3       | 6 (Alto) |
| RS-02   | Suplantación de sesión (session hijacking)   | Cookie de sesión sin HttpOnly/Secure             | 2     | 3       | 6 (Alto) |
| RS-03   | Falsificación de solicitudes (CSRF)          | Formularios POST sin token anti-falsificación    | 2     | 3       | 6 (Alto) |
| RS-04   | Inyección SQL                                | Uso de SQL crudo concatenado o falta de parámetros| 1     | 3       | 3 (Medio)|
| RS-05   | Escalación de privilegios                    | RBAC mal configurado en páginas/endpoints        | 2     | 3       | 6 (Alto) |
| RS-06   | Exposición de secretos en repositorio        | Cadenas de conexión o claves en código fuente    | 2     | 3       | 6 (Alto) |
| RS-07   | Interceptación de datos (MITM)               | Comunicación sin HTTPS                           | 1     | 3       | 3 (Medio)|
| RS-08   | Denegación de servicio (DoS)                 | Sin límite de tasa en endpoints                  | 1     | 2       | 2 (Bajo) |
| RS-09   | Fuga de datos en logs                        | Contraseñas o tokens impresos en logs            | 2     | 2       | 4 (Medio)|
| RS-10   | Dependencias vulnerables                     | Paquetes NuGet con CVEs conocidos                | 2     | 2       | 4 (Medio)|

## 5. Controles propuestos

Cada riesgo identificado se aborda con uno o más controles del Anexo A de ISO 27001:2022, documentados en la declaración de aplicabilidad (`docs/seguridad/declaracion-aplicabilidad-iso27001.md`).

| Riesgo | Controles ISO 27001 aplicables          |
|--------|-----------------------------------------|
| RS-01  | A.5.15 Control de acceso, A.5.17 Información de autenticación |
| RS-02  | A.5.14 Transferencia de información, A.8.5 Autenticación segura |
| RS-03  | A.5.15 Control de acceso, A.8.20 Seguridad de redes |
| RS-04  | A.8.25 Ciclo de vida de desarrollo seguro |
| RS-05  | A.5.15 Control de acceso, A.5.18 Derechos de acceso |
| RS-06  | A.5.10 Política de seguridad de la información, A.5.14 Transferencia |
| RS-07  | A.8.24 Uso de criptografía, A.8.20 Seguridad de redes |
| RS-08  | A.8.20 Seguridad de redes, A.8.22 Filtrado |
| RS-09  | A.8.15 Registro de actividad, A.8.16 Monitoreo |
| RS-10  | A.8.8 Gestión de vulnerabilidades técnicas, A.8.25 Desarrollo seguro |

## 6. Matriz de responsabilidades

| Rol                          | Responsabilidad en el SGSI                            |
|------------------------------|-------------------------------------------------------|
| Responsable de seguridad      | Definir políticas, evaluar riesgos, aprobar controles.|
| Desarrollador                | Implementar controles técnicos, seguir checklist de seguridad. |
| Administrador de sistema     | Gestionar accesos, revisar logs, rotar secretos.      |
| Auditor                      | Verificar cumplimiento de controles, recopilar evidencia. |

---

**Versión**: 1.0 | **Fecha**: Abril 2026 | **Responsable**: Responsable de seguridad
