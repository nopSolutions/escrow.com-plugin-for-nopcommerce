namespace Nop.Plugin.Payments.EscrowCom.Domain;

public class Transaction
{
    public int Id { get; set; }

    public string Description { get; set; }

    public PaymentCurrency Currency { get; set; }

    /// <summary>
    /// The items of the transaction
    /// </summary>
    public PaymentItem[] Items { get; set; }

    /// <summary>
    /// The array of parties involved in the transaction
    /// </summary>
    public Party[] Parties { get; set; }

    public string Reference { get; set; }

    public string CreationDate { get; set; }
    public string CloseDate { get; set; }

    public bool IsCancelled { get; set; }
    public bool IsDraft { get; set; }
}
