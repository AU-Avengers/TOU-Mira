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
    /// Gets the keybind for moving up as Parasite.
    /// </summary>
    public static MiraKeybind ParasitePrimaryUp { get; } = new("Parasite Move Up", KeyboardKeyCode.W, exclusive: false);

    /// <summary>
    /// Gets the keybind for moving left as Parasite.
    /// </summary>
    public static MiraKeybind ParasitePrimaryLeft { get; } = new("Parasite Move Left", KeyboardKeyCode.A, exclusive: false);

    /// <summary>
    /// Gets the keybind for moving down as Parasite.
    /// </summary>
    public static MiraKeybind ParasitePrimaryDown { get; } = new("Parasite Move Down", KeyboardKeyCode.S, exclusive: false);

    /// <summary>
    /// Gets the keybind for moving right as Parasite.
    /// </summary>
    public static MiraKeybind ParasitePrimaryRight { get; } = new("Parasite Move Right", KeyboardKeyCode.D, exclusive: false);

    /// <summary>
    /// Gets the keybind for moving up as the Parasite's Victim.
    /// </summary>
    public static MiraKeybind ParasiteSecondaryUp { get; } = new("Parasite Target Move Up", KeyboardKeyCode.UpArrow, exclusive: false);

    /// <summary>
    /// Gets the keybind for moving left as the Parasite's Victim.
    /// </summary>
    public static MiraKeybind ParasiteSecondaryLeft { get; } = new("Parasite Target Move Left", KeyboardKeyCode.LeftArrow, exclusive: false);

    /// <summary>
    /// Gets the keybind for moving down as the Parasite's Victim.
    /// </summary>
    public static MiraKeybind ParasiteSecondaryDown { get; } = new("Parasite Target Move Down", KeyboardKeyCode.DownArrow, exclusive: false);

    /// <summary>
    /// Gets the keybind for moving right as the Parasite's Victim.
    /// </summary>
    public static MiraKeybind ParasiteSecondaryRight { get; } = new("Parasite Target Move Right", KeyboardKeyCode.RightArrow, exclusive: false);
}
