using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Buttons.Modifiers;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Assailant;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Assailant;

public class OverclockerModifier : TouGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Overclocker,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Overclocker.LoadAsset(),
            "TouMira.Modifier.Assailant.Overclocker", 1.45f));
    public override Color FreeplayFileColor => TownOfUsColors.Overclocker;

    public override string LocaleKey => "Overclocker";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override string IntroInfo => TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurb");

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Overclocker;
    public override ModifierFaction FactionType => ModifierFaction.AssailantUtility;

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int CustomAmount =>
        (int)OptionGroupSingleton<AssailantModifierOptions>.Instance.ImpOverclockerAmount +
        (int)OptionGroupSingleton<AssailantModifierOptions>.Instance.NeutOverclockerAmount;

    public override int CustomChance
    {
        get
        {
            var opts = OptionGroupSingleton<AssailantModifierOptions>.Instance;
            var impChance = (int)opts.ImpOverclockerChance;
            var neutChance = (int)opts.NeutOverclockerChance;
            if ((int)opts.ImpOverclockerAmount > 0 && (int)opts.NeutOverclockerAmount > 0)
            {
                return (impChance + neutChance) / 2;
            }

            if ((int)opts.ImpOverclockerAmount > 0)
            {
                return impChance;
            }
            else if ((int)opts.NeutOverclockerAmount > 0)
            {
                return neutChance;
            }

            return 0;
        }
    }

    public static int ImpostorOverclockerAttempts;
    public static int NeutralOverclockerAttempts;
    public static System.Random Rng;

    public override void BeforeModifierSpawns()
    {
        Rng = new System.Random();
        ImpostorOverclockerAttempts = 0;
        NeutralOverclockerAttempts = 0;
    }

    public static bool ModifierValidCheck(RoleBehaviour role, bool runChecks)
    {
        var opts = OptionGroupSingleton<AssailantModifierOptions>.Instance;
        var neutCount = (int)opts.NeutOverclockerAmount.Value;
        var neutChance = Math.Clamp((int)opts.NeutOverclockerChance.Value, 0, 100);
        var impCount = (int)opts.ImpOverclockerAmount.Value;
        var impChance = Math.Clamp((int)opts.ImpOverclockerChance.Value, 0, 100);
        if (role.Player.GetModifierComponent().HasModifier<AssassinModifier>(true)
            && !role.Player.GetModifierComponent().HasModifier<TouGameModifier>(true, x => x.PreventsOtherModifiers))
        {
            if ((!runChecks || NeutralOverclockerAttempts < neutCount) && neutChance != 0 && role is ITownOfUsRole
                {
                    RoleAlignment: RoleAlignment.NeutralKilling
                })
            {
                if (runChecks)
                {
                    NeutralOverclockerAttempts++;
                }

                if (Rng.Next(100) >= neutChance)
                {
                    return false;
                }

                return true;
            }

            if ((!runChecks || ImpostorOverclockerAttempts < impCount) && impChance != 0 &&
                role.TeamType == RoleTeamTypes.Impostor)
            {
                if (runChecks)
                {
                    ImpostorOverclockerAttempts++;
                }

                if (Rng.Next(100) >= impChance)
                {
                    return false;
                }

                return true;
            }
        }

        return false;
    }

    public override bool IsModifierValidOnPostCheck(RoleBehaviour role)
    {
        return ModifierValidCheck(role, false);
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return ModifierValidCheck(role, true);
    }

    public override int GetAssignmentChance() => 100;
    public override int GetAmountPerGame() => CustomAmount;

    public ChargeState CurrentState = ChargeState.Normal;
    public float OverclockMult;
    public float UnderclockMult;

    public override void OnActivate()
    {
        base.OnActivate();
        var opts = OptionGroupSingleton<OverclockerOptions>.Instance;
        OverclockMult = opts.OverclockMultiplier.Value;
        UnderclockMult = opts.UnderclockMultiplier.Value;
    }

    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        var button = CustomButtonSingleton<OverclockButton>.Instance;
        if (!button.ShowedFeedback && CurrentState is not ChargeState.Normal)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TouLocale.GetParsed("TouModifierOverclockerUnderclockMeetingNotif").Replace("<multi>", OptionGroupSingleton<OverclockerOptions>.Instance.UnderclockMultiplier.Value.ToString(TownOfUsPlugin.Culture))}</b>", Color.white,
                new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Overclocker.LoadAsset());
            notif1.AdjustNotification();
        }

        button.ShowedFeedback = false;
        switch (CurrentState)
        {
            case ChargeState.Overclocked or ChargeState.UnderclockedBegin:
                CurrentState = ChargeState.Underclocked;
                button.OverrideName(
                    TouLocale.GetParsed("TouModifierOverclockerUnderclocked", "Underclocked"));
                break;
            case ChargeState.Underclocked:
                CurrentState = ChargeState.Normal;
                button.OverrideName(
                    TouLocale.GetParsed("TouModifierOverclockerOverclock", "Overclock"));
                button.OverrideSprite(TouAssets.OverclockSprite.LoadAsset());
                break;
        }
    }

    public override void FixedUpdate()
    {
        if (!Player.AmOwner || CurrentState is ChargeState.Normal)
        {
            return;
        }

        if (CurrentState is ChargeState.Overclocked)
        {
            var value = OverclockMult - 1;

            foreach (var ability in CustomButtonManager.Buttons)
            {
                if (ability.EffectActive || ability.TimerPaused || ability is OverclockButton)
                {
                    continue;
                }

                ability.DecreaseTimer(Time.deltaTime * value);
            }

            Player.killTimer -= Time.deltaTime * value;
        }
        else
        {
            var value = UnderclockMult;

            foreach (var ability in CustomButtonManager.Buttons)
            {
                if (ability.EffectActive || ability.TimerPaused || ability is OverclockButton)
                {
                    continue;
                }

                if (ability.Timer > 0)
                {
                    ability.IncreaseTimer(Time.deltaTime * value);
                }
            }

            if (Player.killTimer > 0)
            {
                Player.killTimer *= Time.deltaTime * value;
            }
        }
    }
}

public enum ChargeState
{
    Normal,
    Overclocked,
    UnderclockedBegin,
    Underclocked
}