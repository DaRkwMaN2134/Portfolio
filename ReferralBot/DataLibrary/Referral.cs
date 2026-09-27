using System;
using System.Collections.Generic;
using System.Text;

namespace DataLibrary
{
    public class Referral
    {
        public int Id { get; set; }
        public long ReferrerId { get; set; }
        public User Referrer { get; set; } 
        public long ReferralId { get; set; }
        public User InvitedUser { get; set; }
        public ReferralStatus Status { get; set; }
        public string SubId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
    }
}
