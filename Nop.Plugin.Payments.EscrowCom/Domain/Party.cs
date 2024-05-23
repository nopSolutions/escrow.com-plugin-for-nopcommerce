namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents a customer that is part of a transaction and their status in the transaction
/// </summary>
public class Party
{
    /// <summary>
    /// The email address of the party. You may also pass the value 'me' if the party object is representing the store account
    /// </summary>
    public string Customer { get; set; } = "me";

    /// <summary>
    /// This is the role that the party is in the transaction
    /// </summary>
    public PartyRole Role { get; set; }

    /// <summary>
    /// This field indicates whether or not the party has agreed to the transaction
    /// </summary>
    public bool? Agreed { get; set; } = true;

    /// <summary>
    /// This field indicates whether or not the party was the initiator of the transaction
    /// </summary>
    public bool? Initiator { get; set; }

    /// <summary>
    /// This field contains the first name of the non-initiator party
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// This field contains the last name of the non-initiator party
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// This field contains the phone number of the non-initiator party
    /// </summary>
    public string PhoneNumber { get; set; }
}
