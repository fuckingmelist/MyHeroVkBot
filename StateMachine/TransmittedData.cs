using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyHeroVkBot.StateMachine
{
    public class TransmittedData
    {
        public string State { get; set; }
        public DataStorage Storage { get; set; }
        public long ChatId { get; }

        public TransmittedData(long chatId, string state)
        {
            ChatId = chatId;
            State = state;
            Storage = new DataStorage(chatId);
        }
    }
}
