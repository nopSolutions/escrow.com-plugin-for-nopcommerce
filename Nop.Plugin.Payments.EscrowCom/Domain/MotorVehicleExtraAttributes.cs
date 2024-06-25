namespace Nop.Plugin.Payments.EscrowCom.Domain;

/// <summary>
/// Represents additional information about item (motor vehicle transactions)
/// </summary>
public class MotorVehicleExtraAttributes : ExtraAttributes
{
    /// <summary>
    /// The vehicle's vehicle identification number (VIN).
    /// </summary>
    public string Vin { get; set; }

    /// <summary>
    /// The value of the vehicle's odometer.
    /// </summary>
    public string Odometer { get; set; }

    /// <summary>
    /// The year the vehicle was manufactured.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// The manufacturer of the vehicle.
    /// </summary>
    public string Make { get; set; }

    /// <summary>
    /// The model of the vehicle.
    /// </summary>
    public string Model { get; set; }

    /// <summary>
    /// Enable the title collection service.
    /// </summary>
    public bool TitleCollection { get; set; }

    /// <summary>
    /// Enable the lien holder payoff service.
    /// </summary>
    public bool LienHolderPayoff { get; set; }
}