using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Roles.Impostor;

public sealed class JanitorRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not JanitorRole || Player.HasDied() || !Player.AmOwner ||
            MeetingHud.Instance || (!HudManager.Instance.UseButton.isActiveAndEnabled &&
                                    !HudManager.Instance.PetButton.isActiveAndEnabled))
        {
            return;
        }

        HudManager.Instance.KillButton.ToggleVisible(OptionGroupSingleton<JanitorOptions>.Instance.JanitorKill ||
                                                     (Player != null && Player.GetModifiers<BaseModifier>()
                                                         .Any(x => x is ICachedRole)) ||
                                                     (Player != null && MiscUtils.ImpAliveCount == 1));
    }

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<ForensicRole>());
    public DoomableType DoomHintType => DoomableType.Death;
    public string LocaleKey => "Janitor";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = true,
        Icon = TouRoleIcons.Janitor,
        IntroSound = TouAudio.JanitorCleanSound
    };



    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Clean", "Clean"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}CleanWikiDescription"),
                    TouImpAssets.CleanButtonSprite)
            };
        }
    }

    [MethodRpc((uint)TownOfUsRpc.CleanBody, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcCleanBody(PlayerControl player, byte bodyId)
    {
        TimeLordRewindSystem.BodyLogger?.LogError($"[JanitorRPC] RpcCleanBody called: player={player.Data.PlayerName}, bodyId={bodyId}, isLocal={player.AmOwner}");

        if (player.Data.Role is not JanitorRole)
        {
            TimeLordRewindSystem.BodyLogger?.LogError($"[JanitorRPC] RpcCleanBody - Invalid Janitor role check failed");
            Error("RpcCleanBody - Invalid Janitor");
            return;
        }

        var body = TimeLordRewindSystem.FindDeadBodyIncludingInactive(bodyId);
        if (body == null)
        {
            body = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == bodyId);
        }

        TimeLordRewindSystem.BodyLogger?.LogError($"[JanitorRPC] Body found: body={body != null}, active={body?.gameObject?.activeSelf ?? false}, position={body?.transform?.position}");

        if (body != null)
        {
            var touAbilityEvent = new TouAbilityEvent(AbilityType.JanitorClean, player, body);
            MiraEventManager.InvokeEvent(touAbilityEvent);

            var isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost;
            var optionEnabled = OptionGroupSingleton<TimeLordOptions>.Instance.UncleanBodiesOnRewind;

            var shouldRecord = isHost ? optionEnabled : (optionEnabled || TimeLordRewindSystem.MatchHasTimeLord());

            TimeLordRewindSystem.BodyLogger?.LogError($"[JanitorRPC] Option check: isHost={isHost}, UncleanBodiesOnRewind={optionEnabled}, MatchHasTimeLord={TimeLordRewindSystem.MatchHasTimeLord()}, shouldRecord={shouldRecord}");

            if (shouldRecord)
            {
                TimeLordRewindSystem.BodyLogger?.LogError($"[JanitorRPC] Calling RecordBodyCleaned and CoHideBodyForTimeLord");
                TimeLordRewindSystem.RecordBodyCleaned(body, TimeLordRewindSystem.CleanedBodySource.Janitor);
                Coroutines.Start(TimeLordRewindSystem.CoHideBodyForTimeLord(body));
            }
            else
            {
                TimeLordRewindSystem.BodyLogger?.LogError($"[JanitorRPC] Option disabled and no Time Lord, calling CoClean (body will be destroyed)");
                Coroutines.Start(body.CoClean());
            }
            Coroutines.Start(CrimeSceneComponent.CoClean(body));
        }
        else
        {
            TimeLordRewindSystem.BodyLogger?.LogError($"[JanitorRPC] Body is null, cannot clean");
        }
    }
}