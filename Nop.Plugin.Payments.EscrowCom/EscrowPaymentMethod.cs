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

    private readonly EscrowService _escrowComService;
    private readonly EscrowSettings _escrowSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;
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

    public EscrowPaymentMethod(
            EscrowService escrowComService,
            EscrowSettings escrowSettings,
            IHttpContextAccessor httpContextAccessor,
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
        _escrowComService = escrowComService;
        _escrowSettings = escrowSettings;
        _httpContextAccessor = httpContextAccessor;
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

    private async Task<int> CreateEscrowAttributesAsync()
    {
        var newGroup = new SpecificationAttributeGroup() { Name = EscrowDefaults.EscrowSpecificationAttributeGroupName };
        await _specificationAttributeGroupRepository.InsertAsync(newGroup);

        foreach (var name in EscrowDefaults.ExtraAttributeNames)
        {
            var newSpec = new SpecificationAttribute { Name = name, SpecificationAttributeGroupId = newGroup.Id };
            await _specificationAttributeRepository.InsertAsync(newSpec);

            await _specificationAttributeOptionRepository.InsertAsync(new SpecificationAttributeOption { Name = name, SpecificationAttributeId = newSpec.Id });
        }

        return newGroup.Id;
    }

    private async Task DeleteEscrowAttributesAsync(int groupId)
    {
        if (groupId == 0)
            return;

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

            redirectUrl = await _escrowComService.CreateTransactionAsync(postProcessPaymentRequest.Order, returnUrl);
        }

        //unsuccessful attempt
        if (string.IsNullOrEmpty(redirectUrl))
            redirectUrl = urlHelper.RouteUrl(EscrowDefaults.FailedRouteName);

        _httpContextAccessor.HttpContext?.Response.Redirect(redirectUrl);
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
        var notConfigured = !_escrowComService.IsConfigured();
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
        return Task.FromResult(false);
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
        var attrGroupId = await CreateEscrowAttributesAsync();

        //settings
        _escrowSettings.EscrowSpecGroupId = attrGroupId;
        _escrowSettings.UseSandbox = true;

        await _settingService.SaveSettingAsync(_escrowSettings);

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

        //locales
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Nop.Plugin.Payments.EscrowCom.Fields.ApiKey"] = "API key",
            ["Nop.Plugin.Payments.EscrowCom.Fields.ApiKey.Hint"] = "Escrow API key. API keys are specific to an environment, so you may not use a sandbox API key in production or a production API key in sandbox.",
            ["Nop.Plugin.Payments.EscrowCom.Fields.Email"] = "Email",
            ["Nop.Plugin.Payments.EscrowCom.Fields.Email.Hint"] = "Email address used on Escrow.com.",
            ["Nop.Plugin.Payments.EscrowCom.Fields.UseSandbox"] = "Use Sandbox",
            ["Nop.Plugin.Payments.EscrowCom.Fields.UseSandbox.Hint"] = "Check to enable Sandbox (testing environment).",
            ["Nop.Plugin.Payments.EscrowCom.Fields.Currency"] = "Currency",
            ["Nop.Plugin.Payments.EscrowCom.Fields.Currency.Hint"] = "The currency for the transaction.",
            ["Nop.Plugin.Payments.EscrowCom.Fields.InspectionPeriod"] = "Inspection period",
            ["Nop.Plugin.Payments.EscrowCom.Fields.InspectionPeriod.Hint"] = "The length of the inspection period in seconds. Currently the inspection period must be in whole multiples of days. e.g half a day (43200 seconds) is invalid where as 1 day (86400 seconds) and 2 days (172800 seconds) would be valid.",
            ["Nop.Plugin.Payments.EscrowCom.Fields.PaymentItemType"] = "Transaction item type",
            ["Nop.Plugin.Payments.EscrowCom.Fields.PaymentItemType.Hint"] = "The transaction item type - can affect behaviour of the transaction and can also be used to specify party-specific fees",
            ["Nop.Plugin.Payments.EscrowCom.Fields.PaymentFeeType"] = "Transaction fee type",
            ["Nop.Plugin.Payments.EscrowCom.Fields.PaymentFeeType.Hint"] = "Type of fee used for transactions",

            ["Nop.Plugin.Payments.EscrowCom.Fields.FeePayer"] = "Who will pay the fee?",
            ["Nop.Plugin.Payments.EscrowCom.Fields.FeePayer.Hint"] = "Choose the party who will pay the fee",
            ["Nop.Plugin.Payments.EscrowCom.PaymentMethodDescription"] = "You will be redirected to escrow.com to complete the order.",

            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentCurrency.AUD"] = "AUD",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentCurrency.CAD"] = "CAD",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentCurrency.EUR"] = "EUR",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentCurrency.GBP"] = "GBP",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentCurrency.USD"] = "USD",

            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.FeePayer.Buyer"] = "Buyer",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.FeePayer.Seller"] = "Seller",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.FeePayer.Split"] = "Will be paid in half",

            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PartyRole.Broker"] = "Broker",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PartyRole.Buyer"] = "Buyer",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PartyRole.Partner"] = "Partner",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PartyRole.Seller"] = "Seller",

            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentFeeType.Concierge"] = "Concierge",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentFeeType.CreditCard"] = "Credit card",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentFeeType.Disbursement"] = "Disbursement",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentFeeType.DomainNameHolding"] = "Domain name holding",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentFeeType.Escrow"] = "Escrow",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentFeeType.Intermediary"] = "Intermediary",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentFeeType.LienHolderPayoff"] = "Lien holder payoff",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentFeeType.MotorVehicle"] = "Motor vehicle",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentFeeType.Other"] = "Other",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentFeeType.TitleCollection"] = "Title collection service",

            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentItemType.BrokerFee"] = "Broker fee",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentItemType.DomainName"] = "Domain name",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentItemType.DomainNameHolding"] = "Domain name holding",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentItemType.GeneralMerchandise"] = "General merchandise",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentItemType.Milestone"] = "Milestone",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentItemType.MotorVehicle"] = "Motor vehicle",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentItemType.PartnerFee"] = "Partner fee",
            ["Enums.Nop.Plugin.Payments.EscrowCom.Domain.PaymentItemType.ShippingFee"] = "Shipping fee",
        });


        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        //settings
        if (_paymentSettings.ActivePaymentMethodSystemNames.Contains(EscrowDefaults.SystemName))
        {
            _paymentSettings.ActivePaymentMethodSystemNames.Remove(EscrowDefaults.SystemName);
            await _settingService.SaveSettingAsync(_paymentSettings);
        }

        await DeleteEscrowAttributesAsync(_escrowSettings.EscrowSpecGroupId);

        await _settingService.DeleteSettingAsync<EscrowSettings>();

        //settings
        if (_widgetSettings.ActiveWidgetSystemNames.Contains(EscrowDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(EscrowDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("Nop.Plugin.Payments.EscrowCom");
        await _localizationService.DeleteLocaleResourcesAsync("Enums.Nop.Plugin.Payments.EscrowCom");

        await base.UninstallAsync();
    }

    /// <summary>
    /// Gets a payment method description that will be displayed on checkout pages in the public store
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task<string> GetPaymentMethodDescriptionAsync()
    {
        return await _localizationService.GetResourceAsync("Nop.Plugin.Payments.EscrowCom.PaymentMethodDescription");
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