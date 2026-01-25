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
}
