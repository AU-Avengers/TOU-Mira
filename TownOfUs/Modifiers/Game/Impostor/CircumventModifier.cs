using MiraAPI.GameOptions;
using MiraAPI.Roles;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class CircumventModifier : TouGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Impostor,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Circumvent.LoadAsset(),
            "TouMira.Modifier.Impostor.Circumvent", 1.45f));
    public int VentsAvailable { get; set; }
    public override string IdPart => "Circumvent";
    public bool NoVents => VentsAvailable <= 0;
    public override string ModifierName => MiraLocaleManager.Get($"TouModifier{IdPart}");
    public bool InVent { get; set; }

    public override string IntroInfo => NoVents
        ? MiraLocaleManager.Get($"TouModifier{IdPart}IntroBlurbNone")
        : MiraLocaleManager.Get($"TouModifier{IdPart}IntroBlurb");

    public override string GetDescription()
    {
        return NoVents
            ? MiraLocaleManager.Get($"TouModifier{IdPart}TabDescriptionNone")
            : MiraLocaleManager.Get($"TouModifier{IdPart}TabDescription")
                .Replace("<amount>", VentsAvailable.ToString(TownOfUsPlugin.Culture));
    }

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TouModifier{IdPart}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Circumvent;

    public override ModifierFaction FactionType => ModifierFaction.ImpostorPassive;
    public override Color FreeplayFileColor => new Color32(255, 25, 25, 255);


    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override void OnActivate()
    {
        if (Player.AmOwner)
        {
            var ventUses = OptionGroupSingleton<CircumventOptions>.Instance.GenerateUsesCount();
            VentsAvailable = ventUses;
        }
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.CircumventChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.CircumventAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsImpostor() &&
               (role is not ICustomRole custom || custom.Configuration.CanUseVent) &&
               role is not MinerRole;
    }

    public override bool? CanVent()
    {
        if (!InVent && VentsAvailable <= 0)
        {
            return false;
        }

        return null;
    }
}
