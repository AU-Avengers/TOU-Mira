using System.Runtime.CompilerServices;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.Modifiers;
using PerfectComms.Api;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.HnsImpostor;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Impostor.Herbalist;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Modules.RainbowMod;
using TownOfUs.Patches;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TownOfUs.Integrations;

/// <summary>
/// Optional Perfect Comms entry point plus the source-owned state, RPC, and Jailor UI needed by
/// the integration. Perfect Comms types remain behind a no-inline bridge so TOU still loads when
/// the soft dependency is absent.
/// </summary>
internal static class PerfectCommsIntegration
{
    internal const string PluginId = "com.edgetel.perfectcomms";
    internal const string ModId = "auavengers.tou.mira";
    private const uint SetJaileeVoiceAllowedRpc = 0x50430001;

    private static readonly HashSet<byte> MeetingBlackmailedPlayers = [];
    private static readonly HashSet<byte> NextRoundBlackmailedPlayers = [];
    private static readonly HashSet<byte> JailVoiceAllowedPlayers = [];
    private static readonly Dictionary<byte, GameObject> JailVoiceButtons = [];

    internal static bool Registered { get; private set; }
    internal static bool JailorCanUnmuteJailed { get; set; }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void TryRegister()
    {
        if (!IL2CPPChainloader.Instance.Plugins.ContainsKey(PluginId))
        {
            return;
        }

        RegisterAvailableRuntime();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RegisterAvailableRuntime()
    {
        PerfectCommsRuntime.Register();
    }

    internal static void MarkRegistered()
    {
        Registered = true;
        JailorCanUnmuteJailed = true;
    }

    internal static void Reset()
    {
        Registered = false;
        JailorCanUnmuteJailed = false;
        MeetingBlackmailedPlayers.Clear();
        NextRoundBlackmailedPlayers.Clear();
        JailVoiceAllowedPlayers.Clear();
        foreach (var button in JailVoiceButtons.Values)
        {
            button?.Destroy();
        }
        JailVoiceButtons.Clear();
    }

    internal static void BeginMeeting()
    {
        MeetingBlackmailedPlayers.Clear();
        NextRoundBlackmailedPlayers.Clear();
        JailVoiceAllowedPlayers.Clear();
    }

    internal static void TrackMeetingBlackmail(byte playerId)
    {
        MeetingBlackmailedPlayers.Add(playerId);
    }

    internal static void CommitMeetingBlackmail()
    {
        NextRoundBlackmailedPlayers.Clear();
        NextRoundBlackmailedPlayers.UnionWith(MeetingBlackmailedPlayers);
        MeetingBlackmailedPlayers.Clear();
        JailVoiceAllowedPlayers.Clear();
    }

    internal static bool IsBlackmailedNextRound(byte playerId)
        => NextRoundBlackmailedPlayers.Contains(playerId);

    internal static bool IsJailVoiceAllowed(byte playerId)
        => JailVoiceAllowedPlayers.Contains(playerId);

    private static void SetJailVoiceAllowed(byte playerId, bool allowed)
    {
        if (allowed)
        {
            JailVoiceAllowedPlayers.Add(playerId);
        }
        else
        {
            JailVoiceAllowedPlayers.Remove(playerId);
        }
    }

    [MethodRpc(SetJaileeVoiceAllowedRpc)]
    private static void RpcSetJaileeVoiceAllowed(PlayerControl jailor, byte jaileeId, bool allowed)
    {
        if (jailor?.Data?.Role is not JailorRole)
        {
            return;
        }

        var jailee = MiscUtils.PlayerById(jaileeId);
        var jail = jailee?.GetModifier<JailedModifier>();
        if (jail == null || jail.JailorId != jailor.PlayerId)
        {
            return;
        }

        SetJailVoiceAllowed(jaileeId, allowed);
    }

    internal static void TryCreateJailVoiceButton(JailorRole jailorRole)
    {
        var jailor = jailorRole.Player;
        var jailee = jailorRole.Jailed;
        var meeting = MeetingHud.Instance;
        if (!Registered || !JailorCanUnmuteJailed || meeting == null ||
            !jailor.AmOwner || jailor.HasDied() || jailee == null || jailee.HasDied())
        {
            return;
        }

        var voteArea = meeting.playerStates.FirstOrDefault(area => area.TargetPlayerId == jailee.PlayerId);
        if (voteArea == null)
        {
            return;
        }

        ClearJailVoiceButton(jailor.PlayerId);
        var confirmButton = voteArea.Buttons.transform.GetChild(0).gameObject;
        var buttonObject = Object.Instantiate(confirmButton, voteArea.transform);
        buttonObject.name = "JailVoiceButton";
        buttonObject.transform.position = confirmButton.transform.position + new Vector3(0.75f, 0f, 0f);
        buttonObject.transform.localScale *= 0.8f;
        buttonObject.layer = 5;
        buttonObject.transform.parent = confirmButton.transform.parent.parent;

        var labelObject = Object.Instantiate(
            meeting.MeetingAbilityButton.buttonLabelText.gameObject,
            buttonObject.transform);
        labelObject.transform.localPosition = new Vector3(0f, -0.2f, 0f);
        var label = labelObject.GetComponent<TextMeshPro>();
        label.color = Color.white;
        label.fontSize = 2.25f;
        label.fontSizeMax = 2.25f;
        label.fontSizeMin = 2.25f;
        label.m_enableWordWrapping = false;

        var renderer = buttonObject.GetComponent<SpriteRenderer>();
        renderer.sprite = LegacyAssets.IsLegacy
            ? LegacyCrewAssets.JailSprite.LoadAsset()
            : TouCrewAssets.JailSprite.LoadAsset();

        void RefreshLabel()
        {
            label.text = IsJailVoiceAllowed(jailee.PlayerId)
                ? TouLocale.Get("TouRoleJailorBlockVoice", "Block Voice")
                : TouLocale.Get("TouRoleJailorAllowVoice", "Allow Voice");
        }

        var passive = buttonObject.GetComponent<PassiveButton>();
        passive.OnClick = new Button.ButtonClickedEvent();
        passive.OnClick.AddListener((Action)(() =>
        {
            if (!Registered || !JailorCanUnmuteJailed || jailor.HasDied() || jailee.HasDied())
            {
                return;
            }

            var jail = jailee.GetModifier<JailedModifier>();
            if (jail == null || jail.JailorId != jailor.PlayerId)
            {
                return;
            }

            bool allowed = !IsJailVoiceAllowed(jailee.PlayerId);
            SetJailVoiceAllowed(jailee.PlayerId, allowed);
            RpcSetJaileeVoiceAllowed(jailor, jailee.PlayerId, allowed);
            RefreshLabel();
        }));

        RefreshLabel();
        JailVoiceButtons[jailor.PlayerId] = buttonObject;
    }

    private static void ClearJailVoiceButton(byte jailorId)
    {
        if (!JailVoiceButtons.Remove(jailorId, out var button))
        {
            return;
        }

        button?.Destroy();
    }

    [HarmonyPatch(typeof(ModCompatibility), nameof(ModCompatibility.Initialize))]
    private static class InitializePatch
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            TryRegister();
        }
    }

    [HarmonyPatch(typeof(JailorRole), nameof(JailorRole.OnMeetingStart))]
    private static class JailorMeetingPatch
    {
        [HarmonyPostfix]
        private static void Postfix(JailorRole __instance)
        {
            TryCreateJailVoiceButton(__instance);
        }
    }

    [HarmonyPatch(typeof(JailorRole), nameof(JailorRole.OnVotingComplete))]
    private static class JailorVotingCompletePatch
    {
        [HarmonyPostfix]
        private static void Postfix(JailorRole __instance)
        {
            ClearJailVoiceButton(__instance.Player.PlayerId);
        }
    }
}

/// <summary>
/// Complete source-owned replacement for Perfect Comms' frozen TOU-Mira compatibility adapter.
/// Only the presence-gated bridge above enters this type.
/// </summary>
internal static class PerfectCommsRuntime
{
    private const string MuteBlackmailedInMeetings = nameof(MuteBlackmailedInMeetings);
    private const string MuteBlackmailedNextRound = nameof(MuteBlackmailedNextRound);
    private const string MuteParasiteControlled = nameof(MuteParasiteControlled);
    private const string ParasiteHearFromVictim = nameof(ParasiteHearFromVictim);
    private const string MutePuppeteerControlled = nameof(MutePuppeteerControlled);
    private const string PuppeteerHearFromVictim = nameof(PuppeteerHearFromVictim);
    private const string MuteSwooperWhileSwooped = nameof(MuteSwooperWhileSwooped);
    private const string MuffleBlindedOrFlashedHearing = nameof(MuffleBlindedOrFlashedHearing);
    private const string MuffleHypnotizedDuringHysteria = nameof(MuffleHypnotizedDuringHysteria);
    private const string CrewpostorUsesImpostorVoice = nameof(CrewpostorUsesImpostorVoice);
    private const string MuteGlitchHacked = nameof(MuteGlitchHacked);
    private const string MuteJailedInMeetings = nameof(MuteJailedInMeetings);
    private const string JailPersistsAfterJailorDeath = nameof(JailPersistsAfterJailorDeath);
    private const string JailorCanUnmuteJailed = nameof(JailorCanUnmuteJailed);
    private const string MediumGhostVoice = nameof(MediumGhostVoice);
    private const string TeamRadioVampires = nameof(TeamRadioVampires);
    private const string TeamRadioLovers = nameof(TeamRadioLovers);

    // Object-typed caches keep Perfect Comms types out of this type's field signatures. This matters
    // when Harmony enumerates TOU types while the optional runtime assembly is absent.
    private static readonly object HackedMuted = VoiceRuleResult.Mute("Hacked");
    private static readonly object SwoopedMuted = VoiceRuleResult.Mute("Swooped");
    private static readonly object BlackmailedMuted = VoiceRuleResult.Mute("Blackmailed");
    private static readonly object JailedMuted = VoiceRuleResult.Mute("Jailed");
    private static readonly object ParasiteMuted = VoiceRuleResult.Mute("Parasite controlled");
    private static readonly object PuppeteerMuted = VoiceRuleResult.Mute("Puppeteer controlled");
    private static readonly object MediumPrivateMuted = VoicePairResult.Mute("Medium private voice");
    private static readonly object MediumDirectionMuted = VoicePairResult.Mute("Medium direction disabled");
    private static readonly object NonSelectedGhostMuted = VoicePairResult.Mute("Non-selected ghost");
    private static readonly object ListenerMuffled = new VoiceListenerFilterResult(true);
    private static readonly object ListenerNormal = new VoiceListenerFilterResult(false);
    private static readonly object VampireRadio = new VoiceManagedRadioChannelResult("vampires", "Vampires", "V");
    private static readonly Dictionary<ushort, object> LoverRadios = [];

    private static bool _registered;

    internal static void Register()
    {
        if (_registered)
        {
            return;
        }

        var required =
            VoiceApiCapability.PerSpeakerMuffle |
            VoiceApiCapability.ContextualListeners |
            VoiceApiCapability.PairRouting |
            VoiceApiCapability.PlayerTraits |
            VoiceApiCapability.PhaseObservers |
            VoiceApiCapability.ConditionalHostOptions |
            VoiceApiCapability.OverlayPrivacy |
            VoiceApiCapability.ManagedTeamRadio |
            VoiceApiCapability.PersistentHostOptions |
            VoiceApiCapability.IntegrationOwnership |
            VoiceApiCapability.OverlayAppearance;
        if (!PerfectCommsApi.Supports(required))
        {
            Warning($"Perfect Comms {PerfectCommsApi.RuntimeApiVersion} does not expose every TOU-Mira integration capability; keeping its legacy adapter active.");
            return;
        }

        try
        {
            RegisterOptions();
            PerfectCommsApi.RegisterVoiceRule(PerfectCommsIntegration.ModId, ResolveSpeakerRule);
            PerfectCommsApi.RegisterVoicePhaseObserver(PerfectCommsIntegration.ModId, ObservePhase);
            PerfectCommsApi.RegisterContextualListenerOrigin(PerfectCommsIntegration.ModId, ResolveListenerOrigin);
            PerfectCommsApi.RegisterContextualListenerFilter(PerfectCommsIntegration.ModId, ResolveListenerFilter);
            PerfectCommsApi.RegisterVoicePlayerTraits(PerfectCommsIntegration.ModId, ResolvePlayerTraits);
            PerfectCommsApi.RegisterVoicePairRule(PerfectCommsIntegration.ModId, ResolveMediumPair);
            PerfectCommsApi.RegisterManagedRadioChannel(PerfectCommsIntegration.ModId, ResolveVampireRadio);
            PerfectCommsApi.RegisterManagedRadioChannel(PerfectCommsIntegration.ModId, ResolveLoversRadio);
            PerfectCommsApi.RegisterOverlayViewerRule(PerfectCommsIntegration.ModId, ResolveOverlayViewer);
            PerfectCommsApi.RegisterOverlaySpeakerRule(PerfectCommsIntegration.ModId, ResolveOverlaySpeaker);
            PerfectCommsApi.RegisterAnimatedColorRule(PerfectCommsIntegration.ModId, RainbowUtils.IsRainbow);

            // Ownership is deliberately last: partial registration must never suppress the proven fallback.
            PerfectCommsApi.RegisterIntegrationOwner(
                PerfectCommsIntegration.ModId,
                VoiceIntegrationIds.TouMira);

            _registered = true;
            PerfectCommsIntegration.MarkRegistered();
            Info($"Registered source-owned TOU-Mira voice integration with Perfect Comms API {PerfectCommsApi.RuntimeApiVersion}.");
        }
        catch (Exception ex)
        {
            PerfectCommsApi.Unregister(PerfectCommsIntegration.ModId);
            PerfectCommsIntegration.Reset();
            Error($"Perfect Comms integration failed; its legacy TOU-Mira adapter remains active: {ex}");
        }
    }

    private static void RegisterOptions()
    {
        PerfectCommsApi.RegisterModTab(PerfectCommsIntegration.ModId, "TOU Mira");

        RegisterToggle(MuteBlackmailedInMeetings,
            "<color=#FF0000><b>Blackmailer</b></color>: Mute Blackmailed in Meetings", true,
            "Prevents the currently blackmailed player from transmitting voice during meetings.");
        RegisterToggle(MuteBlackmailedNextRound,
            "<color=#FF0000><b>Blackmailer</b></color>: Mute Blackmailed Next Round", false,
            "Keeps a meeting-blackmailed player voice-muted during the following task round.");
        RegisterToggle(MuteParasiteControlled,
            "<color=#FF0000><b>Parasite</b></color>: Mute Controlled Victim", true,
            "Prevents a player marked by the Parasite from transmitting their own voice while the effect is active.");
        RegisterToggle(ParasiteHearFromVictim,
            "<color=#FF0000><b>Parasite</b></color>: Also Hear Controlled Victim", true,
            "Lets the Parasite also hear the voices audible around its marked victim while remaining at the Parasite's own position.");
        RegisterToggle(MutePuppeteerControlled,
            "<color=#FF0000><b>Puppeteer</b></color>: Mute Controlled Victim", true,
            "Prevents a Puppeteer-controlled player from transmitting their own voice while controlled.");
        RegisterToggle(PuppeteerHearFromVictim,
            "<color=#FF0000><b>Puppeteer</b></color>: Hear From Controlled Victim", true,
            "Lets the Puppeteer hear the voices audible around the player it currently controls.");
        RegisterToggle(MuteSwooperWhileSwooped,
            "<color=#FF0000><b>Swooper</b></color>: Mute While Swooped", true,
            "Prevents an invisible Swooper from transmitting voice until the swoop ends.");
        RegisterToggle(MuffleBlindedOrFlashedHearing,
            "<color=#FF0000><b>Eclipsal/Grenadier</b></color>: Muffle Blinded/Flashed Hearing", true,
            "Muffles incoming voice only for players currently blinded by Eclipsal or flashed by Grenadier.");
        RegisterToggle(MuffleHypnotizedDuringHysteria,
            "<color=#FF0000><b>Hypnotist</b></color>: Muffle Hypnotized During Hysteria", true,
            "Muffles incoming voice only for affected hypnotized players while Mass Hysteria is active.");
        RegisterToggle(CrewpostorUsesImpostorVoice,
            "<color=#FF0000><b>Crewpostor</b></color>: Use Impostor Voice", true,
            "Treats Crewpostor as an impostor for private impostor voice and team-radio routing.");
        RegisterToggle(MuteGlitchHacked,
            "<color=#00FF00><b>Glitch</b></color>: Mute Hacked Players", true,
            "Prevents a player affected by the Glitch's Hack ability from transmitting voice until the hack ends.");
        RegisterToggle(MuteJailedInMeetings,
            "<color=#A6A6A6><b>Jailor</b></color>: Mute Jailee in Meetings", true,
            "Prevents the jailed player from transmitting voice during meetings unless the Jailor temporarily unmutes them.");
        RegisterToggle(JailPersistsAfterJailorDeath,
            "<color=#A6A6A6><b>Jailor</b></color>: Jail Persists If Jailor Dies", false,
            "Keeps the meeting voice jail active even if the Jailor is dead.",
            context => context.GetOption(MuteJailedInMeetings));
        RegisterToggle(JailorCanUnmuteJailed,
            "<color=#A6A6A6><b>Jailor</b></color>: Can Unmute Jailee", true,
            "Lets the Jailor temporarily allow the jailed player to speak during a meeting.");
        RegisterToggle(TeamRadioVampires,
            "Team Radio - <color=#A32929><b>Vampires</b></color>", true,
            "Enables the private Vampire managed Team Radio channel when Team Radio is on.");
        RegisterToggle(TeamRadioLovers,
            "Team Radio - <color=#FF66CC><b>Lovers</b></color>", true,
            "Enables the private Lovers managed Team Radio channel when Team Radio is on.");

        PerfectCommsApi.RegisterHostEnumOption(
            PerfectCommsIntegration.ModId,
            new VoiceHostEnumOption(
                MediumGhostVoice,
                "<color=#A680FF><b>Medium</b></color>: Ghost Voice",
                0,
                ["None", "Medium -> Ghost", "Ghost -> Medium", "Both"])
            {
                Description = "Chooses which voice direction is allowed between a Medium and dead players during tasks.",
            });
    }

    private static void RegisterToggle(
        string key,
        string label,
        bool defaultValue,
        string description,
        Func<VoiceHostOptionContext, bool>? visible = null)
    {
        PerfectCommsApi.RegisterHostOption(
            PerfectCommsIntegration.ModId,
            new VoiceHostOption(key, label, defaultValue)
            {
                Description = description,
                Visible = visible,
            });
    }

    private static VoiceRuleResult ResolveSpeakerRule(VoiceRuleContext context)
    {
        bool deliberation = context.Phase is VoicePhaseKind.Meeting or VoicePhaseKind.Exile;
        bool liveGame = context.Phase is VoicePhaseKind.Tasks or VoicePhaseKind.Meeting or VoicePhaseKind.Exile;
        PerfectCommsIntegration.JailorCanUnmuteJailed = context.GetOption(JailorCanUnmuteJailed);

        if (context.IsDead)
        {
            return VoiceRuleResult.Pass;
        }

        if (deliberation && context.Player.HasModifier<BlackmailedModifier>())
        {
            PerfectCommsIntegration.TrackMeetingBlackmail(context.Player.PlayerId);
            if (context.GetOption(MuteBlackmailedInMeetings))
            {
                return (VoiceRuleResult)BlackmailedMuted;
            }
        }

        if (liveGame && context.GetOption(MuteGlitchHacked) && IsActivelyGlitchHacked(context.Player))
        {
            return (VoiceRuleResult)HackedMuted;
        }

        if (liveGame && context.GetOption(MuteSwooperWhileSwooped) && context.Player.HasModifier<SwoopModifier>())
        {
            return (VoiceRuleResult)SwoopedMuted;
        }

        if (deliberation && context.GetOption(MuteJailedInMeetings) &&
            context.Player.TryGetModifier<JailedModifier>(out var jail))
        {
            bool jailorValid = context.GetOption(JailPersistsAfterJailorDeath) || IsLivingJailor(jail.JailorId);
            bool temporarilyAllowed =
                PerfectCommsIntegration.JailorCanUnmuteJailed &&
                PerfectCommsIntegration.IsJailVoiceAllowed(context.Player.PlayerId);
            if (jailorValid && !temporarilyAllowed)
            {
                return (VoiceRuleResult)JailedMuted;
            }
        }

        if (context.Phase != VoicePhaseKind.Tasks)
        {
            return VoiceRuleResult.Pass;
        }

        if (context.GetOption(MuteBlackmailedNextRound) &&
            PerfectCommsIntegration.IsBlackmailedNextRound(context.Player.PlayerId))
        {
            return (VoiceRuleResult)BlackmailedMuted;
        }

        if (context.GetOption(MuteParasiteControlled) && context.Player.HasModifier<ParasiteInfectedModifier>())
        {
            return (VoiceRuleResult)ParasiteMuted;
        }

        if (context.GetOption(MutePuppeteerControlled) && context.Player.HasModifier<PuppeteerControlModifier>())
        {
            return (VoiceRuleResult)PuppeteerMuted;
        }

        return VoiceRuleResult.Pass;
    }

    private static void ObservePhase(VoicePhaseChangedContext context)
    {
        PerfectCommsIntegration.JailorCanUnmuteJailed = context.GetOption(JailorCanUnmuteJailed);
        if (context.Phase is VoicePhaseKind.Lobby or VoicePhaseKind.Meeting)
        {
            PerfectCommsIntegration.BeginMeeting();
        }
        else if ((context.PreviousPhase is VoicePhaseKind.Meeting or VoicePhaseKind.Exile) &&
                 context.Phase == VoicePhaseKind.Tasks)
        {
            PerfectCommsIntegration.CommitMeetingBlackmail();
        }
    }

    private static VoiceListenerResult? ResolveListenerOrigin(VoiceListenerContext context)
    {
        if (context.Phase != VoicePhaseKind.Tasks)
        {
            return null;
        }

        if (context.GetOption(PuppeteerHearFromVictim) && FindPuppeteerVictim(context.Listener) is { } puppet)
        {
            return new VoiceListenerResult(
                puppet.GetTruePosition(),
                ResolveLightRadius(puppet),
                VoiceListenerMode.Replace);
        }

        if (context.GetOption(ParasiteHearFromVictim) && FindParasiteVictim(context.Listener) is { } victim)
        {
            return new VoiceListenerResult(
                victim.GetTruePosition(),
                ResolveLightRadius(victim),
                VoiceListenerMode.Additive);
        }

        return null;
    }

    private static VoiceListenerFilterResult ResolveListenerFilter(VoiceListenerContext context)
    {
        bool blinded =
            context.Phase == VoicePhaseKind.Tasks &&
            context.GetOption(MuffleBlindedOrFlashedHearing) &&
            (context.Listener.HasModifier<EclipsalBlindModifier>() ||
             context.Listener.HasModifier<GrenadierFlashModifier>());
        bool hypnotized =
            context.GetOption(MuffleHypnotizedDuringHysteria) &&
            context.Listener.GetModifier<HypnotisedModifier>()?.HysteriaActive == true;
        return (VoiceListenerFilterResult)(blinded || hypnotized ? ListenerMuffled : ListenerNormal);
    }

    private static VoicePlayerTraits ResolvePlayerTraits(VoiceRuleContext context)
        => context.GetOption(CrewpostorUsesImpostorVoice) && context.Player.HasModifier<CrewpostorModifier>()
            ? VoicePlayerTraits.ImpostorVoice
            : VoicePlayerTraits.None;

    private static VoicePairResult ResolveMediumPair(VoicePairContext context)
    {
        if (context.Phase != VoicePhaseKind.Tasks)
        {
            return VoicePairResult.Pass;
        }

        int mode = context.GetEnumOption(MediumGhostVoice);
        if (mode == 0)
        {
            return VoicePairResult.Pass;
        }

        bool mediumToGhost = mode is 1 or 3;
        bool ghostToMedium = mode is 2 or 3;
        if (TryGetActiveMedium(context.Speaker, out var speakerMedium))
        {
            if (!context.ListenerIsDead)
            {
                return (VoicePairResult)MediumPrivateMuted;
            }
            if (!mediumToGhost)
            {
                return (VoicePairResult)MediumDirectionMuted;
            }

            return VoicePairResult.Route(
                VoicePairRouteShape.Proximity,
                speakerOrigin: speakerMedium.Spirit!.transform.position,
                listenerOrigin: context.Listener.GetTruePosition(),
                reason: "Medium to ghost");
        }

        if (ghostToMedium && TryGetActiveMedium(context.Listener, out var listenerMedium) && context.SpeakerIsDead)
        {
            var mediated = context.Speaker.GetModifier<MediatedModifier>();
            if (mediated == null || mediated.MediumId != context.Listener.PlayerId)
            {
                return (VoicePairResult)NonSelectedGhostMuted;
            }

            return VoicePairResult.Route(
                VoicePairRouteShape.Ghost,
                speakerOrigin: context.Speaker.GetTruePosition(),
                listenerOrigin: listenerMedium.Spirit!.transform.position,
                reason: "Ghost to Medium");
        }

        return VoicePairResult.Pass;
    }

    private static VoiceManagedRadioChannelResult? ResolveVampireRadio(VoiceRuleContext context)
        => !context.IsDead && context.GetOption(TeamRadioVampires) && context.Player.Data?.Role is VampireRole
            ? (VoiceManagedRadioChannelResult)VampireRadio
            : null;

    private static VoiceManagedRadioChannelResult? ResolveLoversRadio(VoiceRuleContext context)
    {
        if (context.IsDead || !context.GetOption(TeamRadioLovers) ||
            context.Player.GetModifier<LoverModifier>()?.OtherLover is not { } partner)
        {
            return null;
        }

        byte low = Math.Min(context.Player.PlayerId, partner.PlayerId);
        byte high = Math.Max(context.Player.PlayerId, partner.PlayerId);
        ushort pairId = (ushort)((low << 8) | high);
        if (!LoverRadios.TryGetValue(pairId, out var cached))
        {
            cached = new VoiceManagedRadioChannelResult($"lovers:{low}:{high}", "Lovers", "L");
            LoverRadios[pairId] = cached;
        }

        return (VoiceManagedRadioChannelResult)cached;
    }

    private static VoiceOverlayViewerResult ResolveOverlayViewer(VoiceOverlayViewerContext context)
    {
        if (context.Phase == VoicePhaseKind.Lobby)
        {
            return VoiceOverlayViewerResult.Pass;
        }

        bool hideAll =
            context.Viewer.HasModifier<HerbalistConfusedModifier>() ||
            context.Viewer.GetModifier<HypnotisedModifier>()?.HysteriaActive == true ||
            context.Viewer.HasModifier<EclipsalBlindModifier>() ||
            context.Viewer.HasModifier<HnsGlobalCamouflageModifier>() ||
            (int)context.Viewer.CurrentOutfitType == 3 ||
            HudManagerPatches.CamouflageCommsEnabled;
        if (hideAll)
        {
            return VoiceOverlayViewerResult.HideAll;
        }

        if (!context.Viewer.HasModifier<GrenadierFlashModifier>())
        {
            return VoiceOverlayViewerResult.Pass;
        }

        bool impostorVoice =
            context.Viewer.Data?.Role?.IsImpostor == true ||
            (context.GetOption(CrewpostorUsesImpostorVoice) && context.Viewer.HasModifier<CrewpostorModifier>());
        return !context.IsDead && !impostorVoice
            ? VoiceOverlayViewerResult.HideAll
            : VoiceOverlayViewerResult.DimAll;
    }

    private static VoiceOverlaySpeakerResult ResolveOverlaySpeaker(VoiceOverlaySpeakerContext context)
    {
        if (context.Phase == VoicePhaseKind.Lobby)
        {
            return VoiceOverlaySpeakerResult.Pass;
        }

        var source = context.Speaker;
        byte? aliasId = null;
        foreach (var concealed in source.GetModifiers<ConcealedModifier>())
        {
            PlayerControl? target = concealed switch
            {
                MorphlingMorphModifier morph => morph.Target,
                GlitchMimicModifier mimic => mimic.Target,
                ShapeshifterShiftModifier shift => shift.Target,
                _ => null,
            };
            if (target == null || (aliasId.HasValue && aliasId.Value != target.PlayerId))
            {
                return VoiceOverlaySpeakerResult.HideSource;
            }

            aliasId = target.PlayerId;
        }

        int outfitType = (int)source.CurrentOutfitType;
        if (source.HasModifier<ParasiteInfectedModifier>() && outfitType == 7)
        {
            return VoiceOverlaySpeakerResult.HideSource;
        }
        if (aliasId.HasValue)
        {
            return VoiceOverlaySpeakerResult.Alias(aliasId.Value);
        }
        if (outfitType is not 0 and not 2)
        {
            return VoiceOverlaySpeakerResult.HideSource;
        }

        try
        {
            if (source.cosmetics.currentBodySprite.BodySprite.color.a < 0.95f)
            {
                return VoiceOverlaySpeakerResult.HideSource;
            }
        }
        catch
        {
            return VoiceOverlaySpeakerResult.HideSource;
        }

        if (context.Phase == VoicePhaseKind.Tasks && (!source.Visible || source.shouldAppearInvisible))
        {
            return VoiceOverlaySpeakerResult.HideSource;
        }

        return VoiceOverlaySpeakerResult.Pass;
    }

    private static bool IsActivelyGlitchHacked(PlayerControl player)
        => player.GetModifier<GlitchHackedModifier>() is { ShouldHideHacked: false };

    private static bool IsLivingJailor(byte playerId)
    {
        var jailor = MiscUtils.PlayerById(playerId);
        return jailor?.Data?.IsDead != true && jailor?.Data?.Role is JailorRole;
    }

    private static PlayerControl? FindPuppeteerVictim(PlayerControl controller)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player?.GetModifier<PuppeteerControlModifier>()?.Controller?.PlayerId == controller.PlayerId)
            {
                return player;
            }
        }

        return null;
    }

    private static PlayerControl? FindParasiteVictim(PlayerControl controller)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player?.GetModifier<ParasiteInfectedModifier>()?.Controller?.PlayerId == controller.PlayerId)
            {
                return player;
            }
        }

        return null;
    }

    private static float ResolveLightRadius(PlayerControl player)
    {
        try
        {
            if (player.Data != null && ShipStatus.Instance != null)
            {
                return ShipStatus.Instance.CalculateLightRadius(player.Data);
            }
        }
        catch
        {
            // Perfect Comms normalizes -1 to the listener's radius during scene transitions.
        }

        return -1f;
    }

    private static bool TryGetActiveMedium(PlayerControl player, out MediumRole medium)
    {
        medium = player.Data?.Role as MediumRole ?? null!;
        return medium != null && medium.Spirit != null;
    }
}
