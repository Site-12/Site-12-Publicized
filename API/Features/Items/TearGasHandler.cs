namespace Site12.API.Features.Items;

using CustomItems;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.Handlers;
using InventorySystem.Items;
using InventorySystem.Items.Usables.Scp244;
using Mirror;
using UnityEngine;
using static InventorySystem.Items.Usables.Scp244.Hypothermia.Hypothermia;

public sealed class TearGasHandler : CustomItemHandler
{
    private TearGasHandler()
    {
    }

    public static TearGasInfo DefaultInfo => new()
    {
        DamagePS = 2,
        Effects =
        [
            EffectType.Blinded,
            EffectType.Burned,
            EffectType.Concussed,
            EffectType.Deafened
        ],
        GrowingTime = 5,
        FullSizeTime = 30,
        ShrinkingTime = 5,
    };

    public CustomItemContainer<TearGasInfo> Container { get; } = new();

    public override string Name => "Tear Gas Grenade";
    public override string[] Alias => ["tear", "tear gas", "teargas", "tgas"];

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

        ev.Player.ShowHint("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=27><mark=#367ce8><size=23>|💧|</size></mark><mark=#595959>||</mark><mark=#393D4780> <size=23><space=2.6em><b>ᴛᴇᴀʀ ɢᴀꜱ ɢʀᴇɴᴀᴅᴇ</size><space=2.6em><size=0.1>.</size></mark></size>");
    }

    public void ExplodingGrenade(ExplodingGrenadeEventArgs ev)
    {
        if (ev.Projectile.Type != ItemType.GrenadeFlash)
            return;

        if (!Container.HasItem(ev.Projectile.Base, out var info))
            return;

        ev.IsAllowed = false;

        Container.RemoveItem(ev.Projectile.Base);

        var pickup = Pickup.Create(ItemType.SCP244a);

        pickup.Scale = Vector3.zero;
        pickup.Position = ev.Position;
        pickup.IsLocked = true;
        pickup.Rigidbody.constraints = RigidbodyConstraints.FreezePosition;
        pickup.Rigidbody.useGravity = false;
        pickup.Rigidbody.isKinematic = false;
        pickup.Rigidbody.detectCollisions = false;

        var vase = (Scp244DeployablePickup)pickup.Base;
        var gasComp = vase.gameObject.AddComponent<TearGasComponent>();
        gasComp.Info = info;
        vase._syncState = (byte)Scp244State.Active;
        vase.enabled = false;
        pickup.Spawn();
    }

    public override bool HasItem(ushort serial)
    {
        return Container.HasItem(serial);
    }

    public override void ClearItems()
    {
        Container.ClearItems();
    }

    public override ItemBase GiveItem(ExPlayer player)
    {
        var item = player.AddItem(ItemType.GrenadeFlash);
        Container.RegisterItem(item.Base, DefaultInfo);
        return item.Base;
    }

    public sealed class TearGasInfo
    {
        public float GrowingTime;
        public float FullSizeTime;
        public float ShrinkingTime;

        public float DamagePS;
        public EffectType[] Effects;
    }

    public sealed class TearGasComponent : MonoBehaviour
    {
        private const float Interval = 1f;
        private float _intervalTimer;
        private float _aliveTimer;
        private Scp244DeployablePickup _pickup;
        private AnimationCurve _curve;
        private int _decontaminationIndex;

        internal TearGasInfo Info;

        void Awake()
        {
            _decontaminationIndex = -1;
            _pickup = GetComponent<Scp244DeployablePickup>();
        }

        void Start()
        {
            _curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(Info.GrowingTime, 255), new Keyframe(Info.GrowingTime + Info.FullSizeTime, 255), new Keyframe(Info.GrowingTime + Info.FullSizeTime + Info.ShrinkingTime, 0));

            _pickup.UpdateCurrentRoom();
        }

        void OnDestroy()
        {
            if (_decontaminationIndex == -1)
                return;

            foreach (var player in ExPlayer.List)
            {
                Decontaminating decont = player.ReferenceHub.playerEffectsController.GetEffect<Decontaminating>();
                player.ReferenceHub.playerEffectsController._syncEffectsIntensity[_decontaminationIndex] = decont.IsEnabled ? (byte)1 : (byte)0;
            }
        }

        void ApplySize()
        {
            _pickup.UpdateConditions();

            float evaluated = _curve.Evaluate(_aliveTimer);
            _pickup.Network_syncSizePercent = (byte)Mathf.RoundToInt(evaluated);
            _pickup.CurrentSizePercent = evaluated / 255f;
        }

        void Update()
        {
            _aliveTimer += Time.deltaTime;
            _intervalTimer += Time.deltaTime;

            if (_aliveTimer > Info.GrowingTime + Info.FullSizeTime + Info.ShrinkingTime + 5f)
            {
                NetworkServer.Destroy(gameObject);
                return;
            }

            ApplySize();

            if (_intervalTimer < Interval)
                return;

            _intervalTimer -= Interval;

            ApplyIntervalEffects();
        }

        void ApplyIntervalEffects()
        {
            foreach (var player in ExPlayer.List)
            {
                if (_decontaminationIndex == -1)
                {
                    var arr = player.ReferenceHub.playerEffectsController.AllEffects;

                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (arr[i] is Decontaminating)
                        {
                            _decontaminationIndex = i;
                            break;
                        }
                    }
                }

                if (!_pickup.CurrentBounds.Contains(player.Position))
                {
                    Decontaminating decont = player.ReferenceHub.playerEffectsController.GetEffect<Decontaminating>();
                    player.ReferenceHub.playerEffectsController._syncEffectsIntensity[_decontaminationIndex] = decont.IsEnabled ? (byte)1 : (byte)0;
                    continue;
                }

                player.ReferenceHub.playerEffectsController._syncEffectsIntensity[_decontaminationIndex] = 1;

                ForcedHypothermiaMessage msg = new()
                {
                    Exposure = -1f,
                    IsForced = true,
                    PlayerHub = player.ReferenceHub,
                };

                player.Connection.Send(msg);

                if (player.IsGodModeEnabled) // dont annoy godded players
                {
                    player.ShowHint("You are in tear gas.", 1.5f);
                    continue;
                }

                player.ShowHint("You are being tear gassed.", 1.5f);
                player.EnableEffects(Info.Effects, 15, addDurationIfActive: false);
            }
        }
    }
}
