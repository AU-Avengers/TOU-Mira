using System.Globalization;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class PsychicRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public enum Prophecy
    {
        None = 0,
        ImpostorKilling,
        ImpostorPower,
        ImpostorConcealing,
        ImpostorSupport,
        CommonImpostor,
        ImpostorSpecial,
        NeutralEvil,
        NeutralBenign,
        CommonNeutral,
        NeutralKilling
    }
    public Prophecy CurrentProphecy { get; private set; } = Prophecy.None;
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string RoleName => TouLocale.Get(TouNames.Psychic, "Psychic");
    public string RoleDescription => "Check Two Players For An Evil Alignment";
    public string RoleLongDescription => "Select two players and see if they match your selected Prophecy";
    public Color RoleColor => TownOfUsColors.Psychic;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Psychic,
        IntroSound = TouAudio.QuestionSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        stringB.Append(CultureInfo.InvariantCulture, $"\n<b>Prophecy:</b> {CurrentProphecy.GetProphecyString()}");

        return stringB;
    }

    public string GetAdvancedDescription()
    {
        return $"The {RoleName} is a Crewmate Investigative role that can pick two players to see if they match their Prophecy."
               + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Vision",
            "Have a vision of two players with your selected Prophecy.\nIf at least one of them is your Prophecy, you will be informed.\n\n<b>You are not told which one matches your Prophecy.</b>",
            TouCrewAssets.PsychicSprite),
        new("Change Prophecy",
            "Cycle your prophecy between all Impostor and Neutral alignments.",
            TouCrewAssets.ProphecySprite)
    ];

    [MethodRpc((uint)TownOfUsRpc.Vision, SendImmediately = true)]
    public static void RpcVision(PlayerControl psychic, byte player1, byte player2)
    {
        if (psychic.Data.Role is not PsychicRole psychicRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcVision - Invalid Psychic");
            return;
        }

        var t1 = GetTarget(player1);
        var t2 = GetTarget(player2);
        var prophecy = psychicRole.CurrentProphecy;

        if (t1 == null || t2 == null)
        {
            if (psychic.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(Color.red));
                ShowNotification($"<b>You need to pick two targets.</b>");
            }
            return;
        }

        if (t1 == psychic || t2 == psychic)
        {
            Coroutines.Start(MiscUtils.CoFlash(Color.red));
            ShowNotification($"<b>You can't use yourself in a vision!</b>");
            return;
        }

        if (prophecy == Prophecy.None)
        {
            Coroutines.Start(MiscUtils.CoFlash(Color.red));
            ShowNotification($"<b>You haven't set a Prophecy!</b>");
            return;
        }
        
        var play1 = MiscUtils.PlayerById(player1)!;
        var play2 = MiscUtils.PlayerById(player2)!;

        if (play1.TryGetModifier<InvulnerabilityModifier>(out var invic) && invic.AttackAllInteractions)
        {
            if (psychic.AmOwner) play1.RpcCustomMurder(psychic);
            return;
        }

        if (play2.TryGetModifier<InvulnerabilityModifier>(out var invic2) && invic2.AttackAllInteractions)
        {
            if (psychic.AmOwner) play2.RpcCustomMurder(psychic);
            return;
        }

        if (play1.HasModifier<VeteranAlertModifier>())
        {
            if (psychic.AmOwner) play1.RpcCustomMurder(psychic);
            return;
        }

        if (play2.HasModifier<VeteranAlertModifier>())
        {
            if (psychic.AmOwner) play2.RpcCustomMurder(psychic);
            return;
        }

        if (psychic.AmOwner)
        {
            var button = CustomButtonSingleton<Psychic1VisionButton>.Instance;
            button.ResetCooldownAndOrEffect();
        }


        var playerA = play1.CachedPlayerData.PlayerName;
        var playerB = play2.CachedPlayerData.PlayerName;

        void ShowNotification(string message)
        {
            if (psychic.AmOwner)
            {
                var notif = Helpers.CreateAndShowNotification(
                    message, Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Psychic.LoadAsset());
                notif.Text.SetOutlineThickness(0.35f);
            }
        }

        bool matchFound = false;
        switch (prophecy)
        {
            case Prophecy.ImpostorKilling:
                if (play1.Is(RoleAlignment.ImpostorKilling) || play2.Is(RoleAlignment.ImpostorKilling))
                    matchFound = true;
                break;
            case Prophecy.ImpostorPower:
                if (play1.Is(RoleAlignment.ImpostorPower) || play2.Is(RoleAlignment.ImpostorPower))
                    matchFound = true;
                break;
            case Prophecy.ImpostorSupport:
                if (play1.Is(RoleAlignment.ImpostorSupport) || play2.Is(RoleAlignment.ImpostorSupport))
                    matchFound = true;
                break;
            case Prophecy.ImpostorConcealing:
                if (play1.Is(RoleAlignment.ImpostorConcealing) || play2.Is(RoleAlignment.ImpostorConcealing))
                    matchFound = true;
                break;
            case Prophecy.NeutralBenign:
                if (play1.Is(RoleAlignment.NeutralBenign) || play2.Is(RoleAlignment.NeutralBenign))
                    matchFound = true;
                break;
            case Prophecy.NeutralEvil:
                if (play1.Is(RoleAlignment.NeutralEvil) || play2.Is(RoleAlignment.NeutralEvil))
                    matchFound = true;
                break;
            case Prophecy.NeutralKilling:
                if (play1.Is(RoleAlignment.NeutralKilling) || play2.Is(RoleAlignment.NeutralKilling))
                    matchFound = true;
                break;
            // case Prophecy.NeutralOutlier:
            //     if (play1.Is(RoleAlignment.NeutralOutlier) || play2.Is(RoleAlignment.NeutralOutlier))
            //         matchFound = true;
            //     break;
            case Prophecy.CommonImpostor:
                if (play1.Is(RoleAlignment.ImpostorSupport) || play2.Is(RoleAlignment.ImpostorSupport) ||
                    play1.Is(RoleAlignment.ImpostorConcealing) || play2.Is(RoleAlignment.ImpostorConcealing))
                    matchFound = true;
                break;
            case Prophecy.ImpostorSpecial:
                if (play1.Is(RoleAlignment.ImpostorPower) || play2.Is(RoleAlignment.ImpostorPower) ||
                    play1.Is(RoleAlignment.ImpostorKilling) || play2.Is(RoleAlignment.ImpostorKilling))
                    matchFound = true;
                break;
            case Prophecy.CommonNeutral:
                if (play1.Is(RoleAlignment.NeutralEvil) || play2.Is(RoleAlignment.NeutralEvil) ||
                    play1.Is(RoleAlignment.NeutralBenign) || play2.Is(RoleAlignment.NeutralBenign))
                    matchFound = true;
                break;
        }

        if (matchFound)
        {
            Coroutines.Start(MiscUtils.CoFlash(Palette.ImpostorRed));
            ShowNotification($"<b>{Palette.ImpostorRed.ToTextColor()}At least one of {playerA} and {playerB} matches your {prophecy.GetProphecyString()} prophecy!</color></b>");
        }
        else
        {
            Coroutines.Start(MiscUtils.CoFlash(Palette.CrewmateBlue));
            ShowNotification($"<b>{Palette.CrewmateBlue.ToTextColor()}Neither {playerA} nor {playerB} matches your {prophecy.GetProphecyString()} prophecy!</color></b>");
        }

        static MonoBehaviour? GetTarget(byte id)
        {
            var data = GameData.Instance.GetPlayerById(id);
            if (!data)
            {
                return null;
            }

            var body = Helpers.GetBodyById(id);
            if (data.IsDead && body)
            {
                return body;
            }

            var pc = data.Object;
            if (!pc)
            {
                return null;
            }

            return pc;
        }
    }
    [MethodRpc((uint)TownOfUsRpc.Prophecy, SendImmediately = true)]
    public static void RpcProphecy(PlayerControl psychic)
    {
        if (psychic.Data.Role is not PsychicRole psychicRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcProphecy - Invalid Psychic");
            return;
        }

        psychicRole.ChangeProphecy();
    }

public void ChangeProphecy()
{
    var values = ((Prophecy[])Enum.GetValues(typeof(Prophecy)))
        .Where(p =>
            p != Prophecy.None &&

            (OptionGroupSingleton<PsychicOptions>.Instance.UseCommonImpostor
                ? p != Prophecy.ImpostorKilling &&
                  p != Prophecy.ImpostorPower &&
                  p != Prophecy.ImpostorSupport &&
                  p != Prophecy.ImpostorConcealing
                : p != Prophecy.CommonImpostor) &&

            (OptionGroupSingleton<PsychicOptions>.Instance.UseSpecialImpostor
                ? p != Prophecy.ImpostorKilling &&
                  p != Prophecy.ImpostorPower
                : p != Prophecy.ImpostorSpecial) &&

            (OptionGroupSingleton<PsychicOptions>.Instance.UseCommonNeutral
                ? p != Prophecy.NeutralEvil &&
                  p != Prophecy.NeutralBenign
                : p != Prophecy.CommonNeutral)
        )
        .ToArray();

    int currentIndex = Array.IndexOf(values, CurrentProphecy);
    int nextIndex = (currentIndex + 1) % values.Length;
    CurrentProphecy = values[nextIndex];

    if (PlayerControl.LocalPlayer.AmOwner)
    {
        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>Prophecy has been changed to {CurrentProphecy.GetProphecyString()}</color></b>",
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Psychic.LoadAsset());
        notif1.Text.SetOutlineThickness(0.35f);
    }
}

}

public static class ProphecyExtensions
{
    public static string GetProphecyString(this PsychicRole.Prophecy prophecy)
    {
        return prophecy switch
        {
            PsychicRole.Prophecy.ImpostorKilling => "Impostor Killing",
            PsychicRole.Prophecy.ImpostorPower => "Impostor Power",
            PsychicRole.Prophecy.ImpostorConcealing => "Impostor Concealing",
            PsychicRole.Prophecy.ImpostorSupport => "Impostor Support",
            PsychicRole.Prophecy.NeutralEvil => "Neutral Evil",
            PsychicRole.Prophecy.NeutralBenign => "Neutral Benign",
            PsychicRole.Prophecy.NeutralKilling => "Neutral Killing",
            PsychicRole.Prophecy.CommonNeutral => "Common Neutral",
            PsychicRole.Prophecy.CommonImpostor => "Common Impostor",
            PsychicRole.Prophecy.ImpostorSpecial => "Impostor Special",
            _ => "None"
        };
    }
}
