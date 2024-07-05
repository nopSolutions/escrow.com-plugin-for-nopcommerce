namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents a webhook collection
/// </summary>
public record WebhookCollection
{
    public Webhook[] Webhooks { get; set; }
}
