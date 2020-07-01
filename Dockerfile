#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM microsoft/aspnetcore-build:1.1.2 as base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY TemplateAPI/TemplateAPI.csproj TemplateAPI/
RUN dotnet restore "TemplateAPI/TemplateAPI.csproj"
COPY . .
WORKDIR "/src/TemplateAPI"
RUN dotnet build "TemplateAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TemplateAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TemplateAPI.dll"]
