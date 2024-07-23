using MyHeroVkBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model;

namespace MyHeroVkBot.StateMachine
{
    public class ChatsRouter
    {
        private ChatTransmittedDataPairs _chatTransmittedDataPairs;
        private ServiceManager _servicesManager;

        public ChatsRouter()
        {
            _servicesManager = new ServiceManager();
            _chatTransmittedDataPairs = new ChatTransmittedDataPairs();
        }

        public BotMessage Route(long chatId, Message msg)
        {
            if (_chatTransmittedDataPairs.ContainsKey(chatId, msg.Text) == false)
            {
                _chatTransmittedDataPairs.CreateNew(chatId, msg.Text);
            }

            TransmittedData transmittedData = _chatTransmittedDataPairs.GetByChatId(chatId);

            Console.WriteLine($"РОУТЕР chatId = {chatId}; State = {transmittedData.State}");

            return _servicesManager.ProcessBotUpdate(msg, transmittedData);
        }
    }
}
