using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class FootstepsModifier : BaseModifier
{
    public Dictionary<GameObject, SpriteRenderer>? _currentSteps;
    public bool AnonymousPrints;
    private float _footstepInterval;
    public override string ModifierName => "Footsteps";
    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        _currentSteps = [];

        AnonymousPrints = OptionGroupSingleton<InvestigatorOptions>.Instance.ShowAnonymousFootprints;
    }

    public override void OnDeactivate()
    {
        if (_currentSteps == null)
        {
            return;
        }

        _currentSteps.ToList().ForEach(step => Coroutines.Start(FootstepFadeout(step.Key, step.Value)));
        _currentSteps.Clear();
    }

    public override void FixedUpdate()
    {
        if (_currentSteps == null || Player.AmOwner ||
            PlayerControl.LocalPlayer.GetModifiers<HypnotisedModifier>().Any(x => x.HysteriaActive) ||
            Player.GetModifiers<ConcealedModifier>().Any(x => !x.VisibleToOthers) ||
            (Player.TryGetModifier<DisabledModifier>(out var mod) && !mod.IsConsideredAlive) || _footstepInterval <
            OptionGroupSingleton<InvestigatorOptions>.Instance.FootprintInterval)
        {
            _footstepInterval += Time.fixedDeltaTime;
            return;
        }

        if (!OptionGroupSingleton<InvestigatorOptions>.Instance.ShowFootprintVent && ShipStatus.Instance?.AllVents
                .Any(vent => Vector2.Distance(vent.gameObject.transform.position, Player.GetTruePosition()) < 1f) ==
            true)
        {
            return;
        }

        var angle = Mathf.Atan2(Player.MyPhysics.Velocity.y, Player.MyPhysics.Velocity.x) * Mathf.Rad2Deg;

        var footstep = new GameObject("Footstep")
        {
            transform =
            {
                parent = ShipStatus.Instance?.transform,
                position = Player.transform.position,
                rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward)
            }
        };

        if (ModCompatibility.IsSubmerged())
        {
            footstep.AddSubmergedComponent("ElevatorMover");
        }

        var sprite = footstep.AddComponent<SpriteRenderer>();
        sprite.sprite = TouAssets.FootprintSprite.LoadAsset();
        sprite.color = (AnonymousPrints || HudManagerPatches.CommsSaboActive())
            ? new Color(0.2f, 0.2f, 0.2f, 1f)
            : Player.cosmetics.currentBodySprite.BodySprite.material.GetColor(ShaderID.BodyColor);
        footstep.layer = LayerMask.NameToLayer("Players");

        footstep.transform.localScale *= new Vector2(1.2f, 1f) *
                                         (OptionGroupSingleton<InvestigatorOptions>.Instance.FootprintSize / 10);

        _currentSteps.Add(footstep, sprite);
        Coroutines.Start(FootstepDisappear(footstep, sprite));

        _footstepInterval = 0;
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }

    public static IEnumerator FootstepFadeout(GameObject obj, SpriteRenderer rend)
    {
        yield return MiscUtils.FadeOut(rend, 0.0001f, 0.05f);
        obj.DestroyImmediate();
    }

    public static IEnumerator FootstepDisappear(GameObject obj, SpriteRenderer rend)
    {
        yield return new WaitForSeconds(OptionGroupSingleton<InvestigatorOptions>.Instance.FootprintDuration);
        yield return FootstepFadeout(obj, rend);
    }
}