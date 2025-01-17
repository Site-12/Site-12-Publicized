using System.Linq;

namespace Site12.API.Features.Department;

using Exiled.API.Features;
using NorthwoodLib.Pools;

public class PlayerRole
{
    public string RoleName { get; private set; }
    public RoleEntry RoleEntry { get; private set; }
    public RankDetails Rank { get; private set; }
    public string RankName { get; private set; }

    public void ClearRole()
    {
        RoleName = null;
        RoleEntry = null;
        Rank = null;
        RankName = null;
    }

    public bool SetRole(string roleName, string department = null)
    {
        ClearRole();
        RoleEntry = Department.GetRole(roleName, department);

        if (RoleEntry == null)
        {
            Log.Warn($"Role '{roleName}' not found.");
            return false;
        }

        RoleName = roleName;
        return SetDefaultRank();
    }

    public bool SetRank(string rankName)
    {
        if (RoleEntry == null)
        {
            Log.Warn("Role has not been set.");
            return false;
        }

        if (RoleEntry.Role.Ranks.TryGetValue(rankName, out var rank))
        {
            Rank = rank;
            RankName = rankName;
            return true;
        }

        Log.Warn($"Rank '{rankName}' not found in role '{RoleName}'.");
        return false;
    }

    private bool SetDefaultRank()
    {
        var defaultRank = RoleEntry.Role.Ranks.Values.FirstOrDefault(r => r.Default);
        if (defaultRank != null)
        {
            Rank = defaultRank;
            RankName = RoleEntry.Role.Ranks.FirstOrDefault(r => r.Value.Default).Key;
            return true;
        }

        Log.Warn($"No default rank found for role '{RoleName}'.");
        return false;
    }

    public string DisplayPlayerRoleInfo()
    {
        var sb = StringBuilderPool.Shared.Rent();

        sb.AppendLine($"Role: {RoleName}");
        sb.AppendLine($"Rank: {RankName}");
        sb.AppendLine($"Description: {RoleEntry?.Role.Description}");
        sb.AppendLine($"Has PDA: {Rank?.HasPda}");
        sb.AppendLine($"Amount Paid Per RP: {Rank?.AmountPaidPerRP}");
        sb.AppendLine($"Role Type ID: {Rank?.RoleTypeID}");

        return StringBuilderPool.Shared.ToStringReturn(sb);
    }
}