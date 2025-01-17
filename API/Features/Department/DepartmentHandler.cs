namespace Site12.API.Features.Department;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Attributes;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.Events.EventArgs.Player;
using Extensions;
using Other;
using PlayerRoles;
using UnityEngine;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class SetCustomRole : ICommand
{
    public string Command => "SetCustomRole";
    public string[] Aliases => ["scr"];
    public string Description => "Set's Yourself or a Player's Custom Role";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;
        if (!ExPlayer.TryGet(sender, out var player))
        {
            response = "<color=red>Only a player can run this command.";
            return false;
        }

        if (arguments.Count == 0 || arguments.At(0).ToLower() == "list")
        {
            sender.Respond("<color=green>Loading a list of all Roles...");
            foreach (var department in Department.DepartmentsData.Keys)
            {
                sender.Respond($"<color=orange>• {department}");
                foreach (var role in Department.GetAllRoles(department))
                    sender.Respond("<color=yellow> ° " + role.RoleName);
            }

            response = "<color=green>Completed";
            return true;
        }

        var intParse = arguments.At(0).GetBefore('.');
        if (!arguments.At(0).Contains("."))
            intParse = arguments.At(0);

        if (!int.TryParse(intParse, out _))
        {
            response = "<color=red>No Target Found.";
            if (!Physics.Raycast(new Ray(player.CameraTransform.position, player.CameraTransform.forward),
                    out var hit)) return false;
            if (!hit.collider.TryGetComponent(out HitboxIdentity comp)) return false;
            var target = comp.CharacterModel.OwnerHub;
            if (target == player.ReferenceHub)
                return false;
            // Get the arguments after the first one
            var additionalArguments =
                new ArraySegment<string>(arguments.Array!, arguments.Offset + 1, arguments.Count - 1);

            // Join the additional arguments into a single string if needed
            var additionalArgumentsString = string.Join(" ", additionalArguments);
            if (target.authManager.UserId.Contains("ID_Dummy"))
            {
                response = "<color=red>Something went wrong...";
                var role = Department.GetRole(additionalArgumentsString, arguments.At(1));

                var defaultRank = role?.Role.Ranks.FirstOrDefault(r => r.Value.Default);
                if (defaultRank == null)
                    return false;

                ExPlayer.Get(target).Role.Set(defaultRank.Value.Value.RoleTypeID);

                var random = new System.Random();
                var finalName = role.Role.Prefix;
                if (!string.IsNullOrEmpty(role.Role.Prefix))
                {
                    var index = random.Next(Name.LastNames.Count);
                    finalName = finalName.Replace("[L]", Name.LastNames[index]);

                    index = random.Next(Name.FirstNames.Count);
                    finalName = finalName.Replace("[F]", Name.FirstNames[index]);

                    finalName = finalName.Replace("[N]", random.Next(1, 100000).ToString().PadLeft(5, '0'));
                }

                ExPlayer.Get(target).DisplayNickname = finalName;
                ExPlayer.Get(target).CustomInfo = $"[- {defaultRank.Value.Key} -]\n{role.Role.CustomI}";
                ExPlayer.Get(target).Scale = Vector3.one * URandom.Range(0.9f, 1.1f);

                foreach (var item in defaultRank.Value.Value.LoadOut) BeginRoleplay.GetItem(item, out _).GiveItem(ExPlayer.Get(target));

                response = "<color=green>Set the role of the dummy.";
                return true;
            }
            if (!Department.DepartmentsData.ContainsKey(arguments.At(0)))
            {
                response = "<color=red>The Department you put does not exist.";
                return false;
            }
            if (!ExPlayer.Get(target)
                    .SetRole(additionalArgumentsString, arguments.At(0), RoleSpawnFlags.AssignInventory))
            {
                response = $"<color=red>Failed to set the role of {ExPlayer.Get(target).Nickname}";
                return false;
            }

            response = $"<color=green>Successfully set the role of {ExPlayer.Get(target).Nickname} to {additionalArgumentsString}";
            return true;
        }

        var split = arguments.At(0).Split('.');
        var players = new List<ExPlayer>();
        foreach (var obj in split)
        {
            if (!ExPlayer.TryGet(obj, out var plySub))
                continue;
            players.Add(plySub);
        }

        if (!Department.DepartmentsData.ContainsKey(arguments.At(1)))
        {
            response = $"<color=red>The Department you put {arguments.At(1)} does not exist.";
            return false;
        }

        var arg = new ArraySegment<string>(arguments.Array!, arguments.Offset + 2, arguments.Count - 2);

        // Join the additional arguments into a single string if needed
        var argsString = string.Join(" ", arg);

        foreach (var ply in players)
        {
            if (player.UserId.ToUpper().Contains("ID_DUMMY"))
            {
                response = "<color=red>Something went wrong...";
                var role = Department.GetRole(argsString, arguments.At(1));

                var defaultRank = role?.Role.Ranks.FirstOrDefault(r => r.Value.Default);
                if (defaultRank == null)
                    return false;

                ply.Role.Set(defaultRank.Value.Value.RoleTypeID);

                var random = new System.Random();
                var finalName = role.Role.Prefix;
                if (!string.IsNullOrEmpty(role.Role.Prefix))
                {
                    var index = random.Next(Name.LastNames.Count);
                    finalName = finalName.Replace("[L]", Name.LastNames[index]);

                    index = random.Next(Name.FirstNames.Count);
                    finalName = finalName.Replace("[F]", Name.FirstNames[index]);

                    finalName = finalName.Replace("[N]", random.Next(1, 100000).ToString().PadLeft(5, '0'));
                }

                ply.DisplayNickname = finalName;
                ply.CustomInfo = $"[- {defaultRank.Value.Key} -]\n{role.Role.CustomI}";
                ply.Scale = Vector3.one * URandom.Range(0.9f, 1.1f);

                foreach (var item in defaultRank.Value.Value.LoadOut) BeginRoleplay.GetItem(item, out _).GiveItem(ply);

                sender.Respond("<color=green>Set the role of a dummy.");
                continue;
            }
            if (!ply.SetRole(argsString, arguments.At(1), RoleSpawnFlags.AssignInventory))
            {
                sender.Respond($"<color=red>Failed to set the role of {ply.Nickname}");
                continue;
            }

            sender.Respond($"<color=green>Successfully set the role of {ply.Nickname} to {argsString}");
        }

        response = "<color=green>Task Completed...";
        return true;
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class SetCustomRank : ICommand
{
    public string Command => "SetCustomRank";
    public string[] Aliases => ["scrk"];
    public string Description => "Set's Yourself or a Player's Custom Rank";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;
        if (!ExPlayer.TryGet(sender, out var player))
        {
            response = "<color=red>Only a player can run this command.";
            return false;
        }

        if (arguments.Count >= 1 && arguments.At(0) == "me")
        {
            if(arguments.Count == 1)
            {
                response = "<color=red>Player is not playing any role.";
                if (player.ScomPlayer().CurrentRole.RoleEntry == null)
                    return false;
                sender.Respond("<color=green>Loading a list of all ranks available...");
                foreach (var rank in player.ScomPlayer().CurrentRole.RoleEntry.Role.Ranks) sender.Respond($"<color=orange>• {rank.Key}");

                response = "<color=green>Completed";
                return true;
            }

            var additionalArguments =
                new ArraySegment<string>(arguments.Array!, arguments.Offset + 1, arguments.Count - 1);

            // Join the additional arguments into a single string if needed
            var additionalArgumentsString = string.Join(" ", additionalArguments);
            // Join the additional arguments into a single string if needed
            if (!player.ScomPlayer().CurrentRole.SetRank(additionalArgumentsString))
            {
                response = $"<color=red>Failed to set your rank to {additionalArgumentsString}.";
                return false;
            }

            player.CustomInfo = $"[- {player.ScomPlayer().CurrentRole.RankName} -]\n{player.ScomPlayer().CurrentRole.RoleEntry.Role.CustomI}";

            if (Lobby.HasRoleplayStarted || !Lobby.IsRoleplay)
            {
                player.ClearAmmo();
                player.ClearInventory();

                foreach (var item in player.ScomPlayer().CurrentRole.Rank.LoadOut)
                {
                    BeginRoleplay.GetItem(item, out var cost, player).GiveItem(player);

                    if (!Lobby.IsRoleplay) continue;
                    Department.DepartmentsData[Department.GetDepartmentByRole(player.ScomPlayer().CurrentRole.RoleEntry)].Balance -= cost;

                    Department.UpdateDepartmentData(Department.GetDepartmentByRole(player.ScomPlayer().CurrentRole.RoleEntry));
                }
            }

            response = $"<color=green>Successfully set your rank to {additionalArgumentsString}.";
            return true;
        }

        response = "<color=red>No Target Found.";
        if (!Physics.Raycast(new Ray(player.CameraTransform.position, player.CameraTransform.forward),
                out var hit)) return false;
        if (!hit.collider.TryGetComponent(out HitboxIdentity comp)) return false;
        var target = ExPlayer.Get(comp.CharacterModel.OwnerHub);
        if (target == player)
            return false;
        if (target.UserId.Contains("ID_Dummy"))
            return false;

        if (arguments.Count == 0)
        {
            response = "<color=red>Player is not playing any role.";
            if (target.ScomPlayer().CurrentRole.RoleEntry == null)
                return false;
            sender.Respond("<color=green>Loading a list of all ranks available of the player you are looking at...");
            foreach (var rank in target.ScomPlayer().CurrentRole.RoleEntry.Role.Ranks) sender.Respond($"<color=orange>• {rank.Key}");

            response = "<color=green>Completed";
            return true;
        }

        // Join the additional arguments into a single string if needed
        var args = string.Join(" ", arguments);
        if (!target.ScomPlayer().CurrentRole.SetRank(args))
        {
            response = $"<color=red>Failed to set the rank of {target.Nickname} to {args}.";
            return false;
        }

        target.Role.Set(target.ScomPlayer().CurrentRole.Rank.RoleTypeID, SpawnReason.None, RoleSpawnFlags.None);
        target.CustomInfo = $"[- {target.ScomPlayer().CurrentRole.RankName} -]\n{target.ScomPlayer().CurrentRole.RoleEntry.Role.CustomI}";

        if (Lobby.HasRoleplayStarted || !Lobby.IsRoleplay)
        {
            target.ClearAmmo();
            target.ClearInventory();

            foreach (var item in target.ScomPlayer().CurrentRole.Rank.LoadOut)
            {
                BeginRoleplay.GetItem(item, out var cost, target).GiveItem(target);

                if (!Lobby.IsRoleplay) continue;
                var data = Department.DepartmentsData[Department.GetDepartmentByRole(target.ScomPlayer().CurrentRole.RoleEntry)];
                data.Balance -= cost;

                Department.UpdateDepartment(data.Department, data);
            }
        }

        response = $"<color=green>Successfully set the rank of {target.Nickname} to {args}.";
        return true;
    }
}

[CommandHandler(typeof(ClientCommandHandler))]
public class SetRank : ICommand
{
    public string Command => "SetRank";
    public string[] Aliases => ["RankSet", "Rank", "Rnk"];
    public string Description => "Set's your rank";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        response = "Feature is Disabled.";
        if (!Lobby.IsLobby)
            return false;

        var player = ExPlayer.Get(sender);

        response = "What was I doing...?";
        if (player.ScomPlayer().CurrentRole.RoleEntry == null)
            return false;

        if (arguments.Count == 0)
        {
            player.SendConsoleMessage("Loading a list of all ranks available.", "green");
            foreach (var rank in player.ScomPlayer().CurrentRole.RoleEntry.Role.Ranks) player.SendConsoleMessage($"• {rank.Key}", "yellow");

            response = "Completed";
            return true;
        }

        // Join the additional arguments into a single string if needed
        var args = string.Join(" ", arguments);
        if (!player.ScomPlayer().CurrentRole.RoleEntry.Role.Ranks.ContainsKey(args))
        {
            response = $"Failed to set the rank of {player.Nickname} to {args}.\nThe rank does not exist.";
            return false;
        }

        var setRank = player.ScomPlayer().CurrentRole.RoleEntry.Role.Ranks[args];

        if (setRank.RequiresRankInRoster)
        {
            var data = DataPlayer.GetPlayer(player.UserId);
            response = "You are required to join the department and have this rank or above.";

            if (!DataPlayer.IsPlayerInDepartment(player.UserId, Department.GetDepartmentByRole(player.ScomPlayer().CurrentRole.RoleEntry), out _)) return false;

            if (!data.DepartmentData[Department.GetDepartmentByRole(player.ScomPlayer().CurrentRole.RoleEntry)].Roles.TryGetValue(player.ScomPlayer().CurrentRole.RoleName, out var playerData)) return false;

            if (playerData < setRank.RankWeight && setRank.RequiresRankInRoster) return false;
        }

        if (!player.ScomPlayer().CurrentRole.SetRank(args))
        {
            response = $"Failed to set your rank to {args}.";
            return false;
        }

        player.Role.Set(player.ScomPlayer().CurrentRole.Rank.RoleTypeID, SpawnReason.None, RoleSpawnFlags.None);
        player.CustomInfo = $"[- {player.ScomPlayer().CurrentRole.RankName} -]\n{player.ScomPlayer().CurrentRole.RoleEntry.Role.CustomI}";

        response = $"Successfully set your rank to {args}.";
        return true;
    }
}

[CommandHandler(typeof(ClientCommandHandler))]
public class CheckBalance : ICommand
{
    public string Command => "Balance";
    public string[] Aliases => ["Wallet", "Bal", "Wal"];
    public string Description => "Checks how much money you have in your wallet.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        var player = ExPlayer.Get(sender);

        response = "You have DNT enabled in your settings, meaning you cannot use this feature, as it stores your data. (You can request to get it removed if it's currently stored).\nIf you enabled it just to see what happens, your data will not be automatically removed.";
        if (player.DoNotTrack)
            return false;

        response = $"Wallet: ${player.ScomPlayer().PlayerData.Wallet:N0}";
        return true;
    }
}

[CommandHandler(typeof(ClientCommandHandler))]
public class DepartmentLeaderboard : ICommand
{
    public string Command => "departmentleaderboard";
    public string[] Aliases => ["deptleaderboard", "deptlb", "departmentlb"];
    public string Description => "Gets a leaderboard of the departments with the most money.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        ExPlayer player = ExPlayer.Get(sender);

        response = "<color=white>Department Balance Leaderboard:";
        KeyValuePair<string, DepartmentInfo>[] kvps = [.. Department.DepartmentsData.OrderByDescending(x => x.Value.Balance)];
        for (var i = 0; i < kvps.Length; i++)
            if (DataPlayer.IsPlayerInDepartment(player.UserId, kvps[i].Key, out _))
                response += $"\n<b>{i + 1}. {kvps[i].Key}: ${kvps[i].Value.Balance:N0}</b>";
            else
                response += $"\n{i + 1}. {kvps[i].Key}: ${kvps[i].Value.Balance:N0}";

        return true;
    }
}

[CommandHandler(typeof(ClientCommandHandler))]
public class Leaderboard : ICommand
{
    public string Command => "Leaderboard";
    public string[] Aliases => ["baltop"];
    public string Description => "Gets a leaderboard of the richest Site-12 personnel.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        var user = ExPlayer.Get(sender);
        var userData = DataPlayer.GetPlayer(user.UserId);

        var sortedList = DataPlayer.PlayersData.Players.OrderByDescending(o => o.Wallet).ToArray();

        response = "<color=white>Site-12 Balance Leaderboard:";
        for (int i = 0; i < Math.Min(10, sortedList.Length); i++)
            response += ($"\n#{i + 1}. {sortedList[i].NickName}: ${sortedList[i].Wallet:N0}");
        response += $"\n...\n#{sortedList.IndexOf(userData) + 1}. YOU: ${userData.Wallet:N0}";
        return true;
    }
}

public class DepartmentHandler
{
    [OnPluginEnabled]
    public static void InitEvents()
    {
        PlayerHandlers.ChangingRole += ChangingRole;
        PlayerHandlers.Escaping += Escaping;
        // PlayerHandlers.Left += PlayerLeaving;
    }

    private static void Escaping(EscapingEventArgs ev)
    {
        if (!Lobby.IsRoleplay)
            return;
        ev.IsAllowed = false;
    }

    private static void ChangingRole(ChangingRoleEventArgs ev)
    {
        if (ev.Player.UserId.Contains("ID_Dummy"))
            return;
        if (!Lobby.IsRoleplay) return;
        if (ev.Reason is SpawnReason.None or SpawnReason.RoundStart) return;
        ev.Player.DisplayNickname = null;
        if (ev.Player.GameObject && ev.Player.ScomPlayer())
            ev.Player.ScomPlayer().CurrentRole.ClearRole();
        switch (ev.NewRole)
        {
            case RoleTypeId.Scientist:
                ev.Player.SetRole("Researcher", "Research", ev.SpawnFlags);
                break;
            case RoleTypeId.FacilityGuard:
                ev.Player.SetRole("Facility Guard", "Security", ev.SpawnFlags);
                break;
            case RoleTypeId.Scp173:
                ev.Player.SetRole("SCP 173", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.ClassD:
                ev.Player.SetRole("ClassD", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.Spectator:
                if(ev.Reason != SpawnReason.LateJoin)
                    ev.Player.SetRole("Spectator", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.Scp106:
                ev.Player.SetRole("SCP 106", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.NtfSpecialist:
                ev.Player.SetRole("Epsilon-11 Specialist", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.Scp049:
                ev.Player.SetRole("SCP 049", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.Scp079:
                ev.Player.SetRole("SCP 079", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.ChaosConscript:
                ev.Player.SetRole("Chaos Conscript", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.Scp096:
                ev.Player.SetRole("SCP 096", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.Scp0492:
                ev.Player.SetRole("SCP 049-2", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.NtfSergeant:
                ev.Player.SetRole("Epsilon-11 Sergeant", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.NtfCaptain:
                ev.Player.SetRole("Epsilon-11 Captain", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.NtfPrivate:
                ev.Player.SetRole("Epsilon-11 Private", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.Tutorial:
                ev.Player.SetRole("Tutorial", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.Scp939:
                ev.Player.SetRole("SCP 939", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.ChaosRifleman:
                ev.Player.SetRole("Chaos Rifleman", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.ChaosMarauder:
                ev.Player.SetRole("Chaos Marauder", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.ChaosRepressor:
                ev.Player.SetRole("Chaos Repressor", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.Overwatch:
                ev.Player.SetRole("Overwatch", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.Filmmaker:
                ev.Player.SetRole("Filmmaker", "Other", ev.SpawnFlags);
                break;
            case RoleTypeId.Scp3114:
                ev.Player.SetRole("SCP 3114", "Other", ev.SpawnFlags);
                break;
        }
    }

    // Is needed?
    // private static void PlayerLeaving(LeftEventArgs ev)
    // {
    //     if (!Lobby.IsRoleplay) return;
    //     if (ev.Player.ScomPlayer().CurrentRole.RoleEntry == null)
    //         return;
    //     var playerEarned = ev.Player.ScomPlayer().ElapsedTimeAsRole / 3;
    //     var data = Department.DepartmentsData[Department.GetDepartmentByRole(ev.Player.ScomPlayer().CurrentRole.RoleEntry)];
    //     data.Balance += playerEarned;
    //     
    //     Department.UpdateDepartment(Department.GetDepartmentByRole(ev.Player.ScomPlayer().CurrentRole.RoleEntry), data);
    //     
    //     if (!ev.Player.ScomPlayer().DoNotTrack)
    //         DataPlayer.SetWallet(ev.Player.UserId, DataPlayer.GetPlayer(ev.Player.UserId).Wallet += ev.Player.ScomPlayer().CurrentRole.Rank.AmountPaidPerRP);
    // }
}