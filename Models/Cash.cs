using System;
using System.Collections.Generic;

namespace MyHeroVkBot.Models
{
    public partial class Cash
    {
        public string Key { get; set; } = null!;
        public long Value { get; set; }
        public long UserId { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
