namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represent transaction actions
/// </summary>
public enum TransactionActionType
{
    Accept,
    AcceptReturn,
    Agree,
    Batch,
    Cancel,
    Receive,
    Reject,
    Ship
}
