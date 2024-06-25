using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Payments.EscrowCom.Domain;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.EscrowCom.Models;

public record ItemTypeModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Payments.EscrowCom.ItemType")]
    public ItemType EscrowItemType { get; set; } = ItemType.GeneralMerchandise;

    public List<SelectListItem> AvailableItemTypes { get; set; } = new();
}