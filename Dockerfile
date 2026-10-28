# Multi-stage build for the Dharmacorpus website.
#   docker build -t dharmacorpus .
#   docker run -p 8080:8080 dharmacorpus

# 1) Front-end: Tailwind CSS, Flowbite, jQuery and fonts
FROM node:22-alpine AS assets
WORKDIR /src
COPY src/Dharmacorpus.Web/package.json src/Dharmacorpus.Web/package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY src/Dharmacorpus.Web/ ./
RUN npm run build

# 2) .NET publish (client build already done above)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Dharmacorpus.Web/ ./
COPY --from=assets /src/wwwroot ./wwwroot
RUN dotnet publish -c Release -o /app -p:SkipClientBuild=true

# 3) Runtime: small image, non-root user
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Dharmacorpus.Web.dll"]
