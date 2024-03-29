using AICentral.Core;

namespace AICentral.EndpointSelectors.ResultHandlers;

public class StreamResponseHandler
{
    public static async Task<AICentralResponse> Handle(
        HttpContext context,
        CancellationToken cancellationToken,
        HttpResponseMessage openAiResponse,
        DownstreamRequestInformation requestInformation,
        ResponseMetadata responseMetadata)
    {
        //send the headers down to the client
        context.Response.StatusCode = (int)openAiResponse.StatusCode;
        context.Response.Headers.ContentType = openAiResponse.Content.Headers.ContentType?.ToString();

        //squirt the response as it comes in:
        await openAiResponse.Content.CopyToAsync(context.Response.Body, cancellationToken);
        await context.Response.Body.FlushAsync(cancellationToken);

        var chatRequestInformation = new DownstreamUsageInformation(
            requestInformation.LanguageUrl,
            null,
            requestInformation.DeploymentName,
            context.User.Identity?.Name ?? "unknown",
            requestInformation.CallType,
            null,
            requestInformation.Prompt,
            null,
            null,
            null,
            responseMetadata,
            context.Connection.RemoteIpAddress?.ToString() ?? "",
            requestInformation.StartDate,
            requestInformation.Duration,
            openAiResponse.IsSuccessStatusCode);

        return new AICentralResponse(chatRequestInformation, new StreamAlreadySentResultHandler());
    }
}