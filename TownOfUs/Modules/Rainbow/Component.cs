using UnityEngine;

namespace TownOfUs.Modules.RainbowMod;

public sealed class RainbowBehaviour : MonoBehaviour
{
    public Renderer Renderer;
    public int Id;

    public void Update()
    {
        if (!Renderer)
        {
            return;
        }

        if (RainbowUtils.IsRainbow(Id))
        {
            RainbowUtils.SetRainbow(Renderer);
        }
    }

    public void AddRend(Renderer rend, int id)
    {
        Renderer = rend;
        Id = id;
    }
}

public sealed class BasicRainbowBehaviour : MonoBehaviour
{
    public SpriteRenderer Renderer;
    public int Id;

    public void Update()
    {
        if (!Renderer)
        {
            return;
        }

        if (RainbowUtils.IsRainbow(Id))
        {
            Renderer.color = RainbowUtils.SetBasicRainbow();
        }
    }

    public void AddRend(SpriteRenderer rend, int id)
    {
        Renderer = rend;
        Id = id;
    }
}

public sealed class LightRainbowBehaviour : MonoBehaviour
{
    public SpriteRenderer Renderer;
    public int Id;

    public void Update()
    {
        if (!Renderer)
        {
            return;
        }

        if (RainbowUtils.IsRainbow(Id))
        {
            Renderer.color = RainbowUtils.LightUp(RainbowUtils.SetBasicRainbow());
        }
    }

    public void AddRend(SpriteRenderer rend, int id)
    {
        Renderer = rend;
        Id = id;
    }
}