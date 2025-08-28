using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class MorphlingRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    [HideFromIl2Cpp]
    public PlayerControl? Sampled { get; set; }
    public DoomableType DoomHintType => DoomableType.Perception;
    public string RoleName => TouLocale.Get(TouNames.Morphling, "Morphling");
    public string RoleDescription => "Transform Into Crewmates";

    public string RoleLongDescription =>
        "Sample players and morph into them to disguise yourself.\nYour sample clears at the beginning of every round.";

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorConcealing;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Morphling,
        CanUseVent = OptionGroupSingleton<MorphlingOptions>.Instance.CanVent,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Shapeshifter)
    };

    public void LobbyStart()
    {
        Clear();
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        if (Player.HasModifier<MorphlingMorphModifier>())
        {
            stringB.Append(CultureInfo.InvariantCulture,
                $"\n<b>Morphed As:</b> {Sampled!.Data.Color.ToTextColor()}{Sampled.Data.PlayerName}</color>");
        }

        return stringB;
    }

    public string GetAdvancedDescription()
    {
        return $"The {RoleName} is an Impostor Concealing role that can Sample a player and Morph into it's appearance."
               + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Sample",
            "Take a DNA sample of a player to morph into them later.",
            TouImpAssets.SampleSprite),
        new("Morph",
            "Morph into the appearance of the sampled player, which can be cancelled early.",
            TouImpAssets.MorphSprite)
    ];

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        Clear();
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        CustomButtonSingleton<MorphlingMorphButton>.Instance.SetActive(false, this);
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        Clear();
    }

    public void Clear()
    {
        Sampled = null;
    }
}