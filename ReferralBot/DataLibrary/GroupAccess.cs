using System;
using System.Collections.Generic;
using System.Text;

namespace DataLibrary
{
    public class GroupAccess
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
        public string InviteLink { get; set; } = string.Empty;
        public DateTime GrantedAt { get; set; }
    }
}
