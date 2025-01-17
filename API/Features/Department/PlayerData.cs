namespace Site12.API.Features.Department;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Attributes;
using Exiled.API.Features;
using Newtonsoft.Json;

public class DataPlayer
{
    public static PlayersData PlayersData;

    [OnPluginEnabled]
    public static void Init()
    {
        if (!Directory.Exists(Path.Combine(Paths.Plugins, "Site12")))
            Directory.CreateDirectory(Path.Combine(Paths.Plugins, "Site12"));
        if (!File.Exists(Path.Combine(Paths.Plugins, "Site12", "PlayerData.json")))
        {
            PlayersData = new PlayersData();
            File.WriteAllText(Path.Combine(Paths.Plugins, "Site12", "PlayerData.json"), JsonConvert.SerializeObject(PlayersData, Formatting.Indented));
        }
        else PlayersData = JsonConvert.DeserializeObject<PlayersData>(File.ReadAllText(Path.Combine(Paths.Plugins, "Site12", "PlayerData.json")));
    }

    public static PlayerData AddPlayer(string id, string nickName)
    {
        if (id.IsEmpty())
            return null;
        if (PlayersData.Players.FirstOrDefault(entry => entry.PlayerUserID == id) != null)
            return PlayersData.Players.FirstOrDefault(entry => entry.PlayerUserID == id);

        var data = new PlayerData
        {
            PlayerUserID = id,
            NickName = nickName,
            Wallet = 0f,
            DepartmentData = []
        };

        PlayersData.Players.Add(data);
        UpdateData();

        return data;
    }

    public static void SetWallet(string id, float amount)
    {
        var playerData = PlayersData.Players.FirstOrDefault(entry => entry.PlayerUserID == id);
        if (playerData == null)
            return;
        playerData.Wallet = amount;

        UpdateData();
    }

    public static void SetRole(string id, string department, string roleName, int roleWeight)
    {
        var playerData = PlayersData.Players.FirstOrDefault(entry => entry.PlayerUserID == id);
        if (playerData == null)
            return;
        var departmentData = playerData.DepartmentData.FirstOrDefault(entry => entry.Key == department).Value;
        if (departmentData == null)
        {
            if (!Department.DepartmentsData.ContainsKey(department))
                return;
            Department.DepartmentsData.Add(department, null);
            departmentData = playerData.DepartmentData[department];
        }

        if (departmentData.Roles.ContainsKey(roleName))
        {
            departmentData.Roles[roleName] = roleWeight;
            return;
        }

        if (Department.GetAllRoles(department).FirstOrDefault(entry => entry.RoleName == roleName) == null)
            return;
        departmentData.Roles.Add(roleName, roleWeight);

        UpdateData();
    }

    public static PlayerData GetPlayer(string id) => PlayersData.Players.FirstOrDefault(entry => entry.PlayerUserID == id);

    public static bool IsPlayerInDepartment(string id, string department, out RankDetails rankDetails)
    {
        var player = GetPlayer(id);
        rankDetails = null;
        if (player == null)
            return false;
        if (!Department.DepartmentsData.ContainsKey(department))
            return false;
        if (!player.DepartmentData.TryGetValue(department, out var departmentData))
            return false;

        var data = departmentData.Roles.FirstOrDefault(entry => entry.Value >= 0);
        rankDetails = Department.GetRole(data.Key, department).Role.Ranks
            .FirstOrDefault(entry => entry.Value.RankWeight == data.Value).Value;
        return true;
    }
    public static List<PlayerData> GetPlayersInDepartment(string department)
    {
        if (!Department.DepartmentsData.ContainsKey(department))
            return [];
        List<PlayerData> players = [];
        foreach (var player in PlayersData.Players)
        {
            if(!player.DepartmentData.TryGetValue(department, out var playerDepData))
                continue;
            if (playerDepData.Roles.Any(role => role.Value >= 0)) players.Add(player);
        }

        return players;
    }

    public static void UpdateData() => File.WriteAllText(Path.Combine(Paths.Plugins, "Site12", "PlayerData.json"), JsonConvert.SerializeObject(PlayersData, Formatting.Indented));
}

public class PlayersData
{
    public List<PlayerData> Players { get; set; } = [];
}

public class PlayerData
{
    public string PlayerUserID { get; set; }
    public string NickName { get; set; }
    public float Wallet { get; set; }
    public Dictionary<string, DepartmentData> DepartmentData { get; set; }
}

public class DepartmentData
{
    public float TimePlayed { get; set; }
    public Dictionary<string, int> Roles { get; set; }
}