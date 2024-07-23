using System;
using System.Collections.Generic;

namespace MyHeroVkBot.Models
{
    public partial class Hero
    {
        public Hero()
        {
            UserHeroes = new HashSet<UserHero>();
        }

        public int Id { get; set; }
        public long? AudioId { get; set; }
        public long? VideoId { get; set; }
        public long? PhotoId { get; set; }
        public string? Text { get; set; }
        public bool Accepted { get; set; }
        public long? CheckingUserId { get; set; }

        public virtual User? CheckingUser { get; set; }
        public virtual ICollection<UserHero> UserHeroes { get; set; }
    }
}
