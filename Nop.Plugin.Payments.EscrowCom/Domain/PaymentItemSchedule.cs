namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents the way that is used for monetary amounts
/// </summary>
public class PaymentItemSchedule
{
    /// <summary>
    /// Amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// The email address of the customer that will be paying for the item or fee. 
    /// This field may also contain the value 'me', which refers to the store account
    /// </summary>
    public string PayerCustomer { get; set; }

    /// <summary>
    /// The email address of the party that will be receiving the funds from the item or fee.
    /// This field may also contain the value 'me' (which refers to the store account) or the value 'escrow' which refers to Escrow.com
    /// </summary>
    public string BeneficiaryCustomer { get; set; } = "me";
}
