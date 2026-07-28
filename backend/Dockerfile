ARG DOTNET_SDK_IMAGE
ARG DOTNET_ASPNET_IMAGE

FROM ${DOTNET_SDK_IMAGE} AS build

WORKDIR /src

COPY src/Api/Api.csproj src/Api/
RUN dotnet restore src/Api/Api.csproj

COPY . .
RUN dotnet publish src/Api/Api.csproj --configuration Release --output /app/publish --no-restore /p:UseAppHost=false

FROM ${DOTNET_ASPNET_IMAGE} AS runtime

ARG BACKEND_CONTAINER_PORT

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE ${BACKEND_CONTAINER_PORT}

USER app

ENTRYPOINT ["dotnet", "Api.dll"]

