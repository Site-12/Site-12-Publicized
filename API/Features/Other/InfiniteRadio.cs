namespace Site12.API.Features.Other;

using Attributes;
using Exiled.Events.EventArgs.Item;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.Handlers;

public class InfiniteRadio
{
    [OnPluginEnabled]
    private static void Init()
    {
        PlayerHandlers.UsingRadioBattery += Radio;
        Item.UsingRadioPickupBattery += PickupRadio;
    }
    private static void PickupRadio(UsingRadioPickupBatteryEventArgs ev) => ev.IsAllowed = false;
    private static void Radio(UsingRadioBatteryEventArgs ev) => ev.IsAllowed = false;
}