using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class MercenaryRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant, IGuessable
{
    public static int BrideCost => (int)OptionGroupSingleton<MercenaryOptions>.Instance.BribeCost;

    public int Gold { get; set; }
    public bool CanBribe => Gold >= BrideCost;
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<WardenRole>());
    public DoomableType DoomHintType => DoomableType.Insight;
    public string LocaleKey => "Mercenary";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Guard", "Guard"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}GuardWikiDescription"),
                    TouNeutAssets.GuardSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Bribe", "Bribe"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}BribeWikiDescription"),
                    TouNeutAssets.BribeSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Mercenary;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;

    // This is so the role can be guessed without requiring it to be enabled normally
    public bool CanBeGuessed =>
        (MiscUtils.GetPotentialRoles()
             .Contains(RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<GuardianAngelTouRole>())) &&
         OptionGroupSingleton<GuardianAngelOptions>.Instance.OnTargetDeath is BecomeOptions.Mercenary)
        || (MiscUtils.GetPotentialRoles()
                .Contains(RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<ExecutionerRole>())) &&
            OptionGroupSingleton<ExecutionerOptions>.Instance.OnTargetDeath is BecomeOptions.Mercenary);

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = TouAudio.ToppatIntroSound,
        Icon = TouRoleIcons.Mercenary,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        var players = ModifierUtils.GetPlayersWithModifier<MercenaryBribedModifier>();

        stringB.Append(CultureInfo.InvariantCulture, $"\n<b>{TouLocale.GetParsed("TouRoleMercenaryTabGoldCounter").Replace("<count>", $"{Gold}")}</b>");

        var playerControls = players as PlayerControl[] ?? [.. players];
        if (playerControls.Length != 0)
        {
            stringB.Append(CultureInfo.InvariantCulture, $"\n<b>{TouLocale.Get("TouRoleMercenaryTabBribedInfo")}</b>");

            foreach (var player in playerControls)
            {
                stringB.Append(CultureInfo.InvariantCulture, $"\n{player.Data.PlayerName}");
            }
        }

        return stringB;
    }
    
    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        if (!Player.HasModifier<BasicGhostModifier>() && ModifierUtils.GetActiveModifiers<MercenaryBribedModifier>(x => x.Mercenary == Player).Any())
        {
            Player.AddModifier<BasicGhostModifier>();
        }
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        var bribed = ModifierUtils.GetPlayersWithModifier<MercenaryBribedModifier>(x => x.Mercenary == Player);

        return bribed.Any(x =>
            x.Data.Role.DidWin(gameOverReason) ||
            x.GetModifiers<GameModifier>().Any(x => x.DidWin(gameOverReason) == true));
    }

    public void AddPayment()
    {
        Gold++;

        if (CanBribe)
        {
            CustomButtonSingleton<MercenaryBribeButton>.Instance.SetActive(true, this);
        }
    }

    public void Clear()
    {
        Gold = 0;

        CustomButtonSingleton<MercenaryGuardButton>.Instance.SetActive(true, this);
        CustomButtonSingleton<MercenaryBribeButton>.Instance.SetActive(false, this);
    }

    [MethodRpc((uint)TownOfUsRpc.Guarded)]
    public static void RpcGuarded(PlayerControl player)
    {
        if (player.Data.Role is not MercenaryRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcGuarded - Invalid mercenary");
            return;
        }

        if (!player.AmOwner)
        {
            return;
        }

        var mercenary = player.GetRole<MercenaryRole>();
        mercenary?.AddPayment();
    }
}