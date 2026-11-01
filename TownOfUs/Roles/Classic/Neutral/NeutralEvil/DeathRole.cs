using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class DeathRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, IUnguessable
{
    public string GetAmountText(int amount, int chance, string source)
    {
        return $"Transforms from {AppearAs.GetRoleName()} ({source})";
    }
    public bool? ForceShowRoleOnWiki => MiscUtils.GetPotentialRoles().Contains(AppearAs) ? true : null;
    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner)
        {
            return;
        }
        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        orCreateTask.Text = $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralEvilTaskHeader")}</color>";
        orCreateTask.name = "NeutralRoleText";
    }

    public bool Announced { get; set; }
    public bool IsDraftable => false;
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string YouAreText => MiraLocaleManager.Get("YouAre");
    public string YouWereText => MiraLocaleManager.Get("YouWere");
    public string IdPart => "Death";
    public string RoleMedDescriptionLocale => $"TownOfUsMira.Role.{IdPart}.TabDescription";

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Death;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;
    public bool HasImpostorVision => true;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Death, "TouMira.Role.Neutral.Death", 1.45f),
        HideSettings = true,
        CanModifyChance = false,
        DefaultChance = 0,
        DefaultRoleCount = 0,
        MaxRoleCount = 0,
        IntroSound = TouAudio.PhantomIntroSound,
        Icon = TouRoleIcons.Death,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool WinConditionMet()
    {
        if (Player.HasDied() || MiscUtils.KillersAliveCount == 0)
        {
            return false;
        }

        var result = MiscUtils.GetImpactfulLivingPlayers().Count <= 2 && MiscUtils.KillersAliveCount == 1;

        return result;
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = new StringBuilder();
        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"{RoleColor.ToTextColor()}{YouAreText}<b> {this.GetRoleName()},‎ ‎ ‎ \n<size=80%>{this.GetRoleIntroBlurb()}</size></b></color>");
        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"<size=60%>{MiraLocaleManager.Get("Alignment")}: <b>{MiscUtils.GetParsedRoleAlignment(RoleAlignment, true)}</b></size>");
        stringB.Append("<size=70%>");
        stringB.AppendLine(TownOfUsPlugin.Culture, $"{this.GetRoleLongDescription()}");

        return stringB;
    }

    public bool IsGuessable => false;
    public RoleBehaviour AppearAs => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SoulCollectorRole>());
    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (!Player.HasModifier<InvulnerabilityModifier>())
        {
            Player.AddModifier<InvulnerabilityModifier>(true, true, false);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
        Player.TryRemoveModifier<InvulnerabilityModifier>();
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        if (Announced)
        {
            return;
        }
        Announced = true;
        var title = $"<color=#{TownOfUsColors.Plaguebearer.ToHtmlStringRGBA()}>{MiraLocaleManager.Get("TownOfUsMira.Role.PestilenceMessageTitle")}</color>";
        var msg = MiraLocaleManager.Get("TownOfUsMira.Role.PestilenceAnnounceMessage");

        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>{msg.Replace("<role>", $"{TownOfUsColors.Death.ToTextColor()}{this.GetRoleName()}</color>")}</b>", Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Pestilence);

        notif1.AdjustNotification();

        MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, msg.Replace("<role>", MiscUtils.GetReliableHyperlinkText(this)), false, true);
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }
}