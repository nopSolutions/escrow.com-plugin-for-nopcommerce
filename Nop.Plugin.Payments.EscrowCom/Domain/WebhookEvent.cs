namespace Nop.Plugin.Payments.EscrowCom.Domain;
public class WebhookEvent
{
    public WebhookEventType EventType { get; set; } = WebhookEventType.Transaction;

    public WebhookTrigger Event { get; set; }

    public int TransactionId { get; set; }
}