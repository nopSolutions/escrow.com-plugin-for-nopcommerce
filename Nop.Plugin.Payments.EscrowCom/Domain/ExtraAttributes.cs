namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents additional information about item
/// </summary>
public class ExtraAttributes
{
    /// <summary>
    /// The vehicle's vehicle identification number (VIN). Only applicable on motor vehicle transactions (<see cref="PaymentItemType.MotorVehicle"/>)
    /// </summary>
    public string Vin { get; set; }

    /// <summary>
    /// The value of the vehicle's odometer. Only applicable on transactions with <see cref="PaymentItemType.MotorVehicle"/>
    /// </summary>
    public string Odometer { get; set; }

    /// <summary>
    /// The year the vehicle was manufactured. Only applicable on transactions with <see cref="PaymentItemType.MotorVehicle"/>
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// The manufacturer of the vehicle. Only applicable on transactions with <see cref="PaymentItemType.MotorVehicle"/>
    /// </summary>
    public string Make { get; set; }

    /// <summary>
    /// The model of the vehicle. Only applicable on transactions with <see cref="PaymentItemType.MotorVehicle"/>
    /// </summary>
    public string Model { get; set; }

    /// <summary>
    /// Enable the title collection service. Only applicable on transactions with <see cref="PaymentItemType.MotorVehicle"/>
    /// </summary>
    public bool TitleCollection { get; set; }

    /// <summary>
    /// Enable the lien holder payoff service. Only applicable on transactions with <see cref="PaymentItemType.MotorVehicle"/>
    /// </summary>
    public bool? LienHolderPayoff { get; set; }

    /// <summary>
    /// Indicate that a domain name includes content. Only applicable on transactions with <see cref="PaymentItemType.DomainName"/>
    /// </summary>
    /// <remarks>
    /// If this field is set to true, then <see cref="Concierge"/> must not also be set
    /// </remarks>
    public bool? WithContent { get; set; }

    /// <summary>
    /// Indicate that a domain name requires the concierge service. Only applicable on transactions with <see cref="PaymentItemType.DomainName"/>
    /// </summary>
    /// <remarks>
    /// If this field is set to true, then the <see cref="WithContent"/> field must not be set
    /// </remarks>
    public bool? Concierge { get; set; }

    /// <summary>
    /// The party that will be managing the domain name. Only applicable on transactions with <see cref="PaymentItemType.DomainNameHolding"/>
    /// <para>
    /// Valid values: buyer, escrow
    /// </para>
    /// </summary>
    public string DnsManager { get; set; }

    /// <summary>
    /// The duration of the domain name holding service in months. Only applicable on transactions with <see cref="PaymentItemType.DomainNameHolding"/>
    /// </summary>
    public int? TermPeriod { get; set; }

    /// <summary>
    /// Image URL representing the merchandise item
    /// </summary>
    public string ImageUrl { get; set; }

    /// <summary>
    /// URL leading to the item's listing in the partner's page
    /// </summary>
    public string MerchantUrl { get; set; }
}
