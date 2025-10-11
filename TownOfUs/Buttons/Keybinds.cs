namespace TownOfUs.Buttons;

public static class Keybinds
{
    public static BaseKeybind PrimaryAction = VanillaKeybinding<KillButton>.Instance; // Keyboard: used for kill button

    public static BaseKeybind
        SecondaryAction = VanillaKeybinding<AbilityButton>.Instance; // Keyboard: used for vanilla role abilities

    public static BaseKeybind TertiaryAction = MiraGlobalKeybinds.TertiaryAbility; // Keyboard: used for glitch hack

    public static BaseKeybind
        ModifierAction = MiraGlobalKeybinds.ModifierPrimaryAbility; // Keyboard: used for modifier abilities

    public static BaseKeybind VentAction = VanillaKeybinding<VentButton>.Instance; // Keyboard: used for venting

    public const int PrimaryConsole = 8; // Console: used for kill button

    public const int SecondaryConsole = 49; // Console: used for vanilla role abilities

    public const int ModifierConsole = 20; // Console LB: used for modifier abilities

    public const int VentConsole = 50; // Console: used for venting
}