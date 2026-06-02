#Stage 1: Compilation
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY . .

#Compile the application
RUN dotnet publish "Coontrera.Api/Coontrera.Api.csproj" -c Release -o /app/out

#Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

#Port
EXPOSE 5149
ENV ASPNETCORE_URLS=http://+:5149

#DDL
ENTRYPOINT ["dotnet", "Coontrera.Api.dll"]

#Acess
#http://localhost:5000/metrics for Prometheus