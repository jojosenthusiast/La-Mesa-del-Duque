# ADR 0001: Arquitectura en capas

## Estado

**Aceptado**

## Contexto

El sistema **La Mesa del Duque** requiere una arquitectura que separe claramente la lógica de negocio de los detalles de infraestructura y presentación. El proyecto es académico y evolucionará a lo largo de múltiples sprints, por lo que la mantenibilidad y la capacidad de prueba son críticas.

Se evaluaron dos alternativas principales:

1. **Arquitectura en capas (Layered Architecture):** Separación clásica en capas de Dominio, Infraestructura y Presentación. Simple de entender, probar y enseñar.
2. **Arquitectura hexagonal (Ports & Adapters):** Más flexible para reemplazar adaptadores externos, pero introduce mayor complejidad y abstracción.

## Decisión

Se adopta la **arquitectura en capas** con tres capas estrictas:

- **Dominio** (`LaMesaDelDuque.Dominio`): Contiene entidades, value objects, interfaces de repositorio, servicios de dominio y lógica de negocio. No tiene dependencias externas.
- **Infraestructura** (`LaMesaDelDuque.Infraestructura`): Implementa las interfaces del dominio usando EF Core y PostgreSQL. Depende solo del Dominio.
- **Web** (`LaMesaDelDuque.Web`): Contiene Razor Pages, SignalR Hubs, configuración de seguridad. Depende del Dominio y de Infraestructura (inyección de dependencias).

La regla de dependencia es unidireccional hacia el Dominio.

## Consecuencias

### Positivas

- Separación clara de responsabilidades: el dominio se puede probar de forma aislada sin base de datos ni servidor web.
- Curva de aprendizaje baja: la arquitectura en capas es familiar y ampliamente documentada.
- La capa de dominio puede evolucionar sin afectar la infraestructura o la web.
- Facilita la aplicación de principios SOLID.

### Negativas

- Cambios en el dominio pueden requerir cambios en cascada en las capas superiores (pero esto es intencional: refleja que el dominio cambió).
- No es tan flexible como la arquitectura hexagonal para intercambiar adaptadores (no es un requisito en este proyecto).
- Riesgo de que la lógica de negocio se filtre a capas superiores si no hay disciplina.

### Mitigaciones

- Revisiones de código con checklist específico de arquitectura (ver `docs/calidad/checklist-revision-codigo.md`, sección 3.2).
- Pruebas unitarias del dominio ejecutadas en cada PR.
- Documentación de la regla de dependencia en el README del repositorio.

## Alternativas consideradas

| Alternativa                | Razón del descarte                                      |
|----------------------------|---------------------------------------------------------|
| Arquitectura hexagonal     | Complejidad innecesaria para el alcance del proyecto.   |
| Monolito sin capas         | Dificulta pruebas y mantenimiento a largo plazo.        |
| Microservicios             | Excesivo para un solo equipo y un sistema de este tamaño.|

---

**Fecha**: Abril 2026 | **Decisores**: Arquitecto de software, equipo de desarrollo
