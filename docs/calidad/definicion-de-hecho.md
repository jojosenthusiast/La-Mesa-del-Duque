# Definición de Hecho (Definition of Done)

## 1. Propósito

Este documento define las condiciones que debe cumplir toda historia de usuario (HU) o tarea del sistema **La Mesa del Duque** para ser considerada **terminada** (*Done*). La definición de hecho garantiza que cada incremento entregado cumple con los estándares de calidad, seguridad, pruebas y documentación establecidos en el plan de calidad.

## 2. Criterios obligatorios

Toda historia de usuario debe satisfacer **todos** los criterios siguientes antes de ser marcada como terminada:

### 2.1 Implementación

- [ ] El código fuente está implementado en la rama `feature/*` correspondiente.
- [ ] El código compila sin errores ni advertencias (`dotnet build` exitoso).
- [ ] Se respetan las convenciones de nomenclatura del proyecto (español para dominio, inglés para patrones técnicos).
- [ ] Los principios SOLID y la arquitectura en capas se mantienen (Dominio no depende de Infraestructura ni Web).

### 2.2 Criterios de aceptación

- [ ] Todos los criterios de aceptación definidos en `docs/requisitos/criterios-aceptacion.md` para la HU están verificados.
- [ ] Los criterios se verifican con pruebas automatizadas (xUnit) y/o pruebas manuales documentadas.
- [ ] Se ha validado la interfaz de usuario (si aplica) en los navegadores objetivo (Chrome, Firefox, Edge).

### 2.3 Pruebas

- [ ] Las pruebas unitarias asociadas están escritas y pasan (`dotnet test` exitoso).
- [ ] Se cubren los flujos principales, alternativos y casos límite.
- [ ] La cobertura de código de la HU no disminuye la cobertura global por debajo del 80%.
- [ ] Las pruebas de integración verifican la interacción con la base de datos (si la HU involucra persistencia).
- [ ] La suite de regresión se ha ejecutado y no presenta nuevas fallas.

### 2.4 Revisión de código

- [ ] El Pull Request ha sido creado y vinculado a la HU.
- [ ] Al menos un revisor independiente ha completado el checklist de revisión (`docs/calidad/checklist-revision-codigo.md`).
- [ ] Todos los comentarios del revisor han sido resueltos.
- [ ] No quedan conversaciones abiertas en el PR.

### 2.5 Seguridad

- [ ] Se ha completado el checklist de seguridad (`docs/calidad/checklist-seguridad.md`) para la HU.
- [ ] No se introducen vulnerabilidades nuevas (verificación manual y con herramientas automatizadas).
- [ ] Las entradas de usuario se validan en el servidor (no solo en el cliente).
- [ ] Las consultas a base de datos usan parámetros (EF Core / LINQ, sin SQL crudo concatenado).
- [ ] Los tokens CSRF están presentes en todos los formularios que modifican estado.

### 2.6 Documentación

- [ ] La matriz de trazabilidad (`docs/calidad/matriz-trazabilidad.md`) está actualizada con la HU y sus artefactos.
- [ ] Si la HU introduce una decisión arquitectónica, se ha creado o actualizado un ADR en `docs/arquitectura/adr/`.
- [ ] Los comentarios en el código explican el *por qué*, no el *qué* (el código debe ser autoexplicativo).
- [ ] La historia de usuario en `docs/requisitos/historias-usuario.md` refleja el estado actual.
- [ ] Si el cambio constituye una versión relevante del producto, se ha creado o actualizado la documentación curada en `docs/releases/` con resumen ejecutivo, impacto (funcional y técnico), verificación, riesgos y próximos pasos.

### 2.7 Calidad

- [ ] No hay defectos abiertos de severidad **alta** o **crítica** relacionados con la HU.
- [ ] El registro de riesgos (`docs/calidad/registro-riesgos.md`) se ha revisado: los riesgos mitigados se marcan como tal.
- [ ] La matriz de impacto de cambios está actualizada si la HU modificó componentes existentes.

### 2.8 Integración

- [ ] La rama `feature/*` se ha fusionado con `develop` sin conflictos.
- [ ] El pipeline de CI (GitHub Actions) se ejecuta exitosamente sobre el merge.
- [ ] No se introducen regresiones en funcionalidades existentes.

## 3. Criterios adicionales por tipo de historia

| Tipo de historia     | Criterios extra                                                                |
|----------------------|--------------------------------------------------------------------------------|
| Interfaz de usuario  | Verificado en resolución 1366×768 y 1920×1080. Comportamiento responsivo.      |
| Persistencia         | Las migraciones de EF Core se generan correctamente. Rollback probado.         |
| Seguridad            | Prueba de penetración manual básica. Revisión por responsable de seguridad.     |
| Tiempo real (SignalR)| Verificada la reconexión automática ante caída de red.                          |

## 4. Proceso de verificación

1. El desarrollador completa los criterios 2.1 a 2.8 y los marca en esta lista.
2. El revisor de código verifica los criterios 2.4 y 2.5.
3. QA verifica los criterios 2.2, 2.3 y 2.8.
4. El responsable de calidad (o líder técnico) da la aprobación final.
5. La HU se marca como `Done` y se actualiza la matriz de trazabilidad.

## 5. ¿Qué NO es "hecho"?

- Código sin pruebas unitarias asociadas.
- Código sin revisión de pares.
- Funcionalidad que no cumple todos los criterios de aceptación.
- Código con defectos conocidos de severidad alta o crítica.
- Código que rompe la compilación o el pipeline de CI.

---

## 6. Condiciones para revisión

Un cambio está listo para revisión cuando adjunta evidencia de calidad generada por CI o explica explícitamente por qué no aplica. En modo estricto, las validaciones de trazabilidad, regresión y cobertura son bloqueantes.

---

**Versión**: 1.0 | **Fecha**: Abril 2026 | **Responsable**: Equipo de calidad
