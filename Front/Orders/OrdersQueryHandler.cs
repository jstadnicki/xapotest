using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Front.Controllers;
using MediatR;

namespace Front.Orders
{
    public class OrdersQueryHandler : IRequestHandler<OrdersQuery, List<Order>>
    {
        public Task<List<Order>> Handle(OrdersQuery request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}