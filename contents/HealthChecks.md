# Health Checks

Brighter provides an AspNet Core Health check for **Service Activator**

## Configure Health Checks

The below will configure Brighter Health checks for /health and a more detailed page for /health/detail

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

## Health Status

The following will be produced

| Scenario | Status |
| -------- | ------ |
| All Message Pumps are running | Healthy |
| Some Message Pumps are running | Degraded |
| No Message Pumps are running | Unhealthy |

In the event on a Degraded status the /health/details page can be used to find out which Dispatchers have failed pumps