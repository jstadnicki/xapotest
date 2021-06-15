using System;
using MediatR;

namespace Front.BuyCommand
{
    public class BuyRequest : IRequest<Unit>
    {
        public Guid PersonId { get; }
        public CurrencyType Currency { get; }
        public decimal Amount { get; }

        public BuyRequest(Guid personId, CurrencyType currency, decimal amount)
        {
            PersonId = personId;
            Currency = currency;
            Amount = amount;
        }
    }
}