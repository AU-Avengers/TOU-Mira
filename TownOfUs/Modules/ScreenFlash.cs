using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modules;

// Code Review: Should be using a MonoBehaviour
public sealed class ScreenFlash : IDisposable
{
    private static readonly List<ScreenFlash> _screenFlashes = new();

    private readonly SpriteRenderer _renderer;

    public ScreenFlash()
    {
        _renderer = Object.Instantiate(HudManager.Instance.FullScreen, HudManager.Instance.FullScreen.transform.parent);
        _renderer.transform.localPosition = new Vector3(0, 0, -90f);
        _renderer.sprite = TouAssets.ScreenFlash.LoadAsset();
        _renderer.color = Color.white;

        _screenFlashes.Add(this);

        SetActive(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public static void Clear()
    {
        _screenFlashes.Do(x => x.Destroy());
        _screenFlashes.Clear();
    }

    public bool IsActive()
    {
        return _renderer.gameObject.activeSelf;
    }

    public void SetActive(bool isActive)
    {
        _renderer.gameObject.SetActive(isActive);
    }

    public void SetPosition(Vector3 pos)
    {
        _renderer.transform.localPosition = pos;
    }

    public void SetScale(Vector3 scale)
    {
        _renderer.transform.localScale = scale;
    }

    public void SetColour(Color color)
    {
        if (_renderer != null)
        {
            _renderer.color = color;
        }
    }

    public void Destroy()
    {
        Dispose();
    }

    private void Dispose(bool disposing)
    {
        if (disposing && _renderer && _renderer.gameObject != null)
        {
            Object.Destroy(_renderer.gameObject);
        }
    }
}