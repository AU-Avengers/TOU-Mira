using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class CircumventModifier : TouGameModifier, IWikiDiscoverable
{
    public int VentsAvailable { get; set; }
    public override string LocaleKey => "Circumvent";
    public bool NoVents => VentsAvailable <= 0;
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");

    public override string IntroInfo => NoVents
        ? TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurbNone")
        : TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurb");

    public override string GetDescription()
    {
        return NoVents
            ? TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescriptionNone")
            : TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription")
                .Replace("<amount>", VentsAvailable.ToString(TownOfUsPlugin.Culture));
    }

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription") +
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
        if (VentsAvailable <= 0)
        {
            return false;
        }

        return null;
    }
}
