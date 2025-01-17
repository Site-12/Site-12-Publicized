namespace Site12.API.Features.Other;

using System;
using System.Diagnostics.CodeAnalysis;
using Attributes;
using CommandSystem;
using Extensions;
using UnityEngine;

public abstract class Height
{
    public static bool IsEnabled;

    [OnPluginEnabled]
    public static void InitEvents() => ServerHandlers.WaitingForPlayers += WaitingForPlayers;

    private static void WaitingForPlayers() => IsEnabled = false;
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class HeightEnable : ICommand
{
    public string Command => "HeightEnable";
    public string[] Aliases => ["HeightOn", "OnHeight", "EnableHeight"];
    public string Description => "Enables the Height Client Command";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=red>Height Client Command is already Enabled";
        if (Height.IsEnabled)
            return false;

        Height.IsEnabled = !Height.IsEnabled;

        response = "<color=green>Height Client Command is now Enabled";
        return true;
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class HeightDisable : ICommand
{
    public string Command => "HeightDisable";
    public string[] Aliases => ["HeightOff", "OffHeight", "DisableHeight"];
    public string Description => "Disables the Height Client Command";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=red>Height Client Command is already Disabled";
        if (!Height.IsEnabled)
            return false;

        Height.IsEnabled = !Height.IsEnabled;

        response = "<color=green>Height Client Command is now Disabled";
        return true;
    }
}

[CommandHandler(typeof(ClientCommandHandler))]
public class HeightClient : ICommand
{
    public string Command => "Height";
    public string[] Aliases => ["height", "size"];
    public string Description => "Set a custom height between 165cm (5'5) and 201cm (6'7) : Height (New Height)";


    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!Height.IsEnabled)
        {
            response = "This feature is currently disabled!";
            return false;
        }

        var player = ExPlayer.Get((CommandSender)sender);

        if (arguments.Count == 0)
        {
            player.Scale = Vector3.one;
            response = "Height reset successfully!";
            return false;
        }

        float valueCm;
        if (arguments.At(0).Contains("'"))
        {
            var parts = arguments.At(0).Split('\'');
            if (!float.TryParse(parts[0], out var feet))
            {
                response = "Invalid height formatting for feet";
                return false;
            }
            if (!float.TryParse(parts[1], out var inches))
            {
                response = "Invalid height formatting for inches";
                return false;
            }
            valueCm = (feet * 30.48f) + (inches * 2.54f);
        }
        else if (!float.TryParse(arguments.At(0), out valueCm))
        {
            response = "Invalid height formatting for centimetres";
            return false;
        }
        var final = valueCm / 183f;
        final = Mathf.Clamp(final, 0.9f, 1.1f);

        response = "You are an SCP";
        if (player.IsScp)
            return false;
        player.Scale = Vector3.one * final;
        response = $"Height changed successfully to {Mathf.RoundToInt(final * 183f)}cm";
        return true;
    }
}
