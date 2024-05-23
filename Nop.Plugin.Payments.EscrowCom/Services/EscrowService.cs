using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.EscrowCom.Domain;
using Nop.Services.Catalog;
using Nop.Services.Customers;
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
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)
        }
    };

    private readonly ICustomerService _customerService;
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;

    #endregion

    #region Ctor

    public EscrowService(HttpClient httpClient, EscrowSettings escrowComSettings, ICustomerService customerService, IOrderService orderService, IProductService productService)
    {
        _httpClient = httpClient;
        _escrowComSettings = escrowComSettings;
        _customerService = customerService;
        _orderService = orderService;
        _productService = productService;
    }

    #endregion

    #region Utilities

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
    public async Task<string> CreateTransactionAsync(Order order)
    {
        //products
        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
        var paymentItems = new List<PaymentItem>();

        var buyer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

        foreach (var item in orderItems)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);

            paymentItems.Add(new()
            {
                Title = product.Name,
                Description = product.ShortDescription,
                Quantity = item.Quantity,
                Type = PaymentItemType.GeneralMerchandise,
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
            ReturnUrl = "https://localhost:5001",
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
        var requestMessage = new HttpRequestMessage
        {
            RequestUri = new Uri(_escrowComSettings.UseSandbox ? EscrowDefaults.SandboxEndpointUrl : EscrowDefaults.EndpointUrl),
            Method = HttpMethod.Post,
            Content = new StringContent(
                    requestString, Encoding.UTF8, MimeTypes.ApplicationJson)
        };

        var auth = Encoding.ASCII.GetBytes($"{_escrowComSettings.Email}:{_escrowComSettings.ApiKey}");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(auth));
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(MimeTypes.ApplicationJson));

        var httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();

        //return result
        using var responseStream = await httpResponse.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<PaymentResponse>(responseStream, _serializerOptions);

        return result.LandingPage;
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
