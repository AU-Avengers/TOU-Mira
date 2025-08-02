using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class ImmovableModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override string ModifierName => TouLocale.Get(TouNames.Immovable, "Immovable");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Immovable;

    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);

    public Vector3 Location { get; set; } = Vector3.zero;

    public string GetAdvancedDescription()
    {
        return
            "You cannot be teleported to the meeting area, and you cannot get dispersed or teleported.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return "You are unable to be moved via abilities and meetings.";
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.ImmovableChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.ImmovableAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && !(GameOptionsManager.Instance.currentNormalGameOptions.MapId is 4 or 6);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (Player.HasDied() || !Player.CanMove)
        {
            return;
        }

        Location = Player.transform.localPosition;
    }

    public void OnRoundStart()
    {
        if (Player.HasDied())
        {
            return;
        }

        Player.transform.localPosition = Location;
        Player.NetTransform.SnapTo(Location);

        if (ModCompatibility.IsSubmerged())
        {
            ModCompatibility.ChangeFloor(Player.GetTruePosition().y > -7);
            ModCompatibility.CheckOutOfBoundsElevator(PlayerControl.LocalPlayer);
        }
    }
}