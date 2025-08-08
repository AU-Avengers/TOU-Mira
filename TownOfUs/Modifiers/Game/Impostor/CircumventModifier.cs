using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class CircumventModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => TouLocale.Get(TouNames.Circumvent, "Circumvent");
    public override string IntroInfo => $"You also lack the ability to travel through vents.";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Circumvent;

    public override ModifierFaction FactionType => ModifierFaction.ImpostorHarmful;
    public override Color FreeplayFileColor => new Color32(255, 25, 25, 255);

    public string GetAdvancedDescription()
    {
        return
            "You lose the ability to vent around the map."
            + MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return "You cannot vent.";
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
        var isValid = true;
        if ((role is MinerRole) ||
            (role is SwooperRole && !OptionGroupSingleton<SwooperOptions>.Instance.CanVent) ||
            (role is UndertakerRole && !OptionGroupSingleton<UndertakerOptions>.Instance.CanVent) ||
            (role is EscapistRole && !OptionGroupSingleton<EscapistOptions>.Instance.CanVent) ||
            (role is GrenadierRole && !OptionGroupSingleton<GrenadierOptions>.Instance.CanVent) ||
            (role is MorphlingRole && !OptionGroupSingleton<MorphlingOptions>.Instance.CanVent) ||
            (role is BomberRole && !OptionGroupSingleton<BomberOptions>.Instance.CanVent) ||
            (role is AmbusherRole && !OptionGroupSingleton<AmbusherOptions>.Instance.CanVent))
        {
            isValid = false;
        }

        return base.IsModifierValidOn(role) && role.IsImpostor() && isValid;
    }

    public override bool? CanVent()
    {
        return false;
    }
}