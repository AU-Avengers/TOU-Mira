using MiraAPI.GameOptions;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Impostor;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class SaboteurModifier : TouGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Impostor,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Saboteur.LoadAsset(),
            "TouMira.Modifier.Impostor.Saboteur", 1.45f));
    public override string LocaleKey => "Saboteur";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override string IntroInfo => "You also have reduced sabotage cooldowns.";
    public override Color FreeplayFileColor => new Color32(255, 25, 25, 255);

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Saboteur;
    public override ModifierFaction FactionType => ModifierFaction.ImpostorPassive;

    public float Timer { get; set; }

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.SaboteurChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.SaboteurAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsImpostor();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        var system = ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();

        var options = OptionGroupSingleton<SaboteurOptions>.Instance;

        if (system.AnyActive)
        {
            system.Timer = 30f;
        }
        else if (system.Timer > 30f - options.ReducedSaboCooldown)
        {
            system.Timer = 30f - options.ReducedSaboCooldown;
        }
    }
}