namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents the body that is passed to the Escrow.com API when performing an action on a transaction
/// </summary>
public class TransactionAction
{
    /// <summary>
    /// Action to perform
    /// </summary>
    public TransactionActionType Action { get; set; }

    /// <summary>
    /// Role
    /// </summary>
    public PartyRole ActionTo { get; set; }
}
