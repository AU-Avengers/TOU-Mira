using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using MiraAPI.Hud;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modifiers.Neutral;
using MiraAPI.Modifiers;

namespace TownOfUs.Roles.Neutral;

public sealed class PredatorRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<HunterRole>());
    [HideFromIl2Cpp] public List<PlayerControl> CaughtPlayers { get; } = [];
    public DoomableType DoomHintType => DoomableType.Hunter;
    public string RoleName => TouLocale.Get(TouNames.Predator, "Predator");
    public string RoleDescription => "Strike when they are most vulnerable";
    public string RoleLongDescription => "Kill players and target the ones who are most vulnerable!";
    public Color RoleColor => TownOfUsColors.Predator;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Phantom),
        // Using Hunter Role Icon texture as a placeholder
        Icon = TouRoleIcons.Hunter,
        MaxRoleCount = 1,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool HasImpostorVision => true;

    public bool WinConditionMet()
    {
        if (Player.HasDied())
        {
            return false;
        }

        var result = Helpers.GetAlivePlayers().Count <= 2 && MiscUtils.KillersAliveCount == 1;

        return result;
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Neutral Killing role that stares down its prey and strikes when they are most vulnerable."
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Stare",
            $"Choose a target to stare at and if they use any abilities while being stared at you gain the ability to instantly kill them.",
            // Using Hunter Stalk Button texture as a placeholder
            TouCrewAssets.StalkButtonSprite)
    ];

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouNeutAssets.PredatorVentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Predator);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
        }
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    [MethodRpc((uint)TownOfUsRpc.CatchPlayer, SendImmediately = true)]
    public static void RpcCatchPlayer(PlayerControl predator, PlayerControl source)
    {
        if (predator.Data.Role is not PredatorRole role)
        {
            Logger<TownOfUsPlugin>.Error("RpcCatchPlayer - Invalid predator");
            return;
        }

        if (!role.CaughtPlayers.Contains(source))
        {
            role.CaughtPlayers.Add(source);

            CustomButtonSingleton<PredatorStareButton>.Instance.ResetCooldownAndOrEffect();
            source.RemoveModifier<PredatorStaringModifier>();

            if (predator.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Predator));
            }
        }
    }
}