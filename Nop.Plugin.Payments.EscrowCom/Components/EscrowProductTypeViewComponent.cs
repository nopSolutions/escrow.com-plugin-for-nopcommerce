using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Payments.EscrowCom.Domain;
using Nop.Plugin.Payments.EscrowCom.Models;
using Nop.Services;
using Nop.Services.Common;
using Nop.Services.Payments;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.EscrowCom.Components;
public class EscrowProductTypeViewComponent : NopViewComponent
{
    #region Fields

    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IPaymentPluginManager _pluginManager;

    #endregion

    #region Ctor

    public EscrowProductTypeViewComponent(IGenericAttributeService genericAttributeService, IPaymentPluginManager pluginManager)
    {
        _genericAttributeService = genericAttributeService;
        _pluginManager = pluginManager;
    }

    #endregion

    /// <summary>
    /// Invoke the widget view component
    /// </summary>
    /// <param name="widgetZone">Widget zone</param>
    /// <param name="additionalData">Additional parameters</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the view component result
    /// </returns>
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        //ensure that model is passed
        if (additionalData is not BaseNopEntityModel entityModel || entityModel.Id == 0)
            return Content(string.Empty);

        //ensure that plugin is active
        if (!await _pluginManager.IsPluginActiveAsync(EscrowDefaults.SystemName))
            return Content(string.Empty);

        //ensure that it's a proper widget zone
        if (!widgetZone.Equals(AdminWidgetZones.ProductDetailsBlock))
            return Content(string.Empty);

        var itemType = await _genericAttributeService.GetAttributeAsync<Product, ItemType?>(entityModel.Id, EscrowDefaults.EscrowItemTypeAttribute)
            ?? ItemType.GeneralMerchandise;

        var model = new ItemTypeModel
        {
            EscrowItemType = itemType,
            AvailableItemTypes = (await ItemType.GeneralMerchandise.ToSelectListAsync()).ToList()
        };


        return View("~/Plugins/Payments.EscrowCom/Views/EscrowProductType/Default.cshtml", model);
    }
}
