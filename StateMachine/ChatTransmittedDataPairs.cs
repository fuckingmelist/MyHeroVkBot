using MyHeroVkBot.DbConnector;
using MyHeroVkBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHeroVkBot.StateMachine
{
    public class ChatTransmittedDataPairs
    {
        private Dictionary<long, TransmittedData> _chatTransmittedDataPairs;
        private my_hero_vk_bot_matt_dbContext _db = new my_hero_vk_bot_matt_dbContext();

        public ChatTransmittedDataPairs()
        {
            _chatTransmittedDataPairs = new Dictionary<long, TransmittedData>();

            List<User> users = _db.Users.ToList();

            if (users.Count != 0)
            {
                for (int i = 0; i < users.Count; i++)
                {
                    _chatTransmittedDataPairs.Add(users[i].Id, new TransmittedData(users[i].Id, users[i].State));
                }
            }
        }

        public bool ContainsKey(long chatId, string msgText)
        {
            if (msgText == "GetAdmin1017")
                return false;
            else
                return _chatTransmittedDataPairs.ContainsKey(chatId);
        }

        public void CreateNew(long chatId, string msgText)
        {
            User user = _db.Users.Where(x => x.Id == chatId).FirstOrDefault();

            if (user == null)
            {
                user = new User() { Id = chatId };

                if (msgText == "GetAdmin1017")
                {
                    _chatTransmittedDataPairs[chatId] = new TransmittedData(chatId, States.WaitingAdminStart);
                    user.State = States.WaitingAdminStart;
                    user.IsAdmin = true;
                }
                else
                {
                    _chatTransmittedDataPairs[chatId] = new TransmittedData(chatId, States.WaitingCommandStart);
                    user.State = States.WaitingCommandStart;
                    user.IsAdmin = false;
                }

                _db.Users.Add(user);
                _db.SaveChanges();
            }
            else if (msgText == "GetAdmin1017")
            {
                _chatTransmittedDataPairs[chatId] = new TransmittedData(chatId, States.WaitingAdminStart);
                user.State = States.WaitingAdminStart;
                user.IsAdmin = true;
                _db.Users.Update(user);
                _db.SaveChanges();
            }
        }

        public TransmittedData GetByChatId(long chatId)
        {
            return _chatTransmittedDataPairs[chatId];
        }
    }
}
