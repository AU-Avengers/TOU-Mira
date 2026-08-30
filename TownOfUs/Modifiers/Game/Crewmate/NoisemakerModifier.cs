using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class NoisemakerModifier : TouGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        new Color32(232, 105, 158, 255),
        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Noisemaker.LoadAsset(),
            "AmongUs.Role.Noisemaker", 1.45f));
    public override string IdPart => "Noisemaker";
    public override string ModifierName => MiraLocaleManager.Get($"TouModifier{IdPart}");
    public override string IntroInfo => MiraLocaleManager.Get($"TouModifier{IdPart}IntroBlurb");

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}TabDescription").Replace("<noiseTime>",
            $"{OptionGroupSingleton<NoisemakerOptions>.Instance.AlertDuration}");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}WikiDescription")
               + MiscUtils.AppendOptionsText(GetType());
    }

    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Noisemaker;
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);

    public override ModifierFaction FactionType => ModifierFaction.CrewmatePostmortem;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.NoisemakerChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.NoisemakerAmount;
    }

    private static void SoundDynamics(PlayerControl player, AudioSource source)
    {
        if (!PlayerControl.LocalPlayer)
        {
            source.volume = 0f;
            return;
        }

        source.volume = 1f;
        var truePosition = PlayerControl.LocalPlayer.GetTruePosition();
        source.volume = SoundManager.GetSoundVolume(player.GetTruePosition(), truePosition, 7f, 50f, 0.5f);
    }

    public static void NotifyOfDeath(PlayerControl player, bool checkRole)
    {
        var noise = RoleManager.Instance.GetRole(RoleTypes.Noisemaker).Cast<NoisemakerRole>();
        var viperBody = Object.FindObjectsOfType<ViperDeadBody>().FirstOrDefault(x => x.ParentId == player.Data.PlayerId);
        if (checkRole)
        {
            if (player.GetRoleWhenAlive().Role is not RoleTypes.Noisemaker)
            {
                return;
            }

            if (PlayerControl.LocalPlayer.IsImpostor() &&
                !noise.impostorAlert)
            {
                return;
            }

            if (PlayerControl.LocalPlayer.AreCommsAffected())
            {
                return;
            }

            if (Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == player.PlayerId && x.myCollider.enabled) == null &&
                OptionGroupSingleton<NoisemakerOptions>.Instance.BodyCheck)
            {
                return;
            }

            if (Constants.ShouldPlaySfx())
            {
                var audio = SoundManager.Instance.PlaySound(TouAudio.NoisemakerDeathSound.LoadAsset(), false, 1,
                    SoundManager.Instance.SfxChannel);
                SoundDynamics(player, audio);
                VibrationManager.Vibrate(1f, PlayerControl.LocalPlayer.GetTruePosition(), 7f, 1.2f);
            }
            
            var deathArrowPrefab =
                Object.Instantiate(noise.deathArrowPrefab, player.transform.position, Quaternion.identity);

            var deathArrow = deathArrowPrefab.GetComponent<NoisemakerArrow>();
            var duration = noise.alertDuration;
            if (viperBody != null)
            {
                float roleFloat = viperBody.maxDissolveTime;
                deathArrow.SetDuration(roleFloat < duration ? roleFloat : duration);
            }
            else
            {
                deathArrow.SetDuration(duration);
            }
            if (player.AmOwner)
            {
                deathArrow.alwaysMaxSize = true;
            }

            deathArrow.gameObject.SetActive(true);
            deathArrow.target = player.GetTruePosition();
        }
        else
        {
            if (!player.HasModifier<NoisemakerModifier>())
            {
                return;
            }

            if (PlayerControl.LocalPlayer.IsImpostor() &&
                !OptionGroupSingleton<NoisemakerOptions>.Instance.ImpostorsAlerted)
            {
                return;
            }

            if (PlayerControl.LocalPlayer.Is(RoleAlignment.NeutralKilling) &&
                !OptionGroupSingleton<NoisemakerOptions>.Instance.NeutsAlerted)
            {
                return;
            }

            if (PlayerControl.LocalPlayer.AreCommsAffected() &&
                OptionGroupSingleton<NoisemakerOptions>.Instance.CommsAffected)
            {
                return;
            }

            if (Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == player.PlayerId && x.myCollider.enabled) == null &&
                OptionGroupSingleton<NoisemakerOptions>.Instance.BodyCheck)
            {
                return;
            }

            if (Constants.ShouldPlaySfx())
            {
                var audio = SoundManager.Instance.PlaySound(TouAudio.NoisemakerDeathSound.LoadAsset(), false, 1,
                    SoundManager.Instance.SfxChannel);
                SoundDynamics(player, audio);
                VibrationManager.Vibrate(1f, PlayerControl.LocalPlayer.GetTruePosition(), 7f, 1.2f);
            }

            var deathArrowPrefab =
                Object.Instantiate(noise.deathArrowPrefab, player.transform.position, Quaternion.identity);

            var deathArrow = deathArrowPrefab.GetComponent<NoisemakerArrow>();
            var duration = OptionGroupSingleton<NoisemakerOptions>.Instance.AlertDuration;
            if (viperBody != null)
            {
                float roleFloat = viperBody.maxDissolveTime;
                deathArrow.SetDuration(roleFloat < duration ? roleFloat : duration);
            }
            else
            {
                deathArrow.SetDuration(duration);
            }
            if (player.AmOwner)
            {
                deathArrow.alwaysMaxSize = true;
            }

            deathArrow.gameObject.SetActive(true);
            deathArrow.target = player.GetTruePosition();
        }
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate() && role is not NoisemakerRole && role is not AltruistRole;
    }
}