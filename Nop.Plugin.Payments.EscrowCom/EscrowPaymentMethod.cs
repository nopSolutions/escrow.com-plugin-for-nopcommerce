using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Data;
using Nop.Plugin.Payments.EscrowCom.Components;
using Nop.Plugin.Payments.EscrowCom.Services;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Payments.EscrowCom;

/// <summary>
/// Represents a payment method implementation
/// </summary>
public class EscrowPaymentMethod : BasePlugin, IPaymentMethod, IWidgetPlugin
{
    #region Fields

    private readonly EscrowService _escrowService;
    private readonly EscrowSettings _escrowSettings;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly ILocalizationService _localizationService;
    private readonly IRepository<SpecificationAttribute> _specificationAttributeRepository;
    private readonly IRepository<SpecificationAttributeGroup> _specificationAttributeGroupRepository;
    private readonly IRepository<SpecificationAttributeOption> _specificationAttributeOptionRepository;
    private readonly ISettingService _settingService;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IWebHelper _webHelper;
    private readonly PaymentSettings _paymentSettings;
    private readonly WidgetSettings _widgetSettings;

    #endregion

    #region Ctor

    public EscrowPaymentMethod(EscrowService escrowService,
        EscrowSettings escrowSettings,
        IActionContextAccessor actionContextAccessor,
        ILocalizationService localizationService,
        IRepository<SpecificationAttribute> specificationAttributeRepository,
        IRepository<SpecificationAttributeGroup> specificationAttributeGroupRepository,
        IRepository<SpecificationAttributeOption> specificationAttributeOptionRepository,
        ISettingService settingService,
        IUrlHelperFactory urlHelperFactory,
        IWebHelper webHelper,
        PaymentSettings paymentSettings,
        WidgetSettings widgetSettings)
    {
        _escrowService = escrowService;
        _escrowSettings = escrowSettings;
        _actionContextAccessor = actionContextAccessor;
        _localizationService = localizationService;
        _specificationAttributeRepository = specificationAttributeRepository;
        _specificationAttributeGroupRepository = specificationAttributeGroupRepository;
        _specificationAttributeOptionRepository = specificationAttributeOptionRepository;
        _settingService = settingService;
        _urlHelperFactory = urlHelperFactory;
        _webHelper = webHelper;
        _paymentSettings = paymentSettings;
        _widgetSettings = widgetSettings;
    }

    #endregion

    #region Utilities

    private async Task<int> CreateEscrowAttributesAsync(string groupName, Dictionary<string, string> specifications)
    {
        var group = new SpecificationAttributeGroup { Name = groupName };
        await _specificationAttributeGroupRepository.InsertAsync(group);

        foreach (var (name, title) in specifications)
        {
            var newSpec = new SpecificationAttribute { Name = title, SpecificationAttributeGroupId = group.Id };
            await _specificationAttributeRepository.InsertAsync(newSpec);

            //set 'Yes' option for boolean attributes and 'Sample text' for string attributes
            var optionName = name == "with_content" || name == "concierge" || name == "title_collection" || name == "lien_holder_payoff"
                ? "Yes"
                : "Sample text";
            await _specificationAttributeOptionRepository.InsertAsync(new SpecificationAttributeOption { Name = optionName, SpecificationAttributeId = newSpec.Id });
        }

        return group.Id;
    }

    private async Task DeleteEscrowAttributesAsync(int groupId)
    {
        var specGroup = await _specificationAttributeGroupRepository.GetByIdAsync(groupId);
        if (specGroup is null)
            return;

        await _specificationAttributeRepository.DeleteAsync(sa => sa.SpecificationAttributeGroupId == specGroup.Id);
        await _specificationAttributeGroupRepository.DeleteAsync(specGroup);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets widget zones where this widget should be rendered
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the widget zones
    /// </returns>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            AdminWidgetZones.ProductDetailsBlock
        });
    }

    /// <summary>
    /// Gets a type of a view component for displaying widget
    /// </summary>
    /// <param name="widgetZone">Name of the widget zone</param>
    /// <returns>View component type</returns>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone.Equals(AdminWidgetZones.ProductDetailsBlock))
            return typeof(EscrowProductTypeViewComponent);

        return null;
    }

    /// <summary>
    /// Process a payment
    /// </summary>
    /// <param name="processPaymentRequest">Payment info required for an order processing</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the process payment result
    /// </returns>
    public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        return Task.FromResult(new ProcessPaymentResult());
    }

    /// <summary>
    /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
    /// </summary>
    /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
    {
        var redirectUrl = string.Empty;
        var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

        if (postProcessPaymentRequest.Order != null)
        {
            var returnUrl = urlHelper
                .RouteUrl(EscrowDefaults.CompletedRouteName, new { orderId = postProcessPaymentRequest.Order.Id }, _webHelper.GetCurrentRequestProtocol());

            redirectUrl = await _escrowService.CreateTransactionAsync(postProcessPaymentRequest.Order, returnUrl);
        }

        //unsuccessful attempt
        if (string.IsNullOrEmpty(redirectUrl))
            redirectUrl = urlHelper.RouteUrl(EscrowDefaults.FailedRouteName, new { orderId = postProcessPaymentRequest.Order.Id }, _webHelper.GetCurrentRequestProtocol());

        _actionContextAccessor.ActionContext.HttpContext.Response.Redirect(redirectUrl);
    }

    /// <summary>
    /// Captures payment
    /// </summary>
    /// <param name="capturePaymentRequest">Capture payment request</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the capture payment result
    /// </returns>
    public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
    {
        return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
    }

    /// <summary>
    /// Voids a payment
    /// </summary>
    /// <param name="voidPaymentRequest">Request</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
    {
        return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
    }

    /// <summary>
    /// Refunds a payment
    /// </summary>
    /// <param name="refundPaymentRequest">Request</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
    {
        return Task.FromResult(new RefundPaymentResult { Errors = new[] { "Refund method not supported" } });
    }

    /// <summary>
    /// Process recurring payment
    /// </summary>
    /// <param name="processPaymentRequest">Payment info required for an order processing</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the process payment result
    /// </returns>
    public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
    }

    /// <summary>
    /// Cancels a recurring payment
    /// </summary>
    /// <param name="cancelPaymentRequest">Request</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
    {
        return Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
    }

    /// <summary>
    /// Returns a value indicating whether payment method should be hidden during checkout
    /// </summary>
    /// <param name="cart">Shoping cart</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains true - hide; false - display.
    /// </returns>
    public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
    {
        var notConfigured = !_escrowService.IsConfigured();
        return Task.FromResult(notConfigured);
    }

    /// <summary>
    /// Gets additional handling fee
    /// </summary>
    /// <param name="cart">Shoping cart</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the additional handling fee
    /// </returns>
    public Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
    {
        return Task.FromResult(decimal.Zero);
    }

    /// <summary>
    /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
    /// </summary>
    /// <param name="order">Order</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public Task<bool> CanRePostProcessPaymentAsync(Order order)
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Validate payment form
    /// </summary>
    /// <param name="form">The parsed form values</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the list of validating errors
    /// </returns>
    public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
    {
        ArgumentNullException.ThrowIfNull(form);

        return Task.FromResult<IList<string>>(new List<string>());
    }

    /// <summary>
    /// Get payment information
    /// </summary>
    /// <param name="form">The parsed form values</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the payment info holder
    /// </returns>
    public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
    {
        ArgumentNullException.ThrowIfNull(form);

        return Task.FromResult(new ProcessPaymentRequest());
    }

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl(EscrowDefaults.ConfigurationRouteName);
    }

    /// <summary>
    /// Gets a view component for displaying plugin in public store ("payment info" checkout step)
    /// </summary>
    public Type GetPublicViewComponent()
    {
        return typeof(EscrowPaymentInfoViewComponent);
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        var vehicleGroupId = await CreateEscrowAttributesAsync(EscrowDefaults.MotorVehicleExtraAttributes.Name, EscrowDefaults.MotorVehicleExtraAttributes.Attributes);
        var domainNameGroupId = await CreateEscrowAttributesAsync(EscrowDefaults.DomainExtraAttributes.Name, EscrowDefaults.DomainExtraAttributes.Attributes);

        await _settingService.SaveSettingAsync(new EscrowSettings
        {
            EscrowVehicleSpecGroupId = vehicleGroupId,
            EscrowDomainNameSpecGroupId = domainNameGroupId,
            FeePayer = Domain.FeePayer.Buyer,
            UseSandbox = true,
            InspectionPeriod = 1,
            IsApprovedAccount = false
        });

        if (!_paymentSettings.ActivePaymentMethodSystemNames.Contains(EscrowDefaults.SystemName))
        {
            _paymentSettings.ActivePaymentMethodSystemNames.Add(EscrowDefaults.SystemName);
            await _settingService.SaveSettingAsync(_paymentSettings);
        }

        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(EscrowDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(EscrowDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Payments.EscrowCom"] = "Escrow.com",
            ["Plugins.Payments.EscrowCom.AccountConfiguration.Failed"] = "Plugin configuration failed (see details in the <a href=\"{0}\" target=\"_blank\">log</a>)",
            ["Plugins.Payments.EscrowCom.Currency.Warning"] = "The <a href=\"{1}\" target=\"_blank\">primary store currency</a> ({0}) is not supported by Escrow.com. Currently the only currencies that are supported are USD, EUR, AUD, GBP, CAD.",
            ["Plugins.Payments.EscrowCom.Fields.ApiKey"] = "API key",
            ["Plugins.Payments.EscrowCom.Fields.ApiKey.Hint"] = "Escrow API key. API keys are specific to an environment, so you may not use a sandbox API key in production or a production API key in sandbox.",
            ["Plugins.Payments.EscrowCom.Fields.ApiKey.Required"] = "API key is required",
            ["Plugins.Payments.EscrowCom.Fields.Email"] = "Email",
            ["Plugins.Payments.EscrowCom.Fields.Email.Hint"] = "Email address used on Escrow.com.",
            ["Plugins.Payments.EscrowCom.Fields.Email.Required"] = "Email is required",
            ["Plugins.Payments.EscrowCom.Fields.UseSandbox"] = "Use Sandbox",
            ["Plugins.Payments.EscrowCom.Fields.UseSandbox.Hint"] = "Check to enable Sandbox (testing environment).",
            ["Plugins.Payments.EscrowCom.Fields.InspectionPeriod"] = "Inspection period",
            ["Plugins.Payments.EscrowCom.Fields.InspectionPeriod.Hint"] = "The length of the inspection period in days. Currently the inspection period must be in whole multiples of days, e.g. half a day is invalid where as 1 day and 2 days would be valid.",
            ["Plugins.Payments.EscrowCom.Fields.InspectionPeriod.Invalid"] = "Inspection period should be in range 1 to 30",
            ["Plugins.Payments.EscrowCom.Fields.FeePayer"] = "Who will pay the fee?",
            ["Plugins.Payments.EscrowCom.Fields.FeePayer.Hint"] = "Choose the party who will pay the fee.",
            ["Plugins.Payments.EscrowCom.Fields.Verification"] = "Verification status",
            ["Plugins.Payments.EscrowCom.Fields.Verification.Hint"] = "The KYC verification status on Escrow.com.",
            ["Plugins.Payments.EscrowCom.ItemType"] = "Escrow item type",
            ["Plugins.Payments.EscrowCom.ItemType.Hint"] = "The item type - can affect behaviour of the transaction and can also be used to specify party-specific fees.",
            ["Plugins.Payments.EscrowCom.PaymentMethodDescription"] = "You will be redirected to Escrow.com to complete the order.",

            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.FeePayer.Buyer"] = "Buyer",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.FeePayer.Seller"] = "Seller",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.FeePayer.Split"] = "Will be paid in half",

            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.ItemType.GeneralMerchandise"] = "General merchandise",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.ItemType.MotorVehicle"] = "Motor vehicle",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.ItemType.DomainName"] = "Domain name",

            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.VerificationStatus.Verified"] = "Verified",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.VerificationStatus.NotVerified"] = "Not verified",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.VerificationStatus.NotRequired"] = "Not required",
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        if (_paymentSettings.ActivePaymentMethodSystemNames.Contains(EscrowDefaults.SystemName))
        {
            _paymentSettings.ActivePaymentMethodSystemNames.Remove(EscrowDefaults.SystemName);
            await _settingService.SaveSettingAsync(_paymentSettings);
        }

        await _escrowService.RemoveWebhookAsync();

        await DeleteEscrowAttributesAsync(_escrowSettings.EscrowVehicleSpecGroupId);
        await DeleteEscrowAttributesAsync(_escrowSettings.EscrowDomainNameSpecGroupId);

        await _settingService.DeleteSettingAsync<EscrowSettings>();

        if (_widgetSettings.ActiveWidgetSystemNames.Contains(EscrowDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(EscrowDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.EscrowCom");
        await _localizationService.DeleteLocaleResourcesAsync("Enums.Nop.Plugin.Payments.EscrowCom");

        await base.UninstallAsync();
    }

    /// <summary>
    /// Gets a payment method description that will be displayed on checkout pages in the public store
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task<string> GetPaymentMethodDescriptionAsync()
    {
        return await _localizationService.GetResourceAsync("Plugins.Payments.EscrowCom.PaymentMethodDescription");
    }

    #endregion

    #region Properies

    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
    /// </summary>
    public bool HideInWidgetList => true;

    /// <summary>
    /// Gets a value indicating whether capture is supported
    /// </summary>
    public bool SupportCapture => false;

    /// <summary>
    /// Gets a value indicating whether partial refund is supported
    /// </summary>
    public bool SupportPartiallyRefund => false;

    /// <summary>
    /// Gets a value indicating whether refund is supported
    /// </summary>
    public bool SupportRefund => false;

    /// <summary>
    /// Gets a value indicating whether void is supported
    /// </summary>
    public bool SupportVoid => false;

    /// <summary>
    /// Gets a recurring payment type of payment method
    /// </summary>
    public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

    /// <summary>
    /// Gets a payment method type
    /// </summary>
    public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

    /// <summary>
    /// Gets a value indicating whether we should display a payment information page for this plugin
    /// </summary>
    public bool SkipPaymentInfo => false;

    #endregion
}