using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfUs.Modules;
using TownOfUs.Options.Modifiers;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class CelebrityModifier : TouGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Celebrity,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Celebrity.LoadAsset(),
            "TouMira.Modifier.Crewmate.Celebrity", 1.45f));
    public override string IdPart => "Celebrity";
    public override string ModifierName => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}");
    public override string IntroInfo => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.IntroBlurb");

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.WikiDescription");
    }

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Celebrity;
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);

    public override ModifierFaction FactionType => ModifierFaction.CrewmatePostmortem;

    public DateTime DeathTime { get; set; }
    public float DeathTimeMilliseconds { get; set; }
    public string DeathMessage { get; set; }
    public string AnnounceMessage { get; set; }
    public string StoredRoom { get; set; }
    public bool Announced { get; set; }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.CelebrityChance;
    }

    public override int GetAmountPerGame()
    {
        return 1;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }

    public static void CelebrityKilled(PlayerControl source, PlayerControl player, string customDeath = "")
    {
        if (!player.HasModifier<CelebrityModifier>())
        {
            Error("RpcCelebrityKilled - Invalid Celebrity");
            return;
        }

        var room = MiscUtils.GetRoomName(player.GetTruePosition());

        var celeb = player.GetModifier<CelebrityModifier>()!;
        celeb.StoredRoom = room;
        celeb.DeathTime = DateTime.UtcNow;
        var splitCelebrityString = MiraLocaleManager.Get("TownOfUsMira.Modifier.CelebrityPopup").Split(":");

        var announceText = splitCelebrityString[0];
        if (splitCelebrityString.Length > 1)
        {
            announceText = $"<size=90%>{splitCelebrityString[0]}</size>\n<size=70%>{splitCelebrityString[1]}</size>";
        }

        announceText = announceText.Replace("<player>", player.GetDefaultAppearance().PlayerName);

        celeb.AnnounceMessage = announceText;

        if (MeetingHud.Instance || ExileController.Instance)
        {
            celeb.Announced = true;
        }

        var celebHyperlink = $"&{MiraLocaleManager.Get("TownOfUsMira.Modifier.Celebrity")}";

        if (source == player)
        {
            celeb.DeathMessage = MiraLocaleManager.Get("TownOfUsMira.Modifier.CelebrityDetailsSelf");
        }
        else
        {
            var role = source.Data.Role is IGhostRole ? source.Data.Role : source.GetRoleWhenAlive();
            var cod = "Killer";

            var roleToCheck = role is MirrorcasterRole mirror ? mirror.ContainedRole ?? mirror : role;
            var IdPart = roleToCheck.GetRoleIdPart();
            if (IdPart != "KEY_MISS" &&
                !MiraLocaleManager.Get($"DiedTo{IdPart}").Contains("STRMISS"))
            {
                cod = IdPart;
            }

            if (source.Data.Role is IGhostRole && source.Data.Role is ITownOfUsRole touRole)
            {
                cod = touRole.IdPart;
            }

            var text = MiraLocaleManager.Get($"DiedTo{cod}").ToLowerInvariant();
            celeb.DeathMessage = MiraLocaleManager.Get("TownOfUsMira.Modifier.CelebrityDetailsKilled").Replace("<killed>", text);
            celeb.DeathMessage =
                celeb.DeathMessage.Replace("<role>", $"#{role.GetRoleName().ToLowerInvariant().Replace(" ", "-")}");
        }

        celeb.DeathMessage = celeb.DeathMessage.Replace("<modifier>", celebHyperlink);
        celeb.DeathMessage = celeb.DeathMessage.Replace("<player>", player.GetDefaultAppearance().PlayerName);
        celeb.DeathMessage = celeb.DeathMessage.Replace("<room>", celeb.StoredRoom);
    }

    [MethodRpc((uint)TownOfUsRpc.UpdateCelebrityKilled)]
    public static void RpcUpdateCelebrityKilled(PlayerControl player, float milliseconds)
    {
        if (!player.HasModifier<CelebrityModifier>())
        {
            Error("RpcUpdateCelebrityKilled - Invalid Celebrity");
            return;
        }

        Error($"RpcUpdateCelebrityKilled milliseconds: {milliseconds}");

        var celeb = player.GetModifier<CelebrityModifier>()!;

        celeb.DeathTimeMilliseconds = milliseconds;
    }
}