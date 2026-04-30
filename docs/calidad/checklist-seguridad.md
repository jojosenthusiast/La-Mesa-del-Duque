# Checklist de Seguridad — La Mesa del Duque

## 1. Propósito

Este checklist garantiza que cada historia de usuario o cambio significativo en el sistema **La Mesa del Duque** cumple con los requisitos de seguridad establecidos en el SGSI alineado con ISO/IEC 27001. Debe ser completado por el responsable de seguridad o por el desarrollador (con revisión posterior) antes de que la HU sea marcada como *Done*.

## 2. Instrucciones

1. Completar este checklist por cada Pull Request que introduzca funcionalidad nueva o modifique funcionalidad existente.
2. Marcar cada ítem como **✓ Cumple**, **✗ No cumple**, o **N/A** (No aplica).
3. Si algún ítem marcado **✗ No cumple** es de severidad alta, el PR no puede fusionarse hasta que se corrija.
4. Adjuntar este checklist como comentario en el PR.

## 3. Checklist de verificación

### 3.1 Autenticación y gestión de sesiones

| #  | Ítem                                                                 | Cumple |
|----|-----------------------------------------------------------------------|--------|
| 1  | El inicio de sesión usa hash seguro de contraseñas (BCrypt).          | ☐ |
| 2  | Las contraseñas tienen una longitud mínima de 8 caracteres.           | ☐ |
| 3  | Las cuentas se bloquean tras 5 intentos fallidos consecutivos.        | ☐ |
| 4  | La cookie de sesión tiene las banderas `HttpOnly`, `Secure` y `SameSite=Strict`. | ☐ |
| 5  | El cierre de sesión invalida la sesión en el servidor.                | ☐ |
| 6  | No se expone información de usuarios en mensajes de error (ej. "Usuario no encontrado" vs "Credenciales inválidas"). | ☐ |

### 3.2 Control de acceso (RBAC)

| #  | Ítem                                                                 | Cumple |
|----|-----------------------------------------------------------------------|--------|
| 7  | Las páginas que requieren autenticación tienen el atributo `[Authorize]`. | ☐ |
| 8  | Las páginas restringidas por rol verifican `[Authorize(Roles = "...")]`. | ☐ |
| 9  | Los controladores/endpoints de API verifican autorización antes de ejecutar la acción. | ☐ |
| 10 | El principio de menor privilegio se aplica: los roles tienen acceso mínimo necesario. | ☐ |
| 11 | No es posible escalar privilegios modificando parámetros de la URL o del formulario. | ☐ |

### 3.3 Protección CSRF

| #  | Ítem                                                                 | Cumple |
|----|-----------------------------------------------------------------------|--------|
| 12 | Todos los formularios que modifican estado (POST, PUT, DELETE) incluyen token anti-falsificación. | ☐ |
| 13 | El token CSRF se valida en el servidor en cada solicitud de modificación. | ☐ |
| 14 | Las solicitudes AJAX/fetch incluyen el token CSRF en el encabezado.   | ☐ |

### 3.4 Validación de entradas

| #  | Ítem                                                                 | Cumple |
|----|-----------------------------------------------------------------------|--------|
| 15 | Toda entrada de usuario se valida en el servidor (no solo en el cliente). | ☐ |
| 16 | Las validaciones usan Data Annotations o FluentValidation.            | ☐ |
| 17 | Los campos de texto libre se sanitizan contra inyección XSS (Razor lo hace por defecto; verificar escapes manuales con `@Html.Raw()`). | ☐ |
| 18 | Los campos numéricos validan rango y tipo.                            | ☐ |
| 19 | Las cadenas de conexión, rutas o parámetros críticos no provienen de entrada de usuario sin validar. | ☐ |

### 3.5 Protección de datos

| #  | Ítem                                                                 | Cumple |
|----|-----------------------------------------------------------------------|--------|
| 20 | No se almacenan contraseñas en texto plano (ni en BD, ni en logs, ni en archivos). | ☐ |
| 21 | Los datos sensibles (NIT, direcciones) se tratan con cuidado en logs. | ☐ |
| 22 | La conexión a la base de datos usa SSL/TLS.                           | ☐ |
| 23 | Los respaldos de base de datos están protegidos (encriptados si es posible). | ☐ |

### 3.6 Comunicaciones

| #  | Ítem                                                                 | Cumple |
|----|-----------------------------------------------------------------------|--------|
| 24 | Toda comunicación con el servidor usa HTTPS (HSTS habilitado).        | ☐ |
| 25 | Las conexiones SignalR usan WebSockets seguros (wss://).              | ☐ |
| 26 | No se transmiten datos sensibles en parámetros de URL (query string). | ☐ |

### 3.7 Dependencias y configuración

| #  | Ítem                                                                 | Cumple |
|----|-----------------------------------------------------------------------|--------|
| 27 | Las dependencias de NuGet se revisan periódicamente en busca de vulnerabilidades conocidas (`dotnet list package --vulnerable`). | ☐ |
| 28 | Las cadenas de conexión y secretos están en `appsettings.Development.json` o variables de entorno, no en el código fuente. | ☐ |
| 29 | Los archivos de configuración con secretos están en `.gitignore`.     | ☐ |
| 30 | La configuración de CORS es restrictiva (si aplica).                  | ☐ |

### 3.8 Registro y monitoreo

| #  | Ítem                                                                 | Cumple |
|----|-----------------------------------------------------------------------|--------|
| 31 | Los eventos de seguridad (inicios de sesión fallidos, cambios de rol, eliminación de datos) se registran en logs. | ☐ |
| 32 | Los logs no contienen contraseñas ni tokens de autenticación.         | ☐ |
| 33 | Existe un mecanismo para detectar actividades sospechosas (múltiples fallos de autenticación). | ☐ |

## 4. Resumen de verificación de seguridad

| Campo              | Valor                        |
|--------------------|------------------------------|
| **Verificador**    |                              |
| **Fecha**          |                              |
| **PR / HU**        |                              |
| **Total ítems**    |                              |
| **Cumplen**        |                              |
| **No cumplen**     |                              |
| **No aplican**     |                              |

### Hallazgos de seguridad

_(Describir cualquier hallazgo que requiera acción correctiva, con su severidad: Baja / Media / Alta / Crítica.)_

| # | Hallazgo | Severidad | Acción requerida | Responsable |
|---|----------|-----------|------------------|-------------|
| 1 |          |           |                  |             |

---

**Versión**: 1.0 | **Fecha**: Abril 2026
