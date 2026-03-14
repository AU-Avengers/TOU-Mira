using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.KillFrenzy;

public sealed class FrenzyBomberRole(IntPtr cppPtr)
    : FrenzyRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    [HideFromIl2Cpp] public bool IsHiddenFromList => MiscUtils.CurrentGamemode() is not TouGamemode.KillFrenzy;

    public bool CanSpawnOnCurrentMode() => MiscUtils.CurrentGamemode() is TouGamemode.KillFrenzy;

    [HideFromIl2Cpp]
    Func<bool> ICustomRole.VisibleInSettings => () => MiscUtils.CurrentGamemode() is TouGamemode.KillFrenzy;
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

    public string LocaleKey => "Bomber";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription").Replace("<detonateDelay>",
                $"{OptionGroupSingleton<BomberOptions>.Instance.DetonateDelay}") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.FrenzyKiller;

    public CustomRoleConfiguration Configuration => new(this)
    {
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
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Place", "Place"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}PlaceWikiDescription").Replace("<maxKills>",
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