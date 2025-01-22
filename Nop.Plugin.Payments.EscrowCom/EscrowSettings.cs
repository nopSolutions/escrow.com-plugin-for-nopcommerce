using Nop.Core.Configuration;
using Nop.Plugin.Payments.EscrowCom.Domain;

namespace Nop.Plugin.Payments.EscrowCom;

/// <summary>
/// Represents plugin settings
/// </summary>
public class EscrowSettings : ISettings
{
    /// <summary>
    /// Gets or sets an API key
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// Gets or sets an email address
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use sandbox (testing environment)
    /// </summary>
    public bool UseSandbox { get; set; }

    /// <summary>
    /// Gets or sets an inspection period (in days) for transactions
    /// </summary>
    public int InspectionPeriod { get; set; }

    /// <summary>
    /// Gets or sets a fee payer
    /// </summary>
    public FeePayer FeePayer { get; set; }

    /// <summary>
    /// Gets or sets the group identifier with Vehicle extra attributes
    /// </summary>
    public int EscrowVehicleSpecGroupId { get; set; }

    /// <summary>
    /// Gets or sets the group identifier with domain name extra attributes
    /// </summary>
    public int EscrowDomainNameSpecGroupId { get; set; }

    /// <summary>
    /// Gets or sets the Escrow webhook identifier
    /// </summary>
    public int WebhookId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the linked account is approved
    /// </summary>
    public bool IsApprovedAccount { get; set; }

    /// <summary>
    /// Gets or sets the name os webhook event that will be used to mark an order as paid
    /// </summary>
    public WebhookTrigger OrderPaidEvent { get; set; }

    /// <summary>
    /// Gets or sets the name os webhook event that will be used to cancel order
    /// </summary>
    public WebhookTrigger OrderCancelEvent { get; set; }
}