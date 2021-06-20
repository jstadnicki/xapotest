using System;

namespace Common
{
    public class StoreCommand
    {
        public DateTime Created { get; set; }
        public decimal BtcAmount { get; set; }
        public decimal Amount { get; set; }
        public CurrencyType Currency { get; set; }
        public Guid PersonId { get; set; }
        public static string QueueName => "StoreCommand";
        public decimal Charge { get; set; }
    }
}