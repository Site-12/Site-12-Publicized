namespace Site12.API;

using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using MEC;
using UnityEngine;

// Skevandcodiene
public class WeightSystem
{
    public class ItemWeight(ItemType itemType, int weight)
    {
        public ItemType ItemType = itemType;
        public int Weight = weight;
    }

    public List<ItemWeight> ItemWeights = [
        new(ItemType.KeycardJanitor, 0),
        new(ItemType.KeycardScientist, 0),
        new(ItemType.KeycardGuard, 0),
        new(ItemType.KeycardResearchCoordinator, 0),
        new(ItemType.KeycardContainmentEngineer, 0),
        new(ItemType.KeycardFacilityManager, 0),
        new(ItemType.KeycardMTFCaptain, 0),
        new(ItemType.KeycardMTFOperative, 0),
        new(ItemType.KeycardMTFPrivate, 0),
        new(ItemType.KeycardChaosInsurgency, 0),
        new(ItemType.KeycardO5, 0),
        new(ItemType.Medkit, 2),
        new(ItemType.Painkillers, 1),
        new(ItemType.Adrenaline, 1),
        new(ItemType.Lantern, 2),
        new(ItemType.Flashlight, 2),
        new(ItemType.Radio, 3),
        new(ItemType.GunCOM15, 1),
        new(ItemType.GunCOM18, 1),
        new(ItemType.GunRevolver, 4),
        new(ItemType.GunFSP9, 4),
        new(ItemType.GunShotgun, 8),
        new(ItemType.GunCrossvec, 7),
        new(ItemType.GunE11SR, 8),
        new(ItemType.GunAK, 9),
        new(ItemType.GunLogicer, 17),
        new(ItemType.GunFRMG0, 13),
        new(ItemType.GunA7, 6),
        new(ItemType.GunCom45, 4),
        new(ItemType.MicroHID, 20),
        new(ItemType.GrenadeHE, 2),
        new(ItemType.GrenadeFlash, 1),
        new(ItemType.ArmorLight, 3),
        new(ItemType.ArmorCombat, 9),
        new(ItemType.ArmorHeavy, 13),
        new(ItemType.SCP018, 4),
        new(ItemType.SCP500, 0),
        new(ItemType.SCP244a, 5),
        new(ItemType.SCP244b, 5),
        new(ItemType.SCP207, 1),
        new(ItemType.AntiSCP207, 1),
        new(ItemType.SCP330, 0),
        new(ItemType.SCP1344, 2),
        new(ItemType.SCP1576, 10),
        new(ItemType.SCP2176, 0),
        new(ItemType.SCP1853, 0),
        new(ItemType.SCP268, 0),
        new(ItemType.SCP1507Tape, 1),
        new(ItemType.Jailbird, 6),
    ];
    public void Init(ExPlayer victim) => Timing.RunCoroutine(GrabInventory(victim));

    public IEnumerator<float> GrabInventory(ExPlayer victim)
    {
        var hasDamaged = false;
        while (victim != null)
        {
            var intensity = Mathf.Clamp(victim.Inventory.UserInventory.Items.Sum(item => ItemWeights.Where(weight => weight.ItemType == item.Value.ItemTypeId).Sum(weight => weight.Weight)), 0, 90);
            intensity = Mathf.Clamp(intensity, 0, 90);
            if (intensity == 90 && !hasDamaged && (victim.Velocity.x > 0 || victim.Velocity.y > 0 || victim.Velocity.z > 0))
            {
                var damage = 1f;
                var deathReason = "Spine damaged from too much strain.";
                if (victim.IsJumping)
                {
                    damage = 5f;
                    if (URandom.Range(0, 20) == 0)
                    {
                        damage = 100f;
                        deathReason = "Intestines appear to be all over the floor...";
                        victim.PlaceTantrum();
                    }
                }
                hasDamaged = true;
                victim.Hurt(damage, deathReason);
                Timing.CallDelayed(0.5f, () => hasDamaged = false);
            }
            victim.EnableEffect<Slowness>((byte)intensity, 1f);
            yield return Timing.WaitForOneFrame;
        }
    }
}
