using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Buttons.Neutral.NeutralOutlier;
using TownOfUs.Modules.Duelist;
using TownOfUs.Options;
using TownOfUs.Patches;
using TownOfUs.Roles.Neutral.NeutralOutlier;
using TownOfUs.Modifiers;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers.Neutral.NeutralOutlier;
public sealed class DuelModifier(byte opponentId, bool isDuelist, Vector2 returnPos)
    : DisabledModifier, IVisualAppearance
{
    public override string ModifierName => "In Duel";
    public override bool HideOnUi => true;
    public override bool AutoStart => false;
    public bool VisualPriority => true;
    public override bool CanBeInteractedWith => false;
    public override bool CanUseConsoles => true;
    public override bool CanOpenMap => true;

    public override bool IsConsideredAlive
    {
        get
        {
            var observer = PlayerControl.LocalPlayer;
            if (observer == null) return true;
            var isParticipant = observer.PlayerId == Player.PlayerId || observer.PlayerId == OpponentId;
            var isDeadSpectator = !isParticipant && TownOfUs.Modules.GameHistory.IsFullyDead(observer);
            return isParticipant || isDeadSpectator;
        }
    }


    [HideFromIl2Cpp] public bool IsHiddenFromList => true;

    public byte OpponentId { get; } = opponentId;
    public bool IsDuelist { get; } = isDuelist;
    public Vector2 ReturnPos { get; } = returnPos;

    private ArrowBehaviour? _arrow;
    private readonly HashSet<byte> _hidden = new();

    private static float Speed => OptionGroupSingleton<DuelistOptions>.Instance.DuelSpeed.Value;

    public VisualAppearance GetVisualAppearance()
    {
        var observer = PlayerControl.LocalPlayer;

        var shifted = GetShapeshiftAppearance();
        if (shifted != null)
        {
            shifted.Speed = Speed;
            return shifted;
        }

        var app = new VisualAppearance(Player.GetDefaultAppearance(), TownOfUsAppearances.PlayerNameOnly)
        {
            Speed = Speed,
        };
        if (observer != null && observer.PlayerId == OpponentId && IsDuelist)
        {
            ApplyCamo(app);
        }
        return app;
    }

    private VisualAppearance? GetShapeshiftAppearance()
    {
        foreach (var mod in Player.GetModifiers<BaseModifier>())
        {
            if (mod != this && mod is IVisualAppearance { VisualPriority: true } visual)
            {
                return visual.GetVisualAppearance();
            }
        }
        return null;
    }

    private static void ApplyCamo(VisualAppearance app)
    {
        app.HatId = "hat_NoHat";
        app.SkinId = "skin_None";
        app.VisorId = "visor_EmptyVisor";
        app.PetId = "pet_EmptyPet";
        app.PlayerName = string.Empty;
        app.NameVisible = false;
        app.PlayerMaterialColor = Color.grey;
    }

    private static void ApplyInvisible(VisualAppearance app)
    {
        app.HatId = string.Empty;
        app.SkinId = string.Empty;
        app.VisorId = string.Empty;
        app.PetId = string.Empty;
        app.PlayerName = string.Empty;
        app.NameVisible = false;
        app.RendererColor = Color.clear;
        app.NameColor = Color.clear;
        app.ColorBlindTextColor = Color.clear;
    }

    public override void OnActivate()
    {
        base.OnActivate();
        Player.RawSetAppearance(this);
        if (Player.AmOwner)
        {
            _arrow = MiscUtils.CreateArrow(Player.transform, DuelistRole.DuelistColor);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!Player.AmOwner && Player.Collider != null && Player.Collider.enabled)
        {
            Player.Collider.enabled = false;
        }

        var opponent = MiscUtils.PlayerById(OpponentId);
        if (opponent == null || opponent.Data == null || opponent.Data.Disconnected)
        {
            if (Player.AmOwner)
            {
                DuelManager.AbortDuel(Player);
            }
            return;
        }
        if (Player.TryGetModifier<IndirectAttackerModifier>(out var indirect))
        {
            indirect.ResetTimer();
        }
        else
        {
            Player.AddModifier<IndirectAttackerModifier>(true);
        }

        SetAnimHoldersActive(false);

        var mushroom = Object.FindObjectOfType<MushroomMixupSabotageSystem>();
        if ((mushroom && mushroom.IsActive) || GetShapeshiftAppearance() != null
            || Player.GetAppearanceType() == TownOfUsAppearances.Default)
        {
            Player.RawSetAppearance(this);
        }

        if (!Player.AmOwner)
        {
            return;
        }

        if (_arrow != null && opponent.transform != null)
        {
            _arrow.target = opponent.transform.position;
            _arrow.Update();
        }

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p.PlayerId == Player.PlayerId || p.PlayerId == OpponentId)
            {
                continue;
            }
            SetHidden(p, true);
            _hidden.Add(p.PlayerId);
        }
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        SetAnimHoldersActive(true);
        if (Player != null && Player.AmOwner)
        {
            TimeLordDuelGuardPatch.ClearLocalRewindHistory();

            if (IsDuelist)
            {
                var button = CustomButtonSingleton<DuelButton>.Instance;
                if (button != null)
                {
                    button.SetTimer(Mathf.Max(button.Cooldown, button.Timer));
                }
            }
        }

        if (Player != null && Player.TryGetModifier<IndirectAttackerModifier>(out var indirect))
        {
            Player.RemoveModifier(indirect);
        }

        if (_arrow != null && !_arrow.IsDestroyedOrNull())
        {
            _arrow.gameObject.Destroy();
            _arrow.Destroy();
        }
        _arrow = null;

        foreach (var id in _hidden)
        {
            var p = MiscUtils.PlayerById(id);
            if (p != null)
            {
                SetHidden(p, false);
            }
        }
        _hidden.Clear();

        if (Player != null && Player.Collider != null)
        {
            Player.Collider.enabled = true;
        }

        if (Player != null)
        {
            var remaining = GetShapeshiftAppearance();
            if (remaining != null)
            {
                Player.RawSetAppearance(remaining);
            }
            else
            {
                Player.ResetAppearance();
            }
        }
    }

    public override void OnDeath(DeathReason reason)
    {
    }

    [HideFromIl2Cpp]
    private static void SetHidden(PlayerControl p, bool hidden)
    {
        var a = hidden ? 0f : 1f;
        var cos = p.cosmetics;
        if (cos == null)
        {
            return;
        }

        if (cos.currentBodySprite != null && cos.currentBodySprite.BodySprite != null)
        {
            cos.currentBodySprite.BodySprite.color = cos.currentBodySprite.BodySprite.color.SetAlpha(a);
        }

        p.SetHatAndVisorAlpha(a);

        if (cos.skin != null && cos.skin.layer != null)
        {
            cos.skin.layer.color = cos.skin.layer.color.SetAlpha(a);
        }

        if (cos.currentPet != null)
        {
            foreach (var rend in cos.currentPet.renderers)
            {
                rend.color = rend.color.SetAlpha(a);
            }
            foreach (var shadow in cos.currentPet.shadows)
            {
                shadow.color = shadow.color.SetAlpha(a);
            }
        }

        cos.ToggleNameVisible(!hidden);

        SetAnimHolders(p, !hidden);
    }

    [HideFromIl2Cpp]
    private void SetAnimHoldersActive(bool active)
    {
        SetAnimHolders(Player, active);
    }

    [HideFromIl2Cpp]
    private static void SetAnimHolders(PlayerControl p, bool active)
    {
        var t = p == null ? null : p.transform;
        if (t == null || t.childCount < 3)
        {
            return;
        }

        var cosmeticsLayer = t.GetChild(2);
        if (cosmeticsLayer == null)
        {
            return;
        }

        for (var i = 0; i < cosmeticsLayer.childCount; i++)
        {
            var child = cosmeticsLayer.GetChild(i);
            if (child != null && child.name.StartsWith("A_") && child.gameObject.activeSelf != active)
            {
                child.gameObject.SetActive(active);
            }
        }
    }
}
