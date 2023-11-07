﻿using System.Text.RegularExpressions;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AICentral;

public class OpenAiCallInformationExtractor : IIncomingCallExtractor
{
    private static readonly Regex
        OpenAiUrlRegex = new("^/v1/(embeddings|chat|completions)/(.*)$");

    public async Task<AICallInformation> Extract(HttpRequest request, CancellationToken cancellationToken)
    {
        using var
            requestReader = new StreamReader(request.Body);

        var requestRawContent = await requestReader.ReadToEndAsync(cancellationToken);
        var deserializedRequestContent = (JObject)JsonConvert.DeserializeObject(requestRawContent)!;

        var openAiUriParts = OpenAiUrlRegex.Match(request.Path.ToString());
        var requestTypeRaw = openAiUriParts.Groups[1].Captures[0].Value;

        var requestType = requestTypeRaw switch
        {
            "chat" => AICallType.Chat,
            "embeddings" => AICallType.Embeddings,
            "completions" => AICallType.Completions,
            _ => throw new InvalidOperationException($"AICentral does not currently support {requestTypeRaw}")
        };

        var promptText = requestType switch
        {
            AICallType.Chat => string.Join(
                Environment.NewLine,
                deserializedRequestContent["messages"]?.Select(x => x.Value<string>("content")) ??
                Array.Empty<string>()),
            AICallType.Embeddings => deserializedRequestContent.Value<string>("input") ?? string.Empty,
            AICallType.Completions => string.Join(Environment.NewLine,
                deserializedRequestContent["prompt"]?.Select(x => x.Value<string>()) ?? Array.Empty<string>()),
            _ => throw new InvalidOperationException($"Unknown AICallType")
        };

        var incomingModelName = deserializedRequestContent.Value<string>("model");

        return new AICallInformation(
            requestType,
            incomingModelName,
            deserializedRequestContent,
            promptText,
            $"{openAiUriParts.Groups[1].Captures[0].Value}/{openAiUriParts.Groups[2].Captures[0].Value}",
            QueryHelpers.ParseQuery(request.QueryString.Value ?? string.Empty));
    }
}