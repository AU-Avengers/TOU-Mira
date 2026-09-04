using Il2CppInterop.Runtime.InteropTypes.Arrays;
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
    private const uint SetJaileeVoiceAllowedRpc = 0x50430001;
    private const float JailVoiceMaintenanceSeconds = 0.25f;
    private const float JailVoiceHeartbeatSeconds = 2f;
    private const float JailVoiceHitboxMinimumWidth = 0.55f;
    private const float JailVoiceHitboxMinimumHeight = 0.45f;
    private const float JailVoiceHitboxHorizontalPadding = 0.08f;
    private const float JailVoiceHitboxVerticalPadding = 0.06f;

    private static readonly HashSet<byte> MeetingBlackmailedPlayers = [];
    private static readonly HashSet<byte> NextRoundBlackmailedPlayers = [];
    private static readonly HashSet<byte> JailVoiceAllowedPlayers = [];
    private static readonly Dictionary<byte, GameObject> JailVoiceButtons = [];
    private static readonly List<byte> ScratchPlayerIds = [];
    private static float _nextJailVoiceMaintenanceTime;
    private static float _nextJailVoiceHeartbeatTime;
    private static bool _muteBlackmailedInMeetings;
    private static bool _muteBlackmailedNextRound;

    internal static bool Registered { get; private set; }
    internal static bool JailVoiceControlEnabled { get; private set; }

    internal static void MarkRegistered()
    {
        Registered = true;
        JailVoiceControlEnabled = false;
    }

    internal static void Reset()
    {
        Registered = false;
        JailVoiceControlEnabled = false;
        MeetingBlackmailedPlayers.Clear();
        NextRoundBlackmailedPlayers.Clear();
        JailVoiceAllowedPlayers.Clear();
        _muteBlackmailedInMeetings = false;
        _muteBlackmailedNextRound = false;
        _nextJailVoiceMaintenanceTime = 0f;
        _nextJailVoiceHeartbeatTime = 0f;
        ClearAllJailVoiceButtons();
        ScratchPlayerIds.Clear();
    }

    internal static void UpdateJailVoiceOptions(bool muteJailed, bool canUnmute)
    {
        JailVoiceControlEnabled = muteJailed && canUnmute;
    }

    internal static void UpdateBlackmailOptions(bool muteInMeetings, bool muteNextRound)
    {
        if (_muteBlackmailedInMeetings && !muteInMeetings)
        {
            MeetingBlackmailedPlayers.Clear();
        }
        if (_muteBlackmailedNextRound && !muteNextRound)
        {
            NextRoundBlackmailedPlayers.Clear();
        }

        _muteBlackmailedInMeetings = muteInMeetings;
        _muteBlackmailedNextRound = muteNextRound;
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

    internal static void CommitMeetingBlackmail(bool carryIntoNextRound)
    {
        NextRoundBlackmailedPlayers.Clear();
        if (carryIntoNextRound)
        {
            NextRoundBlackmailedPlayers.UnionWith(MeetingBlackmailedPlayers);
        }
        MeetingBlackmailedPlayers.Clear();
        JailVoiceAllowedPlayers.Clear();
    }

    internal static bool IsBlackmailedNextRound(byte playerId)
        => NextRoundBlackmailedPlayers.Contains(playerId);

    internal static bool IsJailVoiceAllowed(byte playerId)
        => JailVoiceAllowedPlayers.Contains(playerId);
    internal static bool IsLivingJailor(byte playerId)
    {
        var jailor = MiscUtils.PlayerById(playerId);
        return jailor?.Data?.IsDead != true && jailor?.Data?.Role is JailorRole;
    }

    private static void SetJailVoiceAllowed(byte playerId)
        => JailVoiceAllowedPlayers.Add(playerId);

    [MethodRpc(SetJaileeVoiceAllowedRpc)]
    private static void RpcSetJaileeVoiceAllowed(PlayerControl jailor, byte jaileeId, bool allowed)
    {
        // The source-owned control is deliberately one-way. A forged or stale re-mute cannot
        // silence someone the Jailor already allowed to speak.
        if (!JailVoiceControlEnabled || !allowed ||
            jailor?.Data?.Role is not JailorRole || jailor.HasDied())
        {
            return;
        }

        var jailee = MiscUtils.PlayerById(jaileeId);
        var jail = jailee?.GetModifier<JailedModifier>();
        if (jailee == null || jailee.HasDied() || jail == null || jail.JailorId != jailor.PlayerId)
        {
            return;
        }

        SetJailVoiceAllowed(jaileeId);
    }

    internal static void MaintainJailVoiceState(
        PlayerControl local,
        bool deliberation,
        bool muteJailed,
        bool canUnmute)
    {
        UpdateJailVoiceOptions(muteJailed, canUnmute);
        if (!deliberation || !JailVoiceControlEnabled)
        {
            JailVoiceAllowedPlayers.Clear();
            ClearAllJailVoiceButtons();
            return;
        }

        if (Time.time >= _nextJailVoiceMaintenanceTime)
        {
            _nextJailVoiceMaintenanceTime = Time.time + JailVoiceMaintenanceSeconds;
            ScratchPlayerIds.Clear();
            foreach (byte jaileeId in JailVoiceAllowedPlayers)
            {
                var jailee = MiscUtils.PlayerById(jaileeId);
                var jail = jailee?.GetModifier<JailedModifier>();
                if (jailee == null || jailee.HasDied() || jail == null ||
                    !IsLivingJailor(jail.JailorId))
                {
                    ScratchPlayerIds.Add(jaileeId);
                }
            }
            foreach (byte jaileeId in ScratchPlayerIds)
            {
                JailVoiceAllowedPlayers.Remove(jaileeId);
            }
        }

        if (local.Data?.Role is not JailorRole jailorRole || !local.AmOwner || local.HasDied())
        {
            ClearAllJailVoiceButtons();
            return;
        }

        TryCreateJailVoiceButton(jailorRole);
        if (JailVoiceAllowedPlayers.Count == 0 || Time.time < _nextJailVoiceHeartbeatTime)
        {
            return;
        }

        _nextJailVoiceHeartbeatTime = Time.time + JailVoiceHeartbeatSeconds;
        foreach (byte jaileeId in JailVoiceAllowedPlayers)
        {
            var jailee = MiscUtils.PlayerById(jaileeId);
            var jail = jailee?.GetModifier<JailedModifier>();
            if (jail?.JailorId == local.PlayerId)
            {
                RpcSetJaileeVoiceAllowed(local, jaileeId, true);
            }
        }
    }

    internal static void TryCreateJailVoiceButton(JailorRole jailorRole)
    {
        var jailor = jailorRole.Player;
        var jailee = jailorRole.Jailed;
        var meeting = MeetingHud.Instance;
        if (!Registered || !JailVoiceControlEnabled || meeting == null ||
            IsJailVoiceAllowed(jailee.PlayerId))
        {
            ClearJailVoiceButton(jailor.PlayerId);
            return;
        }
        if (JailVoiceButtons.TryGetValue(jailor.PlayerId, out var existingButton) &&
            existingButton != null)
        {
            return;
        }

        var voteArea = meeting.playerStates.FirstOrDefault(area => area.PlayerId == jailee.PlayerId);
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
        renderer.sprite = TouAssets.JailUnmute.LoadAsset();
        renderer.color = Color.white;

        label.text = MiraLocaleManager.Get("TownOfUsMira.Role.JailorAllowVoice", "Allow Voice");

        var passive = buttonObject.GetComponent<PassiveButton>();
        ConfigureJailVoiceButtonHitbox(buttonObject, passive, renderer);
        passive.OnClick = new Button.ButtonClickedEvent();
        passive.OnClick.AddListener((Action)(() =>
        {
            if (!Registered || !JailVoiceControlEnabled || jailor.HasDied() || jailee.HasDied())
            {
                return;
            }

            var jail = jailee.GetModifier<JailedModifier>();
            if (jail == null || jail.JailorId != jailor.PlayerId)
            {
                return;
            }

            SetJailVoiceAllowed(jailee.PlayerId);
            RpcSetJaileeVoiceAllowed(jailor, jailee.PlayerId, true);
            ClearJailVoiceButton(jailor.PlayerId);
        }));

        JailVoiceButtons[jailor.PlayerId] = buttonObject;
    }

    private static void ConfigureJailVoiceButtonHitbox(
        GameObject buttonObject,
        PassiveButton passive,
        SpriteRenderer renderer)
    {
        const string hitboxName = "JailVoiceButtonHitbox";
        var hitboxTransform = buttonObject.transform.Find(hitboxName);
        GameObject hitboxObject;
        BoxCollider2D hitbox;

        if (hitboxTransform == null)
        {
            hitboxObject = new GameObject(hitboxName);
            hitboxTransform = hitboxObject.transform;
            hitboxTransform.SetParent(buttonObject.transform, false);
            hitbox = hitboxObject.AddComponent<BoxCollider2D>();
        }
        else
        {
            hitboxObject = hitboxTransform.gameObject;
            hitbox = hitboxObject.GetComponent<BoxCollider2D>() ??
                     hitboxObject.AddComponent<BoxCollider2D>();
        }

        hitboxObject.layer = buttonObject.layer;
        hitboxObject.SetActive(true);
        hitboxTransform.localPosition = Vector3.zero;
        hitboxTransform.localRotation = Quaternion.identity;
        hitboxTransform.localScale = Vector3.one;

        var size = new Vector2(
            JailVoiceHitboxMinimumWidth,
            JailVoiceHitboxMinimumHeight);
        if (renderer.sprite != null)
        {
            var spriteSize = renderer.sprite.bounds.size;
            size.x = Mathf.Max(
                JailVoiceHitboxMinimumWidth,
                spriteSize.x + JailVoiceHitboxHorizontalPadding * 2f);
            size.y = Mathf.Max(
                JailVoiceHitboxMinimumHeight,
                spriteSize.y + JailVoiceHitboxVerticalPadding * 2f);
        }

        hitbox.offset = Vector2.zero;
        hitbox.size = size;
        hitbox.isTrigger = true;

        var colliders = buttonObject.GetComponentsInChildren<Collider2D>(true);
        for (var index = 0; index < colliders.Length; index++)
        {
            if (colliders[index] != hitbox)
            {
                colliders[index].enabled = false;
            }
        }

        hitbox.enabled = true;
        passive.ClickMask = hitbox;
        var authoritativeColliders = new Il2CppReferenceArray<Collider2D>(1);
        authoritativeColliders[0] = hitbox;
        passive.Colliders = authoritativeColliders;
        passive.CachedZ = buttonObject.transform.position.z;
    }

    internal static void ClearJailVoiceButton(byte jailorId)
    {
        if (!JailVoiceButtons.Remove(jailorId, out var button))
        {
            return;
        }

        button?.Destroy();
    }

    private static void ClearAllJailVoiceButtons()
    {
        if (JailVoiceButtons.Count == 0)
        {
            return;
        }

        foreach (var button in JailVoiceButtons.Values)
        {
            button?.Destroy();
        }
        JailVoiceButtons.Clear();
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
    private const string LegacyRoleOptionsSection = "Host.VoiceChat.Roles";
    private const string LegacyTeamRadioSection = "Host.VoiceChat";
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
    private static readonly object ListenerSightObscured =
        new VoiceListenerFilterResult(false) { SightObscured = true };
    private static readonly object ListenerMuffledAndSightObscured =
        new VoiceListenerFilterResult(true) { SightObscured = true };
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
            VoiceApiCapability.OverlayAppearance |
            VoiceApiCapability.ListenerTaskGateBypass |
            VoiceApiCapability.ListenerSightObscuration;
        if (!PerfectCommsApi.Supports(required))
        {
            Warning($"Perfect Comms {PerfectCommsApi.RuntimeApiVersion} does not expose every TOU-Mira integration capability; source-owned voice behavior is disabled.");
            return;
        }

        var id = TownOfUsPlugin.Id;
        try
        {
            RegisterOptions(id);
            PerfectCommsApi.RegisterVoiceRule(id, ResolveSpeakerRule);
            PerfectCommsApi.RegisterVoicePhaseObserver(id, ObservePhase);
            PerfectCommsApi.RegisterContextualListenerOrigin(id, ResolveListenerOrigin);
            PerfectCommsApi.RegisterContextualListenerFilter(id, ResolveListenerFilter);
            PerfectCommsApi.RegisterVoicePlayerTraits(id, ResolvePlayerTraits);
            PerfectCommsApi.RegisterVoicePairRule(id, ResolveMediumPair);
            PerfectCommsApi.RegisterManagedRadioChannel(id, ResolveVampireRadio);
            PerfectCommsApi.RegisterManagedRadioChannel(id, ResolveLoversRadio);
            PerfectCommsApi.RegisterOverlayViewerRule(id, ResolveOverlayViewer);
            PerfectCommsApi.RegisterOverlaySpeakerRule(id, ResolveOverlaySpeaker);
            PerfectCommsApi.RegisterAnimatedColorRule(id, RainbowUtils.IsRainbow);
            _registered = true;
            PerfectCommsIntegration.MarkRegistered();
            Info($"Registered source-owned TOU-Mira voice integration with Perfect Comms API {PerfectCommsApi.RuntimeApiVersion}.");
        }
        catch (Exception ex)
        {
            PerfectCommsApi.Unregister(id);
            PerfectCommsIntegration.Reset();
            Error($"Perfect Comms integration failed and was disabled: {ex}");
        }
    }

    private static void RegisterOptions(string id)
    {
        PerfectCommsApi.RegisterModTab(id, "TOU Mira");

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
            "Muffles incoming voice during tasks for players currently blinded by Eclipsal or flashed by Grenadier.");
        RegisterToggle(MuffleHypnotizedDuringHysteria,
            "<color=#FF0000><b>Hypnotist</b></color>: Muffle Hypnotized During Hysteria", true,
            "Muffles incoming voice during tasks for affected hypnotized players while Mass Hysteria is active.");
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
            "Enables the private Vampire managed Team Radio channel when Team Radio is on.",
            context => context.TeamRadioEnabled);
        RegisterToggle(TeamRadioLovers,
            "Team Radio - <color=#FF66CC><b>Lovers</b></color>", true,
            "Enables the private Lovers managed Team Radio channel when Team Radio is on.",
            context => context.TeamRadioEnabled);

        PerfectCommsApi.RegisterHostEnumOption(
            TownOfUsPlugin.Id,
            new VoiceHostEnumOption(
                MediumGhostVoice,
                "<color=#A680FF><b>Medium</b></color>: Ghost Voice",
                0,
                ["None", "Medium -> Ghost", "Ghost -> Medium", "Both"])
            {
                Description = "Chooses which voice direction is allowed between a Medium and dead players during tasks.",
                LegacyBinding = new VoiceHostOptionLegacyBinding(
                    LegacyRoleOptionsSection,
                    MediumGhostVoice),
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
            TownOfUsPlugin.Id,
            new VoiceHostOption(key, label, defaultValue)
            {
                Description = description,
                Visible = visible,
                LegacyBinding = new VoiceHostOptionLegacyBinding(
                    key is TeamRadioVampires or TeamRadioLovers
                        ? LegacyTeamRadioSection
                        : LegacyRoleOptionsSection,
                    key),
            });
    }

    private static VoiceRuleResult ResolveSpeakerRule(VoiceRuleContext context)
    {
        bool deliberation = context.Phase is VoicePhaseKind.Meeting or VoicePhaseKind.Exile;
        bool liveGame = context.Phase is VoicePhaseKind.Tasks or VoicePhaseKind.Meeting or VoicePhaseKind.Exile;
        bool muteBlackmailedInMeetings = context.GetOption(MuteBlackmailedInMeetings);
        bool muteBlackmailedNextRound = context.GetOption(MuteBlackmailedNextRound);
        PerfectCommsIntegration.UpdateBlackmailOptions(
            muteBlackmailedInMeetings,
            muteBlackmailedNextRound);
        bool muteJailedInMeetings = context.GetOption(MuteJailedInMeetings);
        bool jailorCanUnmuteJailed = context.GetOption(JailorCanUnmuteJailed);
        PerfectCommsIntegration.UpdateJailVoiceOptions(
            muteJailedInMeetings,
            jailorCanUnmuteJailed);
        bool jailPersistsAfterJailorDeath = context.GetOption(JailPersistsAfterJailorDeath);
        if (context.IsLocal)
        {
            PerfectCommsIntegration.MaintainJailVoiceState(
                context.Player,
                deliberation,
                muteJailedInMeetings,
                jailorCanUnmuteJailed);
        }

        if (context.IsDead)
        {
            return VoiceRuleResult.Pass;
        }

        if (deliberation && muteBlackmailedInMeetings &&
            context.Player.HasModifier<BlackmailedModifier>())
        {
            PerfectCommsIntegration.TrackMeetingBlackmail(context.Player.PlayerId);
            return (VoiceRuleResult)BlackmailedMuted;
        }

        if (liveGame && context.GetOption(MuteGlitchHacked) && IsActivelyGlitchHacked(context.Player))
        {
            return (VoiceRuleResult)HackedMuted;
        }

        if (liveGame && context.GetOption(MuteSwooperWhileSwooped) && context.Player.HasModifier<SwoopModifier>())
        {
            return (VoiceRuleResult)SwoopedMuted;
        }

        if (deliberation && muteJailedInMeetings &&
            context.Player.TryGetModifier<JailedModifier>(out var jail))
        {
            bool jailorValid = jailPersistsAfterJailorDeath ||
                PerfectCommsIntegration.IsLivingJailor(jail.JailorId);
            bool temporarilyAllowed =
                jailorCanUnmuteJailed &&
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

        if (muteBlackmailedNextRound &&
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
        PerfectCommsIntegration.UpdateJailVoiceOptions(
            context.GetOption(MuteJailedInMeetings),
            context.GetOption(JailorCanUnmuteJailed));
        bool muteBlackmailedInMeetings = context.GetOption(MuteBlackmailedInMeetings);
        bool muteBlackmailedNextRound = context.GetOption(MuteBlackmailedNextRound);
        PerfectCommsIntegration.UpdateBlackmailOptions(
            muteBlackmailedInMeetings,
            muteBlackmailedNextRound);
        if (context.Phase is VoicePhaseKind.Lobby or VoicePhaseKind.Meeting)
        {
            PerfectCommsIntegration.BeginMeeting();
        }
        else if ((context.PreviousPhase is VoicePhaseKind.Meeting or VoicePhaseKind.Exile) &&
                 context.Phase == VoicePhaseKind.Tasks)
        {
            PerfectCommsIntegration.CommitMeetingBlackmail(
                muteBlackmailedInMeetings && muteBlackmailedNextRound);
        }
    }

    private static VoiceListenerResult? ResolveListenerOrigin(VoiceListenerContext context)
    {
        if (context.Phase != VoicePhaseKind.Tasks)
        {
            return null;
        }

        int mediumMode = context.GetEnumOption(MediumGhostVoice);
        if (mediumMode != 0 && TryGetActiveMedium(context.Listener, out var medium))
        {
            return new VoiceListenerResult(
                medium.Spirit!.transform.position,
                ResolveLightRadius(context.Listener),
                VoiceListenerMode.Replace)
            {
                BypassTaskVoiceGates = true,
            };
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
        if (context.Phase != VoicePhaseKind.Tasks)
        {
            return (VoiceListenerFilterResult)ListenerNormal;
        }

        bool sightObscured =
            context.Listener.HasModifier<EclipsalBlindModifier>() ||
            context.Listener.HasModifier<GrenadierFlashModifier>();
        bool muffled =
            (sightObscured && context.GetOption(MuffleBlindedOrFlashedHearing)) ||
            (context.GetOption(MuffleHypnotizedDuringHysteria) &&
             context.Listener.GetModifier<HypnotisedModifier>()?.HysteriaActive == true);
        if (muffled)
        {
            return (VoiceListenerFilterResult)(sightObscured
                ? ListenerMuffledAndSightObscured
                : ListenerMuffled);
        }
        return (VoiceListenerFilterResult)(sightObscured
            ? ListenerSightObscured
            : ListenerNormal);
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
        if (context.Phase != VoicePhaseKind.Tasks)
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
        if (context.Phase != VoicePhaseKind.Tasks)
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
