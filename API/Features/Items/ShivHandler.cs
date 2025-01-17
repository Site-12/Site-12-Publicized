namespace Site12.API.Features.Items;

using System;
using Attributes;
using CommandSystem;
using CustomItems;
using Exiled.API.Enums;
using Exiled.Events.EventArgs.Player;
using Extensions;
using InventorySystem.Items;
using PluginAPI.Core;
using UnityEngine;

public sealed class ShivHandler : CustomItemHandler
{
    private ShivHandler()
    {
    }

    public CustomItemContainer Container { get; } = new();

    public override string Name => "Shiv";
    public override string[] Alias => ["knife"];

    public override void EnableEvents()
    {
        PlayerHandlers.UsingItem += UsedItem;
        PlayerHandlers.ChangingItem += ChangingItem;
    }

    public void ChangingItem(ChangingItemEventArgs ev)
    {
        if (ev.Item == null)
            return;

        if (!HasItem(ev.Item.Base))
            return;

        if (!Shiv.IsEnabled)
            return;

        ev.Player.ShowHint("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=27><mark=#367ce8><size=23>|🔪|</size></mark><mark=#595959>||</mark><mark=#393D4780> <size=23><space=2.6em><b>ꜱʜɪᴠ ᴍᴀᴅᴇ ᴏꜰ ᴄᴏɴᴄʀᴇᴛᴇ</size><space=2.6em><size=0.1>.</size></mark></size>");
    }

    public override bool HasItem(ushort serial) => Container.HasItem(serial);

    public override void ClearItems() =>   Container.ClearItems();

    public void UsedItem(UsingItemEventArgs ev)
    {
        if (!HasItem(ev.Item.Base))
            return;

        if (!Shiv.IsEnabled)
            return;

        ev.IsAllowed = false;

        if (!Physics.Raycast(new Ray(ev.Player.CameraTransform.position, ev.Player.CameraTransform.forward * 0.2f), out var hit, 5f))
            return;
        if (!ExPlayer.TryGet(hit.collider, out var victim))
            return;

        if (victim == ev.Player)
            return;

        ev.Player.RemoveHeldItem();

        if (victim.IsGodModeEnabled || victim.IsScp)
        {
            victim.ShowHint("Their shiv broke... nice....", 5f);
            ev.Player.ShowHint("I broke my shiv...", 5f);
            return;
        }

        foreach (var staff in ExPlayer.List)
            if (staff.RemoteAdminPermissions != 0)
            {
                staff.SendConsoleMessage(
                    "SCOMBAT Shiv Log:\nSTABBER NAME: " + ev.Player.Nickname + "\nSTABBER ROLE: " +
                    ev.Player.Role.Type + "\nSTABBER ID: " + ev.Player.Id + "\nSTABBER S-ID: " + ev.Player.UserId,
                    "yellow");
                staff.SendConsoleMessage(
                    "SCOMBAT Shiv Log:\nSTABBED NAME: " + victim.Nickname + "\nSTABBED ROLE: " +
                    victim.Role.Type + "\nSTABBED ID: " + victim.Id + "\nSTABBED S-ID: " +
                    victim.UserId, "red");
            }

        victim.EnableEffect(EffectType.Concussed, 255, 50);
        victim.EnableEffect(EffectType.Bleeding, 255, 50);
        victim.EnableEffect(EffectType.Blurred, 255, 50);
        victim.EnableEffect(EffectType.Disabled, 255, 50);
        victim.Hurt(35f, DamageType.Bleeding);
        victim.DropHeldItem(true);
        victim.ShowHint("I can't breathe... I've been stabbed", 5);
    }

    public override ItemBase GiveItem(ExPlayer player)
    {
        var item = player.AddItem(ItemType.Adrenaline);
        Container.RegisterItem(item.Base);
        return item.Base;
    }
}

public abstract class Shiv
{
    public static bool IsEnabled = true;

    [OnPluginEnabled]
    public static void InitEvents() => ServerHandlers.WaitingForPlayers += WaitingForPlayers;

    private static void WaitingForPlayers() => IsEnabled = true;
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class ShivsEnable : ICommand
{
    public string Command => "EnableShivs";
    public string[] Aliases => ["ShivsOn"];
    public string Description => "Enables Shivs...";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=red>Shiv's are already Enabled";
        if (Shiv.IsEnabled)
            return false;

        Shiv.IsEnabled = !Shiv.IsEnabled;

        response = "<color=green>Shiv's are now Enabled";
        return true;
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class ShivsDisable : ICommand
{
    public string Command => "ShivsMining";
    public string[] Aliases => ["ShivsOff"];
    public string Description => "Disables Shivs...";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=red>Shiv's are already Disabled";
        if (!Shiv.IsEnabled)
            return false;

        Shiv.IsEnabled = !Shiv.IsEnabled;

        response = "<color=green>Shiv's are now Disabled";
        return true;
    }
}

public abstract class Mining
{
    public static bool IsEnabled = true;

    [OnPluginEnabled]
    public static void InitEvents() => ServerHandlers.WaitingForPlayers += WaitingForPlayers;
    private static void WaitingForPlayers() => IsEnabled = true;
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class MiningEnable : ICommand
{
    public string Command => "EnableMining";
    public string[] Aliases => ["MiningOn"];
    public string Description => "Enables the Mining of Shivs...";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=red>Mining is already Enabled";
        if (Mining.IsEnabled)
            return false;

        Mining.IsEnabled = !Mining.IsEnabled;

        response = "<color=green>Mining is now Enabled";
        return true;
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class MiningDisable : ICommand
{
    public string Command => "DisableMining";
    public string[] Aliases => ["MiningOff"];
    public string Description => "Disables the Mining of Shivs...";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=red>Mining is already Disabled";
        if (!Mining.IsEnabled)
            return false;

        Mining.IsEnabled = !Mining.IsEnabled;

        response = "<color=green>Mining is now Disabled";
        return true;
    }
}

[CommandHandler(typeof(ClientCommandHandler))]
public class ShivClient : ICommand
{
    public string Command => "shiv";
    public string[] Aliases => ["mine"];
    public string Description => "Punches in the wall to try, to try and break off a piece.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var player = ExPlayer.Get(sender);

        response = "This feature is currently disabled!";
        if (!Mining.IsEnabled)
            return false;

        response = "<color=red>I can't do that...";
        if (player.IsCuffed)
            return false;

        var ray = new Ray(player.CameraTransform.position + (player.CameraTransform.forward * 0.1f), player.CameraTransform.forward);

        if (!Physics.Raycast(ray, out var hit, 2))
        {
            response = "<color=red>You must be at a wall";
            return false;
        }

        if (ExPlayer.TryGet(hit.collider, out var victim))
        {
            response = $"<color=red>You are disgusting, You did not hurt {victim.DisplayNickname}";
            return false;
        }

        response = "<color=red>I wouldn't be able to carry it anyways";
        if (player.InventoryFull())
            return false;

        if (URandom.Range(1, 100) <= 85)
        {
            player.ShowHint("<color=red><b>Damnit... fuck me");
            player.Health -= 10f;

            if (player.Health <= 0) player.Kill("Their hands are bloody, looks like they was chipping at the wall");
            response = "<color=red>Damnit... fuck me";
            return false;
        }

        CustomItemsManager.Get<ShivHandler>().GiveItem(player);

        response = "<color=green>Perfect...";
        return true;
    }
}