using Microsoft.AspNetCore.SignalR;

namespace ModulesRegistry.AppTests;

[TestClass]
public sealed class AzureSignalRHubOptionsTests
{
    [TestMethod]
    public void HandshakeTimeoutIsWithinWhatAzureSignalRAccepts()
    {
        Assert.IsGreaterThanOrEqualTo(TimeSpan.FromSeconds(1), AzureSignalRHubOptions.HandshakeTimeout);
        Assert.IsLessThanOrEqualTo(AzureSignalRHubOptions.MaxAzureHandshakeTimeout, AzureSignalRHubOptions.HandshakeTimeout);
    }

    [TestMethod]
    public void ClientTimeoutIsAtLeastTwiceTheKeepAliveInterval() =>
        Assert.IsGreaterThanOrEqualTo(2 * AzureSignalRHubOptions.KeepAliveInterval, AzureSignalRHubOptions.ClientTimeoutInterval);

    [TestMethod]
    public void ConfigureSetsAllThreeTimeouts()
    {
        var options = new HubOptions();

        AzureSignalRHubOptions.Configure(options);

        Assert.AreEqual(AzureSignalRHubOptions.KeepAliveInterval, options.KeepAliveInterval);
        Assert.AreEqual(AzureSignalRHubOptions.ClientTimeoutInterval, options.ClientTimeoutInterval);
        Assert.AreEqual(AzureSignalRHubOptions.HandshakeTimeout, options.HandshakeTimeout);
    }
}