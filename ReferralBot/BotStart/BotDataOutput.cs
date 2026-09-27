using ConfigurationLibrary;
using DataLibrary;
using DTOLibrary;
using Microsoft.EntityFrameworkCore;

using static ConfigurationLibrary.Interfaces;

namespace BotStart
{
    public class BotDataOutput: IBotWrite
    {
        private readonly AppDbContext _dbContext;

        public BotDataOutput(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task CheckOrCreateUserAsync(UserRegistrationDto dto)
        {
            var chatId = dto.ChatId;
 
            var user = await _dbContext.Users.FirstOrDefaultAsync(c => c.ChatId == chatId);
            if (user == null)
            {
                user = new User
                {
                    ChatId = dto.ChatId,
                    Username = dto.Username,
                    FirstName = dto.FirstName,
                    Points = 0,
                    PremiumUntil = null,
                    RegisteredAt = DateTime.UtcNow,
                    IsAdmin = false,
                    IsBlocked = false
                };
                _dbContext.Users.Add(user);
            }
            else
            {
                if (user.Username != dto.Username || user.FirstName != dto.FirstName)
                {
                    user.Username = dto.Username;
                    user.FirstName = dto.FirstName;
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<User?> GetUserAsync(long chatId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(c => c.ChatId == chatId);
            return user;
        }

        public async Task ConfirmReferralAsync(Referral referral)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(c => c.ChatId == referral.ReferrerId);
            if(user == null)
            {
                return;
            }
            user.Points += 10;
            referral.Status = ReferralStatus.Confirmed;
            referral.ConfirmedAt = DateTime.UtcNow;

            var transaction = new Transaction
            {
                UserId = user.ChatId,
                Amount = 10,
                Reason = TransactionReason.ReferralBonus,
                CreatedAt = DateTime.UtcNow
            };
            
            _dbContext.Transactions.Add(transaction);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Referral>> GetReferralsAsync(long chatId)
        {
            var referrerList = await _dbContext.Referrals
                .Where(r => r.ReferrerId == chatId)
                .Include(r => r.InvitedUser)
                .ToListAsync();
            return referrerList;
        }

        public async Task<Referral> GetReferrerAsync(long chatId, long referrerId)
        {
            var referral = await _dbContext.Referrals.FirstOrDefaultAsync(c => c.ReferralId == chatId);
            if (referral == null)
            {
                referral = new Referral
                {
                    ReferrerId = referrerId,
                    ReferralId = chatId,
                    Status = ReferralStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.Referrals.Add(referral);
                await _dbContext.SaveChangesAsync();
                return referral;
            }
            else
            {
                return referral;
            }
        }
    }
}
