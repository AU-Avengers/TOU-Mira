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

        if (player.HasModifier<PoliticianCampaignedModifier>(x => x.Politician.AmOwner) &&
            PlayerControl.LocalPlayer.IsRole<PoliticianRole>())
        {
            color = Color.cyan;
        }

        if (player.HasModifier<MercenaryBribedModifier>(x => x.Mercenary.AmOwner) &&
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
        if ((player.HasModifier<ExecutionerTargetModifier>(x => x.OwnerId == PlayerControl.LocalPlayer.PlayerId) &&
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
            player.HasModifier<HunterStalkedModifier>(x => x.Hunter.AmOwner))
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
        if ((player.HasModifier<GuardianAngelTargetModifier>(x => x.OwnerId == PlayerControl.LocalPlayer.PlayerId) &&
             PlayerControl.LocalPlayer.IsRole<GuardianAngelTouRole>())
            || (player.HasModifier<GuardianAngelTargetModifier>() &&
                ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !hidden)
                 || (player.AmOwner &&
                     OptionGroupSingleton<GuardianAngelOptions>.Instance.GATargetKnows))))
        {
            name += (player.HasModifier<GuardianAngelProtectModifier>() && OptionGroupSingleton<GuardianAngelOptions>.Instance.ShowProtect is not ProtectOptions.GA)
                ? "<color=#FFD900> ★</color>"
                : "<color=#B3FFFF> ★</color>";
        }

        if ((player.HasModifier<MedicShieldModifier>(x => x.Medic.AmOwner) &&
             PlayerControl.LocalPlayer.IsRole<MedicRole>())
            || (player.HasModifier<MedicShieldModifier>() &&
                ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !hidden)
                 || (player.AmOwner && player.TryGetModifier<MedicShieldModifier>(out var med) && med.VisibleSymbol))))
        {
            name += "<color=#006600> +</color>";
        }
        
        if ((player.HasModifier<MagicMirrorModifier>(x => x.Mirrorcaster.AmOwner) &&
             PlayerControl.LocalPlayer.IsRole<MirrorcasterRole>())
            || (player.HasModifier<MagicMirrorModifier>() &&
                ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !hidden)
                 || (player.AmOwner && player.TryGetModifier<MagicMirrorModifier>(out var mm) && mm.VisibleSymbol))))
        {
            name += "<color=#90A2C3>〚〛</color>";
        }

        if ((player.HasModifier<ClericBarrierModifier>(x => x.Cleric.AmOwner) &&
             PlayerControl.LocalPlayer.IsRole<ClericRole>())
            || (player.HasModifier<ClericBarrierModifier>() &&
                ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !hidden)
                 || (player.AmOwner && player.TryGetModifier<ClericBarrierModifier>(out var cleric) &&
                     cleric.VisibleSymbol))))
        {
            name += "<color=#00FFB3> Ω</color>";
        }

        if ((player.HasModifier<WardenFortifiedModifier>(x => x.Warden.AmOwner) &&
             PlayerControl.LocalPlayer.IsRole<WardenRole>())
            || (player.HasModifier<WardenFortifiedModifier>() &&
                ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !hidden)
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

        if (player.HasModifier<LoverModifier>() && (PlayerControl.LocalPlayer.HasModifier<LoverModifier>() ||
                                                    (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow &&
                                                     !hidden)))
        {
            name += "<color=#FF66CC> ♥</color>";
        }

        if (player.TryGetModifier<EgotistModifier>(out var egoMod) && (player.AmOwner ||
                                                      (EgotistModifier.EgoVisibilityFlag(player) &&
                                                       (player.GetModifiers<RevealModifier>().Any(x => x.Visible && x.RevealRole))) ||
                                                      (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow &&
                                                       !hidden)))
        {
            name += $"<color=#FFFFFF> (<color=#669966>{egoMod.ModifierName}</color>)</color>";
        }
        return name;
    }

    public static string UpdateStatusSymbols(this string name, PlayerControl player, bool hidden = false)
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;

        if ((player.HasModifier<PlaguebearerInfectedModifier>(x =>
                 x.PlagueBearerId == PlayerControl.LocalPlayer.PlayerId) &&
             PlayerControl.LocalPlayer.IsRole<PlaguebearerRole>())
            || (player.HasModifier<PlaguebearerInfectedModifier>() && PlayerControl.LocalPlayer.HasDied() &&
                genOpt.TheDeadKnow && !hidden))
        {
            name += "<color=#E6FFB3> ¥</color>";
        }

        if ((player.HasModifier<ArsonistDousedModifier>(x => x.ArsonistId == PlayerControl.LocalPlayer.PlayerId) &&
             PlayerControl.LocalPlayer.IsRole<ArsonistRole>())
            || (player.HasModifier<ArsonistDousedModifier>() && PlayerControl.LocalPlayer.HasDied() &&
                genOpt.TheDeadKnow && !hidden))
        {
            name += "<color=#FF4D00> Δ</color>";
        }

        if ((player.HasModifier<BlackmailedModifier>(x => x.BlackMailerId == PlayerControl.LocalPlayer.PlayerId) &&
             PlayerControl.LocalPlayer.IsRole<BlackmailerRole>())
            || (player.HasModifier<BlackmailedModifier>() && PlayerControl.LocalPlayer.IsImpostor() &&
                genOpt.ImpsKnowRoles && !genOpt.FFAImpostorMode)
            || (player.HasModifier<BlackmailedModifier>() && PlayerControl.LocalPlayer.HasDied() &&
                genOpt.TheDeadKnow && !hidden))
        {
            name += "<color=#2A1119> M</color>";
        }

        if ((player.HasModifier<HypnotisedModifier>(x => x.Hypnotist.AmOwner) &&
             PlayerControl.LocalPlayer.IsRole<HypnotistRole>())
            || (player.HasModifier<HypnotisedModifier>() && PlayerControl.LocalPlayer.IsImpostor() &&
                genOpt.ImpsKnowRoles && !genOpt.FFAImpostorMode)
            || (player.HasModifier<HypnotisedModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow &&
                !hidden))
        {
            name += "<color=#D53F42> @</color>";
        }

        if (player.HasModifier<SpellslingerHexedModifier>() && (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !hidden || PlayerControl.LocalPlayer.IsImpostor()))
            name += $" {TownOfUsColors.Impostor.ToTextColor()}乂</color>";

        if (player.HasModifier<KnightedModifier>() && (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !hidden || PlayerControl.LocalPlayer.IsRole<MonarchRole>()))
            name += $" {TownOfUsColors.Monarch.ToTextColor()}♠</color>";

        return name;
    }
}