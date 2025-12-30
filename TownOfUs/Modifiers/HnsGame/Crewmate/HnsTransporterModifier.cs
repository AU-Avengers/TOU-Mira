using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Options.Modifiers;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.HnsGame.Crewmate;

public sealed class HnsTransporterModifier : HnsGameModifier
{
    public override string LocaleKey => "Transporter";
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Transporter;
    public override ModifierFaction FactionType => ModifierFaction.HiderPostmortem;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }


    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.TransporterChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.TransporterAmount;
    }

    [MethodRpc((uint)TownOfUsRpc.TransportSeeker)]
    public static void RpcTransportSeeker(PlayerControl transporter, PlayerControl seeker)
    {
        if (!transporter.HasModifier<HnsTransporterModifier>())
        {
            Error("RpcTransportSeeker - Invalid Transporter");
            return;
        }
        var randomVictim = Helpers.GetAlivePlayers().Where(x => x != transporter && x != seeker).Random();
        if (randomVictim == null)
        {
            Error("RpcTransportSeeker - No other players could be swapped with!");
            return;
        }

        var positions = GetAdjustedPositions(seeker, randomVictim);
        if (randomVictim.inVent)
        {
            randomVictim.MyPhysics.ExitAllVents();
        }

        Transport(seeker, positions.Item2);
        Transport(randomVictim, positions.Item1);

        if (seeker.AmOwner || randomVictim.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Transporter.ToTextColor()}{TouLocale.GetParsed("TouRoleTransporterTransportNotif")}</color></b>", Color.white,
                new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Transporter.LoadAsset());

            notif1.AdjustNotification();

            if (Minigame.Instance != null)
            {
                Minigame.Instance.Close();
                Minigame.Instance.Close();
            }
        }

        (Vector2, Vector2) GetAdjustedPositions(PlayerControl player1, PlayerControl player2)
        {
            // assign dummy values so it doesnt error about returning unassigned variables
            var tp1Position = player1.GetTruePosition();
            tp1Position = new Vector2(tp1Position.x, tp1Position.y + 0.3636f);

            var tp2Position = player2.GetTruePosition();
            tp2Position = new Vector2(tp2Position.x, tp2Position.y + 0.3636f);

            if (player1.HasModifier<HnsMiniModifier>())
            {
                tp1Position = new Vector2(tp1Position.x, tp1Position.y + 0.2233912f * 0.75f);
                tp2Position = new Vector2(tp2Position.x, tp2Position.y - 0.2233912f * 0.75f);
            }
            else if (player2.HasModifier<HnsMiniModifier>())
            {
                tp1Position = new Vector2(tp1Position.x, tp1Position.y - 0.2233912f * 0.75f);
                tp2Position = new Vector2(tp2Position.x, tp2Position.y + 0.2233912f * 0.75f);
            }

            return (tp1Position, tp2Position);
        }
    }

    public static void Transport(PlayerControl player, Vector3 position)
    {
        player.transform.position = position;

        player.MyPhysics.ResetMoveState();
        player.transform.position = position;
        player.NetTransform.SnapTo(position);

        if (player.AmOwner)
        {
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(position);
        }

        var cnt = player.TryCast<CustomNetworkTransform>();
        if (cnt != null)
        {
            cnt.SnapTo(position, (ushort)(cnt.lastSequenceId + 1));

            if (cnt.AmOwner && ModCompatibility.IsSubmerged())
            {
                ModCompatibility.ChangeFloor(cnt.myPlayer.GetTruePosition().y > -7);
                ModCompatibility.CheckOutOfBoundsElevator(cnt.myPlayer);
            }
        }

        if (player.AmOwner)
        {
            // If the transported player is a Puppeteer/Parasite controlling someone, snap camera to the victim instead
            PlayerControl? cameraTarget = null;
            
            if (player.Data?.Role is PuppeteerRole puppeteer && puppeteer.Controlled != null)
            {
                cameraTarget = puppeteer.Controlled;
            }
            else if (player.Data?.Role is ParasiteRole parasite && parasite.Controlled != null)
            {
                cameraTarget = parasite.Controlled;
            }
            
            MiscUtils.SnapPlayerCamera(cameraTarget ?? PlayerControl.LocalPlayer);
        }
    }
}