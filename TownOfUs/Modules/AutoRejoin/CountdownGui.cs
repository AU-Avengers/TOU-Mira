
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace TownOfUs.Modules.AutoRejoin;

[RegisterInIl2Cpp]
public class CountdownGui(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    private GUIStyle? _style;
    private GUIStyle? _shadow;

    private void BuildStyles()
    {
        _style = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _style.normal.textColor  = new Color(0f, 0.85f, 1f, 1f);
        _shadow = new GUIStyle(_style);
        _shadow.normal.textColor = new Color(0f, 0f, 0f, 0.65f);
    }

    private void OnGUI()
    {
        string text = RejoinBehaviour.ScreenText;
        if (string.IsNullOrEmpty(text)) return;
        if (_style == null) BuildStyles();

        float w = Screen.width * 0.7f;
        float h = 44f;
        float x = (Screen.width - w) / 2f;
        float y = Screen.height - 80f;

        GUI.Label(new Rect(x + 1f, y + 1f, w, h), text, _shadow);
        GUI.Label(new Rect(x,      y,       w, h), text, _style);
    }
}