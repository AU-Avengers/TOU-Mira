using Reactor.Utilities.Attributes;
using UnityEngine;

namespace TownOfUs.Modules;

[RegisterInIl2Cpp]
public sealed class ScreenFlash(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    private static readonly List<ScreenFlash> _screenFlashes = [];

    private SpriteRenderer _renderer; 

    public static ScreenFlash CreateFlash()
    {
        var renderer = Instantiate(HudManager.Instance.FullScreen, HudManager.Instance.FullScreen.transform.parent);
        renderer.transform.localPosition = new Vector3(0, 0, -90f);
        renderer.sprite = TouAssets.ScreenFlash;
        renderer.color = Color.white;

        var flash = renderer.gameObject.AddComponent<ScreenFlash>();
        flash._renderer = renderer;

        _screenFlashes.Add(flash);

        flash.SetActive(false);

        return flash;
    }

    public static void Clear()
    {
        _screenFlashes.ProcessAndClear(x => x.Destroy());
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
        _renderer?.color = color;
    }

    public void Destroy()
    {
        if (_renderer && _renderer.gameObject != null)
        {
            Destroy(_renderer.gameObject);
        }
        _screenFlashes.Remove(this);
    }
}