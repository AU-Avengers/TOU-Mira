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
using TownOfUs.Options;
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

    [MethodRpc((uint)TownOfUsRpc.PlantBomb)]
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
        else if ((PlayerControl.LocalPlayer.DiedOtherRound() &&
                  OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow))
        {
            Coroutines.Start(Bomb.BombShowTeammate(player, position));
        }
    }
}