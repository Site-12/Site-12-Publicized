namespace Site12.API.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using Exiled.Events.EventArgs.Server;
using Attributes;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.Handlers;
using Extensions;

public abstract class SpawnWaves
{
    public static bool IsEnabled = true;

    [OnPluginEnabled]
    public static void InitEvents()
    {
        ServerHandlers.WaitingForPlayers += WaitingForPlayers;
        ServerHandlers.RespawningTeam += SpawnWave;
        Map.SpawningTeamVehicle += SpawningCar;
    }

    private static void SpawningCar(SpawningTeamVehicleEventArgs ev)
    {
        if(!IsEnabled)
            ev.IsAllowed = false;
    }

    private static void SpawnWave(RespawningTeamEventArgs ev)
    {
        if(!IsEnabled)
            ev.IsAllowed = false;
    }

    private static void WaitingForPlayers() => IsEnabled = true;
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class EnableSpawnWaves : ICommand
{
    public string Command => "EnableSpawnWaves";
    public string[] Aliases => ["SpawnWavesOn"];
    public string Description => "Enables Spawn Waves";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=red>Spawn Waves are already enabled";
        if (SpawnWaves.IsEnabled)
            return false;

        SpawnWaves.IsEnabled = !SpawnWaves.IsEnabled;

        response = "<color=green>Spawn Waves are now Enabled";
        return true;
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class DisableSpawnWaves : ICommand
{
    public string Command => "DisableSpawnWaves";
    public string[] Aliases => ["SpawnWavesOff"];
    public string Description => "Disables Spawn Waves";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=red>Spawn Waves are already disabled";
        if (!SpawnWaves.IsEnabled)
            return false;

        SpawnWaves.IsEnabled = !SpawnWaves.IsEnabled;

        response = "<color=green>Spawn Waves are now Disabled";
        return true;
    }
}