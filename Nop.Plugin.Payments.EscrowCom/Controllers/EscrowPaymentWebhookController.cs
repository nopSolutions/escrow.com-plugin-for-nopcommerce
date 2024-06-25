using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.EscrowCom.Services;

namespace Nop.Plugin.Payments.EscrowCom.Controllers;

public class EscrowPaymentWebhookController : Controller
{

    #region Fields

    private readonly EscrowService _escrowService;

    #endregion

    #region Ctor

    public EscrowPaymentWebhookController(EscrowService escrowService)
    {
        _escrowService = escrowService;
    }

    #endregion

    #region Methods

    [HttpPost]
    public async Task<IActionResult> WebhookHandler()
    {
        await _escrowService.HandleWebhookAsync(Request);
        return Ok();
    }

    #endregion
}