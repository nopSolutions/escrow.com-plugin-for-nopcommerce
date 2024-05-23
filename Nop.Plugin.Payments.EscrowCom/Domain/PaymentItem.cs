namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents a transaction item
/// </summary>
public class PaymentItem
{
    /// <summary>
    /// Additional attributes relevant to an item
    /// </summary>
    public ExtraAttributes ExtraAttributes { get; set; }

    /// <summary>
    /// The length of the inspection period in seconds.
    /// Currently the inspection period must be in whole multiples of days. e.g half a day (43200 seconds) is invalid where as 1 day (86400 seconds) and 2 days (172800 seconds) would be valid
    /// </summary>
    public int InspectionPeriod { get; set; } = 259200;

    /// <summary>
    /// The number of the item that is being sold.
    /// </summary>
    /// <remarks>NOTE: This value does not factor into the price and is purely informative. All values shown in the schedules are the total amount, not the unit price</remarks>
    public int? Quantity { get; set; }

    /// <summary>
    /// The name of the item being transferred
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The item type - can affect behaviour of the transaction and can also be used to specify party-specific fees
    /// </summary>
    public PaymentItemType? Type { get; set; }

    /// <summary>
    /// The way that we represent monetary amounts
    /// </summary>
    public PaymentItemSchedule Schedule { get; set; }

    /// <summary>
    /// The way that we represent fees - the amount, type of fee, and who pays the fee
    /// </summary>
    public PaymentFee[] Fees { get; set; }
}
