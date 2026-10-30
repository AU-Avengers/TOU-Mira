using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Utilities;
using System.Collections;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Modules.Anims;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Patches;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using UnityEngine.UI;

// using Reactor.Utilities.Extensions;

namespace TownOfUs.Roles.Impostor;

public sealed class WraithRole(IntPtr cppPtr) : ImpostorGhostRole(cppPtr), ITownOfUsRole, IGhostRole, IWikiDiscoverable
{
    public bool ShouldUseLivingBody => false;

    public List<PlayerControl> GetAvailableGhosts(List<PlayerControl> basicGhosts, PlayerControl? exiled)
    {
        var wraithData = MiscUtils.GetAssignData((RoleTypes)RoleId.Get<WraithRole>());

        var livingImps = Helpers.GetAlivePlayers().Count(x => x.IsImpostorAligned() && x != exiled);
        if (CustomRoleUtils.GetActiveRoles().OfType<WraithRole>().Count() < wraithData.Count && livingImps > 0)
        {
            var isSkipped = wraithData.Chance < 100 && HashRandom.Next(101) > wraithData.Chance;

            if (!isSkipped)
            {
                var deadImps = basicGhosts.Where(x =>
                    (x.Data.IsDead && x != exiled && x.IsImpostorAligned() && !x.HasModifier<AllianceGameModifier>() &&
                    x.CanGetGhostRole() &&
                    x.Data.Role)).ToList();
                var validImps = new List<PlayerControl>();
                foreach (var dead in deadImps)
                {
                    if (!GameHistory.DeathHistory.Exists(x => x.Item1 == dead.PlayerId))
                    {
                        continue;
                    }
                    var deathInfo = GameHistory.DeathHistory.FirstOrDefault(x => x.Item1 == dead.PlayerId).Item2;
                    if (deathInfo is DeathReason.Exile)
                    {
                        continue;
                    }
                    validImps.Add(dead);
                }

                if (validImps.Count > 0)
                {
                    validImps.Shuffle();

                    var player = validImps.TakeFirst();

                    if (player != null)
                    {
                        player.RpcChangeRole(RoleId.Get<WraithRole>());
                        basicGhosts.Remove(player);
                    }
                }
            }
        }

        return basicGhosts;
    }
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
    public GameObject PlayerRobe { get; set; }
    public SpriteRenderer RobeSprite { get; set; }
    public SpriteRenderer HandSprites { get; set; }
    public bool ShowRobe { get; set; }

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
        Player.cosmetics.SetGhost();
        Player.MyPhysics.Animations.PlayGhostIdleAnimation();
    }
    public override bool IsDead => true;

    public void FadeUpdate()
    {
        if (!Caught && Setup)
        {
            Player.WraithGhostFade(out var color);
            if (RobeSprite)
            {
                RobeSprite.color = color;
            }
            if (HandSprites)
            {
                HandSprites.color = color;
            }
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

        if (!MeetingHud.Instance && PlayerRobe)
        {
            PlayerRobe.SetActive(ShowRobe);
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

    public string IdPart => "Wraith";

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorAfterlife;
    public string RoleLongDescription => MiraLocaleManager.Get("TownOfUsMira.Role.Wraith.TabDescription").Replace("<amount>", "idfk");

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

        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;

        var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x =>
            x.ParentId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);
        var fakePlayer = FakePlayer.FakePlayers.FirstOrDefault(x =>
            x.PlayerId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);

        ShowRobe = Player.AmOwner || PlayerControl.LocalPlayer.IsImpostorAligned() && !genOpt.FFAImpostorMode ||
                   (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body);

        PlayerRobe = AnimStore.SpawnAnimBody(Player, TouAssets.WraithRobe.LoadAsset(), false, -1.1f, -0.1f, 1.5f)!;
        RobeSprite = PlayerRobe.transform.GetChild(0).GetComponent<SpriteRenderer>();
        HandSprites = PlayerRobe.transform.GetChild(1).GetComponent<SpriteRenderer>();

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
        if (PlayerRobe)
        {
            PlayerRobe.Destroy();
        }
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