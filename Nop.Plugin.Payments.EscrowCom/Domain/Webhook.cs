namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents a webhook
/// </summary>
public record Webhook
{
    public int? Id { get; set; }

    public string Url { get; set; }
}