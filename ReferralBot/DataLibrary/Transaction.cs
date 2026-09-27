using System;
using System.Collections.Generic;
using System.Text;

namespace DataLibrary
{
    public class Transaction
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public int Amount { get; set; }
        public TransactionReason Reason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
