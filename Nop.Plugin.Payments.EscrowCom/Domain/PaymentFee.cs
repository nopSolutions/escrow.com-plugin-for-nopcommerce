namespace Nop.Plugin.Payments.EscrowCom.Domain;
public class PaymentFee
{
    #region Fields

    private decimal _splitValue;

    #endregion

    /// <summary>
    /// The type of fee being displayed
    /// </summary>
    public PaymentFeeType Type { get; set; }

    /// <summary>
    /// The party who will pay the fee
    /// </summary>
    public string PayerCustomer { get; set; }

    /// <summary>
    /// Split of total fee to be paid by <see cref="PayerCustomer"/>
    /// <para>
    /// Valid values: <c>0; 0.5; 1</c>
    /// </para>
    /// </summary>
    public decimal Split
    {
        get => _splitValue;
        set
        {
            var validValues = new[] { 0, .5m, 1 };

            if (!validValues.Contains(value))
                throw new ArgumentException($"Invalid value. It must be one of: [{string.Join(", ", validValues)}]");

            _splitValue = value;
        }
    }
}
