namespace Site12.API.Features.CustomItems;

using System;
using System.Collections.Generic;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;

public sealed class CustomItemContainer
{
    private readonly HashSet<ushort> _serials = [];

    public void RegisterItem(ItemPickupBase pickup)
    {
        if (pickup == null)
            throw new ArgumentNullException(nameof(pickup));

        _serials.Add(pickup.Info.Serial);
    }

    public void RegisterItem(ItemBase item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        _serials.Add(item.ItemSerial);
    }

    public void RegisterItem(ushort serial)
    {
        _serials.Add(serial);
    }

    public void RemoveItem(ItemPickupBase pickup)
    {
        if (pickup == null)
            return;

        _serials.Remove(pickup.Info.Serial);
    }

    public void RemoveItem(ItemBase item)
    {
        if (item == null)
            return;

        _serials.Remove(item.ItemSerial);
    }

    public void RemoveItem(ushort serial)
    {
        _serials.Remove(serial);
    }

    public bool HasItem(ItemPickupBase pickup)
    {
        return pickup != null && _serials.Contains(pickup.Info.Serial);
    }

    public bool HasItem(ItemBase item)
    {
        if (item == null)
            return false;

        return _serials.Contains(item.ItemSerial);
    }

    public bool HasItem(ushort serial)
    {
        return _serials.Contains(serial);
    }

    public void ClearItems()
    {
        _serials.Clear();
    }
}

public sealed class CustomItemContainer<T>
{
    private readonly Dictionary<ushort, T> _serials;

    public CustomItemContainer()
    {
        _serials = new();
    }

    public void RegisterItem(ItemPickupBase pickup, T value)
    {
        if (pickup == null)
            throw new ArgumentNullException(nameof(pickup));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        _serials.Add(pickup.Info.Serial, value);
    }

    public void RegisterItem(ItemBase item, T value)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        _serials.Add(item.ItemSerial, value);
    }

    public void RegisterItem(ushort serial, T value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        _serials.Add(serial, value);
    }

    public void SetItemValue(ItemPickupBase pickup, T value)
    {
        if (pickup == null)
            throw new ArgumentNullException(nameof(pickup));

        if (!HasItem(pickup))
            throw new ArgumentException("Key is not present in the dictionary.", nameof(pickup));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        _serials[pickup.Info.Serial] = value;
    }

    public void SetItemValue(ItemBase item, T value)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (!HasItem(item))
            throw new ArgumentException("Key is not present in the dictionary.", nameof(item));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        _serials[item.ItemSerial] = value;
    }

    public void SetItemValue(ushort serial, T value)
    {
        if (!HasItem(serial))
            throw new ArgumentException("Key is not present in the dictionary.", nameof(serial));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        _serials[serial] = value;
    }

    public void RemoveItem(ItemPickupBase pickup)
    {
        if (pickup == null)
            return;

        _serials.Remove(pickup.Info.Serial);
    }

    public void RemoveItem(ItemBase item)
    {
        if (item == null)
            return;

        _serials.Remove(item.ItemSerial);
    }

    public void RemoveItem(ushort serial)
    {
        _serials.Remove(serial);
    }

    public bool HasItem(ItemPickupBase pickup)
    {
        if (pickup == null)
            return false;

        return _serials.ContainsKey(pickup.Info.Serial);
    }

    public bool HasItem(ItemBase item)
    {
        if (item == null)
            return false;

        return _serials.ContainsKey(item.ItemSerial);
    }

    public bool HasItem(ushort serial)
    {
        return _serials.ContainsKey(serial);
    }

    public bool HasItem(ItemPickupBase pickup, out T value)
    {
        if (pickup == null)
        {
            value = default;
            return false;
        }

        return _serials.TryGetValue(pickup.Info.Serial, out value);
    }

    public bool HasItem(ItemBase item, out T value)
    {
        if (item == null)
        {
            value = default;
            return false;
        }

        return _serials.TryGetValue(item.ItemSerial, out value);
    }

    public bool HasItem(ushort serial, out T value)
    {
        return _serials.TryGetValue(serial, out value);
    }

    public void ClearItems()
    {
        _serials.Clear();
    }
}