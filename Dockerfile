# ============================================================================
# La Mesa del Duque — Dockerfile multi-stage
# ============================================================================
# Imagen final: ~120 MB (runtime Alpine)
# Puertos: 8080 (HTTP)
# Variables de entorno:
#   ASPNETCORE_ENVIRONMENT=Production
#   LMD_CONNECTION_STRING=postgresql://... (opcional, sino usa SQLite)
# ============================================================================

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Restaurar dependencias (aprovecha cache de capas)
COPY *.slnx ./
COPY src/LaMesaDelDuque.Dominio/*.csproj src/LaMesaDelDuque.Dominio/
COPY src/LaMesaDelDuque.Aplicacion/*.csproj src/LaMesaDelDuque.Aplicacion/
COPY src/LaMesaDelDuque.Infraestructura/*.csproj src/LaMesaDelDuque.Infraestructura/
COPY src/LaMesaDelDuque.Web/*.csproj src/LaMesaDelDuque.Web/
COPY tests/LaMesaDelDuque.Pruebas/*.csproj tests/LaMesaDelDuque.Pruebas/
RUN dotnet restore "src/LaMesaDelDuque.Web/LaMesaDelDuque.Web.csproj"

# Compilar
COPY . .
RUN dotnet publish "src/LaMesaDelDuque.Web/LaMesaDelDuque.Web.csproj" \
    -c Release -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Imagen final mínima
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
EXPOSE 8080

# Crear directorio para SQLite (desarrollo)
RUN mkdir -p /app/data && chown -R app:app /app/data

# Copiar binarios publicados
COPY --from=build /app/publish .

# Configuración por defecto: SQLite en /app/data
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV LMD_CONNECTION_STRING=

USER app
ENTRYPOINT ["dotnet", "LaMesaDelDuque.Web.dll"]
