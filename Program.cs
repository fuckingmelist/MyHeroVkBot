using MyHeroVkBot.StateMachine;
using System;
using System.Diagnostics;
using VkBotFramework;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;

VkApi api = new VkApi();
Random rnd = new Random();
ChatsRouter _chatsRouter = new ChatsRouter();

string _accessToken = "vk1.a.993a-LO02EGtXdMLe17AkU2DK1gEyZ_qOEt49QU_Kd3Yhbvc5VzaA6vWCfPTSaBLcHwVty2_6hYsrLb0MwSQe2BlVS7KNNpOpVybHYOeN5ODrYdDYg1wuE8I9uf1CUWDILICXnSGNdzURfB0J2EKpZ5fqr42LDt6Py0k_h_UXzJwp3zKpVRCUjMytFK7zlnzWzmQRWq8sOl6_kROxQsMww";

api.Authorize(new ApiAuthParams
{
    AccessToken = _accessToken
});

Console.WriteLine("Bot started");

void SendMessage(long? id, IEnumerable<long> array, string message, MessageKeyboard keyboard)
{
    api.Authorize(new ApiAuthParams
    {
        AccessToken = _accessToken
    });

    if (array != null)
    {
        api.Messages.Send(new MessagesSendParams
        {
            RandomId = rnd.Next(0, 1000000000),
            UserId = id,
            Message = message,
            Keyboard = keyboard,
            ForwardMessages = array
        });
    }
    else
    {
        api.Messages.Send(new MessagesSendParams
        {
            RandomId = rnd.Next(0, 1000000000),
            UserId = id,
            Message = message,
            Keyboard = keyboard
        });
    }
}

while (true)
{
    try
    {
        var conv = api.Messages.GetConversations(new GetConversationsParams() { Filter = GetConversationFilter.Unread }).Items;

        int count = conv.Count;

        for (int i = 0; i < count; i++)
        {
            if (conv[i].Conversation.Peer.Id != 2000000001)
            {
                Message message = conv[i].LastMessage;
                Console.WriteLine(message.Text);

                BotMessage botMessage = _chatsRouter.Route((long)message.FromId, message);

                SendMessage(message.FromId, botMessage.Array, botMessage.Text, botMessage.Keyboard);
                Thread.Sleep(100);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}




