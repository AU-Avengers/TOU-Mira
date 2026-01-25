using MiraAPI.GameEnd;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.GameOver;

public sealed class LoverGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return winners.All(plr => plr.Object.HasModifier<LoverModifier>());
    }

    public override void AfterEndGameSetup(EndGameManager endGameManager)
    {
        endGameManager.BackgroundBar.material.SetColor(ShaderID.Color, TownOfUsColors.Lover);

        var text = Object.Instantiate(endGameManager.WinText);
        text.text = $"{TouLocale.Get("LoversWin")}!";
        text.color = TownOfUsColors.Lover;
        GameHistory.WinningFaction = $"<color=#{TownOfUsColors.Lover.ToHtmlStringRGBA()}>{TouLocale.Get("LoversWin")}</color>";

        var pos = endGameManager.WinText.transform.localPosition;
        pos.y = 1.5f;
        pos += Vector3.down * 0.15f;
        text.transform.localScale = new Vector3(1f, 1f, 1f);

        text.transform.position = pos;
        text.text = $"<size=4>{text.text}</size>";
    }
}