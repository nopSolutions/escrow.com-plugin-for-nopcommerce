using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Payments.EscrowCom.Domain;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Payments;

namespace Nop.Plugin.Payments.EscrowCom.Services;

/// <summary>
/// Represents plugin event consumer
/// </summary>
public class EventConsumer :
    IConsumer<EntityUpdatedEvent<Product>>,
    IConsumer<OrderStatusChangedEvent>
{
    #region Fields

    private readonly EscrowService _escrowService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPaymentPluginManager _pluginManager;

    #endregion

    #region Ctor

    public EventConsumer(EscrowService escrowService,
        IGenericAttributeService genericAttributeService,
        IHttpContextAccessor httpContextAccessor,
        IPaymentPluginManager pluginManager)
    {
        _escrowService = escrowService;
        _genericAttributeService = genericAttributeService;
        _httpContextAccessor = httpContextAccessor;
        _pluginManager = pluginManager;
    }

    #endregion

    /// <summary>
    /// Handle product updated event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
    {
        //ensure that plugin is active
        if (!await _pluginManager.IsPluginActiveAsync(EscrowDefaults.SystemName))
            return;

        //whether there is a form value for the entity use code
        var (keyExists, itemTypeValue) = await _httpContextAccessor.HttpContext.Request.TryGetFormValueAsync(EscrowDefaults.EscrowItemTypeAttribute);
        if (keyExists && !StringValues.IsNullOrEmpty(itemTypeValue) && Enum.TryParse(typeof(ItemType), itemTypeValue, out var itemType))
        {
            //save attribute
            await _genericAttributeService.SaveAttributeAsync(eventMessage.Entity, EscrowDefaults.EscrowItemTypeAttribute, itemType);
        }
    }

    /// <summary>
    /// Handle order status changed event event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(OrderStatusChangedEvent eventMessage)
    {
        //ensure that plugin is active
        if (!await _pluginManager.IsPluginActiveAsync(EscrowDefaults.SystemName))
            return;

        if (eventMessage.Order.OrderStatus == OrderStatus.Cancelled)
        {
            var existingTransaction = await _genericAttributeService.GetAttributeAsync<int>(eventMessage.Order, EscrowDefaults.EscrowTransactionIdAttribute);
            await _escrowService.CancelTransaction(existingTransaction);
        }
    }
}