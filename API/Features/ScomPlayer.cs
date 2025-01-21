namespace Site12.API.Features;

using System.Collections.Generic;
using System.Linq;
using Attributes;
using Exiled.Events.EventArgs.Player;
using MEC;
// using Health;
using UnityEngine;
using Department;
using Dept = Department.Department;
using CustomPlayerEffects;
using Exiled.API.Features;
using Extensions;
using PlayerRoles;
using NorthwoodLib.Pools;
using PlayerRoles.PlayableScps.Scp173;

public class ScomPlayer : MonoBehaviour
{
    public ExPlayer Player { get; set; }
    public PlayerRole CurrentRole { get; set; } = new();
    public PlayerData PlayerData { get; private set; }
    public AudioPlayer AudioPlayer { get; set; }
    public bool ScomEnabled { get; set; } = true;
    public float ElapsedTimeAsRole { get; set; }

    public void Start()
    {
        Player = ExPlayer.Get(gameObject);
        PlayerData = DataPlayer.GetPlayer(Player.UserId);
        if (!Player.DoNotTrack) PlayerData = DataPlayer.AddPlayer(Player.UserId, Player.Nickname);

        if(Plugin.Singleton.Config.WeightSystem)
            new WeightSystem().Init(Player);

        //Create an AudioPlayer for the player.
        AudioPlayer = AudioPlayer.Create($"Player {Player.Nickname}", controllerId: SpeakerExtensions.GetFreeId());
        AudioPlayer.transform.parent = Player.GameObject.transform;
        Log.Info($"Created AudioPlayer for {Player.Nickname} with a controller id of {AudioPlayer.ControllerID}");
    }

    public void Update()
    {
        // HealthSystem.Update();
        // EffectManager.UpdateEffects();
        Timing.RunCoroutine(BlinkRoutine());
    }

    public IEnumerator<float> TrackHours()
    {
        if (Round.IsEnded)
            yield break;
        
        var savedRole = CurrentRole.RoleEntry;
        var savedRank = CurrentRole.Rank;
        while (savedRole.RoleName == CurrentRole.RoleEntry.RoleName && !Round.IsEnded)
        {
            ElapsedTimeAsRole += 1 * Time.deltaTime;
            yield return Timing.WaitForOneFrame;
        }

        var playerEarned = ElapsedTimeAsRole / 3;
        var data = Dept.DepartmentsData[Dept.GetDepartmentByRole(savedRole)];
        
        Log.Info($"Department Cash Before {data.Balance}");
        
        data.Balance += float.Parse($"{playerEarned:N0}");
        data.Balance -= savedRank.AmountPaidPerRP;
        
        Dept.UpdateDepartment(data.Department, data);
        
        Log.Info($"Department Cash After {data.Balance}");

        if (!Player.DoNotTrack)
        {
            DataPlayer.SetWallet(Player.UserId, DataPlayer.GetPlayer(Player.UserId).Wallet += savedRank.AmountPaidPerRP);
            var time = $"{ElapsedTimeAsRole:N0}";
            if (PlayerData.DepartmentData.ContainsKey(Dept.GetDepartmentByRole(savedRole)))
            {
                PlayerData.DepartmentData[Dept.GetDepartmentByRole(savedRole)]
                    .TimePlayed += float.Parse(time);
                DataPlayer.UpdateData();
            }
        }

        Log.Info($"Saved playerData of {Player.UserId}");
        ElapsedTimeAsRole = 0;
    }

    private bool _isBlinking;
    public IEnumerator<float> BlinkRoutine()
    {
        if (!Lobby.IsRoleplay)
            yield break;
        if (_isBlinking)
            yield break;
        if (Player.IsDead || Player.IsScp)
            yield break;
        if (ReferenceHub.AllHubs
                .FirstOrDefault(player => player.roleManager.CurrentRole.RoleTypeId == RoleTypeId.Scp173)?.roleManager
                .CurrentRole is not PlayerRoles.PlayableScps.Scp173.Scp173Role scp173Role)
            yield break;
        if(!scp173Role.SubroutineModule.TryGetSubroutine<Scp173ObserversTracker>(out var observersTracker))
            yield break;
        if(!observersTracker.Observers.Contains(Player.ReferenceHub))
           yield break;

        _isBlinking = true;
        yield return Timing.WaitForSeconds(URandom.Range(2.9f, 3.1f));
        var intensity = 1;
        const int speed = 80;
        Player.EnableEffect<Blindness>((byte)intensity, 14);
        while (intensity < 107)
        {
            Player.GetEffect<Blindness>().Intensity = (byte)intensity;
            intensity += speed;
            yield return Timing.WaitForOneFrame;
        }

        Exiled.API.Features.Roles.Scp173Role.TurnedPlayers.Add(Player);

        yield return Timing.WaitForSeconds(0.15f);

        while (intensity > 0)
        {
            Player.GetEffect<Blindness>().Intensity = (byte)intensity;
            intensity -= speed;
            yield return Timing.WaitForOneFrame;
        }

        _isBlinking = false;

        Exiled.API.Features.Roles.Scp173Role.TurnedPlayers.Remove(Player);
    }
}

public static class ScombatPlayer
{
    public static ScomPlayer ScomPlayer(this ExPlayer player) => player.GameObject.GetComponent<ScomPlayer>();
    public static void AddScomPlayer(this ExPlayer player) => player.GameObject.AddComponent<ScomPlayer>();

    // Was I tweaking?
    public static string ReplaceLetters(this string input)
    {
        const string alphabetLower = "abcdefghijklmnopqrstuvwxyz0123456789";
        const string alphabetUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const string replacementLower = "ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ０１２３４５６７８９";
        const string replacementUpper = "ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９";

        var sb = StringBuilderPool.Shared.Rent();

        foreach (var c in input)
        {
            // Handle lowercase letters
            if (char.IsLower(c))
            {
                var index = alphabetLower.IndexOf(c);
                if (index >= 0)
                    sb.Append(replacementLower[index]);
                else
                    sb.Append(c);
            }
            // Handle uppercase letters
            else if (char.IsUpper(c))
            {
                var index = alphabetUpper.IndexOf(c);
                if (index >= 0)
                    sb.Append(replacementUpper[index]);
                else
                    sb.Append(c);
            }
            // Handle digits
            else if (char.IsDigit(c))
            {
                var index = alphabetLower.IndexOf(c); // Use alphabetLower since numbers are included there
                if (index >= 0)
                    sb.Append(replacementLower[index]);
                else
                    sb.Append(c);
            }
            else
            {
                sb.Append(c); // Keep the character if it's not a letter or number
            }
        }

        return StringBuilderPool.Shared.ToStringReturn(sb);
    }

    [OnPluginEnabled]
    public static void InitEvents() => PlayerHandlers.Verified += OnJoining;

    public static void OnJoining(VerifiedEventArgs ev) => Timing.CallDelayed(0.25f, () => ev.Player.AddScomPlayer());
}