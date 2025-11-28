using AuAvengers.Animations;
using MiraAPI.Modifiers;
using PowerTools;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modules.Anims;

// CODE REVIEW 22/2/2025 AEDT (D/M/Y)
// ---------------------------------
// Keep in mind that animations need to be added for all unimplemented roles,
// and many of the implemented ones.
public static class AnimStore
{
    public static Material PlayerMat => HatManager.InstanceExists
        ? HatManager.Instance.PlayerMaterial
        : throw new InvalidOperationException("Tried to use PlayerMat before it was initialized");

    public static GameObject SpawnAnimPlayer(
        PlayerControl player,
        GameObject prefab,
        bool isCamo = false,
        bool playerIsParent = true)
    {
        var parent = playerIsParent ? player.transform : null;

        var spawned = Object.Instantiate(prefab, parent);

        var a = prefab.transform.localScale;
        var b = player.transform.localScale;
        var scale = new Vector3(a.x / b.x, a.y / b.y, 1);
        if (player.HasModifier<GiantModifier>())
        {
            scale /= 0.7f;
        }
        else if (player.HasModifier<MiniModifier>())
        {
            scale *= 0.7f;
        }

        spawned.transform.localScale = scale;

        var cMat = SetSpriteColourMatch(player, PlayerMat);
        var search = spawned.transform.FindRecursive("Hands");
        if (!search)
        {
            search = spawned.transform.FindRecursive("Hand");
        }

        if (search)
        {
            SetSpriteColourMatch(PlayerControl.LocalPlayer, cMat, search.GetComponent<Renderer>(), isCamo);
        }

        return spawned;
    }

    public static Material SetSpriteColourMatch(PlayerControl player, Material material)
    {
        var newMat = new Material(material);
        var colorId = player.cosmetics.bodyMatProperties.ColorId;
        PlayerMaterial.SetColors(colorId, newMat);
        return newMat;
    }

    public static void SetSpriteColourMatch(
        PlayerControl player,
        Material material,
        Renderer renderer,
        bool isCamo = false)
    {
        var newMat = new Material(material);
        var colorId = player.cosmetics.bodyMatProperties.ColorId;
        renderer.material = newMat;

        if (isCamo)
        {
            PlayerMaterial.SetColors(Color.grey, renderer);
            return;
        }

        PlayerMaterial.SetColors(colorId, renderer);
    }

    public static GameObject SpawnAnimBody(
        PlayerControl player,
        GameObject prefab,
        bool shouldOffset = true,
        float offsetAmount = 0.8f,
        float yOffset = -0.575f,
        float zOffset = 0f)
    {
        var cosmeticsLayer = player.transform.GetChild(2);
        var animation = SpawnAnimPlayer(player, prefab);
        var animationBounceHolder = new GameObject($"A_{prefab.name}");
        animationBounceHolder.transform.SetParent(cosmeticsLayer, false);
        animationBounceHolder.transform.localPosition =
            new Vector3(0, yOffset + 0.05f, zOffset); // -0.04f, 0.575 (but we flip to reverse this)

        animation.transform.SetParent(animationBounceHolder.transform, true);
        var bounceSync = animation.AddComponent<SpriteAnimNodeSync>();
        var hatParent = animationBounceHolder.transform.parent.GetChild(0).GetComponent<HatParent>();

        bounceSync.NodeId = 1;
        bounceSync.Parent = hatParent.SpriteSyncNode.Parent;
        bounceSync.Renderer = bounceSync.ParentRenderer = hatParent.SpriteSyncNode.Renderer;

        var spriteFlipper = animation.AddComponent<UE_SpriteFlipper>();
        spriteFlipper.reference = cosmeticsLayer.GetComponent<CosmeticsLayer>();
        spriteFlipper.DoOffset = shouldOffset;
        spriteFlipper.Offset = offsetAmount;

        return animation;
    }

    public static GameObject SpawnFliplessAnimBody(
        PlayerControl player,
        GameObject prefab,
        float yOffset = -0.575f,
        float zOffset = 0f)
    {
        var cosmeticsLayer = player.transform.GetChild(2);
        var animation = SpawnAnimPlayer(player, prefab);
        var animationBounceHolder = new GameObject($"A_{prefab.name}");
        animationBounceHolder.transform.SetParent(cosmeticsLayer, false);
        animationBounceHolder.transform.localPosition =
            new Vector3(0, yOffset + 0.05f, zOffset); // -0.04f, 0.575 (but we flip to reverse this)

        animation.transform.SetParent(animationBounceHolder.transform, true);
        var bounceSync = animation.AddComponent<SpriteAnimNodeSync>();
        var hatParent = animationBounceHolder.transform.parent.GetChild(0).GetComponent<HatParent>();

        bounceSync.NodeId = 1;
        bounceSync.Parent = hatParent.SpriteSyncNode.Parent;
        bounceSync.Renderer = bounceSync.ParentRenderer = hatParent.SpriteSyncNode.Renderer;
        return animation;
    }

    public static GameObject SpawnAnimAtPlayer(PlayerControl player, GameObject prefab)
    {
        return SpawnAnimPlayer(player, prefab, playerIsParent: false);
    }
}