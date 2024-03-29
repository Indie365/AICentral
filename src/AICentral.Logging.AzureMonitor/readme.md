﻿# Extensions for use in AI Central

## Azure Monitor advanced usage and quota Logging

To use, add a config section like below:

```json
{
  "GenericSteps": [
    {
      "Type": "AspNetCoreFixedWindowRateLimiting",
      "Name": "window-rate-limiter",
      "Properties": {
        "LimitType": "PerConsumer|PerAICentralEndpoint",
        "MetricType": "Requests",
        "Options": {
          "Window": "00:00:10",
          "PermitLimit": 100
        }
      }
    },
    {
      "Type": "AzureMonitorLogger",
      "Name": "azure-monitor-logger",
      "Properties": {
        "WorkspaceId": "<workspace-id>",
        "Key": "<workspace-key>",
        "LogPrompt": true,
        "LogResponse": false
      }
    }
  ]
}
```

```csharp

builder.Services.AddAICentral(
    builder.Configuration,
    additionalComponentAssemblies: typeof(AzureMonitorLoggerFactory).Assembly);

```
