using System.Collections;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace TownOfUs.Modules.Components;

[RegisterInIl2Cpp]
public sealed class DrinkSpillComponent(nint cppPtr) : MonoBehaviour(cppPtr)
{
    public static readonly List<DrinkSpillComponent> _drinkSpills = [];

    private readonly List<byte> _affectedPlayers = [];
    public BoxCollider2D Collider { get; set; }
    public PlayerControl Barkeeper { get; private set; }
    public SpriteRenderer Renderer { get; private set; }
    public SpillStage LocalStage = SpillStage.Hidden;

    public void Awake()
    {
        Renderer = gameObject.AddComponent<SpriteRenderer>();
        Renderer.sprite = TouAssets.CrimeSceneSprite.LoadAsset();
        Renderer.color = new(1, 1, 1, 0);
        var scale = Renderer.transform.localScale;
        scale.x *= 0.5f;
        scale.y *= 0.5f;
        Renderer.transform.localScale = scale;

        Collider = gameObject.AddComponent<BoxCollider2D>();
        Collider.size = new Vector2(Renderer.size.x, Renderer.size.y);
        Collider.isTrigger = true;
        Collider.enabled = true;
    }

    public void FixedUpdate()
    {
        if (LocalStage == SpillStage.Hidden)
        {
            return;
        }
        var killDistances =
            GameOptionsManager.Instance.currentNormalGameOptions.GetFloatArray(FloatArrayOptionNames.KillDistances);

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.Data.IsDead)
            {
                continue;
            }

            // Debug.Log(GetComponent<BoxCollider2D>().IsTouching(player.Collider));
            if (Vector2.Distance(player.GetTruePosition(), gameObject.transform.position) >
                killDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance])
            {
                continue;
            }

            if (!_affectedPlayers.Contains(player.PlayerId))
                // Debug.Log(player.name + " contaminated the crime scene");
            {
                if (player.AmOwner && LocalStage == SpillStage.Triggerable)
                {
                    Coroutines.Start(CoRevealSpill());
                }
                _affectedPlayers.Add(player.PlayerId);
            }
        }
    }

    [HideFromIl2Cpp]
    public List<byte> GetAffectedPlayers()
    {
        return _affectedPlayers;
    }

    public static void CreateDrinkSpill(PlayerControl barkeeper, Vector3 location)
    {
        location.y -= 0.3f;
        location.x -= 0.11f;
        var bloodSplat = new GameObject("DrinkSpill");
        bloodSplat.transform.position = new Vector3(location.x, location.y, location.y / 1000f + 0.01f);
        bloodSplat.layer = LayerMask.NameToLayer("Players");

        var scene = bloodSplat.AddComponent<DrinkSpillComponent>();
        scene.Barkeeper = barkeeper;

        _drinkSpills.Add(scene);
        Coroutines.Start(scene.CoShowSpill());
    }

    private IEnumerator CoShowSpill()
    {
        yield return new WaitForSeconds(5f);
        LocalStage = SpillStage.Triggerable;
        if (Barkeeper && Barkeeper.AmOwner)
        {
            yield return MiscUtils.FadeIn(Renderer);
        }
    }

    private IEnumerator CoRevealSpill()
    {
        LocalStage = SpillStage.Shown;
        if (Renderer.color.a < 1)
        {
            yield return MiscUtils.FadeIn(Renderer);
        }
        var notif = Helpers.CreateAndShowNotification(
            $"<b>You feel a slippery spill beneath you. You've got a speed boost!</b>",
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Barkeeper.LoadAsset());
        notif.Text.SetOutlineThickness(0.35f);
    }

    public static void Clear()
    {
        _drinkSpills.Do(x =>
        {
            if (x != null && x.gameObject != null)
            {
                Destroy(x.gameObject);
            }
        });

        _drinkSpills.Clear();
    }
}

public enum SpillStage
{
    Hidden,
    Triggerable,
    Shown
}