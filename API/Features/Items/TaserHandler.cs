namespace Site12.API.Features.Items;

using System.Collections.Generic;
using CustomItems;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items;
using MEC;
using PlayerRoles;
using UnityEngine;

public sealed class TaserHandler : CustomItemHandler
{
    private TaserHandler()
    {
    }

    public CustomItemContainer Container { get; } = new();

    public override string Name => "Taser";
    public override string[] Alias => ["stun gun"];

    public override void EnableEvents()
    {
        PlayerHandlers.Shot += Shot;
        PlayerHandlers.ReloadingWeapon += Reloading;
        PlayerHandlers.ChangingItem += ChangingItem;
    }

    public void ChangingItem(ChangingItemEventArgs ev)
    {
        if (ev.Item == null)
            return;

        if (!HasItem(ev.Item.Base))
            return;

        ev.Player.ShowHint("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=27><mark=#367ce8><size=23>|⚡|</size></mark><mark=#595959>||</mark><mark=#393D4780> <size=23><space=2.6em><b>ᴛᴀꜱᴇʀ</size><space=2.6em><size=0.1>.</size></mark></size>");
    }

    public override bool HasItem(ushort serial)
    {
        return Container.HasItem(serial);
    }

    public override void ClearItems()
    {
        Container.ClearItems();
    }

    public void Shot(ShotEventArgs ev)
    {
        if (!HasItem(ev.Player.CurrentItem.Base))
            return;
        ev.CanHurt = false;

        if (ev.Target.IsGodModeEnabled)
            return;
        if (ev.Target.IsScp && ev.Target.Role.Type != RoleTypeId.Scp0492)
            return;

        if (ev.Distance > 7)
            return;

        var chance = URandom.Range(0, 100);

        if (chance == 1)
        {
            ev.Target.EnableEffect(EffectType.CardiacArrest, 10f);
            return;
        }

        // I'm not gonna fucking do EnableEffects
        Timing.RunCoroutine(RotateFastLeftRight(ev.Target));
        ev.Target.EnableEffect(EffectType.Deafened, 255, 10f);
        ev.Target.EnableEffect(EffectType.AmnesiaItems, 255, 10f);
        ev.Target.EnableEffect(EffectType.Blinded, 255, 30f);
        ev.Target.EnableEffect(EffectType.Slowness, 20, 60f);
        ev.Target.EnableEffect(EffectType.Ensnared, 255, 6f);
        ev.Target.EnableEffect<Exhausted>(40f);
    }

    public IEnumerator<float> RotateFastLeftRight(ExPlayer player)
    {
        var elapsedTime = 0f;
        const float duration = 6f;
        const float interval = 0.085f; // Time between direction changes
        const float speed = 10f; // Rotation speed (reduce for smoother transition)
        var rotateDirection = 1f; // 1 for right, -1 for left

        while (elapsedTime < duration)
        {
            // Rotate the player smoothly based on the direction
            var targetRotationY = player.Rotation.eulerAngles.y + speed * rotateDirection;
            var targetRotation = Quaternion.Euler(player.Rotation.eulerAngles.x, targetRotationY, player.Rotation.eulerAngles.z);

            // Smoothly interpolate between current and target rotation
            player.Rotation = Quaternion.Lerp(player.Rotation, targetRotation, Time.deltaTime / interval);

            // Switch direction after 0.05 seconds
            elapsedTime += interval;
            rotateDirection *= -1f; // Reverse direction (left or right)
            yield return Timing.WaitForSeconds(interval); // Wait 0.05 seconds before the next rotation
        }
    }

    public void Reloading(ReloadingWeaponEventArgs ev)
    {
        if (!HasItem(ev.Firearm.Base))
            return;

        ev.Firearm.MaxMagazineAmmo = 1;
    }

    public override ItemBase GiveItem(ExPlayer player)
    {
        var item = player.AddItem(ItemType.GunCOM15);
        var firearm = (Exiled.API.Features.Items.Firearm)item;
        firearm.ClearAttachments();
        firearm.MaxMagazineAmmo = 1;
        firearm.MagazineAmmo = 1;

        Container.RegisterItem(item.Base);
        return item.Base;
    }
}
