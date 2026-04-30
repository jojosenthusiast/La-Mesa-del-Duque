# Registro de cambios

Todas las modificaciones notables de este proyecto se documentarán en este archivo.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/),
y este proyecto adhiere al [Versionado Semántico](https://semver.org/lang/es/).

## [0.1.0] — 2026-04-30

### Agregado
- Configuración inicial del repositorio con arquitectura en capas (Dominio, Infraestructura, Web).
- Pruebas unitarias con xUnit y recolección de cobertura con Coverlet.
- Pipeline de integración continua (`ci.yml`) con compilación, pruebas, verificación de paquetes vulnerables y gobernanza de documentación.
- Pipeline de seguridad (`security.yml`) con análisis CodeQL, detección de secretos con Gitleaks y verificación de paquetes vulnerables.
- Pipeline de generación de releases (`release.yml`) con Release Please.
- Plantilla de Pull Request en español con checklist de calidad, seguridad, pruebas, trazabilidad e impacto.
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
