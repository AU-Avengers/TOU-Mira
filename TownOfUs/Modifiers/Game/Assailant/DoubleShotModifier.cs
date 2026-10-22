using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Options.Modifiers;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Assailant;

public class DoubleShotModifier : TouGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.DoubleShot,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.DoubleShot,
            "TouMira.Modifier.Assailant.DoubleShot", 1.45f));
    public override Color FreeplayFileColor => TownOfUsColors.Overclocker;
    public override string IdPart => "DoubleShot";
    public override string ModifierName => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}");
    public override string IntroInfo => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.IntroBlurb");

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.DoubleShot;
    public override ModifierFaction FactionType => ModifierFaction.AssailantUtility;

    public bool Used { get; set; }

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.WikiDescription");
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int CustomAmount =>
        OptionGroupSingleton<AssailantModifierOptions>.Instance.ImpDoubleShotAmount +
        OptionGroupSingleton<AssailantModifierOptions>.Instance.NeutDoubleShotAmount;

    public override int CustomChance
    {
        get
        {
            var opts = OptionGroupSingleton<AssailantModifierOptions>.Instance;
            var impChance = opts.ImpDoubleShotChance;
            var neutChance = opts.NeutDoubleShotChance;
            if (opts.ImpDoubleShotAmount > 0 && opts.NeutDoubleShotAmount > 0)
            {
                return (impChance + neutChance) / 2;
            }

            if (opts.ImpDoubleShotAmount > 0)
            {
                return impChance;
            }
            else if (opts.NeutDoubleShotAmount > 0)
            {
                return neutChance;
            }

            return 0;
        }
    }

    public static int ImpostorDoubleShotAttempts;
    public static int NeutralDoubleShotAttempts;
    public static System.Random Rng;
    public override void BeforeModifierSpawns()
    {
        Rng = new System.Random();
        ImpostorDoubleShotAttempts = 0;
        NeutralDoubleShotAttempts = 0;
    }

    public static bool ModifierValidCheck(RoleBehaviour role, bool runChecks)
    {
        var opts = OptionGroupSingleton<AssailantModifierOptions>.Instance;
        var neutCount = opts.NeutDoubleShotAmount;
        var neutChance = Math.Clamp(opts.NeutDoubleShotChance, 0, 100);
        var impCount = opts.ImpDoubleShotAmount;
        var impChance = Math.Clamp(opts.ImpDoubleShotChance, 0, 100);
        if (role.Player.GetModifierComponent().HasModifier<AssassinModifier>(true)
            && !role.Player.GetModifierComponent().HasModifier<TouGameModifier>(true, x => x.PreventsOtherModifiers))
        {
            if ((!runChecks || NeutralDoubleShotAttempts < neutCount) && neutChance != 0 && role is ITownOfUsRole { RoleAlignment: RoleAlignment.NeutralKilling })
            {
                if (runChecks)
                {
                    NeutralDoubleShotAttempts++;
                }
                if (Rng.Next(100) >= neutChance)
                {
                    return false;
                }

                return true;
            }

            if ((!runChecks || ImpostorDoubleShotAttempts < impCount) && impChance != 0 && role.TeamType == RoleTeamTypes.Impostor)
            {
                if (runChecks)
                {
                    ImpostorDoubleShotAttempts++;
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
}