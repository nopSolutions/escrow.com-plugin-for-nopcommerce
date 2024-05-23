using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.EscrowCom.Models;
using Nop.Services.Configuration;
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
    private readonly EscrowSettings _escrowSettings;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;

    #region Ctor

    public EscrowPaymentController(
        EscrowSettings escrowSettings,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService)
    {
        _escrowSettings = escrowSettings;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _settingService = settingService;
    }

    #endregion

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
            return AccessDeniedView();

        var model = new ConfigurationModel
        {
            Email = _escrowSettings.Email,
            ApiKey = _escrowSettings.ApiKey,
            UseSandbox = _escrowSettings.UseSandbox,
            Currency = _escrowSettings.Currency,
            FeePayer = _escrowSettings.FeePayer,
            InspectionPeriod = _escrowSettings.InspectionPeriod,
            PaymentItemType = _escrowSettings.PaymentItemType,
            PaymentFeeType = _escrowSettings.PaymentFeeType
        };

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
        _escrowSettings.Currency = model.Currency;
        _escrowSettings.FeePayer = model.FeePayer;
        _escrowSettings.InspectionPeriod = model.InspectionPeriod;
        _escrowSettings.PaymentFeeType = model.PaymentFeeType;
        _escrowSettings.PaymentItemType = model.PaymentItemType;

        _settingService.SaveSetting(_escrowSettings);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }
}
