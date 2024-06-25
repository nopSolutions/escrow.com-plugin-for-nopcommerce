namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents the transaction item type
/// </summary>
public enum ItemType
{
    /// <summary>
    /// General merchandise
    /// </summary>
    GeneralMerchandise,

    /// <summary>
    /// Motor vehicle
    /// </summary>
    MotorVehicle,

    /// <summary>
    /// Domain name
    /// </summary>
    DomainName,

    /// <summary>
    /// Shipping fee
    /// </summary>
    ShippingFee
}