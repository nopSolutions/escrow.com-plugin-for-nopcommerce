namespace Nop.Plugin.Payments.EscrowCom.Domain;
public record WebhookCollection
{
    public Webhook[] Webhooks { get; set; }
}
