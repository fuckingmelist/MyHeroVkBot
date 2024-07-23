using MyHeroVkBot.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model;

namespace MyHeroVkBot.Services
{
    public class ServiceManager
    {
        private Dictionary<string, Func<Message, TransmittedData, BotMessage>>
      _methods;

        public ServiceManager()
        {
            MainService mainService = new MainService();

            _methods =
                new Dictionary<string, Func<Message, TransmittedData, BotMessage>>();

            _methods[States.WaitingCommandStart] = mainService.ProcessCommandStart;
            _methods[States.WaitingAction] = mainService.ProcessAction;
            _methods[States.WaitingPhoto] = mainService.ProcessPhoto;
            _methods[States.WaitingVideo] = mainService.ProcessVideo;
            _methods[States.WaitingAudio] = mainService.ProcessAudio;
            _methods[States.WaitingText] = mainService.ProcessText;
            _methods[States.WaitingAdminStart] = mainService.ProcessAdminStart;
            _methods[States.WaitingAdminActions] = mainService.ProcessAdminActions;
            _methods[States.WaitingAcceptingApplications] = mainService.ProcessAcceptingApplications;
            _methods[States.WaitingAcceptance] = mainService.ProcessAcceptance;
            _methods[States.WaitingReason] = mainService.ProcessReason;
            _methods[States.Waiting] = mainService.ProcessWaiting;
        }

        public BotMessage ProcessBotUpdate(Message msg, TransmittedData transmittedData)
        {
            Func<Message, TransmittedData, BotMessage> serviceMethod = _methods[transmittedData.State];
            return serviceMethod.Invoke(msg, transmittedData);
        }
    }
}
