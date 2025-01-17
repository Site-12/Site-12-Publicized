namespace Site12.API.Core;

using System;
using Features.CustomItems;
using InventorySystem;
using InventorySystem.Items;

public class ItemGiver
{
    public int? Amount;
    public ItemType? ItemType;
    public CustomItemHandler CustomItem;
    public Action<ItemBase> GiveItemCallback;

    public bool IsCustomItem => CustomItem != null;

    public static implicit operator ItemGiver(ItemType type)
    {
        ItemGiver result = new();
        result.ItemType = type;
        return result;
    }

    public static implicit operator ItemGiver((ItemType type, int amount) pair)
    {
        ItemGiver result = new();
        result.ItemType = pair.type;
        result.Amount = pair.amount;
        return result;
    }

    public static implicit operator ItemGiver(CustomItemHandler customItem)
    {
        ItemGiver result = new();
        result.CustomItem = customItem;
        return result;
    }

    public static implicit operator ItemGiver((CustomItemHandler, Action<ItemBase>) pair)
    {
        ItemGiver result = new();
        result.CustomItem = pair.Item1;
        result.GiveItemCallback = pair.Item2;
        return result;
    }

    public void GiveItem(ExPlayer player)
    {
        ItemBase item;
        if (IsCustomItem)
        {
            if (player.Inventory.UserInventory.Items.Count < 8)
            {
                item = CustomItem.GiveItem(player);
                GiveItemCallback?.Invoke(item);
            }
            return;
        }

        if (ItemType == null)
            throw new InvalidOperationException("ItemType is null!");

        bool isAmmo = InventoryItemLoader.AvailableItems.TryGetValue(ItemType.Value, out var value) && value.Category == ItemCategory.Ammo;

        if (Amount is not int amount)
            amount = isAmmo ? 30 : 1;

        if (isAmmo)
        {
            player.Inventory.ServerAddAmmo(ItemType.Value, amount);
            return;
        }

        if (player.Inventory.UserInventory.Items.Count < 8)
        {
            item = player.AddItem((ItemType)ItemType).Base;
            GiveItemCallback?.Invoke(item);
        }
    }
}
