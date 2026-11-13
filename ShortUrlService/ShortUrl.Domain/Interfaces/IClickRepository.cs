using System;
using System.Collections.Generic;
using System.Text;
using ShortUrl.Domain.Models;

namespace ShortUrl.Domain.Interfaces
{
    public interface IClickRepository
    {
        public Task<long> CreateClickAsync(Click click);
        public Task<Click?> GetByIdAsync(long clickid);
        public Task UpdateClickAsync(Click currentclick);
        public Task<(int clickount, List<Click> lastclick, int uniqueip)> GetClickAsync(long linkid);
    }
}
