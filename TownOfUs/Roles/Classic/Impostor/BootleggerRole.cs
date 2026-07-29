using System.Text;
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
    public string LocaleKey => "Bootlegger";
    public string RoleName => "Bootlegger";
    public string RoleDescription => "Roleblock Crewmates to stop them";
    public string RoleLongDescription => "Roleblock the crew to disable their abilities";
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
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        var rbdur = OptionGroupSingleton<RoleblockOptions>.Instance.RoleblockDuration.Value;

        // Add a blank line before extra info for spacing
        sb.AppendLine();

        sb.AppendLine(TownOfUsPlugin.Culture, $"Roleblocked players are roleblocked for {rbdur} second(s).");

        if (OptionGroupSingleton<RoleblockOptions>.Instance.Hangover.Value)
            sb.AppendLine("Your target will have a hangover when their roleblock expires.");

        return sb;
    }
    public string GetAdvancedDescription()
    {
        var rbdur = OptionGroupSingleton<RoleblockOptions>.Instance.RoleblockDuration.Value;
        var desc = $"The Bootlegger is an Impostor Support role that can roleblock other players, roleblocking them for {rbdur} second(s).";

        if (OptionGroupSingleton<RoleblockOptions>.Instance.Hangover.Value)
            desc += "\n\nOnce the roleblock expires, the player will be hungover, preventing them from being roleblocked again too quickly.";

        return desc + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Drink",
            $"Drink with a player, roleblocking them for {OptionGroupSingleton<RoleblockOptions>.Instance.RoleblockDuration.Value} second(s)",
            TouImpAssets.SampleSprite)
    ];
}