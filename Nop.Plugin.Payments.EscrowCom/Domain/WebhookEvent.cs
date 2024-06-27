namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents a webhook event
/// </summary>
public record WebhookEvent
{
    /// <summary>
    /// Event type
    /// </summary>
    public WebhookEventType EventType { get; set; }

    /// <summary>
    /// Event trigger
    /// </summary>
    public WebhookTrigger Event { get; set; }

    /// <summary>
    /// Transaction ID
    /// </summary>
    public int TransactionId { get; set; }
}