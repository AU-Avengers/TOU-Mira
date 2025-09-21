using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using TownOfUs.Modules.Anims;

namespace TownOfUs.Modifiers;

[MiraIgnore]
public abstract class BaseShieldModifier : TimedModifier, IAnimated
{
    public override string ModifierName => "Shield Modifier";
    public virtual string ShieldDescription => "You are protected!";
    public override float Duration => 1f;
    public override bool AutoStart => false;
    public override bool HideOnUi => !LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.ShowShieldHudToggle.Value;
    public virtual bool VisibleSymbol => false;
    public bool IsVisible { get; set; } = true;

    public void SetVisible()
    {
    }

    public override string GetDescription()
    {
        return !HideOnUi ? ShieldDescription : string.Empty;
    }
}