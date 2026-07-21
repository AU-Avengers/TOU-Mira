using Rewired;

namespace TownOfUs;

[RegisterCustomKeybinds]
public static class TouKeybinds
{
    /// <summary>
    /// Gets the keybind for opening the in-game wiki.
    /// </summary>
    public static MiraKeybind Wiki { get; } = new("Open In-Game Wiki", KeyboardKeyCode.F1);

    /// <summary>
    /// Gets the keybind for zooming in.
    /// </summary>
    public static MiraKeybind ZoomIn { get; } = new("Zoom In", KeyboardKeyCode.Equals);

    /// <summary>
    /// Gets the keybind for zooming in.
    /// </summary>
    public static MiraKeybind ZoomInKeypad { get; } = new("Zoom In (Alt)", KeyboardKeyCode.KeypadPlus);

    /// <summary>
    /// Gets the keybind for zooming out.
    /// </summary>
    public static MiraKeybind ZoomOut { get; } = new("Zoom Out", KeyboardKeyCode.Minus);

    /// <summary>
    /// Gets the keybind for zooming out.
    /// </summary>
    public static MiraKeybind ZoomOutKeypad { get; } = new("Zoom Out (Alt)", KeyboardKeyCode.KeypadMinus);

    /// <summary>
    /// Gets the keybind for moving up as the ControlRole's Victim.
    /// </summary>
    public static MiraKeybind ControlRoleTargetUp { get; } = new("Control Role Target Move Up", KeyboardKeyCode.None, exclusive: false);

    /// <summary>
    /// Gets the keybind for moving left as the ControlRole's Victim.
    /// </summary>
    public static MiraKeybind ControlRoleTargetLeft { get; } = new("Control Role Target Move Left", KeyboardKeyCode.None, exclusive: false);

    /// <summary>
    /// Gets the keybind for moving down as the ControlRole's Victim.
    /// </summary>
    public static MiraKeybind ControlRoleTargetDown { get; } = new("Control Role Target Move Down", KeyboardKeyCode.None, exclusive: false);

    /// <summary>
    /// Gets the keybind for moving right as the ControlRole's Victim.
    /// </summary>
    public static MiraKeybind ControlRoleTargetRight { get; } = new("Control Role Target Move Right", KeyboardKeyCode.None, exclusive: false);
}
