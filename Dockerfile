# ================ Build Stage ================ #

# Use .NET SDK image to build the application and migration bundle
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Set the working directory inside the build stage
WORKDIR /src

# Copy project files first to optimize Docker build caching
COPY ["Medical.PL/Medical.PL.csproj", "Medical.PL/"]
COPY ["Medical.BLL/Medical.BLL.csproj", "Medical.BLL/"]
COPY ["Medical.DAL/Medical.DAL.csproj", "Medical.DAL/"]

# Restore NuGet dependencies for the startup project and its dependencies
RUN dotnet restore "Medical.PL/Medical.PL.csproj"

# Copy the remaining source code into the build stage
COPY . .

# Move to the presentation project directory
WORKDIR /src/Medical.PL

# Publish the application in Release configuration
RUN dotnet publish "Medical.PL.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Return to the source root directory
WORKDIR /src

# Install the EF Core CLI tool used to generate the migration bundle
RUN dotnet tool install --global dotnet-ef --version 10.0.12

# Add globally installed .NET tools to the executable search path
ENV PATH="$PATH:/root/.dotnet/tools"

# Generate a self-contained executable containing the EF Core migrations
RUN dotnet ef migrations bundle \
    --project "Medical.DAL/Medical.DAL.csproj" \
    --startup-project "Medical.PL/Medical.PL.csproj" \
    --self-contained \
    -r linux-x64 \
    --no-build \
    -c Release \
    -o /app/efbundle

# ===== Build Stage Output ===== #
# - /app/publish/Medical.PL.dll (for runtime)
# - /app/publish/Medical.BLL.dll (for runtime)
# - /app/publish/Medical.DAL.dll (for runtime)
# - /app/efbundle (for migration)




# ================ Migration Stage ================ #

# Use a lightweight Debian image to run database migrations
FROM debian:stable-slim AS migration

# Install system dependencies required by the migration bundle
RUN apt-get update && apt-get install -y \
    --no-install-recommends \
    ca-certificates \
    libicu-dev \
    && rm -rf /var/lib/apt/lists/*

# Set the working directory inside the migration image
WORKDIR /app

# Copy the generated migration executable from the build stage
COPY --from=build /app/efbundle .

# Execute the migration bundle when the container starts
ENTRYPOINT ["/app/efbundle"]

# ===== Migration Stage Output ===== #
# - Database Migration are applied




# ================ Runtime Stage ================ #

# Use the ASP.NET Core runtime image to run the web application
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# Set the working directory inside the runtime image
WORKDIR /app

# Copy published application files from the build stage
COPY --from=build /app/publish .

# Document that the application listens on port 7110
EXPOSE 7110

# Start the ASP.NET Core application
ENTRYPOINT ["dotnet", "Medical.PL.dll"]

# ===== Runtime Stage Output ===== #
# - Runs Medical.PL web application
