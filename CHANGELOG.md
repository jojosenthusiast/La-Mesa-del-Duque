# Registro de cambios

Todas las modificaciones notables de este proyecto se documentarán en este archivo.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/),
y este proyecto adhiere al [Versionado Semántico](https://semver.org/lang/es/).

## [0.3.1](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.3.0...v0.3.1) (2026-05-02)

### Summary

- Establece un estándar profesional para documentar releases relevantes del producto, separando el changelog mecánico de Release Please de la documentación curada del proyecto.

### Details

- Agrega documentación curada para `v0.2.0` y `v0.3.0` en `docs/releases/`, con resumen ejecutivo, impacto funcional, impacto técnico, verificación, riesgos y próximos pasos.
- Actualiza la definición de hecho para exigir documentación curada cuando un cambio constituya una versión relevante del producto.
- Actualiza la plantilla de Pull Request para recordar la documentación de release antes de fusionar incrementos significativos.
- Define que Release Please se mantiene como automatización de versionado y changelog, mientras `docs/releases/` será la fuente de documentación profesional.
- Documentación detallada: [`docs/releases/v0.3.1-documentacion-profesional-releases.md`](docs/releases/v0.3.1-documentacion-profesional-releases.md).

## [0.3.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.2.0...v0.3.0) (2026-05-02)


### Nuevas funcionalidades

* **dominio:** implementar base del modelo de pedidos ([7680ad9](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/7680ad98eaf362f4d06e7aac907b3c85afdf39e9))
* **dominio:** implementar base del modelo de pedidos ([2510e69](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/2510e6979e0585b961bb65ac565942abecd0464c))

## [0.2.0](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.1.1...v0.2.0) (2026-05-01)


### Nuevas funcionalidades

* **marca:** integrar fundación visual ([db3cf42](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/db3cf42c54f5a5d534b4b5796a53d3b8c04c00a7))
* **marca:** integrar fundación visual ([b4b0986](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/b4b09861801f0c273ab349b1c15bd1612dcdd43d))

## [0.1.1](https://github.com/jojosenthusiast/La-Mesa-del-Duque/compare/v0.1.0...v0.1.1) (2026-05-01)


### Documentación

* simplificar redacción de documentación base ([d7ac072](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/d7ac07227dea47579214a7de0cff722213157b71))


### Tareas de mantenimiento

* base inicial del proyecto ([9bada27](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/9bada27c442d0ff57f04e7f3474a27e4de8af44a))
* crear base inicial del proyecto ([34ea98d](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/34ea98d5e3215c2d24b48e378c4411ca00df08c0))
* inicializar repositorio con README base ([f9aa116](https://github.com/jojosenthusiast/La-Mesa-del-Duque/commit/f9aa116932e5ba8dcaa49ffcf4892cbc44ec7b1d))

## [0.1.0] — 2026-04-30

### Agregado
- Configuración inicial del repositorio con arquitectura en capas (Dominio, Infraestructura, Web).
- Pruebas unitarias con xUnit y recolección de cobertura con Coverlet.
- Pipeline de integración continua (`ci.yml`) con compilación, pruebas, verificación de paquetes vulnerables y gobernanza de documentación.
- Pipeline de seguridad (`security.yml`) con análisis CodeQL, detección de secretos con Gitleaks y verificación de paquetes vulnerables.
- Pipeline de generación de releases (`release.yml`) con Release Please.
- Plantilla de Pull Request con checklist de calidad, seguridad, pruebas, trazabilidad e impacto.
- Documentación de calidad: plan de calidad, definición de hecho, matriz de trazabilidad, registro de riesgos, checklist de revisión de código y checklist de seguridad.
- Documentación de arquitectura y ADR (Architecture Decision Records).
- Documentación de requisitos: historias de usuario y criterios de aceptación.
- Documentación de seguridad: alcance SGSI y declaración de aplicabilidad ISO 27001.
- Documentación de pruebas: estrategia de pruebas, suite de regresión y matriz de impacto de cambios.
- Documentación de auditoría: plan de auditoría y checklist de evidencia.
- Métricas e indicadores de calidad.
- Archivo `.editorconfig` para C#, Razor, JSON, YAML y Markdown.
- Archivo `.gitignore` con exclusiones para secretos, artefactos de compilación y resultados de pruebas.

[0.1.0]: https://github.com/jojosenthusiast/La-Mesa-del-Duque/releases/tag/v0.1.0
