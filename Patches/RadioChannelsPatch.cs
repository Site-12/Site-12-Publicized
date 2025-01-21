namespace Site12.Patches;

using API.Attributes;
using Exiled.Events.EventArgs.Player;
using Extensions;
using HarmonyLib;
using Hints;
using PlayerRoles;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using VoiceChat;
using VoiceChat.Networking;
using static HarmonyLib.AccessTools;

/// <summary>
/// Allows the usage of radio channels for radios when using the on/off toggle key.
/// </summary>
[HarmonyPatch]
public static class RadioChannelsPatch
{
    static RadioChannelsPatch()
    {
        PlayerRadioChannel = new();

        HintMessage = new()
        {
            DurationScalar = 3f,
            Parameters =
            [
                HintMessageParameter = new StringHintParameter()
            ],
            _effects =
            [
                new AlphaCurveHintEffect(AnimationCurve.Linear(0, 1, 1, 1))
            ]
        };

        RadioChannels = [
        "Security",
        "Research",
        "Medical",
        "Maintenance",
        "Emergency",
        "Command",
        "(OOC) AdminRequest"
        ];
    }

    public static readonly Dictionary<ReferenceHub, int> PlayerRadioChannel;

    public static int MaxChannels => RadioChannels.Length;

    /// <summary>
    /// The channel names that are accessible to users.
    /// </summary>
    /// <remarks>There must always be atleast one channel to prevent errors.</remarks>
    public static readonly string[] RadioChannels;

    private static readonly TextHint HintMessage;

    private static readonly StringHintParameter HintMessageParameter;

    [OnPluginEnabled]
    internal static void InitEvents()
    {
        ServerHandlers.WaitingForPlayers += PlayerRadioChannel.Clear;
        PlayerHandlers.TogglingRadio += OnTogglingRadio;
    }

    private static void OnTogglingRadio(TogglingRadioEventArgs ev)
    {
        if (ev.NewState) // Set to first channel when enabling radio.
        {
            ShowHintText(ev.Player, $"<b>RADIO CHANNEL: {RadioChannels[0]}</b>");

            PlayerRadioChannel[ev.Player.ReferenceHub] = 0;
            ev.IsAllowed = true;
            return;
        }

        GetRadioChannel(ev.Player.ReferenceHub, out var channel);
        channel = GetNextChannel(channel);

        if (channel == -1) // Turning off radio.
        {
            ShowHintText(ev.Player, "<b><color=#FF0000>RADIO OFF</color></b>");

            PlayerRadioChannel[ev.Player.ReferenceHub] = -1;
            ev.IsAllowed = true;
            return;
        }

        // Cycling to next channel.
        ShowHintText(ev.Player, $"<b>RADIO CHANNEL: {RadioChannels[channel]}</b>");

        PlayerRadioChannel[ev.Player.ReferenceHub] = channel;
        ev.IsAllowed = false;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

        // VoiceChatChannel voiceChatChannel2 = voiceRole2.VoiceModule.ValidateReceive(msg.Speaker, voiceChatChannel);
        //
        // <------- Patch goes here.
        //
        // if (voiceChatChannel2 != 0)
        // {
        //     msg.Channel = voiceChatChannel2;
        //     allHub.connectionToClient.Send(msg);
        // }
        int index = newInstructions.FindLastIndex(x => x.opcode == OpCodes.Stloc_S && x.operand is LocalBuilder local && local.LocalIndex == 5);

        // RadioChannelsPatch.OnTransceiving(ref VoiceMessage msg, ref VoiceChatChannel voiceChatChannel2, hub);
        newInstructions.InsertRange(index + 1, new CodeInstruction[]
        {
            // ref VoiceMessage msg
            new(OpCodes.Ldarga_S, 1),

            // ref VoiceChatChannel voiceChatChannel2
            new(OpCodes.Ldloca_S, 5),

            // ReferenceHub hub
            new(OpCodes.Ldloc_S, 3),

            // RadioChannelsPatch.OnTransceiving(ref VoiceMessage msg, ref VoiceChatChannel voiceChatChannel2, hub);
            new(OpCodes.Call, Method(typeof(RadioChannelsPatch), nameof(OnTransceiving))),
        });

        return newInstructions.FinishTranspiler();
    }

    private static void ShowHintText(ExPlayer player, string value)
    {
        HintMessage.Text = value;
        HintMessageParameter.Value = value;
        player.ReferenceHub.hints.Show(HintMessage);
    }

    private static void OnTransceiving(ref VoiceMessage msg, ref VoiceChatChannel channel, ReferenceHub recv)
    {
        if (channel != VoiceChatChannel.Radio)
            return;

        if (recv.roleManager.CurrentRole.Team == Team.SCPs)
            return;

        if (recv.roleManager.CurrentRole.Team == Team.Dead)
            return;

        GetRadioChannel(recv, out int recvChannel);
        GetRadioChannel(msg.Speaker, out int speakerChannel);

        if (recvChannel == -1 || speakerChannel == -1 || recvChannel != speakerChannel)
        {
            channel = VoiceChatChannel.None;
        }
    }

    private static bool GetRadioChannel(ReferenceHub hub, out int channel)
    {
        if (!PlayerRadioChannel.TryGetValue(hub, out channel))
        {
            PlayerRadioChannel.Add(hub, channel = 0);
        }

        channel = Mathf.Clamp(channel, 0, MaxChannels - 1);
        return true;
    }

    private static int GetNextChannel(int channel)
    {
        channel = Mathf.Clamp(channel, -1, MaxChannels - 1);

        if (channel == -1)
            return 0;

        if (++channel == MaxChannels)
        {
            return -1;
        }

        return channel;
    }
}
