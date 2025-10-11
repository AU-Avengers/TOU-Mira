using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Modules.RainbowMod;

public static class RainbowUtils
{
    public static Color Rainbow => new HSBColor(PP(0, 1, 0.3f), 1, 1).ToColor();
    public static Color RainbowShadow => Shadow(Rainbow);

    public static float PP(float min, float max, float mul)
    {
        return min + Mathf.PingPong(Time.time * mul, max - min);
    }

    public static Color Shadow(Color color)
    {
        return new Color(color.r - 0.3f, color.g - 0.3f, color.b - 0.3f);
    }

    public static void SetRainbow(Renderer rend)
    {
        rend.material.SetColor(ShaderID.BackColor, RainbowShadow);
        rend.material.SetColor(ShaderID.BodyColor, Rainbow);
        rend.material.SetColor(ShaderID.VisorColor, Palette.VisorColor);
    }

    public static Color SetBasicRainbow()
    {
        return Rainbow;
    }

    public static bool IsRainbow(int id)
    {
        try
        {
            return Palette.ColorNames[id] == TownOfUsPlayerColors.Rainbow.Name;
        }
        catch
        {
            return false;
        }
    }
}

[Serializable]
public struct HSBColor
{
    public float h;
    public float s;
    public float b;
    public float a;

    public HSBColor(float h, float s, float b)
    {
        this.h = h;
        this.s = s;
        this.b = b;
        a = 1f;
    }

    public static Color ToColor(HSBColor hsbColor)
    {
        var r = hsbColor.b;
        var g = hsbColor.b;
        var b = hsbColor.b;
        if (hsbColor.s != 0)
        {
            var max = hsbColor.b;
            var dif = hsbColor.b * hsbColor.s;
            var min = hsbColor.b - dif;

            var h = hsbColor.h * 360f;

            if (h < 60f)
            {
                r = max;
                g = h * dif / 60f + min;
                b = min;
            }
            else if (h < 120f)
            {
                r = -(h - 120f) * dif / 60f + min;
                g = max;
                b = min;
            }
            else if (h < 180f)
            {
                r = min;
                g = max;
                b = (h - 120f) * dif / 60f + min;
            }
            else if (h < 240f)
            {
                r = min;
                g = -(h - 240f) * dif / 60f + min;
                b = max;
            }
            else if (h < 300f)
            {
                r = (h - 240f) * dif / 60f + min;
                g = min;
                b = max;
            }
            else if (h <= 360f)
            {
                r = max;
                g = min;
                b = -(h - 360f) * dif / 60 + min;
            }
            else
            {
                r = 0;
                g = 0;
                b = 0;
            }
        }

        return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), hsbColor.a);
    }

    public readonly Color ToColor()
    {
        return ToColor(this);
    }
}