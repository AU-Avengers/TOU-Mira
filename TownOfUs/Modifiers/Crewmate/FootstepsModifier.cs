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
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class FootstepsModifier : BaseModifier
{
    public Dictionary<GameObject, SpriteRenderer>? _currentSteps;
    public bool AnonymousPrints;
    public bool PrintOnVents;
    public float PrintSize;
    public float PrintDuration;
    public float PrintInterval;
    public bool CheckDistance;
    private Vector3 _lastPos;
    private float _footstepInterval;
    public override string ModifierName => "Footsteps";
    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        _currentSteps = [];

        var opts = OptionGroupSingleton<InvestigatorOptions>.Instance;
        AnonymousPrints = opts.ShowAnonymousFootprints;
        PrintSize = opts.FootprintSize;
        PrintInterval = opts.FootprintInterval;
        CheckDistance = (PrintMode)opts.FootprintMode.Value is PrintMode.Distance;
        PrintDuration = opts.FootprintDuration;
        PrintOnVents = opts.ShowFootprintVent;
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
        var cantContinue = _currentSteps == null || Player.AmOwner ||
                           PlayerControl.LocalPlayer.GetModifiers<HypnotisedModifier>().Any(x => x.HysteriaActive) ||
                           !Player.IsVisibleToOthers();
        if (CheckDistance)
        {
            if (cantContinue ||
                Vector3.Distance(_lastPos, Player.transform.position) <
                PrintInterval)
            {
                return;
            }

            if (!PrintOnVents && ShipStatus.Instance?.AllVents
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
                    position = new Vector3(Player.transform.position.x, Player.transform.position.y, 2.5708f),
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
                                             (PrintSize / 10);

            _currentSteps!.Add(footstep, sprite);
            _lastPos = Player.transform.position;
            Coroutines.Start(FootstepDisappear(footstep, sprite, PrintDuration));
        }
        else
        {
            if (cantContinue || _footstepInterval <
                PrintInterval)
            {
                _footstepInterval += Time.fixedDeltaTime;
                return;
            }

            if (!PrintOnVents && ShipStatus.Instance?.AllVents
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
                                             (PrintSize / 10);

            _currentSteps!.Add(footstep, sprite);
            Coroutines.Start(FootstepDisappear(footstep, sprite, PrintDuration));

            _footstepInterval = 0;
        }
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

    public static IEnumerator FootstepDisappear(GameObject obj, SpriteRenderer rend, float duration)
    {
        yield return new WaitForSeconds(duration);
        yield return FootstepFadeout(obj, rend);
    }
}