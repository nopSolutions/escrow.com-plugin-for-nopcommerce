using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Payments.EscrowCom.Domain;
using Nop.Services;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.EscrowCom.Models;

/// <summary>
/// Represents configuration model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Payments.EscrowCom.Fields.ApiKey")]
    [NoTrim]
    [DataType(DataType.Password)]
    public string ApiKey { get; set; }

    [NopResourceDisplayName("Plugins.Payments.EscrowCom.Fields.Email")]
    public string Email { get; set; }

    [NopResourceDisplayName("Plugins.Payments.EscrowCom.Fields.UseSandbox")]
    public bool UseSandbox { get; set; }

    [NopResourceDisplayName("Plugins.Payments.EscrowCom.Fields.InspectionPeriod")]
    public int InspectionPeriod { get; set; }

    [NopResourceDisplayName("Plugins.Payments.EscrowCom.Fields.FeePayer")]
    public FeePayer FeePayer { get; set; }
    public static List<SelectListItem> AvailableFeePayers => FeePayer.Buyer.ToSelectListAsync().Result?.ToList() ?? new();

    [NopResourceDisplayName("Plugins.Payments.EscrowCom.Fields.Verification")]
    public string Verification { get; set; }

    public bool IsConfigured { get; set; }
}