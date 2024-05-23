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
    /// Currency used for transactions
    /// </summary>
    public PaymentCurrency Currency { get; set; } = PaymentCurrency.USD;

    /// <summary>
    /// Inspection period for transactions
    /// </summary>
    public int InspectionPeriod { get; set; }

    /// <summary>
    /// Type of the fee
    /// </summary>
    public PaymentFeeType PaymentFeeType { get; set; }

    /// <summary>
    /// Payment item type
    /// </summary>
    public PaymentItemType PaymentItemType { get; set; }

    /// <summary>
    /// Who pays the fee
    /// </summary>
    public FeePayer FeePayer { get; set; }
}