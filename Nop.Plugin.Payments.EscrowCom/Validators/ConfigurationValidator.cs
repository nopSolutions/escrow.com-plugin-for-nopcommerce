using FluentValidation;
using Nop.Plugin.Payments.EscrowCom.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.EscrowCom.Validators;

/// <summary>
/// Represents configuration model validator
/// </summary>
public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
{
    #region Ctor

    public ConfigurationValidator(ILocalizationService localizationService)
    {
        RuleFor(model => model.ApiKey)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.EscrowCom.Fields.ApiKey.Required"))
            .When(model => model.LiveMode);

        RuleFor(model => model.Email)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.EscrowCom.Fields.Email.Required"))
            .When(model => model.LiveMode);

        RuleFor(model => model.InspectionPeriod)
            .InclusiveBetween(1, 30)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.EscrowCom.Fields.InspectionPeriod.Invalid"));
    }

    #endregion
}