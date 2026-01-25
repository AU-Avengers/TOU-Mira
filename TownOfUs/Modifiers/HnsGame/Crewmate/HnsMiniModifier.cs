using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Game;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.HnsGame.Crewmate;

public sealed class HnsMiniModifier : HnsGameModifier, IVisualAppearance
{
    public override string LocaleKey => "Mini";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Mini;
    public override ModifierFaction FactionType => ModifierFaction.HiderVisibility;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        appearance.Size = new Vector3(0.49f, 0.49f, 1f);
        return appearance;
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.MiniChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.MiniAmount;
    }

    public override void OnActivate()
    {
        Player.RawSetAppearance(this);
    }

    public override void OnDeactivate()
    {
        Player?.ResetAppearance(fullReset: true);
    }
}