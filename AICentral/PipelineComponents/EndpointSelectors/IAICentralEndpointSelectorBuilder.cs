﻿using AICentral.PipelineComponents.Endpoints;

namespace AICentral.PipelineComponents.EndpointSelectors;

public interface IAICentralEndpointSelectorBuilder
{
    static virtual string ConfigName  => throw new NotImplementedException();

    static virtual IAICentralEndpointSelectorBuilder BuildFromConfig(
        Dictionary<string, string> parameters,
        Dictionary<string, IAICentralEndpointDispatcherBuilder> aiCentralEndpoints) => throw new NotImplementedException();

    IEndpointSelector Build(Dictionary<IAICentralEndpointDispatcherBuilder, IAICentralEndpointDispatcher> builtEndpointDictionary);

    void RegisterServices(IServiceCollection services);
}