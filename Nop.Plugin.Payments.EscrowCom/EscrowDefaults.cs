namespace Nop.Plugin.Payments.EscrowCom;

/// <summary>
/// Represents plugin constants
/// </summary>
public class EscrowDefaults
{
    /// <summary>
    /// Gets the plugin system name
    /// </summary>
    public static string SystemName => "Payments.EscrowCom";

    /// <summary>
    /// Gets the configuration route name
    /// </summary>
    public static string ConfigurationRouteName => "Plugin.Payments.EscrowCom.Configure";

    /// <summary>
    /// Gets the webhook route name
    /// </summary>
    public static string WebhookRouteName => "Plugin.Payments.EscrowCom.Webhook";

    /// <summary>
    /// Gets the Escrow Pay API Endpoint for production environment
    /// </summary>
    public static string EndpointUrl => "https://api.escrow.com/integration/pay/2018-03-31";

    /// <summary>
    /// Gets Escrow Pay API Endpoint for sandbox environment
    /// </summary>
    public static string SandboxEndpointUrl => "https://api.escrow-sandbox.com/integration/pay/2018-03-31";
}