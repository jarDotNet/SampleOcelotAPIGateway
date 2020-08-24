# SampleOcelotAPIGateway

This sample demo shows how to building a simple API Gateway in .NET Core with Ocelot.

## What is an API Gateway?

A Gateway is a point of entry that you have to go through if you want to access somewhere is protected. Similarly, API Gateway is a component that sits in front of your APIs and insides an intranet or firewall. Implementing API Gateway will help to make sure that every request from outside (internet) will have to go through it before reach to your APIs.

## Ocelot

Ocelot is an API Gateway based on the .NET Core framework and a rich set of features including:

* Routing
* Request Aggregation
* Service Discovery with Consul & Eureka
* Service Fabric
* Kubernetes
* WebSockets
* Authentication / Authorization
* Rate Limiting
* Caching
* Load Balancing
* Retry policies / QoS using Consul and Polly
* Logging / Tracing / Correlation
* Headers / Query String / Claims Transformation
* Configuration / Administration REST API
* Platform / Cloud Agnostic

Ocelot requires to provide configuration file, that has a list of ```Routes``` (configuration used to map upstream request) and Global Configuration (other configuration like QoS, Rate limiting, etc.).

Through this sample, I just show the following Ocelot features:

* Reverse Proxy
* Re-Routing via URL rewriting
* Authentication using IdentityServer4

## APIGateway project

This sample project has 4 components:

* Two simple Web APIs (CustomerApi and EmployeesApi).
* The Ocelot API Gateway as the APIs public access point.
* An Identity Server authentication service used by the API Gateway to authenticate incoming requests.

### CustomerApi

A simple Web API with a single endpoint deployed at http://localhost:8000/api/customers.

### EmployeesApi

Another simple Web API with a single endpoint deployed at http://localhost:8001/api/employees.

### OcelotAPIGateway

Basically, Ocelot is a set of middlewares that can be applied in a specific order. The API Gateway will run on the port **_5001_** with **HTTPS**.

The main function of Ocelot API Gateway is to take incoming HTTP requests and forward them on to downstream service, currently as another HTTP request. Ocelot describes the routing of one request to another as a Route in a configuration file.

The ```configuration.json``` file is as follows:

```json
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/Customers",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 8000
        }
      ],
      "UpstreamPathTemplate": "/customers",
      "UpstreamHttpMethod": [ "GET" ]
    },
    {
      "DownstreamPathTemplate": "/api/Employees",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 8001
        }
      ],
      "UpstreamPathTemplate": "/employees",
      "UpstreamHttpMethod": []
    }
  ]
```

The DownstreamPathTemplate, Scheme, and DownstreamHostAndPorts make the internal service URL that this request will be forwarded to. In this case, both Web APIs endpoints.

The port is the internal port of service, **_8000_** for the _Customers_ Web Api, and **_8001_** for the _Employees_ one.

The host is where service is hosted. For instance, localhost.

Items that start with Upstream which means that we should use HTTPS GET method with /Employees or /Customers to visit these services.

So, when visiting https://localhost:5001/customers, we will get the result from http://localhost:8000/api/customers.

And when visiting https://localhost:5001/employees, we will get the result from http://localhost:8001/api/employees.

### IdentityServer

In order to protect the API endpoints, we will use IdentityServer bearer tokens. You can access this post: [IdentityServer4: Building a Simple Token Server and Protecting Your ASP.NET Core APIs with JWT](https://vmsdurano.com/apiboilerplate-and-identityserver4-access-control-for-apis/) to get a clearer idea on how to secure **ASP.NET Core Web Applications** using Identity Server.

In our case, we need to install ```IdentityServer4.AccessTokenValidation``` NuGet first in our API Gateway project. This is an ASP.NET Core authentication handler to validate ```JWT``` and reference tokens from IdentityServer4. Next, let's setup JWT Authentication Handler with IdentityServer4 by adding the following code at ```ConfigureServices``` method of ```Startup.cs``` file:

```csharp
    public void ConfigureServices(IServiceCollection services)
    {
        var authenticationProviderKey = "TestKey";

        services.AddAuthentication()
                .AddJwtBearer(authenticationProviderKey, opt =>
                {
                    opt.Authority = "https://localhost:6001";
                    opt.Audience = "SampleApiService";
                    opt.RequireHttpsMetadata = false;
                });

        services.AddControllers();
        services.AddOcelot(Configuration);
    }
```

Finally, we map the authentication provider key to a Route in the configuration file:

```json
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/Customers",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 8000
        }
      ],
      "UpstreamPathTemplate": "/customers",
      "UpstreamHttpMethod": [ "GET" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "TestKey",
        "AllowedScopes": []
      }
    },
```

So, for each route we want to securize, we just need to add the authentication provider key as follows:

```json
    "AuthenticationOptions": {
      "AuthenticationProviderKey": "TestKey",
      "AllowedScopes": []
    }
```

If you add scopes to AllowedScopes, Ocelot will get all the user claims (from the token) of the type scope and make sure that the user has all of the scopes in the list. This is a way to restrict access to a Route on a per scope basis.

## Testing the API Gateway

This repo comes with a Postman collection file that allows you to test the full sample project.

Just remember that in order to be authorized, you need a JWT bearer from our Token Identity Server, or you will get a ```401 Unauthorized``` response. In this case, you should get somethig like this from the Token request:

```json
{
    "access_token": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjIzZDIyYTU1NjhiYWU4M2Q0NWRmNDY1MTlmMmY5NmJmIiwidHlwIjoiYXQrand0In0.eyJuYmYiOjE1OTc5Mjk4NTQsImV4cCI6MTU5NzkzMzQ1NCwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NjAwMSIsImF1ZCI6IlNhbXBsZVNlcnZpY2UiLCJjbGllbnRfaWQiOiJDbGllbnRJZCIsInNjb3BlIjpbIlNhbXBsZVNlcnZpY2UiXX0.vhWMj5SfEoUDQUedKSsodUTxDK3bNVXVM1gZYWSiV1ptzuvekNLQi1sqSDB62YEuuoBeNh0hbxYsoFmu7S17UTtR259aXqCJzG8EccBUg7jD2ilVy9W9PmKDOpc5vZMptLvdsWCXPQCinqVqHEuqIQPmY3C5ZIb4M_DHYFR0C8d50qAwC2vNwLtnbSt1dS18rteBp582CgLN2Fl-eZrjv-wSormAsAyco4pWgolIiGkBru0lVhrIUjzo67gLIhv7MhvOYNAO1N-hhou_fExZG89fgZuxdLFsHLL7aa7Dd_NPKbLxDKyHmmY6nLRIRHdivEPcLBCXupse-x1rEBIFsg",
    "expires_in": 3600,
    "token_type": "Bearer",
    "scope": "SampleApiService"
}
```

So once you get a brand new ```JWT```, the value of the ```acess_token``` must be added to the ```Bearer Authorization``` header of any ```GET HTTPS``` request send to the API Gateway. Then you should be able to see an Http Status ```200``` back with the response from the ```GET HTTP``` Web API call.
