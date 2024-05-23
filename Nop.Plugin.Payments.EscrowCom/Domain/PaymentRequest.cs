namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents a payment requst
/// </summary>
public class PaymentRequest
{
    /// <summary>
    /// The currency for the transaction
    /// </summary>
    public PaymentCurrency Currency { get; set; } = PaymentCurrency.USD;

    /// <summary>
    /// This is a brief description of what the transaction is for
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The redirect url that will be used after user has been redirected to the Escrow paypal success page
    /// </summary>
    public string ReturnUrl { get; set; }

    /// <summary>
    /// The items of the transaction
    /// </summary>
    public PaymentItem[] Items { get; set; }

    /// <summary>
    /// The array of parties involved in the transaction
    /// </summary>
    public Party[] Parties { get; set; }
}
