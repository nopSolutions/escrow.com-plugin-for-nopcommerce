namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents the shipping type
/// </summary>
public enum ShippingType
{
    /// <summary>
    /// No shipping
    /// </summary>
    NoShipping,

    /// <summary>
    /// Cargo shipping
    /// </summary>
    CargoShipping,

    /// <summary>
    /// Standard shipping
    /// </summary>
    StandardShipping
}