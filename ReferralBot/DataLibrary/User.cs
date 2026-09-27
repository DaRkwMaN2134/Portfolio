
namespace DataLibrary
{
    public class User
    {
        public long ChatId { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public int Points { get; set; }
        public DateTime? PremiumUntil { get; set; }
        public List<Referral> ReferralsAsReferrer { get; set; } = new();
        public Referral? ReferralRecord { get; set; }
        public DateTime RegisteredAt { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsBlocked { get; set; }
        public List<Transaction> Transactions { get; set; } = new();
        public List<GroupAccess> GroupAccesses { get; set; } = new();
    }
}
