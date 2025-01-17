namespace Site12.API.Features.CustomItems;

using System;
using System.Collections.Generic;
using System.Linq;
using Attributes;

public static class CustomItemsManager
{
    static CustomItemsManager()
    {
        var types = typeof(CustomItemsManager).Assembly.GetTypes();

        var handlers = ItemHandlers = new Dictionary<Type, CustomItemHandler>();

        foreach (var type in types)
        {
            if (!typeof(CustomItemHandler).IsAssignableFrom(type) || !type.IsClass || type.IsAbstract) continue;
            var handler = (CustomItemHandler)Activator.CreateInstance(type, nonPublic: true);

            handlers.Add(type, handler);
        }

        ServerHandlers.WaitingForPlayers += WaitingForPlayers;
    }

    public static readonly Dictionary<Type, CustomItemHandler> ItemHandlers;

    public static bool IsEnabled { get; private set; }

    public static bool TryGet<T>(out T item)
        where T : CustomItemHandler
    {
        if (ItemHandlers == null)
        {
            item = default;
            return false;
        }

        item = ItemHandlers[typeof(T)] as T;
        return item != null;
    }

    public static T Get<T>()
        where T : CustomItemHandler
    {
        return (T)ItemHandlers[typeof(T)];
    }

    public static bool IsCustomItem(ushort serial, out CustomItemHandler handler)
    {
        foreach (var itemHandler in ItemHandlers.Values.Where(itemHandler => itemHandler.HasItem(serial)))
        {
            handler = itemHandler;
            return true;
        }

        handler = null;
        return false;
    }

    [OnPluginEnabled]
    internal static void EnableEvents()
    {
        if (IsEnabled)
            return;

        foreach (var handler in ItemHandlers)
            handler.Value.EnableEvents();

        IsEnabled = true;
    }

    private static void WaitingForPlayers()
    {
        foreach (var handler in ItemHandlers)
            handler.Value.ClearItems();
    }
}
