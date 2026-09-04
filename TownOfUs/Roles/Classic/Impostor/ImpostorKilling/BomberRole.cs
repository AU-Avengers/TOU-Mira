using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class BomberRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    [HideFromIl2Cpp] public Bomb? Bomb { get; set; }
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<TrapperRole>());
    public DoomableType DoomHintType => DoomableType.Relentless;

    [HideFromIl2Cpp]
    public bool IsModifierApplicable(BaseModifier modifier)
    {
        return modifier is not OverclockerModifier;
    }

    public string IdPart => "Bomber";

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription").Replace("<detonateDelay>",
                $"{OptionGroupSingleton<BomberOptions>.Instance.DetonateDelay}") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Bomber.LoadAsset(), "TouMira.Role.Impostor.Bomber", 1.45f),
        Icon = TouRoleIcons.Bomber,
        IntroSound = TouAudio.ExplodeIntro,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        CanUseVent = OptionGroupSingleton<BomberOptions>.Instance.CanVent
    };



    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Place", "Place"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Place.WikiDescription").Replace("<maxKills>",
                        $"{(int)OptionGroupSingleton<BomberOptions>.Instance.MaxKillsInDetonation}"),
                    TouImpAssets.PlaceSprite)
            ];
        }
    }

    [MethodRpc((uint)TownOfUsRpc.PlantBomb)]
    public static void RpcPlantBomb(PlayerControl player, Vector2 position)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        if (player.Data.Role is not BomberRole role)
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
        else if (OptionGroupSingleton<BomberOptions>.Instance.AllImpsSeeBomb && PlayerControl.LocalPlayer.IsImpostorAligned())
        {
            Coroutines.Start(Bomb.BombShowTeammate(player, position));
        }
        else if ((PlayerControl.LocalPlayer.DiedOtherRound() &&
                  OptionGroupSingleton<PostmortemOptions>.Instance.TheDeadKnow))
        {
            Coroutines.Start(Bomb.BombShowTeammate(player, position));
        }
    }
}