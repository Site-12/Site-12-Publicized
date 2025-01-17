namespace Site12.API.Features.Items;

using System.Collections.Generic;
using CustomItems;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.Handlers;
using InventorySystem.Items;
using InventorySystem.Items.Usables.Scp244;
using MEC;
using Mirror;
using UnityEngine;

public sealed class SmokeGrenadeHandler : CustomItemHandler
{
    private SmokeGrenadeHandler()
    {
    }

    public static SmokeGrenadeInfo GetDefaultInfo => new()
    {
        GrowingTime = 5f,
        FullSizeTime = 60,
        ShrinkingTime = 15,
    };

    public CustomItemContainer<SmokeGrenadeInfo> Container { get; } = new();

    public override string Name => "Smoke Grenade";
    public override string[] Alias => ["smoke"];

    public override void EnableEvents()
    {
        Map.ExplodingGrenade += ExplodingGrenade;
        PlayerHandlers.ChangingItem += ChangingItem;
    }

    public void ChangingItem(ChangingItemEventArgs ev)
    {
        if (ev.Item == null)
            return;

        if (!HasItem(ev.Item.Base))
            return;

        ev.Player.ShowHint("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=27><mark=#367ce8><size=23>|🌁|</size></mark><mark=#595959>||</mark><mark=#393D4780> <size=23><space=2.6em><b>ꜱᴍᴏᴋᴇ ɢʀᴇɴᴀᴅᴇ</size><space=2.6em><size=0.1>.</size></mark></size>");
    }

    public override bool HasItem(ushort serial)
    {
        return Container.HasItem(serial);
    }

    public void ExplodingGrenade(ExplodingGrenadeEventArgs ev)
    {
        if (ev.Projectile.Type != ItemType.GrenadeFlash)
            return;

        if (!Container.HasItem(ev.Projectile.Base, out var info))
            return;

        ev.IsAllowed = false;

        Container.RemoveItem(ev.Projectile.Base);
        CreateSmokeGrenade(ev.Position, info.GrowingTime, info.FullSizeTime, info.ShrinkingTime).RunCoroutine();
    }

    public override void ClearItems()
    {
        Container.ClearItems();
    }

    public IEnumerator<float> CreateSmokeGrenade(Vector3 pos, float growingTime, float fullSizeTime, float shrinkingTime)
    {
        var pickup = Pickup.Create(ItemType.SCP244a);

        pickup.Scale = Vector3.zero;
        pickup.Position = pos;
        pickup.IsLocked = true;
        pickup.Rigidbody.constraints = RigidbodyConstraints.FreezePosition;
        pickup.Rigidbody.useGravity = false;
        pickup.Rigidbody.isKinematic = false;
        pickup.Rigidbody.detectCollisions = false;

        var vase = (Scp244DeployablePickup)pickup.Base;
        vase._syncState = (byte)Scp244State.Active;
        vase.enabled = false;
        pickup.Spawn();

        float maxTime = growingTime + fullSizeTime + shrinkingTime;

        GameObject obj = vase.gameObject;

        AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(growingTime / 2f, 191), new Keyframe(growingTime, 255), new Keyframe(growingTime + fullSizeTime, 255), new Keyframe(maxTime, 0));

        float time = 0;

        while (obj && time <= maxTime)
        {
            vase.Network_syncSizePercent = (byte)Mathf.RoundToInt(curve.Evaluate(time));

            yield return Timing.WaitForOneFrame;
            time += Timing.DeltaTime;
        }

        yield return Timing.WaitForSeconds(5f);

        NetworkServer.Destroy(obj);
    }

    public override ItemBase GiveItem(ExPlayer player)
    {
        var item = player.AddItem(ItemType.GrenadeFlash);
        Container.RegisterItem(item.Base, GetDefaultInfo);
        return item.Base;
    }

    public sealed class SmokeGrenadeInfo
    {
        public float GrowingTime;
        public float FullSizeTime;
        public float ShrinkingTime;
    }
}
