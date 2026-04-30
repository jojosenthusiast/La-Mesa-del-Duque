# Indicadores de Calidad — La Mesa del Duque

## 1. Propósito

Este documento define y monitorea los indicadores de calidad del sistema **La Mesa del Duque**, alineados con el modelo ISO/IEC 25010 (calidad del producto) e ISO/IEC 25002 (medición de la calidad). Los indicadores permiten evaluar objetivamente el estado del sistema en cada sprint y tomar decisiones informadas sobre la liberación de funcionalidades.

## 2. Estructura de cada indicador

Cada indicador se describe con los siguientes atributos:

| Atributo        | Descripción                                              |
|-----------------|----------------------------------------------------------|
| **ID**          | Identificador único.                                     |
| **Nombre**      | Nombre descriptivo del indicador.                        |
| **Característica ISO 25010** | Característica de calidad que mide.         |
| **Fórmula**     | Cómo se calcula.                                         |
| **Unidad**      | Unidad de medida (%, cantidad, horas, etc.).             |
| **Objetivo**    | Valor deseado.                                           |
| **Umbral de alerta** | Valor que dispara una acción correctiva.            |
| **Frecuencia**  | Cada cuánto se mide.                                     |
| **Fuente**      | De dónde se obtienen los datos.                          |

## 3. Indicadores

### IND-01: Densidad de defectos

| Atributo             | Valor                                                     |
|----------------------|-----------------------------------------------------------|
| **Característica**   | Fiabilidad — Madurez                                      |
| **Fórmula**          | `defectos_encontrados_en_el_sprint / total_historias_usuario` |
| **Unidad**           | Defectos por HU                                           |
| **Objetivo**         | < 2 defectos por HU                                       |
| **Umbral de alerta** | ≥ 3 defectos por HU                                       |
| **Frecuencia**       | Al cierre de cada sprint                                  |
| **Fuente**           | Registro de defectos en el repositorio o backlog          |

### IND-02: Cobertura de código

| Atributo             | Valor                                                     |
|----------------------|-----------------------------------------------------------|
| **Característica**   | Mantenibilidad — Modularidad, Analizabilidad               |
| **Fórmula**          | `(líneas_cubiertas_por_pruebas / total_líneas) × 100`     |
| **Unidad**           | Porcentaje (%)                                            |
| **Objetivo**         | ≥ 80% global                                              |
| **Umbral de alerta** | < 70%                                                     |
| **Frecuencia**       | Cada PR y al cierre de sprint                             |
| **Fuente**           | Coverlet (reporte XML/HTML) ejecutado en CI               |

### IND-03: Cobertura de requisitos por pruebas

| Atributo             | Valor                                                     |
|----------------------|-----------------------------------------------------------|
| **Característica**   | Funcionalidad — Completitud funcional                      |
| **Fórmula**          | `(criterios_aceptacion_con_pruebas / total_criterios) × 100` |
| **Unidad**           | Porcentaje (%)                                            |
| **Objetivo**         | 100%                                                      |
| **Umbral de alerta** | < 90%                                                     |
| **Frecuencia**       | Al cierre de cada sprint                                  |
| **Fuente**           | Matriz de trazabilidad (`docs/calidad/matriz-trazabilidad.md`) |

### IND-04: Tasa de aprobación en revisión de código

| Atributo             | Valor                                                     |
|----------------------|-----------------------------------------------------------|
| **Característica**   | Mantenibilidad — Analizabilidad                            |
| **Fórmula**          | `(PR_aprobados_en_primera_revision / total_PR) × 100`     |
| **Unidad**           | Porcentaje (%)                                            |
| **Objetivo**         | ≥ 90%                                                     |
| **Umbral de alerta** | < 70%                                                     |
| **Frecuencia**       | Semanal                                                   |
| **Fuente**           | GitHub (historial de PR)                                  |

### IND-05: Vulnerabilidades detectadas

| Atributo             | Valor                                                     |
|----------------------|-----------------------------------------------------------|
| **Característica**   | Seguridad — Confidencialidad, Integridad                   |
| **Fórmula**          | `vulnerabilidades_criticas + altas` (conteo)              |
| **Unidad**           | Cantidad                                                  |
| **Objetivo**         | 0 críticas, 0 altas                                       |
| **Umbral de alerta** | ≥ 1 vulnerabilidad alta o crítica                         |
| **Frecuencia**       | Cada PR (checklist) y semanal (análisis de dependencias)  |
| **Fuente**           | Checklist de seguridad, `dotnet list package --vulnerable` |

### IND-06: Cobertura de la suite de regresión

| Atributo             | Valor                                                     |
|----------------------|-----------------------------------------------------------|
| **Característica**   | Fiabilidad — Madurez                                      |
| **Fórmula**          | `(casos_regresion_ejecutados_exitosamente / total_casos_regresion) × 100` |
| **Unidad**           | Porcentaje (%)                                            |
| **Objetivo**         | 100%                                                      |
| **Umbral de alerta** | < 95%                                                     |
| **Frecuencia**       | Cada PR y pre-release                                     |
| **Fuente**           | `tests/regresion/resultados/`                             |

### IND-07: Tiempo de corrección de defectos

| Atributo             | Valor                                                     |
|----------------------|-----------------------------------------------------------|
| **Característica**   | Fiabilidad — Madurez                                      |
| **Fórmula**          | `Σ(fecha_correccion - fecha_deteccion) / total_defectos`  |
| **Unidad**           | Días                                                      |
| **Objetivo**         | ≤ 2 días para defectos altos; ≤ 5 días para medios        |
| **Umbral de alerta** | > 3 días para altos                                       |
| **Frecuencia**       | Al cierre de cada sprint                                  |
| **Fuente**           | Registro de defectos                                      |

### IND-08: Cumplimiento de la definición de hecho

| Atributo             | Valor                                                     |
|----------------------|-----------------------------------------------------------|
| **Característica**   | Funcionalidad — Completitud                                |
| **Fórmula**          | `(HU_que_cumplen_DoD / total_HU_entregadas) × 100`        |
| **Unidad**           | Porcentaje (%)                                            |
| **Objetivo**         | 100%                                                      |
| **Umbral de alerta** | < 90%                                                     |
| **Frecuencia**       | Al cierre de cada sprint                                  |
| **Fuente**           | Checklist DoD por HU                                      |

## 4. Tablero de indicadores — Sprint 1

| ID     | Indicador                         | Objetivo | Valor actual | Estado |
|--------|-----------------------------------|----------|--------------|--------|
| IND-01 | Densidad de defectos              | < 2/HU   | Por medir    | ⬜     |
| IND-02 | Cobertura de código               | ≥ 80%    | Por medir    | ⬜     |
| IND-03 | Cobertura de requisitos           | 100%     | Por medir    | ⬜     |
| IND-04 | Tasa de aprobación en revisión    | ≥ 90%    | Por medir    | ⬜     |
| IND-05 | Vulnerabilidades detectadas       | 0 críticas | Por medir  | ⬜     |
| IND-06 | Cobertura de regresión            | 100%     | Por medir    | ⬜     |
| IND-07 | Tiempo de corrección de defectos  | ≤ 2 días | Por medir    | ⬜     |
| IND-08 | Cumplimiento DoD                  | 100%     | Por medir    | ⬜     |

**Leyenda**: 🟢 Cumple objetivo | 🟡 En umbral de alerta | 🔴 No cumple | ⬜ Por medir

## 5. Acciones correctivas

Cuando un indicador entra en **umbral de alerta** o no cumple el objetivo:

1. Registrar el evento en este documento (tabla de acciones correctivas).
2. Analizar la causa raíz.
3. Definir y ejecutar acción correctiva.
4. Medir nuevamente en la siguiente iteración.

| Fecha | Indicador | Valor real | Causa | Acción | Responsable | Resultado |
|-------|-----------|------------|-------|--------|-------------|-----------|
|       |           |            |       |        |             |           |

---

**Versión**: 1.0 | **Fecha**: Abril 2026 | **Actualizar al cierre de cada sprint**
