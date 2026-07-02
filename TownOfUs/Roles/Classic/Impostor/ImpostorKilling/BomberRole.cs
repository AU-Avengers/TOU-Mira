using AmongUs.GameOptions;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class BomberRole
    : ImpostorRole, ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{ public Bomb? Bomb { get; set; }
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
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Bomber.LoadAsset(), "TouMira.Role.Impostor.Bomber", 1.45f),
        Icon = TouRoleIcons.Bomber,
        IntroSound = TouAudio.ExplodeIntro,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        CanUseVent = OptionGroupSingleton<BomberOptions>.Instance.CanVent
    };



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
            AmongUsClient.Instance.StartCoroutine(Bomb.BombShowTeammate(player, position));
        }
        else if ((PlayerControl.LocalPlayer.DiedOtherRound() &&
                  OptionGroupSingleton<PostmortemOptions>.Instance.TheDeadKnow))
        {
            AmongUsClient.Instance.StartCoroutine(Bomb.BombShowTeammate(player, position));
        }
    }
}