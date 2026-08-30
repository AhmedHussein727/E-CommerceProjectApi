using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Services.Abstraction;
using ECommerce.Shared.DTOs.BasketDTOs;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Presentation.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class BasketsController : ControllerBase
    {
        private readonly IBasketService _basketService;

        public BasketsController(IBasketService basketService)
        {
            _basketService = basketService;
        }

        //Basket ids are supplied by the client. Requiring a well-formed GUID keeps callers
        //from probing for other customers' baskets with guessable ids such as "1" or "basket01".
        private const string InvalidBasketId = "Invalid basket id.";

        //GET:BaseUrl/api/Baskets?id=Basket01

        [HttpGet]
        public async Task<ActionResult<BasketDTO>> GetBasket(string basketId)
        {
            if (!Guid.TryParse(basketId, out _))
                return BadRequest(InvalidBasketId);

            var basket = await _basketService.GetBasketAsync(basketId);
            return Ok(basket);
        }

        //POST:BaseUrl/api/Baskets
        [HttpPost]
        public async Task<ActionResult<BasketDTO>> CreateOrUpdateBasket(BasketDTO basket)
        {
            if (!Guid.TryParse(basket.Id, out _))
                return BadRequest(InvalidBasketId);

            var Basket = await _basketService.CreateOrUpdateBasketAsync(basket);
            return Ok(Basket);
        }

        //DELETE:BaseUrl/api/Baskets/Basket01

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteBasket([FromRoute] string id)
        {
            if (!Guid.TryParse(id, out _))
                return BadRequest(InvalidBasketId);

            var result = await _basketService.DeleteBasketAsync(id);
            return Ok(result);
        }
    }
}
