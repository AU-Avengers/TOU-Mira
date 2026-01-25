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
        //PoolablePlayer[] array = Object.FindObjectsOfType<PoolablePlayer>();
        //if (array[0] != null)
        //{
        //    array[0].SetFlipX(true);

        //    array[0].gameObject.transform.position -= new Vector3(1.5f, 0f, 0f);
        //    array[0].cosmetics.skin.transform.localScale = new Vector3(-1, 1, 1);
        //    array[0].cosmetics.nameText.color = new Color(1f, 0.4f, 0.8f, 1f);
        //}

        //if (array[1] != null)
        //{
        //    array[1].SetFlipX(false);

        //    array[1].gameObject.transform.position = array[0].gameObject.transform.position + new Vector3(1.2f, 0f, 0f);
        //    array[1].gameObject.transform.localScale *= 0.92f;
        //    array[1].cosmetics.skin.transform.localScale = new Vector3(-1, 1, 1);
        //    array[1].cosmetics.hat.transform.position += new Vector3(0.1f, 0f, 0f);
        //    array[1].cosmetics.nameText.color = new Color(1f, 0.4f, 0.8f, 1f);
        //}

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