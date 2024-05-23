namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents a payment response
/// </summary>
public class PaymentResponse
{
    /// <summary>
    /// Gets the URL to which the buyer should be redirected
    /// </summary>
    public string LandingPage { get; set; }

    /// <summary>
    /// The token which is used to identify the buyer's transaction on the the Escrow Pay wizard. While it is embedded in the <see cref="LandingPage" />, it is split out here for convenience
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// The identifier of the transaction that was created by the call to the Escrow Pay API. This is provided so that subsequent calls may be made to the standard Escrow API
    /// </summary>
    public string TransactionId { get; set; }
}
