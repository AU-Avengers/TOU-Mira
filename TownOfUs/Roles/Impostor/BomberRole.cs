using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class BomberRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    [HideFromIl2Cpp] public Bomb? Bomb { get; set; }
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<TrapperRole>());
    public DoomableType DoomHintType => DoomableType.Relentless;

    public string RoleName => TouLocale.Get(TouNames.Bomber, "Bomber");
    public string RoleDescription => "Plant Bombs To Kill Multiple Crewmates At Once";
    public string RoleLongDescription => "Plant bombs to kill several crewmates at once";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Bomber,
        CanUseVent = OptionGroupSingleton<BomberOptions>.Instance.CanVent
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is an Impostor Killing role that can drop a bomb on the map, which detonates after {OptionGroupSingleton<BomberOptions>.Instance.DetonateDelay} second(s)" +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Place",
            $"Place a bomb, showing the radius in which it'll kill, killing up to {(int)OptionGroupSingleton<BomberOptions>.Instance.MaxKillsInDetonation} player(s)",
            TouImpAssets.PlaceSprite)
    ];

    [MethodRpc((uint)TownOfUsRpc.PlantBomb, SendImmediately = true)]
    public static void RpcPlantBomb(PlayerControl player, Vector2 position)
    {
        if (player.Data.Role is not BomberRole role)
        {
            Logger<TownOfUsPlugin>.Error("RpcPlantBomb - Invalid bomber");
            return;
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.BomberPlant, player);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (player.AmOwner)
        {
            role.Bomb = Bomb.CreateBomb(player, position);
        }
        else if (OptionGroupSingleton<BomberOptions>.Instance.AllImpsSeeBomb && PlayerControl.LocalPlayer.IsImpostor())
        {
            Coroutines.Start(Bomb.BombShowTeammate(player, position));
        }
    }
}