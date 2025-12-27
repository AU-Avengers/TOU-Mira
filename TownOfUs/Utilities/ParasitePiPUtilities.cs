using MiraAPI.Hud;
using UnityEngine;

namespace TownOfUs.Utilities;

/// <summary>
/// Utility class for managing Parasite Picture-in-Picture camera positioning, sizing, and dragging.
/// </summary>
public static class ParasitePiPUtilities
{
    private static Vector2? _manualPosition;
    private static bool _hasBeenManuallyMoved;

    /// <summary>
    /// Resets the manual position tracking. Call this when the overlay is created or destroyed.
    /// </summary>
    public static void ResetManualPosition()
    {
        _manualPosition = null;
        _hasBeenManuallyMoved = false;
    }

    /// <summary>
    /// Checks if there are any buttons on the left side of the screen that are visible and active.
    /// </summary>
    public static bool HasLeftSideModifierButtons()
    {
        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data?.Role == null)
        {
            return false;
        }

        var role = PlayerControl.LocalPlayer.Data.Role;

        var buttons = CustomButtonManager.Buttons;
        foreach (var button in buttons)
        {
            if (button == null || !button.Enabled(role))
            {
                continue;
            }

            if (button.Button != null && button.Location == ButtonLocation.BottomLeft && button.Button.isActiveAndEnabled)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the dynamic position based on mobile detection and modifier buttons.
    /// </summary>
    public static ParasitePiPLocation GetDynamicLocation()
    {
        var hasLeftModifiers = HasLeftSideModifierButtons();
        var isMobile = TownOfUsPlugin.IsMobile;

        if (!hasLeftModifiers)
        {
            return ParasitePiPLocation.BottomLeft;
        }

        if (isMobile)
        {
            return ParasitePiPLocation.TopRight;
        }

        return ParasitePiPLocation.MiddleRight;
    }

    /// <summary>
    /// Gets the effective location (resolves Dynamic to actual location).
    /// </summary>
    public static ParasitePiPLocation GetEffectiveLocation()
    {
        var setting = LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.ParasitePiPLocation.Value;
        return setting == ParasitePiPLocation.Dynamic ? GetDynamicLocation() : setting;
    }

    /// <summary>
    /// Calculates the world position for the PiP overlay based on the location setting.
    /// </summary>
    public static Vector3 CalculateWorldPosition(Camera hudCam, float z, bool respectManualPosition = true)
    {
        var screenWidth = Screen.width;
        var screenHeight = Screen.height;

        var worldBottomLeft = hudCam.ScreenToWorldPoint(new Vector3(0f, 0f, hudCam.nearClipPlane));
        var worldTopRight = hudCam.ScreenToWorldPoint(new Vector3(screenWidth, screenHeight, hudCam.nearClipPlane));

        var worldWidth = Mathf.Abs(worldTopRight.x - worldBottomLeft.x);
        var worldHeight = Mathf.Abs(worldTopRight.y - worldBottomLeft.y);

        if (respectManualPosition && _hasBeenManuallyMoved && _manualPosition.HasValue)
        {
            var manualScreenPos = _manualPosition.Value;
            var worldPos = hudCam.ScreenToWorldPoint(new Vector3(manualScreenPos.x, manualScreenPos.y, hudCam.nearClipPlane));
            return new Vector3(worldPos.x, worldPos.y, z);
        }

        var location = GetEffectiveLocation();
        var offsetX = 0f;
        var offsetY = 0f;

        switch (location)
        {
            case ParasitePiPLocation.TopLeft:
            case ParasitePiPLocation.MiddleLeft:
            case ParasitePiPLocation.BottomLeft:
                offsetX = worldBottomLeft.x + worldWidth * 0.15f;
                break;
            case ParasitePiPLocation.TopRight:
            case ParasitePiPLocation.MiddleRight:
            case ParasitePiPLocation.BottomRight:
                offsetX = worldTopRight.x - worldWidth * 0.15f;
                break;
        }

        switch (location)
        {
            case ParasitePiPLocation.TopLeft:
            case ParasitePiPLocation.TopRight:
                offsetY = worldTopRight.y - worldHeight * 0.15f;
                break;
            case ParasitePiPLocation.MiddleLeft:
            case ParasitePiPLocation.MiddleRight:
                offsetY = (worldBottomLeft.y + worldTopRight.y) * 0.5f;
                break;
            case ParasitePiPLocation.BottomLeft:
            case ParasitePiPLocation.BottomRight:
                offsetY = worldBottomLeft.y + worldHeight * 0.15f;
                break;
        }

        return new Vector3(offsetX, offsetY, z);
    }

    /// <summary>
    /// Calculates the scale multiplier based on the size setting.
    /// </summary>
    public static float GetScaleMultiplier()
    {
        var size = LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.ParasitePiPSize.Value;
        return size switch
        {
            ParasitePiPSize.Small => 0.85f,
            ParasitePiPSize.Large => 1.25f,
            _ => 1.0f
        };
    }

    /// <summary>
    /// Gets the snap zones for the PiP overlay.
    /// </summary>
    public static Vector2[] GetSnapZones(Camera hudCam)
    {
        var screenWidth = Screen.width;
        var screenHeight = Screen.height;

        var worldBottomLeft = hudCam.ScreenToWorldPoint(new Vector3(0f, 0f, hudCam.nearClipPlane));
        var worldTopRight = hudCam.ScreenToWorldPoint(new Vector3(screenWidth, screenHeight, hudCam.nearClipPlane));

        var worldWidth = Mathf.Abs(worldTopRight.x - worldBottomLeft.x);
        var worldHeight = Mathf.Abs(worldTopRight.y - worldBottomLeft.y);

        var snapZones = new Vector2[6];
        var margin = 0.15f;

        // Top Left
        snapZones[0] = new Vector2(
            worldBottomLeft.x + worldWidth * margin,
            worldTopRight.y - worldHeight * margin
        );

        // Middle Left
        snapZones[1] = new Vector2(
            worldBottomLeft.x + worldWidth * margin,
            (worldBottomLeft.y + worldTopRight.y) * 0.5f
        );

        // Bottom Left
        snapZones[2] = new Vector2(
            worldBottomLeft.x + worldWidth * margin,
            worldBottomLeft.y + worldHeight * margin
        );

        // Top Right
        snapZones[3] = new Vector2(
            worldTopRight.x - worldWidth * margin,
            worldTopRight.y - worldHeight * margin
        );

        // Middle Right
        snapZones[4] = new Vector2(
            worldTopRight.x - worldWidth * margin,
            (worldBottomLeft.y + worldTopRight.y) * 0.5f
        );

        // Bottom Right
        snapZones[5] = new Vector2(
            worldTopRight.x - worldWidth * margin,
            worldBottomLeft.y + worldHeight * margin
        );

        return snapZones;
    }

    /// <summary>
    /// Finds the closest snap zone to the given position.
    /// </summary>
    public static Vector2 SnapToClosestZone(Vector2 position, Camera hudCam)
    {
        var snapZones = GetSnapZones(hudCam);
        var closestZone = snapZones[0];
        var minDistance = Vector2.Distance(position, closestZone);

        for (var i = 1; i < snapZones.Length; i++)
        {
            var distance = Vector2.Distance(position, snapZones[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestZone = snapZones[i];
            }
        }

        return closestZone;
    }

    private static bool _isCurrentlyDragging;

    /// <summary>
    /// Handles dragging the PiP overlay. Returns true if dragging occurred.
    /// </summary>
    public static bool HandleDragging(GameObject overlayRoot, Camera hudCam, float z)
    {
        if (overlayRoot == null || hudCam == null)
        {
            return false;
        }

        var overlayRenderer = overlayRoot.GetComponentInChildren<SpriteRenderer>();
        if (overlayRenderer == null || overlayRenderer.sprite == null)
        {
            return false;
        }

        // Use collider for better hit detection if available
        var collider = overlayRoot.GetComponent<Collider2D>();
        Bounds bounds;
        if (collider != null)
        {
            bounds = collider.bounds;
        }
        else
        {
            // Fallback to sprite bounds
            var overlayPos = overlayRoot.transform.position;
            var localScale = overlayRoot.transform.localScale;
            var spriteSize = overlayRenderer.sprite.bounds.size;
            var worldSize = new Vector2(spriteSize.x * localScale.x, spriteSize.y * localScale.y);
            bounds = new Bounds(overlayPos, worldSize);
        }

        var minX = bounds.min.x;
        var maxX = bounds.max.x;
        var minY = bounds.min.y;
        var maxY = bounds.max.y;

        Vector2? currentInputPos = null;
        bool inputDown = false;
        bool inputUp = false;

        // Check for mouse input
        if (Input.GetMouseButtonDown(0))
        {
            currentInputPos = Input.mousePosition;
            inputDown = true;
        }
        else if (Input.GetMouseButton(0))
        {
            currentInputPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            inputUp = true;
            if (_isCurrentlyDragging)
            {
                currentInputPos = Input.mousePosition;
            }
        }
        // Check for touch input
        else if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            currentInputPos = touch.position;
            
            if (touch.phase == TouchPhase.Began)
            {
                inputDown = true;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                inputUp = true;
            }
        }

        if (currentInputPos.HasValue)
        {
            var worldPos = hudCam.ScreenToWorldPoint(new Vector3(currentInputPos.Value.x, currentInputPos.Value.y, hudCam.nearClipPlane));

            var isOverOverlay = worldPos.x >= minX && worldPos.x <= maxX && worldPos.y >= minY && worldPos.y <= maxY;

            if (inputDown && isOverOverlay)
            {
                _isCurrentlyDragging = true;
                _manualPosition = currentInputPos;
                _hasBeenManuallyMoved = true;
                overlayRoot.transform.position = new Vector3(worldPos.x, worldPos.y, z);
                return true;
            }

            if (_isCurrentlyDragging && (Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)))
            {
                overlayRoot.transform.position = new Vector3(worldPos.x, worldPos.y, z);
                _manualPosition = currentInputPos;
                return true;
            }
        }

        if (inputUp && _isCurrentlyDragging)
        {
            _isCurrentlyDragging = false;
            return false;
        }

        return _isCurrentlyDragging;
    }

    /// <summary>
    /// Handles snapping when drag ends. Call this when input is released.
    /// </summary>
    public static void HandleSnapping(GameObject overlayRoot, Camera hudCam, float z)
    {
        if (overlayRoot == null || hudCam == null || !_hasBeenManuallyMoved || !_manualPosition.HasValue)
        {
            return;
        }

        var worldPos = hudCam.ScreenToWorldPoint(new Vector3(_manualPosition.Value.x, _manualPosition.Value.y, hudCam.nearClipPlane));
        var snappedPos = SnapToClosestZone(new Vector2(worldPos.x, worldPos.y), hudCam);
        overlayRoot.transform.position = new Vector3(snappedPos.x, snappedPos.y, z);
        var snappedScreenPos = hudCam.WorldToScreenPoint(new Vector3(snappedPos.x, snappedPos.y, hudCam.nearClipPlane));
        _manualPosition = new Vector2(snappedScreenPos.x, snappedScreenPos.y);
    }
}