namespace ModulesRegistry.Services.Implementations;

/// <summary>Whether a MAC address is free for a WiFRED throttle, for the form's asynchronous validation (Migration step 3.6).</summary>
public interface IWiFredMacAddresses
{
    /// <summary>
    /// The MAC address (in any notation) belongs to another throttle: for an edited throttle (<paramref name="throttleId"/> &gt; 0)
    /// any other throttle, since the address is unique; for a new one an active throttle, since a deleted one is registered again.
    /// </summary>
    Task<bool> IsMacAddressTakenAsync(string? macAddress, int throttleId, CancellationToken cancellationToken = default);
}