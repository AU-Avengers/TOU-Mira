using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TMPro;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers.Impostor;

/// <summary>
/// Applied to the victim while they are controlled by a Parasite.
/// - Disables their buttons/actions (via DisabledModifier base checks).
/// - Forces their appearance to match the Parasite (visible to others).
/// Movement/input suppression is handled by Harmony patches while this modifier is present.
/// </summary>
public sealed class ParasiteInfectedModifier(PlayerControl controller) : DisabledModifier, IVisualAppearance
{
    public override string ModifierName => "Parasite Infected";
    public override bool HideOnUi => true;
    public override bool AutoStart => true;

    public override bool CanUseAbilities => false;
    public override bool CanReport => false;

    public override float Duration => Mathf.Max(9999f, OptionGroupSingleton<ParasiteOptions>.Instance.ControlDuration);

    public bool VisualPriority => true;
    public PlayerControl Controller { get; } = controller;

    private GameObject? _overlayRoot;
    private SpriteRenderer? _controlOverlay;
    private LobbyNotificationMessage? _controlledNotification;

    public void UpdateOverlayLayout()
    {
        if (_overlayRoot == null || _controlOverlay == null || _controlOverlay.sprite == null || !HudManager.InstanceExists ||
            Camera.main == null)
        {
            return;
        }

        // NOTE: This overlay is for the *infected* client (feedback that they're being controlled),
        // not the Parasite's PiP camera. Keep it full-screen and non-interactive.
        var screenWidth = Screen.width;
        var screenHeight = Screen.height;

        var hudCam = Camera.main;
        var worldBottomLeft = hudCam.ScreenToWorldPoint(new Vector3(0f, 0f, hudCam.nearClipPlane));
        var worldTopRight =
            hudCam.ScreenToWorldPoint(new Vector3(screenWidth, screenHeight, hudCam.nearClipPlane));

        var baseZ = HudManager.Instance.FullScreen != null ? HudManager.Instance.FullScreen.transform.position.z : 0f;
        var z = baseZ + 1f;

        var worldCenter = new Vector3(
            (worldBottomLeft.x + worldTopRight.x) * 0.5f,
            (worldBottomLeft.y + worldTopRight.y) * 0.5f,
            z
        );
        _overlayRoot.transform.position = worldCenter;

        var worldWidth = Mathf.Abs(worldTopRight.x - worldBottomLeft.x);
        var worldHeight = Mathf.Abs(worldTopRight.y - worldBottomLeft.y);

        var spriteSize = _controlOverlay.sprite.bounds.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return;
        }

        const float scaleMultiplier = 1.42f;
        _overlayRoot.transform.localScale = new Vector3(
            (worldWidth * scaleMultiplier) / spriteSize.x,
            (worldHeight * scaleMultiplier) / spriteSize.y,
            1f
        );
    }

    public VisualAppearance? GetVisualAppearance()
    {
        if (!OptionGroupSingleton<ParasiteOptions>.Instance.InfectedLooksLikeParasite)
        {
            return null;
        }
        return new VisualAppearance(Controller.GetDefaultModifiedAppearance(), TownOfUsAppearances.Morph);
    }

    public override void OnActivate()
    {
        if (OptionGroupSingleton<ParasiteOptions>.Instance.InfectedLooksLikeParasite)
        {
            Player.RawSetAppearance(this);
        }

        if (Player.AmOwner)
        {
            TouAudio.PlaySound(TouAudio.HackedSound);
            CreateNotification();
        }

        if (Player.AmOwner && HudManager.InstanceExists)
        {
            var pingTracker = Object.FindObjectOfType<PingTracker>(true);
            if (pingTracker != null && HudManager.Instance != null)
            {
                _overlayRoot = Object.Instantiate(pingTracker.gameObject, HudManager.Instance.transform);
                _overlayRoot.name = "ParasiteInfectedOverlay";

                var tmp = _overlayRoot.GetComponent<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.enabled = false;
                }

                var meshRenderer = _overlayRoot.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.enabled = false;
                }

                var aspect = _overlayRoot.GetComponent<AspectPosition>();
                if (aspect != null)
                {
                    aspect.enabled = false;
                }
                
                var borderObj = UnityEngine.Object.Instantiate(TouAssets.ParasiteOverlay.LoadAsset(), _overlayRoot.transform);
                borderObj.layer = _overlayRoot.layer;
                borderObj.transform.localPosition = new Vector3(0f, 0f, 0f);

                _controlOverlay = borderObj.GetComponent<SpriteRenderer>();
                _controlOverlay.sortingOrder = 1000;
                _controlOverlay.color = new Color(1f, 1f, 1f, 0.95f);

                UpdateOverlayLayout();
                _overlayRoot.SetActive(true);
            }
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        Player.ResetAppearance(true);

        if (_overlayRoot != null)
        {
            _overlayRoot.SetActive(false);
            Object.Destroy(_overlayRoot);
            _overlayRoot = null;
            _controlOverlay = null;
        }

        ClearNotification();
    }

    private void CreateNotification()
    {
        if (Player == null || !Player.AmOwner || PlayerControl.LocalPlayer == null)
        {
            return;
        }

        if (_controlledNotification == null)
        {
            var controlledText = TouLocale.GetParsed("TouRoleParasiteControlledNotif", "You are being controlled by a Parasite!");
            _controlledNotification = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Impostor.ToTextColor()}{controlledText}</color></b>",
                Color.white, new Vector3(0f, 2f, -20f), spr: TouRoleIcons.Parasite.LoadAsset());
            _controlledNotification?.AdjustNotification();
        }
    }

    private void ClearNotification()
    {
        if (_controlledNotification != null && _controlledNotification.gameObject != null)
        {
            UnityEngine.GameObject.Destroy(_controlledNotification);
            _controlledNotification = null;
        }
    }
}