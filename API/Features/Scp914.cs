namespace Site12.API.Features;

using System;
using CommandSystem;
using Exiled.Events.EventArgs.Scp914;
using MEC;
using Attributes;
using Exiled.API.Enums;
using Exiled.API.Features;
using Extensions;
using MapGeneration;

public static class Scp914
{
    public static bool IsEnabled;

    [OnPluginEnabled]
    public static void InitEvents()
    {
        ServerHandlers.WaitingForPlayers += WaitingForPlayers;
        Scp914Handlers.Activating += Using914;
    }

    public static void Using914(ActivatingEventArgs ev)
    {
        if (!IsEnabled)
            return;
        IsEnabled = false;
        Timing.CallDelayed(70, () => IsEnabled = true);
        Timing.CallDelayed(1, () =>
        {
            var audioPlayer = AudioPlayer.CreateOrGet("914alarm", destroyWhenAllClipsPlayed: true, controllerId: SpeakerExtensions.GetFreeId());
            audioPlayer.AddSpeaker("914Speaker", Room.Get(RoomType.Lcz914).Position, maxDistance: 6000, minDistance: 6000, isSpatial: false);
            audioPlayer.AddClip("Scp914alarm");
        });
    }

    private static void WaitingForPlayers() => IsEnabled = false;
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class Scp914AlarmEnable : ICommand
{
    public string Command => "Scp914AlarmOn";
    public string[] Aliases => ["Scp914AlarmEnable"];
    public string Description => "Enables the 914 Alarm...";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=red>SCP-914's Alarm is already Enabled";
        if (Scp914.IsEnabled)
            return false;

        Scp914.IsEnabled = !Scp914.IsEnabled;

        response = "<color=green>SCP-914's Alarm is now Enabled";
        return true;
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class Scp914AlarmDisable : ICommand
{
    public string Command => "Scp914AlarmOff";
    public string[] Aliases => ["Scp914AlarmDisable"];
    public string Description => "Disables the 914 Alarm...";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=red>SCP-914's Alarm is already Disabled";
        if (!Scp914.IsEnabled)
            return false;

        Scp914.IsEnabled = !Scp914.IsEnabled;

        response = "<color=green>SCP-914's Alarm is now Disabled";
        return true;
    }
}