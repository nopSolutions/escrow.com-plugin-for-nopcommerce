using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Payments.EscrowCom.Domain;
using Nop.Plugin.Payments.EscrowCom.Models;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.EscrowCom.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class EscrowPaymentController : BasePaymentController
{
    #region Fields

    private readonly CurrencySettings _currencySettings;
    private readonly EscrowSettings _escrowSettings;
    private readonly ICurrencyService _currencyService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public EscrowPaymentController(CurrencySettings currencySettings,
        EscrowSettings escrowSettings,
        ICurrencyService currencyService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService)
    {
        _currencySettings = currencySettings;
        _escrowSettings = escrowSettings;
        _currencyService = currencyService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _settingService = settingService;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
            return AccessDeniedView();

        var model = new ConfigurationModel
        {
            Email = _escrowSettings.Email,
            ApiKey = _escrowSettings.ApiKey,
            UseSandbox = _escrowSettings.UseSandbox,
            FeePayer = _escrowSettings.FeePayer,
            InspectionPeriod = _escrowSettings.InspectionPeriod
        };

        //check currency
        var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
        if (!Enum.TryParse(typeof(PaymentCurrency), currency.CurrencyCode, out _))
        {
            var locale = await _localizationService.GetResourceAsync("Plugins.Payments.EscrowCom.Currency.Warning");
            var warning = string.Format(locale, currency.CurrencyCode, Url.Action("List", "Currency"));
            _notificationService.WarningNotification(warning, false);
        }

        return View("~/Plugins/Payments.EscrowCom/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
            return AccessDeniedView();

        if (!ModelState.IsValid)
            return await Configure();

        //save settings
        _escrowSettings.Email = model.Email;
        _escrowSettings.ApiKey = model.ApiKey;
        _escrowSettings.UseSandbox = model.UseSandbox;
        _escrowSettings.FeePayer = model.FeePayer;
        _escrowSettings.InspectionPeriod = model.InspectionPeriod;

        _settingService.SaveSetting(_escrowSettings);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #endregion
}