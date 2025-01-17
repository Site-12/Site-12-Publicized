namespace Site12.API.Features.Items;

using System;
using CommandSystem;
using CustomItems;
using Exiled.Events.EventArgs.Player;
using Extensions;
using InventorySystem.Items;

public sealed class CustomHandler : CustomItemHandler
{
    private CustomHandler() { }
    public CustomItemContainer<Item> Container { get; } = new();

    public override string Name => "";
    public override string[] Alias => [];

    public override void EnableEvents()
    {
        PlayerHandlers.ChangingItem += ChangingItems;
        PlayerHandlers.UsingItem += ItemUse;
    }

    public override bool HasItem(ushort serial)
    {
        return Container.HasItem(serial);
    }

    public override ItemBase GiveItem(ExPlayer player)
    {
        var item = player.AddItem(ItemType.Coin);
        Container.RegisterItem(item.Base, new Item("Radical Coin"));
        return item.Base;
    }

    public void ChangingItems(ChangingItemEventArgs ev)
    {
        if (ev.Item == null)
            return;

        if (!Container.HasItem(ev.Item.Base, out var item))
            return;

        ev.Player.ShowHint($"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=27><mark=#367ce8><size=23>|✨|</size></mark><mark=#595959>||</mark><mark=#393D4780> <size=21><space=2.6em><b>{item.Name.ReplaceLetters()}</size><space=2.6em><size=0.1>.</size></mark></size>");
    }

    private void ItemUse(UsingItemEventArgs ev)
    {
        if (!Container.HasItem(ev.Item.Base))
            return;

        if (ev.Item.Type != ItemType.Medkit)
            return;

        ev.IsAllowed = false;
        ev.Player.RemoveHeldItem();
        ev.Player.ShowHint("Mmmm, that tasted good!", 5f);
    }

    public ItemBase GiveItem(ExPlayer player, string name, ItemType type)
    {
        var item = player.AddItem(type);
        Container.RegisterItem(item.Base, new Item(name));
        return item.Base;
    }

    public override void ClearItems()
    {
        Container.ClearItems();
    }

    public sealed class Item(string name)
    {
        public readonly string Name = name;
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class GiveCustomItem : ICommand
{
    public string Command => "gic";
    public string[] Aliases { get; } = ["givecustomitem"];
    public string Description => "gic {Item Type} {Custom Item Name}";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        if (arguments.Count < 1 || !Enum.TryParse(arguments.At(0), true, out ItemType type))
        {
            response = "<color=red>Invalid Item Type";
            return false;
        }

        // Get the arguments after the first one
        var additionalArguments = new ArraySegment<string>(arguments.Array!, arguments.Offset + 1, arguments.Count - 1);

        // Join the additional arguments into a single string if needed
        var additionalArgumentsString = string.Join(" ", additionalArguments);

        CustomItemsManager.Get<CustomHandler>().GiveItem(ExPlayer.Get(sender), additionalArgumentsString, type);

        response = $"<color=green>You gave yourself {additionalArgumentsString}";
        return true;
    }
}