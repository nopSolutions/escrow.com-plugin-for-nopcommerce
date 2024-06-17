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
    /// Gets a route name to redirect after a failed API calling  
    /// </summary>
    public static string FailedRouteName => "Homepage";

    /// <summary>
    /// Gets a route name to redirect after successful payment in Escrow.com wizard
    /// </summary>
    public static string CompletedRouteName => "CheckoutCompleted";

    /// <summary>
    /// Gets the webhook route name
    /// </summary>
    public static string WebhookRouteName => "Plugin.Payments.EscrowCom.Webhook";

    /// <summary>
    /// Gets the host URL for the production environment
    /// </summary>
    public static string ApiHost => "https://api.escrow.com";

    /// <summary>
    /// Gets the host URL for the sandbox environment
    /// </summary>
    public static string SandboxApiHost => "https://api.escrow-sandbox.com";

    /// <summary>
    /// Gets the Escrow Pay API Endpoint
    /// </summary>
    public static string PayPath => $"/integration/pay/2018-03-31";

    /// <summary>
    /// Gets the name of the generic attribute that is used to store the Escrow item type
    /// </summary>
    public static string EscrowItemTypeAttribute => "EscrowItemType";

    /// <summary>
    /// Gets the name of the generic attribute that is used to store specification attribute group
    /// </summary>
    public static string EscrowSpecificationAttributeGroupIdAttribute => "EscrowSpecificationAttributeGroup";

    /// <summary>
    /// Gets the group name for extra attributes
    /// </summary>
    public static string EscrowSpecificationAttributeGroupName => "Escrow extra attributes";

    /// <summary>
    /// Gets the names of Escrow extra attributes
    /// </summary>
    public static string[] ExtraAttributeNames => ["vin", "odometer", "year", "make", "model", "title_collection", "lien_holder_payoff", "with_content", "concierge", "dns_manager", "term_period", "image_url", "merchant_url"];
}