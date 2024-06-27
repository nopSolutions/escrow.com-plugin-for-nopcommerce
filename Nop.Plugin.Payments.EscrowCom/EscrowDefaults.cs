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
    public static string FailedRouteName => "OrderDetails";

    /// <summary>
    /// Gets a route name to redirect after successful payment in Escrow.com wizard
    /// </summary>
    public static string CompletedRouteName => "CheckoutCompleted";

    /// <summary>
    /// Gets the webhook route name
    /// </summary>
    public static string WebhookRouteName => "Plugin.Payments.EscrowCom.Webhook";

    /// <summary>
    /// Gets the host URL
    /// </summary>
    public static (string Sandbox, string Production) ApiHost => ("https://api.escrow-sandbox.com", "https://api.escrow.com");

    /// <summary>
    /// Gets the Escrow Pay API Endpoint
    /// </summary>
    public static string PayPath => $"/integration/pay/2018-03-31";

    /// <summary>
    /// Gets the name of the generic attribute that is used to store the Escrow item type
    /// </summary>
    public static string EscrowItemTypeAttribute => "EscrowItemType";

    /// <summary>
    /// Gets the group name for extra attributes
    /// </summary>
    public static string EscrowSpecificationAttributeGroupName => "Escrow extra attributes";

    /// <summary>
    /// Gets the names of Escrow extra attributes
    /// </summary>
    public static Dictionary<string, string> DomainExtraAttributeNames => new()
    {
        ["lien_holder_payoff"] = "Lien Payoff Service",
        ["with_content"] = "With Content",
        ["concierge"] = "Domain Concierge Service"
    };

    /// <summary>
    /// Gets the names of Escrow extra attributes
    /// </summary>
    public static Dictionary<string, string> MotorVehicleExtraAttributeNames => new()
    {
        ["vin"] = "VIN",
        ["odometer"] = "Odometer",
        ["year"] = "Year",
        ["make"] = "Make",
        ["model"] = "Model",
        ["title_collection"] = "Title Collection Service"
    };
}