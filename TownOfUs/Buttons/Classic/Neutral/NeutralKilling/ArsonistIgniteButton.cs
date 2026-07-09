using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class ArsonistIgniteButton : TownOfUsRoleButton<ArsonistRole>, ILegacyCapable
{
    public PlayerControl? ClosestTarget;
    public override string Name => TouLocale.GetParsed("TouRoleArsonistIgnite", "Ignite");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Arsonist;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<ArsonistOptions>.Instance.DouseCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? LegacyNeutAssets.IgniteButtonSprite : TouNeutAssets.IgniteButtonSprite;

    private static List<PlayerControl> PlayersInRange => Helpers.GetClosestPlayers(PlayerControl.LocalPlayer,
        OptionGroupSingleton<ArsonistOptions>.Instance.IgniteRadius.Value * ShipStatus.Instance.MaxLightRadius);

    [HideFromIl2Cpp] public Ignite? Ignite { get; set; }

    public override bool CanUse()
    {
        if (OptionGroupSingleton<ArsonistOptions>.Instance.LegacyArsonist)
        {
            return base.CanUse() && ClosestTarget != null;
        }

        var count = PlayersInRange.Count(x => x.HasModifier<ArsonistDousedModifier>());

        if (count > 0 && !PlayerControl.LocalPlayer.HasDied() && Timer <= 0)
        {
            var pos = PlayerControl.LocalPlayer.transform.position;
            pos.z += 0.001f;

            if (Ignite == null)
            {
                Ignite = Ignite.CreateIgnite(pos);
            }
            else
            {
                Ignite.Transform.localPosition = pos;
            }
        }
        else
        {
            if (Ignite != null)
            {
                Ignite.Clear();
                Ignite = null;
            }
        }

        return base.CanUse() && count > 0;
    }

    protected override void OnClick()
    {
        var legacy = OptionGroupSingleton<ArsonistOptions>.Instance.LegacyArsonist;

        var dousedPlayers = legacy
            ? ModifierUtils.GetPlayersWithModifier<ArsonistDousedModifier>().ToList()
            : PlayersInRange.Where(x => x.HasModifier<ArsonistDousedModifier>()).ToList();

        // A Pestilence only retaliates when it is the *direct* victim of this ignite:
        //   - Legacy: the player we ignited on (ClosestTarget) is the Pestilence.
        //   - Non-legacy: the Pestilence is within the ignite radius.
        // Being incidentally doused elsewhere does NOT trigger a kill-back.
        var retaliatingPest = legacy
            ? (ClosestTarget != null && ClosestTarget.Data.Role is PestilenceRole ? ClosestTarget : null)
            : dousedPlayers.FirstOrDefault(x => x.Data.Role is PestilenceRole);

        // The Pestilence is invulnerable, so exclude it from the mass murder; it retaliates instead.
        var victims = dousedPlayers.Where(x => x.Data.Role is not PestilenceRole).ToList();

        if (victims.Count > 0)
        {
            PlayerControl.LocalPlayer.RpcSpecialMultiMurder(victims, MeetingCheck.OutsideMeeting, true,
                teleportMurderer: false,
                playKillSound: false,
                causeOfDeath: "Arsonist");
        }

        // Igniting directly on the Pestilence gets the Arsonist killed, reliably (no indirect race).
        retaliatingPest?.RpcCustomMurder(PlayerControl.LocalPlayer, MeetingCheck.OutsideMeeting);

        if (victims.Count > 0 || retaliatingPest != null)
        {
            TouAudio.PlaySound(TouAudio.ArsoIgniteSound);

            CustomButtonSingleton<ArsonistDouseButton>.Instance.ResetCooldownAndOrEffect();
        }
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);
        if (MeetingHud.Instance || !OptionGroupSingleton<ArsonistOptions>.Instance.LegacyArsonist)
        {
            return;
        }

        var killDistances =
            GameOptionsManager.Instance.currentNormalGameOptions.GetFloatArray(FloatArrayOptionNames.KillDistances);
        ClosestTarget = PlayerControl.LocalPlayer.GetClosestLivingPlayer(true,
            killDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance],
            predicate: x => x.HasModifier<ArsonistDousedModifier>());
    }
}