namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents the fee payer
/// </summary>
public enum FeePayer
{
    /// <summary>
    /// Buyer
    /// </summary>
    Buyer,

    /// <summary>
    /// Seller
    /// </summary>
    Seller,
    /// <summary>
    /// Fee will be paid in half
    /// </summary>
    Split
}
