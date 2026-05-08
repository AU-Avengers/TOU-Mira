using System.Collections;
using Il2CppInterop.Runtime;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using PowerTools;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Events;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modules.Anims;
using TownOfUs.Patches;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Impostor;

public sealed class AmbusherConcealedModifier(PlayerControl target) : ConcealedModifier, IVisualAppearance
{
    public override string ModifierName => "Swooped";
    public override bool HideOnUi => true;
    public override bool AutoStart => false;
    public bool VisualPriority => true;
    public override bool VisibleToOthers => false;
    public PlayerControl Target => target;

    public VisualAppearance GetVisualAppearance()
    {
        return new VisualAppearance(Player.GetDefaultModifiedAppearance(), TownOfUsAppearances.Swooper)
        {
            HatId = "hat_NoHat",
            SkinId = "skin_None",
            VisorId = "visor_EmptyVisor",
            PlayerName = string.Empty,
            PetId = "pet_EmptyPet",
            RendererColor = Color.clear,
            NameColor = Color.clear,
            ColorBlindTextColor = Color.clear
        };
    }

    public override void OnDeath(DeathReason reason)
    {
        Player.RemoveModifier(this);
    }

    public override void OnMeetingStart()
    {
        Player.RemoveModifier(this);
    }

    public override void OnActivate()
    {
        Player.RawSetAppearance(this);
        Player.cosmetics.ToggleNameVisible(false);
        Coroutines.Start(CoSetBodyReportable());
    }

    private IEnumerator CoSetBodyReportable()
    {
        var ogPos = Player.transform.position;
        yield return new WaitForSeconds(0.01f);
        if (!Target.HasDied() || MeetingHud.Instance || Player.HasDied())
        {
            Player.RemoveModifier(this);
            yield break;
        }

        var bodyId = Target.PlayerId;
        var waitDelegate =
            DelegateSupport.ConvertDelegate<Il2CppSystem.Func<bool>>(() => Helpers.GetBodyById(bodyId) != null);
        yield return new WaitUntil(waitDelegate);
        var body = Helpers.GetBodyById(bodyId);

        if (body != null)
        {
            DeathHandlerModifier.UpdateDeathHandlerImmediate(Target, TouLocale.Get("DiedToAmbusherAmbush"),
                DeathEventHandlers.CurrentRound,
                DeathHandlerOverride.SetTrue,
                TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", Player.Data.PlayerName),
                lockInfo: DeathHandlerOverride.SetTrue);

            var bodyPos = body.transform.position;
            if (!MeetingHud.Instance && Player.AmOwner)
            {
                Player.moveable = false;
                Player.MyPhysics.ResetMoveState();
                Player.NetTransform.SetPaused(true);
                bodyPos.y += 0.175f;
                bodyPos.z = bodyPos.y / 1000f;
                Player.RpcSetPos(bodyPos);
            }

            // Hide real player
            Player.Visible = false;
            foreach (var shield in Player.GetModifiers<BaseShieldModifier>())
            {
                shield.IsVisible = false;
                shield.SetVisible();
            }

            if (Player.HasModifier<FirstDeadShield>())
            {
                Player.GetModifier<FirstDeadShield>()!.IsVisible = false;
                Player.GetModifier<FirstDeadShield>()!.SetVisible();
            }

            var bodySprite = body.transform.GetChild(1).gameObject;
            var ambushAnim = AnimStore.SpawnFliplessAnimBody(Player, TouAssets.AmbushPrefab.LoadAsset());
            ambushAnim.name = $"{Player.Data.PlayerName} Ambush Animation";
            ambushAnim.SetActive(false);

            yield return new WaitForSeconds(1.3f);

            if (!Target.HasDied() || MeetingHud.Instance || Player.HasDied())
            {
                ambushAnim.gameObject.Destroy();
                Player.Visible = true;

                foreach (var shield in Player.GetModifiers<BaseShieldModifier>())
                {
                    shield.IsVisible = true;
                    shield.SetVisible();
                }

                if (Player.HasModifier<FirstDeadShield>())
                {
                    Player.GetModifier<FirstDeadShield>()!.IsVisible = true;
                    Player.GetModifier<FirstDeadShield>()!.SetVisible();
                }

                if (!Player.AmOwner)
                {
                    Player.RemoveModifier(this);
                    yield break;
                }

                Player.moveable = true;
                Player.NetTransform.SetPaused(false);
                Player.RemoveModifier(this);
                yield break;
            }

            ambushAnim.SetActive(true);
            var spriteAnim = ambushAnim.GetComponent<SpriteAnim>();
            var animationRend = ambushAnim.transform.GetChild(0).GetComponent<SpriteRenderer>();
            animationRend.material = bodySprite.GetComponent<SpriteRenderer>().material;
            body.gameObject.transform.position = new Vector3(bodyPos.x, bodyPos.y, bodyPos.z + 1000f);

            if (Player.HasModifier<GiantModifier>())
            {
                ambushAnim.transform.localScale *= 0.7f;
            }
            else if (Player.HasModifier<MiniModifier>())
            {
                ambushAnim.transform.localScale /= 0.7f;
            }

            if (Target.HasModifier<MiniModifier>())
            {
                ambushAnim.transform.localScale *= 0.7f;
            }
            else if (Target.HasModifier<GiantModifier>())
            {
                ambushAnim.transform.localScale /= 0.7f;
            }

            yield return new WaitForSeconds(spriteAnim.m_defaultAnim.length);

            if (!Target.HasDied() || MeetingHud.Instance || Player.HasDied())
            {
                ambushAnim.gameObject.Destroy();
                Player.Visible = true;

                foreach (var shield in Player.GetModifiers<BaseShieldModifier>())
                {
                    shield.IsVisible = true;
                    shield.SetVisible();
                }

                if (Player.HasModifier<FirstDeadShield>())
                {
                    Player.GetModifier<FirstDeadShield>()!.IsVisible = true;
                    Player.GetModifier<FirstDeadShield>()!.SetVisible();
                }

                if (!MeetingHud.Instance && !Player.HasDied())
                {
                    Player.transform.position = ogPos;
                    Player.NetTransform.SnapTo(ogPos);
                }

                if (!Player.AmOwner)
                {
                    Player.RemoveModifier(this);
                    yield break;
                }

                Player.moveable = true;
                Player.NetTransform.SetPaused(false);
                Player.RemoveModifier(this);
                yield break;
            }

            ambushAnim.gameObject.Destroy();

            if (!MeetingHud.Instance && Target.HasDied())
            {
                if (Player.AmOwner)
                {
                    Player.RpcSetPos(ogPos);
                }
                var targetPos = ogPos + new Vector3(-0.05f, 0.175f, 0f);
                targetPos.z = targetPos.y / 1000f;
                body.transform.position = (Player.Collider.bounds.center - targetPos) + targetPos;
            }

            Player.Visible = true;

            foreach (var shield in Player.GetModifiers<BaseShieldModifier>())
            {
                shield.IsVisible = true;
                shield.SetVisible();
            }

            if (Player.HasModifier<FirstDeadShield>())
            {
                Player.GetModifier<FirstDeadShield>()!.IsVisible = true;
                Player.GetModifier<FirstDeadShield>()!.SetVisible();
            }

            if (!Player.AmOwner)
            {
                Player.RemoveModifier(this);
                yield break;
            }

            Player.moveable = true;
            Player.NetTransform.SetPaused(false);
            Player.RemoveModifier(this);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (VanillaSystemCheckPatches.ShroomSabotageSystem && VanillaSystemCheckPatches.ShroomSabotageSystem.IsActive)
        {
            Player.RawSetAppearance(this);
            Player.cosmetics.ToggleNameVisible(false);
        }
    }

    public override void OnDeactivate()
    {
        var ambushAnim = GameObject.Find($"{Player.Data.PlayerName} Ambush Animation");

        if (ambushAnim != null)
        {
            ambushAnim.gameObject.Destroy();

            Player.Visible = true;

            foreach (var shield in Player.GetModifiers<BaseShieldModifier>())
            {
                shield.IsVisible = true;
                shield.SetVisible();
            }

            if (Player.HasModifier<FirstDeadShield>())
            {
                Player.GetModifier<FirstDeadShield>()!.IsVisible = true;
                Player.GetModifier<FirstDeadShield>()!.SetVisible();
            }
        }

        Player.ResetAppearance();
        Player.cosmetics.ToggleNameVisible(true);

        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            Player.cosmetics.ToggleNameVisible(false);
        }

        if (VanillaSystemCheckPatches.ShroomSabotageSystem && VanillaSystemCheckPatches.ShroomSabotageSystem.IsActive)
        {
            MushroomMixUp(VanillaSystemCheckPatches.ShroomSabotageSystem, Player);
        }
    }

    public static void MushroomMixUp(MushroomMixupSabotageSystem instance, PlayerControl player)
    {
        if (player != null && !player.Data.IsDead && instance.currentMixups.ContainsKey(player.PlayerId))
        {
            var condensedOutfit = instance.currentMixups[player.PlayerId];
            var playerOutfit = instance.ConvertToPlayerOutfit(condensedOutfit);
            playerOutfit.NamePlateId = player.Data.DefaultOutfit.NamePlateId;

            player.MixUpOutfit(playerOutfit);
        }
    }
}