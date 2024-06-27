using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Net.Http.Headers;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.EscrowCom.Domain;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.EscrowCom.Services;

public class EscrowService
{
    #region Fields

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
        DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)
        }
    };

    private readonly CurrencySettings _currencySettings;
    private readonly EscrowSettings _escrowSettings;
    private readonly HttpClient _httpClient;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly ICurrencyService _currencyService;
    private readonly ICustomerService _customerService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger _logger;
    private readonly INopUrlHelper _nopUrlHelper;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IOrderService _orderService;
    private readonly IPictureService _pictureService;
    private readonly IProductService _productService;
    private readonly ISpecificationAttributeService _specificationAttributeService;
    private readonly IStoreService _storeService;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public EscrowService(CurrencySettings currencySettings,
        EscrowSettings escrowSettings,
        HttpClient httpClient,
        IActionContextAccessor actionContextAccessor,
        ICurrencyService currencyService,
        ICustomerService customerService,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        ILogger logger,
        INopUrlHelper nopUrlHelper,
        IOrderProcessingService orderProcessingService,
        IOrderService orderService,
        IPictureService pictureService,
        IProductService productService,
        ISpecificationAttributeService specificationAttributeService,
        IStoreService storeService,
        IUrlHelperFactory urlHelperFactory,
        IUrlRecordService urlRecordService,
        IWebHelper webHelper)
    {
        _currencySettings = currencySettings;
        _escrowSettings = escrowSettings;
        _httpClient = httpClient;
        _actionContextAccessor = actionContextAccessor;
        _currencyService = currencyService;
        _customerService = customerService;
        _genericAttributeService = genericAttributeService;
        _localizationService = localizationService;
        _logger = logger;
        _nopUrlHelper = nopUrlHelper;
        _orderProcessingService = orderProcessingService;
        _orderService = orderService;
        _pictureService = pictureService;
        _productService = productService;
        _specificationAttributeService = specificationAttributeService;
        _storeService = storeService;
        _urlHelperFactory = urlHelperFactory;
        _urlRecordService = urlRecordService;
        _webHelper = webHelper;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Fill the passed dictionary with extra attributes
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="specGroupId">The specification attribute group identifier</param>
    /// <param name="extraAttributes">The dictionary for extra attributes</param>
    /// A task that represents the asynchronous operation
    /// </returns>
    private async Task AddExtraAttributesAsync(int productId, int specGroupId, Dictionary<string, string> extraAttributes)
    {
        var attrs = await _specificationAttributeService.GetSpecificationAttributesByGroupIdAsync(specGroupId);
        foreach (var attr in attrs)
        {
            var option = (await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(attr.Id))?.FirstOrDefault();

            if (option is null)
                continue;

            var psa = (await _specificationAttributeService.GetProductSpecificationAttributesAsync(productId, specificationAttributeOptionId: option.Id))?.FirstOrDefault();

            if (string.IsNullOrEmpty(psa?.CustomValue))
                continue;

            extraAttributes.Add(attr.Name, psa?.CustomValue);
        }
    }

    /// <summary>
    /// Configure HTTP client for work 
    /// </summary>
    private void EnsureHttpClient(EscrowSettings escrowSettings = null)
    {
        escrowSettings ??= _escrowSettings;

        if (!_httpClient.DefaultRequestHeaders.Contains(HeaderNames.Authorization))
        {
            var auth = Encoding.ASCII.GetBytes($"{escrowSettings.Email}:{escrowSettings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(auth));
        }

        if (!_httpClient.DefaultRequestHeaders.Contains(HeaderNames.Accept))
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MimeTypes.ApplicationJson));
    }

    /// <summary>
    /// Get transaction fees according to current settings 
    /// </summary>
    /// <param name="customer">Buyer</param>
    /// <returns>Array of transaction fees</returns>
    private PaymentFee[] GetFees(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        return _escrowSettings.FeePayer switch
        {
            FeePayer.Buyer => [new() { PayerCustomer = customer.Email, Type = PaymentFeeType.Escrow, Split = 1 }],
            FeePayer.Seller => [new() { PayerCustomer = _escrowSettings.Email, Type = PaymentFeeType.Escrow, Split = 1 }],
            FeePayer.Split =>
            [
                new() { PayerCustomer = customer.Email, Type = PaymentFeeType.Escrow, Split = .5m },
                new() { PayerCustomer = _escrowSettings.Email, Type = PaymentFeeType.Escrow, Split = .5m }
            ],
            _ => []
        };
    }

    #endregion

    #region Methods

    /// <summary>
    /// Calling the Escrow Pay API
    /// </summary>
    /// <param name="order">An order</param>
    /// <param name="returnUrl">URL to redirect after successful payment in Escrow.com wizard</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the URL to which the buyer will be redirected
    /// </returns>
    public async Task<string> CreateTransactionAsync(Order order, string returnUrl)
    {
        try
        {
            if (!IsConfigured())
                throw new NopException("Escrow.com plugin is not configured");

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            var store = await _storeService.GetStoreByIdAsync(order.StoreId);
            var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);

            if (!Enum.TryParse(typeof(PaymentCurrency), currency.CurrencyCode, out _))
                throw new NopException($"Currency '{currency.CurrencyCode}' not supported");

            //products
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            var paymentItems = new List<TransactionItem>();
            foreach (var item in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                var seName = await _urlRecordService.GetSeNameAsync(product);
                var productUrl = await _nopUrlHelper.RouteGenericUrlAsync<Product>(new { SeName = seName }, _webHelper.GetCurrentRequestProtocol());
                var itemPicture = await _pictureService.GetProductPictureAsync(product, item.AttributesXml);
                var imageUrl = (await _pictureService.GetPictureUrlAsync(itemPicture)).Url;
                var itemType = await _genericAttributeService.GetAttributeAsync<ItemType?>(product, EscrowDefaults.EscrowItemTypeAttribute)
                    ?? ItemType.GeneralMerchandise;

                var extraAttributes = new Dictionary<string, string>()
                {
                    ["image_url"] = imageUrl,
                    ["merchant_url"] = productUrl
                };

                if (itemType == ItemType.DomainName)
                    await AddExtraAttributesAsync(product.Id, _escrowSettings.EscrowDomainNameSpecGroupId, extraAttributes);

                if (itemType == ItemType.MotorVehicle)
                    await AddExtraAttributesAsync(product.Id, _escrowSettings.EscrowVehicleSpecGroupId, extraAttributes);

                paymentItems.Add(new()
                {
                    Title = CommonHelper.EnsureMaximumLength(product.Name, 200),
                    Description = CommonHelper.EnsureMaximumLength(product.ShortDescription, 500),
                    Quantity = item.Quantity,
                    InspectionPeriod = _escrowSettings.InspectionPeriod * 86400,
                    Type = itemType,
                    ExtraAttributes = extraAttributes,
                    Schedule = [new()
                    {
                        Amount = item.PriceInclTax,
                        PayerCustomer = customer.Email,
                        BeneficiaryCustomer = _escrowSettings.Email
                    }],
                    Fees = GetFees(customer)
                });
            }

            //add shipping fee
            if (order.OrderShippingInclTax > decimal.Zero)
            {
                paymentItems.Add(new()
                {
                    Type = ItemType.ShippingFee,
                    InspectionPeriod = _escrowSettings.InspectionPeriod * 86400,
                    Schedule = [new()
                    {
                        Amount = order.OrderShippingInclTax,
                        PayerCustomer = customer.Email,
                        BeneficiaryCustomer = _escrowSettings.Email
                    }]
                });
            }

            //prepare request parameters
            var request = new PaymentRequest
            {
                Currency = currency.CurrencyCode.ToLower(),
                Description = CommonHelper.EnsureMaximumLength($"Transaction for order #{order.OrderGuid} in '{store.Name}'", 256),
                Reference = order.OrderGuid.ToString(),
                ReturnUrl = returnUrl,
                Items = paymentItems.ToArray(),
                Parties = new[]
                {
                    new Party
                    {
                        Customer = _escrowSettings.Email,
                        Role = PartyRole.Seller,
                        Initiator = true,
                        Agreed = true
                    },
                    new Party
                    {
                        Customer = customer.Email,
                        Role = PartyRole.Buyer,
                        Agreed = true
                    }
                }
            };

            //execute request and get response
            var apiHost = _escrowSettings.UseSandbox ? EscrowDefaults.ApiHost.Sandbox : EscrowDefaults.ApiHost.Production;

            var requestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri($"{apiHost}{EscrowDefaults.PayPath}"),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonSerializer.Serialize(request, _serializerOptions), Encoding.UTF8, MimeTypes.ApplicationJson)
            };

            EnsureHttpClient();

            var httpResponse = await _httpClient.SendAsync(requestMessage);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException(responseContent, null, httpResponse.StatusCode);
            }

            //return result
            using var responseStream = await httpResponse.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<PaymentResponse>(responseStream, _serializerOptions);

            return result.LandingPage;
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync($"{EscrowDefaults.SystemName} error: {exception.Message}", exception);

            return string.Empty;
        }
    }

    /// <summary>
    /// Create a webhook on escrow.com
    /// </summary>
    /// <param name="escrowSettings">Plugin settings</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains information about the created webhook
    /// </returns>
    public async Task<Webhook> ConfigureWebhookAsync(EscrowSettings escrowSettings)
    {
        EnsureHttpClient(escrowSettings);

        //execute request and get response
        var apiHost = escrowSettings.UseSandbox ? EscrowDefaults.ApiHost.Sandbox : EscrowDefaults.ApiHost.Production;

        //generate the relative URL
        var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
        var webhookUrl = urlHelper.RouteUrl(EscrowDefaults.WebhookRouteName, null, protocol: _webHelper.GetCurrentRequestProtocol());

        var requestMessage = new HttpRequestMessage
        {
            RequestUri = new Uri($"{apiHost}/2017-09-01/customer/me/webhook"),
            Method = HttpMethod.Post,
            Content = new StringContent(JsonSerializer.Serialize(new Webhook { Url = webhookUrl }, _serializerOptions), Encoding.UTF8, MimeTypes.ApplicationJson)
        };

        var httpResponse = await _httpClient.SendAsync(requestMessage);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            throw new HttpRequestException(responseContent, null, httpResponse.StatusCode);
        }

        //return result
        using var responseStream = await httpResponse.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<Webhook>(responseStream, _serializerOptions);

        return result;
    }

    /// <summary>
    /// Get escrow.com account information
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the Escrow account information
    /// </returns>
    public async Task<AccountInfo> GetAccountInfoAsync()
    {
        //ensure that plugin is configured
        if (!IsConfigured())
            return null;

        //execute request and get response
        var apiHost = _escrowSettings.UseSandbox ? EscrowDefaults.ApiHost.Sandbox : EscrowDefaults.ApiHost.Production;

        var requestMessage = new HttpRequestMessage
        {
            RequestUri = new Uri($"{apiHost}/2017-09-01/customer/me"),
            Method = HttpMethod.Get
        };

        EnsureHttpClient();

        var response = await _httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
            return null;

        //return result
        using var responseStream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<AccountInfo>(responseStream, _serializerOptions);

        return result;
    }


    /// <summary>
    /// Handle webhook request
    /// </summary>
    /// <param name="settings">Plugin settings</param>
    /// <param name="request">HTTP request</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleWebhookAsync(HttpRequest request)
    {
        //ensure that plugin is configured
        if (!IsConfigured())
            throw new NopException("Plugin not configured");

        using var reader = new StreamReader(request.Body);

        var body = await reader.ReadToEndAsync();
        var eventResult = JsonSerializer.Deserialize<WebhookEvent>(body, _serializerOptions);

        if (eventResult is null or { TransactionId: 0 })
            return;

        //execute request and get response
        var apiHost = _escrowSettings.UseSandbox ? EscrowDefaults.ApiHost.Sandbox : EscrowDefaults.ApiHost.Production;

        var requestMessage = new HttpRequestMessage
        {
            RequestUri = new Uri($"{apiHost}/2017-09-01/transaction/{eventResult.TransactionId}"),
            Method = HttpMethod.Get
        };

        EnsureHttpClient();

        var response = await _httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var exeption = new HttpRequestException(responseContent, null, response.StatusCode);

            await _logger.ErrorAsync($"Webhook handling Escrow.com plugin: {response.ReasonPhrase}", exeption);

            return;
        }

        //return result
        using var responseStream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<TransactionInfo>(responseStream, _serializerOptions);

        if (!Guid.TryParse(result.Reference, out var orderGuid))
            return;

        var order = await _orderService.GetOrderByGuidAsync(orderGuid);

        try
        {
            var note = string.Empty;

            switch (eventResult.Event)
            {
                case WebhookTrigger.CustomerVerificationApproved:
                    _escrowSettings.IsApprovedAccount = true;
                    break;
                case WebhookTrigger.PaymentApproved:
                    await _orderProcessingService.MarkOrderAsPaidAsync(order);
                    note = "Escrow.com has approved the payment for the transaction and the goods may now be shipped by the seller";
                    break;
                case WebhookTrigger.PaymentRejected:
                    await _orderProcessingService.CancelOrderAsync(order, true);
                    note = "Escrow.com has rejected the payment for the transaction";
                    break;
                case WebhookTrigger.PaymentReceived:
                    await _orderProcessingService.MarkAsAuthorizedAsync(order);
                    note = "Escrow.com has received payment from the buyer";
                    break;
                default:
                    return;
            }

            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = note,
                CreatedOnUtc = DateTime.UtcNow,
            });
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("Escrow.com webhook has been failed", ex);
        }
    }

    /// <summary>
    /// Remove linked webhook
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task RemoveWebhookAsync()
    {
        if (_escrowSettings.WebhookId == 0)
            return;

        if (!IsConfigured())
            throw new NopException("Escrow.com plugin is not configured");

        EnsureHttpClient();

        //execute request and get response
        var apiHost = _escrowSettings.UseSandbox ? EscrowDefaults.ApiHost.Sandbox : EscrowDefaults.ApiHost.Production;

        var requestMessage = new HttpRequestMessage
        {
            RequestUri = new Uri($"{apiHost}/2017-09-01/customer/me/webhook/{_escrowSettings.WebhookId}"),
            Method = HttpMethod.Delete
        };

        var httpResponse = await _httpClient.SendAsync(requestMessage);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            throw new HttpRequestException(responseContent, null, httpResponse.StatusCode);
        }
    }

    /// <summary>
    /// Check whether the plugin is configured
    /// </summary>
    /// <returns>Result</returns>
    public bool IsConfigured()
    {
        //email and API key are required to request services
        return !string.IsNullOrEmpty(_escrowSettings.Email) && !string.IsNullOrEmpty(_escrowSettings.ApiKey) && _escrowSettings.WebhookId != 0;
    }

    #endregion
}