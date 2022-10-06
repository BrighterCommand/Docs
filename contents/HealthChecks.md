# Health Checks

Brighter provides an AspNet Core Health check for **Service Activator**

## Configure Health Checks

The below will configure ASP.Net Core Health checks for Brighter's **Service Activator**, for more information on [ASP.NET Core Health Check](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-6.0)

```csharp
// Web Application Builder code goes here

builder.Services.AddHealthChecks()
    .AddCheck<BrighterServiceActivatorHealthCheck>("Brighter", HealthStatus.Unhealthy);

var  app = builder.Build();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health");
    endpoints.MapHealthChecks("/health/detail", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            var content = new
            {
                Status = report.Status.ToString(),
                Results = report.Entries.ToDictionary(e => e.Key,
                    e => new
                    {
                        Status = e.Value.Status.ToString(),
                        Description = e.Value.Description,
                        Duration = e.Value.Duration
                    }),
                TotalDuration = report.TotalDuration
            };

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(content, JsonSerialisationOptions.Options));
        }
    });
});

app.Run();

```

The /health endpoing will return a Status 200 with the Body of the Health status (i.e. Healthy) 

The /health/detail endpoint will return a detailed response with all of your information for example:

```json
{
  "status": "Healthy",
  "results": {
    "Brighter": {
      "status": "Healthy",
      "description": "21 healthy consumers.",
      "duration": "00:00:00.0000132"
    }
  },
  "totalDuration": "00:00:00.0029747"
}
```

## Health Status

The following will be produced

| Scenario | Status |
| -------- | ------ |
| All Message Pumps are running | Healthy |
| Some Message Pumps are running | Degraded |
| No Message Pumps are running | Unhealthy |

In the event on a Degraded status the /health/details page can be used to find out which Dispatchers have failed pumps