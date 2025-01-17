namespace Site12.Extensions;

using CommandSystem;
using RemoteAdmin;

public static class CommandExtensions
{
    public static bool CheckRemoteAdmin(this ICommandSender sender, out string response)
    {
        response = string.Empty;

        if (sender is not PlayerCommandSender plySender)
            return true;

        if (plySender.ReferenceHub.serverRoles.RemoteAdmin)
            return true;

        response = "No permissions.";
        return false;
    }
}