using System;
using System.Collections.Generic;

namespace MyHeroVkBot.Models
{
    public partial class User
    {
        public User()
        {
            Cashes = new HashSet<Cash>();
            Heroes = new HashSet<Hero>();
            UserHeroes = new HashSet<UserHero>();
        }

        public long Id { get; set; }
        public bool IsAdmin { get; set; }
        public string State { get; set; } = null!;

        public virtual ICollection<Cash> Cashes { get; set; }
        public virtual ICollection<Hero> Heroes { get; set; }
        public virtual ICollection<UserHero> UserHeroes { get; set; }
    }
}
