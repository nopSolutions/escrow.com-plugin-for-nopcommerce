namespace Nop.Plugin.Payments.EscrowCom.Domain;
public record DomainNameExtraAttributes : ExtraAttributes
{
    /// <summary>
    /// Indicate that a domain name includes content.
    /// </summary>
    /// <remarks>
    /// If this field is set to true, then <see cref="Concierge"/> must not also be set
    /// </remarks>
    public bool? WithContent { get; set; }

    /// <summary>
    /// Indicate that a domain name requires the concierge service.
    /// </summary>
    /// <remarks>
    /// If this field is set to true, then the <see cref="WithContent"/> field must not be set
    /// </remarks>
    public bool? Concierge { get; set; }
}
