using MiraAPI.GameOptions;
using TownOfUs.Modules;
using TownOfUs.Options.Modifiers;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class ImmovableModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Immovable,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Immovable.LoadAsset(),
            "TouMira.Modifier.Universal.Immovable", 1.45f));
    public override string IdPart => "Immovable";
    public override string ModifierName => MiraLocaleManager.Get($"TouModifier{IdPart}");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Immovable;

    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);

    public Vector3 Location { get; set; } = Vector3.zero;

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

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
        return base.IsModifierValidOn(role) && !(GameOptionsManager.Instance.currentGameOptions.MapId is 4 or 6);
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