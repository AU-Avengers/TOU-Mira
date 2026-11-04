namespace TownOfUs.Roles;

public interface ICustomKillAnimRole
{
    bool ShouldOverrideKillAnim(OverlayKillAnimation killAnimation, KillOverlayInitData initData, bool isCamoCommsOn, bool sensitiveKillAnimsOn);
}