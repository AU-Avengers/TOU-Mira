using System.Collections;
using HarmonyLib;
using InnerNet;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.GameOver;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;
using Random = System.Random;

namespace TownOfUs.Modifiers.Game.Alliance;

public sealed class LoverModifier : AllianceGameModifier, IWikiDiscoverable, IAssignableTargets
{
    public override string ModifierName => TouLocale.Get(TouNames.Lover, "Lover");
    public override string Symbol => "♥";
    public override string IntroInfo => LoverString();
    public override float IntroSize => 3f;
    public override Color FreeplayFileColor => new Color32(220, 220, 220, 255);

    public override bool DoesTasks =>
        (OtherLover == null || OtherLover.IsCrewmate()) &&
        Player.IsCrewmate(); // Lovers do tasks if they are not lovers with an Evil

    public override bool HideOnUi => false;
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Lover;

    public PlayerControl? OtherLover { get; set; }

    public override int CustomAmount =>
        (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.LoversChance != 0 ? 2 : 0;

    public override int CustomChance => (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.LoversChance;
    public int Priority { get; set; } = 4;

    public void AssignTargets()
    {
        foreach (var lover in PlayerControl.AllPlayerControls.ToArray().Where(x => x.HasModifier<LoverModifier>())
                     .ToList())
        {
            lover.RemoveModifier<LoverModifier>();
        }

        Random rnd = new();
        var chance = rnd.Next(1, 101);

        if (chance <= (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.LoversChance)
        {
            var loveOpt = OptionGroupSingleton<LoversOptions>.Instance;
            var impTargetPercent = (int)loveOpt.LovingImpPercent;

            var players = PlayerControl.AllPlayerControls.ToArray()
                .Where(x => !x.HasDied() && !x.HasModifier<ExecutionerTargetModifier>() &&
                            x.Data.Role is not InquisitorRole && (loveOpt.NeutralLovers || !x.IsNeutral())).ToList();
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
                         ((player.Is(RoleAlignment.NeutralBenign) || player.Is(RoleAlignment.NeutralEvil)) &&
                          loveOpt.NeutralLovers))
                {
                    crewmates.Add(player);
                }
            }

            if (crewmates.Count < 2 || impostors.Count < 1)
            {
                Logger<TownOfUsPlugin>.Error("Not enough players to select lovers");
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

    public string GetAdvancedDescription()
    {
        return
            $"As a {ModifierName.ToLowerInvariant()}, you can chat with your other {ModifierName.ToLowerInvariant()} (signified with <color=#FF66CCFF>♥</color>) during the round, and you can win with your {ModifierName.ToLowerInvariant()} if you are both a part of the final 3 players."
            + MiscUtils.AppendOptionsText(GetType());
    }

    public override string GetDescription()
    {
        return LoverString();
    }

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
        if (!Player.AmOwner)
        {
            return;
        }

        HudManager.Instance.Chat.gameObject.SetActive(true);
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

        var players = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => !x.HasDied() && !x.HasModifier<ExecutionerTargetModifier>() &&
                        x.Data.Role is not InquisitorRole).ToList();
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
                     ((player.Is(RoleAlignment.NeutralBenign) || player.Is(RoleAlignment.NeutralEvil)) &&
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
        loverMod!.OtherLover = randomTarget;
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
            var players = ModifierUtils.GetPlayersWithModifier<LoverModifier>().ToList();
            players.Do(x => x.RpcRemoveModifier<LoverModifier>());
        }
    }

    public override bool? DidWin(GameOverReason reason)
    {
        return reason == CustomGameOver.GameOverReason<LoverGameOver>() ? true : null;
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
    }

    public string LoverString()
    {
        return !OtherLover ? "You are in love with nobody" : $"You are in love with {OtherLover!.Data.PlayerName}";
    }

    public PlayerControl? GetOtherLover()
    {
        return OtherLover;
    }

    [MethodRpc((uint)TownOfUsRpc.SetOtherLover, SendImmediately = true)]
    private static void RpcSetOtherLover(PlayerControl player, PlayerControl target)
    {
        if (PlayerControl.AllPlayerControls.ToArray().Where(x => x.HasModifier<LoverModifier>()).ToList().Count > 0)
        {
            Logger<TownOfUsPlugin>.Error("RpcSetOtherLover - Lovers Already Spawned!");
            return;
        }

        var targetModifier = target.AddModifier<LoverModifier>();
        var sourceModifier = player.AddModifier<LoverModifier>();
        targetModifier!.OtherLover = player;
        sourceModifier!.OtherLover = target;
    }
}