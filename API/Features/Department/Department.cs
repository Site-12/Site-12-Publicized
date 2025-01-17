namespace Site12.API.Features.Department;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Attributes;
using Exiled.API.Features;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PlayerRoles;

public class Department
{
    public static Dictionary<string, DepartmentInfo> DepartmentsData = new();

    [OnPluginEnabled]
    public static void Init()
    {
        if (!Directory.Exists(Path.Combine(Paths.Plugins, "Site12")))
            Directory.CreateDirectory(Path.Combine(Paths.Plugins, "Site12"));
        foreach (var files in Directory.GetFiles(Path.Combine(Paths.Plugins, "Site12")))
        {
            if (files == Path.Combine(Paths.Plugins, "Site12", "PlayerData.json"))
                continue;
            if (files == Path.Combine(Paths.Plugins, "Site12", "Shop.json"))
                continue;
            if (files == Path.Combine(Paths.Plugins, "Site12", "Users.json"))
                continue;
            var data = JsonConvert.DeserializeObject<DepartmentInfo>(File.ReadAllText(files));
            DepartmentsData.Add(data.Department, data);
        }

        foreach (var department in Plugin.Singleton.Config.Departments.Where(department => !DepartmentsData.ContainsKey(department))) CreateNewDepartment(department, []);

        if (!DepartmentsData.ContainsKey("Other"))
            CreateNewDepartment("Other", []);
    }

    public static List<RoleEntry> GetAllRoles(string department) => DepartmentsData.FirstOrDefault(departments => departments.Value.Department == department).Value.Roles;

    public static RoleEntry GetRole(string roleName, string department = null)
    {
        if (department != null && DepartmentsData.TryGetValue(department, out var departmentInfo)) return departmentInfo.Roles.FirstOrDefault(entry => entry.RoleName == roleName);
        return DepartmentsData.Select(departments => departments.Value.Roles.FirstOrDefault(entry => entry.RoleName == roleName)).FirstOrDefault(role => role != null);
    }

    public static string GetDepartmentByRole(RoleEntry role) => role == null ? null : (from department in DepartmentsData where department.Value.Roles.Any(entry => entry == role) select department.Key).FirstOrDefault();

    public static void UpdateDepartment(string name, DepartmentInfo departmentInfo)
    {
        if (!File.Exists(Path.Combine(Paths.Plugins, "Site12", name + ".json")))
            return;

        if (departmentInfo == null)
            return;

        if(DepartmentsData.ContainsKey(name))
            DepartmentsData[name] = departmentInfo;

        File.WriteAllText(Path.Combine(Paths.Plugins, "Site12", name + ".json"), JsonConvert.SerializeObject(departmentInfo, Formatting.Indented));
    }

    public static DepartmentInfo CreateNewDepartment(string name, List<RoleEntry> roleEntries)
    {
        if (File.Exists(Path.Combine(Paths.Plugins, "Site12", name + ".json")))
            return null;

        var departmentInfo = new DepartmentInfo
        {
            Department = name,
            Balance = 25000,
            Roles = roleEntries
        };

        DepartmentsData.Add(name, departmentInfo);

        File.WriteAllText(Path.Combine(Paths.Plugins, "Site12", name + ".json"), JsonConvert.SerializeObject(departmentInfo, Formatting.Indented));

        return departmentInfo;
    }

    public static void UpdateDepartmentData(string departmentName)
    {
        if(DepartmentsData.TryGetValue(departmentName, out var value))
            File.WriteAllText(Path.Combine(Paths.Plugins, "Site12", departmentName + ".json"), JsonConvert.SerializeObject(value, Formatting.Indented));
    }
}

public class DepartmentInfo
{
    public string Department { get; init; }
    public float Balance { get; set; }
    public List<RoleEntry> Roles { get; set; }
}

public class RoleEntry
{
    public string RoleName { get; set; }
    public RoleDetails Role { get; set; }
}

public class RoleDetails
{
    public string Prefix { get; set; }
    public string CustomI { get; set; }
    public string Description { get; set; }
    public Dictionary<string, RankDetails> Ranks { get; set; }
}

public class RankDetails
{
    public int RankWeight { get; set; }
    public bool RequiresRankInRoster { get; set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public RoleTypeId RoleTypeID { get; set; }
    public bool Default { get; set; }
    public bool HasPda { get; set; }
    public List<LoadOutItem> LoadOut { get; set; }
    public float AmountPaidPerRP { get; set; }
}

public class LoadOutItem
{
    public string ItemType { get; set; } // The type of the item (e.g., KeycardJanitor, GunCOM15)
    public int? Level { get; set; } // Optional level for keycards (null for non-keycards)
    public List<string> Permissions { get; set; } // Permissions for keycards (null or empty for non-keycards)
}