FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["expenses-tracker-pb.Server/expenses-tracker-pb.Server.csproj", "expenses-tracker-pb.Server/"]
RUN dotnet restore "expenses-tracker-pb.Server/expenses-tracker-pb.Server.csproj"

RUN curl -fsSL https://deb.nodesource.com/setup_16.x | bash -
RUN apt-get install -y nodejs

COPY . .

WORKDIR "/src/expenses-tracker-pb.Server"
RUN dotnet build "expenses-tracker-pb.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

WORKDIR /src/expenses-tracker-pb.client
RUN npm install
RUN npm run build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/expenses-tracker-pb.Server
RUN dotnet publish "expenses-tracker-pb.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /src/expenses-tracker-pb.client/dist ./wwwroot
ENTRYPOINT ["dotnet", "expenses-tracker-pb.Server.dll"]