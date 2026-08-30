using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Services.Abstraction;
using ECommerce.Shared.DTOs.BasketDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ECommerce.Presentation.Controllers
{
    [Authorize]
    public class PaymentsController : ApiBaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            ILogger<PaymentsController> logger
        )
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        // POST: baseUrl/api/payments/{basketId}

        [HttpPost("{BasketId}")]
        public async Task<ActionResult<BasketDTO>> CreateOrUpdatePaymentIntent(string BasketId)
        {
            if (!Guid.TryParse(BasketId, out _))
                return BadRequest("Invalid basket id.");

            var result = await _paymentService.CreateOrUpdatePaymentIntentAsync(BasketId);
            return HandleResult(result);
        }

        //Stripe calls this endpoint directly, so it cannot carry a user token.
        //It is authenticated instead by verifying the Stripe-Signature header.
        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> WebHook()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var stripeSignature = Request.Headers["Stripe-Signature"];

                await _paymentService.UpdateOrderPaymentStatus(json, stripeSignature!);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stripe webhook processing failed");
                return BadRequest();
            }
        }
    }
}
