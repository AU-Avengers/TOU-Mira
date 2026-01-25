namespace TownOfUs.Modules.ControlSystem;

/// <summary>
/// Submerged-specific compatibility for controlling roles (Parasite/Puppeteer/etc).
/// When we override PlayerPhysics.FixedUpdate we can accidentally skip Submerged's floor/elevator bookkeeping.
/// This helper keeps the local client's floor selection in sync with the local player's current Y / elevator state.
/// </summary>
public static class SubmergedControlledMovementCompat
{
    private static bool _lastInElevator;
    private static bool? _lastUpperDeck;

    public static void Reset()
    {
        _lastInElevator = false;
        _lastUpperDeck = null;
    }

    /// <summary>
    /// Updates floor selection for the local player only.
    /// Safe to call every FixedUpdate while a controlling role is active.
    /// </summary>
    public static void UpdateLocalFloor(PlayerControl player)
    {
        if (player == null || PlayerControl.LocalPlayer == null)
        {
            return;
        }

        if (player != PlayerControl.LocalPlayer)
        {
            return;
        }

        if (!ModCompatibility.IsSubmerged())
        {
            Reset();
            return;
        }

        var isUpperDeck = player.transform.position.y > -7f;
        var inElevator = ModCompatibility.GetPlayerElevator(player).Item1;

        var floorChanged = _lastUpperDeck == null || _lastUpperDeck.Value != isUpperDeck;
        var elevatorChanged = inElevator != _lastInElevator;

        _lastUpperDeck = isUpperDeck;
        _lastInElevator = inElevator;

        if (floorChanged || elevatorChanged)
        {
            ModCompatibility.ChangeFloor(isUpperDeck);
        }

        if (inElevator)
        {
            ModCompatibility.CheckOutOfBoundsElevator(player);
        }
    }
}







