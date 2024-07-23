using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model.Keyboard;

namespace MyHeroVkBot.StateMachine
{
    public class BotMessage
    {
        public string Text { get; }
        public MessageKeyboard Keyboard { get; }
        public IEnumerable<long> Array { get; set; }

        public BotMessage(string text, MessageKeyboard keyboard)
        {
            Text = text;
            Keyboard = keyboard;
        }
    }
}
