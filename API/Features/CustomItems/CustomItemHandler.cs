namespace Site12.API.Features.CustomItems;

using InventorySystem.Items;
using InventorySystem.Items.Pickups;

public abstract class CustomItemHandler
{
    public abstract string Name { get; }
    public abstract string[] Alias { get; }

    public abstract void EnableEvents();

    public virtual bool HasItem(ItemPickupBase pickup)
    {
        return HasItem(pickup.Info.Serial);
    }

    public virtual bool HasItem(ItemBase item)
    {
        return HasItem(item.ItemSerial);
    }

    public abstract bool HasItem(ushort serial);

    public abstract ItemBase GiveItem(ExPlayer player);

    public abstract void ClearItems();
}
