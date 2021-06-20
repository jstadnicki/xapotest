using MediatR;
using System.Collections.Generic;
using Front.Controllers;

namespace Front.Orders
{
    public class OrdersQuery : IRequest<List<Order>>
    {
    }
}