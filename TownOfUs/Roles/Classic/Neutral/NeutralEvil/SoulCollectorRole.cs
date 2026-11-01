using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class SoulCollectorRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner)
        {
            return;
        }
        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        orCreateTask.Text = $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralEvilTaskHeader")}</color>";
        orCreateTask.name = "NeutralRoleText";
    }

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<DeputyRole>());
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string IdPart => "SoulCollector";

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Reap", "Reap"),
                    MiraLocaleManager.Get("MiraApi.AbilityType.Interaction"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Reap.WikiDescription"),
                    TouNeutAssets.ReapSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Collect", "Collect"),
                    MiraLocaleManager.Get("MiraApi.AbilityType.Interaction"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Collect.WikiDescription"),
                    TouNeutAssets.CollectSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.SoulCollector;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.SoulCollector, "TouMira.Role.Neutral.SoulCollector", 1.45f),
        IntroSound = TouAudio.PhantomIntroSound,
        Icon = TouRoleIcons.SoulCollector,
        OptionsScreenshot = TouBanners.NeutralRoleBanner,
        MaxRoleCount = 1,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };
    public bool WinConditionMet()
    {
        if (Player.HasDied() || MiscUtils.KillersAliveCount == 0)
        {
            return false;
        }

        var result = MiscUtils.GetImpactfulLivingPlayers().Count <= 2 && MiscUtils.KillersAliveCount == 1;

        return result;
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

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }

    [MethodRpc((uint)TownOfUsRpc.ReapPlayers)]
    public static void RpcReapPlayers(PlayerControl player, Dictionary<byte, string> targets)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        var victims = PlayerControl.AllPlayerControls.Where(x => targets.ContainsKey(x.PlayerId));
        foreach (var victim in victims)
        {
            victim.AddModifier<ReapedModifier>();
        }
    }
}