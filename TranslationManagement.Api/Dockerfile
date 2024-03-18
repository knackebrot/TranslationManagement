FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TranslationManagement.Domain/TranslationManagement.Domain.csproj", "TranslationManagement.Domain/"]
COPY ["TranslationManagement.Infrastructure/TranslationManagement.Infrastructure.csproj", "TranslationManagement.Infrastructure/"]
COPY ["TranslationManagement.Api/TranslationManagement.Api.csproj", "TranslationManagement.Api/"]
RUN dotnet restore "TranslationManagement.Domain/TranslationManagement.Domain.csproj"
RUN dotnet restore "TranslationManagement.Infrastructure/TranslationManagement.Infrastructure.csproj"
RUN dotnet restore "TranslationManagement.Api/TranslationManagement.Api.csproj"
COPY . .
WORKDIR "/src/TranslationManagement.Api"
RUN dotnet build "TranslationManagement.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TranslationManagement.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TranslationManagement.Api.dll"]