FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine
WORKDIR /app

RUN dotnet tool install --global dotnet-sql-cache && dotnet tool install --global dotnet-ef
ENV ASPNETCORE_URLS=http://+:5000 
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet build && chmod +x ./entrypoint.sh
ENTRYPOINT ["./entrypoint.sh"]
