using MediatR;
using System.Collections.Generic;

namespace Front.Orders
{
    public class OrdersQuery : IRequest<List<XapoTransaction>>
    {
    }
}