using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Payments.EscrowCom.Domain;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Payments;
using Nop.Services.Security;

namespace Nop.Plugin.Payments.EscrowCom.Services;
/// <summary>
/// Represents plugin event consumer
/// </summary>
public class EventConsumer :
    IConsumer<EntityUpdatedEvent<Product>>
{
    #region Fields

    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPaymentPluginManager _pluginManager;

    #endregion

    #region Ctor

    public EventConsumer(IGenericAttributeService genericAttributeService, IHttpContextAccessor httpContextAccessor, IPaymentPluginManager pluginManager)
    {
        _genericAttributeService = genericAttributeService;
        _httpContextAccessor = httpContextAccessor; 
        _pluginManager = pluginManager;
    }

    #endregion

    public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
    {
        //ensure that Avalara tax provider is active
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
}
