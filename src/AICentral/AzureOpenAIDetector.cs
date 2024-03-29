﻿using System.Text.Json;
using AICentral.Core;
using Microsoft.AspNetCore.WebUtilities;

namespace AICentral;

public class AzureOpenAIDetector
{
    public bool CanDetect(HttpRequest request)
    {
        return request.Path.StartsWithSegments("/openai");
    }

    public async Task<IncomingCallDetails> Detect(HttpRequest request, CancellationToken cancellationToken)
    {
        request.Path.StartsWithSegments("/openai", out var remainingUrlSegments);

        var remaining = remainingUrlSegments.ToString().Split('/');
        var callTypeFromUrl = remaining[1];
        string? incomingModelName = default;

        if (remaining[1] == "deployments")
        {
            incomingModelName = remaining[2];
            callTypeFromUrl = string.Join('/', remaining[3..]);
        }

        var callType = callTypeFromUrl switch
        {
            "chat/completions" => AICallType.Chat,
            "completions" => AICallType.Completions,
            "embeddings" => AICallType.Embeddings,
            "images" => AICallType.DALLE2,
            "operations" => AICallType.Other,
            "images/generations" => AICallType.DALLE3,
            "audio/transcriptions" => AICallType.Transcription,
            "audio/translations" => AICallType.Translation,
            _ => AICallType.Other
        };

        if (request.ContentType?.Contains("json", StringComparison.InvariantCultureIgnoreCase) ?? false)
        {
            //Pull out the text
            var requestContent = await JsonDocument.ParseAsync(request.Body, cancellationToken: cancellationToken);

            var promptText = callType switch
            {
                AICallType.Chat => string.Join(
                    '\n',
                    requestContent.RootElement.GetProperty("messages").EnumerateArray().Select(x => x.GetProperty("content").GetString()) ??
                    Array.Empty<string>()),
                AICallType.Embeddings => requestContent.RootElement.GetProperty("input").GetString() ?? string.Empty,
                AICallType.DALLE2 => requestContent.RootElement.GetProperty("prompt").GetString() ?? string.Empty,
                AICallType.DALLE3 => requestContent.RootElement.GetProperty("prompt").GetString() ?? string.Empty,
                AICallType.Completions => string.Join('\n', requestContent.RootElement.GetProperty("prompt").EnumerateArray().Select(x => x.GetString())),
                _ => throw new ArgumentOutOfRangeException()
            };

            return new IncomingCallDetails(
                callType,
                promptText,
                incomingModelName,
                requestContent,
                QueryHelpers.ParseQuery(request.QueryString.Value));
        }

        if (callType == AICallType.Transcription || callType == AICallType.Translation)
        {
            var model = request.Form["model"];
            return new IncomingCallDetails(callType, null, model, null,
                QueryHelpers.ParseQuery(request.QueryString.Value));
        }

        if (request.Method == "GET")
        {
            return new IncomingCallDetails(callType, null, null, null,
                QueryHelpers.ParseQuery(request.QueryString.Value));
        }

        throw new NotSupportedException("Call Type not supported by AI Central");
    }
}