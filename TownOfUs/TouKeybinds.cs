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
    /// Gets the keybind for moving up as ControlRole.
    /// </summary>
    public static MiraKeybind ControlRolePrimaryUp { get; } = new("Control Role Move Up", KeyboardKeyCode.W, exclusive: false);

    /// <summary>
    /// Gets the keybind for moving left as ControlRole.
    /// </summary>
    public static MiraKeybind ControlRolePrimaryLeft { get; } = new("Control Role Move Left", KeyboardKeyCode.A, exclusive: false);

    /// <summary>
    /// Gets the keybind for moving down as ControlRole.
    /// </summary>
    public static MiraKeybind ControlRolePrimaryDown { get; } = new("Control Role Move Down", KeyboardKeyCode.S, exclusive: false);

    /// <summary>
    /// Gets the keybind for moving right as ControlRole.
    /// </summary>
    public static MiraKeybind ControlRolePrimaryRight { get; } = new("Control Role Move Right", KeyboardKeyCode.D, exclusive: false);

    /// <summary>
    /// Gets the keybind for moving up as the ControlRole's Victim.
    /// </summary>
    public static MiraKeybind ControlRoleSecondaryUp { get; } = new("Control Role Target Move Up", KeyboardKeyCode.UpArrow, exclusive: false);

    /// <summary>
    /// Gets the keybind for moving left as the ControlRole's Victim.
    /// </summary>
    public static MiraKeybind ControlRoleSecondaryLeft { get; } = new("Control Role Target Move Left", KeyboardKeyCode.LeftArrow, exclusive: false);

    /// <summary>
    /// Gets the keybind for moving down as the ControlRole's Victim.
    /// </summary>
    public static MiraKeybind ControlRoleSecondaryDown { get; } = new("Control Role Target Move Down", KeyboardKeyCode.DownArrow, exclusive: false);

    /// <summary>
    /// Gets the keybind for moving right as the ControlRole's Victim.
    /// </summary>
    public static MiraKeybind ControlRoleSecondaryRight { get; } = new("Control Role Target Move Right", KeyboardKeyCode.RightArrow, exclusive: false);
}
