using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class ArsonistIgniteButton : TownOfUsRoleButton<ArsonistRole>
{
    public PlayerControl? ClosestTarget;
    public override string Name => TouLocale.GetParsed("TouRoleArsonistIgnite", "Ignite");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Arsonist;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<ArsonistOptions>.Instance.DouseCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.IgniteButtonSprite;

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
        var dousedPlayers = PlayersInRange.Where(x => x.HasModifier<ArsonistDousedModifier>()).ToList();
        if (OptionGroupSingleton<ArsonistOptions>.Instance.LegacyArsonist)
        {
            dousedPlayers = PlayerControl.AllPlayerControls.ToArray()
                .Where(x => x.HasModifier<ArsonistDousedModifier>()).ToList();
        }

        PlayerControl.LocalPlayer.RpcSpecialMultiMurder(dousedPlayers, true, teleportMurderer: false,
            playKillSound: false,
            causeOfDeath: "Arsonist");

        TouAudio.PlaySound(TouAudio.ArsoIgniteSound);

        CustomButtonSingleton<ArsonistDouseButton>.Instance.ResetCooldownAndOrEffect();
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