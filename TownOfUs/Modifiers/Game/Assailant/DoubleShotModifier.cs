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
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.DoubleShot.LoadAsset(),
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
        (int)OptionGroupSingleton<AssailantModifierOptions>.Instance.ImpDoubleShotAmount +
        (int)OptionGroupSingleton<AssailantModifierOptions>.Instance.NeutDoubleShotAmount;

    public override int CustomChance
    {
        get
        {
            var opts = OptionGroupSingleton<AssailantModifierOptions>.Instance;
            var impChance = (int)opts.ImpDoubleShotChance;
            var neutChance = (int)opts.NeutDoubleShotChance;
            if ((int)opts.ImpDoubleShotAmount > 0 && (int)opts.NeutDoubleShotAmount > 0)
            {
                return (impChance + neutChance) / 2;
            }

            if ((int)opts.ImpDoubleShotAmount > 0)
            {
                return impChance;
            }
            else if ((int)opts.NeutDoubleShotAmount > 0)
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
        var neutCount = (int)opts.NeutDoubleShotAmount.Value;
        var neutChance = Math.Clamp((int)opts.NeutDoubleShotChance.Value, 0, 100);
        var impCount = (int)opts.ImpDoubleShotAmount.Value;
        var impChance = Math.Clamp((int)opts.ImpDoubleShotChance.Value, 0, 100);
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