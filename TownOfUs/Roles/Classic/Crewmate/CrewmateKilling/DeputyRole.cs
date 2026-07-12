using MiraAPI.Hud;
using System.Collections;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TownOfUs.Roles.Crewmate;

public sealed class DeputyRole : CrewmateRole, ITouCrewRole, IWikiDiscoverable, IDoomable, IMeetingKiller
{
    public bool ShotThisMeeting;
    public void TriggerMeetingAnimation(PlayerControl source, PlayerControl target, PlayerVoteArea targetVoteArea,
        int associatedAnimKey = -1)
    {
        var revealMode = (DeputyReveal)OptionGroupSingleton<DeputyOptions>.Instance.RevealDeputyUponShot.Value;
        if (revealMode == DeputyReveal.NoReveal)
        {
            Coroutines.Start(CustomTouMurderRpcs.CoAnimateDeath(targetVoteArea, associatedAnimKey));
        }
        else
        {
            ShotThisMeeting = true;
            if (revealMode is DeputyReveal.AnnounceRole)
            {
                TriggerKillAnimation(HudManager.Instance.KillOverlay, target.Data, target.Data, targetVoteArea);
            }
            else
            {
                TriggerKillAnimation(HudManager.Instance.KillOverlay, source.Data, target.Data, targetVoteArea);
                source.AddModifier<DeputyRevealedModifier>();
            }
            Coroutines.Start(CoStopShot());
        }
    }
    public static void TriggerKillAnimation(KillOverlay overlay, NetworkedPlayerInfo killer,
        NetworkedPlayerInfo victim, PlayerVoteArea targetVoteArea)
    {
        OverlayKillAnimation killAnimation;
        if (!CustomTouMurderRpcs.StoredKillAnimations.HasAny())
        {
            OverlayKillAnimation[] self = overlay.KillAnims;
            CustomTouMurderRpcs.StoredKillAnimations = self.AddRangeToArray(overlay.CustomKillAnimations);
        }
        killAnimation = overlay.HorseWrangleAnims.Random()!;
        Coroutines.Start(CoShowAnim(overlay, (killAnimation as HorseWrangleOverlay)!,
            new KillOverlayInitData(killer, victim), targetVoteArea));
    }

    private static IEnumerator CoShowAnim(KillOverlay overlay, HorseWrangleOverlay killAnimation, KillOverlayInitData initData, PlayerVoteArea targetVoteArea)
    {
        HorseWrangleOverlay overlayKillAnimation = Instantiate(killAnimation, overlay.transform);
        var outfit = new VisualAppearance(PlayerControl.LocalPlayer.GetDefaultAppearance(),
            TownOfUsAppearances.Camouflage)
        {
            ColorId = PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId,
            HatId = "hat_NoHat",
            SkinId = "skin_None",
            VisorId = "visor_EmptyVisor",
            PlayerName = string.Empty,
            PetId = "pet_EmptyPet",
            NameVisible = false,
            PlayerMaterialColor = new Color(0, 0, 0, 0),
        };
        initData.victimOutfit = outfit;
        overlayKillAnimation.Initialize(initData);
        overlayKillAnimation.victimParts.transform.localScale = new Vector3(0, 0, 0);
        overlayKillAnimation.victimParts.transform.localPosition = new Vector3(10000, 10000, 0);
        foreach (var sprite in overlayKillAnimation.victimSprites)
        {
            sprite.transform.localScale = new Vector3(0, 0, 0);
            sprite.transform.localPosition = new Vector3(10000, 10000, 0);
        }
        overlayKillAnimation.victimParts.Destroy();
        var sheriffCloseup = overlayKillAnimation.transform.GetChild(2);
        foreach (var sprite in sheriffCloseup.GetComponentsInChildren<SpriteRenderer>())
        {
            sprite.gameObject.layer = LayerMask.NameToLayer("UI");
        }
        overlayKillAnimation.gameObject.SetActive(false);
        return CoShowOne(overlay, overlayKillAnimation, targetVoteArea);
    }
    private static IEnumerator CoShowOne(KillOverlay overlay, HorseWrangleOverlay anim, PlayerVoteArea targetVoteArea)
    {
        PlayerMaterial.SetColors(anim.killerParts.ColorId, anim.impostorForeground);
        PlayerMaterial.SetColors(anim.killerParts.ColorId, anim.impostorHand);
        TouAudio.PlaySound(TouAudio.DeputyReveal);
        overlay.background.enabled = true;
        yield return Effects.Wait(0.083333336f);
        overlay.background.enabled = false;
        var flameSprite = overlay.flameParent.transform.FindChild("BackgroundFlame").GetComponent<SpriteRenderer>();
        flameSprite.sprite = TouAssets.DeputyRevealBg.LoadAsset();
        flameSprite.transform.localPosition = new Vector3(0, -2f);
        if (KillOverlayPatch.material == null)
        {
            KillOverlayPatch.material = new Material(flameSprite.material);
        }
        flameSprite.material = KillOverlayPatch.material;
        overlay.flameParent.SetActive(true);
        overlay.flameParent.transform.localScale = new Vector3(1f, 0.3f, 1f);
        overlay.flameParent.transform.localEulerAngles = new Vector3(0f, 0f, 25f);
        yield return Effects.Wait(0.083333336f);
        overlay.flameParent.transform.localScale = new Vector3(1f, 0.5f, 1f);
        overlay.flameParent.transform.localEulerAngles = new Vector3(0f, 0f, -15f);
        yield return Effects.Wait(0.083333336f);
        overlay.flameParent.transform.localScale = new Vector3(1f, 1f, 1f);
        overlay.flameParent.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        anim.gameObject.SetActive(true);
        var sheriffCloseup = anim.transform.GetChild(2);
        var outfit = sheriffCloseup.GetChild(1).GetComponent<SpriteRenderer>();
        outfit.sprite = TouAssets.DeputyOutfit.LoadAsset();
        
        var clonedForeground = Instantiate(anim.impostorForeground, anim.impostorForeground.transform.parent);
        clonedForeground.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        var clonedHand = Instantiate(anim.impostorHand, anim.impostorHand.transform.parent);
        clonedHand.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        yield return new WaitForLerp(1.23f, new Action<float>(t =>
        {
            var adj = t / 200;
            sheriffCloseup.localPosition += new Vector3(adj, 0f, 0f);
        }));
        anim.gameObject.SetActive(false);
        yield return new WaitForLerp(0.16666667f, new Action<float>(t =>
        {
            overlay.flameParent.transform.localScale = new Vector3(1f, 1f - t, 1f);
        }));
        flameSprite.sprite = TouAssets.KillBG.LoadAsset();
        flameSprite.transform.localPosition = new Vector3(0, 0);
        overlay.flameParent.SetActive(false);
        
        Destroy(anim.gameObject);
        overlay.showOne = null;
        yield return CustomTouMurderRpcs.CoAnimateDeath(targetVoteArea, Random.RandomRangeInt(0, 2), true);
    }

    private IEnumerator CoStopShot()
    {
        yield return new WaitForSeconds(3);
        ShotThisMeeting = false;
    }
    private MeetingMenu meetingMenu;
    public override bool IsAffectedByComms => false;
 public PlayerControl? Killer { get; set; }
    public DoomableType DoomHintType => DoomableType.Relentless;
    public string LocaleKey => "Deputy";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Camp", "Camp"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}CampWikiDescription"),
                    TouCrewAssets.CampButtonSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Deputy;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;

    public bool IsPowerCrew =>
        Killer || ModifierUtils.GetActiveModifiers<DeputyCampedModifier>()
            .HasAny(); // Only stop end game checks if the deputy can actually kill someone

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Deputy.LoadAsset(), "TouMira.Role.Crewmate.Deputy", 1.45f),
        Icon = TouRoleIcons.Deputy,
        OptionsScreenshot = TouBanners.DeputyRoleBanner,
        IntroSound = TouAudio.DeputyIntroSound,
    };

    public static void OnRoundStart()
    {
        CustomButtonSingleton<CampButton>.Instance.Usable = true;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (Player.AmOwner)
        {
            meetingMenu = new MeetingMenu(
                this,
                ClickGuess,
                MeetingAbilityType.Click,
                LegacyAssets.IsLegacy ? LegacyAssets.ShootMeetingSprite : TouAssets.ShootMeetingSprite,
                null!,
                IsExempt)
            {
                Position = new Vector3(-0.40f, 0f, -3f)
            };
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        ShotThisMeeting = false;
        var meeting = MeetingHud.Instance;
        if (Player.AmOwner && meeting != null)
        {
            meetingMenu.GenButtons(meeting,
                Player.AmOwner && !Player.HasDied() && Killer != null && !Player.HasModifier<JailedModifier>());
        }
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        if (Player.AmOwner)
        {
            meetingMenu.HideButtons();
        }

        Clear();
    }

    public override void OnDeath(DeathReason reason)
    {
        RoleBehaviourStubs.OnDeath(this, reason);

        Clear();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        Clear();

        if (Player.AmOwner)
        {
            meetingMenu?.Dispose();
            meetingMenu = null!;
        }
    }

    public void Clear()
    {
        var player = ModifierUtils.GetPlayersWithModifier<DeputyCampedModifier>(x => x.Deputy.AmOwner).FirstOrDefault();

        if (player != null && Player.AmOwner)
        {
            player.RpcRemoveModifier<DeputyCampedModifier>();
        }
    }

    public void ClickGuess(PlayerVoteArea voteArea, MeetingHud __)
    {
        var target = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId).Object;
        var role = Player.GetRole<DeputyRole>()!;

        if (role.Killer == target && !target.HasModifier<InvulnerabilityModifier>())
        {
            // Even though Deputy doesn't use the nameplate normally, it should grab it anyways incase Deputy isn't meant to reveal themselves
            Player.RpcMeetingMurder(target, MeetingAnimation.RoleSpecific, CustomTouMurderRpcs.GetRandomMeetingAnim(DeathAnimType.Nameplate),
                causeOfDeath: "Deputy");
        }
        else
        {
            var title =
                $"<color=#{TownOfUsColors.Deputy.ToHtmlStringRGBA()}>{TouLocale.Get("TouRoleDeputyMessageTitle")}</color>";
            var msg = TouLocale.Get("TouRoleDeputyMissedShot");
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, msg, false, true);
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Deputy.ToTextColor()}{msg}</b></color>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Deputy.LoadAsset());
            notif1.AdjustNotification();
        }

        if (Player.AmOwner)
        {
            meetingMenu?.HideButtons();
        }

        Clear();
    }

    public bool IsExempt(PlayerVoteArea voteArea)
    {
        return voteArea?.TargetPlayerId == Player.PlayerId || Player.Data.IsDead || voteArea!.AmDead ||
               voteArea.GetPlayer()?.HasModifier<JailedModifier>() == true;
    }
}