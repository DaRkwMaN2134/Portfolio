using System;
using System.Collections.Generic;
using System.Text;

namespace ParserBot
{
    public class BotState
    {
        public long ChatId { get; set; }
        public bool IsAuthorized { get; set; }
        public int ActiveMenuMessageId { get; set; }
    }
}
