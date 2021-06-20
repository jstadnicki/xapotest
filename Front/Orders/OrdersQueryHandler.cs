using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Front.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Front.Orders
{
    public class OrdersQueryHandler : IRequestHandler<OrdersQuery, List<XapoTransaction>>
    {
        private readonly IConfiguration _configuration;

        public OrdersQueryHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<XapoTransaction>> Handle(OrdersQuery request, CancellationToken cancellationToken)
        {
            var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            var query = "SELECT [Created],[BtcAmount],[Amount],[Currency],[PersonId],[Charge],[Action] FROM[xapo].[dbo].[XapoTransaction]";
            var executeScalarAsync = await connection.QueryAsync<XapoTransaction>(query);
            return executeScalarAsync.ToList();
        }
    }

    public class XapoTransaction
    {
        public DateTime Created { get; set; }
        public decimal BtcAmount { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public Guid PersonId { get; set; }
        public decimal Charge { get; set; }
        public string Action { get; set; }
    }

    public class OrdersQueryOptions
    {
        public string User { get; set; }
        public string Pass { get; set; }
    }
}