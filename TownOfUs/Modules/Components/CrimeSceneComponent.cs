using System.Collections;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modules.Components;

[RegisterInIl2Cpp]
public sealed class CrimeSceneComponent(nint cppPtr) : MonoBehaviour(cppPtr)
{
    public static readonly List<CrimeSceneComponent> _crimeScenes = [];

    private readonly List<byte> _scenePlayers = [];
    public PlayerControl? DeadPlayer { get; set; }
    public BoxCollider2D? Collider { get; set; }

    public void Awake()
    {
        var render = gameObject.AddComponent<SpriteRenderer>();
        render.sprite = TouAssets.CrimeSceneSprite.LoadAsset();
        var scale = render.transform.localScale;
        scale.x *= 0.5f;
        scale.y *= 0.5f;
        render.transform.localScale = scale;

        Collider = gameObject.AddComponent<BoxCollider2D>();
        Collider.size = new Vector2(render.size.x, render.size.y);
        Collider.isTrigger = true;
        Collider.enabled = true;
    }

    public void FixedUpdate()
    {
        var killDistances =
            GameOptionsManager.Instance.currentNormalGameOptions.GetFloatArray(FloatArrayOptionNames.KillDistances);

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.Data.IsDead)
            {
                continue;
            }

            if (player.AmOwner)
            {
                continue;
            }

            // Debug.Log(GetComponent<BoxCollider2D>().IsTouching(player.Collider));
            if (Vector2.Distance(player.GetTruePosition(), gameObject.transform.position) >
                killDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance])
            {
                continue;
            }

            if (!_scenePlayers.Contains(player.PlayerId))
                // Debug.Log(player.name + " contaminated the crime scene");
            {
                _scenePlayers.Add(player.PlayerId);
            }
        }
    }

    [HideFromIl2Cpp]
    public List<byte> GetScenePlayers()
    {
        return _scenePlayers;
    }

    public static void CreateCrimeScene(PlayerControl victim, Vector3 location)
    {
        var bloodSplat = new GameObject("CrimeScene");
        bloodSplat.transform.position = new Vector3(location.x, location.y, location.y / 1000f + 0.01f);
        bloodSplat.layer = LayerMask.NameToLayer("Players");

        var scene = bloodSplat.AddComponent<CrimeSceneComponent>();
        scene.DeadPlayer = victim;

        _crimeScenes.Add(scene);

        scene.gameObject.SetActive(false);
        if (PlayerControl.LocalPlayer.Data.Role is DetectiveTouRole)
        {
            scene.gameObject.SetActive(true);
        }
    }

    public static IEnumerator CoClean(DeadBody body)
    {
        var crimeScene = _crimeScenes.FirstOrDefault(x => x.DeadPlayer == MiscUtils.PlayerById(body.ParentId));

        if (crimeScene == null)
        {
            yield break;
        }

        var renderer = crimeScene.gameObject.GetComponent<SpriteRenderer>();

        yield return MiscUtils.PerformTimedAction(1f, t => renderer!.color = renderer.color.SetAlpha(1 - t));

        Destroy(crimeScene.gameObject);
        _crimeScenes.Remove(crimeScene);
    }

    public static void Clear()
    {
        _crimeScenes.Do(x =>
        {
            if (x != null && x.gameObject != null)
            {
                Destroy(x.gameObject);
            }
        });

        _crimeScenes.Clear();
    }
}