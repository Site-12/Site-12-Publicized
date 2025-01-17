namespace Site12.API.Features;

using System.Collections.Generic;
using System.IO;
using Attributes;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Firearms.Attachments;
using MEC;
using UnityEngine;
using Speaker = Speaker;

public class SoundSfx
{
    [OnPluginEnabled]
    public static void InitEvents()
    {
        if (!Directory.Exists(Path.Combine(Paths.Plugins, "audio")))
            Directory.CreateDirectory(Path.Combine(Paths.Plugins, "audio"));
        if (!Directory.Exists(Path.Combine(Paths.Plugins, "audio", "PluginSFX")))
            Directory.CreateDirectory(Path.Combine(Paths.Plugins, "audio", "PluginSFX"));
        AudioClipStorage.LoadClip(Path.Combine(Paths.Plugins, "audio", "PluginSFX", "SubMachineSFX.ogg"), "SubMachineSFX");
        AudioClipStorage.LoadClip(Path.Combine(Paths.Plugins, "audio", "PluginSFX", "ComSFX.ogg"), "ComSFX");
        AudioClipStorage.LoadClip(Path.Combine(Paths.Plugins, "audio", "PluginSFX", "RevolverSFX.ogg"), "RevolverSFX");
        AudioClipStorage.LoadClip(Path.Combine(Paths.Plugins, "audio", "PluginSFX", "ShotgunSFX.ogg"), "ShotgunSFX");
        AudioClipStorage.LoadClip(Path.Combine(Paths.Plugins, "audio", "PluginSFX", "NatoRifleSFX.ogg"), "NatoRifleSFX");
        AudioClipStorage.LoadClip(Path.Combine(Paths.Plugins, "audio", "PluginSFX", "BricsRifleSFX.ogg"), "BricsRifleSFX");
        AudioClipStorage.LoadClip(Path.Combine(Paths.Plugins, "audio", "PluginSFX", "DisruptorShot.ogg"), "DisruptorShot");
        AudioClipStorage.LoadClip(Path.Combine(Paths.Plugins, "audio", "PluginSFX", "GrenadeSFX.ogg"), "GrenadeSFX");
        AudioClipStorage.LoadClip(Path.Combine(Paths.Plugins, "audio", "PluginSFX", "173Vent.ogg"), "VentingSFX");
        AudioClipStorage.LoadClip(Path.Combine(Paths.Plugins, "audio", "PluginSFX", "Scp914alarm.ogg"), "Scp914alarm");

        PlayerHandlers.Shot += Shot;
        Exiled.Events.Handlers.Map.ExplodingGrenade += ExplodingGrenade;
    }

    private static float _uses;

    public static void ExplodingGrenade(ExplodingGrenadeEventArgs ev)
    {
        var audioPlayer = AudioPlayer.CreateOrGet("Grenade", destroyWhenAllClipsPlayed: true, controllerId: 1);
        audioPlayer.AddSpeaker("GrenadeSpeaker" + _uses, ev.Position, maxDistance: 259, minDistance: 56);
        audioPlayer.AddClip("GrenadeSFX");
        if (_uses >= 500)
            _uses = 0;
        _uses++;
    }

    private static Dictionary<ExPlayer, ActiveSpeaker> _speakerIsActive = new();

    private class ActiveSpeaker(bool justActivated, float timing)
    {
        public bool JustActivated = justActivated;
        public float Timing = timing;
    }
    public static void Shot(ShotEventArgs ev)
    {
        if (ev.Firearm.HasAttachment(AttachmentName.SoundSuppressor))
            return;
        if(!ev.Player.ScomPlayer().AudioPlayer.SpeakersByName.TryGetValue("ShotSFX", out var speaker))
            speaker = ev.Player.ScomPlayer().AudioPlayer.AddSpeaker("ShotSFX", ev.Player.Position, minDistance: 40, maxDistance: 400);

        speaker.Position = ev.Player.Position;

        if(!_speakerIsActive.ContainsKey(ev.Player))
            _speakerIsActive.Add(ev.Player, new ActiveSpeaker(true, 0));

        AudioClipPlayback audioClipPlayback;
        switch (ev.Firearm.FirearmType)
        {
            case FirearmType.Com45:
            case FirearmType.Com15:
            case FirearmType.Com18:
                audioClipPlayback = speaker.Owner.AddClip("ComSFX");
                break;

            case FirearmType.Revolver:
                audioClipPlayback = speaker.Owner.AddClip("RevolverSFX");
                break;

            case FirearmType.FSP9:
            case FirearmType.Crossvec:
                audioClipPlayback = speaker.Owner.AddClip("SubMachineSFX");
                break;

            case FirearmType.Shotgun:
                audioClipPlayback = speaker.Owner.AddClip("ShotgunSFX");
                break;

            case FirearmType.E11SR:
            case FirearmType.FRMG0:
                audioClipPlayback = speaker.Owner.AddClip("NatoRifleSFX");
                break;

            case FirearmType.Logicer:
            case FirearmType.A7:
            case FirearmType.AK:
                audioClipPlayback = speaker.Owner.AddClip("BricsRifleSFX");
                break;

            case FirearmType.ParticleDisruptor:
                audioClipPlayback = speaker.Owner.AddClip("DisruptorShot");
                break;

            case FirearmType.None:
            default:
                speaker.Destroy();
                return;
        }

        _speakerIsActive[ev.Player].Timing = (float)audioClipPlayback.Duration.TotalSeconds;
        if(_speakerIsActive[ev.Player].JustActivated)
            Timing.RunCoroutine(DestroySpeakerAfter(ev.Player, speaker));
    }

    public static IEnumerator<float> DestroySpeakerAfter(ExPlayer player, Speaker speaker)
    {
        _speakerIsActive[player].JustActivated = false;
        while (_speakerIsActive[player].Timing > 0)
        {
            _speakerIsActive[player].Timing -= 1 * Time.deltaTime;
            yield return Timing.WaitForOneFrame;
        }

        _speakerIsActive.Remove(player);
        speaker.Destroy();
    }
}