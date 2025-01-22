using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Payments.EscrowCom.Domain;
using Nop.Plugin.Payments.EscrowCom.Models;
using Nop.Plugin.Payments.EscrowCom.Services;
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
    private readonly EscrowService _escrowService;
    private readonly EscrowSettings _escrowSettings;
    private readonly ICurrencyService _currencyService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public EscrowPaymentController(CurrencySettings currencySettings,
        EscrowService escrowService,
        EscrowSettings escrowSettings,
        ICurrencyService currencyService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService)
    {
        _currencySettings = currencySettings;
        _escrowService = escrowService;
        _escrowSettings = escrowSettings;
        _currencyService = currencyService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _settingService = settingService;
    }

    #endregion

    #region Utilities

    private async Task<string> GetVerificationStatusAsync()
    {
        var account = await _escrowService.GetAccountInfoAsync();
        if (account?.Verification is null)
            return string.Empty;

        var personalStatus = await _localizationService.GetLocalizedEnumAsync(account.Verification.Personal?.Status ?? VerificationStatus.NotVerified);
        var companyStatus = await _localizationService.GetLocalizedEnumAsync(account.Verification.Company?.Status ?? VerificationStatus.NotVerified);

        return $"Personal: {personalStatus} | Company: {companyStatus}";
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
            LiveMode = !_escrowSettings.UseSandbox,
            FeePayer = _escrowSettings.FeePayer,
            InspectionPeriod = _escrowSettings.InspectionPeriod,
            IsConfigured = _escrowService.IsConfigured()
        };

        //check currency
        var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
        if (!Enum.TryParse(typeof(PaymentCurrency), currency.CurrencyCode, out _))
        {
            var locale = await _localizationService.GetResourceAsync("Plugins.Payments.EscrowCom.Currency.Warning");
            var warning = string.Format(locale, currency.CurrencyCode, Url.Action("List", "Currency"));
            _notificationService.WarningNotification(warning, false);
        }

        if (model.IsConfigured)
            model.Verification = await GetVerificationStatusAsync();

        return View("~/Plugins/Payments.EscrowCom/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
            return AccessDeniedView();

        if (!ModelState.IsValid)
            return await Configure();

        _escrowSettings.Email = model.Email;
        _escrowSettings.ApiKey = model.ApiKey;
        _escrowSettings.UseSandbox = !model.LiveMode;
        _escrowSettings.FeePayer = model.FeePayer;
        _escrowSettings.InspectionPeriod = model.InspectionPeriod;

        if (_escrowSettings.WebhookId == 0)
        {
            var webhook = await _escrowService.ConfigureWebhookAsync(_escrowSettings);

            if (webhook?.Id is null)
            {
                var locale = await _localizationService.GetResourceAsync("Plugins.Payments.EscrowCom.AccountConfiguration.Failed");
                _notificationService.ErrorNotification(string.Format(locale, Url.Action("List", "Log")), false);
                return await Configure();
            }

            _escrowSettings.WebhookId = webhook.Id.Value;
        }

        //save settings if everything is ok
        _settingService.SaveSetting(_escrowSettings);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #endregion
}