using MiraAPI.Colors;
using MiraAPI.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace TownOfUs.Modules.RainbowMod;

[RegisterInIl2Cpp]
public sealed class RainbowBehaviour(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    public Renderer Renderer;
    public int Id;

    public void Update()
    {
        if (!Renderer)
        {
            return;
        }

        if (PaletteManager.IsVariable(Id))
        {
            SetVariableRend(Renderer, Id);
        }
    }

    public void AddRend(Renderer rend, int id)
    {
        Renderer = rend;
        Id = id;
    }

    public static void SetVariableRend(Renderer rend, int id)
    {
        rend.material.SetColor(ShaderID.BackColor, PaletteManager.GetShadowColor(id));
        rend.material.SetColor(ShaderID.BodyColor, PaletteManager.GetMainColor(id));
        rend.material.SetColor(ShaderID.VisorColor, Palette.VisorColor);
    }

    public static Color LightUp(Color color)
    {
        return new Color(color.r + 0.3f, color.g + 0.3f, color.b + 0.3f);
    }
}

[RegisterInIl2Cpp]
public sealed class BasicRainbowBehaviour(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    public SpriteRenderer Renderer;
    public int Id;

    public void Update()
    {
        if (!Renderer)
        {
            return;
        }

        if (PaletteManager.IsVariable(Id))
        {
            Renderer.color = PaletteManager.GetMainColor(Id);
        }
    }

    public void AddRend(SpriteRenderer rend, int id)
    {
        Renderer = rend;
        Id = id;
    }
}

[RegisterInIl2Cpp]
public sealed class LightRainbowBehaviour(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    public SpriteRenderer Renderer;
    public int Id;

    public void Update()
    {
        if (!Renderer)
        {
            return;
        }

        if (PaletteManager.IsVariable(Id))
        {
            Renderer.color = RainbowBehaviour.LightUp(PaletteManager.GetMainColor(Id));
        }
    }

    public void AddRend(SpriteRenderer rend, int id)
    {
        Renderer = rend;
        Id = id;
    }
}