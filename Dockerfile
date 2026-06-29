FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY Core/Core.csproj Core/
COPY Backend/Backend.csproj Backend/
COPY Frontend/Frontend.csproj Frontend/

RUN dotnet restore Backend/Backend.csproj
RUN dotnet restore Frontend/Frontend.csproj

COPY . .

RUN dotnet publish Frontend/Frontend.csproj \
    --configuration Release \
    --output /app/frontend \
    --no-restore

RUN dotnet publish Backend/Backend.csproj \
    --configuration Release \
    --output /app/backend \
    --no-restore

RUN mkdir -p /app/backend/wwwroot \
    && cp -r /app/frontend/wwwroot/. /app/backend/wwwroot/

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

COPY --from=build /app/backend .

# Copy .env into the runtime container.
# This works because .env is NOT ignored by .dockerignore in this setup.
COPY .env .env

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

ENTRYPOINT ["dotnet", "Backend.dll"]
