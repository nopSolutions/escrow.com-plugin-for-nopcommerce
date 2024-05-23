namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents the transaction item type
/// </summary>
public enum PaymentItemType
{
    BrokerFee,
    DomainName,
    DomainNameHolding,
    GeneralMerchandise,
    Milestone,
    MotorVehicle,
    PartnerFee,
    ShippingFee
}
