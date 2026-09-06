using MiraAPI.Events.Mira;
using MiraAPI.Hud;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events;

/// <summary>
/// Button click event for <see cref="CustomActionButton"/>s only. Do not use for vanilla <see cref="AbilityButton"/>s.
/// </summary>
public sealed class ExtendedMiraButtonClickEvent : MiraButtonClickEvent
{
    /// <summary>
    /// Gets the <see cref="CustomActionButton"/> that was clicked.
    /// </summary>
    public CustomActionButton Button { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the murder is done indirectly.
    /// This is commonly used to prevent the local <see cref="PlayerControl"/> from being killed.
    /// </summary>
    public bool IsIndirectInteraction { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the button should bypass protective roles or general defense, such as <see cref="WardenRole"/>.
    /// </summary>
    public bool IgnoreDefense { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MiraButtonClickEvent"/> class.
    /// </summary>
    /// <param name="button">The <see cref="CustomActionButton"/> that was clicked.</param>
    public ExtendedMiraButtonClickEvent(CustomActionButton button) : base(button)
    {
        Button = button;
    }
}