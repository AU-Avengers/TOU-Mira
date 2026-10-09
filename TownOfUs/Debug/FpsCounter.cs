using Reactor.Utilities.Attributes;
using UnityEngine;

namespace TownOfUs.DebugTools;

[RegisterInIl2Cpp]
public sealed class FpsCounter(nint cppPtr) : MonoBehaviour(cppPtr)
{
    private float _deltaTime;

    private void Update()
    {
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        var fps = 1.0f / _deltaTime;
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            normal = { textColor = Color.white },
        };
        GUI.Label(new Rect(10, 10, 200, 40), $"{fps:F1} FPS", style);
    }
}
