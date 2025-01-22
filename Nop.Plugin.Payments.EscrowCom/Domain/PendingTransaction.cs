namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents the web URL for transaction
/// </summary>
public class PendingTransaction
{
    /// <summary>
    /// URL which the customer can navigate to on Escrow.com
    /// </summary>
    public string LandingPage { get; set; }

    /// <summary>
    /// Token which is used to identify the buyer's transaction on the the Escrow Pay wizard
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// Transaction identifier
    /// </summary>
    public int TransactionId { get; set; }
}
