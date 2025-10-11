using System.Collections;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modules;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class DisperserModifier : TouGameModifier, IWikiDiscoverable
{
    public override string LocaleKey => "Disperser";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override string IntroInfo => TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurb");

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Disperser;
    public override ModifierFaction FactionType => ModifierFaction.ImpostorUtility;
    public override Color FreeplayFileColor => new Color32(255, 25, 25, 255);

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.Get($"TouModifier{LocaleKey}Disperse"),
                    TouLocale.GetParsed($"TouModifier{LocaleKey}DisperseWikiDescription"),
                    TouAssets.DisperseSprite)
            };
        }
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.DisperserChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.DisperserAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsImpostor() &&
               !role.Player.GetModifierComponent().HasModifier<SatelliteModifier>(true) &&
               !role.Player.GetModifierComponent().HasModifier<ButtonBarryModifier>(true);
    }

    public static IEnumerator CoDisperse(Dictionary<byte, Vector2> coordinates)
    {
        yield return HudManager.Instance.CoFadeFullScreen(Color.clear, new Color(0.6f, 0.1f, 0.2f, 1f), 11f / 24f);
        yield return HudManager.Instance.CoFadeFullScreen(new Color(0.6f, 0.1f, 0.2f, 1f), Color.clear);

        DispersePlayersToCoordinates(coordinates);

        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}Everyone has been dispersed to a vent!</color></b>", Color.white,
            new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Disperser.LoadAsset());

        notif1.AdjustNotification();
    }

    public static void DispersePlayersToCoordinates(Dictionary<byte, Vector2> coordinates)
    {
        if (coordinates.ContainsKey(PlayerControl.LocalPlayer.PlayerId))
        {
            if (Minigame.Instance)
            {
                try
                {
                    Minigame.Instance.Close();
                }
                catch
                {
                    /* ignored */
                }
            }

            if (PlayerControl.LocalPlayer.inVent)
            {
                PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                PlayerControl.LocalPlayer.MyPhysics.ExitAllVents();
            }
        }

        foreach (var (key, value) in coordinates)
        {
            var player = MiscUtils.PlayerById(key)!;
            player.transform.position = value;

            if (PlayerControl.LocalPlayer == player)
            {
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(value);
            }
        }

        if (PlayerControl.LocalPlayer.walkingToVent)
        {
            PlayerControl.LocalPlayer.inVent = false;
            Vent.currentVent = null;
            PlayerControl.LocalPlayer.moveable = true;
            PlayerControl.LocalPlayer.MyPhysics.StopAllCoroutines();
        }

        if (ModCompatibility.SubLoaded)
        {
            ModCompatibility.ChangeFloor(PlayerControl.LocalPlayer.transform.position.y > -7f);
        }
    }

    public static Dictionary<byte, Vector2> GenerateDisperseCoordinates()
    {
        var targets = PlayerControl.AllPlayerControls.ToArray()
            .Where(player => !player.Data.IsDead && !player.Data.Disconnected).ToList();

        // players with the ImmovableModifier can't be dispersed
        targets.RemoveAll(x => x.HasModifier<ImmovableModifier>());

        var vents = Object.FindObjectsOfType<Vent>();

        var coordinates = new Dictionary<byte, Vector2>(targets.Count);

        foreach (var target in targets)
        {
            var destination = vents.Random()!.transform.position + new Vector3(0f, 0.3636f, 0f);
            coordinates.Add(target.PlayerId, destination);
        }

        return coordinates;
    }
}