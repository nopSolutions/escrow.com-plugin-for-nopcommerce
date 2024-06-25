namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents the party transaction role
/// </summary>
public enum PartyRole
{
    /// <summary>
    /// A third party to the transaction and are commonly a third party between the <see cref="Buyer"/> and <see cref="Seller"/>.
    /// </summary>
    Broker,

    /// <summary>
    /// The customer that are funding the transaction and will be receiving the items
    /// </summary>
    Buyer,

    /// <summary>
    /// The customer is similar to a <see cref="Broker"/> however partners may also be able to perform actions on behalf of users
    /// </summary>
    Partner,

    /// <summary>
    /// The customer that will be sending the items to the <see cref="Buyer"/> and will be receiving the funds at the end of the transaction
    /// </summary>
    Seller
}