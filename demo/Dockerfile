FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build-env
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out && chmod +x ./entrypoint.sh

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine
WORKDIR /app
RUN dotnet tool install --global dotnet-sql-cache
ENV ASPNETCORE_URLS=http://+:5000 
COPY --from=build-env /app/out .
COPY --from=build-env  /app/entrypoint.sh .
ENTRYPOINT ["./entrypoint.sh"]
