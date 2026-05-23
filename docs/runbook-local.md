# Runbook local - La Mesa del Duque

## Objetivo

Ejecutar la aplicación localmente sin depender de Supabase ni exponer credenciales.

## Desarrollo local

1. Verificar SDK:
   `dotnet --version`

2. Restaurar:
   `dotnet restore LaMesaDelDuque.slnx`

3. Ejecutar tests:
   `dotnet test LaMesaDelDuque.slnx --no-restore`

4. Levantar aplicación:
   `dotnet run --project src/LaMesaDelDuque.Web/LaMesaDelDuque.Web.csproj --urls http://localhost:5103 --no-launch-profile --environment Development`

## Base local

En Development, si `ConnectionStrings:DefaultConnection` está vacío, el sistema usa:

`Data Source=la-mesa-del-duque-dev.db`

## Producción / remoto

La cadena PostgreSQL debe venir desde secretos de entorno o configuración segura, nunca desde `appsettings.Development.json`.
