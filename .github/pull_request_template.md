# Pull Request

## Descripción

<!-- Explica el propósito de este cambio. ¿Qué problema resuelve? ¿Qué funcionalidad agrega? -->

## Tipo de cambio

- [ ] Nueva funcionalidad (`feat:`)
- [ ] Corrección de defecto (`fix:`)
- [ ] Documentación (`docs:`)
- [ ] Refactorización (`refactor:`)
- [ ] Pruebas (`test:`)
- [ ] Configuración / CI-CD (`chore:`)
- [ ] Seguridad (`security:`)

## Checklist de calidad

### Implementación
- [ ] El código compila sin errores ni advertencias (`dotnet build` exitoso).
- [ ] Se respetan las convenciones de nomenclatura del proyecto.
- [ ] Los principios SOLID y la arquitectura en capas se mantienen.
- [ ] No se introdujeron dependencias innecesarias al proyecto.
- [ ] El código nuevo sigue la convención del proyecto: español para dominio, inglés para patrones técnicos.

### Pruebas
- [ ] Las pruebas unitarias asociadas están escritas y pasan (`dotnet test` exitoso).
- [ ] Se cubren los flujos principales, alternativos y casos límite.
- [ ] La suite de regresión (`tests/regresion/`) se ejecutó y no presenta nuevas fallas.
- [ ] Se evaluó el impacto sobre funcionalidades existentes (ver `tests/impacto-cambios/matriz-impacto.md`).
- [ ] La cobertura de código no disminuye por debajo del 80 %.

### Seguridad
- [ ] Se completó el checklist de seguridad (`docs/calidad/checklist-seguridad.md`).
- [ ] Las entradas de usuario se validan en el servidor (no solo en el cliente).
- [ ] Las consultas a base de datos usan parámetros (EF Core / LINQ, sin SQL crudo concatenado).
- [ ] Los tokens CSRF están presentes en todos los formularios que modifican estado.
- [ ] No se exponen credenciales ni secretos en el código fuente.
- [ ] Se verificó que `dotnet list package --vulnerable --include-transitive` no reporta nuevas vulnerabilidades.

### Trazabilidad
- [ ] La historia de usuario asociada está documentada en `docs/requisitos/historias-usuario.md`.
- [ ] Los criterios de aceptación en `docs/requisitos/criterios-aceptacion.md` están verificados.
- [ ] La matriz de trazabilidad (`docs/calidad/matriz-trazabilidad.md`) está actualizada.
- [ ] Si el cambio introduce una decisión arquitectónica, se creó o actualizó un ADR en `docs/arquitectura/adr/`.

### Documentación
- [ ] La documentación técnica relevante fue actualizada (README, arquitectura, etc.).
- [ ] Los comentarios en el código explican el *por qué*, no el *qué*.
- [ ] Se actualizó `CHANGELOG.md` con los cambios introducidos (si la rama es `main`).
- [ ] Si el cambio genera una versión relevante, se creó o actualizó la documentación curada en `docs/releases/`.

### Impacto del cambio
- [ ] Se documentó qué módulos o capas se ven afectados por este cambio.
- [ ] Se evaluó el riesgo de regresión sobre funcionalidades existentes.
- [ ] Si el cambio es rompedor (*breaking change*), se comunicó al equipo.
- [ ] Se revisó el registro de riesgos (`docs/calidad/registro-riesgos.md`) y se actualizó si corresponde.

## Cómo probar este cambio

<!-- Instrucciones paso a paso para que el revisor pueda verificar el cambio: -->
1.
2.
3.

## Capturas de pantalla

<!-- Si el cambio afecta la interfaz de usuario, incluye capturas antes/después. -->

## Referencias

<!-- Enlaza la historia de usuario, issue o documento relacionado: -->
- HU:
- Issue:
- ADR (si aplica):

## Evidencia de calidad

HU: 
Riesgo: 
Pruebas: 
Trazabilidad: docs/calidad/matriz-trazabilidad.md

---

> **Nota para el revisor**: Por favor, completa el checklist de revisión de código en `docs/calidad/checklist-revision-codigo.md` antes de aprobar este PR.
