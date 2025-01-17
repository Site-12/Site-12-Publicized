namespace Site12.API.Features.Other;

using System;
using System.Collections.Generic;
using System.Linq;
using Attributes;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Extensions;
using VoiceChat;
using VoiceChat.Networking;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class ToggleScpChat : ICommand
{
    public string Command => "togglescpchat";
    public string[] Aliases => ["scpchat", "tsc"];
    public string Description => "Toggles the input player's ability to use SCP chat chat.";

    public static readonly HashSet<ExPlayer> ToggledPlayers = [];

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        ExPlayer player;

        if (arguments.Count == 0)
        {
            player = ExPlayer.Get(sender);

            if (player.IsScp)
            {
                response = "You are already an SCP.";
                return false;
            }

            if (!ToggledPlayers.Add(player))
            {
                ToggledPlayers.Remove(player);
                response = $"You have disabled SCP chat for yourself.";
            }
            else
                response = $"You have enabled SCP chat for yourself.";

            return true;
        }

        if (arguments.Count != 1)
        {
            response = "Invalid arguments. Usage: scpchat ?(id)";
            return false;
        }

        if (!ExPlayer.TryGet(arguments.At(0), out player))
        {
            response = $"Could not find player '{arguments.At(0)}'.";
            return false;
        }

        if (player.IsScp)
        {
            response = $"{player.Nickname} is already an SCP.";
            return false;
        }

        if (!ToggledPlayers.Add(player))
        {
            ToggledPlayers.Remove(player);
            response = $"You have disabled SCP chat for {player.Nickname}.";
        }
        else
            response = $"You have enabled SCP chat for {player.Nickname}.";

        return true;
    }

    [OnPluginEnabled]
    private static void InitEvents()
    {
        PlayerHandlers.VoiceChatting += VoiceChatting;
        ServerHandlers.WaitingForPlayers += WaitingForPlayers;
    }

    private static void VoiceChatting(VoiceChattingEventArgs ev)
    {
        VoiceMessage msgCopy = ev.VoiceMessage;

        if (ToggledPlayers.Contains(ev.Player) && !ev.Player.IsScp)
        {
            foreach (ExPlayer player in ExPlayer.List.Where(p => p != ev.Player && (p.IsScp || ToggledPlayers.Contains(p))))
            {
                msgCopy.Channel = VoiceChatChannel.RoundSummary; // ScpChat doesn't work for some reason
                player.ReferenceHub.connectionToClient.Send(msgCopy);
            }

            ev.IsAllowed = false;
            return;
        }

        if (msgCopy.Channel == VoiceChatChannel.ScpChat)
        {
            foreach (ExPlayer player in ToggledPlayers)
            {
                msgCopy.Channel = VoiceChatChannel.Intercom;
                player.ReferenceHub.connectionToClient.Send(msgCopy);
            }
        }
    }

    private static void WaitingForPlayers()
    {
        ToggledPlayers.Clear();
    }
}