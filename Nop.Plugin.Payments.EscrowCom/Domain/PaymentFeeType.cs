namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents the type of the payment fee
/// </summary>
public enum PaymentFeeType
{
    Concierge,
    CreditCard,
    Disbursement,
    DomainNameHolding,
    Escrow,
    Intermediary,
    LienHolderPayoff,
    MotorVehicle,
    Other,
    TitleCollection
}
