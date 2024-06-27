namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents a transaction
/// </summary>
public record TransactionInfo
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// This is a brief description of what the transaction is for
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The currency for the transaction
    /// </summary>
    public PaymentCurrency Currency { get; set; }

    /// <summary>
    /// The external reference for the transaction you want to get the details of.
    /// Here we will store the order GUID
    /// </summary>
    public string Reference { get; set; }

    /// <summary>
    /// The date at which the transaction was created.
    /// This field will be populated by Escrow.com when you create the transaction
    /// </summary>
    public string CreationDate { get; set; }

    /// <summary>
    /// The date at which the transaction was closed.
    /// This field will be populated by Escrow.com when the transaction is complete
    /// </summary>
    public string CloseDate { get; set; }

    /// <summary>
    /// This signifies if the transaction has been cancelled
    /// </summary>
    public bool IsCancelled { get; set; }

    /// <summary>
    /// This signifies if the transaction is a draft. Usually when it is an offer that has not been accepted yet
    /// </summary>
    public bool IsDraft { get; set; }
}
