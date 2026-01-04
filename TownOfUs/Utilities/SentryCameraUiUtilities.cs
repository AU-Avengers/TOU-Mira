using BepInEx.Logging;
using Il2CppInterop.Runtime;
using MiraAPI.Utilities.Assets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Utilities;

public static class SentryCameraUiUtilities
{
    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("SentryCameraUiUtilities");

    private static Sprite? _polusPageArrowSprite;
    private static AudioClip? _polusPageFlipSound;
    private static bool _polusLeftArrowFlipX;
    private static bool _polusRightArrowFlipX;
    private static float _polusLeftArrowScaleX = 1f;
    private static float _polusRightArrowScaleX = 1f;
    private static Sprite? _polusDotSelectedSprite;
    private static Sprite? _polusDotUnselectedSprite;

    public static int CurrentPage { get; set; }
    public static bool ForceRefresh { get; set; }
    public static float UiRepairTimer { get; set; }
    public static int SuppressCloseFrame { get; set; } = -9999;

    public static void ResetPageState()
    {
        CurrentPage = 0;
        UiRepairTimer = 0f;
        ForceRefresh = true;
        SuppressCloseFrame = -9999;
    }

    public static bool IsMouseOverSprite(Transform? tf, Vector3 mouseWorld)
    {
        if (tf == null) return false;
        if (!tf.gameObject.activeInHierarchy) return false;

        var sr = tf.GetComponent<SpriteRenderer>();
        if (sr == null || !sr.enabled || sr.sprite == null) return false;

        var b = sr.bounds;
        return mouseWorld.x >= b.min.x && mouseWorld.x <= b.max.x &&
               mouseWorld.y >= b.min.y && mouseWorld.y <= b.max.y;
    }

    public static int GetSkeldPages()
    {
        var ship = ShipStatus.Instance;
        var count = ship?.AllCameras?.Length ?? 0;
        var pages = Mathf.CeilToInt(count / 4f);
        if (pages <= 0) pages = 1;
        return pages;
    }

    public static GameObject CreateSkeldArrow(Transform parent, Transform closeBtn, string name, Vector3 worldPos, bool isRight)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, true);
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.position = worldPos;
        go.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
        go.SetActive(true);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = _polusPageArrowSprite ?? MiraAssets.NextButton.LoadAsset();
        sr.enabled = true;

        if (closeBtn.TryGetComponent<SpriteRenderer>(out var closeSr))
        {
            sr.sortingLayerID = closeSr.sortingLayerID;
            sr.sortingOrder = closeSr.sortingOrder + 10;
            sr.material = closeSr.material;
            sr.color = closeSr.color;
        }

        if (isRight)
        {
            var desiredFlip = _polusRightArrowFlipX;
            if (_polusRightArrowScaleX < 0f) desiredFlip = !_polusRightArrowFlipX;
            if (_polusPageArrowSprite == null) desiredFlip = true;
            sr.flipX = desiredFlip;
        }
        else
        {
            var desiredFlip = _polusLeftArrowFlipX;
            if (_polusLeftArrowScaleX < 0f) desiredFlip = !_polusLeftArrowFlipX;
            if (_polusPageArrowSprite == null) desiredFlip = false;
            sr.flipX = desiredFlip;
        }

        return go;
    }

    public static void EnsureSkeldPagingButtons(SurveillanceMinigame minigame)
    {
        var ship = ShipStatus.Instance;
        if (ship == null || ship.AllCameras == null || ship.AllCameras.Length <= 4)
        {
            return;
        }

        Transform? viewables = null;
        try
        {
            viewables = minigame.Viewables != null ? minigame.Viewables.transform : null;
        }
        catch
        {
            // ignored
        }

        if (viewables == null)
        {
            return;
        }

        var closeBtn = viewables.Find("CloseButton");
        if (closeBtn == null)
        {
            return;
        }

        var viewPorts = minigame.ViewPorts;
        if (viewPorts == null || viewPorts.Length == 0)
        {
            return;
        }

        var minX = viewPorts.Min(v => v.transform.position.x);
        var maxX = viewPorts.Max(v => v.transform.position.x);
        var minY = viewPorts.Min(v => v.transform.position.y);
        var buttonY = minY - 0.45f;
        var buttonZ = closeBtn.position.z;
        var xOffset = 1.1f;
        var existingRight = viewables.Find("SentryRightArrow");
        var existingLeft = viewables.Find("SentryLeftArrow");

        GameObject right;
        if (existingRight == null)
        {
            Logger.LogInfo("[BUTTONS] Creating NEW right arrow button");
            right = CreateSkeldArrow(viewables, closeBtn, "SentryRightArrow",
                new Vector3(maxX + xOffset, buttonY, buttonZ), isRight: true);
            Logger.LogInfo($"[BUTTONS] Right arrow created, active: {right.activeSelf}");
        }
        else
        {
            right = existingRight.gameObject;
            var rightSr = right.GetComponent<SpriteRenderer>();
            if (rightSr != null) rightSr.enabled = true;
            right.SetActive(true);
        }

        GameObject left;
        if (existingLeft == null)
        {
            Logger.LogInfo("[BUTTONS] Creating NEW left arrow button");
            left = CreateSkeldArrow(viewables, closeBtn, "SentryLeftArrow",
                new Vector3(minX - xOffset, buttonY, buttonZ), isRight: false);
            Logger.LogInfo($"[BUTTONS] Left arrow created, active: {left.activeSelf}");
        }
        else
        {
            left = existingLeft.gameObject;
            var leftSr = left.GetComponent<SpriteRenderer>();
            if (leftSr != null) leftSr.enabled = true;
            left.SetActive(true);
        }

        EnsureSkeldDotIndicatorExists(minigame, GetSkeldPages());

        try
        {
            if (viewables != null)
            {
                var viewablesPb = viewables.GetComponent<PassiveButton>();
                var viewablesCol = viewables.GetComponent<Collider2D>();

                Logger.LogInfo($"[BACKGROUND] Viewables PB: {(viewablesPb != null ? $"enabled={viewablesPb.enabled}" : "NULL")}, Col: {(viewablesCol != null ? $"enabled={viewablesCol.enabled}" : "NULL")}");

                if (viewablesPb != null)
                {
                    var wasEnabled = viewablesPb.enabled;
                    viewablesPb.enabled = true;
                    if (!wasEnabled)
                    {
                        Logger.LogWarning("[BACKGROUND] Viewables PassiveButton was DISABLED! Re-enabled it.");
                    }
                }

                if (viewablesCol != null)
                {
                    var wasEnabled = viewablesCol.enabled;
                    viewablesCol.enabled = true;
                    if (!wasEnabled)
                    {
                        Logger.LogWarning("[BACKGROUND] Viewables Collider2D was DISABLED! Re-enabled it.");
                    }
                }

                for (int i = 0; i < viewables.childCount; i++)
                {
                    var child = viewables.GetChild(i);
                    if (child == null || child.name.Contains("Sentry") || child.name == "CloseButton") continue;

                    var childPb = child.GetComponent<PassiveButton>();
                    var childCol = child.GetComponent<Collider2D>();

                    if (childCol != null && childCol.bounds.size.magnitude > 1.0f)
                    {
                        var wasEnabled = childCol.enabled;
                        childCol.enabled = true;
                        if (!wasEnabled)
                        {
                            Logger.LogWarning($"[BACKGROUND] Large background collider on '{child.name}' was DISABLED! Re-enabled it.");
                        }
                    }

                    if (childPb != null && childPb != right.GetComponent<PassiveButton>() && childPb != left.GetComponent<PassiveButton>())
                    {
                        var wasEnabled = childPb.enabled;
                        childPb.enabled = true;
                        if (!wasEnabled)
                        {
                            Logger.LogWarning($"[BACKGROUND] Background PassiveButton on '{child.name}' was DISABLED! Re-enabled it.");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"[BACKGROUND] Exception checking background close handler: {ex}");
        }
    }

    public static void EnsureSkeldDotIndicatorExists(SurveillanceMinigame minigame, int numberOfPages)
    {
        if (numberOfPages <= 1) return;

        Transform? viewables = null;
        try { viewables = minigame.Viewables != null ? minigame.Viewables.transform : null; } catch { /* ignored */ }
        if (viewables == null) return;

        var container = viewables.Find("ObserverPageDots") ?? viewables.Find("SentryPageDots");
        if (container != null)
        {
            if (container.childCount == numberOfPages && container.name == "ObserverPageDots") return;
            Object.DestroyImmediate(container.gameObject);
        }

        if (_polusDotSelectedSprite == null || _polusDotUnselectedSprite == null)
        {
            try
            {
                if (_polusDotSelectedSprite == null)
                {
                    var allSprites = Resources.FindObjectsOfTypeAll(Il2CppType.From(typeof(Sprite)));
                    _polusDotSelectedSprite =
                        allSprites.FirstOrDefault(s => s != null && s.TryCast<Sprite>() != null &&
                                                       s.Cast<Sprite>().name == "panel_security_camselected")?.TryCast<Sprite>();
                }
                if (_polusDotUnselectedSprite == null)
                {
                    var allSprites = Resources.FindObjectsOfTypeAll(Il2CppType.From(typeof(Sprite)));
                    _polusDotUnselectedSprite =
                        allSprites.FirstOrDefault(s => s != null && s.TryCast<Sprite>() != null &&
                                                       s.Cast<Sprite>().name == "panel_security_camnotselect")?.TryCast<Sprite>();
                }
            }
            catch
            {
                // ignored
            }
        }
        if (_polusDotSelectedSprite == null || _polusDotUnselectedSprite == null) return;

        var go = new GameObject("ObserverPageDots");
        go.transform.SetParent(viewables, false);

        var closeBtn = viewables.Find("CloseButton");
        var closeZ = go.transform.position.z;
        SpriteRenderer? closeSr = null;
        if (closeBtn != null)
        {
            go.layer = closeBtn.gameObject.layer;
            closeZ = closeBtn.position.z;
            closeBtn.TryGetComponent(out closeSr);
        }

        var dotScale = 0.80f;
        for (var i = 0; i < numberOfPages; i++)
        {
            var dot = new GameObject($"Dot{i}");
            dot.transform.SetParent(go.transform, false);
            dot.layer = go.layer;

            var sr = dot.AddComponent<SpriteRenderer>();
            sr.sprite = _polusDotUnselectedSprite;
            sr.enabled = true;

            if (closeSr != null)
            {
                sr.sortingLayerID = closeSr.sortingLayerID;
                sr.sortingOrder = closeSr.sortingOrder + 1;
                sr.material = closeSr.material;
                sr.color = closeSr.color;
            }

            dot.transform.localScale = new Vector3(dotScale, dotScale, 1f);
            dot.transform.position = new Vector3(dot.transform.position.x, dot.transform.position.y, closeZ);
        }
    }

    public static void UpdateSkeldDotIndicator(SurveillanceMinigame minigame, int numberOfPages)
    {
        if (numberOfPages <= 1) return;

        Transform? viewables = null;
        try { viewables = minigame.Viewables != null ? minigame.Viewables.transform : null; } catch { /* ignored */ }
        if (viewables == null) return;

        EnsureSkeldDotIndicatorExists(minigame, numberOfPages);

        var container = viewables.Find("ObserverPageDots");
        if (container == null) return;
        if (container.childCount != numberOfPages) return;

        var viewPorts = minigame.ViewPorts;
        if (viewPorts == null || viewPorts.Length == 0) return;

        var minX = viewPorts.Min(v => v.transform.position.x);
        var maxX = viewPorts.Max(v => v.transform.position.x);
        var minY = viewPorts.Min(v => v.transform.position.y);
        var buttonY = minY - 0.45f;
        var dotsY = buttonY - 0.22f;
        var centerX = (minX + maxX) * 0.5f;

        var z = container.position.z;
        try
        {
            var closeBtn = viewables.Find("CloseButton");
            if (closeBtn != null) z = closeBtn.position.z;
        }
        catch
        {
            // ignored
        }

        var dotSpacing = 0.25f;
        var startX = centerX - ((numberOfPages - 1) * dotSpacing) * 0.5f;

        var pages = Math.Max(1, numberOfPages);
        var pageIdx = ((CurrentPage % pages) + pages) % pages;

        for (var i = 0; i < numberOfPages; i++)
        {
            var dotTf = container.GetChild(i);
            if (dotTf == null) continue;

            dotTf.position = new Vector3(startX + i * dotSpacing, dotsY, z);

            var sr = dotTf.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            sr.sprite = i == pageIdx ? _polusDotSelectedSprite : _polusDotUnselectedSprite;
            sr.enabled = true;
        }
        container.gameObject.SetActive(true);
    }

    public static void CachePolusPagingUi(PlanetSurveillanceMinigame minigame)
    {
        try
        {
            var buttons = minigame.GetComponentsInChildren<PassiveButton>(true);
            foreach (var b in buttons)
            {
                if (b == null) continue;
                var name = b.gameObject.name ?? string.Empty;
                var sr = b.GetComponent<SpriteRenderer>();
                if (sr == null || sr.sprite == null) continue;

                if ((name.Contains("arrow", StringComparison.OrdinalIgnoreCase) ||
     name.Contains("Arrow", StringComparison.OrdinalIgnoreCase)) &&
    sr.sprite.name == "panel_security_arrow")
                {
                    _polusPageArrowSprite ??= sr.sprite;
                    if (name.Equals("arrow_left", StringComparison.OrdinalIgnoreCase))
                    {
                        _polusLeftArrowFlipX = sr.flipX;
                        _polusLeftArrowScaleX = sr.transform.localScale.x;
                    }
                    else if (name.Equals("arrow_right", StringComparison.OrdinalIgnoreCase))
                    {
                        _polusRightArrowFlipX = sr.flipX;
                        _polusRightArrowScaleX = sr.transform.localScale.x;
                    }

                    if (b.ClickSound != null && b.ClickSound.name == "UI_Select")
                    {
                        _polusPageFlipSound ??= b.ClickSound;
                    }
                }
            }

            try
            {
                var srs = minigame.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var sr in srs)
                {
                    if (sr == null || sr.sprite == null) continue;
                    if (sr.sprite.name == "panel_security_camselected") _polusDotSelectedSprite ??= sr.sprite;
                    if (sr.sprite.name == "panel_security_camnotselect") _polusDotUnselectedSprite ??= sr.sprite;
                }
            }
            catch
            {
               // ignored
            }

            if (_polusPageArrowSprite == null)
            {
                var allSprites = Resources.FindObjectsOfTypeAll(Il2CppType.From(typeof(Sprite)));
                _polusPageArrowSprite = allSprites.FirstOrDefault(s => s != null && s.TryCast<Sprite>() != null && s.Cast<Sprite>().name == "panel_security_arrow")?.TryCast<Sprite>();
            }

            if (_polusPageFlipSound == null)
            {
                var allClips = Resources.FindObjectsOfTypeAll(Il2CppType.From(typeof(AudioClip)));
                _polusPageFlipSound = allClips.FirstOrDefault(a => a != null && a.TryCast<AudioClip>() != null && a.Cast<AudioClip>().name == "UI_Select")?.TryCast<AudioClip>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error caching Polus UI: {ex}");
        }
    }

    public static void TryCachePolusPagingUi()
    {
        if (_polusPageArrowSprite != null && _polusPageFlipSound != null)
        {
            return;
        }

        try
        {
            var anyObj = Resources.FindObjectsOfTypeAll(Il2CppType.From(typeof(PlanetSurveillanceMinigame)))
    .FirstOrDefault(x => x != null);
            var any = anyObj != null ? anyObj.TryCast<PlanetSurveillanceMinigame>() : null;
            if (any != null)
            {
                CachePolusPagingUi(any);
            }
        }
        catch
        {
            // ignored
        }
    }

    public static AudioClip? GetPageFlipSound() => _polusPageFlipSound;
}