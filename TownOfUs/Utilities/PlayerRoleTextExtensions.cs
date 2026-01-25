using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Utilities;

public static class PlayerRoleTextExtensions
{
    private static Func<GuardianAngelTargetModifier, bool> GuardianAngelPredicate { get; } =
        gaModifier => gaModifier.OwnerId == PlayerControl.LocalPlayer.PlayerId;
    
    private static Func<MedicShieldModifier, bool> MedicShieldPredicate { get; } =
        msModifier => msModifier.Medic.AmOwner;

    private static Func<MagicMirrorModifier, bool> MagicMirrorPredicate { get; } =
        mmModifier => mmModifier.Mirrorcaster.AmOwner;

    private static Func<ClericBarrierModifier, bool> ClericBarrierPredicate { get; } =
        cbModifier => cbModifier.Cleric.AmOwner;
    
    private static Func<WardenFortifiedModifier, bool> WardenFortifiedPredicate { get; } =
        wfModifier => wfModifier.Warden.AmOwner;

    private static Func<PoliticianCampaignedModifier, bool> PoliticianCampaignedPredicate { get; } =
        pcModifier => pcModifier.Politician.AmOwner && pcModifier.Politician.IsCrewmate();
    private static Func<MercenaryBribedModifier, bool> MercenaryBribedPredicate { get; } =
        mbModifier => mbModifier.Mercenary.AmOwner;
    private static Func<ExecutionerTargetModifier, bool> ExecutionerTargetPredicate { get; } =
        etModifier => etModifier.OwnerId == PlayerControl.LocalPlayer.PlayerId;
    private static Func<HunterStalkedModifier, bool> HunterStalkedPredicate { get; } =
        hsModifier => hsModifier.Hunter.AmOwner;
    private static Func<RevealModifier, bool> RevealVisibleRolePredicate { get; } =
        revealModifier => revealModifier.Visible && revealModifier.RevealRole;

    private static Func<PlaguebearerInfectedModifier, bool> PbPredicate1 { get; } =
        pbModifier => pbModifier.PlagueBearerId == PlayerControl.LocalPlayer.PlayerId 
                      && pbModifier.PlagueBearerId != pbModifier.Player.PlayerId;

    private static Func<PlaguebearerInfectedModifier, bool> PbPredicate2 { get; } =
        pbModifier => pbModifier.PlagueBearerId != pbModifier.Player.PlayerId;

    private static Func<ArsonistDousedModifier, bool> ArsonistDousedPredicate { get; } =
        adModifier => adModifier.ArsonistId == PlayerControl.LocalPlayer.PlayerId;

    private static Func<BlackmailedModifier, bool> BlackmailedPredicate { get; } =
        bmModifier => bmModifier.BlackMailerId == PlayerControl.LocalPlayer.PlayerId;
    
    private static Func<HypnotisedModifier, bool> HypnotisedPredicate { get; } =
        hModifier => hModifier.Hypnotist.AmOwner;

    private static Func<SpellslingerHexedModifier, bool> SpellslingerHexedPredicate { get; } =
        shModifier => shModifier.Spellslinger.AmOwner;

    public static Color UpdateTargetColor(this Color color, PlayerControl player, bool hidden = false)
    {
        if (player.HasModifier<EclipsalBlindModifier>() && PlayerControl.LocalPlayer.IsImpostor())
        {
            color = Color.black;
        }

        if (player.HasModifier<GrenadierFlashModifier>() && !player.IsImpostor() &&
            PlayerControl.LocalPlayer.IsImpostor())
        {
            color = Color.black;
        }

        if (player.HasModifier<SeerGoodRevealModifier>() && PlayerControl.LocalPlayer.IsRole<SeerRole>())
        {
            color = Color.green;
        }
        else if (player.HasModifier<SeerEvilRevealModifier>() && PlayerControl.LocalPlayer.IsRole<SeerRole>())
        {
            color = Color.red;
        }

        if (player.HasModifier(PoliticianCampaignedPredicate) &&
            (PlayerControl.LocalPlayer.IsRole<PoliticianRole>() || PlayerControl.LocalPlayer.IsRole<MayorRole>()))
        {
            color = Color.cyan;
        }

        if (player.HasModifier(MercenaryBribedPredicate) &&
            PlayerControl.LocalPlayer.IsRole<MercenaryRole>())
        {
            color = Color.green;

            if (player.Is(RoleAlignment.NeutralEvil) || player.IsRole<AmnesiacRole>() || player.IsRole<MercenaryRole>())
            {
                color = Color.red;
            }
        }

        return color;
    }

    public static string UpdateTargetSymbols(this string name, PlayerControl player, bool hidden = false)
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        if ((player.HasModifier(ExecutionerTargetPredicate) &&
             PlayerControl.LocalPlayer.IsRole<ExecutionerRole>())
            || (player.HasModifier<ExecutionerTargetModifier>() && PlayerControl.LocalPlayer.HasDied() &&
                genOpt.TheDeadKnow && !hidden))
        {
            name += "<color=#643B1F> X</color>";
        }

        if (player.HasModifier<InquisitorHereticModifier>() && PlayerControl.LocalPlayer.HasDied() &&
            (genOpt.TheDeadKnow || PlayerControl.LocalPlayer.GetRoleWhenAlive() is InquisitorRole) && !hidden)
        {
            name += "<color=#D94291> $</color>";
        }

        if (PlayerControl.LocalPlayer.Data.Role is HunterRole &&
            player.HasModifier(HunterStalkedPredicate))
        {
            name += "<color=#29AB87> &</color>";
        }

        if (PlayerControl.LocalPlayer.Data.Role is HunterRole hunter && hunter.CaughtPlayers.Contains(player))
        {
            name += "<color=#21453B> &</color>";
        }

        return name;
    }

    public static string UpdateProtectionSymbols(this string name, PlayerControl player, bool hidden = false)
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var isDead = PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !hidden;

        if (player.Data != null && !player.Data.Disconnected &&
            ((player.HasModifier(GuardianAngelPredicate) &&
              PlayerControl.LocalPlayer.IsRole<FairyRole>())
             || (player.HasModifier<GuardianAngelTargetModifier>() &&
                 (isDead
                  || (player.AmOwner &&
                      OptionGroupSingleton<FairyOptions>.Instance.FairyTargetKnows)))))
        {
            name += (player.HasModifier<GuardianAngelProtectModifier>() &&
                     OptionGroupSingleton<FairyOptions>.Instance.ShowProtect is not ProtectOptions.Fairy)
                ? "<color=#FFD900> ★</color>"
                : "<color=#B3FFFF> ★</color>";
        }

        if ((player.HasModifier(MedicShieldPredicate) &&
             PlayerControl.LocalPlayer.IsRole<MedicRole>())
            || (player.HasModifier<MedicShieldModifier>() &&
                (isDead
                 || (player.AmOwner && player.TryGetModifier<MedicShieldModifier>(out var med) && med.VisibleSymbol))))
        {
            name += "<color=#006600> +</color>";
        }

        if ((player.HasModifier(MagicMirrorPredicate) &&
             PlayerControl.LocalPlayer.IsRole<MirrorcasterRole>())
            || (player.HasModifier<MagicMirrorModifier>() &&
                (isDead
                 || (player.AmOwner && player.TryGetModifier<MagicMirrorModifier>(out var mm) && mm.VisibleSymbol))))
        {
            name += "<color=#90A2C3>〚〛</color>";
        }

        if ((player.HasModifier(ClericBarrierPredicate) &&
             PlayerControl.LocalPlayer.IsRole<ClericRole>())
            || (player.HasModifier<ClericBarrierModifier>() &&
                (isDead
                 || (player.AmOwner && player.TryGetModifier<ClericBarrierModifier>(out var cleric) &&
                     cleric.VisibleSymbol))))
        {
            name += "<color=#00FFB3> Ω</color>";
        }

        if ((player.HasModifier(WardenFortifiedPredicate) &&
             PlayerControl.LocalPlayer.IsRole<WardenRole>())
            || (player.HasModifier<WardenFortifiedModifier>() &&
                (isDead
                 || (player.AmOwner && player.TryGetModifier<WardenFortifiedModifier>(out var warden) &&
                     warden.VisibleSymbol))))
        {
            name += "<color=#9900FF> π</color>";
        }

        return name;
    }

    public static string UpdateAllianceSymbols(this string name, PlayerControl player, bool hidden = false)
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var isDead = PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !hidden;

        if (player.HasModifier<LoverModifier>() && (PlayerControl.LocalPlayer.HasModifier<LoverModifier>() || isDead))
        {
            name += "<color=#FF66CC> ♥</color>";
        }

        if (player.IsCrewmate() && player.TryGetModifier<EgotistModifier>(out var egoMod) && (player.AmOwner ||
                                                                       (EgotistModifier.EgoVisibilityFlag(player) &&
                                                                        (player.GetModifiers<RevealModifier>().Any(RevealVisibleRolePredicate))) || isDead))
        {
            name += $"<color=#FFFFFF> (<color=#669966>{egoMod.ModifierName}</color>)</color>";
        }

        if (player.IsCrewmate() && player.TryGetModifier<CrewpostorModifier>(out var postorMod) && (CrewpostorModifier.CrewpostorVisibilityFlag(player) || isDead))
        {
            name += $"<color=#FFFFFF> (<color=#D64042>{postorMod.ModifierName}</color>)</color>";
        }

        return name;
    }

    public static string UpdateStatusSymbols(this string name, PlayerControl player, bool hidden = false)
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var isImp = PlayerControl.LocalPlayer.IsImpostor() && genOpt.ImpsKnowRoles && !genOpt.FFAImpostorMode;
        var isDead = PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !hidden;

        if ((player.HasModifier(PbPredicate1) && PlayerControl.LocalPlayer.IsRole<PlaguebearerRole>())
            || (player.HasModifier(PbPredicate2) && isDead))
        {
            name += "<color=#E6FFB3> ¥</color>";
        }

        if ((player.HasModifier(ArsonistDousedPredicate) && PlayerControl.LocalPlayer.IsRole<ArsonistRole>())
            || (player.HasModifier<ArsonistDousedModifier>() && isDead))
        {
            name += "<color=#FF4D00> Δ</color>";
        }

        if ((player.HasModifier(BlackmailedPredicate) &&
             PlayerControl.LocalPlayer.IsRole<BlackmailerRole>())
            || (player.HasModifier<BlackmailedModifier>() && (isDead || isImp)))
        {
            name += "<color=#2A1119> M</color>";
        }

        if ((player.HasModifier(HypnotisedPredicate) &&
             PlayerControl.LocalPlayer.IsRole<HypnotistRole>())
            || (player.HasModifier<HypnotisedModifier>() && (isDead || isImp)))
        {
            name += "<color=#D53F42> @</color>";
        }

        if (player.HasModifier(SpellslingerHexedPredicate) &&
            PlayerControl.LocalPlayer.IsRole<SpellslingerRole>()
            || player.HasModifier<SpellslingerHexedModifier>() && (isDead || isImp))
        {
            name += $" {TownOfUsColors.Impostor.ToTextColor()}乂</color>";
        }
        if (player.HasModifier<KnightedModifier>() && (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !hidden || PlayerControl.LocalPlayer.IsRole<MonarchRole>()))
            name += $" {TownOfUsColors.Monarch.ToTextColor()}♠</color>";

        return name;
    }
}