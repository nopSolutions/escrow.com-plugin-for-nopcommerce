namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents the verification information
/// </summary>
public record AccountVerification
{
    public AccountVerification()
    {
        Company = new Verification();
        Personal = new Verification();
    }

    /// <summary>
    /// The customer's company verification
    /// </summary>
    public Verification Company { get; set; }

    /// <summary>
    /// The customer's personal verification
    /// </summary>
    public Verification Personal { get; set; }
}
