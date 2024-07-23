using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHeroVkBot.StateMachine
{
    public class States
    {
        public static string WaitingAdminStart { get; } = "WaitingAdminStart";
        public static string WaitingAcceptingApplications { get; } = "WaitingAcceptingApplications";
        public static string WaitingReason { get; } = "WaitingReason";
        public static string WaitingCommandStart { get; } = "WaitingCommandStart";
        public static string WaitingAction { get; } = "WaitingAction";
        public static string WaitingPhoto { get; } = "WaitingPhoto";
        public static string WaitingVideo { get; } = "WaitingVideo";
        public static string WaitingAudio { get; } = "WaitingAudio";
        public static string WaitingText { get; } = "WaitingText";
        public static string WaitingAdminActions { get; } = "WaitingAdminActions";
        public static string WaitingAcceptance { get; } = "WaitingAcceptance";
        public static string Waiting { get; } = "Waiting";
    }
}
