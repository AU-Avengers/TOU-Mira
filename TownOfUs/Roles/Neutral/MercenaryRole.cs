using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class MercenaryRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public static int BrideCost => (int)OptionGroupSingleton<MercenaryOptions>.Instance.BribeCost;

    public int Gold { get; set; }
    public bool CanBribe => Gold >= BrideCost;
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<WardenRole>());
    public DoomableType DoomHintType => DoomableType.Insight;
    public string RoleName => TouLocale.Get(TouNames.Mercenary, "Mercenary");
    public string RoleDescription => "Bribe The Crewmates";
    public string RoleLongDescription => "Guard crewmates, and then bribe the winners!";
    public Color RoleColor => TownOfUsColors.Mercenary;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;

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

        stringB.Append(CultureInfo.InvariantCulture, $"\n<b>Gold:</b> {Gold}");

        var playerControls = players as PlayerControl[] ?? [.. players];
        if (playerControls.Length != 0)
        {
            stringB.Append("\n<b>Bribed:</b>");
        }

        foreach (var player in playerControls)
        {
            stringB.Append(CultureInfo.InvariantCulture, $"\n{player.Data.PlayerName}");
        }

        return stringB;
    }

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Neutral Benign role that can only win by bribing players, allowing them to gain multiple win conditions."
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Guard",
            "Guarding a player allows the Mercenary to absorb an ability used on the target. This will grant them gold, for Bribing. If any bribed targets win, the Mercenary will win with them.",
            TouNeutAssets.GuardSprite),
        new("Bribe",
            "Bribing a player allows the Mercenary to gain their win condition, given that they have gold to spare.",
            TouNeutAssets.BribeSprite)
    ];

    public override bool DidWin(GameOverReason gameOverReason)
    {
        var bribed = ModifierUtils.GetPlayersWithModifier<MercenaryBribedModifier>(x => x.Mercenary == Player);

        return bribed.Any(x => x.Data.Role.DidWin(gameOverReason) || x.GetModifiers<GameModifier>().Any(x => x.DidWin(gameOverReason) == true));
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

    [MethodRpc((uint)TownOfUsRpc.Guarded, SendImmediately = true)]
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