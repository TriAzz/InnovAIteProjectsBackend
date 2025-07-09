# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build the project (not the solution)
COPY . ./
RUN dotnet publish innovaite-projects-dashboard.csproj -c Release -o out

# Use the official .NET 8 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose the port that the app runs on
EXPOSE 8080

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production
# Use PORT environment variable provided by Render or default to 8080
ENV PORT=8080
# Explicitly log to console
ENV Logging__Console__LogLevel__Default=Information
ENV Logging__Console__LogLevel__Microsoft=Warning
ENV Logging__Console__LogLevel__System=Warning

# MongoDB connection string will be provided by Render environment variable
# If not running with proper environment variables, this will show a clear error
ENV MONGODB_CONNECTION_STRING_INFO="Using MongoDB connection from Render environment variables"

# Start the application
ENTRYPOINT ["dotnet", "innovaite-projects-dashboard.dll"]
