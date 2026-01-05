using System.Collections;
using MiraAPI.Modifiers.Types;
using Reactor.Utilities;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers;

/// <summary>
/// Temporary immobilize-on-vent effect. Only enforces movement/position for the owning client.
/// </summary>
public sealed class TrappedOnVentModifier(Vector2 ventTopPos, float durationSeconds, int ventId) : TimedModifier
{
    public override string ModifierName => "Trapped";
    public override bool HideOnUi => true;
    public override bool AutoStart => true;
    public override bool RemoveOnComplete => true;
    public override float Duration => Mathf.Max(0.05f, durationSeconds);

    public Vector2 VentTopPos { get; } = ventTopPos;
    public int VentId { get; } = ventId;

    public override void OnActivate()
    {
        base.OnActivate();

        if (!Player.AmOwner)
        {
            return;
        }

        if (Player == null || Player.HasDied() || !Player.AmOwner)
            return;

        if (Player.inVent && Vent.currentVent != null)
        {
            Player.MyPhysics.RpcExitVent(Vent.currentVent.Id);
            Player.MyPhysics?.ExitAllVents();
        }

        if (Player == null || Player.HasDied() || !Player.AmOwner)
            return;

        Player.inVent = false;
        Vent.currentVent = null;
        Player.RpcSetPos(VentTopPos);
        Coroutines.Start(CoReassertPos(Player, VentTopPos));
    }

    private static IEnumerator CoReassertPos(PlayerControl player, Vector2 pos)
    {
        yield return null;
        if (player != null && player.AmOwner && !player.HasDied() && !MeetingHud.Instance)
        {
            player.RpcSetPos(pos);
        }

        yield return new WaitForSeconds(0.2f);
        if (player != null && player.AmOwner && !player.HasDied() && !MeetingHud.Instance)
        {
            player.RpcSetPos(pos);
        }
    }
}
