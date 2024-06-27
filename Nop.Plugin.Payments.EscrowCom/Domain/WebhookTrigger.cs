namespace Nop.Plugin.Payments.EscrowCom.Domain;
public enum WebhookTrigger
{
    /// <summary>
    /// A new transaction has been created
    /// </summary>
    Create,

    /// <summary>
    /// All parties have agreed to the transaction
    /// </summary>
    Agree,

    /// <summary>
    /// A party has submitted their verification for review
    /// </summary>
    PartyVerificationSubmitted,

    /// <summary>
    /// A party has had their verification reviewed and rejected
    /// </summary>
    PartyVerificationRejected,

    /// <summary>
    /// A party has had their verification reviewed and approved
    /// </summary>
    PartyVerificationApproved,

    /// <summary>
    /// Escrow.com has approved the payment for the transaction and the goods may now be shipped by the seller
    /// </summary>
    PaymentApproved,

    /// <summary>
    /// Escrow.com has rejected the payment for the transaction
    /// </summary>
    PaymentRejected,

    /// <summary>
    /// The buyer has sent payment to Escrow.com
    /// </summary>
    PaymentSent,

    /// <summary>
    /// Escrow.com has received payment from the buyer
    /// </summary>
    PaymentReceived,

    /// <summary>
    /// Escrow.com has refunded a buyer's payment
    /// </summary>
    PaymentRefunded,

    /// <summary>
    /// Escrow.com has disbursed payment to the seller
    /// </summary>
    PaymentDisbursed,

    /// <summary>
    /// The seller has indicated that the goods have been shipped
    /// </summary>
    Ship,

    /// <summary>
    /// The buyer has indicated that the goods have been received
    /// </summary>
    Receive,

    /// <summary>
    /// The buyer has indicated that the goods have been accepted
    /// </summary>
    Accept,

    /// <summary>
    /// The buyer has indicated that the goods have been rejected
    /// </summary>
    Reject,

    /// <summary>
    /// The buyer has indicated that the goods to be returned following rejection have been shipped
    /// </summary>
    ShipReturn,

    /// <summary>
    /// The seller has indicated that the goods to be returned following rejection have been received
    /// </summary>
    ReceiveReturn,

    /// <summary>
    /// The seller has indicated that the goods to be returned following rejection have been accepted
    /// </summary>
    AcceptReturn,

    /// <summary>
    /// The seller has indicated that the goods to be returned following rejection have been rejected
    /// </summary>
    RejectReturn,

    /// <summary>
    /// All disbursements have been made to the seller, closing statements have been sent. Escrow.com marked the transaction as complete
    /// </summary>
    Complete,

    /// <summary>
    /// Escrow.com has marked the payment as cancelled
    /// </summary>
    Cancel,

    /// <summary>
    /// This is sent for Escrow Offer transactions, when an offer has been accepted
    /// </summary>
    OfferAccepted,

    /// <summary>
    /// This is sent when Escrow.com has approved to process a refund for a transaction
    /// </summary>
    RefundResolved,

    /// <summary>
    /// This is sent when Escrow.com has rejected to process a refund for the transaction
    /// </summary>
    RefundRejected,

    /// <summary>
    /// Escrow.com has approved the verification submission of the merchant linked to the plugin
    /// </summary>
    CustomerVerificationApproved
}
