using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using TownOfUs.Modifiers;
using UnityEngine;

namespace TownOfUs.Options;

/// <summary>
/// Base class for option groups. An option group is a collection of options that are displayed together in the options menu.
/// </summary>
/// <typeparam name="T">The custom role that the group is for.</typeparam>
[MiraIgnore]
public abstract class AbstractTouModifierOptionGroup<T>() : AbstractOptionGroup<T> where T : TouBaseGameModifier
{
    /// <inheritdoc />
    public override Type OptionableType => typeof(T);

    /// <inheritdoc />
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;

    /// <inheritdoc />
    public override OptionNotifConfiguration Configuration
    {
        get
        {
            var modifier = MiscUtils.AllBaseGameModifiers.FirstOrDefault(x => x.GetType() == OptionableType);
            if (modifier == null)
            {
                return new(new Color(0.7333f, 0.7333f, 0.7333f, 1));
            }
            return new(modifier.Configuration.UiColor, modifier.Configuration.PopUpIconTmp);
        }
    }
}