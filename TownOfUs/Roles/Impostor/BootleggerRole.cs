using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;
using System.Globalization;
using AmongUs.GameOptions;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Roles.Impostor;

public sealed class BootleggerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<BarkeeperRole>());
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string RoleName => "Bootlegger";
    public string RoleDescription => "Roleblock Crewmates to stop them";
    public string RoleLongDescription => "Roleblock the crew to disable their abilities";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Bootlegger,
        // IntroSound = TouAudio.ToppatIntroSound,
        MaxRoleCount = 15
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        var formatProvider = CultureInfo.InvariantCulture;
        var rbdur = OptionGroupSingleton<BootleggerOptions>.Instance.RoleblockDuration;

        // Add a blank line before extra info for spacing
        sb.AppendLine();

        sb.AppendLine(formatProvider, $"Roleblocked players are roleblocked for {rbdur} second(s).");

        if (OptionGroupSingleton<BootleggerOptions>.Instance.Hangover)
            sb.AppendLine("Your target will have a hangover when their roleblock expires.");

        return sb;
    }
    public string GetAdvancedDescription()
    {
        var rbdur = OptionGroupSingleton<BootleggerOptions>.Instance.RoleblockDuration;
        var desc = $"The Bootlegger is an Impostor Support role that can roleblock other players, roleblocking them for {rbdur} second(s).";

        if (OptionGroupSingleton<BootleggerOptions>.Instance.Hangover)
            desc += "\n\nOnce the roleblock expires, the player will be hungover, preventing them from being roleblocked again too quickly.";

        return desc + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Drink",
            $"Drink with a player, roleblocking them for {OptionGroupSingleton<BootleggerOptions>.Instance.RoleblockDuration} second(s)",
            TouImpAssets.SampleSprite)
    ];
}