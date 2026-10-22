using System;
using System.Collections.Generic;
using System.Text;
using ShortUrl.Domain.Models;

namespace ShortUrl.Domain.Interfaces
{
    public interface IClickRepository
    {
        public Task CreateClickAsync(long linkid, Click currentclick);
        //public Task<Click?> GetClickAsync(int linkid); в разработке
    }
}
