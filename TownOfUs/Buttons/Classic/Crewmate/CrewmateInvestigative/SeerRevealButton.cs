using System.Text;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class SeerRevealButton : TownOfUsRoleButton<SeerRole, PlayerControl>, ILegacyCapable
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.SeerReveal", "Reveal");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Seer;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<SeerOptions>.Instance.SeerCooldown + MapCooldown, 5f, 120f);
    public override int MaxUses => (int)OptionGroupSingleton<SeerOptions>.Instance.MaxCompares;
    public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? LegacyCrewAssets.SeerSprite : TouCrewAssets.SeerSprite;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) &&
               !OptionGroupSingleton<SeerOptions>.Instance.SalemSeer;
    }

    public override bool CanUse()
    {
        return base.CanUse() &&
               (OptionGroupSingleton<SeerOptions>.Instance.CanUseMultiplePerRound || !Role.UsedThisRound);
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return base.IsTargetValid(target) && !target!.HasModifier<SeerGoodRevealModifier>() &&
               !target!.HasModifier<SeerEvilRevealModifier>();
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        RevealAlliance(Target);
        Role.UsedThisRound = true;
        TouAudio.PlaySound(TouAudio.QuestionSound);

        Target?.cosmetics.SetOutline(false, new Il2CppSystem.Nullable<Color>(TownOfUsColors.Seer));
    }

    public static void RevealAlliance(PlayerControl target)
    {
        var options = OptionGroupSingleton<SeerOptions>.Instance;
        var possibleAlignment = new StringBuilder();

        if (IsEvil(target))
        {
            target.AddModifier<SeerEvilRevealModifier>();

            var evilRevealKey =
                options.ShowCrewmateKillingAsRed.Value ||
                options.ShowNeutralBenignAsRed.Value
                    ? "TouSeerPossiblyEvilReveal"
                    : "TouSeerEvilReveal";

            var evilRevealText = MiraLocaleManager.Get(evilRevealKey)
                .Replace("<player>", target.Data.PlayerName);

            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}{evilRevealText}</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: TouRoleIcons.Seer.LoadAsset());
                
            notif1.AdjustNotification();

            if (options.ShowCrewmateKillingAsRed.Value)
            {
                possibleAlignment.Append(
                    MiraLocaleManager.Get("TouSeerAlignmentCrewKiller") + ", ");
            }

            if (options.ShowNeutralBenignAsRed.Value)
            {
                possibleAlignment.Append(
                    MiraLocaleManager.Get("TouSeerAlignmentNeutralBenign") + ", ");
            }

            if (options.ShowNeutralEvilAsRed.Value)
            {
                possibleAlignment.Append(
                    MiraLocaleManager.Get("TouSeerAlignmentNeutralEvil") + ", ");
            }

            if (options.ShowNeutralKillingAsRed.Value)
            {
                possibleAlignment.Append(
                    MiraLocaleManager.Get("TouSeerAlignmentNeutralKiller") + ", ");
            }

            if (options.ShowNeutralOutlierAsRed.Value)
            {
                possibleAlignment.Append(
                    MiraLocaleManager.Get("TouSeerAlignmentNeutralOutlier") + ", ");
            }

            if (options.SwapTraitorColors.Value)
            {
                possibleAlignment.Append(
                    MiraLocaleManager.Get("TouSeerAlignmentTraitor") + ", ");
            }

            if (possibleAlignment.Length > 3)
            {
                possibleAlignment = possibleAlignment.Remove(possibleAlignment.Length - 2, 2);
            }

            var evilAlignmentText = possibleAlignment.Length > 1
                ? MiraLocaleManager.Get("TouSeerPossibleImpostorAlignment")
                    .Replace("<alignments>", possibleAlignment.ToString())
                : MiraLocaleManager.Get("TouSeerImpostorAlignment");

            Helpers.CreateAndShowNotification(
                MiraLocaleManager.Get("TouSeerMustBeAlignment")
                    .Replace("<alignments>", evilAlignmentText),
                TownOfUsColors.ImpSoft);
        }
        else
        {
            target.AddModifier<SeerGoodRevealModifier>();
            var goodRevealKey = "TouSeerGoodReveal";

            if (!options.ShowNeutralBenignAsRed.Value)
            {
                goodRevealKey = "TouSeerLikelyGoodReveal";
            }

            if (!options.ShowNeutralEvilAsRed)
            {
                goodRevealKey = "TouSeerProbablyGoodReveal";
            }

            if (!options.ShowNeutralKillingAsRed)
            {
                goodRevealKey = "TouSeerPossiblyGoodReveal";
            }

            var goodRevealText = MiraLocaleManager.Get(goodRevealKey)
                .Replace("<player>", target.Data.PlayerName);

            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{Palette.CrewmateBlue.ToTextColor()}{goodRevealText}</color></b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Seer.LoadAsset());
            notif1.AdjustNotification();

            if (!options.ShowNeutralBenignAsRed.Value)
            {
                possibleAlignment.Append(
                    MiraLocaleManager.Get("TouSeerAlignmentNeutralBenign") + ", ");
            }

            if (!options.ShowNeutralEvilAsRed.Value)
            {
                possibleAlignment.Append(
                    MiraLocaleManager.Get("TouSeerAlignmentNeutralEvil") + ", ");
            }

            if (!options.ShowNeutralKillingAsRed.Value)
            {
                possibleAlignment.Append(
                    MiraLocaleManager.Get("TouSeerAlignmentNeutralKiller") + ", ");
            }

            if (!options.ShowNeutralOutlierAsRed.Value)
            {
                possibleAlignment.Append(
                    MiraLocaleManager.Get("TouSeerAlignmentNeutralOutlier") + ", ");
            }

            if (possibleAlignment.Length > 3)
            {
                possibleAlignment = possibleAlignment.Remove(possibleAlignment.Length - 2, 2);
            }

            var goodAlignmentText = possibleAlignment.Length > 1
                ? MiraLocaleManager.Get("TouSeerPossibleCrewmateAlignment")
                    .Replace("<alignments>", possibleAlignment.ToString())
                : MiraLocaleManager.Get("TouSeerCrewmateAlignment");

            var notif2 = Helpers.CreateAndShowNotification(
                $"<b>{MiraLocaleManager.Get("TouSeerMustBeAlignment")
                    .Replace("<alignments>", goodAlignmentText)}</b>",
                Palette.CrewmateBlue);

            notif2.AdjustNotification();
        }
    }

    public static bool IsEvil(PlayerControl target)
    {
        var options = OptionGroupSingleton<SeerOptions>.Instance;
        return ((target.Is(RoleAlignment.CrewmateKilling) && options.ShowCrewmateKillingAsRed.Value) ||
                (target.Is(RoleAlignment.NeutralBenign) && options.ShowNeutralBenignAsRed.Value) ||
                (target.Is(RoleAlignment.NeutralEvil) && options.ShowNeutralEvilAsRed.Value) ||
                (target.Is(RoleAlignment.NeutralKilling) && options.ShowNeutralKillingAsRed.Value) ||
                (target.Is(RoleAlignment.NeutralOutlier) && options.ShowNeutralOutlierAsRed.Value) ||
                (target.IsImpostor() && !target.IsTraitor()) ||
                (target.IsTraitor() && options.SwapTraitorColors.Value));
    }
}