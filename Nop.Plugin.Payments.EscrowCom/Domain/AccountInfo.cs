namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents an account information
/// </summary>
public record AccountInfo
{
    public AccountInfo()
    {
        Verification = new AccountVerification();
    }

    /// <summary>
    /// Account ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Verification information
    /// </summary>
    public AccountVerification Verification { get; set; }

    /// <summary>
    /// Existing webhooks
    /// </summary>
    public Webhook[] Webhooks { get; set; }
}
