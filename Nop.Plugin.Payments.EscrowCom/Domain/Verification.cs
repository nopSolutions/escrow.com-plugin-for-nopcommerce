namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents the verification instance
/// </summary>
public record Verification
{
    /// <summary>
    /// Verification status
    /// </summary>
    public VerificationStatus Status { get; set; } = VerificationStatus.NotVerified;
}
