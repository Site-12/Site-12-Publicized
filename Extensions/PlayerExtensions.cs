namespace Site12.Extensions;

using API.Features;
using API.Features.Other;
using Exiled.API.Enums;
using InventorySystem.Items.Radio;
using MEC;
using PlayerRoles;
using PlayerRoles.Voice;
using API.Features.Department;
using UnityEngine;

public static class PlayerExtensions
{
    public static bool SetRole(this ExPlayer player, string roleName, string department = null, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.None)
    {
        var scomPlayer = player.ScomPlayer();
        var curRole = scomPlayer.CurrentRole;

        if (!curRole.SetRole(roleName, department))
            return false;

        player.Role.Set(curRole.Rank.RoleTypeID, SpawnReason.None, roleSpawnFlags.HasFlag(RoleSpawnFlags.UseSpawnpoint) ? RoleSpawnFlags.UseSpawnpoint : RoleSpawnFlags.None);
        var random = new System.Random();
        var finalName = curRole.RoleEntry.Role.Prefix;
        if (!string.IsNullOrEmpty(curRole.RoleEntry.Role.Prefix))
        {
            var index = random.Next(Name.LastNames.Count);
            finalName = finalName.Replace("[L]", Name.LastNames[index]);

            index = random.Next(Name.FirstNames.Count);
            finalName = finalName.Replace("[F]", Name.FirstNames[index]);

            finalName = finalName.Replace("[N]", random.Next(1, 100000).ToString().PadLeft(5, '0'));
        }

        player.DisplayNickname = finalName;
        player.CustomInfo = $"[- {curRole.RankName} -]\n{curRole.RoleEntry.Role.CustomI}";
        player.Scale = Vector3.one * URandom.Range(0.9f, 1.1f);

        if (roleSpawnFlags.HasFlag(RoleSpawnFlags.AssignInventory) && Lobby.HasRoleplayStarted || !Lobby.IsRoleplay && roleSpawnFlags.HasFlag(RoleSpawnFlags.AssignInventory))
        {
            player.ClearAmmo();
            player.ClearInventory();
            foreach (var item in curRole.Rank.LoadOut)
            {
                BeginRoleplay.GetItem(item, out var cost, player).GiveItem(player);
                if (!Lobby.IsRoleplay) continue;
                Department.DepartmentsData[Department.GetDepartmentByRole(curRole.RoleEntry)].Balance -= cost;

                Department.UpdateDepartmentData(Department.GetDepartmentByRole(curRole.RoleEntry));
            }
        }

        if (Lobby.HasRoleplayStarted)
            Timing.RunCoroutine(player.ScomPlayer().TrackHours());
        return true;
    }

    public static bool InventoryFull(this ExPlayer player) => player.Inventory.UserInventory.Items.Count >= 8;

    public static bool InventoryFull(this ReferenceHub player) => player.inventory.UserInventory.Items.Count >= 8;

    public static void SendScomMessage(this ExPlayer sender, ExPlayer receiver, string message)
    {
        if (sender.ScomPlayer().CurrentRole.RoleEntry == null)
        {
            sender.SendConsoleMessage("You aren't any Role Playing role...?", "red");
            return;
        }
        if (sender.ScomPlayer().CurrentRole.Rank == null)
        {
            sender.SendConsoleMessage($"You aren't any Rank somehow... what the fuck Code: {sender.ScomPlayer().CurrentRole.Rank}?", "red");
            return;
        }
        if (!sender.ScomPlayer().CurrentRole.Rank.HasPda)
        {
            sender.SendConsoleMessage("mmm...", "red");
            return;
        }

        if (!sender.ScomPlayer().ScomEnabled)
        {
            sender.SendConsoleMessage("> Your SCOM is deactivated, reactivate it to use it...", "red");
            return;
        }

        var unknown = sender.Role.Team != Team.ChaosInsurgency && sender.Role.Type != RoleTypeId.Scp079;

        sender.SendConsoleMessage("> Your SCOM message is being received please be patient...", "red");
        receiver.ReceiveScomMessage(sender, message, unknown);
    }

    public static void ReceiveScomMessage(this ExPlayer receiver, ExPlayer sender, string message, bool unknown)
    {
        var name = unknown ? sender.DisplayNickname : "Unknown";
        var id = unknown ? sender.Id : -1;

        if (receiver.ScomPlayer().CurrentRole.RoleEntry == null)
        {
            sender.SendConsoleMessage("> Unknown SCOM User", "red");
            return;
        }

        if (receiver.ScomPlayer().CurrentRole.Rank == null)
        {
            sender.SendConsoleMessage("> Unknown SCOM User", "red");
            return;
        }

        if (!receiver.ScomPlayer().CurrentRole.Rank.HasPda)
        {
            sender.SendConsoleMessage("> Unknown SCOM User", "red");
            return;
        }

        if (!receiver.ScomPlayer().ScomEnabled)
        {
            sender.SendConsoleMessage("> Their SCOM System is deactivated, you cannot message them until they have reactivated it...", "red");
            return;
        }

        receiver.SendConsoleMessage($"> Loading SCOM Message\n> Scom User: {name}\n> Scom User ID:{id}\n> (Moderation){sender.Id}\n> {message}", "white");
        const string attached = "You have one new unread message...";
        receiver.ShowHint($"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=27><mark=#367ce8><size=23>|🌐|</size></mark><mark=#595959>||</mark><mark=#393D4780> <size=23><space=2.6em><b>{attached.ReplaceLetters()}</size><space=2.6em><size=0.1>.</size></mark></size>");
        sender.SendConsoleMessage("> Your message has been sent, and received on their end...", "green");
    }

    public static bool HasRadio(this ExPlayer player, out RadioItem radio)
    {
        if (player.RoleManager.CurrentRole is IVoiceRole { VoiceModule: IRadioVoiceModule radioModule })
            return radioModule.RadioPlayback.TryGetUserRadio(out radio);
        radio = null;
        return false;
    }

    public static bool HasRadio(this ReferenceHub player, out RadioItem radio)
    {
        if (player.roleManager.CurrentRole is IVoiceRole { VoiceModule: IRadioVoiceModule radioModule })
            return radioModule.RadioPlayback.TryGetUserRadio(out radio);
        radio = null;
        return false;
    }
}