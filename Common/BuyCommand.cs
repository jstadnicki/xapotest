using System;

namespace Common
{
    public class BuyCommand
    {
        public Guid PersonId { get; set; }
        public CurrencyType Currency { get; set; }
        public decimal Amount { get; set; }
        public static string QueueName => "BuyCommand";
    }
}