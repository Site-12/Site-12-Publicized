namespace Site12.API.Features.Other;

using System;
using CommandSystem;
using Exiled.API.Features;
using Extensions;
using UnityEngine;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class FacilityColor : ICommand
{
    public string Command => "fcolor";

    public string[] Aliases => ["FacilityColor"];

    public string Description => "FacilityColor";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=red>Invalid Arguments:\n> R G B\n> Reset";
        if (arguments.Count < 1) return false;
        if (arguments.At(0) == "reset")
        {
            foreach (var room in Room.List)
                room.ResetColor();
            response = "<color=green>Reset Lights Successfully";
            return true;
        }

        if (!float.TryParse(arguments.At(0), out var r) || !float.TryParse(arguments.At(1), out var g) || !float.TryParse(arguments.At(2), out var b))
        {
            response = "<color=red>One of the provided RGB values was incorrect! Follow format:\nfcolor [r] [g] [b]";
            return false;
        }

        var color = new Color(r / byte.MaxValue, g / byte.MaxValue, b / byte.MaxValue);

        foreach (var room in Room.List)
            room.Color = color;
        response = "<color=green>Light's have been set";
        return true;
    }
}