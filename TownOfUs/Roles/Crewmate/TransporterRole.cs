using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Events.Crewmate;
using TownOfUs.Events.TouEvents;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Roles.Impostor;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class TransporterRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string RoleName => TouLocale.Get(TouNames.Transporter, "Transporter");
    public string RoleDescription => "Choose Two Players To Swap Locations";
    public string RoleLongDescription => "Choose two players to swap locations with one another";
    public Color RoleColor => TownOfUsColors.Transporter;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Transporter,
        IntroSound = TouAudio.TimeLordIntroSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }
    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Crewmate Support role that can transport two players, dead or alive, to swap their locations."
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Transport",
            "Switch the positions of two players. Players can be transported out of vents." +
            "A red flash means one of the players became an invalid target," +
            "such as going on a ladder or zipline",
            TouCrewAssets.Transport)
    ];

    [MethodRpc((uint)TownOfUsRpc.Transport, SendImmediately = true)]
    public static void RpcTransport(PlayerControl transporter, byte player1, byte player2)
    {
        if (transporter.Data.Role is not TransporterRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcTransport - Invalid Transporter");
            return;
        }

        var t1 = GetTarget(player1);
        var t2 = GetTarget(player2);

        //also check again incase they went on the usable while the transporter was picking but ignore vents
        if (t1 == null || t2 == null)
        {
            if (transporter.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(Color.red));
            }

            return;
        }

        var play1 = MiscUtils.PlayerById(player1)!;
        var play2 = MiscUtils.PlayerById(player2)!;

        var warden = play1.GetModifier<WardenFortifiedModifier>()?.Warden.GetRole<WardenRole>();
        if (warden != null)
        {
            if (transporter.AmOwner)
            {
                WardenRole.RpcWardenNotify(warden.Player, transporter, play1);
            }

            return;
        }

        var warden2 = play2.GetModifier<WardenFortifiedModifier>()?.Warden.GetRole<WardenRole>();
        if (warden2 != null)
        {
            if (transporter.AmOwner)
            {
                WardenRole.RpcWardenNotify(warden2.Player, transporter, play2);
            }

            return;
        }

        var cleric = play1.GetModifier<ClericBarrierModifier>()?.Cleric.GetRole<ClericRole>();
        if (cleric != null)
        {
            if (transporter.AmOwner)
            {
                ClericRole.RpcClericBarrierAttacked(cleric.Player, transporter, play1);
            }

            return;
        }

        var cleric2 = play2.GetModifier<ClericBarrierModifier>()?.Cleric.GetRole<ClericRole>();
        if (cleric2 != null)
        {
            if (transporter.AmOwner)
            {
                ClericRole.RpcClericBarrierAttacked(cleric2.Player, transporter, play2);
            }

            return;
        }

        var infectedtrans = transporter.GetModifier<PlaguebearerInfectedModifier>();
        var infectedplayer1 = play1.GetModifier<PlaguebearerInfectedModifier>();
        var infectedplayer2 = play2.GetModifier<PlaguebearerInfectedModifier>();
        if (infectedtrans != null)
        {
            if (infectedplayer1 == null)
            {
                play1.AddModifier<PlaguebearerInfectedModifier>(infectedtrans.PlagueBearerId);
            }

            if (infectedplayer2 == null)
            {
                play2.AddModifier<PlaguebearerInfectedModifier>(infectedtrans.PlagueBearerId);
            }
        }
        else if (infectedtrans == null && infectedplayer1 != null)
        {
            transporter.AddModifier<PlaguebearerInfectedModifier>(infectedplayer1.PlagueBearerId);
        }
        else if (infectedtrans == null && infectedplayer2 != null)
        {
            transporter.AddModifier<PlaguebearerInfectedModifier>(infectedplayer2.PlagueBearerId);
        }

        LookoutEvents.CheckForLookoutWatched(transporter, play1);
        LookoutEvents.CheckForLookoutWatched(transporter, play2);

        var mercenary = PlayerControl.LocalPlayer.Data.Role as MercenaryRole;
        if (play1.HasModifier<MercenaryGuardModifier>() || (play2.HasModifier<MercenaryGuardModifier>() && mercenary))
        {
            mercenary!.AddPayment();
        }

        if (play1.TryGetModifier<InvulnerabilityModifier>(out var invic) && invic.AttackAllInteractions)
        {
            if (transporter.AmOwner)
            {
                play1.RpcCustomMurder(transporter);
            }

            return;
        }

        if (play2.TryGetModifier<InvulnerabilityModifier>(out var invic2) && invic2.AttackAllInteractions)
        {
            if (transporter.AmOwner)
            {
                play2.RpcCustomMurder(transporter);
            }

            return;
        }

        if (play1.HasModifier<VeteranAlertModifier>())
        {
            if (transporter.AmOwner)
            {
                play1.RpcCustomMurder(transporter);
            }

            return;
        }

        if (play2.HasModifier<VeteranAlertModifier>())
        {
            if (transporter.AmOwner)
            {
                play2.RpcCustomMurder(transporter);
            }

            return;
        }

        if (play1.TryGetModifier<ShyModifier>(out var shy))
        {
            shy.OnRoundStart();
        }

        if (play2.TryGetModifier<ShyModifier>(out var shy2))
        {
            shy2.OnRoundStart();
        }

        if (t1.TryCast<DeadBody>())
        {
            PreCheckUndertaker(t1.TryCast<DeadBody>()!);
        }

        if (t2.TryCast<DeadBody>())
        {
            PreCheckUndertaker(t2.TryCast<DeadBody>()!);
        }

        var positions = GetAdjustedPositions(t1, t2);
        if (t1.TryCast<PlayerControl>() != null && t2.TryCast<DeadBody>() != null)
        {
            positions.Item1 = play1.Collider.bounds.center;
        }
        if (t2.TryCast<PlayerControl>() != null && t1.TryCast<DeadBody>() != null)
        {
            positions.Item2 = play2.Collider.bounds.center;
        }
        Transport(t1, positions.Item2);
        Transport(t2, positions.Item1);
        var touAbilityEvent = new TouAbilityEvent(AbilityType.TransporterTransport, transporter, t1, t2);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (transporter.AmOwner)
        {
            var button = CustomButtonSingleton<TransporterTransportButton>.Instance;
            button.DecreaseUses();
            button.ResetCooldownAndOrEffect();

            TownOfUsColors.UseBasic = false;
            if (button.TextOutlineColor != Color.clear)
            {
                button.SetTextOutline(button.TextOutlineColor);
                if (button.Button != null)
                {
                    button.Button.usesRemainingSprite.color = button.TextOutlineColor;
                }
            }

            TownOfUsColors.UseBasic = TownOfUsPlugin.UseCrewmateTeamColor.Value;
        }

        if (play1.AmOwner && t1 is PlayerControl || play2.AmOwner && t2 is PlayerControl)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Transporter.ToTextColor()}You were transported!</color></b>", Color.white,
                spr: TouRoleIcons.Transporter.LoadAsset());

            notif1.Text.SetOutlineThickness(0.35f);
            notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
            
            if (Minigame.Instance != null)
            {
                Minigame.Instance.Close();
                Minigame.Instance.Close();
            }
        }

        MonoBehaviour? GetTarget(byte id)
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
            
            if (pc.HasModifier<NoTransportModifier>())
            {
                return null;
            }
            
            if (pc.GetModifiers<BaseModifier>().Any(x => x is IUntransportable))
            {
                return null;
            }
            // FAKE PLAYER MUST BE A MONO BEHAVIOUR FOR THIS TO WORK
            /*var fakePlayer = Utilities.MiscUtils.GetFakePlayer(pc)?.body;

            if (fakePlayer)
            {
                return fakePlayer;
            } */

            if (pc.moveable || pc.inVent || (pc.TryGetModifier<DisabledModifier>(out var mod) &&
                                             (!mod.IsConsideredAlive || !mod.CanBeInteractedWith)))
            {
                if (pc.inVent)
                {
                    pc.MyPhysics.ExitAllVents();
                }

                return pc;
            }

            return null;
        }

        void PreCheckUndertaker(DeadBody body)
        {
            if (PlayerControl.LocalPlayer.Data.Role is not TransporterRole)
            {
                return;
            }

            var mods = ModifierUtils.GetActiveModifiers<DragModifier>();

            foreach (var mod in mods)
            {
                if (mod.BodyId == body.ParentId)
                {
                    UndertakerRole.RpcStopDragging(mod.Player, body.transform.position);
                }
            }
        }

        (Vector2, Vector2) GetAdjustedPositions(MonoBehaviour transportable, MonoBehaviour transportable2)
        {
            // assign dummy values so it doesnt error about returning unassigned variables
            Vector2 TP1Position = new(0, 0);
            Vector2 TP2Position = new(0, 0);

            if (transportable.TryCast<DeadBody>() == null && transportable2.TryCast<DeadBody>() == null)
            {
                Logger<TownOfUsPlugin>.Error($"type: {transportable.GetIl2CppType().Name}");
                var TP1 = transportable.TryCast<PlayerControl>()!;
                TP1Position = TP1.GetTruePosition();
                TP1Position = new Vector2(TP1Position.x, TP1Position.y + 0.3636f);

                var TP2 = transportable2.TryCast<PlayerControl>()!;
                TP2Position = TP2.GetTruePosition();
                TP2Position = new Vector2(TP2Position.x, TP2Position.y + 0.3636f);

                if (TP1.HasModifier<MiniModifier>())
                {
                    TP1Position = new Vector2(TP1Position.x, TP1Position.y + 0.2233912f * 0.75f);
                    TP2Position = new Vector2(TP2Position.x, TP2Position.y - 0.2233912f * 0.75f);
                }
                else if (TP2.HasModifier<MiniModifier>())
                {
                    TP1Position = new Vector2(TP1Position.x, TP1Position.y - 0.2233912f * 0.75f);
                    TP2Position = new Vector2(TP2Position.x, TP2Position.y + 0.2233912f * 0.75f);
                }
            }
            else if (transportable.TryCast<DeadBody>() != null && transportable2.TryCast<DeadBody>() == null)
            {
                var Player1Body = transportable.TryCast<DeadBody>()!;
                TP1Position = Player1Body.TruePosition;
                TP1Position = new Vector2(TP1Position.x, TP1Position.y + 0.3636f);

                var TP2 = transportable2.TryCast<PlayerControl>()!;
                TP2Position = TP2.GetTruePosition();
                TP2Position = new Vector2(TP2Position.x, TP2Position.y + 0.3636f);

                if (TP2.HasModifier<MiniModifier>())
                {
                    TP1Position = new Vector2(TP1Position.x, TP1Position.y - 0.2233912f * 0.75f);
                    TP2Position = new Vector2(TP2Position.x, TP2Position.y + 0.2233912f * 0.75f);
                }
            }
            else if (transportable.TryCast<DeadBody>() == null && transportable2.TryCast<DeadBody>() != null)
            {
                var TP1 = transportable.TryCast<PlayerControl>()!;
                TP1Position = TP1.GetTruePosition();
                TP1Position = new Vector2(TP1Position.x, TP1Position.y + 0.3636f);

                var Player2Body = transportable2.TryCast<DeadBody>()!;
                TP2Position = Player2Body.TruePosition;
                TP2Position = new Vector2(TP2Position.x, TP2Position.y + 0.3636f);
                if (TP1.HasModifier<MiniModifier>())
                {
                    TP1Position = new Vector2(TP1Position.x, TP1Position.y + 0.2233912f * 0.75f);
                    TP2Position = new Vector2(TP2Position.x, TP2Position.y - 0.2233912f * 0.75f);
                }
            }
            else if (transportable.TryCast<DeadBody>() != null && transportable2.TryCast<DeadBody>() != null)
            {
                TP1Position = transportable.TryCast<DeadBody>()!.TruePosition;
                TP2Position = transportable2.TryCast<DeadBody>()!.TruePosition;
            }

            return (TP1Position, TP2Position);
        }
    }

    public static void Transport(MonoBehaviour mono, Vector3 position)
    {
        var deadBody = mono.TryCast<DeadBody>();
        var player = mono.TryCast<PlayerControl>();
        if (player != null && player.HasModifier<ImmovableModifier>())
        {
            return;
        }

        if (deadBody != null &&
            MiscUtils.PlayerById(deadBody.ParentId)?.HasModifier<ImmovableModifier>() == true)
        {
            return;
        }

        mono.transform.position = position;
        Collider2D cd = mono.GetComponent<Collider2D>();
        if (cd != null && deadBody != null)
        {
            mono.transform.position += cd.bounds.center - position;
        }
        if (player != null)
        {
            player.MyPhysics.ResetMoveState();
            player.transform.position = position;
            player.NetTransform.SnapTo(position);
        }

        var cnt = mono.TryCast<CustomNetworkTransform>();
        if (cnt != null)
        {
            cnt.SnapTo(position, (ushort)(cnt.lastSequenceId + 1));

            if (cnt.AmOwner && ModCompatibility.IsSubmerged())
            {
                ModCompatibility.ChangeFloor(cnt.myPlayer.GetTruePosition().y > -7);
                ModCompatibility.CheckOutOfBoundsElevator(cnt.myPlayer);
            }
        }

        if (player != null && player.AmOwner)
        {
            MiscUtils.SnapPlayerCamera(PlayerControl.LocalPlayer);
        }
    }
}