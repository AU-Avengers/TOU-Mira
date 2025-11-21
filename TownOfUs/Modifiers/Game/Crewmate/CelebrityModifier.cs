using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using TownOfUs.Modules;
using TownOfUs.Options.Modifiers;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class CelebrityModifier : TouGameModifier, IWikiDiscoverable
{
    public override string LocaleKey => "Celebrity";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override string IntroInfo => TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurb");

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription");
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
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.CelebrityAmount != 0 ? 1 : 0;
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
        var splitCelebrityString = TouLocale.GetParsed("TouModifierCelebrityPopup").Split(":");

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

        var celebHyperlink = $"&{TouLocale.Get("TouModifierCelebrity")}";

        if (source == player)
        {
            celeb.DeathMessage = TouLocale.GetParsed("TouModifierCelebrityDetailsSelf");
        }
        else
        {
            var role = source.GetRoleWhenAlive();
            var cod = "Killer";
            switch (role)
            {
                case MirrorcasterRole mirror:
                    cod = mirror.UnleashString != string.Empty
                        ? mirror.UnleashString
                        : TouLocale.Get("DiedToKiller");
                    mirror.UnleashString = string.Empty;
                    mirror.ContainedRole = null;
                    break;
                default:
                    var localeKey = role.GetRoleLocaleKey();
                    if (localeKey == "KEY_MISS" ||
                        TouLocale.Get($"DiedTo{localeKey}").Contains("STRMISS"))
                    {
                        break;
                    }

                    cod = localeKey;
                    break;
            }

            if (source.Data.Role is SpectreRole phantomTouRole)
            {
                role = source.Data.Role;
                cod = phantomTouRole.LocaleKey;
            }

            var text = role is MirrorcasterRole
                ? cod.ToLowerInvariant()
                : TouLocale.Get($"DiedTo{cod}").ToLowerInvariant();
            celeb.DeathMessage = TouLocale.GetParsed("TouModifierCelebrityDetailsKilled").Replace("<killed>", text);
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