using System.Threading;
using System.Threading.Tasks;
using MediatR;
using XapoWrappers;

namespace Front.BuyCommand
{
    public class BuyRequestHandler : IRequestHandler<BuyRequest, Unit>
    {
        private readonly IXapoQueue _queue;

        public BuyRequestHandler(IXapoQueue queue)
        {
            _queue = queue;
        }

        public Task<Unit> Handle(BuyRequest request, CancellationToken cancellationToken)
        {
            var bc = new Common.BuyCommand
            {
                Amount = request.Amount,
                Currency = request.Currency,
                PersonId = request.PersonId
            };

            _queue.Send(bc, Common.BuyCommand.QueueName);
            return Task.FromResult(Unit.Value);
        }
    }
}