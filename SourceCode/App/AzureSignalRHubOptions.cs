using Microsoft.AspNetCore.SignalR;

namespace ModulesRegistry;

/// <summary>
/// Hub timeouts used with the Azure SignalR Service in production.
/// </summary>
/// <remarks>
/// The Azure SignalR SDK only accepts a handshake timeout from 1 to 30 seconds. The earlier 1 minute was rejected at every
/// negotiation (a Warning from <c>Microsoft.Azure.SignalR.NegotiateHandler</c>) and the service used its default instead.
/// </remarks>
public static class AzureSignalRHubOptions
{
    public static readonly TimeSpan KeepAliveInterval = TimeSpan.FromSeconds(15);
    public static readonly TimeSpan ClientTimeoutInterval = TimeSpan.FromMinutes(3);
    public static readonly TimeSpan HandshakeTimeout = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan MaxAzureHandshakeTimeout = TimeSpan.FromSeconds(30);

    public static void Configure(HubOptions options)
    {
        options.KeepAliveInterval = KeepAliveInterval;
        options.ClientTimeoutInterval = ClientTimeoutInterval;
        options.HandshakeTimeout = HandshakeTimeout;
    }
}