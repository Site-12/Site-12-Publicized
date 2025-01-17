namespace Site12.API.Features.Scombat.SCombatCmds;

using System;
using CommandSystem;
using CustomItems;
using Extensions;
using Enumerable = System.Linq.Enumerable;

public sealed class GiveCommand : ICommand, IUsageProvider
{
    public string Command => "give";
    public string[] Aliases => ["g"];
    public string Description => "Give a player a custom item.";
    public string[] Usage => ["ItemHandler} {User/Name(Optional)"];

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        if (arguments.Count < 1)
        {
            response = "<color=red>Arguments : scombat give {item} {Optional: ID}";
            return true;
        }

        var name = string.Join(" ", arguments);
        var handler = Enumerable.FirstOrDefault(CustomItemsManager.ItemHandlers.Values, handler1 => string.Equals(handler1.Name, name, StringComparison.CurrentCultureIgnoreCase) || handler1.Alias.Contains(name.ToLower()));

        if (handler == null)
        {
            response = "<color=red>Please use a valid item. View items with 'scombat list'";
            return false;
        }

        if (!ExPlayer.TryGet(arguments.At(arguments.Count - 1), out var player))
            player = ExPlayer.Get(sender);

        if (player.InventoryFull())
        {
            response = $"<color=red>Player {player.Nickname} Inventory IS FULL";
            return false;
        }

        handler.GiveItem(player);

        response = $"<color=green>You gave {player.Nickname} a {handler.Name}";
        return true;
    }
}