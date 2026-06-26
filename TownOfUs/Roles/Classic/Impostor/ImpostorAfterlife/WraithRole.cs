using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Utilities;
using System.Collections;
using MiraAPI.GameOptions;
using TownOfUs.Modifiers;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Patches;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using UnityEngine.UI;

// using Reactor.Utilities.Extensions;

namespace TownOfUs.Roles.Impostor;

public sealed class WraithRole(IntPtr cppPtr) : ImpostorGhostRole(cppPtr), ITownOfUsRole, IGhostRole, IWikiDiscoverable
{

    public bool Setup { get; set; }
    public bool Caught { get; set; }
    public bool Faded { get; set; }

    public bool CanBeClicked
    {
        get
        {
            return ScaresAvailable <= ScaresLeftToClick;
        }
        set
        {
            // Left Alone
        }
    }

    public int ScaresAvailable;
    public int ScaresLeftToClick;
    public bool GhostActive => Setup && !Caught;
    public List<byte> UniqueClicks;
    public bool ClicksQuotaMet => UniqueClicks.Count >= (int)OptionGroupSingleton<WraithOptions>.Instance.ScaresRequiredToClick.Value;

    public bool CanCatch()
    {
        if (PlayerControl.LocalPlayer.IsImpostorAligned())
        {
            return false;
        }

        RpcAddUniqueClick(PlayerControl.LocalPlayer, Player);
        return ClicksQuotaMet;
    }

    public static void RpcAddUniqueClick(PlayerControl sender, PlayerControl wraith)
    {
        if (wraith.Data.Role is not WraithRole role)
        {
            return;
        }

        if (!role.UniqueClicks.Contains(sender.PlayerId))
        {
            role.UniqueClicks.Add(sender.PlayerId);
        }
    }

    public void Spawn()
    {
        Setup = true;

        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            Player.SetCamouflage(false);
        }

        Player.gameObject.layer = LayerMask.NameToLayer("Players");

        Player.gameObject.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
        Player.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() => Player.OnClick()));
        Player.gameObject.GetComponent<BoxCollider2D>().enabled = true;

        if (Player.AmOwner)
        {
            Player.SpawnAtRandomVent();
            Player.MyPhysics.ResetMoveState();

            HudManager.Instance.SetHudActive(false);
            HudManager.Instance.SetHudActive(true);
            HudManager.Instance.AbilityButton.SetDisabled();
            HudManagerPatches.ResetZoom();
        }
    }

    public void FadeUpdate()
    {
        if (!Caught && Setup)
        {
            Player.GhostFade();
            Faded = true;
        }
        else if (Faded)
        {
            Player.ResetAppearance();
            Player.cosmetics.ToggleNameVisible(true);

            Player.cosmetics.currentBodySprite.BodySprite.color = Color.white;
            Player.gameObject.layer = LayerMask.NameToLayer("Ghost");
            Player.MyPhysics.ResetMoveState();

            Faded = false;
        }
    }

    public void FixedUpdate()
    {
        if (!Player || Player.Data.Role is not WraithRole || MeetingHud.Instance)
        {
            return;
        }

        FadeUpdate();
    }

    public void Clicked()
    {
        Caught = true;
        Player.Exiled();

        if (Player.AmOwner)
        {
            HudManager.Instance.AbilityButton.SetEnabled();
        }
    }

    public string LocaleKey => "Wraith";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorAfterlife;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Wraith,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        HideSettings = false,
        ShowInFreeplay = true
    };

    public override void UseAbility()
    {
        if (GhostActive)
        {
            return;
        }

        base.UseAbility();
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        var opts = OptionGroupSingleton<WraithOptions>.Instance;
        ScaresAvailable = (int)opts.ScareUses.Value;
        ScaresLeftToClick = (int)opts.ScaresRequiredToClick.Value;
        if (!Player.HasModifier<BasicGhostModifier>())
        {
            Player.AddModifier<BasicGhostModifier>();
        }
        if (TutorialManager.InstanceExists)
        {
            Setup = true;

            if (HudManagerPatches.CamouflageCommsEnabled)
            {
                Player.SetCamouflage(false);
            }

            Coroutines.Start(SetTutorialCollider(Player));

            if (Player.AmOwner)
            {
                Player.MyPhysics.ResetMoveState();

                HudManager.Instance.SetHudActive(false);
                HudManager.Instance.SetHudActive(true);
                HudManager.Instance.AbilityButton.SetDisabled();
                HudManagerPatches.ResetZoom();
                // var dangerMeter = GameManagerCreator.Instance.HideAndSeekManagerPrefab.LogicDangerLevel.dangerMeter;
                // ImpostorMeter = UnityEngine.Object.Instantiate(dangerMeter, HudManager.Instance.transform.parent);
            }
        }
    }
    private static IEnumerator SetTutorialCollider(PlayerControl player)
    {
        yield return new WaitForSeconds(0.01f);
        player.gameObject.layer = LayerMask.NameToLayer("Players");

        player.gameObject.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
        player.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() => player.OnClick()));
        player.gameObject.GetComponent<BoxCollider2D>().enabled = true;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        if (TutorialManager.InstanceExists)
        {
            Player.ResetAppearance();
            Player.cosmetics.ToggleNameVisible(true);

            Player.cosmetics.currentBodySprite.BodySprite.color = Color.white;
            Player.gameObject.layer = LayerMask.NameToLayer("Ghost");
            Player.MyPhysics.ResetMoveState();

            Faded = false;
        }
    }


    public override bool CanUse(IUsable usable)
    {
        var validUsable = usable.TryCast<Console>() ||
                          usable.TryCast<DoorConsole>() ||
                          usable.TryCast<OpenDoorConsole>() ||
                          usable.TryCast<DeconControl>() ||
                          usable.TryCast<PlatformConsole>() ||
                          usable.TryCast<Ladder>() ||
                          usable.TryCast<ZiplineConsole>();

        return GhostActive && validUsable;
    }
}