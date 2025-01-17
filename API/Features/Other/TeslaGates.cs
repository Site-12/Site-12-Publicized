namespace Site12.API.Features.Other;

using System;
using Attributes;
using CommandSystem;
using Extensions;

public static class TeslaGate
{
    public static bool IsEnabled = true;

    [OnPluginEnabled]
    public static void InitEvents()
    {
        ServerHandlers.WaitingForPlayers += WaitingForPlayers;
        PlayerHandlers.TriggeringTesla += TeslaGates;
    }

    private static void TeslaGates(Exiled.Events.EventArgs.Player.TriggeringTeslaEventArgs ev)
    {
        if(!IsEnabled)
            ev.IsAllowed = false;
    }

    private static void WaitingForPlayers() => IsEnabled = true;
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class TeslaGateEnable : ICommand
{
    public string Command => "TeslaGateOn";
    public string[] Aliases => ["EnableTeslaGate", "TeslaGateEnable"];
    public string Description => "Enables the Tesla Gate's...";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=red>Tesla Gate's are already Enabled";
        if (TeslaGate.IsEnabled)
            return false;

        TeslaGate.IsEnabled = !TeslaGate.IsEnabled;

        response = "<color=green>Tesla Gate's are now Enabled";
        return true;
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class TeslaGateDisable : ICommand
{
    public string Command => "TeslaGateOff";
    public string[] Aliases => ["DisableTeslaGate", "TeslaGateDisable"];
    public string Description => "Disables the Tesla Gate's...";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=red>Tesla Gate's are already Disabled";
        if (!TeslaGate.IsEnabled)
            return false;

        TeslaGate.IsEnabled = !TeslaGate.IsEnabled;

        response = "<color=green>Tesla Gate's are now Disabled";
        return true;
    }
}