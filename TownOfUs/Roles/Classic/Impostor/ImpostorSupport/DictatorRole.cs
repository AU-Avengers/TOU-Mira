using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class DictatorRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    private MeetingMenu meetingMenu = null!;

    public bool HasCoerced { get; set; }
    public bool CoerceActive { get; set; }
    public byte CoerceTargetId { get; set; } = byte.MaxValue;

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<LookoutRole>());
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string LocaleKey => "Dictator";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public static int MinInfluencedBeforeCoercing =>
        (int)OptionGroupSingleton<DictatorOptions>.Instance.MinNumberOfPlayersToInfluenceBeforeCoercing;

    public static int MaxInfluences =>
        (int)OptionGroupSingleton<DictatorOptions>.Instance.MaxNumberOfPlayersToInfluence;

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
        Icon = TouRoleIcons.Dictator
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Influence", "Influence"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}InfluenceWikiDescription"),
                    TouImpAssets.DictatorInfluenceButtonSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Coerce", "Coerce (Meeting)"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}CoerceWikiDescription"),
                    TouImpAssets.DictatorCoerceButtonSprite)
            ];
        }
    }

    public int GetActiveInfluenceCount()
    {
        if (!Player)
        {
            return 0;
        }

        return ModifierUtils.GetActiveModifiers<DictatorInfluencedModifier>(x =>
            x.DictatorId == Player.PlayerId && !x.Player.HasDied() && !x.Player.Data.Disconnected && x.Player.IsCrewmate()).Count();
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        HasCoerced = false;
        CoerceActive = false;
        CoerceTargetId = byte.MaxValue;

        if (Player.AmOwner)
        {
            meetingMenu = new MeetingMenu(
                this,
                Click,
                TouLocale.GetParsed("TouRoleDictatorCoerce", "Coerce (Meeting)"),
                MeetingAbilityType.Click,
                TouImpAssets.DictatorCoerceButtonSprite,
                null!,
                IsExempt)
            {
                Position = new Vector3(-0.40f, 0f, -3f)
            };
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        if (Player.AmOwner)
        {
            var canCoerce = Player.AmOwner &&
                            !Player.HasDied() &&
                            !Player.HasModifier<JailedModifier>() &&
                            !HasCoerced &&
                            GetActiveInfluenceCount() >= MinInfluencedBeforeCoercing;

            meetingMenu.GenButtons(MeetingHud.Instance, canCoerce);
        }
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        if (Player.AmOwner)
        {
            meetingMenu.HideButtons();
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);

        HasCoerced = false;
        CoerceActive = false;
        CoerceTargetId = byte.MaxValue;

        if (Player.AmOwner)
        {
            meetingMenu?.Dispose();
            meetingMenu = null!;
        }
    }

    public void Click(PlayerVoteArea voteArea, MeetingHud __)
    {
        var target = voteArea?.GetPlayer();
        if (target == null || target.PlayerId != Player.PlayerId || target.HasDied() || target.Data.Disconnected)
        {
            return;
        }

        RpcCoerce(Player);

        if (Player.AmOwner)
        {
            meetingMenu.HideButtons();
        }
    }

    public bool IsExempt(PlayerVoteArea voteArea)
    {
        var player = voteArea?.GetPlayer();
        return player == null || player.PlayerId != Player.PlayerId || player.HasDied() || player.Data.Disconnected;
    }

    [MethodRpc((uint)TownOfUsRpc.Coerce)]
    public static void RpcCoerce(PlayerControl dictator)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(dictator);
            return;
        }

        if (dictator.Data.Role is not DictatorRole role)
        {
            Error("RpcCoerce - Invalid dictator");
            return;
        }

        if (role.HasCoerced || role.GetActiveInfluenceCount() < MinInfluencedBeforeCoercing)
        {
            return;
        }

        role.HasCoerced = true;
        role.CoerceActive = true;
        role.CoerceTargetId = byte.MaxValue;

        var touAbilityEvent = new TouAbilityEvent(AbilityType.DictatorCoerce, dictator);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (!dictator.AmOwner)
        {
            return;
        }

        var notif = Helpers.CreateAndShowNotification(
            TouLocale.GetParsed("TouRoleDictatorCoerceActiveNotif",
                "Coerce activated. Influenced votes will follow your vote."),
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Dictator.LoadAsset());
        notif.AdjustNotification();
    }
}
