# Suite de Regresión — La Mesa del Duque

Este directorio contiene la suite de pruebas de regresión del sistema.

## Estructura

```
tests/regresion/
├── README.md              ← Este archivo
├── CasosPrueba/           ← Casos de prueba automatizados (xUnit)
├── Manuales/              ← Guiones de pruebas manuales
└── resultados/            ← Reportes de ejecución por fecha
    └── YYYY-MM-DD-ejecucion-N.md
```

## Cómo ejecutar las pruebas de regresión

### Pruebas automatizadas

```bash
# Desde la raíz del repositorio
dotnet test --filter "Category=Regression"
```

### Pruebas manuales

Los guiones de pruebas manuales complementan las automatizadas, especialmente para:

- Flujos que involucran SignalR (notificaciones en tiempo real).
- Verificación visual de la interfaz de usuario.
- Pruebas de usabilidad y accesibilidad.

Cada guion manual se encuentra en `tests/regresion/Manuales/` y describe los pasos exactos a seguir y el resultado esperado.

## Catálogo de casos de regresión

El catálogo completo está documentado en `docs/calidad/suite-regresion.md`. Los casos están organizados por nivel:

| Nivel | Nombre        | Tiempo estimado | Frecuencia de ejecución     |
|-------|---------------|-----------------|-----------------------------|
| 1     | Smoke tests   | 5 min           | Cada commit en feature/*    |
| 2     | Regresión HU  | 15-20 min       | Cada Pull Request           |
| 3     | Completa      | 30-45 min       | Pre-release, merge a main   |

## Registro de resultados

Después de cada ejecución, crear un archivo en `resultados/` con el formato `YYYY-MM-DD-ejecucion-N.md` incluyendo:

```markdown
# Ejecución de regresión — DD/MM/YYYY

- **Rama**: feature/xxx
- **Commit**: abc123
- **Nivel ejecutado**: 1, 2, 3

## Resultados

| Total | Pasaron | Fallaron | % éxito |
|-------|---------|----------|---------|
| X     | X       | X        | X%      |

## Fallos

| ID caso | Descripción | Error | Acción |
|---------|-------------|-------|--------|
|         |             |       |        |
```

## Integración con CI

La suite de regresión se ejecuta automáticamente en GitHub Actions:

- **Nivel 1**: En cada push a cualquier rama.
- **Nivel 1 + 2**: En cada Pull Request hacia `develop` o `main`.
- **Nivel 1 + 2 + 3**: Manualmente antes de crear una release.

---

**Versión**: 1.0 | **Fecha**: Abril 2026
