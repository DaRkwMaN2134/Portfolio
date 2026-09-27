using DataLibrary;
using DTOLibrary;

namespace ConfigurationLibrary
{
    public class Interfaces
    {
        public interface IBotWrite
        {
            public Task CheckOrCreateUserAsync(UserRegistrationDto dto);
            public Task<User?> GetUserAsync(long chatId);
            public Task<List<Referral>> GetReferralsAsync(long chatId);
            public Task<Referral> GetReferrerAsync(long chatId, long referrerId);
            public Task ConfirmReferralAsync(Referral referral);
        }
    }
}
