# EndpointMapper.TestApplication

## Sample application for EndpointMapper

This is a sample application build with EndpointMapper for testing the 
library during development and providing a few examples of the Developer Experience (DX)

## Routes

/api is the prefix of all routes in this application

| Method |        Route         |                   File                   |
|:------:|:--------------------:|:----------------------------------------:|
|  GET   |      /api/auth       |   Endpoints/AuthenticationEndpoint.cs    |
|  GET   |     /api/auth/2      |   Endpoints/AuthenticationEndpoint.cs    |
|  GET   |       /api/di        | Endpoints/DependencyInjectionEndpoint.cs |
|  GET   | /api/weatherForecast |   Endpoints/WeatherForecastEndpoint.cs   |
|  GET   |      /api/multi      |        Endpoints/MultiEndpoint.cs        |
|  GET   |     /api/multi/2     |        Endpoints/MultiEndpoint.cs        |
| DELETE |      /api/multi      |        Endpoints/MultiEndpoint.cs        |
| DELETE |     /api/multi/2     |        Endpoints/MultiEndpoint.cs        |
|  POST  |     /api/multi/3     |        Endpoints/MultiEndpoint.cs        |
| DELETE |   /api/HelloWorld    |                Program.cs                |

## Generate JWT(s) for testing Authentication

You can use the build-in dotnet tool user-jwts

```sh
# using the default scheme (Bearer)
dotnet user-jwts create

# using the AnotherJWT scheme
dotnet user-jwts create --scheme AnotherJWT
```
