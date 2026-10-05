using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using UnityEngine;
using AmongUs.GameOptions;
using TownOfUs.Options;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Roles.Impostor;

public sealed class BootleggerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<BarkeeperRole>());
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string IdPart => "Bootlegger";

    public string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescription").Replace("<blockTime>",
        OptionGroupSingleton<RoleblockOptions>.Instance.RoleblockDuration.Value.ToString(TownOfUsPlugin.Culture));

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Bootlegger.LoadAsset(), "TouMira.Role.Impostor.Bootlegger", 1.45f),
        Icon = TouRoleIcons.Bootlegger,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        IntroSound = TouAudio.PotionIntro
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(MiraLocaleManager.Get("TownOfUsMira.Role.BarkeeperRoleblock"),
            (OptionGroupSingleton<RoleblockOptions>.Instance.Hangover.Value
                ? MiraLocaleManager.Get("TownOfUsMira.Role.BarkeeperRoleblockWikiDescriptionWithHangover").Replace("<overTime>",
                    OptionGroupSingleton<RoleblockOptions>.Instance.HangoverDuration.Value.ToString(TownOfUsPlugin
                        .Culture))
                : MiraLocaleManager.Get("TownOfUsMira.Role.BarkeeperRoleblock.WikiDescription")).Replace("<blockTime>",
                OptionGroupSingleton<RoleblockOptions>.Instance.RoleblockDuration.Value
                    .ToString(TownOfUsPlugin.Culture)),
            TouImpAssets.DrinkPoisonSprite)
    ];
}