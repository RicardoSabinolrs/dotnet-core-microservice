# SabinoLabs

This repository is intended to be part of a reference architecture for microservices with .NET Core.

## Development

To start your application in the Debug configuration, simply run:

```bash
dotnet run --verbosity normal --project ./src/SabinoLabs/SabinoLabs.csproj
```

## Code style / formatting

To format the dotnet code, run

    dotnet format

## Testing

To launch your application's tests, run:

    dotnet test --verbosity normal

### Code quality

By Script :

1. Run Sonar in container : `docker-compose -f ./docker/sonar.yml up -d`

2. Wait container was up Run `SonarAnalysis.ps1` and go to http://localhost:9001

Manually :

1. Run Sonar in container : `docker-compose -f ./docker/sonar.yml up -d`

2. Install sonar scanner for .net :

`dotnet tool install --global dotnet-sonarscanner`

3. Run `` dotnet sonarscanner begin /d:sonar.login=admin /d:sonar.password=admin /k:"SabinoLabs" /d:sonar.host.url="http://localhost:9001" /s:"`pwd`/SonarQube.Analysis.xml" ``

4. Build your application : `dotnet build`

5. Publish sonar results : `dotnet sonarscanner end /d:sonar.login=admin /d:sonar.password=admin`

6. Go to http://localhost:9001

### Monitoring

1. Run container (uncomment chronograf and kapacitor if you would use it): `docker-compose -f ./docker/monitoring.yml up -d`

2. Go to http://localhost:3000 (or http://localhost:8888 if you use chronograf)

3. (Only for chronograf) Change influxdb connection string by `YourApp-influxdb`

4. (Only for chronograf) Change kapacitor connection string by `YourApp-kapacitor`

5. (Only for chronograf) You can now add dashboard (like docker), see your app log in Cronograf Log viewer and send alert with kapacitor

## Build a Docker image

You can also fully dockerize your application and all the services that it depends on. To achieve this, first build a docker image of your app by running:

```bash
docker build -f ./Dockerfile-Back -t sabinolabs .
```

Then run:

```bash
docker run -p 8080:80 sabinolabs
```

Or you can simply run :

```bash
docker-compose -f .\docker\app.yml build
```

And

```bash
docker-compose -f .\docker\app.yml up
```

[gatling]: http://gatling.io/
