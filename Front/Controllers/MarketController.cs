using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Front.BuyCommand;
using Front.Orders;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Front.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MarketController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MarketController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Post(Guid personId, CurrencyType currency, decimal amount)
        {
            var request = new BuyRequest(personId, currency, amount);
            await _mediator.Send(request);
            return Ok();
        }
        
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<List<XapoTransaction>> Get()
        {
            var request = new OrdersQuery();
            var orders = await _mediator.Send(request);
            return orders;
        }
    }
}
