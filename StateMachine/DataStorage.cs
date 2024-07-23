using MyHeroVkBot.DbConnector;
using MyHeroVkBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHeroVkBot.StateMachine
{
    public class DataStorage
    {
        private Dictionary<string, object> _data;
        private my_hero_vk_bot_matt_dbContext _db = new my_hero_vk_bot_matt_dbContext();

        public DataStorage(long chatId)
        {
            _data = new Dictionary<string, object>();

            List<Cash> cashes = _db.Cashes.Where(x => x.UserId == chatId).ToList();

            if (cashes != null)
            {
                for (int i = 0; i < cashes.Count; i++)
                {
                    _data[cashes[i].Key] = cashes[i].Value;
                }
            }

            Hero hero = _db.Heroes.Where(x => x.Id == _db.UserHeroes.Where(x => x.UserId == chatId).OrderBy(x => x.Id).Last().HeroId && x.Accepted == false).FirstOrDefault();

            if (hero != null)
            {
                if (hero.AudioId != null)
                    _data["Audio"] = hero.AudioId;

                if (hero.VideoId != null)
                    _data["Video"] = hero.VideoId;

                if (hero.PhotoId != null)
                    _data["Photo"] = hero.PhotoId;

                if (hero.AudioId != null)
                    _data["Text"] = hero.Text;
            }
        }

        public bool ContainsKey(string key)
        {
            return _data.ContainsKey(key);
        }

        public void AddOrUpdate(string key, object value, long chatId)
        {
            using (my_hero_vk_bot_matt_dbContext dbCash = new my_hero_vk_bot_matt_dbContext())
            {
                if (key != "Text" && key != "Audio" && key != "Video" && key != "Photo")
                {
                    Cash cash = dbCash.Cashes.Where(x => x.UserId == chatId && x.Key == key).FirstOrDefault();

                    if (cash == null)
                    {
                        dbCash.Cashes.Add(new Cash() { Value = long.Parse(value.ToString()), Key = key, UserId = chatId });
                        dbCash.SaveChanges();
                    }
                    else
                    {
                        cash.Value = long.Parse(value.ToString());
                        dbCash.Cashes.Update(cash);
                        dbCash.SaveChanges();
                    }
                }
                else
                {
                    Hero hero = dbCash.Heroes.Where(x => x.Id == dbCash.UserHeroes.Where(x => x.UserId == chatId).OrderBy(x => x.Id).Last().HeroId && x.Accepted == false).FirstOrDefault();

                    switch (key)
                    {
                        case "Text":
                            if (hero != null)
                            {
                                hero.Text = (string)value;
                                dbCash.Heroes.Update(hero);
                                dbCash.SaveChanges();
                            }
                            else
                            {
                                hero = new Hero();
                                hero.Text = (string)value;
                                dbCash.Heroes.Add(hero);
                                dbCash.SaveChanges();
                                hero.Id = dbCash.Heroes.OrderBy(x => x.Id).Last().Id;
                                dbCash.UserHeroes.Add(new UserHero() { HeroId = hero.Id, UserId = chatId });
                                dbCash.SaveChanges();
                            }
                            break;
                        case "Audio":
                            if (hero != null)
                            {
                                hero.AudioId = (long)value;
                                dbCash.Heroes.Update(hero);
                                dbCash.SaveChanges();
                            }
                            else
                            {
                                hero = new Hero();
                                hero.AudioId = (long)value;
                                dbCash.Heroes.Add(hero);
                                dbCash.SaveChanges();
                                hero.Id = dbCash.Heroes.OrderBy(x => x.Id).Last().Id;
                                dbCash.UserHeroes.Add(new UserHero() { HeroId = hero.Id, UserId = chatId });
                                dbCash.SaveChanges();
                            }
                            break;
                        case "Video":
                            if (hero != null)
                            {
                                hero.VideoId = (long)value;
                                dbCash.Heroes.Update(hero);
                                dbCash.SaveChanges();
                            }
                            else
                            {
                                hero = new Hero();
                                hero.VideoId = (long)value;
                                dbCash.Heroes.Add(hero);
                                dbCash.SaveChanges();
                                hero.Id = dbCash.Heroes.OrderBy(x => x.Id).Last().Id;
                                dbCash.UserHeroes.Add(new UserHero() { HeroId = hero.Id, UserId = chatId });
                                dbCash.SaveChanges();
                            }
                            break;
                        case "Photo":
                            if (hero != null)
                            {
                                hero.PhotoId = (long)value;
                                dbCash.Heroes.Update(hero);
                                dbCash.SaveChanges();
                            }
                            else
                            {
                                hero = new Hero();
                                hero.PhotoId = (long)value;
                                dbCash.Heroes.Add(hero);
                                dbCash.SaveChanges();
                                hero.Id = dbCash.Heroes.OrderBy(x => x.Id).Last().Id;
                                dbCash.UserHeroes.Add(new UserHero() { HeroId = hero.Id, UserId = chatId });
                                dbCash.SaveChanges();
                            }
                            break;
                    }
                }
            }

            _data[key] = value;
        }

        public void Delete(string key)
        {
            _data.Remove(key);
        }

        public void Clear(long chatId)
        {
            using (my_hero_vk_bot_matt_dbContext db = new my_hero_vk_bot_matt_dbContext())
            {
                List<Cash> cashes = db.Cashes.Where(x => x.UserId == chatId).ToList();

                for (int i = 0; i < cashes.Count; i++)
                {
                    using (my_hero_vk_bot_matt_dbContext dbClear = new my_hero_vk_bot_matt_dbContext())
                    {
                        dbClear.Cashes.Remove(cashes[i]);
                        dbClear.SaveChanges();
                    }
                }
            }

            _data.Clear();
        }

        public object Get(string key)
        {
            return _data[key];
        }
    }
}
