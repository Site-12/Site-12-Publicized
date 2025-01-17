namespace Site12.API.Features.Items;

using CustomItems;
using Exiled.API.Enums;
using Exiled.Events.EventArgs.Item;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.Handlers;
using InventorySystem.Items;

public sealed class BatonHandler : CustomItemHandler
{
    private BatonHandler()
    {
    }

    public CustomItemContainer Container { get; } = new();

    public override string Name => "Baton";
    public override string[] Alias => [];

    public override void EnableEvents()
    {
        PlayerHandlers.Hurting += SwingJailBird;
        Item.ChargingJailbird += ChargeJailBird;
        PlayerHandlers.ChangingItem += ChangingItem;
    }

    public void ChangingItem(ChangingItemEventArgs ev)
    {
        if (ev.Item == null)
            return;

        if (!HasItem(ev.Item.Base))
            return;

        ev.Player.ShowHint("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=27><mark=#367ce8><size=23>|⚡|</size></mark><mark=#595959>||</mark><mark=#393D4780> <size=23><space=2.6em><b>ᴇʟᴇᴄᴛʀɪᴄ ʙᴀᴛᴏɴ</size><space=2.6em><size=0.1>.</size></mark></size>");
    }

    public override bool HasItem(ushort serial) => Container.HasItem(serial);

    public override void ClearItems() => Container.ClearItems();

    private void SwingJailBird(HurtingEventArgs ev)
    {
        if (ev.Attacker?.CurrentItem == null)
            return;

        if (!HasItem(ev.Attacker.CurrentItem.Base))
            return;

        if (ev.Attacker == ev.Player)
            return;

        if (ev.DamageHandler.Type != DamageType.Jailbird)
            return;

        if (ev.Player.IsGodModeEnabled)
            return;

        ev.Amount = 5f;

        ev.Player.ShowHint("You've been hit by a Baton", 5f);
        ev.Player.EnableEffect(EffectType.Blinded, 5f);
        ev.Player.EnableEffect(EffectType.Concussed, 5f);
        ev.Player.EnableEffect(EffectType.Deafened, 5f);
        ev.Player.EnableEffect(EffectType.Flashed, 2f);
        ev.Player.EnableEffect(EffectType.Disabled, 5f);
        ev.Player.EnableEffect(EffectType.Traumatized, 50f);
    }

    private void ChargeJailBird(ChargingJailbirdEventArgs ev)
    {
        if (!HasItem(ev.Player.CurrentItem.Base))
            return;

        ev.Jailbird.ChargeDamage = 15f;
    }

    public override ItemBase GiveItem(ExPlayer player)
    {
        var item = player.AddItem(ItemType.Jailbird);
        Container.RegisterItem(item.Base);
        return item.Base;
    }
}