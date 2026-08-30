using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.GameModes;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Roles.KillFrenzy;

public sealed class FrenzyBomberRole(IntPtr cppPtr)
    : FrenzyRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public bool WinConditionMet()
    {
        var wwCount = CustomRoleUtils.GetActiveRolesOfType<FrenzyBomberRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > wwCount)
        {
            return false;
        }

        return wwCount >= Helpers.GetAlivePlayers().Count - wwCount;
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
    [HideFromIl2Cpp] public Bomb? Bomb { get; set; }

    public string IdPart => "Bomber";
    public string RoleName => MiraLocaleManager.Get($"TouRole{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TouRole{IdPart}IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TouRole{IdPart}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TouRole{IdPart}WikiDescription").Replace("<detonateDelay>",
                $"{OptionGroupSingleton<BomberOptions>.Instance.DetonateDelay}") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.FrenzyKiller;

    public CustomRoleConfiguration Configuration => new(this)
    {
        AssociatedGameMode = typeof(KillFrenzyMode),
        GhostRole = (RoleTypes)RoleId.Get<FrenzyGhostRole>(),
        FreeplayFolder = "Kill Frenzy",
        Icon = TouRoleIcons.Bomber,
        CanUseVent = false
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(MiraLocaleManager.Get($"TouRole{IdPart}Place", "Place"),
                    MiraLocaleManager.Get($"TouRole{IdPart}PlaceWikiDescription").Replace("<maxKills>",
                        $"{(int)OptionGroupSingleton<BomberOptions>.Instance.MaxKillsInDetonation}"),
                    TouImpAssets.PlaceSprite)
            };
        }
    }

    [MethodRpc((uint)TownOfUsRpc.FrenzyPlantBomb)]
    public static void RpcPlantBomb(PlayerControl player, Vector2 position)
    {
        if (LobbyBehaviour.Instance)
        {
            return;
        }
        if (player.Data.Role is not FrenzyBomberRole role)
        {
            Error("RpcPlantBomb - Invalid bomber");
            return;
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.BomberPlant, player);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (player.AmOwner)
        {
            role.Bomb = Bomb.CreateBomb(player, position);
        }
        else if (PlayerControl.LocalPlayer.HasDied())
        {
            Coroutines.Start(Bomb.BombShowTeammate(player, position));
        }
    }
}