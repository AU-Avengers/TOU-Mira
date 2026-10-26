using System.Collections;
using HarmonyLib;
using InnerNet;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.GameOver;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Roles.Other;
using UnityEngine;
using Random = System.Random;

namespace TownOfUs.Modifiers.Game.Alliance;

public sealed class LoverModifier : AllianceGameModifier, IWikiDiscoverable, IAssignableTargets
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Lover,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Lover,
            "TouMira.Modifier.Alliance.Lover", 1.45f));
    public override string IdPart => "Lover";
    public override string ModifierName => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}");
    public override string IntroInfo => LoverString();
    public bool LoverDisconnected { get; internal set; }
    public string LoverDcString { get; internal set; } = " ";

    public override string GetDescription()
    {
        return LoverString();
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.WikiDescription")
            .Replace("<symbol>", "<color=#FF66CCFF>♥</color>") + MiscUtils.AppendOptionsText(GetType());
    }

    public string LoverString()
    {
        return LoverDisconnected || OtherLover == null ? LoverDcString : MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}Info")
            .Replace("<player>", OtherLover.Data.PlayerName);
    }

    public override string Symbol => "♥";
    public override float IntroSize => 3f;
    public override Color FreeplayFileColor => new Color32(220, 220, 220, 255);

    public override bool DoesTasks =>
        (OtherLover == null || OtherLover.IsCrewmate()) &&
        Player.IsCrewmate() && !ForceDisableTasks; // Lovers do tasks if they are not lovers with an Evil

    public bool ForceDisableTasks;

    public override bool HideOnUi => false;
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Lover;

    public PlayerControl? OtherLover { get; set; }

    public override int CustomAmount =>
        OptionGroupSingleton<AllianceModifierOptions>.Instance.LoversChance != 0 ? 2 : 0;

    public override int CustomChance => OptionGroupSingleton<AllianceModifierOptions>.Instance.LoversChance;
    public int Priority { get; set; } = 4;

    public void AssignTargets()
    {
        if (!RoleOptions.IsClassicRoleAssignment)
        {
            return;
        }

        foreach (var lover in PlayerControl.AllPlayerControls.Where(x => x.HasModifier<LoverModifier>()))
        {
            lover.RpcRemoveModifier<LoverModifier>();
        }

        Random rnd = new();
        var chance = rnd.Next(1, 101);

        if (chance <= OptionGroupSingleton<AllianceModifierOptions>.Instance.LoversChance)
        {
            var loveOpt = OptionGroupSingleton<LoversOptions>.Instance;
            var impTargetPercent = loveOpt.LovingImpPercent;

            var players = PlayerControl.AllPlayerControls.Where(x => !x.HasDied() && !x.HasModifier<ExecutionerTargetModifier>() &&
                            !x.HasModifier<AllianceGameModifier>() &&
                            !SpectatorRole.TrackedSpectators.Contains(x.GetBestPlayerDenomination()) &&
                            (x.Data.Role is not IUnlovable { IsUnlovable: true }) && (loveOpt.NeutralLovers || !x.IsNeutral())).ToList();
            players.Shuffle();

            Random rndIndex1 = new();
            var randomLover = players[rndIndex1.Next(0, players.Count)];
            players.Remove(randomLover);

            var crewmates = new List<PlayerControl>();
            var impostors = new List<PlayerControl>();

            foreach (var player in players.SelectMany(_ => players))
            {
                if (player.IsImpostor() || (player.Is(RoleAlignment.NeutralKilling) &&
                                            loveOpt.NeutralLovers))
                {
                    impostors.Add(player);
                }
                else if (player.Is(ModdedRoleTeams.Crewmate) ||
                         ((player.Is(RoleAlignment.NeutralBenign) || player.Is(RoleAlignment.NeutralEvil) || player.Is(RoleAlignment.NeutralOutlier)) &&
                          loveOpt.NeutralLovers))
                {
                    crewmates.Add(player);
                }
            }

            if (crewmates.Count < 2 || impostors.Count < 1)
            {
                Error("Not enough players to select lovers");
                return;
            }

            if (randomLover.IsImpostor())
            {
                impostors = impostors.Where(player => !player.IsImpostor()).ToList();
                players = players.Where(player => !player.IsImpostor()).ToList();
            }

            if (impTargetPercent > 0f)
            {
                Random rnd2 = new();
                var chance2 = rnd2.Next(0, 100);

                if (chance2 < impTargetPercent)
                {
                    players = impostors;
                }
            }
            else
            {
                players = crewmates;
            }

            Random rndIndex = new();
            var randomTarget = players[rndIndex.Next(0, players.Count)];
            RpcSetOtherLover(randomLover, randomTarget);
        }
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAmountPerGame()
    {
        return 0;
    }

    public override int GetAssignmentChance()
    {
        return 0;
    }

    public override void OnActivate()
    {
        base.OnActivate();
        if (!Player.AmOwner)
        {
            return;
        }

        HudManager.Instance.Chat.gameObject.SetActive(true);
        var buttonArray = new []
            { TouChatAssets.LoveChatIdle, TouChatAssets.LoveChatHover, TouChatAssets.LoveChatOpen};
        HudManager.Instance.Chat.chatButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite = buttonArray[0];
        HudManager.Instance.Chat.chatButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite = buttonArray[1];
        HudManager.Instance.Chat.chatButton.transform.Find("Selected").GetComponent<SpriteRenderer>().sprite = buttonArray[2];
        if (TutorialManager.InstanceExists && OtherLover == null && Player.AmOwner && Player.IsHost() &&
            AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
        {
            Coroutines.Start(SetTutorialTarget(this, Player));
        }
    }

    private static IEnumerator SetTutorialTarget(LoverModifier loverMod, PlayerControl localPlr)
    {
        yield return new WaitForSeconds(0.01f);
        var impTargetPercent = (int)OptionGroupSingleton<LoversOptions>.Instance.LovingImpPercent;

        var players = PlayerControl.AllPlayerControls.Where(x => !x.HasDied() && !x.HasModifier<ExecutionerTargetModifier>() &&
                        (x.Data.Role is not IUnlovable { IsUnlovable: true })).ToList();
        players.Shuffle();

        players.Remove(localPlr);

        var crewmates = new List<PlayerControl>();
        var impostors = new List<PlayerControl>();

        foreach (var player in players.SelectMany(_ => players))
        {
            if (player.IsImpostor() || (player.Is(RoleAlignment.NeutralKilling) &&
                                        OptionGroupSingleton<LoversOptions>.Instance.NeutralLovers))
            {
                impostors.Add(player);
            }
            else if (player.Is(ModdedRoleTeams.Crewmate) ||
                     ((player.Is(RoleAlignment.NeutralBenign) || player.Is(RoleAlignment.NeutralEvil) || player.Is(RoleAlignment.NeutralOutlier)) &&
                      OptionGroupSingleton<LoversOptions>.Instance.NeutralLovers))
            {
                crewmates.Add(player);
            }
        }

        if (localPlr.IsImpostor())
        {
            impostors = impostors.Where(player => !player.IsImpostor()).ToList();
            players = players.Where(player => !player.IsImpostor()).ToList();
        }

        if (impTargetPercent > 0f && impostors.Count != 0)
        {
            Random rnd = new();
            var chance2 = rnd.Next(1, 101);

            if (chance2 <= impTargetPercent)
            {
                players = impostors;
            }
        }
        else
        {
            players = crewmates;
        }

        Random rndIndex = new();
        var randomTarget = players[rndIndex.Next(0, players.Count)];

        var sourceModifier = randomTarget.AddModifier<LoverModifier>();
        yield return new WaitForSeconds(0.01f);
        sourceModifier!.OtherLover = localPlr;
        sourceModifier.LoverDcString = MiraLocaleManager.Get("TownOfUsMira.Modifier.LoverInfoDisconnected")
            .Replace("<player>", localPlr.Data.PlayerName);
        loverMod!.OtherLover = randomTarget;
        loverMod.LoverDcString = MiraLocaleManager.Get("TownOfUsMira.Modifier.LoverInfoDisconnected")
            .Replace("<player>", randomTarget.Data.PlayerName);
    }

    public override void OnDeactivate()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        HudManager.Instance.Chat.gameObject.SetActive(false);
        if (TutorialManager.InstanceExists)
        {
            var players = ModifierUtils.GetPlayersWithModifier<LoverModifier>();
            players.Do(x => x.RpcRemoveModifier<LoverModifier>());
        }
    }

    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        if (!Player.AmOwner)
        {
            return;
        }

        var buttonArray = new Sprite[]
        {
            TouChatAssets.NormalChatIdle, TouChatAssets.NormalChatHover,
            TouChatAssets.NormalChatOpen
        };
        HudManager.Instance.Chat.chatButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite =
            buttonArray[0];
        HudManager.Instance.Chat.chatButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite =
            buttonArray[1];
        HudManager.Instance.Chat.chatButton.transform.Find("Selected").GetComponent<SpriteRenderer>().sprite =
            buttonArray[2];
    }

    public override bool? DidWin(GameOverReason reason)
    {
        if (reason == CustomGameOver.GameOverReason<LoverGameOver>())
        {
            return true;
        }

        // Co-win: if the Jester is a Lover and gets voted out (NeutralGameOver winner),
        // both Lovers should be marked as winners.
        if (reason == CustomGameOver.GameOverReason<NeutralGameOver>())
        {
            var winners = EndGameResult.CachedWinners;
            if (winners != null && winners.Count == 1)
            {
                var winner = winners[0];
                var winnerRole = RoleManager.Instance.GetRole(winner.RoleWhenAlive);
                if (winnerRole is JesterRole)
                {
                    var winnerName = winner.PlayerName;
                    if (winnerName == Player.Data.PlayerName ||
                        (OtherLover != null && winnerName == OtherLover.Data.PlayerName))
                    {
                        return true;
                    }
                }
            }
        }

        return null;
    }

    public static bool WinConditionMet(LoverModifier[] lovers)
    {
        var bothLoversAlive = Helpers.GetAlivePlayers().Count(x => x.HasModifier<LoverModifier>()) >= 2;

        return Helpers.GetAlivePlayers().Count <= 3 && lovers.Length == 2 && bothLoversAlive;
    }

    public void OnRoundStart()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        HudManager.Instance.Chat.SetVisible(true);
        var buttonArray = new []
                { TouChatAssets.LoveChatIdle, TouChatAssets.LoveChatHover, TouChatAssets.LoveChatOpen};
        HudManager.Instance.Chat.chatButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite = buttonArray[0];
        HudManager.Instance.Chat.chatButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite = buttonArray[1];
        HudManager.Instance.Chat.chatButton.transform.Find("Selected").GetComponent<SpriteRenderer>().sprite = buttonArray[2];
    }

    public PlayerControl? GetOtherLover()
    {
        return OtherLover;
    }

    /// <summary>
    /// Debug helper (Freeplay/Tutorial) to force-assign a pair of players as Lovers.
    /// </summary>
    public static void DebugSetLovers(PlayerControl loverA, PlayerControl loverB, bool clearExisting = true)
    {
        if (loverA == null || loverB == null || loverA == loverB)
        {
            return;
        }

        if (clearExisting)
        {
            foreach (var player in PlayerControl.AllPlayerControls.Where(x => x != null && x.HasModifier<LoverModifier>()))
            {
                player.RpcRemoveModifier<LoverModifier>();
            }
        }

        // Uses the existing RPC path to ensure both sides are wired correctly.
        RpcSetOtherLover(loverA, loverB);
    }

    [MethodRpc((uint)TownOfUsRpc.SetOtherLover)]
    private static void RpcSetOtherLover(PlayerControl player, PlayerControl target)
    {
        if (PlayerControl.AllPlayerControls.Any(x => x.HasModifier<LoverModifier>()))
        {
            Error("RpcSetOtherLover - Lovers Already Spawned!");
            return;
        }

        var targetModifier = target.AddModifier<LoverModifier>();
        var sourceModifier = player.AddModifier<LoverModifier>();
        targetModifier!.OtherLover = player;
        targetModifier.LoverDcString = MiraLocaleManager.Get("TownOfUsMira.Modifier.LoverInfoDisconnected")
            .Replace("<player>", player.Data.PlayerName);
        sourceModifier!.OtherLover = target;
        sourceModifier.LoverDcString = MiraLocaleManager.Get("TownOfUsMira.Modifier.LoverInfoDisconnected")
            .Replace("<player>", target.Data.PlayerName);
        if (!player.IsCrewmate() || !target.IsCrewmate())
        {
            targetModifier.ForceDisableTasks = true;
            sourceModifier.ForceDisableTasks = true;
        }
    }
}