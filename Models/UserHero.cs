using System;
using System.Collections.Generic;

namespace MyHeroVkBot.Models
{
    public partial class UserHero
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public int? HeroId { get; set; }

        public virtual Hero? Hero { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
