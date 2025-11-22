using AmongUs.Data;
using AmongUs.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.HnsCrewmate;

public sealed class HnsMysticRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public string LocaleKey => "Mystic";
    public string RoleName => TouLocale.Get($"HnsRole{LocaleKey}");
    public string RoleDescription => "...";
    public string RoleLongDescription => TouLocale.GetParsed($"HnsRole{LocaleKey}TabDescription");
    public string RoleHintText => TouLocale.GetParsed($"HnsRole{LocaleKey}TabHint");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"HnsRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Mystic;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateHider;

    public CustomRoleConfiguration Configuration => new(this)
    {
        /*HideSettings = MiscUtils.CurrentGamemode() is not TouGamemode.HideAndSeek,*/
        FreeplayFolder = "Hide n Seek",
        Icon = TouRoleIcons.Mystic,
        RoleHintType = RoleHintType.TaskHint
    };

    public override void AppendTaskHint(Il2CppSystem.Text.StringBuilder taskStringBuilder)
    {
        taskStringBuilder.AppendLine($"\n{RoleHintText}\n{RoleLongDescription}");
    }

    public bool IsHiddenFromList => MiscUtils.CurrentGamemode() is not TouGamemode.HideAndSeek;

    public bool CanSpawnOnCurrentMode() => MiscUtils.CurrentGamemode() is TouGamemode.HideAndSeek;
    public override bool IsAffectedByComms => false;

    private Vent currentTarget;

    private float cooldownSecondsRemaining;

    private float inVentTimeRemaining;

    private int usesRemaining = 1;

    private bool isExitVentQueued;

    public void Awake()
    {
        var engineer = RoleManager.Instance.GetRole(RoleTypes.Engineer).Cast<EngineerRole>();
        Ability = engineer.Ability;
    }

    public override void UseAbility()
    {
        PlayerControl localPlayer = PlayerControl.LocalPlayer;
        if (!currentTarget)
        {
            return;
        }
        if (base.isActiveAndEnabled && !IsCoolingDown)
        {
            inVentTimeRemaining = GetVentTime();
            bool flag3 = localPlayer.inVent && !localPlayer.walkingToVent;
            if (!PlayerControl.LocalPlayer.inVent)
            {
                if (GameManager.Instance.LogicOptions is LogicOptionsHnS && flag3)
                {
                    usesRemaining--;
                    HudManager.Instance.AbilityButton.SetUsesRemaining(usesRemaining);
                }

                PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(currentTarget.Id);
                currentTarget.SetButtons(true);
            }
            else
            {
                Vent.currentVent.SetButtons(false);
                PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                HudManager.Instance.AbilityButton.cooldownTimerText.gameObject.SetActive(false);
            }

            if (!flag3)
            {
                if (GameManager.Instance.IsHideAndSeek())
                {
                    DataManager.Player.Stats.IncrementStat(StatID.HideAndSeek_TimesVented);
                    return;
                }

                DataManager.Player.Stats.IncrementStat(StatID.Role_Engineer_Vents);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!Player || !Player.AmOwner || Player.Data.Role is not HnsMysticRole || MeetingHud.Instance)
        {
            return;
        }

        if (cooldownSecondsRemaining > 0f)
        {
            cooldownSecondsRemaining -= Time.deltaTime;
            HudManager.Instance.AbilityButton.SetCoolDown(cooldownSecondsRemaining, GetCooldown());
        }

        if (Player.inVent)
        {
            float ventTime = GetVentTime();
            if (ventTime > 0f)
            {
                inVentTimeRemaining -= Time.deltaTime;
                HudManager.Instance.AbilityButton.SetFillUp(inVentTimeRemaining, ventTime);
            }

            if ((inVentTimeRemaining < 0f || base.CommsSabotaged) && base.isActiveAndEnabled && currentTarget &&
                !isExitVentQueued)
            {
                Vent.currentVent.SetButtons(false);
                PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                isExitVentQueued = true;
                if (GameManager.Instance.IsHideAndSeek())
                {
                    usesRemaining--;
                    HudManager.Instance.AbilityButton.SetUsesRemaining(usesRemaining);
                }
                HudManager.Instance.AbilityButton.cooldownTimerText.gameObject.SetActive(false);
            }
        }
        else
        {
            isExitVentQueued = false;
        }
    }

    public override void SetCooldown()
    {
        cooldownSecondsRemaining = GetCooldown();
        HudManager.Instance.AbilityButton.SetCoolDown(cooldownSecondsRemaining, GetCooldown());
    }

    private bool IsCoolingDown
    {
        get { return cooldownSecondsRemaining > 0f; }
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (GameManager.Instance.LogicOptions is LogicOptionsHnS logicOptionsHnS)
        {
            usesRemaining = logicOptionsHnS.GetCrewmateVentUses();
            HudManager.Instance.AbilityButton.OverrideText(
                DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.HideActionButton));
            HudManager.Instance.AbilityButton.SetUsesRemaining(usesRemaining);
        }
    }

    private static float GetCooldown()
    {
        return GameManager.Instance.LogicOptions.GetEngineerCooldown();
    }

    private static float GetVentTime()
    {
        return GameManager.Instance.LogicOptions.GetEngineerInVentTime();
    }

    public override bool CanUse(IUsable console)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(console, Player))
        {
            return false;
        }
        var console2 = console.TryCast<Console>()!;
        if (console2 == null)
        {
            return false;
        }

        if (console2.TryCast<Vent>())
        {
            return (!IsAffectedByComms || Player.inVent) && usesRemaining > 0;
        }

        return console2.TryCast<ZiplineConsole>() || console2.TryCast<Ladder>() || console2.TryCast<PlatformConsole>() ||
               console2.TryCast<DoorConsole>();
    }

    public override void SetUsableTarget(IUsable target)
    {
        var newTarget = GetTarget();
        if (newTarget != currentTarget)
        {
            currentTarget?.SetOutline(false, false);
        }

        currentTarget = newTarget!;
        currentTarget?.SetOutline(true, true, Palette.CrewmateBlue);
    }
    private static readonly ContactFilter2D Filter = Helpers.CreateFilter(Constants.Usables);
    public Vent? GetTarget()
    {
        var vent = PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(GetAbilityDistance() / 4, Filter);
        if (vent == null)
        {
            vent = PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(GetAbilityDistance() / 3, Filter);
        }

        if (vent == null)
        {
            vent = PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(GetAbilityDistance() / 2, Filter);
        }

        if (vent == null)
        {
            vent = PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(GetAbilityDistance(), Filter);
        }

        if (vent != null && PlayerControl.LocalPlayer.CanUseVent(vent))
        {
            HudManager.Instance.AbilityButton.SetEnabled();
            return vent;
        }
        HudManager.Instance.AbilityButton.SetDisabled();

        return null;
    }
}
