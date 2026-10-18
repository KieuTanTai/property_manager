# Build context is Backend/ so that the referenced ../../Shared/Shared.csproj is available.
# Build with: docker compose -f Backend/Module/Payment/compose.yaml build
#         or: docker build -f Backend/Module/Payment/Dockerfile Backend

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Restore is split from the copy so it stays cached while only source changes.
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["Module/Payment/Payment.csproj", "Module/Payment/"]
RUN dotnet restore "Module/Payment/Payment.csproj"

COPY Shared/ Shared/
COPY Module/Payment/ Module/Payment/
RUN dotnet publish "Module/Payment/Payment.csproj" \
        -c $BUILD_CONFIGURATION \
        -o /app/publish \
        /p:UseAppHost=false

# Dev-only self-signed certificate so the container serves HTTPS like the local "https" profile.
# [RequireHttps] would otherwise reject every client-facing endpoint over plain http.
# Replace it with a real certificate in production (mount one and set the Kestrel env vars).
RUN mkdir -p /https \
    && dotnet dev-certs https -ep /https/payment.pfx -p paymentdev \
    && chmod 644 /https/payment.pfx

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
COPY --from=build /https/payment.pfx /https/payment.pfx

ENTRYPOINT ["dotnet", "Payment.dll"]
