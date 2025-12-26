using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using System.Collections;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class SentryPlaceCameraButton : TownOfUsRoleButton<SentryRole>, IAftermathableButton
{
    public override string Name => TouLocale.GetParsed("TouRoleSentryPlaceCamera", "Deploy");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Sentry;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<SentryOptions>.Instance.PlacementCooldown + MapCooldown, 5f, 120f);
    public override float EffectDuration => 3.001f;
    public override int MaxUses => (int)OptionGroupSingleton<SentryOptions>.Instance.InitialCameras.Value;
    public override LoadableAsset<Sprite> Sprite => TouAssets.PlaceCameraButtonSprite;
    public override bool ZeroIsInfinite { get; set; } = true;
    public int ExtraUses { get; set; }

    public override bool Enabled(RoleBehaviour? role)
    {
        if (role is not SentryRole sentryRole)
        {
            return false;
        }

        if (PlacementInProgress)
        {
            return true;
        }

        if (!sentryRole.CompletedAllTasks)
        {
            return true;
        }

        if (!LimitedUses)
        {
            return true;
        }

        return UsesLeft > 0;
    }

    public bool PlacementInProgress =>
        EffectActive ||
        SavedPos.HasValue ||
        _pendingPlacement ||
        placementCoroutine != null;

    public Vector3? SavedPos { get; set; }
    private const float MaxPlacementDistance = 0.5f; private IEnumerator? placementCoroutine;
    private bool _pendingPlacement;
    private bool _refundApplied;

    public void AftermathHandler()
    {
        ClickHandler();
    }

    public override bool CanUse()
    {
        if (!base.CanUse())
        {
            return false;
        }

        var options = OptionGroupSingleton<SentryOptions>.Instance;
        if (IsInDisabledRoom(PlayerControl.LocalPlayer.transform.position, options))
        {
            return false;
        }

        if (options.MaxCamerasPlaced > 0 && SentryRole.Cameras.Count >= (int)options.MaxCamerasPlaced)
        {
            return false;
        }

        var hits = Physics2D.OverlapBoxAll(PlayerControl.LocalPlayer.transform.position, Vector2.one * 0.5f, 0);
        hits = hits.Where(c =>
                        (c.name.Contains("Vent") || c.name.Contains("Door") || !c.isTrigger) &&
            c.gameObject.layer != 8 &&
            c.gameObject.layer != 5).ToArray();

        var noConflict = !PhysicsHelpers.AnythingBetween(PlayerControl.LocalPlayer.Collider,
            PlayerControl.LocalPlayer.Collider.bounds.center, PlayerControl.LocalPlayer.transform.position,
            Constants.ShipAndAllObjectsMask,
            false);

        return hits.Count == 0 && noConflict && !ModCompatibility.GetPlayerElevator(PlayerControl.LocalPlayer).Item1;
    }

    private static bool IsInDisabledRoom(Vector3 position, SentryOptions options)
    {
        var count = (int)options.BlindspotsCount.Value;
        if (count <= 0)
        {
            return false;
        }

        var roomId = GetRoomId(position);
        if (count >= 1 && (SystemTypes)options.Blindspot1Room.Value == roomId) return true;
        if (count >= 2 && (SystemTypes)options.Blindspot2Room.Value == roomId) return true;
        if (count >= 3 && (SystemTypes)options.Blindspot3Room.Value == roomId) return true;
        if (count >= 4 && (SystemTypes)options.Blindspot4Room.Value == roomId) return true;
        if (count >= 5 && (SystemTypes)options.Blindspot5Room.Value == roomId) return true;
        if (count >= 6 && (SystemTypes)options.Blindspot6Room.Value == roomId) return true;
        if (count >= 7 && (SystemTypes)options.Blindspot7Room.Value == roomId) return true;
        if (count >= 8 && (SystemTypes)options.Blindspot8Room.Value == roomId) return true;
        if (count >= 9 && (SystemTypes)options.Blindspot9Room.Value == roomId) return true;
        if (count >= 10 && (SystemTypes)options.Blindspot10Room.Value == roomId) return true;

        return false;
    }

    private static SystemTypes GetRoomId(Vector3 position)
    {
        if (ShipStatus.Instance == null)
        {
            return SystemTypes.Outside;
        }

        var rooms = ShipStatus.Instance.FastRooms;
        if (rooms != null)
        {
            foreach (var room in rooms.Values)
            {
                if (room != null && room.roomArea != null && room.roomArea.OverlapPoint(position))
                {
                    return room.RoomId;
                }
            }
        }

        return SystemTypes.Outside;
    }

    protected override void OnClick()
    {
        var legacy = OptionGroupSingleton<SentryOptions>.Instance.LegacyMode;
        if (!legacy)
        {
            SavedPos = PlayerControl.LocalPlayer.transform.position;
            return;
        }

        _pendingPlacement = true;
        _refundApplied = false;
        SavedPos = PlayerControl.LocalPlayer.transform.position;
        if (placementCoroutine != null)
        {
            Coroutines.Stop(placementCoroutine);
        }
        placementCoroutine = Coroutines.Start(PlaceCameraCoroutine());
    }

    private IEnumerator PlaceCameraCoroutine()
    {
        var startPos = SavedPos!.Value;
        yield return new WaitForSeconds(3f);

        if (SavedPos.HasValue && _pendingPlacement)
        {
            var distance = Vector2.Distance(PlayerControl.LocalPlayer.transform.position, startPos);
            if (distance <= MaxPlacementDistance)
            {
                var pos2D = new Vector2(SavedPos.Value.x, SavedPos.Value.y);
                SentryRole.RpcPlaceCamera(PlayerControl.LocalPlayer, pos2D);
                SentryRole.RpcRevealCamera(PlayerControl.LocalPlayer, pos2D, SavedPos.Value.z);
                TouAudio.PlaySound(TouAudio.SentryPlaceSound);
                RefreshPortableButtons();
            }
            else
            {
                if (LimitedUses && !_refundApplied)
                {
                    _refundApplied = true;
                    UsesLeft++;
                    SetUses(UsesLeft);
                }

                EffectActive = false;
                Timer = 0f;
            }
            SavedPos = null;
        }
        _pendingPlacement = false;
        placementCoroutine = null;
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);

        if (!OptionGroupSingleton<SentryOptions>.Instance.LegacyMode)
        {
            return;
        }

        if (SavedPos.HasValue && placementCoroutine != null && _pendingPlacement)
        {
            var distance = Vector2.Distance(playerControl.transform.position, SavedPos.Value);
            if (distance > MaxPlacementDistance)
            {
                Coroutines.Stop(placementCoroutine);
                placementCoroutine = null;
                _pendingPlacement = false;

                if (LimitedUses && !_refundApplied)
                {
                    _refundApplied = true;
                    UsesLeft++;
                    SetUses(UsesLeft);
                }

                EffectActive = false;
                Timer = 0f;
                SavedPos = null;
            }
        }
    }

    public override void OnEffectEnd()
    {
        base.OnEffectEnd();

        if (OptionGroupSingleton<SentryOptions>.Instance.LegacyMode)
        {
            return;
        }

        if (!SavedPos.HasValue)
        {
            return;
        }

        var pos = SavedPos.Value;
        var pos2D = new Vector2(pos.x, pos.y);
        SentryRole.RpcPlaceCamera(PlayerControl.LocalPlayer, pos2D);
        SentryRole.RpcRevealCamera(PlayerControl.LocalPlayer, pos2D, pos.z);
        TouAudio.PlaySound(TouAudio.SentryPlaceSound);
        SavedPos = null;
        RefreshPortableButtons();
    }

    private static void RefreshPortableButtons()
    {
        try
        {
            var role = PlayerControl.LocalPlayer?.Data?.Role as SentryRole;
            if (role == null) return;

            CustomButtonSingleton<SentryPortableCameraButton>.Instance.SetActive(true, role);
            CustomButtonSingleton<SentryPortableCameraSecondaryButton>.Instance.SetActive(true, role);
        }
        catch
        {
            // ignored
        }
    }
}