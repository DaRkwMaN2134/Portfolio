using System;
using System.Collections.Generic;
using System.Text;

namespace DataLibrary
{
    public class Group
    {
        public int Id { get; set; }
        public long ChatId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int RequiredReferrals  { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<GroupAccess> GroupAccesses { get; set; } = new();
    }
}
