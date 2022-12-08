# EndpointMapper.TestApplication

## Sample application for EndpointMapper

This is a sample application build with EndpointMapper for testing the 
library during development and providing a few examples of the Developer Experience (DX)

## Generate JWT(s) for testing Authentication

You can use the build-in dotnet tool user-jwts

```sh
# using the default scheme (Bearer)
dotnet user-jwts create

# using the AnotherJWT scheme
dotnet user-jwts create --scheme AnotherJWT
```
