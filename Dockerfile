FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS build
WORKDIR /app
RUN apt-get update -yq && apt-get install -yq curl

COPY src/*/*.csproj ./src/
COPY Directory.Build.targets Directory.Build.targets
RUN for file in $(ls src/*.csproj); do mkdir -p ${file%.*} && mv $file ${file%.*}; done
RUN dotnet restore "src/SabinoLabs/SabinoLabs.csproj"

COPY . ./
WORKDIR src/SabinoLabs
RUN dotnet publish "SabinoLabs.csproj" -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/out .

ENV ASPNETCORE_ENVIRONMENT=Production

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs
ENV LC_ALL en_US.UTF-8
ENV LANG en_US.UTF-8

ENTRYPOINT ["dotnet", "sabino-labs.dll"]
