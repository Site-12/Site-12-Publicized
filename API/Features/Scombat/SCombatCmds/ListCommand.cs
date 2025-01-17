namespace Site12.API.Features.Scombat.SCombatCmds;

using System;
using System.Linq;
using CommandSystem;
using CustomItems;
using Extensions;

public sealed class ListCommand : ICommand, IUsageProvider
{
    public string Command => "list";

    public string[] Aliases => ["l"];

    public string Description => "List all available custom items.";
    public string[] Usage => [];

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        if (CustomItemsManager.ItemHandlers.IsEmpty())
        {
            response = "<color=red>No Items to show...";
            return false;
        }

        var results = CustomItemsManager.ItemHandlers.Values.Aggregate("Custom Items:\n", (current, handler) => current + $"- {handler.Name}\n");
        response = results;
        return true;
    }
}
