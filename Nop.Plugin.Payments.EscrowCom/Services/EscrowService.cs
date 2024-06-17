using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.EscrowCom.Domain;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Orders;

namespace Nop.Plugin.Payments.EscrowCom.Services;
public class EscrowService
{
    #region Fields

    private readonly EscrowSettings _escrowComSettings;
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)
        }
    };

    private readonly ICustomerService _customerService;
    private readonly ILogger _logger;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IOrderService _orderService;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IProductService _productService;
    private readonly ISpecificationAttributeService _specificationAttributeService;

    #endregion

    #region Ctor

    public EscrowService(
        HttpClient httpClient,
        EscrowSettings escrowComSettings,
        ICustomerService customerService,
        ILogger logger,
        IGenericAttributeService genericAttributeService,
        IOrderService orderService,
        IOrderProcessingService orderProcessingService,
        IProductService productService,
        ISpecificationAttributeService specificationAttributeService)
    {
        _httpClient = httpClient;
        _escrowComSettings = escrowComSettings;
        _customerService = customerService;
        _logger = logger;
        _genericAttributeService = genericAttributeService;
        _orderService = orderService;
        _orderProcessingService = orderProcessingService;
        _productService = productService;
        _specificationAttributeService = specificationAttributeService;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Configure HTTP client for work 
    /// </summary>
    private void EnsureHttpClient()
    {
        if (!IsConfigured())
            throw new NopException("Escrow.com plugin is not configured");

        if (!_httpClient.DefaultRequestHeaders.Contains(HeaderNames.Authorization))
        {
            var auth = Encoding.ASCII.GetBytes($"{_escrowComSettings.Email}:{_escrowComSettings.ApiKey}");
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
    /// <exception cref="ArgumentNullException">It will be thrown if buyer is fee payer</exception>
    private PaymentFee[] GetFees(Customer customer)
    {
        if (customer == null && _escrowComSettings.FeePayer == FeePayer.Buyer)
            throw new ArgumentNullException(nameof(customer));

        return _escrowComSettings.FeePayer switch
        {
            FeePayer.Buyer => [new() { PayerCustomer = customer.Email, Type = _escrowComSettings.PaymentFeeType, Split = 1 }],
            FeePayer.Seller => [new() { PayerCustomer = "me", Type = _escrowComSettings.PaymentFeeType, Split = 1 }],
            FeePayer.Split => [
                new() { PayerCustomer = customer.Email, Type = _escrowComSettings.PaymentFeeType, Split = .5m },
                new() { PayerCustomer = "me", Type = _escrowComSettings.PaymentFeeType, Split = .5m }],
            _ => []
        };
    }

    #endregion

    /// <summary>
    /// Calling the Escrow Pay API
    /// </summary>
    /// <param name="order">An order</param>
    /// <returns>The URL to which the buyer will be redirected</returns>
    public async Task<string> CreateTransactionAsync(Order order, string returnUrl)
    {
        //products
        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
        var paymentItems = new List<PaymentItem>();

        var buyer = await _customerService.GetCustomerByIdAsync(order.CustomerId);


        foreach (var item in orderItems)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            var itemType = await _genericAttributeService.GetAttributeAsync<ItemType?>(product, EscrowDefaults.EscrowItemTypeAttribute)
                ?? ItemType.GeneralMerchandise;

            var attrs = await _specificationAttributeService.GetSpecificationAttributesByGroupIdAsync(_escrowComSettings.EscrowSpecGroupId);


            var extraAttributes = new Dictionary<string, string>();

            foreach (var attr in attrs)
            {

                var option = (await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(attr.Id))?.FirstOrDefault();

                if (option is null)
                    continue;

                var psa = (await _specificationAttributeService.GetProductSpecificationAttributesAsync(product.Id, specificationAttributeOptionId: option.Id))?.FirstOrDefault();

                if (string.IsNullOrEmpty(psa?.CustomValue))
                    continue;

                extraAttributes.Add(attr.Name, psa?.CustomValue);
            }


            paymentItems.Add(new()
            {
                Title = product.Name,
                Description = product.ShortDescription,
                Quantity = item.Quantity,
                Type = itemType,
                ExtraAttributes = extraAttributes,
                Schedule = [new()
                {
                    Amount = item.UnitPriceInclTax,
                    PayerCustomer = buyer.Email
                }],
                Fees = GetFees(buyer)
            });
        }

        //prepare request parameters
        var requestString = JsonSerializer.Serialize(new PaymentRequest
        {
            Description = $"Escrow Transaction for Order #{order.OrderGuid}",
            Reference = order.OrderGuid.ToString(),
            ReturnUrl = returnUrl,
            Items = paymentItems.ToArray(),
            Parties = new[]
            {
                new Party
                {
                    Customer = "me",
                    Role = PartyRole.Seller,
                    Initiator = true,
                },
                new Party
                {
                    Customer = buyer.Email,
                    Role = PartyRole.Buyer
                }
            }

        }, _serializerOptions);

        //execute request and get response
        var apiHost = _escrowComSettings.UseSandbox ? EscrowDefaults.SandboxApiHost : EscrowDefaults.ApiHost;

        var requestMessage = new HttpRequestMessage
        {
            RequestUri = new Uri($"{apiHost}{EscrowDefaults.PayPath}"),
            Method = HttpMethod.Post,
            Content = new StringContent(
                    requestString, Encoding.UTF8, MimeTypes.ApplicationJson)
        };

        EnsureHttpClient();

        var httpResponse = await _httpClient.SendAsync(requestMessage);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            var exeption = new HttpRequestException(responseContent, null, httpResponse.StatusCode);

            await _logger.ErrorAsync($"Escrow.com plugin: {httpResponse.ReasonPhrase}", exeption);

            return string.Empty;
        }

        //return result
        using var responseStream = await httpResponse.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<PaymentResponse>(responseStream, _serializerOptions);

        return result.LandingPage;
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
        var apiHost = _escrowComSettings.UseSandbox ? EscrowDefaults.SandboxApiHost : EscrowDefaults.ApiHost;


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
        var result = await JsonSerializer.DeserializeAsync<Transaction>(responseStream, _serializerOptions);

        if (!Guid.TryParse(result.Reference, out var orderGuid))
            return;

        var order = await _orderService.GetOrderByGuidAsync(orderGuid);

        try
        {
            var note = string.Empty;

            switch (eventResult.Event)
            {
                case WebhookTrigger.PartyVerificationApproved:
                    order.OrderStatus = OrderStatus.Processing;
                    await _orderService.UpdateOrderAsync(order);
                    note = "Escrow.com has approved the verification submission";
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
    /// Check whether the plugin is configured
    /// </summary>
    /// <param name="settings">Plugin settings</param>
    /// <returns>Result</returns>
    public bool IsConfigured()
    {
        //email and API key are required to request services
        return !string.IsNullOrEmpty(_escrowComSettings.Email) && !string.IsNullOrEmpty(_escrowComSettings.ApiKey);
    }
}
