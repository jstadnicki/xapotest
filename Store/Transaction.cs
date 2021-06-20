using System;
using System.ComponentModel.DataAnnotations;

namespace Core
{
    internal class XapoTransaction
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public decimal BtcAmount { get; set; }
        public decimal Amount { get; set; }
        [Required]
        public string Currency { get; set; }
        public Guid PersonId { get; set; }
        public decimal Charge { get; set; }
        public TransactionAction Action { get; set; }
    }
}