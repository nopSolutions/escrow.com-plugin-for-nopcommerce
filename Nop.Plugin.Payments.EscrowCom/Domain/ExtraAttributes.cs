namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents additional information about item
/// </summary>
public class ExtraAttributes
{
    /// <summary>
    /// Image URL representing the merchandise item
    /// </summary>
    public string ImageUrl { get; set; }

    /// <summary>
    /// URL leading to the item's listing in the partner's page
    /// </summary>
    public string MerchantUrl { get; set; }
}
