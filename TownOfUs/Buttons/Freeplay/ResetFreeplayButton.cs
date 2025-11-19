using System.Collections;
using AmongUs.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Freeplay;

public sealed class ResetFreeplayButton : TownOfUsButton
{
    public override string Name => TouLocale.GetParsed("FreeplayRestartButton", "Reset Game");
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 0.001f;
    public override float InitialCooldown => 0.001f;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => TouAssets.BroadcastSprite;
    public override bool UsableInDeath => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null &&
               TutorialManager.InstanceExists;
    }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        OnClick();
    }

    protected override void OnClick()
    {
        HudManager.Instance.ShowPopUp(TouLocale.GetParsed("FreeplayRestartPopup"));
        ShipStatus.Instance.Begin();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReviveEveryoneFreeplay();
        }
        Coroutines.Start(SetDummyData());
    }

    private static IEnumerator SetDummyData()
    {
        while (PlayerControl.LocalPlayer == null)
        {
            yield return null;
        }

        while (PlayerControl.LocalPlayer.Data == null)
        {
            yield return null;
        }

        while (PlayerControl.LocalPlayer.Data.Role == null)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.01f);

        var roleList = RoleManager.Instance.AllRoles.ToArray()
            .Where(role => !role.IsDead)
            .Where(role => !role.IsImpostor())
            .ToList();

        foreach (var dummy in PlayerControl.AllPlayerControls.ToArray().Where(x => !x.AmOwner))
        {
            var random = roleList.Random();
            if (random != null)
            {
                try
                {
                    var roleType = RoleId.Get(random.GetType());
                    dummy.RpcChangeRole(roleType);
                    roleList.Remove(random);
                }
                catch
                {
                    dummy.RpcChangeRole((ushort)RoleTypes.Crewmate);
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
    }
}