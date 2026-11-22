# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["Medical.PL/Medical.PL.csproj", "Medical.PL/"]
COPY ["Medical.BLL/Medical.BLL.csproj", "Medical.BLL/"]
COPY ["Medical.DAL/Medical.DAL.csproj", "Medical.DAL/"]

RUN dotnet restore "Medical.PL/Medical.PL.csproj"

COPY . .

WORKDIR /src/Medical.PL

RUN dotnet publish "Medical.PL.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore


# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 7110

ENTRYPOINT ["dotnet", "Medical.PL.dll"]
