using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class MercenaryRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant, IGuessable
{
    public void InitialSetup()
    {
        TmpSpriteUtils.CreateSpriteAsset(TouNeutAssets.BribeSprite.LoadAsset(),
            "TouMira.Role.Neutral.Mercenary.Ui.Bribe", 1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouNeutAssets.GuardSprite.LoadAsset(),
            "TouMira.Role.Neutral.Mercenary.Ui.Guard", 1.45f);
    }
    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner)
        {
            return;
        }
        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        orCreateTask.Text = $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralBenignTaskHeader")}</color>";
        orCreateTask.name = "NeutralRoleText";
    }

    public static int BrideCost => (int)OptionGroupSingleton<MercenaryOptions>.Instance.BribeCost;

    public int Gold { get; set; }
    public bool CanBribe => Gold >= BrideCost;
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<WardenRole>());
    public DoomableType DoomHintType => DoomableType.Insight;
    public string IdPart => "Mercenary";
    public string RoleMedDescriptionLocale => $"TownOfUsMira.Role.{IdPart}.TabDescription";

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Guard", "Guard"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Guard.WikiDescription"),
                    TouNeutAssets.GuardSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Bribe", "Bribe"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Bribe.WikiDescription"),
                    TouNeutAssets.BribeSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Mercenary;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;

    // This is so the role can be guessed without requiring it to be enabled normally
    public bool CanBeGuessed =>
        (MiscUtils.GetPotentialRoles()
             .Contains(RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<FairyRole>())) &&
         OptionGroupSingleton<FairyOptions>.Instance.OnTargetDeath is BecomeOptions.Mercenary)
        || (MiscUtils.GetPotentialRoles()
                .Contains(RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<ExecutionerRole>())) &&
            OptionGroupSingleton<ExecutionerOptions>.Instance.OnTargetDeath is BecomeOptions.Mercenary);

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Mercenary.LoadAsset(), "TouMira.Role.Neutral.Mercenary", 1.45f),
        IntroSound = TouAudio.ToppatIntroSound,
        Icon = TouRoleIcons.Mercenary,
        OptionsScreenshot = TouBanners.NeutralRoleBanner,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        var players = ModifierUtils.GetPlayersWithModifier<MercenaryBribedModifier>();
        
        stringB.Append(TownOfUsPlugin.Culture, $"\n<b><sprite name=\"TouMira.Role.Neutral.Mercenary.Ui.Bribe\">{MiraLocaleManager.Get("TownOfUsMira.Role.MercenaryTabGoldCounter").Replace("<count>", $"{Gold}")}</b>");

        var playerControls = players as PlayerControl[] ?? [.. players];
        if (playerControls.Length != 0)
        {
            stringB.Append(TownOfUsPlugin.Culture, $"\n<b>{MiraLocaleManager.Get("TownOfUsMira.Role.MercenaryTabBribedInfo")}</b>");

            foreach (var player in playerControls)
            {
                stringB.Append(TownOfUsPlugin.Culture, $"\n{player.Data.PlayerName}");
            }
        }

        return stringB;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);

        if (!Player.HasModifier<BasicGhostModifier>() && ModifierUtils
                .GetActiveModifiers<MercenaryBribedModifier>([HideFromIl2Cpp](x) => x.Mercenary == Player).HasAny())
        {
            Player.AddModifier<BasicGhostModifier>();
        }

        var guardedMods = ModifierUtils.GetActiveModifiers<MercenaryGuardModifier>().ToList();
        if (!guardedMods.HasAny())
        {
            return;
        }

        foreach (var guarded in guardedMods)
        {
            if (guarded.Mercenary != Player)
            {
                continue;
            }

            guarded.Player.RemoveModifier(guarded);
        }
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        var bribed = ModifierUtils.GetPlayersWithModifier<MercenaryBribedModifier>(x => x.Mercenary == Player);

        return bribed.Any(x =>
            x.Data.Role.DidWin(gameOverReason) ||
            x.GetModifiers<GameModifier>().Any(x => x.DidWin(gameOverReason) == true));
    }

    public void AddPayment(int gold = 1)
    {
        Gold += gold;
    }

    public void Clear()
    {
        Gold = 0;
    }

    [MethodRpc((uint)TownOfUsRpc.Guarded)]
    public static void RpcGuarded(PlayerControl player, PlayerControl target, bool isMurder = false)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        if (player.Data.Role is not MercenaryRole mercenary)
        {
            Error("RpcGuarded - Invalid mercenary");
            return;
        }

        var mercOpts = OptionGroupSingleton<MercenaryOptions>.Instance;
        if (isMurder && mercOpts.GuardProtection.Value)
        {
            mercenary.AddPayment(mercOpts.GoldGivenFromAttack);
        }
        else
        {
            mercenary.AddPayment();
        }

        if (target.TryGetModifier<MercenaryGuardModifier>(out var mercGuard))
        {
            // This will remove it after a few seconds
            mercGuard.StartTimer();
        }
    }
}