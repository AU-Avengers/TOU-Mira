using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using TownOfUs.Modifiers.Game;

namespace TownOfUs.Modifiers;

[MiraIgnore]
public abstract class TouBaseGameModifier : GameModifier
{
    public virtual string LocaleKey => "KEY_MISS";
    public virtual string IntroInfo => $"{TouLocale.Get("Modifier")}: {ModifierName}";
    public abstract float IntroSize { get; }
    public virtual ModifierFaction FactionType => ModifierFaction.Universal;

    public virtual int CustomAmount => GetAmountPerGame();
    public virtual int CustomChance => GetAssignmentChance();

    public override bool HideOnUi => false;

    public override int GetAmountPerGame()
    {
        return 1;
    }
}
