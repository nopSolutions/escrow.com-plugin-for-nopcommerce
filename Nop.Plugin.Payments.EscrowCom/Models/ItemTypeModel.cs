using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Payments.EscrowCom.Domain;

namespace Nop.Plugin.Payments.EscrowCom.Models;
public record ItemTypeModel
{
    public ItemType EscrowItemType { get; set; } = ItemType.GeneralMerchandise;
    public List<SelectListItem> AvailableItemTypes { get; set; } = new();
}
