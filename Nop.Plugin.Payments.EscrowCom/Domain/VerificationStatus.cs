namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents the verification status
/// </summary>
public enum VerificationStatus
{
    /// <summary>
    /// Verified
    /// </summary>
    Verified,

    /// <summary>
    /// Not verified
    /// </summary>
    NotVerified,

    /// <summary>
    /// Pending
    /// </summary>
    Pending,

    /// <summary>
    /// Not required
    /// </summary>
    NotRequired
}