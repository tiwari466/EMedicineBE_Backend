# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["YourProjectName.csproj", "./"]
RUN dotnet restore "./YourProjectName.csproj"

# Copy the rest of the code and build
COPY . .
RUN dotnet publish "YourProjectName.csproj" -c Release -o /app/publish

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render expects the app to listen on port 10000 by default
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "YourProjectName.dll"]