using BepInEx.Configuration;
using MiraAPI.GameOptions;
using MiraAPI.LocalSettings.Attributes;
using MiraAPI.LocalSettings.SettingTypes;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs;

public class TouLocalTabGameplay(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "<size=80%>Gameplay</size>";
    protected override bool ShouldCreateLabels => true;

    public override void OnOptionChanged(ConfigEntryBase configEntry)
    {
        base.OnOptionChanged(configEntry);
        var roleAvailable = PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data &&
                            PlayerControl.LocalPlayer.Data.Role;
        if ((configEntry == ParasitePiPLocation || configEntry == ParasitePiPSize) &&
            roleAvailable &&
                 PlayerControl.LocalPlayer.Data.Role is Roles.Impostor.ParasiteRole parasiteRole)
        {
            // Apply PiP changes to the Parasite (controller) side.
            parasiteRole.MarkPiPSettingsDirty(resetManualThisSession: true);
            parasiteRole.TickPiP();
        }
        else if (configEntry == SonarTargetType && roleAvailable && PlayerControl.LocalPlayer.Data.Role is SonarRole)
        {
            var update = OptionGroupSingleton<SonarOptions>.Instance.UpdateInterval;
            if (SonarTargetType.Value is SonarTargetStyle.Arrows)
            {
                var currentHeartbeats = ModifierUtils.GetPlayersWithModifier<SonarHeartbeatTargetModifier>();
                foreach (var plr in currentHeartbeats)
                {
                    plr.RemoveModifier<SonarHeartbeatTargetModifier>();

                    Color color = Palette.PlayerColors[plr.GetDefaultAppearance().ColorId];
                    plr.AddModifier<SonarArrowTargetModifier>(PlayerControl.LocalPlayer, color, update);
                }
            }
            else
            {
                var currentArrows = ModifierUtils.GetPlayersWithModifier<SonarArrowTargetModifier>();
                foreach (var plr in currentArrows)
                {
                    plr.RemoveModifier<SonarArrowTargetModifier>();

                    Color color = Palette.PlayerColors[plr.GetDefaultAppearance().ColorId];
                    plr.AddModifier<SonarHeartbeatTargetModifier>(PlayerControl.LocalPlayer, color, update);
                }
            }
        }
    }

    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabIcon = TouAssets.LocalGameplay,
        HideIconOnHover = false,
    };

    [LocalEnumSetting(names: ["ArrowDefault", "ArrowDarkGlow", "ArrowColorGlow", "ArrowLegacy"])]
    public ConfigEntry<ArrowStyleType> ArrowStyleEnum { get; private set; } =
        config.Bind("Gameplay", "ArrowStyle", ArrowStyleType.Default);

    [LocalEnumSetting(names: ["PiPLocationTopLeft", "PiPLocationMiddleLeft", "PiPLocationBottomLeft", "PiPLocationTopRight", "PiPLocationMiddleRight", "PiPLocationBottomRight", "PiPLocationDynamic"])]
    public ConfigEntry<ParasitePiPLocation> ParasitePiPLocation { get; private set; } =
        config.Bind("Role Visuals", "ParasitePiPLocation", TownOfUs.ParasitePiPLocation.Dynamic);

    [LocalEnumSetting(names: ["PiPSizeNormal", "PiPSizeSmall", "PiPSizeLarge"])]
    public ConfigEntry<ParasitePiPSize> ParasitePiPSize { get; private set; } =
        config.Bind("Role Visuals", "ParasitePiPSize", TownOfUs.ParasitePiPSize.Normal);

    [LocalEnumSetting(names: ["FlashWhite", "FlashLightGray", "FlashGray", "FlashDarkGray"])]
    public ConfigEntry<GrenadeFlashColor> GrenadierFlashColor { get; private set; } =
        config.Bind("Role Visuals", "GrenadierFlashColor", GrenadeFlashColor.LightGray);

    [LocalEnumSetting(names: ["SonarHeartbeats", "SonarArrows"])]
    public ConfigEntry<SonarTargetStyle> SonarTargetType { get; private set; } =
        config.Bind("Role Visuals", "SonarTargetType", SonarTargetStyle.Heartbeats);

    [LocalToggleSetting]
    public ConfigEntry<bool> ProsecutorProsToggling { get; private set; } =
        config.Bind("Role Visuals", "ProsecutorProsToggling", false);
}

public enum SonarTargetStyle
{
    Heartbeats,
    Arrows
}

public enum ArrowStyleType
{
    Default,
    DarkGlow,
    ColorGlow,
    Legacy
}

public enum GrenadeFlashColor
{
    White,
    LightGray,
    Gray,
    DarkGray
}

public enum ParasitePiPLocation
{
    TopLeft,
    MiddleLeft,
    BottomLeft,
    TopRight,
    MiddleRight,
    BottomRight,
    Dynamic
}

public enum ParasitePiPSize
{
    Normal,
    Small,
    Large
}