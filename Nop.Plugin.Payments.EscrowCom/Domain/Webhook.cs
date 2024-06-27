namespace Nop.Plugin.Payments.EscrowCom.Domain;
public record Webhook
{
    public int? Id { get; set; }
    public string Url { get; set; }
}
